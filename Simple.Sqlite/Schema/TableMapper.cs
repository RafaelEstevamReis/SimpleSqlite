using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Simple.DatabaseWrapper.Interfaces;
using Simple.DatabaseWrapper.TypeReader;

namespace Simple.Sqlite
{
    /// <summary>
    /// Table mapping class
    /// </summary>
    public class TableMapper : IColumnMapper
    {
        private readonly SqliteDB db;
        private readonly ReaderCachedCollection typeCollection;
        private readonly List<Table> tables;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        public TableMapper(SqliteDB database, ReaderCachedCollection typeCollection)
        {
            db = database;
            this.typeCollection = typeCollection;
            tables = new List<Table>();
        }
        /// <summary>
        /// Adds a table
        /// </summary>
        public IColumnMapper Add<T>() where T : new()
        {
            var info = typeCollection.GetInfo<T>();
            tables.Add(Table.FromType(info));
            return this;
        }
        /// <summary>
        /// Allows last added table to be editted
        /// </summary>
        public ITableMapper ConfigureTable(Action<ITable> Options)
        {
#if NETSTANDARD || NETFRAMEWORK
            Options(tables.Last());
#else
            Options(tables[^1]);
#endif
            return this;
        }
        /// <summary>
        /// Commit all new tables to the db (old schemas are not updated (yet)
        /// </summary>
        public ITableCommitResult[] Commit()
        {
            var results = new List<TableCommitResult>();

            var tableNames = db.Query<string>(@"SELECT name
 FROM sqlite_master
 WHERE type = 'table' ", null).ToArray();

            foreach (var t in tables)
            {
                var tResult = commitTable(t, existingTable: tableNames.Contains(t.TableName));
                if (tResult != null) results.Add(tResult);
            }

            tables.Clear();
            return results.ToArray();
        }

        private TableCommitResult commitTable(Table t, bool existingTable)
        {
            if (!existingTable)
            {
                db.Execute(t.ExportCreateTable(), null);
                return new TableCommitResult()
                {
                    TableName = t.TableName,
                    WasTableCreated = true,
                    ColumnsAdded = new string[0],
                };
            }

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
                db.Execute($"ALTER TABLE {t.TableName} {addColumn}", null);
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

            return null;
        }
        /// <summary>
        /// Class for the table commit results
        /// </summary>
        public class TableCommitResult : ITableCommitResult
        {
            /// <summary>
            /// Name of the table added/altered
            /// </summary>
            public string TableName { get; set; }
            /// <summary>
            /// Gets if the table was altered
            /// </summary>
            public bool WasTableCreated { get; set; }
            /// <summary>
            /// Gets the new added columns, if any
            /// </summary>
            public string[] ColumnsAdded { get; set; }

        }
    }
}
