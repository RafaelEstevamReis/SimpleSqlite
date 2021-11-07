using Microsoft.Data.Sqlite;
using Simple.DatabaseWrapper.TypeReader;
using System;

namespace Simple.Sqlite
{
    public interface ISqliteConnection : IDisposable
    {
        internal SqliteConnection connection { get; }
        internal ReaderCachedCollection typeCollection { get; }

        public SqliteConnection GetUnderlyingConnection();
    }
    public interface ISqliteTransaction : IDisposable
    {
        internal ISqliteConnection connection { get; }
        internal SqliteTransaction transaction { get; }
    }
}
