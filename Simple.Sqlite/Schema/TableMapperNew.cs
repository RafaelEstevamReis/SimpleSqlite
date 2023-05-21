using Simple.DatabaseWrapper.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Simple.Sqlite
{
    public partial class TableMapperNew : IColumnMapper
    {
        private readonly List<Table> tables;
        private readonly ISqliteConnection connection;

        public bool DisposeOnCommit { get; set; } = false;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        public TableMapperNew(ISqliteConnection connection)
        {
            tables = new List<Table>();
            this.connection = connection;
        }

        /// <summary>
        /// Adds a table
        /// </summary>
        public IColumnMapper Add<T>() where T : new()
        {
            var info = connection.typeCollection.GetInfo<T>();
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

            var tableNames = connection.Query<string>(@"SELECT name
 FROM sqlite_master
 WHERE type = 'table' ", null).ToArray();

            foreach (var t in tables)
            {
                var tResult = commitTable(t, existingTable: tableNames.Contains(t.TableName));
                if (tResult != null) results.Add(tResult);
            }

            tables.Clear();
            if (DisposeOnCommit) connection.Dispose();
            return results.ToArray();
        }

        private TableCommitResult commitTable(Table t, bool existingTable)
        {
            if (!existingTable)
            {
                connection.Execute(t.ExportCreateTable(), null);
                return new TableCommitResult()
                {
                    TableName = t.TableName,
                    WasTableCreated = true,
                    ColumnsAdded = new string[0],
                };
            }

            // migrate ?
            var dbColumns = connection.GetTableColumnNames(t.TableName);
            var newColumns = t.Columns
                              .Where(c => !dbColumns.Contains(c.ColumnName))
                              .ToArray();

            foreach (var c in newColumns)
            {
                string addColumn = c.ExportAddColumnAsStatement();
                connection.Execute($"ALTER TABLE {t.TableName} {addColumn}", null);
            }

            var lstExistingIndexes = connection.Query<string>("SELECT name FROM sqlite_master WHERE type = 'index';", null)
                                               .ToList();
            var newIndexes = t.Columns.SelectMany(c => c.Indexes)
                                      .Distinct()
                                      .Where(ix => !lstExistingIndexes.Contains(ix))
                                      .ToArray();

            foreach (var ix in newIndexes)
            {
                var columns = t.Columns.Where(c => c.Indexes.Contains(ix))
                                       .Select(c => c.ColumnName)
                                       .ToArray();

                string columnList = string.Join(", ", columns);
                connection.Execute($"CREATE INDEX {ix} ON {t.TableName} ({columnList});", null);
            }

            if (newColumns.Length > 0)
            {
                return new TableCommitResult()
                {
                    TableName = t.TableName,
                    WasTableCreated = false,
                    ColumnsAdded = newColumns.Select(o => o.ColumnName)
                                               .ToArray(),
                    IndexesAdded = newIndexes,
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
            public string[] IndexesAdded { get; set; }

        }
    }
}
