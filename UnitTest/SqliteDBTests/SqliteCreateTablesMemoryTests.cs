using Simple.Sqlite;
using UnitTest.TestModels;
using Xunit;

namespace UnitTest.SqliteDBTests
{
    public class SqliteCreateTablesMemoryTests
    {
        //[Fact]
        //public void SqliteDB_InMemory_CreateTable_Result()
        //{
        //    var db = SqliteDB.CreateInMemory();
        //    // Assert that thar are no tables
        //    Assert.Empty(db.GetAllTables());

        //    // Create the table
        //    var result = db.CreateTables()
        //        .Add<SimpleModel>()
        //        .Commit();

        //    // One table was created
        //    Assert.Single(result);
        //    // The one is the model
        //    Assert.Equal("SimpleModel", result[0].TableName);
        //    Assert.True(result[0].WasTableCreated);
        //    Assert.Empty(result[0].ColumnsAdded);
        //}


    }
}
