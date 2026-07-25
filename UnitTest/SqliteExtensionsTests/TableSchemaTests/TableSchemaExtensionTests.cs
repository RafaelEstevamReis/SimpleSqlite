using Simple.DatabaseWrapper.Attributes;
using Simple.Sqlite;
using Simple.Sqlite.Attributes;
using System.Linq;
using Xunit;

namespace UnitTest.SqliteExtensionsTests.TableSchemaTests
{
    public class TableSchemaExtensionTests
    {
        [Fact]
        public void GetTableColumnNames_ReturnsColumnsInOrder()
        {
            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<Model>().Commit();

            var cols = db.GetTableColumnNames("Model");

            Assert.Equal(new[] { "Id", "Name", "Age" }, cols);
        }

        [Fact]
        public void GetTableSchema_ReturnsRowPerColumn()
        {
            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<Model>().Commit();

            var dt = db.GetTableSchema("Model");

            Assert.Equal(3, dt.Rows.Count);
        }

        [Fact]
        public void GetAllTables_FilterExcludesSqliteInternalTables()
        {
            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<Model>().Commit();
            db.Insert(new Model { Id = 1, Name = "a", Age = 1 }); // triggers sqlite_sequence (AUTOINCREMENT)

            var all = db.GetAllTables(include_sqlite_tables: true);
            var userOnly = db.GetAllTables(include_sqlite_tables: false);

            Assert.Contains("Model", all);
            Assert.Contains(all, t => t.StartsWith("sqlite_"));
            Assert.Contains("Model", userOnly);
            Assert.DoesNotContain(userOnly, t => t.StartsWith("sqlite_"));
        }

        [Fact]
        public void GetAllIndexes_ContainsCreatedIndex()
        {
            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<Indexed>().Commit();

            Assert.Contains("IX_Name", db.GetAllIndexes());
        }

        [Fact]
        public void GetTableInfo_ReportsColumnsAndPrimaryKey()
        {
            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<Model>().Commit();

            var info = db.GetTableInfo("Model");

            Assert.Equal(3, info.Length);
            var id = info.Single(c => c.name == "Id");
            Assert.Equal(1, id.pk);                 // Id is the primary key
            Assert.Equal(0, info.Single(c => c.name == "Name").pk);
        }

        [Fact]
        public void GetTableInfo_NonExistentTable_ReturnsEmpty()
        {
            using var db = ConnectionFactory.CreateInMemory();

            Assert.Empty(db.GetTableInfo("Nope"));
        }

        [Fact]
        public void GetTableList_ReportsTableAndStrictFlag()
        {
            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<Model>().Add<StrictModel>().Commit();

            var list = db.GetTableList();

            var model = list.Single(t => t.name == "Model");
            Assert.Equal("table", model.type);
            Assert.False(model.strict);
            Assert.True(list.Single(t => t.name == "StrictModel").strict);
        }

        public class Model
        {
            [PrimaryKey] public int Id { get; set; }
            public string Name { get; set; }
            public int Age { get; set; }
        }

        public class Indexed
        {
            [PrimaryKey] public int Id { get; set; }
            [Index("IX_Name")] public string Name { get; set; }
        }

        [StrictTable]
        public class StrictModel
        {
            [PrimaryKey] public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}
