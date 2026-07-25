using Microsoft.Data.Sqlite;
using Simple.DatabaseWrapper.Attributes;
using Simple.Sqlite;
using System;
using System.Linq;
using Xunit;

namespace UnitTest.SqliteExtensionsTests.InsertTests
{
    public class BulkInsertTests
    {
        private static ISqliteConnection NewDb()
        {
            var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<Row>().Commit();
            return db;
        }

        [Fact]
        public void BulkInsert_AutoIncrement_ReturnsSequentialIds()
        {
            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<Auto>().Commit();

            var ids = db.BulkInsert(new[]
            {
                new Auto { Name = "a" },
                new Auto { Name = "b" },
                new Auto { Name = "c" },
            });

            Assert.Equal(new long[] { 1, 2, 3 }, ids);
            Assert.Equal(3, db.GetAll<Auto>().Count());
        }

        [Fact]
        public void BulkInsert_Empty_ReturnsEmpty_NoThrow()
        {
            using var db = NewDb();

            var ids = db.BulkInsert(Array.Empty<Row>());

            Assert.Empty(ids);
            Assert.Empty(db.GetAll<Row>());
        }

        [Fact]
        public void BulkInsert_AbortConflict_RollsBackWholeBatch()
        {
            using var db = NewDb();
            db.Insert(new Row { Id = 2, Name = "existing" }); // committed separately

            // second item collides -> whole bulk transaction must roll back
            Assert.Throws<SqliteException>(() => db.BulkInsert(new[]
            {
                new Row { Id = 1, Name = "a" },
                new Row { Id = 2, Name = "b" },
                new Row { Id = 3, Name = "c" },
            }));

            Assert.Single(db.GetAll<Row>());       // only the pre-existing row remains
            Assert.Null(db.Get<Row>(1));           // row 1 was rolled back
            Assert.Equal("existing", db.Get<Row>(2).Name);
        }

        [Fact]
        public void BulkInsert_Ignore_SkipsConflictsKeepsRest()
        {
            using var db = NewDb();
            db.Insert(new Row { Id = 2, Name = "orig" });

            db.BulkInsert(new[]
            {
                new Row { Id = 1, Name = "a" },
                new Row { Id = 2, Name = "b" },
                new Row { Id = 3, Name = "c" },
            }, OnConflict.Ignore);

            Assert.Equal(3, db.GetAll<Row>().Count());
            Assert.Equal("orig", db.Get<Row>(2).Name); // conflict ignored
        }

        [Fact]
        public void BulkInsert_Replace_OverwritesConflicts()
        {
            using var db = NewDb();
            db.Insert(new Row { Id = 2, Name = "orig" });

            db.BulkInsert(new[] { new Row { Id = 2, Name = "new" } }, OnConflict.Replace);

            Assert.Equal("new", db.Get<Row>(2).Name);
        }

        [Fact]
        public void BulkInsertRaw_InsertsRows()
        {
            using var db = NewDb();

            var ids = db.BulkInsertRaw("Row", new[] { "Id", "Name" }, new[]
            {
                new object?[] { 1, "a" },
                new object?[] { 2, "b" },
            });

            Assert.Equal(2, ids.Length);
            Assert.Equal("b", db.Get<Row>(2).Name);
        }

        [Fact]
        public void BulkInsertRaw_ColumnMismatch_ThrowsAndRollsBack()
        {
            using var db = NewDb();

            Assert.Throws<InvalidOperationException>(() =>
                db.BulkInsertRaw("Row", new[] { "Id", "Name" }, new[]
                {
                    new object?[] { 1, "a" },
                    new object?[] { 2, "b", "extra" }, // mismatch on 2nd row
                }));

            Assert.Empty(db.GetAll<Row>()); // first row rolled back with the transaction
        }

        public class Row
        {
            [PrimaryKey]
            public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Auto
        {
            [PrimaryKey]
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
