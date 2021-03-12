using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using Simple.DatabaseWrapper.Attributes;
using Simple.DatabaseWrapper.Interfaces;

namespace Simple.Sqlite
{
    partial class TableMapper
    {
        /// <summary>
        /// Class to map a column schema
        /// </summary>
        public class Column : IColumn
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
            /// Is Unique indexed ?
            /// </summary>
            public bool IsUnique { get; set; }
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
                SqliteType dataType = mapType(info);

                //Props
                bool isKey = IsKeyProp(info) || IsPKProp(info);
                // Auto select
                bool allowNulls = dataType == SqliteType.TEXT
                                  || dataType == SqliteType.BLOB;
                // was specified ?
                if (IsAllowNullsProp(info)) allowNulls = true;
                if (IsNotNullsProp(info)) allowNulls = false;

                bool isUnique = IsUniqueProp(info);

                object defVal = null;
                if (IsDefaultValueProp(info, out object outObj))
                {
                    defVal = outObj;
                }

                // create
                return new Column()
                {
                    ColumnName = info.Name,
                    AllowNulls = allowNulls,
                    NativeType = info.PropertyType,
                    SqliteType = dataType,
                    DefaultValue = defVal,
                    IsPK = isKey,
                    IsAI = isKey && dataType == SqliteType.INTEGER,
                    IsUnique = isUnique,
                };
            }

            private static SqliteType mapType(PropertyInfo info)
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
                else if (info.PropertyType == typeof(Color)) dataType = SqliteType.BLOB;
                else if (info.PropertyType == typeof(byte[])) dataType = SqliteType.BLOB;
                //Int enums
                else if (info.PropertyType.IsEnum) dataType = SqliteType.INTEGER;
                else
                {
                    throw new Exception($"Type {info.PropertyType.Name} is not supported on field {info.Name}");
                }
                return dataType;
            }
            internal static bool IsKeyProp(PropertyInfo info)
            {
                return info.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.KeyAttribute), true)
                           .FirstOrDefault() != null;
            }
            internal static bool IsPKProp(PropertyInfo info)
            {
                return info.GetCustomAttributes(typeof(PrimaryKeyAttribute), true)
                           .FirstOrDefault() != null;
            }
            internal static string GetKeyColumn(Type typeT)
            {
                return typeT.GetProperties()
                            .Where(p => IsPKProp(p) || IsKeyProp(p))
                            .FirstOrDefault()
                            ?.Name;
            }
            internal static bool IsUniqueProp(PropertyInfo info)
            {
                return info.GetCustomAttributes(typeof(UniqueAttribute), true)
                           .FirstOrDefault() != null;
            }
            internal static bool IsAllowNullsProp(PropertyInfo info)
            {
                return info.GetCustomAttributes(typeof(AllowNullAttribute), true)
                           .FirstOrDefault() != null;
            }
            internal static bool IsNotNullsProp(PropertyInfo info)
            {
                return info.GetCustomAttributes(typeof(NotNullAttribute), true)
                           .FirstOrDefault() != null;
            }
            internal static bool IsDefaultValueProp(PropertyInfo info, out object Value)
            {
                Value = null;
                var prop = info.GetCustomAttributes(typeof(DefaultValueAttribute), true)
                           .FirstOrDefault() as DefaultValueAttribute;
                if (prop == null) return false;

                Value = prop.DefaultValue;
                return true;
            }
            /// <summary>
            /// Creates a CREATE TABLE column statment from current schema
            /// </summary>
            public string ExportColumnDefinitionAsStatement()
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
                if (IsUnique) sb.Append("UNIQUE ");

                if (!AllowNulls) sb.Append("NOT NULL ");

                if (DefaultValue != null)
                {
                    sb.Append($"DEFAULT '{DefaultValue}'");
                }

                return sb.ToString();
            }

            /// <summary>
            /// Creates a ADD COLUMN from current schema. 
            /// This MAY change de [DefaultValue] when [NotNull] to Comply with Sqlite
            /// </summary>
            /// <returns></returns>
            public string ExportAddColumnAsStatement()
            {
                if (string.IsNullOrEmpty(ColumnName)) throw new ArgumentNullException("ColumnName can not be null");
                if (ColumnName.Any(c => char.IsWhiteSpace(c))) throw new ArgumentNullException("ColumnName can not contain whitespaces");
                if (ColumnName.Any(c => char.IsSymbol(c))) throw new ArgumentNullException("ColumnName can not contain symbols");

                StringBuilder sb = new StringBuilder();

                sb.Append(" ADD COLUMN ");

                sb.Append(ColumnName);
                sb.Append(" ");

                sb.Append(SqliteType.ToString());
                sb.Append(" ");

                if (IsAI) sb.Append("AUTOINCREMENT ");

                // Columns with PK cannot be added in Sqlite
                // Columns with UNIQUE cannot be added in Sqlite
                // Columns with [NOT NULL] MUST HAVE a [DEFAULT VALUE]
                if (!AllowNulls)
                {
                    sb.Append("NOT NULL ");

                    if (DefaultValue == null)
                    {
                        setReasonableDefault(this);
                    }
                }

                if (DefaultValue != null)
                {
                    sb.Append($"DEFAULT '{DefaultValue}'");
                }

                return sb.ToString();
            }

            private void setReasonableDefault(Column column)
            {
                if (column.NativeType == typeof(DateTime))
                {
                    column.DefaultValue = DateTime.MinValue;
                    return;
                }

                switch (SqliteType)
                {
                    case SqliteType.NUMERIC:
                    case SqliteType.INTEGER:
                    case SqliteType.REAL:
                        column.DefaultValue = 0;
                        break;
                    case SqliteType.TEXT: // NotNull text, is empty
                        column.DefaultValue = string.Empty;
                        break;
                }
            }
        }
    }
}
