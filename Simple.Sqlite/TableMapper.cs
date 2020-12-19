using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Simple.Sqlite
{
    public partial class TableMapper
    {
        private readonly SqliteDB db;
        private readonly List<Table> tables;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        public TableMapper(SqliteDB database)
        {
            db = database;
            tables = new List<Table>();
        }
        /// <summary>
        /// Adds a table
        /// </summary>
        public TableMapper Add<T>() where T : new()
        {
            tables.Add(Table.FromType(typeof(T)));
            return this;
        }
        /// <summary>
        /// Allows last added table to be editted
        /// </summary>
        public TableMapper ConfigureTable(Action<Table> Options)
        {
            Options(tables[^1]);
            return this;
        }
        /// <summary>
        /// Commit all new tables to the db (old schemas are not updated (yet)
        /// </summary>
        public TableCommitResult[] Commit()
        {
            var results = new List<TableCommitResult>();

            foreach (var t in tables)
            {
                var tResult = commitTable(t);
                if (tResult != null) results.Add(tResult);
            }

            tables.Clear();
            return results.ToArray();
        }

        private TableCommitResult commitTable(Table t)
        {
            int val = db.ExecuteNonQuery(t.ExportCreateTable(), null);
            if (val == 0) // table created
            {
                return new TableCommitResult()
                {
                    TableName = t.TableName,
                    WasTableCreated = true,
                    ColumnsAdded = new string[0],
                };
            }

            if (val == -1) // Table not created
            {
                // migrate ?
                var dbColumns = db.GetTableSchema(t.TableName)
                                  .Rows.Cast<DataRow>()
                                  .Select(r => (string)r["ColumnName"]);

                var newColumns = t.Columns
                                  .Where(c => !dbColumns.Contains(c.ColumnName))
                                  .ToArray();

                foreach (var c in newColumns)
                {
                    string addColumn = c.ExportAddColumnAsStatement();
                    db.ExecuteNonQuery($"ALTER TABLE {t.TableName} {addColumn}", null);
                }

                if (newColumns.Length > 0)
                {
                    return new TableCommitResult()
                    {
                        TableName = t.TableName,
                        WasTableCreated = false,
                        ColumnsAdded = newColumns.Select(o => o.ColumnName)
                                                   .ToArray(),
                    };
                }
            }
            return null;
        }

        public class TableCommitResult
        {
            public string TableName { get; set; }
            public bool WasTableCreated { get; set; }
            public string[] ColumnsAdded { get; set; }

        }

    }
}
