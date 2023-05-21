using UnitTest.TestModels;
using Xunit;

namespace UnitTest.SqliteDBTests
{
    public class SqliteFileQuery : SqliteFileTests
    {
        //[Fact]
        //public void SqliteDB_File_Query()
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

        //    var v1 = db.Get<SimpleModel>(1);
        //    Assert.Equal(1, v1.Id);
        //    Assert.Equal("First", v1.Name);
        //    Assert.Equal(10, v1.Value);
        //    Assert.True(v1.Enabled);

        //    var v2 = db.QueryOrDefault<SimpleModel>("SELECT * FROM SimpleModel WHERE Id = @id", new { id = 2 });
        //    Assert.Equal(2, v2.Id);
        //    Assert.Equal("Second", v2.Name);
        //    Assert.Equal(20, v2.Value);
        //    Assert.True(v2.Enabled);

        //    var v4 = db.QueryOrDefault<SimpleModel>("SELECT * FROM SimpleModel WHERE Id = @id", new { id = 4 });
        //    Assert.Null(v4);
        //}
    }
}
