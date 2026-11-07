namespace UnitTest.SqliteExtensionsTests;

using Simple.Sqlite;
using System;
using System.IO;
using Xunit;

public class FromImmutableFileTests : IDisposable
{
    private readonly string dbFile = $"demoDb_{Guid.NewGuid()}.db";

    public void Dispose()
    {
        // Microsoft.Data.Sqlite pools connections, keeping the file handle open
        ConnectionFactory.ClearAllPools();
        if (File.Exists(dbFile)) File.Delete(dbFile);
    }

    private void Seed()
    {
        using var seed = ConnectionFactory.CreateConnection(dbFile);
        seed.Execute("CREATE TABLE t (x INTEGER)");
        seed.Execute("INSERT INTO t VALUES (42)");
    }

    [Fact]
    public void FromImmutableFile_ReadsExisting()
    {
        Seed();

        var factory = ConnectionFactory.FromImmutableFile(dbFile);
        using var cnn = factory.GetConnection();

        Assert.Equal(42L, cnn.ExecuteScalar<long>("SELECT x FROM t"));
    }

    [Fact]
    public void FromImmutableFile_BlocksWrites()
    {
        Seed();

        var factory = ConnectionFactory.FromImmutableFile(dbFile);
        using var cnn = factory.GetConnection();

        Assert.Throws<Microsoft.Data.Sqlite.SqliteException>(
            () => cnn.Execute("INSERT INTO t VALUES (1)"));
    }

    [Fact]
    public void FromImmutableFile_OpensReadOnlyFile()
    {
        Seed();
        File.SetAttributes(dbFile, FileAttributes.ReadOnly);

        try
        {
            var factory = ConnectionFactory.FromImmutableFile(dbFile);
            using var cnn = factory.GetConnection();

            Assert.Equal(42L, cnn.ExecuteScalar<long>("SELECT x FROM t"));
        }
        finally
        {
            File.SetAttributes(dbFile, FileAttributes.Normal);
        }
    }

    [Fact]
    public void FromImmutableFile_PathWithSpaces_ReadsExisting()
    {
        var spaced = Path.Combine(Path.GetDirectoryName(dbFile)!, "demo db " + Guid.NewGuid() + ".db");
        try
        {
            using (var seed = ConnectionFactory.CreateConnection(spaced))
            {
                seed.Execute("CREATE TABLE t (x INTEGER)");
                seed.Execute("INSERT INTO t VALUES (42)");
            }

            var factory = ConnectionFactory.FromImmutableFile(spaced);
            using var cnn = factory.GetConnection();

            Assert.Equal(42L, cnn.ExecuteScalar<long>("SELECT x FROM t"));
        }
        finally
        {
            ConnectionFactory.ClearAllPools();
            if (File.Exists(spaced)) File.Delete(spaced);
        }
    }
}
