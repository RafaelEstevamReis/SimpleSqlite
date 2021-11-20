﻿using Simple.DatabaseWrapper.Interfaces;

namespace Simple.Sqlite.Extension
{
    /// <summary>
    /// Extension for "MigrationExtension" stuff
    /// </summary>
    public static class MigrationExtension
    {
        /// <summary>
        /// Initialize a migration
        /// </summary>
        /// <param name="connection">The connection to be used</param>
        /// <returns>A database migration object</returns>
        public static ITableMapper CreateTables(this ISqliteConnection connection) => new TableMapperNew(connection);
    }
}
