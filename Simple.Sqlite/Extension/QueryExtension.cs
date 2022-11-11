using Simple.DatabaseWrapper.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace Simple.Sqlite.Extension
{
    /// <summary>
    /// Extension for "QueryExtension" stuff
    /// </summary>
    public static class QueryExtension
    {
        /// <summary>
        /// Executes a query and map the result into a model
        /// </summary>
        /// <typeparam name="T">Returning model type</typeparam>
        /// <param name="connection">The connection to be used</param>
        /// <param name="query">Query yo be executed</param>
        /// <param name="parameters">Query parameters</param>
        /// <param name="buffered">Defines if the results should be buffered in memory</param>
        /// <returns>A collection of values mapped from the result rows</returns>
        public static IEnumerable<T> Query<T>(this ISqliteConnection connection, string query, object parameters, bool buffered = true)
            => query<T>(connection, null, query, parameters, buffered);

        /// <summary>
        /// Executes a query and map the result into a model within a transaction
        /// </summary>
        /// <typeparam name="T">Returning model type</typeparam>
        /// <param name="transaction">The transaction to be used</param>
        /// <param name="query">Query yo be executed</param>
        /// <param name="parameters">Query parameters</param>
        /// <param name="buffered">Defines if the results should be buffered in memory</param>
        /// <returns>A collection of values mapped from the result rows</returns>
        public static IEnumerable<T> Query<T>(this ISqliteTransaction transaction, string query, object parameters, bool buffered = true)
           => query<T>(transaction.connection, transaction, query, parameters, buffered);

        static IEnumerable<T> query<T>(ISqliteConnection connection, ISqliteTransaction transaction, string query, object parameters, bool buffered)
        {
            var q = _query<T>(connection, transaction, query, parameters);
            if (buffered) q = q.ToList();
            return q;
        }
        static IEnumerable<T> _query<T>(ISqliteConnection connection, ISqliteTransaction transaction, string query, object parameters)
        {
            using var cmd = new Microsoft.Data.Sqlite.SqliteCommand(query, connection.connection, transaction?.transaction);
            HelperFunctions.fillParameters(cmd, parameters, connection.typeCollection);

            using var reader = cmd.ExecuteReader();

            if (!reader.HasRows)
            {
                yield break;
            }

            var colNames = HelperFunctions.getSchemaColumns(reader);
            var typeT = typeof(T);
            bool isSimple = typeT.CheckIfSimpleType();
            while (reader.Read())
            {
                // build new
                if (isSimple)
                {
                    yield return (T)TypeMapper.ReadValue(reader, typeT, 0);
                }
                else
                {
                    yield return TypeMapper.MapObject<T>(colNames, reader, connection.typeCollection);
                }
            }
        }
    }
}
