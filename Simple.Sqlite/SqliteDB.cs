using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace Simple.Sqlite
{
    /// <summary>
    /// Easy access a local database
    /// </summary>
    public class SqliteDB
    {
        public string DatabaseFileName { get; }

        public SqliteDB(string DatabaseFile)
        {
            var fi = new FileInfo(DatabaseFile);
            DatabaseFileName = fi.FullName;
            // if now exists, creates one (can be done in the ConnectionString)
            if (!fi.Exists) SQLiteConnection.CreateFile(DatabaseFileName);
        }

        #region Database initialization and manage
        /// <summary>
        /// Creates a table if it not exists, returns total number of tables created
        /// </summary>
        public int CreateTables(SimpleTableSchema[] tables)
        {
            int tablesCreated = 0;
            foreach (var table in tables)
            {
                var sql = table.ExportCreateTableStatement(true);

                int result = ExecuteNonReader(sql, null);
                // -1: Table already exists
                if (result == 0) tablesCreated++;
            }
            return tablesCreated;
        }

        public string[] GetAllTables()
        {
            var dt = ExecuteReader(@"SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;", null);
            return dt.Rows.Cast<DataRow>()
                          .Select(row => (string)row[0])
                          .ToArray();
        }
        public DataTable GetTableSchema(string TableName)
        {
            using var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = $"SELECT * FROM {TableName} LIMIT 0";

            var reader = cmd.ExecuteReader();

            return reader.GetSchemaTable();
        }
        #endregion

        #region Basic Sql Operations
        private SQLiteConnection getConnection()
        {
            // uses builder to avoid escape issues
            SQLiteConnectionStringBuilder sb = new SQLiteConnectionStringBuilder
            {
                DataSource = DatabaseFileName,
                Version = 3
            };

            var sqliteConnection = new SQLiteConnection(sb.ToString());
            sqliteConnection.Open();
            return sqliteConnection;
        }

        public int ExecuteNonReader(string Text, Dictionary<string, object> Parameters)
        {
            using var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = Text;
            fillParameters(cmd, Parameters);

            return cmd.ExecuteNonQuery();
        }
        public DataTable ExecuteReader(string Text, Dictionary<string, object> Parameters)
        {
            using var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = Text;
            fillParameters(cmd, Parameters);

            DataTable dt = new DataTable();
            var da = new SQLiteDataAdapter(cmd.CommandText, cnn);
            da.Fill(dt);
            return dt;
        }

        private static void fillParameters(SQLiteCommand cmd, Dictionary<string, object> parameters)
        {
            if (parameters == null) return;

            foreach (var p in parameters)
            {
                cmd.Parameters.AddWithValue(p.Key, p.Value);
            }
        }
        #endregion

        #region Helpers
        /// <summary>
        /// Exports all object properties as Parameteres
        /// </summary>
        public Dictionary<string, object> BuildParameters<T>(T Object) where T : new()
        {
            var dic = new Dictionary<string, object>();
            foreach (var p in typeof(T).GetProperties())
            {
                dic.Add(p.Name, p.GetValue(Object));
            }
            return dic;
        }

        #endregion

        #region Automatic Type actions
        /// <summary>
        /// Inserts an Object into a table converting all properties to parameters
        /// </summary>
        public int InsertInto<T>(string TableName, T Object) where T : new()
        {
            // build parameters
            var pars = BuildParameters(Object);

            string values = string.Join(',', pars.Select(o => $"@{o.Key}"));
            string fields = string.Join(',', pars.Select(o => o.Key));

            // build Sql
            string sql = $"INSERT INTO {TableName} ({fields}) VALUES ({values})";

            // execute
            return ExecuteNonReader(sql, pars);
        }
        /// <summary>
        /// Inserts all objects in a single transaction
        /// </summary>
        public void BulkInsertInto<T>(string TableName, IEnumerable<T> Objects) where T : new()
        {
            using var cnn = getConnection();
            using var tr = cnn.BeginTransaction();

            var objProps = typeof(T).GetProperties();
            var values = string.Join(',', objProps.Select(o => $"@{o.Name}"));
            var fields = string.Join(',', objProps.Select(o => o.Name));

            // build Sql
            var sql = $"INSERT INTO {TableName} ({fields}) VALUES ({values})";

            foreach (var o in Objects)
            {
                using var cmd = new SQLiteCommand(sql, cnn, tr);

                foreach (var p in objProps)
                {
                    cmd.Parameters.AddWithValue(p.Name, p.GetValue(o));
                }

                cmd.ExecuteNonQuery();
            }
            tr.Commit();
        }

        /// <summary>
        /// Executes a query and parse the results as an IEnumerable of T objects
        /// </summary>
        public IEnumerable<T> ExecuteQuery<T>(string Text, Dictionary<string, object> Parameters) where T : new()
        {
            using var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = Text;
            fillParameters(cmd, Parameters);

            var reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                var schema = reader.GetSchemaTable();
                var colNames = schema.Rows
                    .Cast<DataRow>()
                    .Select(r => (string)r["ColumnName"])
                    .ToArray();

                while (reader.Read())
                {
                    // build new
                    T t = new T();

                    foreach (var p in typeof(T).GetProperties())
                    {
                        if (!colNames.Contains(p.Name)) continue;

                        object objVal;

                        if (p.PropertyType == typeof(int)) objVal = reader.GetInt32(p.Name);
                        else if (p.PropertyType == typeof(DateTime)) objVal = reader.GetDateTime(p.Name);
                        else if (p.PropertyType == typeof(double)) objVal = reader.GetDouble(p.Name);
                        else if (p.PropertyType == typeof(float)) objVal = reader.GetFloat(p.Name);
                        else if (p.PropertyType == typeof(bool)) objVal = reader.GetBoolean(p.Name);
                        else if (p.PropertyType == typeof(long)) objVal = reader.GetInt64(p.Name);
                        else objVal = reader.GetValue(p.Name);
                        
                        if (objVal is DBNull) objVal = null;

                        p.SetValue(t, objVal);
                    }
                    yield return t;
                }
            }

        }
        #endregion
    }
}
