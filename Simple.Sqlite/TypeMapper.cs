using System;
using System.Data.SQLite;
using System.Drawing;

namespace Simple.Sqlite
{
    internal static class TypeMapper
    {
        internal static bool CheckIfSimpleType(this Type typeT)
        {
            if (typeT.IsPrimitive) return true;
            if (typeT == typeof(string)) return true;
            if (typeT == typeof(decimal)) return true;
            if (typeT == typeof(DateTime)) return true;
            if (typeT == typeof(DateTimeOffset)) return true;
            if (typeT == typeof(TimeSpan)) return true;
            if (typeT == typeof(Guid)) return true;

            if (IsNullableSimpleType(typeT)) return true;

            return false;
        }
        private static bool IsNullableSimpleType(Type typeT)
        {
            var underlyingType = Nullable.GetUnderlyingType(typeT);
            return underlyingType != null && CheckIfSimpleType(underlyingType);
        }

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
        private static void mapColumn<T>(T obj, System.Reflection.PropertyInfo p, SQLiteDataReader reader, int clnIdx)
            where T : new()
        {
            object objVal = ReadValue(reader, p.PropertyType, clnIdx);

            if (p.PropertyType == typeof(Color) && objVal != null)
            {
                var argb = (byte[])objVal;
                objVal = Color.FromArgb(argb[0], argb[1], argb[2], argb[3]);
            }

            p.SetValue(obj, objVal);
        }
        internal static object ReadValue(SQLiteDataReader reader, Type type, int ColumnIndex)
        {
            object objVal;
            if (reader.IsDBNull(ColumnIndex))
            {
                objVal = null;
            }
            else
            {
                if (type == typeof(string)) objVal = reader.GetValue(ColumnIndex);
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
                else if (type == typeof(byte[])) objVal = (byte[])reader.GetValue(ColumnIndex);
                else if (type == typeof(Guid))
                {
                    objVal = reader.GetValue(ColumnIndex);
                    if (objVal is string) objVal = Guid.Parse((string)objVal);
                    else objVal = new Guid((byte[])objVal);
                }
                else if (type.IsEnum) objVal = reader.GetInt32(ColumnIndex);
                else objVal = reader.GetValue(ColumnIndex);
            }
            return objVal;
        }

        internal static object ReadParam(System.Reflection.PropertyInfo p, object parameters)
        {
            var objVal = p.GetValue(parameters);
            if (p.PropertyType == typeof(Color)) 
            {
                var color = (Color)objVal;
                return new byte[] { color.A, color.R, color.G, color.B };
            }

            return objVal;
        }
    }
}
