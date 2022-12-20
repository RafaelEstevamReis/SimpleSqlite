using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace Simple.Sqlite
{
    /// <summary>
    /// Extension for "InsertExtension" stuff
    /// </summary>
    public static class InsertExtension
    {
        /// <summary>
        /// Inserts a value into a table
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="connection">The connection to be used</param>
        /// <param name="item">Item to be inserted</param>
        /// <param name="resolution">Conflict resolution policy</param>
        /// <param name="tableName">The name of the table, NULL to use `T` type name as the name of the table</param>
        /// <returns>Returns the integer Primary Key or __ROWID__ of the inserted row</returns>
        public static long Insert<T>(this ISqliteConnection connection, T item, OnConflict resolution = OnConflict.Abort, string tableName = null)
              => connection.ExecuteScalar<long>(HelperFunctions.buildInsertSql<T>(connection.typeCollection, resolution, tableName), item);
        /// <summary>
        /// Inserts a value into a table with a transaction
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="transaction">The transaction to be used</param>
        /// <param name="item">Item to be inserted</param>
        /// <param name="resolution">Conflict resolution policy</param>
        /// <param name="tableName">The name of the table, NULL to use `T` type name as the name of the table</param>
        /// <returns>Returns the integer Primary Key or __ROWID__ of the inserted row</returns>
        public static long Insert<T>(this ISqliteTransaction transaction, T item, OnConflict resolution = OnConflict.Abort, string tableName = null)
             => transaction.ExecuteScalar<long>(HelperFunctions.buildInsertSql<T>(transaction.connection.typeCollection, resolution, tableName), item);

        /// <summary>
        /// Inserts multiple values into a table efficiently using a transaction
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="connection">The connection to be used</param>
        /// <param name="items">Items to be inserted</param>
        /// <param name="resolution">Conflict resolution policy</param>
        /// <param name="tableName">The name of the table, NULL to use `T` type name as the name of the table</param>
        /// <returns>Returns the integer Primary Key or __ROWID__ of the inserted rows</returns>
        public static long[] BulkInsert<T>(this ISqliteConnection connection, IEnumerable<T> items, OnConflict resolution = OnConflict.Abort, string tableName = null)
        {
            List<long> ids = new List<long>();
            string sql = HelperFunctions.buildInsertSql<T>(connection.typeCollection, resolution, tableName);

            using var trn = connection.connection.BeginTransaction();

            using var cmd = new SqliteCommand(sql, connection.connection, trn);
            foreach (var item in items)
            {
                cmd.Parameters.Clear();
                HelperFunctions.fillParameters(cmd, item, connection.typeCollection);

                var scalar = cmd.ExecuteScalar();
                if (scalar is long sL) ids.Add(sL);
            }

            trn.Commit();
            return ids.ToArray();
        }
    }
}
