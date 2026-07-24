namespace Simple.Sqlite;

using Microsoft.Data.Sqlite;
using System;

/// <summary>
/// Extension for "Backup" related stuff
/// </summary>
public static class BackupExtension
{
    /// <summary>
    ///  Backup of the connected database
    /// </summary>
    /// <param name="source">Source database</param>
    /// <param name="fileName">Destination database filename</param>
    public static void BackupDatabase(this ISqliteConnection source, string fileName)
    {
        var fi = new System.IO.FileInfo(fileName);
        if (!fi.Directory!.Exists) fi.Directory.Create();

        var sb = new SqliteConnectionStringBuilder
        {
            DataSource = fileName,
        };
        using var destination = new SqliteConnection(sb.ToString());
        destination.Open();
        source.connection.BackupDatabase(destination);
    }

    [Obsolete("Use BackupDatabase instead")]
    public static void CreateBackup(this ISqliteConnection source, string fileName)
        => BackupDatabase(source, fileName);
}
