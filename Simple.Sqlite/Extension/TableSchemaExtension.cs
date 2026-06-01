namespace Simple.Sqlite;

using System.Data;
using System.Linq;

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
    /// Gets columns names for a table selecting 0 rows from it
    /// </summary>
    public static string[] GetTableColumnNames(this ISqliteConnection Connection, string tableName)
    {
        using var cmd = Connection.connection.CreateCommand();

        cmd.CommandText = $"SELECT * FROM {tableName} LIMIT 0";

        using var reader = cmd.ExecuteReader();
        return Enumerable.Range(0, reader.FieldCount)
                         .Select(reader.GetName)
                         .ToArray();
    }

    /// <summary>
    /// Get all tables querying sqlite_master for type='table'
    /// </summary>
    public static string[] GetAllTables(this ISqliteConnection Connection, bool include_sqlite_tables = true)
    {
        return Connection.Query<string>("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;", null)
                         .Where(o => include_sqlite_tables || !o.StartsWith("sqlite_"))
                         .ToArray();
    }

    /// <summary>
    /// Get all indexes
    /// </summary>
    public static string[] GetAllIndexes(this ISqliteConnection Connection, bool include_sqlite_indexes = true)
    {
        return Connection.Query<string>("SELECT name FROM sqlite_master WHERE type='index' ORDER BY name;", null)
                         .Where(o => include_sqlite_indexes || !o.StartsWith("sqlite_"))
                         .ToArray();
    }

    /// <summary>
    /// Get TableInfo using PRAGMA table_info function
    /// </summary>
    public static SqliteTableInfo[] GetTableInfo(this ISqliteConnection Connection, string tableName)
    {
        return Connection.Query<SqliteTableInfo>($"PRAGMA table_info({tableName});")
                         .ToArray();
    }

    /// <summary>
    /// Get TableList using PRAGMA table_list function
    /// </summary>
    public static SqliteTableList[] GetTableList(this ISqliteConnection Connection)
    {
        return Connection.Query<SqliteTableList>("PRAGMA table_list;")
                         .ToArray();
    }
}