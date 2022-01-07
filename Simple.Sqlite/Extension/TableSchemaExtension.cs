﻿using System.Data;
using System.Linq;

namespace Simple.Sqlite.Extension
{
    /// <summary>
    /// Extension for "TableSchemaExtension" stuff
    /// </summary>
    public static class TableSchemaExtension
    {
        /// <summary>
        /// Returns a DataTable containing the table schema
        /// </summary>
        public static DataTable GetTableSchema(this ISqliteConnection Connection, string tableName)
        {
            using var cmd = Connection.connection.CreateCommand();

            cmd.CommandText = $"SELECT * FROM {tableName} LIMIT 0";

            var reader = cmd.ExecuteReader();
            var dt = reader.GetSchemaTable();

            return dt;
        }
        /// <summary>
        /// Gets columns names for a table
        /// </summary>
        public static string[] GetTableColumnNames(this ISqliteConnection Connection, string tableName)
        {
            using var cmd = Connection.connection.CreateCommand();

            cmd.CommandText = $"SELECT * FROM {tableName} LIMIT 0";

            using var reader = cmd.ExecuteReader();
            return Enumerable.Range(0, reader.FieldCount)
                             .Select(idx => reader.GetName(idx))
                             .ToArray();
        }

        /// <summary>
        /// Get all tables
        /// </summary>
        public static string[] GetAllTables(this ISqliteConnection Connection)
        {
            return Connection.Query<string>("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;", null).ToArray();
        }
    }
}
