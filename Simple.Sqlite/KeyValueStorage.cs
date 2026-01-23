namespace Simple.Sqlite;

using Simple.DatabaseWrapper.Attributes;
using System;
using System.Linq;

/// <summary>
/// A Simple KeyValue storage
/// </summary>
public class KeyValueStorage
{
    private readonly ConnectionFactory db;

    /// <summary>
    /// Creates a new KeyValueStorage using a ConnectionFactory
    /// </summary>
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
    public void SetKey<T>(string key, T? value)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException($"'{nameof(key)}' cannot be null or empty.", nameof(key));
        }

        using var cnn = db.GetConnection();

        if (value == null)
        {
            cnn.Execute($"DELETE FROM KVStorageTable WHERE {nameof(KVStorageTable.Key)} = @key", new { key });
        }
        else
        {
            cnn.Insert(new KVStorageTable { Key = normalizeKey(key), Value = value }, OnConflict.Replace);
        }
    }

    /// <summary>
    /// Gets the Value from a Key
    /// Inexistent keys returns as null
    /// </summary>
    /// <returns>Key's value or NULL</returns>
    public T? GetKey<T>(string key)
    {
        using var cnn = db.GetConnection();
        var values = cnn.Query<T>($"SELECT Value FROM KVStorageTable WHERE {nameof(KVStorageTable.Key)} = @Key", new { Key = normalizeKey(key) })
                        .ToArray();

        if (values.Length == 0) return default;
        return values[0];
    }

    private static string normalizeKey(string key) => key.Trim().ToUpper();

    internal record KVStorageTable
    {
        [PrimaryKey]
        public string Key { get; set; } = default!;
        public object Value { get; set; } = default!;
    }
}
