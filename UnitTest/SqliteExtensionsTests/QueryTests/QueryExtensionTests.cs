using Simple.DatabaseWrapper.Attributes;
using Simple.Sqlite;
using System;
using System.Linq;
using Xunit;

namespace UnitTest.SqliteExtensionsTests.QueryTests
{
    public class QueryExtensionTests
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

        // ---- raw SQL ----

        [Fact]
        public void Query_Sql_MapsModelRows()
        {
            using var db = Seed();

            var all = db.Query<Person>("SELECT * FROM Person ORDER BY Id").ToArray();

            Assert.Equal(3, all.Length);
            Assert.Equal("alice", all[0].Name);
            Assert.Equal(50, all[2].Age);
        }

        [Fact]
        public void Query_Sql_WithParameters_Filters()
        {
            using var db = Seed();

            var rows = db.Query<Person>("SELECT * FROM Person WHERE Age >= @min", new { min = 40 }).ToArray();

            Assert.Equal(2, rows.Length);
        }

        [Fact]
        public void Query_SimpleType_ReadsFirstColumn()
        {
            using var db = Seed();

            var ids = db.Query<int>("SELECT Id FROM Person ORDER BY Id").ToArray();
            var name = db.Query<string>("SELECT Name FROM Person WHERE Id = 1").Single();

            Assert.Equal(new[] { 1, 2, 3 }, ids);
            Assert.Equal("alice", name);
        }

        [Fact]
        public void Query_NoRows_ReturnsEmpty()
        {
            using var db = Seed();
            Assert.Empty(db.Query<Person>("SELECT * FROM Person WHERE Id = 999"));
        }

        [Fact]
        public void Query_Unbuffered_ReturnsRows()
        {
            using var db = Seed();

            var rows = db.Query<Person>("SELECT * FROM Person", null, buffered: false).ToList();

            Assert.Equal(3, rows.Count);
        }

        [Fact]
        public void Query_InTransaction_SeesUncommittedRows()
        {
            using var db = Seed();

            using var trn = db.BeginTransaction();
            trn.Insert(new Person { Id = 99, Name = "temp", Age = 1 });

            Assert.Single(trn.Query<Person>("SELECT * FROM Person WHERE Id = 99"));
        }

        // ---- parameter-built WHERE ----

        [Fact]
        public void Query_ParamBuild_SingleColumn_Filters()
        {
            using var db = Seed();

            var bobs = db.Query<Person>(new { Name = "bob" }).ToArray();

            Assert.Equal(2, bobs.Length);
            Assert.All(bobs, p => Assert.Equal("bob", p.Name));
        }

        [Fact]
        public void Query_ParamBuild_MultipleColumns_AreAnded()
        {
            using var db = Seed();

            var row = db.Query<Person>(new { Name = "bob", Age = 40 }).Single();

            Assert.Equal(2, row.Id);
        }

        [Fact]
        public void Query_ParamBuild_IsCaseInsensitiveOnColumnName()
        {
            using var db = Seed();

            var row = db.Query<Person>(new { name = "alice" }).Single(); // lowercase property

            Assert.Equal(1, row.Id);
        }

        [Fact]
        public void Query_ParamBuild_NullParameters_Throws()
        {
            using var db = Seed();
            Assert.Throws<ArgumentNullException>(() => db.Query<Person>((object?)null));
        }

        [Fact]
        public void Query_ParamBuild_UnknownProperty_ThrowsClearError()
        {
            using var db = Seed();
            Assert.Throws<ArgumentException>(() => db.Query<Person>(new { Nope = 1 }));
        }

        public class Person
        {
            [PrimaryKey] public int Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
        }
    }
}
