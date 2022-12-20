using System.Collections.Generic;
using System;
using System.Linq;

namespace Simple.Sqlite
{
    /// <summary>
    /// Extension for "GetDataExtension" stuff
    /// </summary>
    public static class GetDataExtension
    {
        /// <summary>
        /// Select a value from a table `T` using it's primary key or `__rowid__`
        /// </summary>
        public static T Get<T>(this ISqliteConnection connection, object keyValue)
            => connection.Get<T>(null, keyValue);
        /// <summary>
        /// Select a value from a table `T` using specified column and value
        /// </summary>
        public static T Get<T>(this ISqliteConnection connection, string keyColumn, object keyValue)
        {
            var info = connection.typeCollection.GetInfo<T>();
            string column = keyColumn
                        ?? info.Items.Where(o => o.Is(DatabaseWrapper.ColumnAttributes.PrimaryKey))
                               .Select(o => o.Name)
                               .FirstOrDefault()
                        ?? "_rowid_";

            return Get<T>(connection, info.TypeName, column, keyValue);
        }
        /// <summary>
        /// Select a value from a table using specified column and value
        /// </summary>
        public static T Get<T>(this ISqliteConnection connection, string tableName, string keyColumn, object keyValue)
        {
            if (tableName is null) throw new ArgumentNullException(nameof(tableName));
            if (keyColumn is null) throw new ArgumentNullException(nameof(keyColumn));

            var data = connection.Query<T>($"SELECT * FROM {tableName} WHERE {keyColumn} = @keyValue LIMIT 1 ", new { keyValue });

            return SqliteDB.getFirstOrDefault(data);
        }
        /// <summary>
        /// Select all values from a table `T`
        /// </summary>
        public static IEnumerable<T> GetAll<T>(this ISqliteConnection connection)
            => GetAll<T>(connection, typeof(T).Name);
        /// <summary>
        /// Select all values from a table
        /// </summary>
        public static IEnumerable<T> GetAll<T>(this ISqliteConnection connection, string tableName)
            => connection.Query<T>($"SELECT * FROM {tableName} ", null);
 
        /// <summary>
        /// Select filtered values from the table T WHERE filterColumn = filterValue
        /// </summary>
        public static IEnumerable<T> GetWhere<T>(this ISqliteConnection connection, string filterColumn, object filterValue)
            => connection.Query<T>($"SELECT * FROM {typeof(T).Name} WHERE {filterColumn} = @filterValue ", new { filterValue });

    }
}
