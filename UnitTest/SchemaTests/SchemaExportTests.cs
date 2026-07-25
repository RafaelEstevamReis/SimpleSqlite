namespace UnitTest.SchemaTests;

using Simple.DatabaseWrapper.Attributes;
using Simple.DatabaseWrapper.TypeReader;
using Simple.Sqlite;
using Simple.Sqlite.Attributes;
using Xunit;

// Direct unit tests for the Schema classes (Table/Column), no database needed.
public class SchemaExportTests
{
    private static Table TableOf<T>() => Table.FromType(TypeInfo.FromType(typeof(T)), typeof(T));
    private static Column Col(Table t, string name) => (Column)t[name];

    [Fact]
    public void ExportCreateTable_QuotesIdentifiers_AndMapsPrimaryKey()
    {
        var sql = TableOf<Model>().ExportCreateTable();

        Assert.Contains("CREATE TABLE IF NOT EXISTS \"Model\"", sql);
        Assert.Contains("\"Id\" INTEGER", sql);
        Assert.Contains("PRIMARY KEY", sql);
        Assert.Contains("AUTOINCREMENT", sql);
        Assert.Contains("\"Name\" TEXT", sql);
    }

    [Fact]
    public void ExportCreateTable_Strict_AppendsStrictKeyword()
    {
        Assert.Contains("STRICT", TableOf<StrictModel>().ExportCreateTable());
    }

    [Fact]
    public void ExportCreateTable_EmptyTableName_Throws()
    {
        var t = new Table { TableName = "", Columns = TableOf<Model>().Columns };
        Assert.ThrowsAny<System.ArgumentException>(() => t.ExportCreateTable());
    }

    [Fact]
    public void ExportCreateTable_NoColumns_Throws()
    {
        var t = new Table { TableName = "X", Columns = [] };
        Assert.ThrowsAny<System.ArgumentException>(() => t.ExportCreateTable());
    }

    [Fact]
    public void FromType_IgnoreAttribute_ExcludesColumn()
    {
        var t = TableOf<WithIgnored>();

        Assert.Contains(t.Columns, c => c.ColumnName == "Kept");
        Assert.DoesNotContain(t.Columns, c => c.ColumnName == "Skipped");
    }

    [Theory]
    [InlineData("AnInt", SqliteType.INTEGER)]
    [InlineData("AText", SqliteType.TEXT)]
    [InlineData("AReal", SqliteType.REAL)]
    [InlineData("ABool", SqliteType.INTEGER)]
    [InlineData("AGuid", SqliteType.BLOB)]
    [InlineData("ABlob", SqliteType.BLOB)]
    public void FromType_MapsTypeAffinity(string column, SqliteType expected)
    {
        Assert.Equal(expected, Col(TableOf<Affinities>(), column).SqliteType);
    }

    [Fact]
    public void ColumnDefinition_UniqueAndDefault_AreEmitted()
    {
        var t = TableOf<Constrained>();

        Assert.Contains("UNIQUE", Col(t, "Code").ExportColumnDefinitionAsStatement());
        Assert.Contains("DEFAULT 5", Col(t, "Amount").ExportColumnDefinitionAsStatement());
    }

    [Fact]
    public void AddColumn_NotNullInt_GetsReasonableDefaultZero()
    {
        var stmt = Col(TableOf<Model>(), "Age").ExportAddColumnAsStatement();

        Assert.Contains("ADD COLUMN", stmt);
        Assert.Contains("NOT NULL", stmt);
        Assert.Contains("DEFAULT 0", stmt);
    }

    [Fact]
    public void AddColumn_NotNullText_GetsEmptyStringDefault()
    {
        var stmt = Col(TableOf<NotNullText>(), "Name").ExportAddColumnAsStatement();

        Assert.Contains("NOT NULL", stmt);
        Assert.Contains("DEFAULT ''", stmt);
    }

    [Fact]
    public void Commit_ExistingTableDifferentCase_StillMigrates()
    {
        using var db = ConnectionFactory.CreateInMemory();
        // table created with lowercase name; model resolves to "Model"
        db.Execute("CREATE TABLE model (Id INTEGER PRIMARY KEY)");

        var results = db.CreateTables().Add<Model>().Commit();

        var r = Assert.Single(results);
        Assert.False(r.WasTableCreated);          // detected as existing despite case
        Assert.Contains("Name", r.ColumnsAdded);  // migration actually ran
    }

    public class Model
    {
        [PrimaryKey] public int Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }

    [StrictTable]
    public class StrictModel
    {
        [PrimaryKey] public int Id { get; set; }
        public string Name { get; set; }
    }

    public class WithIgnored
    {
        [PrimaryKey] public int Kept { get; set; }
        [Ignore] public string Skipped { get; set; }
    }

    public class Affinities
    {
        [PrimaryKey] public int Id { get; set; }
        public int AnInt { get; set; }
        public string AText { get; set; }
        public double AReal { get; set; }
        public bool ABool { get; set; }
        public System.Guid AGuid { get; set; }
        public byte[] ABlob { get; set; }
    }

    public class Constrained
    {
        [PrimaryKey] public int Id { get; set; }
        [Unique] public string Code { get; set; }
        [DefaultValue(5)] public int Amount { get; set; }
    }

    public class NotNullText
    {
        [PrimaryKey] public int Id { get; set; }
        [NotNull] public string Name { get; set; }
    }
}
