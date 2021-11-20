using System;

namespace Simple.Sqlite.Extension
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
        {
            using var cmd = connection.connection.CreateCommand();

            cmd.CommandText = query;
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
        {
            using var cmd = connection.connection.CreateCommand();

            cmd.CommandText = query;
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
