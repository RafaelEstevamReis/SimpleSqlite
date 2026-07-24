namespace UnitTest.SchemaTests;

using Microsoft.Data.Sqlite;
using Simple.DatabaseWrapper.Attributes;
using Simple.Sqlite;
using Simple.Sqlite.Attributes;
using System.Linq;
using Xunit;

public class MigrationSchemaTests
{
    [Fact]
    public void Commit_NewTable_ReportsCreated()
    {
        using var db = ConnectionFactory.CreateInMemory();

        var results = db.CreateTables().Add<Model>().Commit();

        var r = Assert.Single(results);
        Assert.True(r.WasTableCreated);
        Assert.Equal("Model", r.TableName);
    }

    [Fact]
    public void Commit_SecondTimeUnchanged_IsNoOp()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<Model>().Commit();

        var second = db.CreateTables().Add<Model>().Commit();

        Assert.Empty(second);
    }

    [Fact]
    public void Commit_ExistingTableMissingColumns_AddsColumns()
    {
        using var db = ConnectionFactory.CreateInMemory();
        // pre-create a partial table (only Id)
        db.Execute("CREATE TABLE Model (Id INTEGER PRIMARY KEY)");

        var results = db.CreateTables().Add<Model>().Commit();

        var r = Assert.Single(results);
        Assert.False(r.WasTableCreated);
        Assert.Contains("Name", r.ColumnsAdded);
        Assert.Contains("Age", r.ColumnsAdded);

        // migrated schema is usable end-to-end
        db.Insert(new Model { Id = 1, Name = "x", Age = 5 });
        var back = db.Get<Model>(1);
        Assert.Equal("x", back.Name);
        Assert.Equal(5, back.Age);
    }

    [Fact]
    public void Commit_IndexAttribute_CreatesCompositeIndexInOrder()
    {
        using var db = ConnectionFactory.CreateInMemory();

        db.CreateTables().Add<Indexed>().Commit();

        // index exists
        Assert.Equal(1, db.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type='index' AND name='IX_Composite'"));
        // and covers both columns in the declared order (B before A -> ColumnOrder 0 then 1)
        var cols = db.Query<string>("SELECT name FROM pragma_index_info('IX_Composite') ORDER BY seqno").ToArray();
        Assert.Equal(new[] { "B", "A" }, cols);
    }

    [Fact]
    public void Commit_StrictTable_EmitsStrictKeyword()
    {
        using var db = ConnectionFactory.CreateInMemory();

        db.CreateTables().Add<StrictModel>().Commit();

        var sql = db.ExecuteScalar<string>(
            "SELECT sql FROM sqlite_master WHERE type='table' AND name='StrictModel'");
        Assert.Contains("STRICT", sql);
    }

    [Fact]
    public void Commit_StringDefaultWithApostrophe_IsEscaped()
    {
        using var db = ConnectionFactory.CreateInMemory();

        // would produce  DEFAULT 'O'Brien'  (syntax error) without escaping
        db.CreateTables().Add<Defaulted>().Commit();

        // default applies when the column is omitted
        db.Execute("INSERT INTO Defaulted (Id) VALUES (1)");
        var row = db.Get<Defaulted>(1);
        Assert.Equal("O'Brien", row.Name);
    }

    public class Model
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }

    public class Indexed
    {
        [PrimaryKey]
        public int Id { get; set; }
        [Index("IX_Composite", 1)]
        public string A { get; set; }
        [Index("IX_Composite", 0)]
        public string B { get; set; }
    }

    [StrictTable]
    public class StrictModel
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Defaulted
    {
        [PrimaryKey]
        public int Id { get; set; }
        [DefaultValue("O'Brien")]
        public string Name { get; set; }
    }
}
