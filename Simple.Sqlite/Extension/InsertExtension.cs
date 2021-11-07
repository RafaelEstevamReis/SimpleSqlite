using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace Simple.Sqlite.Extension
{
    public static class InsertExtension
    {
        public static long Insert<T>(this ISqliteConnection connection, T item, OnConflict resolution = OnConflict.Abort, string tableName = null)
              => connection.ExecuteScalar<long>(HelperFunctions.buildInsertSql<T>(connection.typeCollection, resolution, tableName), item);

    }
}
