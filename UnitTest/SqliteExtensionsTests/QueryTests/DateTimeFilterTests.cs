namespace UnitTest.SqliteExtensionsTests.QueryTests;

using Simple.Sqlite;
using Simple.Sqlite.Attributes;
using System;
using System.Linq;
using Xunit;

public class DateTimeFilterTests
{
    [DateTimePolicy(DateTimePolicyAttribute.Policies.EpochMs)]
    class EpochModel
    {
        public int Id { get; set; }
        public DateTime Moment { get; set; }
    }
    class PlainModel
    {
        public int Id { get; set; }
        public DateTime Moment { get; set; }
    }

    static ISqliteConnection NewDb<T>(params DateTime[] moments) where T : class, new()
    {
        var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<T>().Commit();
        int id = 1;
        foreach (var m in moments)
        {
            // Insert through the lib: the model type carries the policy
            var item = Activator.CreateInstance<T>();
            typeof(T).GetProperty("Id")!.SetValue(item, id);
            typeof(T).GetProperty("Moment")!.SetValue(item, m);
            db.Insert(item);
            id++;
        }
        return db;
    }

    [Fact]
    public void Query_EpochModel_DateTimeParam_BindsAsEpoch()
    {
        using var db = NewDb<EpochModel>(
            new DateTime(2021, 6, 15, 00, 00, 00, DateTimeKind.Utc),
            new DateTime(2021, 6, 16, 00, 00, 00, DateTimeKind.Utc),
            new DateTime(2021, 6, 17, 00, 00, 00, DateTimeKind.Utc));

        var from = new DateTime(2021, 6, 16, 00, 00, 00, DateTimeKind.Utc);
        var to   = new DateTime(2021, 6, 17, 00, 00, 00, DateTimeKind.Utc);

        // anonymous param with free names (from/to): the T context drives the epoch conversion
        var rows = db.Query<EpochModel>("SELECT * FROM EpochModel WHERE Moment >= @from AND Moment < @to", new { from, to });

        Assert.Single(rows);
        Assert.Equal(2, rows.First().Id);
        Assert.Equal(DateTimeKind.Utc, rows.First().Moment.Kind);
    }

    [Fact]
    public void Query_EpochModel_DateTimeParamOfAnyKind_BindsAsEpoch()
    {
        using var db = NewDb<EpochModel>(
            new DateTime(2021, 6, 15, 10, 00, 00, DateTimeKind.Utc),
            new DateTime(2021, 6, 15, 12, 00, 00, DateTimeKind.Utc));

        // Local param: converted to UTC before binding, same rule as the write path
        var local = new DateTime(2021, 6, 15, 9, 00, 00, DateTimeKind.Local); // 12:00Z on GMT-3
        var rows = db.Query<EpochModel>("SELECT * FROM EpochModel WHERE Moment = @at", new { at = local });

        Assert.Single(rows);
        Assert.Equal(2, rows.First().Id);
    }

    [Fact]
    public void GetWhere_EpochModel_DateTimeFilter_Matches()
    {
        var db = ConnectionFactory.CreateInMemory();
        db.CreateTables().Add<EpochModel>().Commit();
        db.Insert(new EpochModel { Id = 1, Moment = new DateTime(2021, 6, 15, 10, 20, 30, DateTimeKind.Utc) });
        db.Insert(new EpochModel { Id = 2, Moment = new DateTime(2021, 6, 16, 10, 20, 30, DateTimeKind.Utc) });

        var rows = db.GetWhere<EpochModel>("Moment", new DateTime(2021, 6, 15, 10, 20, 30, DateTimeKind.Utc)).ToArray();

        Assert.Single(rows);
        Assert.Equal(1, rows[0].Id);
        Assert.Equal(DateTimeKind.Utc, rows[0].Moment.Kind);
    }

    [Fact]
    public void Query_PlainModel_DateTimeParam_StillBindsAsText()
    {
        using var db = NewDb<PlainModel>(
            new DateTime(2021, 6, 15, 10, 20, 30, DateTimeKind.Utc));

        // no policy: TEXT-Z param against TEXT column, legacy behavior unchanged
        var rows = db.Query<PlainModel>("SELECT * FROM PlainModel WHERE Moment = @at", new { at = new DateTime(2021, 6, 15, 10, 20, 30, DateTimeKind.Utc) });

        Assert.Single(rows);
        Assert.Equal(1, rows.First().Id);
    }

    [Fact]
    public void Query_SimpleTypeParam_Unaffected()
    {
        using var db = NewDb<EpochModel>(
            new DateTime(2021, 6, 15, 10, 20, 30, DateTimeKind.Utc));

        // T is a simple type: no epoch context, scalar queries keep working
        var count = db.Query<long>("SELECT COUNT(*) FROM EpochModel WHERE Moment > @m", new { m = 0L }).Single();

        Assert.Equal(1, count);
    }
}
