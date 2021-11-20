using Microsoft.Data.Sqlite;
using Simple.Sqlite.Extension;
using System.IO;

namespace Simple.Sqlite
{
    public static class ConnectionFactory
    {
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
    }
}
