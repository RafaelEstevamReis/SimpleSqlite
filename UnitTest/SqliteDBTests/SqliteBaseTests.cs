using Simple.Sqlite;
using Xunit;

namespace UnitTest.SqliteDBTests
{
    public class SqliteBaseTests
    {
        [Fact]
        public void SqliteDB_TestConnection()
        {
            var db = SqliteDB.CreateInMemory();
            Assert.Equal(1, db.ExecuteScalar<int>("SELECT 1", null));
        }

    }
}
