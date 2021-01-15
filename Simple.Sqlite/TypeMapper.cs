using System;
using System.Collections.Generic;
using System.Data;
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

        internal static T MapObject<T>(HashSet<string> colNames, SQLiteDataReader reader)
        {
            object t = Activator.CreateInstance<T>();
            foreach (var p in typeof(T).GetProperties())
            {
                if (!colNames.Contains(p.Name)) continue;

                mapColumn(t, p, reader);
            }
            return (T)t;
        }
        private static void mapColumn<T>(T obj, System.Reflection.PropertyInfo p, SQLiteDataReader reader)
            where T : new()
        {
            object objVal = ReadValue(reader, p.PropertyType, p.Name);
            p.SetValue(obj, objVal);
        }
        internal static object ReadValue(SQLiteDataReader reader, Type type, string name)
        {
            object objVal;
            if (reader.IsDBNull(name))
            {
                objVal = null;
            }
            else
            {
                if (type == typeof(string)) objVal = reader.GetValue(name);
                else if (type == typeof(Uri)) objVal = new Uri((string)reader.GetValue(name));
                else if (type == typeof(double)) objVal = reader.GetDouble(name);
                else if (type == typeof(float)) objVal = reader.GetFloat(name);
                else if (type == typeof(decimal)) objVal = reader.GetDecimal(name);
                else if (type == typeof(int)) objVal = reader.GetInt32(name);
                else if (type == typeof(uint)) objVal = Convert.ToUInt32(reader.GetValue(name));
                else if (type == typeof(long)) objVal = reader.GetInt64(name);
                else if (type == typeof(ulong)) objVal = Convert.ToUInt64(reader.GetValue(name));
                else if (type == typeof(bool)) objVal = reader.GetBoolean(name);
                else if (type == typeof(DateTime)) objVal = reader.GetDateTime(name);
                else if (type == typeof(byte[])) objVal = (byte[])reader.GetValue(name);
                else if (type == typeof(Color))
                {
                    var bytes = (byte[])reader.GetValue(name);
                    objVal = Color.FromArgb(bytes[0], bytes[1], bytes[2], bytes[3]);
                }
                else if (type == typeof(Guid))
                {
                    objVal = reader.GetValue(name);
                    if (objVal is string) objVal = Guid.Parse((string)objVal);
                    else objVal = new Guid((byte[])objVal);
                }
                else if (type.IsEnum) objVal = reader.GetInt32(name);
                else objVal = reader.GetValue(name);
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
