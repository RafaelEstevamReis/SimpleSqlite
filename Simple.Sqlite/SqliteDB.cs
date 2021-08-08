using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Microsoft.Data.Sqlite;
using Simple.DatabaseWrapper.Helpers;
using Simple.DatabaseWrapper.Interfaces;
using Simple.DatabaseWrapper.TypeReader;

namespace Simple.Sqlite
{
    /// <summary>
    /// Easy access a local database
    /// How to use: Create new instance, call CreateTables(), chain Add[T] to add tables to it then Commit(), after that just call the other methods
    /// </summary>
    public class SqliteDB
    {
        /// <summary>
        /// Allows any instance of SqliteDB to execute a backup of the current database
        /// </summary>
        public static bool EnabledDatabaseBackup = true;

        // Manual lock on Writes to avoid Exceptions
        private readonly object lockNonQuery;
        private readonly string cnnString;
        private readonly ReaderCachedCollection typeCollection;

        #region In Memory
        /// <summary>
        /// Gets if this instance is an InMemoryDatabase
        /// </summary>
        public bool IsInMemoryDatabase { get; private set; }
        SqliteConnection permanentConnection;
        #endregion

        /// <summary>
        /// Database file full path
        /// </summary>
        public string DatabaseFileName { get; }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        public SqliteDB(string fileName)
        {
            DatabaseFileName = new FileInfo(fileName).FullName;
            // if now exists, creates one (can be done in the ConnectionString)

            //if (!File.Exists(DatabaseFileName)) SqliteConnection.CreateFile(DatabaseFileName);
            //else backupDatabase();
            if (File.Exists(DatabaseFileName)) backupDatabase();

            // uses builder to avoid escape issues
            SqliteConnectionStringBuilder sb = new SqliteConnectionStringBuilder
            {
                DataSource = DatabaseFileName,
                //Version = 3
            };

            cnnString = sb.ToString();
            typeCollection = new ReaderCachedCollection();
            lockNonQuery = new object();
        }
        private SqliteDB(SqliteConnectionStringBuilder sb, string databaseFileName)
        {
            DatabaseFileName = databaseFileName;
            cnnString = sb.ToString();
            typeCollection = new ReaderCachedCollection();
            lockNonQuery = new object();
        }

        private void backupDatabase()
        {
            if (!EnabledDatabaseBackup) return;

            var temp = Path.GetTempFileName();
            using var fsInput = File.Open(DatabaseFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using (var fsTempOut = File.OpenWrite(temp))
            {
                using var compressionStream = new GZipStream(fsTempOut, CompressionMode.Compress);
                fsInput.CopyTo(compressionStream);
            }
            string bkp = $"{DatabaseFileName}.bak.gz";
            string bkpOld = $"{DatabaseFileName}.old.gz";

            // DO NOT replace this with "File.Replace"
            if (File.Exists(bkpOld)) File.Delete(bkpOld);
            if (File.Exists(bkp)) File.Move(bkp, bkpOld);
            File.Move(temp, bkp);
        }

        private SqliteConnection getNewConnection()
        {
            var SqliteConnection = new SqliteConnection(cnnString);
            SqliteConnection.Open();
            return SqliteConnection;
        }
        private SqliteConnection getConnection()
        {
            if (IsInMemoryDatabase)
            {
                if (permanentConnection == null)
                    permanentConnection = getNewConnection();

                return permanentConnection;
            }
            else
            {
                return getNewConnection();
            }
        }

        /// <summary>
        /// Builds the table creation sequence, should be finished with Commit()
        /// </summary>
        public ITableMapper CreateTables()
        {
            return new TableMapper(this, typeCollection);
        }
        /// <summary>
        /// Get a list of all tables
        /// </summary>
        public string[] GetAllTables()
        {
            return Query<string>(@"SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;", null).ToArray();
        }
        /// <summary>
        /// Gets the schema for a table
        /// </summary>
        public DataTable GetTableSchema(string tableName)
        {
            var cnn = getConnection();

            using var cmd = cnn.CreateCommand();

            cmd.CommandText = $"SELECT * FROM {tableName} LIMIT 0";

            var reader = cmd.ExecuteReader();
            var dt = reader.GetSchemaTable();

            if (!IsInMemoryDatabase) cnn.Close();
            return dt;
        }

        /// <summary>
        /// Use 'Execute' instead
        /// </summary>
        [Obsolete("Use 'Execute' instead")]
        public int ExecuteNonQuery(string text, object parameters = null) => Execute(text, parameters);

        /// <summary>
        /// Executes a NonQuery command, this method locks the execution
        /// </summary>
        public int Execute(string text, object parameters = null)
        {
            var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = text;
            fillParameters(cmd, parameters);

            lock (lockNonQuery)
            {
                int val = cmd.ExecuteNonQuery();
                if (!IsInMemoryDatabase) cnn.Close();
                return val;
            }
        }
        /// <summary>
        /// Executes a Scalar commands and return the value as T
        /// </summary>
        public T ExecuteScalar<T>(string text, object parameters)
        {
            var cnn = getConnection();

            using var cmd = cnn.CreateCommand();

            cmd.CommandText = text;
            fillParameters(cmd, parameters);

            var obj = cmd.ExecuteScalar();
            if (!IsInMemoryDatabase) cnn.Close();

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

        /*

        /// <summary>
        /// Executes a query and returns as DataTable
        /// </summary>
        public DataTable ExecuteReader(string text, object parameters)
        {
            var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = text;
            fillParameters(cmd, parameters);

            DataTable dt = new DataTable();
            using var da = new SqliteDataAdapter(cmd.CommandText, cnn);
            da.Fill(dt);

            if (!IsInMemoryDatabase) cnn.Close();

            return dt;
        }
        */

        /// <summary>
        /// Executes a query and returns the value as a T collection
        /// </summary>
        public IEnumerable<T> Query<T>(string text, object parameters)
        {
            var cnn = getConnection();

            var typeT = typeof(T);
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = text;
            fillParameters(cmd, parameters);

            using var reader = cmd.ExecuteReader();

            if (!reader.HasRows)
            {
                if (!IsInMemoryDatabase) cnn.Close();
                yield break;
            }

            var colNames = getSchemaColumns(reader);
            while (reader.Read())
            {
                // build new
                if (typeT.CheckIfSimpleType())
                {
                    yield return (T)TypeMapper.ReadValue(reader, typeT, 0);
                }
                else
                {
                    yield return TypeMapper.MapObject<T>(colNames, reader, typeCollection);
                }
            }
            if (!IsInMemoryDatabase) cnn.Close();
        }

        /// <summary>
        /// Use 'Query' instead
        /// </summary>
        [Obsolete("Use 'Query' instead")]
        public IEnumerable<T> ExecuteQuery<T>(string text, object parameters)
            => Query<T>(text, parameters);

        /// <summary>
        /// Executes a query and returns the result the first T, 
        /// or InvalidOperationException if empty
        /// </summary>
        public T QueryFirst<T>(string text, object parameters)
            => getFirstOrException(Query<T>(text, parameters));
        /// <summary>
        /// Executes a query and returns the result the first T or Defult(T)
        /// </summary>
        public T QueryOrDefault<T>(string text, object parameters)
            => getFirstOrDefault(Query<T>(text, parameters));

        /// <summary>
        /// Gets a single T with specified table KeyValue on KeyColumn
        /// </summary>
        public T Get<T>(object keyValue) => Get<T>(null, keyValue);
        /// <summary>
        /// Gets a single T with specified table KeyValue on KeyColumn
        /// </summary>
        public T Get<T>(string keyColumn, object keyValue)
        {
            var info = typeCollection.GetInfo<T>();

            string column = keyColumn
                            ?? info.Items.Where(o => o.Is(DatabaseWrapper.ColumnAttributes.PrimaryKey))
                                   .Select(o => o.Name)
                                   .FirstOrDefault()
                            ?? "_rowid_";
            var data = Query<T>($"SELECT * FROM {info.TypeName} WHERE {column} = @keyValue LIMIT 1 ", new { keyValue });
            // The enumeration should finalize to connection be closed
            return getFirstOrDefault(data);
        }
        /// <summary>
        /// Work around to fornce enumerables to finalize
        /// The enumeration should finalize to connection be closed
        /// </summary>
        private static T getFirstOrDefault<T>(IEnumerable<T> data)
        {
            T val = default;
            bool first = true;
            foreach (var d in data)
            {
                if (!first) continue;
                val = d;
                first = false;
            }
            return val;
        }
        /// <summary>
        /// Work around to fornce enumerables to finalize
        /// The enumeration should finalize to connection be closed
        /// </summary>
        private static T getFirstOrException<T>(IEnumerable<T> data)
        {
            T val = default;
            bool first = true;
            foreach (var d in data)
            {
                if (!first) continue;
                val = d;
                first = false;
            }
            if (first) throw new InvalidOperationException("The source sequence is empty");
            return val;
        }

        /// <summary>
        /// Queries the database to all T rows in the table
        /// </summary>
        public IEnumerable<T> GetAll<T>()
            => Query<T>($"SELECT * FROM {typeof(T).Name} ", null);

        /// <summary>
        /// Queries the database to all T rows in the table with specified table KeyValue on KeyColumn
        /// </summary>
        public IEnumerable<T> GetAllWhere<T>(string filterColumn, object filterValue)
        {
            if (filterColumn is null) throw new ArgumentNullException(nameof(filterColumn));

            return Query<T>($"SELECT * FROM {typeof(T).Name} WHERE {filterColumn} = @FilterValue ", new { filterValue });
        }

        private string[] getSchemaColumns(IDataReader reader)
        {
            return Enumerable.Range(0, reader.FieldCount)
                             .Select(idx => reader.GetName(idx))
                             .ToArray();
        }

        /// <summary>
        /// Inserts a new T and return it's ID, this method locks the execution
        /// </summary>
        /// <param name="item">Item to be added</param>
        /// <param name="resolution">Conflict resolution method</param>
        /// <param name="tableName">Name of the table, uses T class name if null</param>
        /// <returns></returns>
        public long Insert<T>(T item, OnConflict resolution = OnConflict.Abort, string tableName = null)
            => ExecuteScalar<long>(buildInsertSql<T>(resolution, tableName), item);

        /// <summary>
        /// Inserts a new T and return it's ID, this method locks the execution
        /// </summary>
        /// <returns></returns>
        [Obsolete("Use Insert<T> instead", true)]
        public long InsertInto<T>(T item, OnConflict resolution = OnConflict.Abort, string tableName = null)
            => Insert<T>(item, resolution, tableName);

        /// <summary>
        /// Inserts a new T or replace with current T and return it's ID, this method locks the execution.
        /// When a Replace occurs, the row is first deleted then re-inserted.
        /// Must have a [Unique] or PK column. 
        /// </summary>
        /// <returns>Returns `sqlite3:last_insert_rowid()`</returns>
        public long InsertOrReplace<T>(T item)
            => Insert<T>(item, OnConflict.Replace);

        /// <summary>
        /// Inserts many T items into the database and return their IDs, this method locks the execution
        /// </summary>
        /// <param name="items">Items to be inserted</param>
        /// <param name="resolution">Conflict resolution method</param>
        /// <param name="tableName">Name of the table, uses T class name if null</param>
        public long[] BulkInsert<T>(IEnumerable<T> items, OnConflict resolution = OnConflict.Abort, string tableName = null)
        {
            var cnn = getConnection();

            List<long> ids = new List<long>();
            string sql = buildInsertSql<T>(resolution, tableName);

            lock (lockNonQuery)
            {
                using var trn = cnn.BeginTransaction();

                foreach (var item in items)
                {
                    using var cmd = new SqliteCommand(sql, cnn, trn);
                    fillParameters(cmd, item);

                    var scalar = cmd.ExecuteScalar();
                    if (scalar is long sL) ids.Add(sL);
                }

                trn.Commit();
            }

            if (!IsInMemoryDatabase) cnn.Close();
            return ids.ToArray();
        }

        /// <summary>
        /// Inserts many T items into the database and return their IDs, this method locks the execution
        /// </summary>
        public long[] BulkInsert<T>(IEnumerable<T> items, bool addReplace) => BulkInsert<T>(items, addReplace ? OnConflict.Replace : OnConflict.Abort, null);

        /// <summary>
        /// Creates an online backup of the current database.
        /// Can be used to save an InMemory database
        /// </summary>
        public void CreateBackup(string fileName)
        {
            var source = getConnection();
            SqliteConnectionStringBuilder sb = new SqliteConnectionStringBuilder
            {
                DataSource = fileName,
                //Version = 3
            };
            using var destination = new SqliteConnection(sb.ToString());
            destination.Open();
            source.BackupDatabase(destination); //, "main", "main", -1, null, 0);

            if (!IsInMemoryDatabase) source.Close();
        }

        private string buildInsertSql<T>(OnConflict resolution, string tableName = null)
        {
            var info = typeCollection.GetInfo<T>();
            if (tableName == null) tableName = info.TypeName;

            var names = getNames(info, !info.IsAnonymousType);
            var fields = string.Join(",", names);
            var values = string.Join(",", names.Select(n => $"@{n}"));

            if (resolution == OnConflict.Abort)
            {
                return $"INSERT INTO {tableName} ({fields}) VALUES ({values}); SELECT last_insert_rowid();";
            }
            else
            {
                string txtConflict = resolution switch
                {
                    OnConflict.RollBack => "ROLLBACK",
                    OnConflict.Fail => "FAIL",
                    OnConflict.Ignore => "IGNORE",
                    OnConflict.Replace => "REPLACE",
                    _ => throw new ArgumentException($"Invalid resolution: {resolution}"),
                };

                return $"INSERT OR {txtConflict} INTO {tableName} ({fields}) VALUES ({values}); SELECT last_insert_rowid();";
            }
        }

        private void fillParameters(SqliteCommand cmd, object parameters, TypeInfo type = null)
        {
            if (parameters == null) return;

            if (type == null) type = typeCollection.GetInfo(parameters.GetType());

            foreach (var p in type.Items)
            {
                if (!p.CanRead) continue;
                var value = TypeHelper.ReadParamValue(p, parameters);
                adjustInsertValue(ref value, p, parameters);

                cmd.Parameters.AddWithValue(p.Name, value);
            }
        }
        private void adjustInsertValue(ref object value, TypeItemInfo p, object parameters)
        {
            if (!p.Is(DatabaseWrapper.ColumnAttributes.PrimaryKey)) return;

            if (p.Type == typeof(int) || p.Type == typeof(long))
            {
                if (!value.Equals(0)) return;
                // PK ints are AI
                //value = null;
                value = DBNull.Value;
            }
            else if (p.Type == typeof(Guid))
            {
                if (!value.Equals(Guid.Empty)) return;

                value = Guid.NewGuid();
                // write new guid on object
                p.SetValue(parameters, value);
            }
        }

        /* Statics */
        /// <summary>
        /// Creates a InMemory database instance
        /// </summary>
        public static SqliteDB CreateInMemory()
        {
            var builder = new SqliteConnectionStringBuilder($"Data Source=:memory:");
            return new SqliteDB(builder, "")
            {
                IsInMemoryDatabase = true,
            };
        }

        private static IEnumerable<string> getNames(TypeInfo type, bool needWrite)
        {
            return type.Items
                       .Where(o => !o.Is(DatabaseWrapper.ColumnAttributes.Ignore))
                       .Where(o => o.CanRead)
                       .Where(o => !needWrite || o.CanWrite) // This is confusing
                       .Select(o => o.Name);
        }

    }
}
