using System.Data;

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

    }
}
