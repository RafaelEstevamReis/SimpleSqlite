using Microsoft.Data.Sqlite;
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

            return new Extension.Connection()
            {
                typeCollection = new DatabaseWrapper.TypeReader.ReaderCachedCollection(),
                connection = SqliteConnection,
            };
        }
    }
}
