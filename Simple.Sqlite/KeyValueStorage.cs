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
    /// Creates a new KeyValueStorage using a Sqlite File
    /// </summary>
    public KeyValueStorage(string databaseFile)
        : this(ConnectionFactory.FromFile(databaseFile))
    { }

    /// <summary>
    /// Sets a new KeyValue pair for a category
    /// </summary>
    public void SetKey<T>(string category, string key, T? value)
        => SetKey(buildCategorizedKey(category, key), value);

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
    /// Gets the Value from a Key of a category
    /// </summary>
    public T? GetKey<T>(string category, string key, T @default)
        => GetKey<T>(buildCategorizedKey(category, key), @default);

    /// <summary>
    /// Gets the Value from a Key
    /// Inexistent keys returns as null
    /// </summary>
    /// <returns>Key's value or NULL</returns>
    public T? GetKey<T>(string key, T @default)
    {
        using var cnn = db.GetConnection();
        var values = cnn.Query<T>($"SELECT Value FROM KVStorageTable WHERE {nameof(KVStorageTable.Key)} = @Key", new { Key = normalizeKey(key) })
                        .ToArray();

        if (values.Length == 0) return @default;
        return values[0];
    }

    public static string buildCategorizedKey(string category, string key) => $"{category}::{key}";
    private static string normalizeKey(string key) => key.Trim().ToUpper();

    internal record KVStorageTable
    {
        [PrimaryKey]
        public string Key { get; set; } = default!;
        public object Value { get; set; } = default!;
    }
}
