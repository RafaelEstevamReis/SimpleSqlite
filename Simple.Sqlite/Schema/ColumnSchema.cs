using System;
using System.Drawing;
using System.Linq;
using System.Text;
using Simple.DatabaseWrapper.Attributes;
using Simple.DatabaseWrapper.Interfaces;
using Simple.DatabaseWrapper.TypeReader;

namespace Simple.Sqlite
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
        /// Create a column schema from TypeInfoItem
        /// </summary>
        public static IColumn FromInfo(TypeInfo info, TypeItemInfo pi)
        {
            SqliteType dataType = mapType(pi);

            //Props
            bool isKey = pi.Is(DatabaseWrapper.ColumnAttributes.PrimaryKey);
            // Auto select
            bool allowNulls = dataType == SqliteType.TEXT
                              || dataType == SqliteType.BLOB;
            // was specified ?
            if (pi.Is(DatabaseWrapper.ColumnAttributes.AllowNull)) allowNulls = true;
            if (pi.Is(DatabaseWrapper.ColumnAttributes.NotNull)) allowNulls = false;

            bool isUnique = pi.Is(DatabaseWrapper.ColumnAttributes.Unique);

            object defVal = null;
            foreach (var attr in pi.DBAttributes)
            {
                if (attr.Attribute is DefaultValueAttribute def)
                {
                    defVal = def;
                    break;
                }
            }

            // create
            return new Column()
            {
                ColumnName = pi.Name,
                AllowNulls = allowNulls,
                NativeType = pi.Type,
                SqliteType = dataType,
                DefaultValue = defVal,
                IsPK = isKey,
                IsAI = isKey && dataType == SqliteType.INTEGER,
                IsUnique = isUnique,
            };
        }

        /// <summary>
        /// Creates a column schema from a Type
        /// </summary>

        private static SqliteType mapType(TypeItemInfo info)
        {
            SqliteType dataType;
            // Texts
            if (info.Type == typeof(string)) dataType = SqliteType.TEXT;
            else if (info.Type == typeof(Uri)) dataType = SqliteType.TEXT;
            // Float point Numbers
            else if (info.Type == typeof(float)) dataType = SqliteType.REAL;
            else if (info.Type == typeof(double)) dataType = SqliteType.REAL;
            // Fixed FloatPoint
            else if (info.Type == typeof(decimal)) dataType = SqliteType.NUMERIC;
            // Integers
            else if (info.Type == typeof(byte)) dataType = SqliteType.INTEGER;
            else if (info.Type == typeof(int)) dataType = SqliteType.INTEGER;
            else if (info.Type == typeof(uint)) dataType = SqliteType.INTEGER;
            else if (info.Type == typeof(long)) dataType = SqliteType.INTEGER;
            else if (info.Type == typeof(ulong)) dataType = SqliteType.INTEGER;
            // Others Mapped of NUMERIC
            else if (info.Type == typeof(bool)) dataType = SqliteType.NUMERIC;
            else if (info.Type == typeof(DateTime)) dataType = SqliteType.NUMERIC;
            // Other
            else if (info.Type == typeof(Guid)) dataType = SqliteType.BLOB;
            else if (info.Type == typeof(Color)) dataType = SqliteType.BLOB;
            else if (info.Type == typeof(byte[])) dataType = SqliteType.BLOB;
            //Int enums
            else if (info.Type.IsEnum) dataType = SqliteType.INTEGER;
            else
            {
                throw new Exception($"Type {info.Type.Name} is not supported on field {info.Name}");
            }
            return dataType;
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
