using System.Collections.Generic;
using System;
using System.Linq;

namespace Simple.Sqlite.Extension
{
    public static class GetDataExtension
    {
        public static T Get<T>(this ISqliteConnection connection, object keyValue)
            => connection.Get<T>(null, keyValue);
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
        public static T Get<T>(this ISqliteConnection connection, string tableName, string keyColumn, object keyValue)
        {
            if (tableName is null) throw new ArgumentNullException(nameof(tableName));
            if (keyColumn is null) throw new ArgumentNullException(nameof(keyColumn));

            var data = connection.Query<T>($"SELECT * FROM {tableName} WHERE {keyColumn} = @keyValue LIMIT 1 ", new { keyValue });

            return SqliteDB.getFirstOrDefault(data);
        }

        public static IEnumerable<T> GetAll<T>(this ISqliteConnection connection)
            => GetAll<T>(connection, typeof(T).Name);
        public static IEnumerable<T> GetAll<T>(this ISqliteConnection connection, string tableName)
            => connection.Query<T>($"SELECT * FROM {tableName} ", null);
    }
}
