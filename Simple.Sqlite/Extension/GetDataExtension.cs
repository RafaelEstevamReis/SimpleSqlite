using System.Collections.Generic;

namespace Simple.Sqlite.Extension
{
    public static class GetDataExtension
    {
        public static IEnumerable<T> GetAll<T>(this ISqliteConnection connection)
            => connection.Query<T>($"SELECT * FROM {typeof(T).Name} ", null);

    }
}
