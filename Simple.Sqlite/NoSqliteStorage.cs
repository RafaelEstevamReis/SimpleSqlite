#if !NET40

namespace Simple.Sqlite;

using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Easy access a local no-sql document storage
/// </summary>
public class NoSqliteStorage : IEnumerable<string>
{
    /// <summary>
    /// Exposes the internal database engine
    /// </summary>
    internal protected SqliteDB internalDb;

    /// <summary>
    /// Specify if new values should be compressed before storage
    /// </summary>
    public bool CompressEachEntry { get; set; } = true;
    /// <summary>
    /// Database file full path
    /// </summary>
    public string DatabaseFileName => internalDb.DatabaseFileName;

    /// <summary>
    /// Creates a new instance
    /// </summary>
    public NoSqliteStorage(string fileName)
        : this(new SqliteDB(fileName))
    { }
    private NoSqliteStorage(SqliteDB internalDb)
    {
        this.internalDb = internalDb;
        createDocumentTable();
    }

    private void createDocumentTable()
    {
        internalDb.Execute(
@"CREATE TABLE IF NOT EXISTS nsDocuments (
    Id         TEXT NOT NULL,
    Object     BLOB,
    Compressed BOOL DEFAULT(0) NOT NULL,
    PRIMARY KEY(Id)
);");
    }
    /// <summary>
    /// Stores a new item
    /// </summary>
    /// <typeparam name="T">Type of stored item</typeparam>
    /// <param name="Key">A Key to locate the item later</param>
    /// <param name="Object">The item to be stored</param>
    public void Store<T>(Guid Key, T Object) => Store(Key.ToString(), Object);

    /// <summary>
    /// Stores a new item
    /// </summary>
    /// <typeparam name="T">Type of stored item</typeparam>
    /// <param name="Key">A Key to locate the item later</param>
    /// <param name="Object">The item to be stored</param>
    public void Store<T>(string Key, T Object)
    {
        var doc = nsDocuments.Build(Key, Object, CompressEachEntry);
        internalDb.InsertOrReplace(doc);
    }
    /// <summary>
    /// Stores many items
    /// </summary>
    public void Store<T>(IEnumerable<T> Objects, Func<T, Guid> keySelector)
    {
        Store(Objects, t => keySelector(t).ToString());
    }
    /// <summary>
    /// Stores many items
    /// </summary>
    public void Store<T>(IEnumerable<T> Objects, Func<T, string> keySelector)
    {
        var docs = Objects.Select(d => nsDocuments.Build(keySelector(d), d, CompressEachEntry));
        internalDb.BulkInsert(docs, true);
    }

    /// <summary>
    /// Retrieves a stored item
    /// </summary>
    /// <typeparam name="T">Type of stored item</typeparam>
    /// <param name="Key">The Key to locate the stored item</param>
    /// <returns>Stored item or Defult(T)</returns>
    public T Retrieve<T>(Guid Key) => Retrieve<T>(Key.ToString());
    /// <summary>
    /// Retrieves a stored item
    /// </summary>
    /// <typeparam name="T">Type of stored item</typeparam>
    /// <param name="Key">The Key to locate the stored item</param>
    /// <returns>Stored item or Defult(T)</returns>
    public T Retrieve<T>(string Key)
    {
        var doc = internalDb.Get<nsDocuments>(Key);
        if (doc == null) return default(T)!;
        return doc.Unpack<T>();
    }
    /// <summary>
    /// Remove a stored item
    /// </summary>
    public void Remove(Guid Key) => Remove(Key.ToString());
    /// <summary>
    /// Remove a stored item
    /// </summary>
    public void Remove(string Key)
    {
        internalDb.Execute("DELETE FROM nsDocuments WHERE Id = @id", new { id = Key });
    }

    /// <summary>
    /// Retrieves all stored Keys
    /// </summary>
    public IEnumerable<string> GetAllKeys()
    {
        return internalDb.Query<string>("SELECT Id FROM nsDocuments", null);
    }
    /// <summary>
    /// Retrieves all stored Guids
    /// </summary>
    public IEnumerable<Guid> GetAllGuids()
    {
        return internalDb.Query<Guid>("SELECT Id FROM nsDocuments", null);
    }

    /// <summary>
    /// Create a new instance based on an existing ConfigurationDB
    /// </summary>
    public static NoSqliteStorage FromDB(ConfigurationDB cfg)
    {
        return new NoSqliteStorage(cfg.internalDb);
    }
    /// <summary>
    /// Create a new instance based on an existing NoSqliteStorage
    /// </summary>
    public static NoSqliteStorage FromDB(NoSqliteStorage nss)
    {
        return new NoSqliteStorage(nss.internalDb);
    }
    /// <summary>
    /// Create a new instance based on an existing SqliteDB
    /// </summary>
    public static NoSqliteStorage FromDB(SqliteDB db)
    {
        return new NoSqliteStorage(db);
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    public IEnumerator<string> GetEnumerator()
    {
        return GetAllKeys().GetEnumerator();
    }
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetAllKeys().GetEnumerator();
    }
}
#endif
