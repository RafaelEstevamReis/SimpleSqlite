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
        public SqliteConnection GetUnderlyingConnection() => connection;

        public void Dispose()
        {
            if (connection.State == System.Data.ConnectionState.Open) connection.Close();
            connection.Dispose();
        }
    }
    internal class Transaction : ISqliteTransaction
    {
        ISqliteConnection ISqliteTransaction.connection => throw new System.NotImplementedException();
        SqliteTransaction ISqliteTransaction.transaction => throw new System.NotImplementedException();

        public void Dispose()
        {
            //ISqliteTransaction.transaction.Dispose();
        }
    }
}
