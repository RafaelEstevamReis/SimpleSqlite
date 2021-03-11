using System;
using System.Data.Common;
using System.Data.SQLite;
using System.Drawing;

namespace Simple.Sqlite
{
    internal static class TypeMapper
    {
        internal static T MapObject<T>(string[] colNames, SQLiteDataReader reader)
        {
            object t = Activator.CreateInstance<T>();
            foreach (var p in typeof(T).GetProperties())
            {
                var clnIdx = Array.IndexOf(colNames, p.Name);
                if (clnIdx < 0) continue;

                mapColumn(t, p, reader, clnIdx);
            }
            return (T)t;
        }
        private static void mapColumn<T>(T obj, System.Reflection.PropertyInfo p, DbDataReader reader, int clnIdx)
            where T : new()
        {
            object objVal = ReadValue(reader, p.PropertyType, clnIdx);

            p.SetValue(obj, objVal);
        }
        internal static object ReadValue(DbDataReader reader, Type type, int ColumnIndex)
        {
            object objVal;
            if (reader.IsDBNull(ColumnIndex))
            {
                objVal = null;
            }
            else
            {
                if (type == typeof(string)) objVal = reader.GetString(ColumnIndex);
                else if (type == typeof(Uri)) objVal = new Uri((string)reader.GetValue(ColumnIndex));
                else if (type == typeof(double)) objVal = reader.GetDouble(ColumnIndex);
                else if (type == typeof(float)) objVal = reader.GetFloat(ColumnIndex);
                else if (type == typeof(decimal)) objVal = reader.GetDecimal(ColumnIndex);
                else if (type == typeof(int)) objVal = reader.GetInt32(ColumnIndex);
                else if (type == typeof(uint)) objVal = Convert.ToUInt32(reader.GetValue(ColumnIndex));
                else if (type == typeof(long)) objVal = reader.GetInt64(ColumnIndex);
                else if (type == typeof(ulong)) objVal = Convert.ToUInt64(reader.GetValue(ColumnIndex));
                else if (type == typeof(bool)) objVal = reader.GetBoolean(ColumnIndex);
                else if (type == typeof(DateTime)) objVal = reader.GetDateTime(ColumnIndex);
                else if (type == typeof(TimeSpan)) objVal = TimeSpan.FromTicks(reader.GetInt64(ColumnIndex));
                else if (type == typeof(byte[])) objVal = (byte[])reader.GetValue(ColumnIndex);
                else if (type == typeof(Guid)) objVal = reader.GetGuid(ColumnIndex);
                else if (type == typeof(Color))
                {
                    var argb = (byte[])reader.GetValue(ColumnIndex);
                    objVal = Color.FromArgb(argb[0], argb[1], argb[2], argb[3]);
                }
                else if (type.IsEnum) objVal = reader.GetInt32(ColumnIndex);
                else objVal = reader.GetValue(ColumnIndex);
            }
            return objVal;
        }
    }
}
