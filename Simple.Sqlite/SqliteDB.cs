﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.IO.Compression;
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
            else backupDatabase();

            // uses builder to avoid escape issues
            SQLiteConnectionStringBuilder sb = new SQLiteConnectionStringBuilder
            {
                DataSource = DatabaseFileName,
                Version = 3
            };
            cnnString = sb.ToString();
        }

        private void backupDatabase()
        {
            var temp = Path.GetTempFileName();
            using var fsInput = File.OpenRead(DatabaseFileName);
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
        {
            var typeT = typeof(T);
            using var cnn = getConnection();
            using var cmd = cnn.CreateCommand();

            cmd.CommandText = Text;
            fillParameters(cmd, Parameters);

            using var reader = cmd.ExecuteReader();

            if (!reader.HasRows) yield break;

            var colNames = getSchemaColumns(reader);
            while (reader.Read())
            {
                // build new
                if (typeT.CheckIfSimpleType())
                {
                    yield return (T)TypeMapper.ReadValue(reader, typeT, colNames.First());
                }
                else
                {
                    yield return TypeMapper.MapObject<T>(colNames, reader);
                }
            }
        }


        /// <summary>
        /// Gets a single T with specified table KeyValue on KeyColumn
        /// </summary>
        public T Get<T>(object KeyValue) => Get<T>(null, KeyValue);
        /// <summary>
        /// Gets a single T with specified table KeyValue on KeyColumn
        /// </summary>
        public T Get<T>(string KeyColumn, object KeyValue)
        {
            var TypeT = typeof(T);

            string keyColumn = KeyColumn
                            ?? TableMapper.Column.GetKeyColumn(TypeT)
                            ?? "_rowid_";

            return ExecuteQuery<T>($"SELECT * FROM {TypeT.Name} WHERE {keyColumn} = @KeyValue ", new { KeyValue })
                    .FirstOrDefault();
        }
        /// <summary>
        /// Queries the database to all T rows in the table
        /// </summary>
        public IEnumerable<T> GetAll<T>() => ExecuteQuery<T>($"SELECT * FROM {typeof(T).Name} ", null);

        /// <summary>
        /// Queries the database to all T rows in the table with specified table KeyValue on KeyColumn
        /// </summary>
        public IEnumerable<T> GetAllWhere<T>(string FilterColumn, object FilterValue)
        {
            if (FilterColumn is null) throw new ArgumentNullException(nameof(FilterColumn));

            return ExecuteQuery<T>($"SELECT * FROM {typeof(T).Name} WHERE {FilterColumn} = @FilterValue ", new { FilterValue });
        }

        private HashSet<string> getSchemaColumns(SQLiteDataReader reader)
        {
            return reader.GetSchemaTable()
                .Rows
                .Cast<DataRow>()
                .Select(r => (string)r["ColumnName"])
                .ToHashSet();
        }

        /// <summary>
        /// Inserts a new T and return it's ID, this method locks the execution
        /// </summary>
        /// <returns>Returns `sqlite3:last_insert_rowid()`</returns>
        public long Insert<T>(T Item) => ExecuteScalar<long>(buildInsertSql<T>(), Item);

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
        public void InsertOrReplace<T>(T Item) => ExecuteNonQuery(buildInsertSql<T>(true), Item);

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
                    if (info.PropertyType.FullName == "System.Int32")
                    {
                        continue;
                    }
                    if (info.PropertyType.FullName == "System.Int64")
                    {
                        continue;
                    }
                }

                yield return info.Name;
            }
        }
    }
}
