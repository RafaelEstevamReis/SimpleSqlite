namespace Simple.Sqlite;

using Simple.DatabaseWrapper;
using Simple.DatabaseWrapper.Helpers;
using System;
using System.IO;
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
    public static void LoadFromCsvFile<T>(this ISqliteConnection connection, string csvFile, Func<string[], T> mapping, int bufferSize = 10_000, Encoding? encoding = null, char delimiter = ';', char quote = '"')
    {
        using var fs = File.OpenRead(csvFile);
        LoadFromCsvStream(connection, fs, encoding ?? Encoding.UTF8, mapping, bufferSize, delimiter, quote);
    }

#if !NET472
    /// <summary>
    /// Load a Zipped CSV file into a database. 
    /// For custom CSV ingestion try DatabaseWrapper.CsvParser functions and insert with BulkInsert
    /// </summary>
    public static void LoadFromCsvZippedFile<T>(this ISqliteConnection connection, string zipFile, Func<string, bool> entryFilter, Func<string[], T> mapping, int bufferSize = 10_000, Encoding? encoding = null, char delimiter = ';', char quote = '"')
    {
        using var fs = File.OpenRead(zipFile);
        using var zip = new System.IO.Compression.ZipArchive(fs, System.IO.Compression.ZipArchiveMode.Read);

        foreach (var entry in zip.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name)) continue;
            if (!entryFilter(entry.FullName)) continue;

            using var steam = entry.Open();
            LoadFromCsvStream(connection, steam, encoding ?? Encoding.UTF8, mapping, bufferSize, delimiter, quote);
        }
    }
#endif

    /// <summary>
    /// Load a CSV file stream into a database. 
    /// For custom CSV ingestion try DatabaseWrapper.CsvParser functions and insert with BulkInsert
    /// </summary>
    public static void LoadFromCsvStream<T>(this ISqliteConnection connection, Stream dataStream, Encoding encoding, Func<string[], T> mapping, int bufferSize = 10_000, char delimiter = ';', char quote = '"')
    {
        using var sr = new StreamReader(dataStream, encoding);
        LoadFromCsvStream(connection, sr, mapping, bufferSize, delimiter, quote);
    }

    /// <summary>
    /// Load a CSV stream into a database. 
    /// For custom CSV ingestion try DatabaseWrapper.CsvParser functions and insert with BulkInsert
    /// </summary>
    public static void LoadFromCsvStream<T>(this ISqliteConnection connection, StreamReader csvStream, Func<string[], T> mapping, int bufferSize = 10_000, char delimiter = ';', char quote = '"')
    {
        using DataBuffer<T> buffer = new DataBuffer<T>(bufferSize, data =>
        {
            connection.BulkInsert<T>(data, OnConflict.Ignore);
        });
        var csvRows = CsvParser.ParseCsv(csvStream, delimiter: delimiter, quote: quote);

        foreach (var row in csvRows)
        {
            buffer.Add(mapping(row));
        }
        buffer.Flush();
    }
}
