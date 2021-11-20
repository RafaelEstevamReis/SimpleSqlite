using Simple.DatabaseWrapper.Helpers;
using System.Collections.Generic;

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
        /// <returns>A collection of values mapped from the result rows</returns>
        public static IEnumerable<T> Query<T>(this ISqliteConnection connection, string query, object parameters)
        {
            var typeT = typeof(T);
            using var cmd = connection.connection.CreateCommand();

            cmd.CommandText = query;
            HelperFunctions.fillParameters(cmd, parameters, connection.typeCollection);

            using var reader = cmd.ExecuteReader();

            if (!reader.HasRows)
            {
                yield break;
            }

            var colNames = HelperFunctions.getSchemaColumns(reader);
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
