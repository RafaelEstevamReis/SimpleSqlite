using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace Simple.Sqlite
{
    /// <summary>
    /// Creates ISqliteConnection connections
    /// </summary>
    public class ConnectionFactory
    {
        public static bool HandleGuidAsByteArray
        {
            get => HelperFunctions.handleGuidAsByteArray;
            set => HelperFunctions.handleGuidAsByteArray = value;
        }

        /// <summary>
        /// Current connection string
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Factory constructor
        /// </summary>
        public ConnectionFactory(SqliteConnectionStringBuilder builder)
            : this(builder.ToString())
        { }

        /// <summary>
        /// Factory constructor
        /// </summary>
        public ConnectionFactory(string cnnString)
        {
            if (string.IsNullOrWhiteSpace(cnnString))
            {
                throw new ArgumentException($"'{nameof(cnnString)}' cannot be null or whitespace.", nameof(cnnString));
            }
            ConnectionString = cnnString;
        }

        /// <summary>
        /// Opens a connection to the database
        /// </summary>
        /// <returns>An open connection of the database</returns>
        public ISqliteConnection GetConnection()
        {
            var SqliteConnection = new SqliteConnection(ConnectionString);
            SqliteConnection.Open();

            return new Connection()
            {
                typeCollection = new DatabaseWrapper.TypeReader.ReaderCachedCollection(),
                connection = SqliteConnection,
            };
        }

        /// <summary>
        /// Creates a Factory to a sqlite file
        /// </summary>
        public static ConnectionFactory FromFile(string databaseFile, bool readOnly = false)
        {
            var cnnString = sqliteFileToCnnString(databaseFile, readOnly ? SqliteOpenMode.ReadOnly : SqliteOpenMode.ReadWriteCreate);
            return new ConnectionFactory(cnnString);
        }

        protected static string sqliteFileToCnnString(string databaseFile, SqliteOpenMode mode = SqliteOpenMode.ReadWriteCreate)
        {
            var fi = new FileInfo(databaseFile);
            if (!fi.Directory.Exists) fi.Directory.Create();

            var sb = new SqliteConnectionStringBuilder
            {
                DataSource = databaseFile,
                Mode = mode
            };
            return sb.ToString();
        }

        /// <summary>
        /// Creates a ISqliteConnection instance from a file
        /// </summary>
        public static ISqliteConnection CreateConnection(string databaseFile)
        {
            var cnnString = sqliteFileToCnnString(databaseFile);
            var SqliteConnection = new SqliteConnection(cnnString);
            SqliteConnection.Open();

            return new Connection()
            {
                typeCollection = new DatabaseWrapper.TypeReader.ReaderCachedCollection(),
                connection = SqliteConnection,
            };
        }
       
        /// <summary>
        /// Opens a non-Shared in memory connection
        /// </summary>
        public static ISqliteConnection CreateInMemory()
        {
            var SqliteConnection = new SqliteConnection("Data Source=:memory:");
            SqliteConnection.Open();

            return new Connection()
            {
                typeCollection = new DatabaseWrapper.TypeReader.ReaderCachedCollection(),
                connection = SqliteConnection,
            };
        }
        /// <summary>
        /// Opens a non-Shared in memory connection
        /// </summary>
        /// <param name="sharedName">Data source shared name</param>
        public static ISqliteConnection CreateInMemoryShared(string sharedName)
        {
            var SqliteConnection = new SqliteConnection($"Data Source={sharedName};Mode=Memory;Cache=Shared");
            SqliteConnection.Open();

            return new Connection()
            {
                typeCollection = new DatabaseWrapper.TypeReader.ReaderCachedCollection(),
                connection = SqliteConnection,
            };
        }

        /// <summary>
        /// Some operational systems do not support DELETE journal mode (like Azure linux AppServices)
        /// This method creates a new database file already in WAL mode
        /// </summary>
        /// <param name="filename">Filename for new database</param>
        /// <returns>True if a new file was created, false if a file already exists</returns>
        public static bool CreateEmptyWalDB(string filename)
        {
            if (File.Exists(filename)) return false;
            var bytesRes = BinaryResources.getEmptyV3_WAL();
            File.WriteAllBytes(filename, bytesRes);

            return true;
        }

#if DEBUG
        // BinaryResources builder functions
        public static bool checkEquals(string fileName)
        {
            var bytesOrg = File.ReadAllBytes(fileName);
            var bytesRes = BinaryResources.getEmptyV3_WAL();

            if (bytesOrg.Length != bytesRes.Length) return false;

            for (int i = 0; i < bytesOrg.Length; i++)
            {
                if (bytesOrg[i] != bytesRes[i]) return false;
            }
            return true;
        }
        public static void buidEmptyWalFile(string fileName)
        {
            string tmpFileName = "tmp.db.gz";

            using (FileStream originalFileStream = File.Open(fileName, FileMode.Open))
            {
                using FileStream compressedFileStream = File.Create(tmpFileName);
                using var compressor = new GZipStream(compressedFileStream, (CompressionLevel)3);
                originalFileStream.CopyTo(compressor);
            }

            var bytesGz = File.ReadAllBytes(tmpFileName);
            var sb = new StringBuilder();

            sb.AppendLine("byte[] source = new byte[] {");
            for (int i = 0; i < bytesGz.Length; i++)
            {
                if (i > 0) sb.Append(", ");
                if (i % 16 == 0 && i > 0) sb.AppendLine();

                sb.Append("0x");
                sb.Append(bytesGz[i].ToString("X2"));
            }
            sb.AppendLine();
            sb.AppendLine("}");

            File.WriteAllText("source.txt", sb.ToString());
        }
#endif

    }
}
