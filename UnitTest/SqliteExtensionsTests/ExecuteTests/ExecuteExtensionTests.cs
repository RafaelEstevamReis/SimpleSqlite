using Simple.DatabaseWrapper.Attributes;
using Simple.Sqlite;
using System;
using Xunit;

namespace UnitTest.SqliteExtensionsTests.ExecuteTests
{
    public class ExecuteExtensionTests
    {
        private static ISqliteConnection Seed()
        {
            var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<Item>().Commit();
            db.Insert(new Item { Id = 1, Name = "x" });
            db.Insert(new Item { Id = 2, Name = "y" });
            return db;
        }

        // ---- Execute ----

        [Fact]
        public void Execute_Update_ReturnsAffectedRowCount()
        {
            using var db = Seed();

            int n = db.Execute("UPDATE Item SET Name = @n WHERE Id = @id", new { n = "z", id = 1 });

            Assert.Equal(1, n);
            Assert.Equal("z", db.Get<Item>(1).Name);
        }

        [Fact]
        public void Execute_Delete_ReturnsAffectedRowCount()
        {
            using var db = Seed();

            int n = db.Execute("DELETE FROM Item");

            Assert.Equal(2, n);
            Assert.Empty(db.GetAll<Item>());
        }

        [Fact]
        public void Execute_Select_ReturnsMinusOne()
        {
            using var db = Seed();

            Assert.Equal(-1, db.Execute("SELECT 1"));
        }

        [Fact]
        public void Execute_InTransaction_Rollback_IsNotPersisted()
        {
            using var db = Seed();

            using (var trn = db.BeginTransaction())
            {
                trn.Execute("INSERT INTO Item (Id, Name) VALUES (10, 'a')");
                trn.Rollback();
            }

            Assert.Null(db.Get<Item>(10));
        }

        [Fact]
        public void Execute_InTransaction_Commit_IsPersisted()
        {
            using var db = Seed();

            using (var trn = db.BeginTransaction())
            {
                trn.Execute("INSERT INTO Item (Id, Name) VALUES (10, 'a')");
                trn.Commit();
            }

            Assert.NotNull(db.Get<Item>(10));
        }

        // ---- ExecuteScalar ----

        [Fact]
        public void ExecuteScalar_Count_ReturnsValue()
        {
            using var db = Seed();
            Assert.Equal(2, db.ExecuteScalar<int>("SELECT COUNT(*) FROM Item"));
        }

        [Fact]
        public void ExecuteScalar_String_ReturnsValue()
        {
            using var db = Seed();
            Assert.Equal("x", db.ExecuteScalar<string>("SELECT Name FROM Item WHERE Id = @id", new { id = 1 }));
        }

        [Fact]
        public void ExecuteScalar_EmptyResultSet_ReturnsDefault()
        {
            using var db = Seed();
            Assert.Equal(0, db.ExecuteScalar<int>("SELECT Id FROM Item WHERE Id = 999"));
        }

        [Fact]
        public void ExecuteScalar_SqlNull_ReturnsDefault()
        {
            using var db = Seed();
            Assert.Equal(0, db.ExecuteScalar<int>("SELECT MAX(Id) FROM Item WHERE Id > 1000"));
        }

        [Fact]
        public void ExecuteScalar_DateTime_IsParsed()
        {
            using var db = Seed();

            var dt = db.ExecuteScalar<DateTime>("SELECT '2020-01-02 03:04:05'");

            Assert.Equal(new DateTime(2020, 1, 2, 3, 4, 5), dt);
        }

        [Fact]
        public void ExecuteScalar_NullableValueType_ReturnsValue()
        {
            using var db = Seed();
            Assert.Equal(5, db.ExecuteScalar<int?>("SELECT 5"));
        }

        [Fact]
        public void ExecuteScalar_NullableValueType_EmptyOrNull_ReturnsNull()
        {
            using var db = Seed();
            Assert.Null(db.ExecuteScalar<int?>("SELECT Id FROM Item WHERE Id = 999"));
            Assert.Null(db.ExecuteScalar<int?>("SELECT MAX(Id) FROM Item WHERE Id > 1000"));
        }

        [Fact]
        public void ExecuteScalar_NullableDateTime_IsParsed()
        {
            using var db = Seed();

            var dt = db.ExecuteScalar<DateTime?>("SELECT '2020-01-02'");

            Assert.Equal(new DateTime(2020, 1, 2), dt);
        }

        [Fact]
        public void ExecuteScalar_InTransaction_SeesUncommittedRows()
        {
            using var db = Seed();

            using var trn = db.BeginTransaction();
            trn.Execute("INSERT INTO Item (Id, Name) VALUES (10, 'a')");

            Assert.Equal(3, trn.ExecuteScalar<int>("SELECT COUNT(*) FROM Item"));
        }

        public class Item
        {
            [PrimaryKey]
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
