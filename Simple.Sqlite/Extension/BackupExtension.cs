using Microsoft.Data.Sqlite;

namespace Simple.Sqlite.Extension
{    /// <summary>
     /// Extension for "Backup" related stuff
     /// </summary>
    public static class BackupExtension
    {
        /// <summary>
        ///  Backup of the connected database
        /// </summary>
        /// <param name="source">Source database</param>
        /// <param name="fileName">Destination database filename</param>
        public static void CreateBackup(this ISqliteConnection source, string fileName)
        {
            SqliteConnectionStringBuilder sb = new SqliteConnectionStringBuilder
            {
                DataSource = fileName,
                //Version = 3
            };
            using var destination = new SqliteConnection(sb.ToString());
            destination.Open();
            source.connection.BackupDatabase(destination); //, "main", "main", -1, null, 0);
        }
    }
}
