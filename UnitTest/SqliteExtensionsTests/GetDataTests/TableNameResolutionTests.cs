namespace UnitTest.SqliteExtensionsTests.GetDataTests;

using Simple.DatabaseWrapper.Attributes;
using Simple.Sqlite;
using System.Linq;
using Xunit;

// Locks the contract that GetAll<T>/GetWhere<T> resolve the SAME table that
// Insert<T>/Get<T> use (via the type-info table name), instead of diverging
// to a separately-computed name.
public class TableNameResolutionTests
{
    [Fact]
    public void GetData_GetAll_ReadsRowsWrittenByInsert()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<Person>().Commit();

        db.Insert(new Person { Id = 1, Name = "a" });
        db.Insert(new Person { Id = 2, Name = "b" });

        Assert.Equal(2, db.GetAll<Person>().Count());
    }

    [Fact]
    public void GetData_GetWhere_ReadsRowsWrittenByInsert()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<Person>().Commit();

        db.Insert(new Person { Id = 1, Name = "a" });
        db.Insert(new Person { Id = 2, Name = "b" });

        var found = db.GetWhere<Person>("Name", "b").Single();

        Assert.Equal(2, found.Id);
    }

    [Fact]
    public void GetData_InsertGetGetAllGetWhere_AllAgreeOnTable()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<Person>().Commit();

        db.Insert(new Person { Id = 10, Name = "x" });

        var byPk = db.Get<Person>(10);
        var all = db.GetAll<Person>().ToArray();
        var where = db.GetWhere<Person>("Id", 10).ToArray();

        Assert.NotNull(byPk);
        Assert.Single(all);
        Assert.Single(where);
        Assert.Equal("x", byPk.Name);
        Assert.Equal(10, all[0].Id);
        Assert.Equal(10, where[0].Id);
    }

    public class Person
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
