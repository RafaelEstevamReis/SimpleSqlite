#if !NET40

using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Simple.DatabaseWrapper.Attributes;

namespace Simple.Sqlite
{
    internal class nsDocuments
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
#endif
