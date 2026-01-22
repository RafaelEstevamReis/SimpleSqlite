namespace Simple.Sqlite;

using Simple.DatabaseWrapper.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

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
    public static IEnumerable<T> Query<T>(this ISqliteConnection connection, string query, object? parameters = null, bool buffered = true)
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
    public static IEnumerable<T> Query<T>(this ISqliteTransaction transaction, string query, object? parameters = null, bool buffered = true)
       => query<T>(transaction.connection, transaction, query, parameters, buffered);

    /// <summary>
    /// Builds and Executes a query using the parameters for the where clause then map the result into a model
    /// </summary>
    /// <typeparam name="T">Returning model type</typeparam>
    /// <param name="connection">The connection to be used</param>
    /// <param name="parameters">Columns to be filtered. A Where clause will be builded with the paramters property's name.</param>
    /// <param name="buffered">Defines if the results should be buffered in memory</param>
    /// <returns>A collection of values mapped from the result rows</returns>
    public static IEnumerable<T> Query<T>(this ISqliteConnection connection, object? parameters, bool buffered = true)
       => queryWithBuild<T>(connection, null, parameters, buffered);

    /// <summary>
    /// Builds and Executes a query using the parameters for the where clause then map the result into a model
    /// </summary>
    /// <typeparam name="T">Returning model type</typeparam>
    /// <param name="transaction">The transaction to be used</param>
    /// <param name="parameters">Columns to be filtered. A Where clause will be builded with the paramters property's name.</param>
    /// <param name="buffered">Defines if the results should be buffered in memory</param>
    /// <returns>A collection of values mapped from the result rows</returns>
    public static IEnumerable<T> Query<T>(this ISqliteTransaction transaction, object? parameters, bool buffered = true)
       => queryWithBuild<T>(transaction.connection, transaction, parameters, buffered);

    static IEnumerable<T> queryWithBuild<T>(ISqliteConnection connection, ISqliteTransaction? transaction, object? parameters, bool buffered = true)
    {
        if (parameters is null)
        {
            throw new ArgumentNullException(nameof(parameters));
        }

        return query<T>(connection, transaction, queryBuilder<T>(connection, parameters), parameters, buffered);
    }

    static string queryBuilder<T>(ISqliteConnection connection, object parameters)
    {
        var tInfo = connection.typeCollection.GetInfo(typeof(T));
        var tColumns = tInfo.GetNames().ToArray();

        string baseQuery = $"SELECT * FROM {tInfo.TypeName} WHERE ";
        var filterColumns = HelperFunctions.getParametersNames(parameters, connection.typeCollection);

        var pairs = filterColumns.Select(c =>
        {
            var columnParamName = c;
            var columnName = tColumns.First(cn => cn.Equals(columnParamName, StringComparison.InvariantCultureIgnoreCase));

            return $"{columnName} = @{columnParamName}";
        });

        return $"SELECT * FROM {tInfo.TypeName} WHERE {string.Join(" AND ", pairs)}";
    }

    static IEnumerable<T> query<T>(ISqliteConnection connection, ISqliteTransaction? transaction, string query, object parameters, bool buffered)
    {
        var q = _query<T>(connection, transaction, query, parameters);
        if (buffered) q = q.ToList();
        return q;
    }
    static IEnumerable<T> _query<T>(ISqliteConnection connection, ISqliteTransaction? transaction, string query, object parameters)
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
