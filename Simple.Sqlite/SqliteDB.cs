namespace Simple.Sqlite;

using Simple.DatabaseWrapper.Interfaces;
using Simple.Sqlite.Helpers;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.IO.Compression;

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

    public static bool HandleGuidAsByteArray
    {
        get => HelperFunctions.handleGuidAsByteArray;
        set => HelperFunctions.handleGuidAsByteArray = value;
    }

    private readonly ConnectionFactory db;

    /// <summary>
    /// Database file full path
    /// </summary>
    public string DatabaseFileName { get; }

    /// <summary>
    /// Creates a new instance
    /// </summary>
    public SqliteDB(string fileName)
        : this(fileName, EnabledDatabaseBackup)
    { }
    public SqliteDB(string fileName, bool executeBackup)
    {
        var fi = new FileInfo(fileName);
        if (!fi.Directory!.Exists) fi.Directory.Create();

        DatabaseFileName = fi.FullName;
        if (executeBackup)
        {
            if (File.Exists(DatabaseFileName)) backupDatabase();
        }

        db = ConnectionFactory.FromFile(fileName);
    }

    private void backupDatabase()
    {
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

    /// <summary>
    /// Builds the table creation sequence, should be finished with Commit()
    /// </summary>
    public ITableMapper CreateTables()
    {
        var cnn = db.GetConnection();
        var mapper = new TableMapper(cnn);
        mapper.DisposeOnCommit = true;
        return mapper;
    }

    /// <summary>
    /// Get a list of all tables
    /// </summary>
    public string[] GetAllTables()
    {
        //return Query<string>("SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;", null).ToArray();
        using var cnn = db.GetConnection();
        return cnn.GetAllTables();
    }
    /// <summary>
    /// Gets the schema for a table
    /// </summary>
    public DataTable GetTableSchema(string tableName)
    {
        using var cnn = db.GetConnection();
        return cnn.GetTableSchema(tableName);
    }
    /// <summary>
    /// Gets columns names for a table
    /// </summary>
    public string[] GetTableColumnNames(string tableName)
    {
        using var cnn = db.GetConnection();
        return cnn.GetTableColumnNames(tableName);
    }

    /// <summary>
    /// Executes a NonQuery command, this method locks the execution
    /// </summary>
    public int Execute(string text, object parameters = null)
    {
        using var cnn = db.GetConnection();
        return cnn.Execute(text, parameters);
    }
    /// <summary>
    /// Executes a Scalar commands and return the value as T
    /// </summary>
    public T ExecuteScalar<T>(string text, object parameters)
    {
        using (var cnn = db.GetConnection())
        {
            return cnn.ExecuteScalar<T>(text, parameters);
        }
    }

    /// <summary>
    /// Executes a query and returns the value as a T collection
    /// </summary>
    public IEnumerable<T> Query<T>(string text, object? parameters)
    {
        using var cnn = db.GetConnection();
        return cnn.Query<T>(text, parameters);
    }

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
        using var cnn = db.GetConnection();
        return cnn.Get<T>(keyColumn, keyValue);
    }
    /// <summary>
    /// Work around to force enumerables to finalize
    /// The enumeration should finalize to connection be closed
    /// </summary>
    internal static T getFirstOrDefault<T>(IEnumerable<T> data)
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
    /// Work around to force enumerables to finalize
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

        return Query<T>($"SELECT * FROM {typeof(T).Name} WHERE {filterColumn} = @filterValue ", new { filterValue });
    }

    /// <summary>
    /// Inserts a new T and return it's ID, this method locks the execution
    /// </summary>
    /// <param name="item">Item to be added</param>
    /// <param name="resolution">Conflict resolution method</param>
    /// <param name="tableName">Name of the table, uses T class name if null</param>
    /// <returns></returns>
    public long Insert<T>(T item, OnConflict resolution = OnConflict.Abort, string tableName = null)
    {
        using var cnn = db.GetConnection();
        return cnn.Insert(item, resolution, tableName);
    }

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
        using var cnn = db.GetConnection();
        return cnn.BulkInsert<T>(items, resolution, tableName);
    }

    /// <summary>
    /// Inserts many T items into the database and return their IDs, this method locks the execution
    /// </summary>
    public long[] BulkInsert<T>(IEnumerable<T> items, bool addReplace)
        => BulkInsert<T>(items, addReplace ? OnConflict.Replace : OnConflict.Abort, null);

    /// <summary>
    /// Creates an online backup of the current database.
    /// Can be used to save an InMemory database
    /// </summary>
    public void CreateBackup(string fileName)
    {
        using var source = db.GetConnection();
        source.CreateBackup(fileName); //, "main", "main", -1, null, 0);
    }

}
