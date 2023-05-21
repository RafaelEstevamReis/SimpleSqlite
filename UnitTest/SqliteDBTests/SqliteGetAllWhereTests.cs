using Simple.Sqlite;
using System.Linq;
using UnitTest.TestModels;
using Xunit;

namespace UnitTest.SqliteDBTests
{
    public class SqliteGetAllWhereTests
    {
        //[Fact]
        //public void SqliteDB_InMemory_GetAllWhere()
        //{
        //    var db = SqliteDB.CreateInMemory();
        //    // Assert that thar are no tables
        //    Assert.Empty(db.GetAllTables());

        //    // Create the table
        //    db.CreateTables()
        //        .Add<SimpleModel>()
        //        .Commit();

        //    db.Insert(new SimpleModel()
        //    {
        //        Name = "Test",
        //        Value = 0.2f
        //    });

        //    var data = db.GetAllWhere<SimpleModel>("Name", "Test");
        //    Assert.Single(data);

        //    Assert.Equal(0.2f, data.First().Value);
        //}
    }
}
