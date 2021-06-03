using Simple.Sqlite;
using Xunit;

namespace UnitTest.SqliteDBTests
{
    public class SqliteGetAllTables
    {
        [Fact]
        public void SqliteDB_GetAllTables_Empty()
        {
            var db = SqliteDB.CreateInMemory();
            
            // Asser that there are no tables
            int count = db.ExecuteScalar<int>("SELECT COUNT(1) FROM sqlite_master WHERE type='table'", null);
            Assert.Equal(0, count);
            // Check that matches with GetAll
            Assert.Empty(db.GetAllTables());
        }

        [Fact]
        public void SqliteDB_GetAllTables_Create()
        {
            var db = SqliteDB.CreateInMemory();
            Assert.Empty(db.GetAllTables());

            // Creates a new table and check it
            db.Execute("CREATE TABLE test (c1, c2)", null);

            // Should be one table
            var tables = db.GetAllTables();
            Assert.Single(tables);

            Assert.Equal("test", tables[0]);
        }
    }
}
