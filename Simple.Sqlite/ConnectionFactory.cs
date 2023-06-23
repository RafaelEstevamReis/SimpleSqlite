﻿using Microsoft.Data.Sqlite;
using System;
using System.IO;

namespace Simple.Sqlite
{
    /// <summary>
    /// Creates ISqliteConnection connections
    /// </summary>
    public class ConnectionFactory
    {
        public static bool HandleGuidAsByteArray
        {
            get => HelperFunctions.handleGuidAsByteArray;
            set => HelperFunctions.handleGuidAsByteArray = value;
        }

        /// <summary>
        /// Current connection string
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Factory constructor
        /// </summary>
        public ConnectionFactory(SqliteConnectionStringBuilder builder)
            : this(builder.ToString())
        { }

        /// <summary>
        /// Factory constructor
        /// </summary>
        public ConnectionFactory(string cnnString)
        {
            if (string.IsNullOrWhiteSpace(cnnString))
            {
                throw new ArgumentException($"'{nameof(cnnString)}' cannot be null or whitespace.", nameof(cnnString));
            }
            ConnectionString = cnnString;
        }

        /// <summary>
        /// Opens a connection to the database
        /// </summary>
        /// <returns>An open connection of the database</returns>
        public ISqliteConnection GetConnection()
        {
            var SqliteConnection = new SqliteConnection(ConnectionString);
            SqliteConnection.Open();

            return new Connection()
            {
                typeCollection = new DatabaseWrapper.TypeReader.ReaderCachedCollection(),
                connection = SqliteConnection,
            };
        }

        /// <summary>
        /// Creates a Factory to a sqlite file
        /// </summary>
        public static ConnectionFactory FromFile(string databaseFile, bool readOnly = false)
        {
            var cnnString = sqliteFileToCnnString(databaseFile, readOnly ? SqliteOpenMode.ReadOnly : SqliteOpenMode.ReadWriteCreate);
            return new ConnectionFactory(cnnString);
        }

        protected static string sqliteFileToCnnString(string databaseFile, SqliteOpenMode mode = SqliteOpenMode.ReadWriteCreate)
        {
            var fi = new FileInfo(databaseFile);
            if (!fi.Directory.Exists) fi.Directory.Create();

            var sb = new SqliteConnectionStringBuilder
            {
                DataSource = databaseFile,
                Mode = mode
            };
            return sb.ToString();
        }

        /// <summary>
        /// Creates a ISqliteConnection instance from a file
        /// </summary>
        public static ISqliteConnection CreateConnection(string databaseFile)
        {
            var cnnString = sqliteFileToCnnString(databaseFile);
            var SqliteConnection = new SqliteConnection(cnnString);
            SqliteConnection.Open();

            return new Connection()
            {
                typeCollection = new DatabaseWrapper.TypeReader.ReaderCachedCollection(),
                connection = SqliteConnection,
            };
        }
       
        /// <summary>
        /// Opens a non-Shared in memory connection
        /// </summary>
        public static ISqliteConnection CreateInMemory()
        {
            var SqliteConnection = new SqliteConnection("Data Source=:memory:");
            SqliteConnection.Open();

            return new Connection()
            {
                typeCollection = new DatabaseWrapper.TypeReader.ReaderCachedCollection(),
                connection = SqliteConnection,
            };
        }
        /// <summary>
        /// Opens a non-Shared in memory connection
        /// </summary>
        /// <param name="sharedName">Data source shared name</param>
        public static ISqliteConnection CreateInMemoryShared(string sharedName)
        {
            var SqliteConnection = new SqliteConnection($"Data Source={sharedName};Mode=Memory;Cache=Shared");
            SqliteConnection.Open();

            return new Connection()
            {
                typeCollection = new DatabaseWrapper.TypeReader.ReaderCachedCollection(),
                connection = SqliteConnection,
            };
        }

    }
}
