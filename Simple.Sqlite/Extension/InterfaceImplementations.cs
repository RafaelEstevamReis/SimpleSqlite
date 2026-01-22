namespace Simple.Sqlite;

using Microsoft.Data.Sqlite;
using Simple.DatabaseWrapper.TypeReader;

internal class Connection : ISqliteConnection
{
    internal SqliteConnection connection { get; set; }
    internal ReaderCachedCollection typeCollection { get; set; }

    SqliteConnection ISqliteConnection.connection => connection;
    ReaderCachedCollection ISqliteConnection.typeCollection => typeCollection;

    public string DatabasFilePath => connection.DataSource;
    public SqliteConnection GetUnderlyingConnection() => connection;

    public ISqliteTransaction BeginTransaction(System.Data.IsolationLevel isolationLevel = System.Data.IsolationLevel.Unspecified)
    {
        return new Transaction(this, isolationLevel);
    }

    public void Dispose()
    {
        if (connection.State == System.Data.ConnectionState.Open) connection.Close();
        connection.Dispose();
    }
}
internal class Transaction : ISqliteTransaction
{
    private Connection connection;
    private SqliteTransaction transaction;

    public Transaction(Connection connection, System.Data.IsolationLevel isolationLevel)
    {
        this.connection = connection;
        this.transaction = connection.connection.BeginTransaction(isolationLevel);
    }

    ISqliteConnection ISqliteTransaction.connection => connection;
    SqliteTransaction ISqliteTransaction.transaction => transaction;

    public void Commit()
    {
        transaction.Commit();
    }
    public void Rollback()
    {
        transaction.Rollback();
    }

    public void Dispose()
    {
        transaction.Dispose();
    }
}
