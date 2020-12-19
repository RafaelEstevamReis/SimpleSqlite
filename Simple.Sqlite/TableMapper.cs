using System;
using System.Collections.Generic;
using System.Text;

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
        public void Commit()
        {
            foreach (var t in tables)
            {
                db.ExecuteNonQuery(t.ExportCreateTable(), null);
            }

            tables.Clear();
        }
    }
}
