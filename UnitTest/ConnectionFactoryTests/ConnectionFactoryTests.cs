namespace UnitTest.ConnectionFactoryTests;

using Microsoft.Data.Sqlite;
using Simple.DatabaseWrapper.Attributes;
using Simple.Sqlite;
using System;
using System.IO;
using System.Linq;
using Xunit;

public class ConnectionFactoryTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Ctor_NullOrWhitespaceConnectionString_Throws(string cnn)
    {
        Assert.Throws<ArgumentException>(() => new ConnectionFactory(cnn));
    }

    [Fact]
    public void Ctor_FromBuilder_SetsConnectionString()
    {
        var builder = new SqliteConnectionStringBuilder { DataSource = ":memory:" };

        var factory = new ConnectionFactory(builder);

        Assert.Equal(builder.ToString(), factory.ConnectionString);
    }

    [Fact]
    public void GetConnection_ReturnsOpenUsableConnection()
    {
        var factory = new ConnectionFactory(new SqliteConnectionStringBuilder { DataSource = ":memory:" });

        using var db = factory.GetConnection();

        Assert.Equal(1, db.ExecuteScalar<int>("SELECT 1"));
    }

    [Fact]
    public void CreateInMemory_IsUsable()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<CfModel>().Commit();

        db.Insert(new CfModel { Id = 1, Name = "a" });

        Assert.Single(db.GetAll<CfModel>());
    }

    [Fact]
    public void CreateInMemory_ConnectionsAreIsolated()
    {
        using var db1 = ConnectionFactory.CreateInMemory();
        using var db2 = ConnectionFactory.CreateInMemory();
        db1.CreateTables().Add<CfModel>().Commit();
        db1.Insert(new CfModel { Id = 1, Name = "a" });

        // db2 is a separate :memory: database -> table does not exist there
        Assert.Throws<SqliteException>(() => db2.GetAll<CfModel>().ToArray());
    }

    [Fact]
    public void CreateInMemoryShared_SharesDataAcrossConnections()
    {
        // keep db1 open so the shared in-memory db stays alive
        using var db1 = ConnectionFactory.CreateInMemoryShared("shared_plain");
        db1.CreateTables().Add<CfModel>().Commit();
        db1.Insert(new CfModel { Id = 1, Name = "a" });

        using var db2 = ConnectionFactory.CreateInMemoryShared("shared_plain");

        Assert.Single(db2.GetAll<CfModel>());
    }

    [Fact]
    public void CreateInMemoryShared_NameWithSpecialChars_Works()
    {
        // ';', space and '=' would break a naively-interpolated connection string
        const string name = "shared name; x=1";

        using var db1 = ConnectionFactory.CreateInMemoryShared(name);
        db1.CreateTables().Add<CfModel>().Commit();
        db1.Insert(new CfModel { Id = 7, Name = "z" });

        using var db2 = ConnectionFactory.CreateInMemoryShared(name);

        Assert.Equal("z", db2.GetAll<CfModel>().Single().Name);
    }

    [Fact]
    public void FromFile_CreatesMissingDirectoryAndFile_RoundTrips()
    {
        var dir = Path.Combine(Path.GetTempPath(), "sqlt_" + Guid.NewGuid().ToString("N"));
        var file = Path.Combine(dir, "nested", "test.db");
        try
        {
            var factory = ConnectionFactory.FromFile(file);
            using (var db = factory.GetConnection())
            {
                db.CreateTables().Add<CfModel>().Commit();
                db.Insert(new CfModel { Id = 1, Name = "a" });
            }

            Assert.True(File.Exists(file));

            using (var db = factory.GetConnection())
            {
                Assert.Single(db.GetAll<CfModel>());
            }
        }
        finally
        {
            ConnectionFactory.ClearAllPools();
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    [Fact]
    public void FromFile_ReadOnly_CannotWrite()
    {
        var dir = Path.Combine(Path.GetTempPath(), "sqlt_" + Guid.NewGuid().ToString("N"));
        var file = Path.Combine(dir, "test.db");
        try
        {
            using (var db = ConnectionFactory.FromFile(file).GetConnection())
            {
                db.CreateTables().Add<CfModel>().Commit();
                db.Insert(new CfModel { Id = 1, Name = "a" });
            }
            ConnectionFactory.ClearAllPools();

            using (var ro = ConnectionFactory.FromFile(file, readOnly: true).GetConnection())
            {
                Assert.Single(ro.GetAll<CfModel>());
                Assert.Throws<SqliteException>(() => ro.Insert(new CfModel { Id = 2, Name = "b" }));
            }
        }
        finally
        {
            ConnectionFactory.ClearAllPools();
            if (Directory.Exists(dir)) Directory.Delete(dir, true);
        }
    }

    public class CfModel
    {
        [PrimaryKey]
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
