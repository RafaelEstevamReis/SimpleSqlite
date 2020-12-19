using System;
using System.Data;
using System.Linq;
using System.Text;

namespace Simple.Sqlite
{
    /// <summary>
    /// Class to map a table schema
    /// </summary>
    partial class TableMapper
    {
        /// <summary>
        /// Represents a table schema
        /// </summary>
        public class Table
        {
            /// <summary>
            /// Table's name
            /// </summary>
            public string TableName { get; set; }
            /// <summary>
            /// Table's columns
            /// </summary>
            public Column[] Columns { get; set; }

            /// <summary>
            /// Creates a CREATE TABLE statment from current schema
            /// </summary>
            public string ExportCreateTable()
            {
                if (string.IsNullOrEmpty(TableName)) throw new ArgumentNullException("TableName can not be null");
                if (TableName.Any(c => char.IsWhiteSpace(c))) throw new ArgumentNullException("TableName can not contain whitespaces");
                if (TableName.Any(c => char.IsSymbol(c))) throw new ArgumentNullException("TableName can not contain symbols");

                if (Columns == null) throw new ArgumentNullException("Columns can not be null");
                if (Columns.Length == 0) throw new ArgumentNullException("Columns can not empty");

                /*
                 CREATE TABLE [IF NOT EXISTS] [schema_name].table_name (
                     column_1 data_type PRIMARY KEY,
                     column_2 data_type NOT NULL,
                     column_3 data_type DEFAULT 0,
                     table_constraints
                 ) [WITHOUT ROWID];
                 */

                StringBuilder sb = new StringBuilder();

                sb.Append("CREATE TABLE IF NOT EXISTS ");
                sb.Append(TableName);
                sb.Append("(\n");

                var columns = string.Join(',', Columns.Select(c => c.ExportColumnDefinitionAsStatement()));
                sb.Append(columns);
                sb.Append("\n);");

                return sb.ToString();
            }

            /// <summary>
            /// Creates a table schema from a Type
            /// </summary>
            public static Table FromType(Type t)
            {
                var props = t.GetProperties();

                return new Table()
                {
                    TableName = t.Name,
                    Columns = props.Select(pi => Column.FromType(t, pi))
                                   .ToArray(),
                };
            }
        }
    }
}
