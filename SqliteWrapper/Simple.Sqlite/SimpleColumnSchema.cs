using System;
using System.Linq;
using System.Text;

namespace Simple.Sqlite
{
    public class SimpleColumnSchema
    {
        public string ColumnName { get; set; }
        public SqliteType DataType { get; set; }
        public SqliteCollate Collate { get; set; }

        public bool IsPK { get; set; }
        public bool IsAI { get; set; }
        public bool AllowNulls { get; set; }
        public object DefaultValue { get; set; }

        public string ExportColumnAsStatement()
        {
            if (string.IsNullOrEmpty(ColumnName)) throw new ArgumentNullException("ColumnName can not be null");
            if (ColumnName.Any(c => char.IsWhiteSpace(c))) throw new ArgumentNullException("ColumnName can not contain whitespaces");
            if (ColumnName.Any(c => char.IsSymbol(c))) throw new ArgumentNullException("ColumnName can not contain symbols");

            StringBuilder sb = new StringBuilder();

            sb.Append(ColumnName);
            sb.Append(" ");

            sb.Append(DataType.ToString());
            sb.Append(" ");

            if (IsPK)
            {
                sb.Append("PRIMARY KEY ");
            }
            if (IsPK)
            {
                sb.Append("AUTOINCREMENT ");
            }

            if (!AllowNulls)
            {
                sb.Append("NOT NULL ");
            }

            // Collate
            /*
             CREATE TABLE t1(
                x INTEGER PRIMARY KEY,
                a,                 -- collating sequence BINARY
                b COLLATE BINARY,  -- collating sequence BINARY
                c COLLATE RTRIM,   -- collating sequence RTRIM
                d COLLATE NOCASE   -- collating sequence NOCASE);
            */
            if (Collate != SqliteCollate.None)
            {
                sb.Append("COLLATE ");
                string collate = Collate switch
                {
                    SqliteCollate.Binary => "BINARY ",
                    SqliteCollate.RTtrim => "RTRIM ",
                    SqliteCollate.Nocase => "NOCASE ",
                    _ => "",
                };
                sb.Append(collate);
            }

            if (DefaultValue != null)
            {
                sb.Append($"DEFAULT '{DefaultValue}'");
            }

            return sb.ToString();
        }

        internal static SimpleColumnSchema BuildFromProperty(System.Reflection.PropertyInfo info)
        {
            SqliteType dataType;

            // Texts
            if (info.PropertyType == typeof(string)) dataType = SqliteType.TEXT;
            // Float point Numbers
            else if (info.PropertyType == typeof(float)) dataType = SqliteType.REAL;
            else if (info.PropertyType == typeof(double)) dataType = SqliteType.REAL;
            // Fixed FloatPoint
            else if (info.PropertyType == typeof(decimal)) dataType = SqliteType.NUMERIC;
            // Integers
            else if (info.PropertyType == typeof(int)) dataType = SqliteType.INTEGER;
            else if (info.PropertyType == typeof(byte)) dataType = SqliteType.INTEGER;
            else if (info.PropertyType == typeof(long)) dataType = SqliteType.INTEGER;
            // Others Mappep of NUMERIC
            else if (info.PropertyType == typeof(bool)) dataType = SqliteType.NUMERIC;
            else if (info.PropertyType == typeof(DateTime)) dataType = SqliteType.NUMERIC;
            else
            {
                throw new Exception($"Type {info.PropertyType.Name} is not supported on field {info.Name}");
            }

            return new SimpleColumnSchema()
            {
                ColumnName = info.Name,
                DataType = dataType,
                Collate = SqliteCollate.None,
                AllowNulls = true,
                IsPK = false,
                IsAI = false,
                DefaultValue = null
            };
        }
    }
}
