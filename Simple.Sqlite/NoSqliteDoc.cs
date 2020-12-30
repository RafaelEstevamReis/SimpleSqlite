using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Simple.Sqlite.Attributes;

namespace Simple.Sqlite
{
    /// <summary>
    /// Easy access a local no-sql document storage
    /// </summary>
    public class NoSqliteDoc
    {
        private SqliteDB internalDb;
        public bool CompressEachEntry { get; set; } = true;
        public string DatabaseFileName => internalDb.DatabaseFileName;

        public NoSqliteDoc(string fileName)
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

        public void Store<T>(string Key, T Object)
        {
            var doc = nsDocuments.Build(Key, Object, CompressEachEntry);
            internalDb.InsertOrReplace(doc);
        }
        public T Retrieve<T>(string Key)
        {
            var doc = internalDb.Get<nsDocuments>(Key);
            if (doc == null) return default(T);
            return doc.Unpack<T>();
        }
        public IEnumerable<string> GetAllKeys()
        {
            return internalDb.ExecuteQuery<object>("SELECT Id FROM nsDocuments", null)
                             .Cast<string>();
        }
    }

}
