using Simple.DatabaseWrapper.Attributes;
using Simple.Sqlite;
using System.Linq;
using Xunit;

namespace UnitTest.SqliteExtensionsTests.QuotingTests
{
    // Exercises SQL identifier quoting: table/column names that are reserved SQLite
    // keywords ("Order", "Group", "Select") must round-trip through DDL and CRUD.
    public class IdentifierQuotingTests
    {
        [Fact]
        public void Quoting_ReservedNames_CreateAndInsert()
        {
            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<Order>().Commit();

            db.Insert(new Order { Id = 1, Group = "g1", Select = "s1" });
            db.Insert(new Order { Id = 2, Group = "g2", Select = "s2" });

            Assert.Equal(2, db.GetAll<Order>().Count());
        }

        [Fact]
        public void Quoting_ReservedNames_GetByPrimaryKey()
        {
            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<Order>().Commit();
            db.Insert(new Order { Id = 7, Group = "g", Select = "s" });

            var row = db.Get<Order>(7);

            Assert.NotNull(row);
            Assert.Equal("g", row.Group);
        }

        [Fact]
        public void Quoting_ReservedColumn_GetWhere()
        {
            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<Order>().Commit();
            db.Insert(new Order { Id = 1, Group = "target", Select = "s1" });
            db.Insert(new Order { Id = 2, Group = "other", Select = "s2" });

            var found = db.GetWhere<Order>("Group", "target").Single();

            Assert.Equal(1, found.Id);
        }

        [Fact]
        public void Quoting_ReservedColumn_QueryParameterBuild()
        {
            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<Order>().Commit();
            db.Insert(new Order { Id = 1, Group = "a", Select = "s1" });
            db.Insert(new Order { Id = 2, Group = "b", Select = "s2" });

            var found = db.Query<Order>(new { Group = "b" }).Single();

            Assert.Equal(2, found.Id);
        }

        [Fact]
        public void Quoting_ReservedNames_IndexIsCreated()
        {
            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<Order>().Commit();

            var exists = db.ExecuteScalar<int>(
                "SELECT COUNT(*) FROM sqlite_master WHERE type='index' AND name='IX_Order_Group'");

            Assert.Equal(1, exists);
        }

        [Fact]
        public void Quoting_ReservedTable_GetTableInfo()
        {
            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<Order>().Commit();

            var info = db.GetTableInfo("Order");

            Assert.Equal(3, info.Length);
        }

        [Fact]
        public void Quoting_RowidFallback_StaysUnquoted()
        {
            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<NoPk>().Commit();

            long rowid = db.Insert(new NoPk { Value = 42 });
            var back = db.Get<NoPk>(rowid);

            Assert.NotNull(back);
            Assert.Equal(42, back.Value);
        }

        public class Order
        {
            [PrimaryKey]
            public int Id { get; set; }
            [Index("IX_Order_Group")]
            public string Group { get; set; }
            public string Select { get; set; }
        }

        public class NoPk
        {
            public int Value { get; set; }
        }
    }
}
