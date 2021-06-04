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
            // Each new instance creates a new number to allow shared access to unique databases
            // For that, each new test has a new number
            // the test, cannot know the current number
            // "InMemoryXXX"
            Assert.StartsWith("InMemory", db.DatabaseFileName);

            Assert.True(db.IsInMemoryDatabase);

        }
    }
}
