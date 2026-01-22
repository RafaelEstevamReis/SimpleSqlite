namespace Simple.Sqlite;

using Simple.DatabaseWrapper.Attributes;
using System;

/// <summary>
/// A Simple KeyValue storage
/// </summary>
public class KeyValueStorage
{
    private readonly ConnectionFactory db;

    public KeyValueStorage(ConnectionFactory db)
    {
        this.db = db;

        using var cnn = db.GetConnection();
        cnn.CreateTables()
           .Add<KVStorageTable>()
           .Commit();
    }
    /// <summary>
    /// Sets a new KeyValue pair
    /// </summary>
    public void SetKey(string key, string value)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));
        }

        using var cnn = db.GetConnection();
        cnn.Insert(new KVStorageTable { Key = normalizeKey(key), Value = value }, OnConflict.Replace);
    }

    /// <summary>
    /// Gets the Value from a Key
    /// Inexistent keys returns as null
    /// </summary>
    /// <returns>Key's value or NULL</returns>
    public string? GetKey(string key)
    {
        using var cnn = db.GetConnection();
        var kvp = cnn.Get<KVStorageTable>("Key", normalizeKey(key));

        return kvp?.Value;
    }

    private static string normalizeKey(string key) => key.Trim().ToUpper();

    internal record KVStorageTable
    {
        [Unique]
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
