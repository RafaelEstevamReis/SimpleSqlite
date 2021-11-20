using Microsoft.Data.Sqlite;
using System.Collections.Generic;

namespace Simple.Sqlite.Extension
{
    public static class InsertExtension
    {
        public static long Insert<T>(this ISqliteConnection connection, T item, OnConflict resolution = OnConflict.Abort, string tableName = null)
              => connection.ExecuteScalar<long>(HelperFunctions.buildInsertSql<T>(connection.typeCollection, resolution, tableName), item);

        public static long[] BulkInsert<T>(this ISqliteConnection connection, IEnumerable<T> items, OnConflict resolution = OnConflict.Abort, string tableName = null)
        {
            List<long> ids = new List<long>();
            string sql = HelperFunctions.buildInsertSql<T>(connection.typeCollection, resolution, tableName);

            using var trn = connection.connection.BeginTransaction();

            using var cmd = new SqliteCommand(sql, connection.connection, trn);
            foreach (var item in items)
            {
                cmd.Parameters.Clear();
                HelperFunctions.fillParameters(cmd, item, connection.typeCollection);

                var scalar = cmd.ExecuteScalar();
                if (scalar is long sL) ids.Add(sL);
            }

            trn.Commit();

            return ids.ToArray();
        }

    }
}
