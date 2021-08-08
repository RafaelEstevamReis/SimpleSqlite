using Simple.Sqlite;
using Xunit;

namespace UnitTest.SqliteDBTests
{
    public class SqliteGetTableSchemaTests
    {
        [Fact]
        public void SqliteDB_GetAllTables_Create()
        {
            var db = SqliteDB.CreateInMemory();
            // Creates a new table and check it
            db.Execute("CREATE TABLE test (c1 int, c2 text)", null);

            var schema = db.GetTableSchema("test");

            Assert.Equal(2, schema.Rows.Count);
            Assert.Equal("c1", schema.Rows[0][0].ToString());
            Assert.StartsWith("System.Int", schema.Rows[0]["DataType"].ToString());

            Assert.Equal("c2", schema.Rows[1][0].ToString());
            Assert.Equal("System.String", schema.Rows[1]["DataType"].ToString());

        }
    }
}
