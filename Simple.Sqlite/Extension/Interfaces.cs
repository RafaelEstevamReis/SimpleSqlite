using Microsoft.Data.Sqlite;
using Simple.DatabaseWrapper.TypeReader;
using System;

namespace Simple.Sqlite.Extension
{
    /// <summary>
    /// SqliteConnection interface wrapper
    /// </summary>
    public interface ISqliteConnection : IDisposable
    {
        internal SqliteConnection connection { get; }
        internal ReaderCachedCollection typeCollection { get; }
        /// <summary>
        /// Database's file path
        /// </summary>
        public string DatabasFilePath { get; }
        /// <summary>
        /// Gets the underlying SqliteConnection
        /// </summary>
        public SqliteConnection GetUnderlyingConnection();
    }
    public interface ISqliteTransaction : IDisposable
    {
        internal ISqliteConnection connection { get; }
        internal ISqliteTransaction transaction { get; }

        void Commit();
        void Rollback();
    }
}
