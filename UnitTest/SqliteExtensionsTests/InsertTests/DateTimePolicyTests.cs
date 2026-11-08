namespace UnitTest.SqliteExtensionsTests.InsertTests;

using Simple.Sqlite;
using Simple.Sqlite.Attributes;
using System;
using Xunit;

public class DateTimePolicyTests
{
    [DateTimePolicy(DateTimePolicyAttribute.Policies.EpochMs)]
    class EpochModel
    {
        public int Id { get; set; }
        public DateTime Moment { get; set; }
        public DateTime? Nullable { get; set; }
    }
    class PlainModel
    {
        public int Id { get; set; }
        public DateTime Moment { get; set; }
    }
    [StrictTable]
    [DateTimePolicy(DateTimePolicyAttribute.Policies.EpochMs)]
    class StrictEpoch
    {
        public int Id { get; set; }
        public DateTime Moment { get; set; }
    }

    [Fact]
    public void EpochMs_UtcDateTime_IsStoredAsInteger()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<EpochModel>().Commit();

        var utc = new DateTime(2021, 6, 15, 10, 20, 30, 123, DateTimeKind.Utc);
        db.Insert(new EpochModel { Id = 1, Moment = utc });

        Assert.Equal("integer", db.ExecuteScalar<string>("SELECT typeof(Moment) FROM EpochModel"));
        var expected = new DateTimeOffset(utc).ToUnixTimeMilliseconds();
        Assert.Equal(expected, db.ExecuteScalar<long>("SELECT Moment FROM EpochModel"));
    }

    [Fact]
    public void EpochMs_LocalDateTime_IsConvertedToUtcBeforeStoring()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<EpochModel>().Commit();

        var local = new DateTime(2021, 6, 15, 10, 20, 30, DateTimeKind.Local);
        db.Insert(new EpochModel { Id = 1, Moment = local });

        var expected = new DateTimeOffset(local).ToUnixTimeMilliseconds();
        Assert.Equal(expected, db.ExecuteScalar<long>("SELECT Moment FROM EpochModel"));
    }

    [Fact]
    public void EpochMs_UnspecifiedDateTime_IsStoredAsUtcWall()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<EpochModel>().Commit();

        var unspec = new DateTime(2021, 6, 15, 10, 20, 30);
        db.Insert(new EpochModel { Id = 1, Moment = unspec });

        var expected = new DateTimeOffset(DateTime.SpecifyKind(unspec, DateTimeKind.Utc)).ToUnixTimeMilliseconds();
        Assert.Equal(expected, db.ExecuteScalar<long>("SELECT Moment FROM EpochModel"));
    }

    [Fact]
    public void EpochMs_NullableNull_IsStoredAsNull()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<EpochModel>().Commit();

        db.Insert(new EpochModel { Id = 1, Moment = DateTime.UtcNow, Nullable = null });

        Assert.Equal("null", db.ExecuteScalar<string>("SELECT typeof(Nullable) FROM EpochModel"));
        Assert.Null(db.Get<EpochModel>(1).Nullable);
    }

    [Fact]
    public void EpochMs_RoundTripsWithUtcKind()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<EpochModel>().Commit();

        var utc = new DateTime(2021, 6, 15, 10, 20, 30, 123, DateTimeKind.Utc);
        db.Insert(new EpochModel { Id = 1, Moment = utc, Nullable = utc });

        var back = db.Get<EpochModel>(1);

        Assert.Equal(utc, back.Moment);
        Assert.Equal(DateTimeKind.Utc, back.Moment.Kind);
        Assert.Equal(utc, back.Nullable);
        Assert.Equal(DateTimeKind.Utc, back.Nullable!.Value.Kind);
    }

    [Fact]
    public void EpochMs_ColumnIsTypedInteger()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<EpochModel>().Commit();

        var ddl = db.ExecuteScalar<string>("SELECT sql FROM sqlite_master WHERE name='EpochModel'");

        Assert.Contains("\"Moment\" INTEGER", ddl);
    }

    [Fact]
    public void EpochMs_WorksOnStrictTables()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<StrictEpoch>().Commit();

        var utc = new DateTime(2021, 6, 15, 10, 20, 30, DateTimeKind.Utc);
        db.Insert(new StrictEpoch { Id = 1, Moment = utc });

        var back = db.Get<StrictEpoch>(1);
        Assert.Equal(utc, back.Moment);
        Assert.Equal(DateTimeKind.Utc, back.Moment.Kind);
    }

    [Fact]
    public void EpochMs_MixedLegacyText_ReadsBothFormats()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.Execute("CREATE TABLE EpochModel (Id INTEGER PRIMARY KEY, Moment INTEGER, Nullable INTEGER)");
        db.Execute("INSERT INTO EpochModel VALUES (1, 1767323045000, NULL)");                    // epoch
        db.Execute("INSERT INTO EpochModel VALUES (2, '2021-06-15 10:20:30Z', NULL)");          // legacy TEXT

        var epochBack = db.Get<EpochModel>(1).Moment;
        var textBack = db.Get<EpochModel>(2).Moment;

        Assert.Equal(DateTimeKind.Utc, epochBack.Kind);
        Assert.Equal(new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc), epochBack);
        Assert.Equal(new DateTime(2021, 6, 15, 10, 20, 30, DateTimeKind.Utc), textBack);
    }

    [Fact]
    public void NoPolicy_StillWritesTextWithZ()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<PlainModel>().Commit();

        var utc = new DateTime(2021, 6, 15, 10, 20, 30, DateTimeKind.Utc);
        db.Insert(new PlainModel { Id = 1, Moment = utc });

        Assert.Equal("2021-06-15 10:20:30Z", db.ExecuteScalar<string>("SELECT CAST(Moment AS TEXT) FROM PlainModel"));
    }

    [Fact]
    public void ExecuteScalar_DateTime_OverEpochInteger()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.Execute("CREATE TABLE t (epoch INTEGER)");
        var utc = new DateTime(2021, 6, 15, 10, 20, 30, DateTimeKind.Utc);
        db.Execute("INSERT INTO t VALUES (@ms)", new { ms = new DateTimeOffset(utc).ToUnixTimeMilliseconds() });

        var back = db.ExecuteScalar<DateTime>("SELECT epoch FROM t");
        var max = db.ExecuteScalar<DateTime>("SELECT MAX(epoch) FROM t");

        Assert.Equal(utc, back);
        Assert.Equal(DateTimeKind.Utc, back.Kind);
        Assert.Equal(utc, max);
    }

    [Fact]
    public void ExecuteScalar_DateTime_OverTextStillParses()
    {
        using var db = ConnectionFactory.CreateInMemory();

        var back = db.ExecuteScalar<DateTime>("SELECT '2021-06-15 10:20:30Z'");

        Assert.Equal(new DateTime(2021, 6, 15, 10, 20, 30, DateTimeKind.Utc), back);
        Assert.Equal(DateTimeKind.Utc, back.Kind);
    }

    [Fact]
    public void BulkInsert_EpochMs_StoresAllAsIntegerAndRoundTrips()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<EpochModel>().Commit();

        var items = new[]
        {
            new EpochModel { Id = 1, Moment = new DateTime(2021, 6, 15, 10, 20, 30, 123, DateTimeKind.Utc) },
            new EpochModel { Id = 2, Moment = new DateTime(2021, 6, 15, 11, 20, 30, DateTimeKind.Local) },
            new EpochModel { Id = 3, Moment = new DateTime(2021, 6, 15, 12, 20, 30) },          // Unspecified
            new EpochModel { Id = 4, Moment = DateTime.UtcNow, Nullable = DateTime.UtcNow },
        };

        db.BulkInsert(items);

        // all INTEGER storage
        Assert.Equal("integer", db.ExecuteScalar<string>("SELECT typeof(Moment) FROM EpochModel WHERE Id = 1"));
        // Local converted to UTC before storing
        Assert.Equal(new DateTimeOffset(items[1].Moment).ToUnixTimeMilliseconds(),
                     db.ExecuteScalar<long>("SELECT Moment FROM EpochModel WHERE Id = 2"));
        // Unspecified as UTC wall
        var wall = DateTime.SpecifyKind(items[2].Moment, DateTimeKind.Utc);
        Assert.Equal(new DateTimeOffset(wall).ToUnixTimeMilliseconds(),
                     db.ExecuteScalar<long>("SELECT Moment FROM EpochModel WHERE Id = 3"));
        // nullable epoch
        Assert.Equal("integer", db.ExecuteScalar<string>("SELECT typeof(Nullable) FROM EpochModel WHERE Id = 4"));

        // full read-back with Kind
        var back = db.Get<EpochModel>(1);
        Assert.Equal(items[0].Moment, back.Moment);
        Assert.Equal(DateTimeKind.Utc, back.Moment.Kind);
        Assert.Equal(DateTimeKind.Utc, db.Get<EpochModel>(4).Nullable!.Value.Kind);
    }
}
