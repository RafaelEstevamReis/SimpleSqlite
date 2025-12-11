using System;
using System.Linq;
using System.Text;
using Simple.DatabaseWrapper.Interfaces;
using Simple.DatabaseWrapper.TypeReader;

namespace Simple.Sqlite
{
    /// <summary>
    /// Represents a table schema
    /// </summary>
    public class Table : ITable
    {
        /// <summary>
        /// Table's name
        /// </summary>
        public string TableName { get; set; }
        /// <summary>
        /// Table's columns
        /// </summary>
        public IColumn[] Columns { get; set; }
        /// <summary>
        /// Creates table with 'STRICT' keyword
        /// </summary>
        public bool AsStrict { get; set; }

        /// <summary>
        /// Gets the N-th column
        /// </summary>
        public IColumn this[int index] => Columns[index];
        /// <summary>
        /// Gets the column by name
        /// </summary>
        public IColumn this[string name] => Columns.First(c => c.ColumnName == name);

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

            var columns = string.Join(",", Columns.Select(c => c.ExportColumnDefinitionAsStatement()));
            sb.Append(columns);
            sb.Append("\n)");

            if(AsStrict) sb.Append(" STRICT ");

            sb.Append(';');

            return sb.ToString();
        }

        /// <summary>
        /// Creates a table schema from a Type
        /// </summary>
        public static Table FromType(TypeInfo info)
        {
            var props = info.Items
                .Where(o => o.ItemType == DatabaseWrapper.ItemType.Property)
                .Where(o => !o.Is(DatabaseWrapper.ColumnAttributes.Ignore))
                .Where(o => o.CanRead && o.CanWrite);

            return new Table()
            {
                TableName = info.TypeName,
                Columns = props.Select(pi => Column.FromInfo(info, pi))
                               .ToArray(),
            };
        }
    }
}