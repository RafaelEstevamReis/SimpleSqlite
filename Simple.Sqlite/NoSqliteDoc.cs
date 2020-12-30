using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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

        public void Insert<T>(string Key, T Object)
        {
            var doc = nsDocuments.Build<T>(Key, Object, false);
            internalDb.InsertOrReplace(doc);
        }
        public T Retrieve<T>(string Key)
        {
            var doc = internalDb.Get<nsDocuments>(Key);
            if (doc == null) return default(T);
            return doc.Unpack<T>();
        }
        public IEnumerable<string> GetAllIds()
        {
            return null;
        }

        private class nsDocuments
        {
            [PrimaryKey]
            public string Id { get; set; }
            public byte[] Object { get; set; }
            public bool Compressed { get; set; }

            internal T Unpack<T>()
            {
                var ms = new MemoryStream(Object);
                if (Compressed)
                {
                    using (var compressionStream = new DeflateStream(ms, CompressionMode.Decompress))
                    {
                        return deserialize<T>(compressionStream);
                    }
                }
                else
                {
                    return deserialize<T>(ms);
                }

            }
            private static T deserialize<T>(Stream ms)
            {
                using (var reader = new BsonDataReader(ms))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    return (T)serializer.Deserialize(reader, typeof(T));
                }
            }

            internal static nsDocuments Build<T>(string key, T obj, bool compress)
            {
                MemoryStream ms = new MemoryStream();

                if (compress)
                {
                    using (var compressionStream = new DeflateStream(ms, CompressionMode.Compress))
                    {
                        serialize(compressionStream, obj);
                    }
                }
                else
                {
                    serialize(ms, obj);
                }                              

                return new nsDocuments()
                {
                    Id = key,
                    Object = ms.ToArray(),
                    Compressed = compress,
                };
            }

            private static void serialize<T>(Stream ms, T obj)
            {
                using (var writer = new BsonDataWriter(ms))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(writer, obj);
                }
            }

        }
    }

}
