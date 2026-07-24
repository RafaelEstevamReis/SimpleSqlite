namespace UnitTest.SqliteExtensionsTests.ManagementTests;

using Simple.DatabaseWrapper.Attributes;
using Simple.Sqlite;
using System;
using System.IO;
using System.Linq;
using Xunit;

public class BackupAndManagementTests
{
    [Fact]
    public void IntegrityCheck_HealthyDb_ReturnsOk()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<Row>().Commit();

        Assert.Equal(new[] { "ok" }, db.IntegrityCheck());
        Assert.Equal(new[] { "ok" }, db.IntegrityCheckQuick());
    }

    [Fact]
    public void Vacuum_And_Optimize_PreserveData()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<Row>().Commit();
        db.Insert(new Row { Id = 1, Name = "a" });

        db.Vacuum();
        db.Optimize();

        Assert.Single(db.GetAll<Row>());
    }

    [Fact]
    public void SetJournalMode_And_Synchronous_ApplyOnFileDb()
    {
        var dir = Path.Combine(Path.GetTempPath(), "sqlt_" + Guid.NewGuid().ToString("N"));
        var file = Path.Combine(dir, "j.db");
        try
        {
            using (var db = ConnectionFactory.FromFile(file).GetConnection())
            {
                db.CreateTables().Add<Row>().Commit();

                db.SetJournalMode(JournalMode.WAL);
                Assert.Equal("wal", db.ExecuteScalar<string>("PRAGMA journal_mode"));

                db.SetSchemaSynchronous(SynchronousMode.NORMAL);
                Assert.Equal(1, db.ExecuteScalar<int>("PRAGMA synchronous"));
            }
        }
        finally
        {
            ConnectionFactory.ClearAllPools();
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void VacuumIntoFile_WithApostropheInPath_CreatesFile()
    {
        var dir = Path.Combine(Path.GetTempPath(), "sqlt_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var target = Path.Combine(dir, "o'brien.db"); // apostrophe must be escaped
        try
        {
            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<Row>().Commit();
            db.Insert(new Row { Id = 1, Name = "a" });

            db.VacuumIntoFile(target);

            Assert.True(File.Exists(target));
        }
        finally
        {
            ConnectionFactory.ClearAllPools();
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void BackupDatabase_CopiesData_AndCreatesMissingDirectory()
    {
        var dir = Path.Combine(Path.GetTempPath(), "sqlt_" + Guid.NewGuid().ToString("N"));
        var backup = Path.Combine(dir, "nested", "backup.db"); // nested dir does not exist yet
        try
        {
            using (var db = ConnectionFactory.CreateInMemory())
            {
                db.CreateTables().Add<Row>().Commit();
                db.Insert(new Row { Id = 1, Name = "a" });
                db.Insert(new Row { Id = 2, Name = "b" });

                db.BackupDatabase(backup);
            }

            Assert.True(File.Exists(backup));

            using (var restored = ConnectionFactory.FromFile(backup, readOnly: true).GetConnection())
            {
                Assert.Equal(2, restored.GetAll<Row>().Count());
            }
        }
        finally
        {
            ConnectionFactory.ClearAllPools();
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    public class Row
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
