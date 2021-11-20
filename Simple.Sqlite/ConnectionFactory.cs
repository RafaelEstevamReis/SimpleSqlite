using Microsoft.Data.Sqlite;
using Simple.Sqlite.Extension;
using System.IO;

namespace Simple.Sqlite
{
    /// <summary>
    /// Creates ISqliteConnection connections
    /// </summary>
    public static class ConnectionFactory
    {
        /// <summary>
        /// Creates a ISqliteConnection instance
        /// </summary>
        public static ISqliteConnection CreateConnection(string databaseFile)
        {
            var fi = new FileInfo(databaseFile);
            if (!fi.Directory.Exists) fi.Directory.Create();

            var sb = new SqliteConnectionStringBuilder
            {
                DataSource = databaseFile,
            };

            var SqliteConnection = new SqliteConnection(sb.ToString());
            SqliteConnection.Open();

            return new Connection()
            {
                typeCollection = new DatabaseWrapper.TypeReader.ReaderCachedCollection(),
                connection = SqliteConnection,
            };
        }
        /// <summary>
        /// Creates a ISqliteConnection instance from a Microsoft.Data.Sqlite.SqliteConnection
        /// </summary>
        public static ISqliteConnection CreateConnection(SqliteConnection connection)
        {
            return new Connection()
            {
                typeCollection = new DatabaseWrapper.TypeReader.ReaderCachedCollection(),
                connection = connection,
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
    }
}
