using System.Collections.Generic;
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
            var data = connection.Query<T>($"SELECT * FROM {info.TypeName} WHERE {column} = @keyValue LIMIT 1 ", new { keyValue });
            // The enumeration should finalize to connection be closed
            return data.ToArray().FirstOrDefault();
        }

        public static IEnumerable<T> GetAll<T>(this ISqliteConnection connection)
            => connection.Query<T>($"SELECT * FROM {typeof(T).Name} ", null);
    }
}
