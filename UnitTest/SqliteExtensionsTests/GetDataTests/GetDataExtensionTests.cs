using Simple.DatabaseWrapper.Attributes;
using Simple.Sqlite;
using System;
using System.Linq;
using Xunit;

namespace UnitTest.SqliteExtensionsTests.GetDataTests
{
    public class GetDataExtensionTests
    {
        private static ISqliteConnection Seed()
        {
            var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<Person>().Commit();
            db.Insert(new Person { Id = 1, Name = "alice", Age = 30 });
            db.Insert(new Person { Id = 2, Name = "bob", Age = 40 });
            db.Insert(new Person { Id = 3, Name = "bob", Age = 50 });
            return db;
        }

        [Fact]
        public void Get_ByPrimaryKey_ReturnsRow()
        {
            using var db = Seed();

            var p = db.Get<Person>(2);

            Assert.NotNull(p);
            Assert.Equal("bob", p.Name);
        }

        [Fact]
        public void Get_ByPrimaryKey_NotFound_ReturnsNull()
        {
            using var db = Seed();

            Assert.Null(db.Get<Person>(999));
        }

        [Fact]
        public void Get_ByColumn_ReturnsFirstMatch()
        {
            using var db = Seed();

            // two rows named "bob" -> LIMIT 1 returns one
            var p = db.Get<Person>("Name", "bob");

            Assert.NotNull(p);
            Assert.Equal("bob", p.Name);
        }

        [Fact]
        public void Get_ExplicitTableAndColumn_ReturnsRow()
        {
            using var db = Seed();

            var p = db.Get<Person>("Person", "Age", 40);

            Assert.Equal(2, p.Id);
        }

        [Fact]
        public void Get_ThreeArg_NullTableName_Throws()
        {
            using var db = Seed();
            Assert.Throws<ArgumentNullException>(() => db.Get<Person>(null, "Id", 1));
        }

        [Fact]
        public void Get_ThreeArg_NullKeyColumn_Throws()
        {
            using var db = Seed();
            Assert.Throws<ArgumentNullException>(() => db.Get<Person>("Person", null, 1));
        }

        [Fact]
        public void Get_NoPrimaryKey_UsesRowIdFallback()
        {
            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<NoPk>().Commit();
            long rowid = db.Insert(new NoPk { Value = 42, Name = "x" });

            var back = db.Get<NoPk>(rowid);

            Assert.NotNull(back);
            Assert.Equal(42, back.Value);
        }

        [Fact]
        public void Get_ByGuidPrimaryKey_RoundTrips()
        {
            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<GuidModel>().Commit();
            var key = Guid.NewGuid();
            db.Insert(new GuidModel { Key = key, Name = "g" });

            var back = db.Get<GuidModel>(key);

            Assert.NotNull(back);
            Assert.Equal("g", back.Name);
        }

        [Fact]
        public void GetAll_ReturnsEveryRow()
        {
            using var db = Seed();
            Assert.Equal(3, db.GetAll<Person>().Count());
        }

        [Fact]
        public void GetAll_EmptyTable_ReturnsEmpty()
        {
            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<Person>().Commit();

            Assert.Empty(db.GetAll<Person>());
        }

        [Fact]
        public void GetAll_Unbuffered_StillReturnsRows()
        {
            using var db = Seed();

            var rows = db.GetAll<Person>(buffered: false).ToList();

            Assert.Equal(3, rows.Count);
        }

        [Fact]
        public void GetWhere_ReturnsMatchingRows()
        {
            using var db = Seed();

            var bobs = db.GetWhere<Person>("Name", "bob").ToArray();

            Assert.Equal(2, bobs.Length);
            Assert.All(bobs, p => Assert.Equal("bob", p.Name));
        }

        [Fact]
        public void GetWhere_NoMatch_ReturnsEmpty()
        {
            using var db = Seed();
            Assert.Empty(db.GetWhere<Person>("Name", "zzz"));
        }

        [Fact]
        public void GetWhere_Unbuffered_ReturnsMatchingRows()
        {
            using var db = Seed();

            var bobs = db.GetWhere<Person>("Name", "bob", buffered: false).ToList();

            Assert.Equal(2, bobs.Count);
        }

        [Fact]
        public void GetWhere_NullFilterColumn_Throws()
        {
            using var db = Seed();
            Assert.Throws<ArgumentNullException>(() => db.GetWhere<Person>(null, "x").ToArray());
        }

        public class Person
        {
            [PrimaryKey] public int Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
        }

        public class NoPk
        {
            public int Value { get; set; }
            public string Name { get; set; }
        }

        public class GuidModel
        {
            [PrimaryKey] public Guid Key { get; set; }
            public string Name { get; set; }
        }
    }
}
