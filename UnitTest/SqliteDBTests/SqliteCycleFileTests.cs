using System.Linq;
using UnitTest.TestModels;
using Xunit;

namespace UnitTest.SqliteDBTests
{
    public class SqliteCycleFileTests : SqliteFileTests
    {
        //[Fact]
        //public void SqliteDB_File_GetAll()
        //{
        //    var db = GetDb();

        //    // Create the table
        //    var result = db.CreateTables()
        //        .Add<SimpleModel>()
        //        .Commit();

        //    db.Insert(new SimpleModel()
        //    {
        //        Name = "First",
        //        Value = 10,
        //        Enabled = true,
        //    });
        //    db.Insert(new SimpleModel()
        //    {
        //        Name = "Second",
        //        Value = 20,
        //        Enabled = true,
        //    });
        //    db.Insert(new SimpleModel()
        //    {
        //        Name = "Third",
        //        Value = 30,
        //        Enabled = false,
        //    });

        //    var allData = db.GetAll<SimpleModel>()
        //                    .ToArray();

        //    Assert.Equal(1, allData[0].Id);
        //    Assert.Equal("First", allData[0].Name);
        //    Assert.Equal(10, allData[0].Value);
        //    Assert.True(allData[0].Enabled);

        //    Assert.Equal(2, allData[1].Id);
        //    Assert.Equal("Second", allData[1].Name);
        //    Assert.Equal(20, allData[1].Value);
        //    Assert.True(allData[1].Enabled);

        //    Assert.Equal(3, allData[2].Id);
        //    Assert.Equal("Third", allData[2].Name);
        //    Assert.Equal(30, allData[2].Value);
        //    Assert.False(allData[2].Enabled);

        //}
    }
}
