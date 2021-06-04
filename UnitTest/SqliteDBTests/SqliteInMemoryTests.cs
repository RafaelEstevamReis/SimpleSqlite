using Simple.Sqlite;
using Xunit;

namespace UnitTest.SqliteDBTests
{
    public class SqliteInMemoryTests
    {
        [Fact]
        public void SqliteDB_InMemory_FileName()
        {
            var db = SqliteDB.CreateInMemory();
            Assert.StartsWith("", db.DatabaseFileName);
            Assert.True(db.IsInMemoryDatabase);
        }
    }
}
