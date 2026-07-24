namespace UnitTest.SqliteExtensionsTests.CsvTests;

using Simple.DatabaseWrapper.Attributes;
using Simple.Sqlite;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Xunit;

public class CsvIngestionTests
{
    private static ISqliteConnection NewDb()
    {
        var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<CsvRow>().Commit();
        return db;
    }

    private static CsvRow Map(string[] c) => new CsvRow { Id = int.Parse(c[0]), Name = c[1] };

    [Fact]
    public void LoadFromCsvStream_Stream_IngestsAllRows()
    {
        using var db = NewDb();
        var bytes = Encoding.UTF8.GetBytes("1;alpha\n2;beta\n3;gamma");

        db.LoadFromCsvStream(new MemoryStream(bytes), Encoding.UTF8, Map);

        Assert.Equal(3, db.GetAll<CsvRow>().Count());
        Assert.Equal("beta", db.Get<CsvRow>(2).Name);
    }

    [Fact]
    public void LoadFromCsvStream_StreamReader_IngestsAllRows()
    {
        using var db = NewDb();
        using var sr = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes("1;a\n2;b")));

        db.LoadFromCsvStream(sr, Map);

        Assert.Equal(2, db.GetAll<CsvRow>().Count());
    }

    [Fact]
    public void LoadFromCsvStream_CustomDelimiter_IsRespected()
    {
        using var db = NewDb();
        var bytes = Encoding.UTF8.GetBytes("1,alpha\n2,beta");

        db.LoadFromCsvStream(new MemoryStream(bytes), Encoding.UTF8, Map, delimiter: ',');

        Assert.Equal("alpha", db.Get<CsvRow>(1).Name);
    }

    [Fact]
    public void LoadFromCsvStream_SmallBuffer_FlushesAllBatches()
    {
        using var db = NewDb();
        var csv = string.Join("\n", Enumerable.Range(1, 5).Select(i => $"{i};n{i}"));
        var bytes = Encoding.UTF8.GetBytes(csv);

        db.LoadFromCsvStream(new MemoryStream(bytes), Encoding.UTF8, Map, bufferSize: 2);

        Assert.Equal(5, db.GetAll<CsvRow>().Count());
    }

    [Fact]
    public void LoadFromCsvFile_DefaultIgnore_KeepsExistingOnConflict()
    {
        var file = Path.Combine(Path.GetTempPath(), "csv_" + Guid.NewGuid().ToString("N") + ".csv");
        File.WriteAllText(file, "1;fromcsv\n2;two");
        try
        {
            using var db = NewDb();
            db.Insert(new CsvRow { Id = 1, Name = "orig" });

            db.LoadFromCsvFile(file, Map); // default OnConflict.Ignore

            Assert.Equal(2, db.GetAll<CsvRow>().Count());
            Assert.Equal("orig", db.Get<CsvRow>(1).Name);
        }
        finally { File.Delete(file); }
    }

    [Fact]
    public void LoadFromCsvFile_Replace_OverwritesOnConflict()
    {
        var file = Path.Combine(Path.GetTempPath(), "csv_" + Guid.NewGuid().ToString("N") + ".csv");
        File.WriteAllText(file, "1;fromcsv");
        try
        {
            using var db = NewDb();
            db.Insert(new CsvRow { Id = 1, Name = "orig" });

            db.LoadFromCsvFile(file, Map, conflictResolution: OnConflict.Replace);

            Assert.Equal("fromcsv", db.Get<CsvRow>(1).Name);
        }
        finally { File.Delete(file); }
    }

    [Fact]
    public void LoadFromCsvZippedFile_Typed_FiltersEntries()
    {
        var zip = CreateZip(("data.csv", "1;a\n2;b"), ("skip.txt", "9;z"));
        try
        {
            using var db = NewDb();

            db.LoadFromCsvZippedFile<CsvRow>(zip, name => name.EndsWith(".csv"), Map);

            Assert.Equal(2, db.GetAll<CsvRow>().Count());
            Assert.Null(db.Get<CsvRow>(9)); // skip.txt was filtered out
        }
        finally { File.Delete(zip); }
    }

    [Fact]
    public void LoadFromCsvZippedFile_Raw_IngestsRows()
    {
        var zip = CreateZip(("data.csv", "1;a\n2;b"));
        try
        {
            using var db = NewDb();

            db.LoadFromCsvZippedFile(zip, name => name.EndsWith(".csv"), "CsvRow",
                ["Id", "Name"], (r, i) => r.GetString(i));

            Assert.Equal(2, db.ExecuteScalar<int>("SELECT COUNT(*) FROM CsvRow"));
        }
        finally { File.Delete(zip); }
    }

    [Fact]
    public void LoadFromCsvZippedFile_Raw_ColumnMismatch_Throws()
    {
        var zip = CreateZip(("data.csv", "1;a;extra")); // 3 fields vs 2 columns
        try
        {
            using var db = NewDb();

            Assert.Throws<InvalidOperationException>(() =>
                db.LoadFromCsvZippedFile(zip, name => name.EndsWith(".csv"), "CsvRow",
                    ["Id", "Name"], (r, i) => r.GetString(i)));
        }
        finally { File.Delete(zip); }
    }

    private static string CreateZip(params (string name, string content)[] entries)
    {
        var path = Path.Combine(Path.GetTempPath(), "csv_" + Guid.NewGuid().ToString("N") + ".zip");
        using var fs = File.Create(path);
        using var zip = new ZipArchive(fs, ZipArchiveMode.Create);
        foreach (var (name, content) in entries)
        {
            var entry = zip.CreateEntry(name);
            using var w = new StreamWriter(entry.Open());
            w.Write(content);
        }
        return path;
    }

    public class CsvRow
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
