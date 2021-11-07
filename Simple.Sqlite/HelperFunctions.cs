using Microsoft.Data.Sqlite;
using Simple.DatabaseWrapper.Helpers;
using Simple.DatabaseWrapper.TypeReader;
using System;
using System.Data;
using System.Linq;

namespace Simple.Sqlite
{
    internal class HelperFunctions
    {
        internal static void fillParameters(SqliteCommand cmd, object parameters, ReaderCachedCollection typeCollection)
        {
            if (parameters == null) return;

            var type = typeCollection.GetInfo(parameters.GetType());

            foreach (var p in type.Items)
            {
                if (!p.CanRead) continue;
                var value = TypeHelper.ReadParamValue(p, parameters);
                adjustInsertValue(ref value, p, parameters);

                if (value is null) value = DBNull.Value;

                cmd.Parameters.AddWithValue(p.Name, value);
            }
        }
        internal static void adjustInsertValue(ref object value, TypeItemInfo p, object parameters)
        {
            if (!p.Is(DatabaseWrapper.ColumnAttributes.PrimaryKey)) return;

            if (p.Type == typeof(int) || p.Type == typeof(long))
            {
                if (!value.Equals(0)) return;
                // PK ints are AI
                value = null;
            }
            else if (p.Type == typeof(Guid))
            {
                if (!value.Equals(Guid.Empty)) return;

                value = Guid.NewGuid();
                // write new guid on object
                p.SetValue(parameters, value);
            }
        }

        internal static string[] getSchemaColumns(IDataReader reader)
        {
            return Enumerable.Range(0, reader.FieldCount)
                             .Select(idx => reader.GetName(idx))
                             .ToArray();
        }

    }
}
