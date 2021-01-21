using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.Sqlite
{
    /// <summary>
    /// Easy access a local no-sql document storage
    /// </summary>
    public class NoSqliteStorage
    {
        private SqliteDB internalDb;
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
        {
            internalDb = new SqliteDB(fileName);
            createDocumentTable();
        }

        private void createDocumentTable()
        {
            internalDb.ExecuteNonQuery(
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
            if (doc == null) return default(T);
            return doc.Unpack<T>();
        }
        /// <summary>
        /// Retrieves all stored Keys
        /// </summary>
        public IEnumerable<string> GetAllKeys()
        {
            return internalDb.ExecuteQuery<string>("SELECT Id FROM nsDocuments", null);
        }
        /// <summary>
        /// Retrieves all stored Guids
        /// </summary>
        public IEnumerable<Guid> GetAllGuids()
        {
            return internalDb.ExecuteQuery<Guid>("SELECT Id FROM nsDocuments", null);
        }
    }
}
