using Microsoft.Data.Sqlite;
using Simple.DatabaseWrapper.Helpers;
using Simple.DatabaseWrapper.TypeReader;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Simple.Sqlite
{
    internal class HelperFunctions
    {
        internal static bool handleGuidAsByteArray = true;
        internal static void fillParameters(SqliteCommand cmd, object parameters, ReaderCachedCollection typeCollection)
        {
            if (parameters == null) return;

            var type = typeCollection.GetInfo(parameters.GetType());

            foreach (var p in type.Items)
            {
                if (!p.CanRead) continue;
                var value = TypeHelper.ReadParamValue(p, parameters, handleGuidAsByteArray);
                adjustInsertValue(ref value, p, parameters);

                if (value is null) value = DBNull.Value;

                cmd.Parameters.AddWithValue(p.Name, value);
            }
        }
        internal static void adjustInsertValue(ref object value, TypeItemInfo p, object parameters)
        {
            if (value is Uri uri)
            {
                value = uri.ToString();
            }

            // Adjust TypeHelper.ReadParamValue(p, parameters);
            // Fixes #25ff772 from simple.databasewrapper
            // Reverted back on #1abea14
            //if (value is byte[] bVal
            //    && bVal.Length == 16
            //    && (p.Type == typeof(Guid) || p.Type == typeof(object)))
            //{
            //    value = new Guid((byte[])value);
            //}

            if (!p.Is(DatabaseWrapper.ColumnAttributes.PrimaryKey)) return;

            //  Check for INT or LONGs equals to zero
            if (p.Type == typeof(int))
            {
                if ((int)value != 0) return;
                value = null;
            }
            else if (p.Type == typeof(long))
            {
                if ((long)value != 0) return;
                value = null;
            }
            else if (p.Type == typeof(Guid))
            {
                // Se for Guid <=> Guid, preencher os EMPTY
                if (value is Guid guid)
                {
                    if (guid != Guid.Empty) return;
                    value = Guid.NewGuid();
                }
                // Se for byte[] <=> Guid, preencher os [0,0,0,..]
                if (value is byte[] b)
                {
                    // Se todos forem zero, inicializar
                    if (b.All(o => o == 0))
                    {
                        value = Guid.NewGuid().ToByteArray();
                    }
                }

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

        internal static string buildInsertSql<T>(ReaderCachedCollection typeCollection, OnConflict resolution, string tableName = null)
        {
            var info = typeCollection.GetInfo<T>();
            if (tableName == null) tableName = info.TypeName;

            var names = getNames(info, !info.IsAnonymousType);
            var fields = string.Join(",", names);
            var values = string.Join(",", names.Select(n => $"@{n}"));

            if (resolution == OnConflict.Abort)
            {
                return $"INSERT INTO {tableName} ({fields}) VALUES ({values}); SELECT last_insert_rowid();";
            }
            else
            {
                string txtConflict = resolution switch
                {
                    OnConflict.RollBack => "ROLLBACK",
                    OnConflict.Fail => "FAIL",
                    OnConflict.Ignore => "IGNORE",
                    OnConflict.Replace => "REPLACE",
                    _ => throw new ArgumentException($"Invalid resolution: {resolution}"),
                };

                return $"INSERT OR {txtConflict} INTO {tableName} ({fields}) VALUES ({values}); SELECT last_insert_rowid();";
            }
        }
        private static IEnumerable<string> getNames(TypeInfo type, bool needWrite)
        {
            return type.Items
                       .Where(o => !o.Is(DatabaseWrapper.ColumnAttributes.Ignore))
                       .Where(o => o.CanRead)
                       .Where(o => !needWrite || o.CanWrite) // Be careful with NOTs and ORs
                       .Select(o => o.Name);
        }
    }
}
