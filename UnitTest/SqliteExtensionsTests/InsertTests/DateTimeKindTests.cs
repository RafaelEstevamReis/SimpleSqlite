namespace UnitTest.SqliteExtensionsTests.InsertTests;

using Simple.Sqlite;
using System;
using Xunit;

public class DateTimeKindTests
{
    class Dated
    {
        public int Id { get; set; }
        public DateTime Moment { get; set; }
        public DateTime? Nullable { get; set; }
    }

    [Fact]
    public void Insert_UtcDateTime_IsStoredWithZSuffix()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<Dated>().Commit();

        db.Insert(new Dated { Id = 1, Moment = new DateTime(2021, 6, 15, 10, 20, 30, DateTimeKind.Utc) });

        var stored = db.ExecuteScalar<string>("SELECT CAST(Moment AS TEXT) FROM Dated");
        Assert.Equal("2021-06-15 10:20:30Z", stored);
    }

    [Fact]
    public void Insert_LocalDateTime_IsConvertedToUtcAndStoredWithZ()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<Dated>().Commit();

        var local = new DateTime(2021, 6, 15, 10, 20, 30, DateTimeKind.Local);
        db.Insert(new Dated { Id = 1, Moment = local });

        var stored = db.ExecuteScalar<string>("SELECT CAST(Moment AS TEXT) FROM Dated");
        var expected = local.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss'Z'");
        Assert.Equal(expected, stored);
    }

    [Fact]
    public void Insert_UnspecifiedDateTime_IsStoredWithoutSuffix()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<Dated>().Commit();

        db.Insert(new Dated { Id = 1, Moment = new DateTime(2021, 6, 15, 10, 20, 30) });

        var stored = db.ExecuteScalar<string>("SELECT CAST(Moment AS TEXT) FROM Dated");
        Assert.Equal("2021-06-15 10:20:30", stored);
    }

    [Fact]
    public void Insert_UtcDateTime_RoundTripsWithUtcKind()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<Dated>().Commit();

        var utc = new DateTime(2021, 6, 15, 10, 20, 30, DateTimeKind.Utc);
        db.Insert(new Dated { Id = 1, Moment = utc, Nullable = utc });

        var back = db.Get<Dated>(1);

        Assert.Equal(utc, back.Moment);
        Assert.Equal(DateTimeKind.Utc, back.Moment.Kind);
        Assert.Equal(utc, back.Nullable);
        Assert.Equal(DateTimeKind.Utc, back.Nullable!.Value.Kind);
    }

    [Fact]
    public void Insert_LocalDateTime_RoundTripsAsUtcInstant()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<Dated>().Commit();

        var local = new DateTime(2021, 6, 15, 10, 20, 30, DateTimeKind.Local);
        db.Insert(new Dated { Id = 1, Moment = local });

        var back = db.Get<Dated>(1);

        Assert.Equal(local.ToUniversalTime(), back.Moment);
        Assert.Equal(DateTimeKind.Utc, back.Moment.Kind);
    }

    [Fact]
    public void Insert_UnspecifiedDateTime_RoundTripsWithUnspecifiedKind()
    {
        using var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<Dated>().Commit();

        var unspec = new DateTime(2021, 6, 15, 10, 20, 30);
        db.Insert(new Dated { Id = 1, Moment = unspec });

        var back = db.Get<Dated>(1);

        Assert.Equal(unspec, back.Moment);
        Assert.Equal(DateTimeKind.Unspecified, back.Moment.Kind);
    }
}
