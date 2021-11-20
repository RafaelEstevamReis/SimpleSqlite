using System.Data;
using System.Linq;

namespace Simple.Sqlite.Extension
{
    public static class TableSchemaExtension
    {
        public static DataTable GetTableSchema(this ISqliteConnection Connection, string tableName)
        {
            using var cmd = Connection.connection.CreateCommand();

            cmd.CommandText = $"SELECT * FROM {tableName} LIMIT 0";

            var reader = cmd.ExecuteReader();
            var dt = reader.GetSchemaTable();

            return dt;
        }
        public static string[] GetAllTables(this ISqliteConnection Connection)
        {
            return Connection.Query<string>("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;", null).ToArray();
        }
    }
}
