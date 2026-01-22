namespace Simple.Sqlite;

using Simple.DatabaseWrapper;
using Simple.DatabaseWrapper.Helpers;
using System;
using System.Text;

/// <summary>
/// Extension for Ingesting CSVs
/// </summary>
public static class CsvIngestionExtensions
{
    /// <summary>
    /// Load a CSV file into a database. 
    /// For custom CSV ingestion try DatabaseWrapper.CsvParser functions and insert with BulkInsert
    /// </summary>
    /// <typeparam name="T">Table class</typeparam>
    /// <param name="connection">Connection to be used</param>
    /// <param name="csvFile">FilePath of CSV file to be ingested</param>
    /// <param name="mapping">T mapping</param>
    /// <param name="bufferSize">Buffer size to be used on ingestion</param>
    /// <param name="encoding">CSV Enconding</param>
    /// <param name="delimiter">CSV delimiter char</param>
    /// <param name="quote">CSV quote char</param>
    public static void LoadFromCsvFile<T>(this ISqliteConnection connection, string csvFile, Func<string[], T> mapping, int bufferSize = 10_000, Encoding encoding = null, char delimiter = ';', char quote = '"')
    {
        using DataBuffer<T> buffer = new DataBuffer<T>(bufferSize, data =>
        {
            connection.BulkInsert<T>(data, OnConflict.Ignore);
        });
        var csvRows = CsvParser.ParseCsvFile(csvFile, encoding, delimiter: delimiter, quote: quote);

        foreach (var row in csvRows)
        {
            buffer.Add(mapping(row));
        }
        buffer.Flush();
    }

}
