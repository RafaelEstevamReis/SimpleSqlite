using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Simple.Sqlite
{
    public class SimpleTableSchema
    {
        public string TableName { get; set; }
        public SimpleColumnSchema[] Columns { get; set; }

        public string ExportCreateTableStatement(bool AddIfNotExists)
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

            sb.Append("CREATE TABLE ");
            if (AddIfNotExists)
            {
                sb.Append("IF NOT EXISTS ");
            }
            sb.Append(TableName);
            sb.Append("(\n");

            var columns = string.Join(',', Columns.Select(c => c.ExportColumnAsStatement()));
            sb.Append(columns);
            sb.Append("\n);");

            return sb.ToString();
        }

        public static SimpleTableSchema BuildFromType<T>(string TableName, string AI_PK_Column = null)
        {
            var t = typeof(T);

            List<SimpleColumnSchema> columns = new List<SimpleColumnSchema>();

            foreach (var v in t.GetProperties())
            {
                var cln = SimpleColumnSchema.BuildFromProperty(v);

                if (!string.IsNullOrEmpty(AI_PK_Column))
                {
                    if (AI_PK_Column.Equals(cln.ColumnName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        if (v.PropertyType != typeof(int)) throw new Exception("AI_PK_Column must be INT");

                        cln.IsAI = true;
                        cln.IsPK = true;
                    }
                }

                columns.Add(cln);
            }

            return new SimpleTableSchema()
            {
                TableName = TableName,
                Columns = columns.ToArray()
            };
        }
    }
}
