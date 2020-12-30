using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace Simple.Sqlite
{
    /// <summary>
    /// Easy access a local database
    /// How to use: Create new instance, call CreateTables(), chain Add[T] to add tables to it then Commit(), after that just call the other methods
    /// </summary>
    public class SqliteDB
    {
        // Manual lock on Writes to avoid Exceptions
        private readonly object lockNonQuery;
        private readonly string cnnString;

        /// <summary>
        /// Database file full path
        /// </summary>
        public string DatabaseFileName { get; }
        /// <summary>
        /// Creates a new instance
        /// </summary>
        public SqliteDB(string fileName)
        {
            lockNonQuery = new object();
            DatabaseFileName = new FileInfo(fileName).FullName;
            // if now exists, creates one (can be done in the ConnectionString)
            if (!File.Exists(DatabaseFileName)) SQLiteConnection.CreateFile(DatabaseFileName);

            // uses builder to avoid escape issues
            SQLiteConnectionStringBuilder sb = new SQLiteConnectionStringBuilder
            {
                DataSource = DatabaseFileName,
                Version = 3
            };
            cnnString = sb.ToString();
        }

        private SQLiteConnection getConnection()
        {
            var sqliteConnection = new SQLiteConnection(cnnString);
            sqliteConnection.Open();
            return sqliteConnection;
        }
        /// <summary>
        /// Builds the table creation sequence, should be finished with Commit()
        /// </summary>
        public TableMapper CreateTables()
        {
            return new TableMapper(this);
        }
        /// <summary>
        /// Get a list of all tables
        /// </summary>
        public string[] GetAllTables()
        {
            var dt = ExecuteReader(@"SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;", null);
            return dt.Rows.Cast<DataRow>()
                          .Select(row => (string)row[0])
                          .ToArray();
        }
        /// <summary>
        /// Gets the schema for a table
        /// </summary>
        public DataTable GetTableSchema(string TableName)
        {
            using var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = $"SELECT * FROM {TableName} LIMIT 0";

            var reader = cmd.ExecuteReader();

            return reader.GetSchemaTable();
        }
        /// <summary>
        /// Executes a NonQUery command, this method locks the execution
        /// </summary>
        public int ExecuteNonQuery(string Text, object Parameters = null)
        {
            using var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = Text;
            fillParameters(cmd, Parameters);

            lock (lockNonQuery)
            {
                return cmd.ExecuteNonQuery();
            }
        }
        /// <summary>
        /// Executes a Scalar commands and return the value as T
        /// </summary>
        public T ExecuteScalar<T>(string Text, object Parameters)
        {
            using var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = Text;
            fillParameters(cmd, Parameters);

            var obj = cmd.ExecuteScalar();

            // In SQLite DateTime is returned as STRING after aggregate operations
            if (typeof(T) == typeof(DateTime))
            {
                if (DateTime.TryParse(obj.ToString(), out DateTime dt))
                {
                    return (T)(object)dt;
                }
                return default;
            }

            return (T)Convert.ChangeType(obj, typeof(T));
        }
        /// <summary>
        /// Executes a query and returns as DataTable
        /// </summary>
        public DataTable ExecuteReader(string Text, object Parameters)
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
        /// <summary>
        /// Executes a query and returns the value as a T collection
        /// </summary>
        public IEnumerable<T> ExecuteQuery<T>(string Text, object Parameters)
            where T : new()
        {
            var typeT = typeof(T);
            using var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = Text;
            fillParameters(cmd, Parameters);

            using var reader = cmd.ExecuteReader();

            if (reader.HasRows)
            {
                var colNames = getSchemaColumns(reader);

                while (reader.Read())
                {
                    // build new
                    object t = new T();
                    if (checkIfSimpleType(  typeT))
                    {
                        t = readValue(reader, typeT, colNames.First());
                    }
                    else
                    {
                        foreach (var p in typeof(T).GetProperties())
                        {
                            if (!colNames.Contains(p.Name)) continue;

                            mapColumn(t, p, reader);
                        }
                    }
                    yield return (T)t;
                }
            }
        }

        static bool checkIfSimpleType(Type typeT)
        {
            if (typeT.IsPrimitive) return true;
            if (typeT == typeof(string)) return true;
            if (typeT == typeof(decimal)) return true;
            if (typeT == typeof(DateTime)) return true;
            if (typeT == typeof(DateTimeOffset)) return true;
            if (typeT == typeof(TimeSpan)) return true;
            if (typeT == typeof(Guid)) return true;

            if (IsNullableSimpleType(typeT)) return true;

            return false;
        }
        static bool IsNullableSimpleType(Type typeT)
        {
            var underlyingType = Nullable.GetUnderlyingType(typeT);
            return underlyingType != null && checkIfSimpleType(underlyingType);
        }

        private static void mapColumn<T>(T obj, System.Reflection.PropertyInfo p, SQLiteDataReader reader)
            where T : new()
        {
            object objVal = readValue(reader, p.PropertyType, p.Name);
            p.SetValue(obj, objVal);
        }
        private static object readValue(SQLiteDataReader reader, Type type, string name)
        {
            object objVal;
            if (reader.IsDBNull(name))
            {
                objVal = null;
            }
            else
            {
                if (type == typeof(string)) objVal = reader.GetValue(name);
                else if (type == typeof(Uri)) objVal = new Uri((string)reader.GetValue(name));
                else if (type == typeof(double)) objVal = reader.GetDouble(name);
                else if (type == typeof(float)) objVal = reader.GetFloat(name);
                else if (type == typeof(decimal)) objVal = reader.GetDecimal(name);
                else if (type == typeof(int)) objVal = reader.GetInt32(name);
                else if (type == typeof(uint)) objVal = Convert.ToUInt32(reader.GetValue(name));
                else if (type == typeof(long)) objVal = reader.GetInt64(name);
                else if (type == typeof(ulong)) objVal = Convert.ToUInt64(reader.GetValue(name));
                else if (type == typeof(bool)) objVal = reader.GetBoolean(name);
                else if (type == typeof(DateTime)) objVal = reader.GetDateTime(name);
                else if (type == typeof(byte[])) objVal = (byte[])reader.GetValue(name);
                else if (type == typeof(Guid)) objVal = new Guid((byte[])reader.GetValue(name));
                else if (type.IsEnum) objVal = reader.GetInt32(name);
                else objVal = reader.GetValue(name);
            }
            return objVal;
        }


        /// <summary>
        /// Gets a single T with specified table KeyValue on KeyColumn
        /// </summary>
        public T Get<T>(object KeyValue)
                 where T : new()
        {
            return Get<T>(null, KeyValue);
        }
        /// <summary>
        /// Gets a single T with specified table KeyValue on KeyColumn
        /// </summary>
        public T Get<T>(string KeyColumn, object KeyValue)
            where T : new()
        {
            var TypeT = typeof(T);

            string keyColumn = KeyColumn
                            ?? TableMapper.Column.GetKeyColumn(TypeT)
                            ?? "_rowid_";

            var tableName = TypeT.Name;

            return ExecuteQuery<T>($"SELECT * FROM {tableName} WHERE {keyColumn} = @KeyValue ", new { KeyValue })
                    .FirstOrDefault();
        }
        /// <summary>
        /// Queries the database to all T rows in the table
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public IEnumerable<T> GetAll<T>()
            where T : new()
        {
            var tableName = typeof(T).Name;

            return ExecuteQuery<T>($"SELECT * FROM {tableName} ", null);
        }

        private HashSet<string> getSchemaColumns(SQLiteDataReader reader)
        {
            var schema = reader.GetSchemaTable();
            return schema.Rows
                .Cast<DataRow>()
                .Select(r => (string)r["ColumnName"])
                .ToHashSet();
        }

        /// <summary>
        /// Inserts a new T and return it's ID, this method locks the execution
        /// </summary>
        /// <returns>Returns `sqlite3:last_insert_rowid()`</returns>
        public long Insert<T>(T Item)
        {
            string sql = buildInsertSql<T>();
            return ExecuteScalar<long>(sql, Item);
        }
        private static string buildInsertSql<T>(bool addReplace = false)
        {
            var TypeT = typeof(T);
            var tableName = TypeT.Name;

            var names = getNames(TypeT, isInsert: true);
            var fields = string.Join(',', names);
            var values = string.Join(',', names.Select(n => $"@{n}"));

            if (addReplace)
            {
                return $"INSERT OR REPLACE INTO {tableName} ({fields}) VALUES ({values});";
            }
            else
            {
                return $"INSERT INTO {tableName} ({fields}) VALUES ({values}); SELECT last_insert_rowid();";
            }
        }
        /// <summary>
        /// Inserts many T items into the database and return their IDs, this method locks the execution
        /// </summary>
        public long[] BulkInsert<T>(IEnumerable<T> Items)
        {
            List<long> ids = new List<long>();
            string sql = buildInsertSql<T>();

            using var cnn = getConnection();

            lock (lockNonQuery)
            {
                using var trn = cnn.BeginTransaction();

                foreach (var item in Items)
                {
                    using var cmd = new SQLiteCommand(sql, cnn, trn);
                    fillParameters(cmd, item);
                    ids.Add((long)cmd.ExecuteScalar());
                }

                trn.Commit();
            }
            return ids.ToArray();
        }
        /// <summary>
        /// Inserts a new T or replace with current T and return it's ID, this method locks the execution
        /// Must have a [Unique] or PK column
        /// </summary>
        public void InsertOrReplace<T>(T Item)
        {
            string sql = buildInsertSql<T>(addReplace: true);
            ExecuteNonQuery(sql, Item);
        }

        private static void fillParameters(SQLiteCommand cmd, object Parameters)
        {
            if (Parameters == null) return;
            foreach (var p in Parameters.GetType().GetProperties())
            {
                cmd.Parameters.AddWithValue(p.Name, p.GetValue(Parameters));
            }
        }
        private static IEnumerable<string> getNames(Type type, bool isInsert = true)
        {
            var keyName = TableMapper.Column.GetKeyColumn(type);
            foreach (var info in type.GetProperties())
            {
                if (isInsert && info.Name == keyName)
                {
                    if (info.PropertyType.FullName == "System.Guid")
                    {
                        // Keep Guids
                    }
                    else
                    {
                        continue;
                    }
                }

                yield return info.Name;
            }
        }
    }
}
