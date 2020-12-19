using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Simple.Sqlite
{
    partial class TableMapper
    {
        /// <summary>
        /// Class to map a column schema
        /// </summary>
        public class Column

        {
            /// <summary>
            /// Column name
            /// </summary>
            public string ColumnName { get; set; }
            /// <summary>
            /// Type on SQLite database
            /// </summary>
            public SqliteType SqliteType { get; set; }
            /// <summary>
            /// Native object type
            /// </summary>
            public Type NativeType { get; set; }
            /// <summary>
            /// Is PrimaryKey ?
            /// </summary>
            public bool IsPK { get; set; }
            /// <summary>
            /// Is Auto-Increment ?
            /// </summary>
            public bool IsAI { get; set; }
            /// <summary>
            /// Allow null values ?
            /// </summary>
            public bool AllowNulls { get; set; }
            /// <summary>
            /// Default value on NULL
            /// </summary>
            public object DefaultValue { get; set; }

            /// <summary>
            /// Creates a column schema from a Type
            /// </summary>
            public static Column FromType(Type t, PropertyInfo info)
            {

                SqliteType dataType;

                // Texts
                if (info.PropertyType == typeof(string)) dataType = SqliteType.TEXT;
                else if (info.PropertyType == typeof(Uri)) dataType = SqliteType.TEXT;
                // Float point Numbers
                else if (info.PropertyType == typeof(float)) dataType = SqliteType.REAL;
                else if (info.PropertyType == typeof(double)) dataType = SqliteType.REAL;
                // Fixed FloatPoint
                else if (info.PropertyType == typeof(decimal)) dataType = SqliteType.NUMERIC;
                // Integers
                else if (info.PropertyType == typeof(byte)) dataType = SqliteType.INTEGER;
                else if (info.PropertyType == typeof(int)) dataType = SqliteType.INTEGER;
                else if (info.PropertyType == typeof(uint)) dataType = SqliteType.INTEGER;
                else if (info.PropertyType == typeof(long)) dataType = SqliteType.INTEGER;
                else if (info.PropertyType == typeof(ulong)) dataType = SqliteType.INTEGER;
                // Others Mapped of NUMERIC
                else if (info.PropertyType == typeof(bool)) dataType = SqliteType.NUMERIC;
                else if (info.PropertyType == typeof(DateTime)) dataType = SqliteType.NUMERIC;
                // Other
                else if (info.PropertyType == typeof(Guid)) dataType = SqliteType.BLOB;
                else if (info.PropertyType == typeof(byte[])) dataType = SqliteType.BLOB;
                else
                {
                    throw new Exception($"Type {info.PropertyType.Name} is not supported on field {info.Name}");
                }
                bool isKey = IsKeyProp(info);

                return new Column()
                {
                    ColumnName = info.Name,
                    AllowNulls = dataType == SqliteType.TEXT
                                 || dataType == SqliteType.BLOB,
                    NativeType = info.PropertyType,
                    SqliteType = dataType,
                    DefaultValue = null,
                    IsPK = isKey,
                    IsAI = isKey && dataType == SqliteType.INTEGER
                };
            }

            internal static bool IsKeyProp(PropertyInfo info)
            {
                return info.GetCustomAttributes(typeof(KeyAttribute), true)
                           .FirstOrDefault() != null;
            }
            /// <summary>
            /// Creates a CREATE TABLE column statment from current schema
            /// </summary>
            public string ExportColumnAsStatement()
            {
                {
                    if (string.IsNullOrEmpty(ColumnName)) throw new ArgumentNullException("ColumnName can not be null");
                    if (ColumnName.Any(c => char.IsWhiteSpace(c))) throw new ArgumentNullException("ColumnName can not contain whitespaces");
                    if (ColumnName.Any(c => char.IsSymbol(c))) throw new ArgumentNullException("ColumnName can not contain symbols");

                    StringBuilder sb = new StringBuilder();

                    sb.Append(ColumnName);
                    sb.Append(" ");

                    sb.Append(SqliteType.ToString());
                    sb.Append(" ");

                    if (IsPK) sb.Append("PRIMARY KEY ");
                    if (IsAI) sb.Append("AUTOINCREMENT ");

                    if (!AllowNulls) sb.Append("NOT NULL ");

                    if (DefaultValue != null)
                    {
                        sb.Append($"DEFAULT '{DefaultValue}'");
                    }

                    return sb.ToString();
                }
            }

        }
    }
}