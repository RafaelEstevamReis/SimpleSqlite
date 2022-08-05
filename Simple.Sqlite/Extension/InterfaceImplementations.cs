using Microsoft.Data.Sqlite;
using Simple.DatabaseWrapper.TypeReader;
using System;

namespace Simple.Sqlite.Extension
{
    internal class Connection : ISqliteConnection
    {
        internal SqliteConnection connection { get; set; }
        internal ReaderCachedCollection typeCollection { get; set; }

        SqliteConnection ISqliteConnection.connection => connection;
        ReaderCachedCollection ISqliteConnection.typeCollection => typeCollection;

        public string DatabasFilePath => connection.DataSource;
        public SqliteConnection GetUnderlyingConnection() => connection;

        public ISqliteTransaction OpenTransaction()
        {
            return new Transaction(this);
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
        private ISqliteTransaction transaction;

        public Transaction(Connection connection)
        {
            this.connection = connection;
            this.transaction = connection.OpenTransaction();
        }

        ISqliteConnection ISqliteTransaction.connection => connection;
        ISqliteTransaction ISqliteTransaction.transaction => transaction;

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

}
