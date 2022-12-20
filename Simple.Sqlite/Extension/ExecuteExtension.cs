using System;

namespace Simple.Sqlite
{
    /// <summary>
    /// Extension for "Execute" stuff
    /// </summary>
    public static class ExecuteExtension
    {
        /// <summary>
        /// Executes a query
        /// </summary>
        /// <param name="connection">The connection to be used</param>
        /// <param name="query">SQL to be eecuted</param>
        /// <param name="parameters">object with parameters</param>
        /// <returns>Returns affected rows. -1 for selects</returns>
        public static int Execute(this ISqliteConnection connection, string query, object parameters = null)
            => execute(connection, null, query, parameters);
        /// <summary>
        /// Executes a query in a transaction
        /// </summary>
        /// <param name="transaction">The transaction to be used</param>
        /// <param name="query">SQL to be eecuted</param>
        /// <param name="parameters">object with parameters</param>
        /// <returns>Returns affected rows. -1 for selects</returns>
        public static int Execute(this ISqliteTransaction transaction, string query, object parameters = null)
            => execute(transaction.connection, transaction, query, parameters);

        private static int execute(ISqliteConnection connection, ISqliteTransaction transaction, string query, object parameters = null)
        {
            //using var cmd = connection.connection.CreateCommand();
            //cmd.CommandText = query;
            using var cmd = new Microsoft.Data.Sqlite.SqliteCommand(query, connection.connection, transaction?.transaction);
            HelperFunctions.fillParameters(cmd, parameters, connection.typeCollection);

            return cmd.ExecuteNonQuery();
        }


        /// <summary>
        /// Executes a ScalarQuery
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="connection">The connection to be used</param>
        /// <param name="query">Query yo be executed</param>
        /// <param name="parameters">object with parameters</param>
        /// <returns>Scalar value</returns>
        public static T ExecuteScalar<T>(this ISqliteConnection connection, string query, object parameters = null)
            => executeScalar<T>(connection, null, query, parameters);
        /// <summary>
        /// Executes a ScalarQuery in a transaction
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="transaction">The transaction to be used</param>
        /// <param name="query">Query yo be executed</param>
        /// <param name="parameters">object with parameters</param>
        /// <returns>Scalar value</returns>
        public static T ExecuteScalar<T>(this ISqliteTransaction transaction, string query, object parameters = null)
           => executeScalar<T>(transaction.connection, transaction, query, parameters);

        static T executeScalar<T>(this ISqliteConnection connection, ISqliteTransaction transaction, string query, object parameters = null)
        {
            //using var cmd = connection.connection.CreateCommand();
            //cmd.CommandText = query;

            using var cmd = new Microsoft.Data.Sqlite.SqliteCommand(query, connection.connection, transaction?.transaction);
            HelperFunctions.fillParameters(cmd, parameters, connection.typeCollection);

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
    }
}
