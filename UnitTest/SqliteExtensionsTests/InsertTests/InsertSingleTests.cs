using Microsoft.Data.Sqlite;
using Simple.DatabaseWrapper.Attributes;
using Simple.Sqlite;
using System;
using System.Linq;
using Xunit;

namespace UnitTest.SqliteExtensionsTests.InsertTests
{
    public class InsertSingleTests
    {
        private static ISqliteConnection NewDb<T>() where T : new()
        {
            var db = ConnectionFactory.CreateInMemory();
            db.CreateTables().Add<T>().Commit();
            return db;
        }

        [Fact]
        public void Insert_AutoIncrement_ReturnsSequentialRowId()
        {
            using var db = NewDb<Rec>();

            Assert.Equal(1, db.Insert(new Rec { Name = "a" }));
            Assert.Equal(2, db.Insert(new Rec { Name = "b" }));
        }

        [Fact]
        public void Insert_ExplicitIntKey_ReturnsThatKey()
        {
            using var db = NewDb<Rec>();

            Assert.Equal(42, db.Insert(new Rec { Id = 42, Name = "x" }));
        }

        [Fact]
        public void Insert_RoundTripsSupportedTypes()
        {
            using var db = NewDb<Typed>();
            var item = new Typed { Id = 1, Text = "hi", Number = 3.5, Flag = true, Moment = new DateTime(2021, 6, 15, 10, 20, 30) };

            db.Insert(item);
            var back = db.Get<Typed>(1);

            Assert.Equal("hi", back.Text);
            Assert.Equal(3.5, back.Number);
            Assert.True(back.Flag);
            Assert.Equal(item.Moment, back.Moment);
        }

        [Fact]
        public void Insert_NullStringField_IsStoredAsNull()
        {
            using var db = NewDb<Typed>();

            db.Insert(new Typed { Id = 1, Text = null, Number = 0, Flag = false, Moment = DateTime.MinValue });

            Assert.Null(db.Get<Typed>(1).Text);
        }

        [Fact]
        public void Insert_EmptyGuidKey_IsGeneratedAndWrittenBack()
        {
            using var db = NewDb<GuidModel>();
            var item = new GuidModel { Key = Guid.Empty, Name = "g" };

            db.Insert(item);

            Assert.NotEqual(Guid.Empty, item.Key);          // generated + written back to the object
            Assert.Equal("g", db.Get<GuidModel>(item.Key).Name);
        }

        [Fact]
        public void Insert_ExplicitGuidKey_IsKept()
        {
            using var db = NewDb<GuidModel>();
            var key = Guid.NewGuid();

            db.Insert(new GuidModel { Key = key, Name = "g" });

            Assert.NotNull(db.Get<GuidModel>(key));
        }

        [Fact]
        public void Insert_DuplicateKey_DefaultAbort_Throws()
        {
            using var db = NewDb<Rec>();
            db.Insert(new Rec { Id = 1, Name = "a" });

            Assert.Throws<SqliteException>(() => db.Insert(new Rec { Id = 1, Name = "dup" }));
        }

        [Fact]
        public void Insert_Ignore_KeepsExisting()
        {
            using var db = NewDb<Rec>();
            db.Insert(new Rec { Id = 1, Name = "a" });

            db.Insert(new Rec { Id = 1, Name = "dup" }, OnConflict.Ignore);

            Assert.Equal("a", db.Get<Rec>(1).Name);
        }

        [Fact]
        public void Insert_Replace_Overwrites()
        {
            using var db = NewDb<Rec>();
            db.Insert(new Rec { Id = 1, Name = "a" });

            db.Insert(new Rec { Id = 1, Name = "dup" }, OnConflict.Replace);

            Assert.Equal("dup", db.Get<Rec>(1).Name);
        }

        [Fact]
        public void Insert_TableNameOverride_TargetsGivenTable()
        {
            using var db = NewDb<Rec>();
            db.Execute("CREATE TABLE Rec2 (Id INTEGER PRIMARY KEY, Name TEXT)");

            db.Insert(new Rec { Id = 1, Name = "a" }, tableName: "Rec2");

            Assert.Equal(1, db.ExecuteScalar<int>("SELECT COUNT(*) FROM Rec2"));
            Assert.Equal(0, db.ExecuteScalar<int>("SELECT COUNT(*) FROM Rec"));
        }

        [Fact]
        public void Insert_InTransaction_RollbackDiscards_CommitPersists()
        {
            using var db = NewDb<Rec>();

            using (var trn = db.BeginTransaction())
            {
                trn.Insert(new Rec { Id = 1, Name = "a" });
                trn.Rollback();
            }
            Assert.Null(db.Get<Rec>(1));

            using (var trn = db.BeginTransaction())
            {
                trn.Insert(new Rec { Id = 2, Name = "b" });
                trn.Commit();
            }
            Assert.NotNull(db.Get<Rec>(2));
        }

        [Fact]
        public void Insert_EnumPolicies_StoreAsNumberAndText()
        {
            using var db = NewDb<EnumModel>();

            db.Insert(new EnumModel { Id = 1, Num = Color.Blue, Txt = Color.Green });

            Assert.Equal(2, db.ExecuteScalar<int>("SELECT Num FROM EnumModel WHERE Id = 1"));
            Assert.Equal("Green", db.ExecuteScalar<string>("SELECT Txt FROM EnumModel WHERE Id = 1"));

            var back = db.Get<EnumModel>(1);
            Assert.Equal(Color.Blue, back.Num);
            Assert.Equal(Color.Green, back.Txt);
        }

        [Fact]
        public void Insert_Uri_IsStoredAsString()
        {
            using var db = NewDb<UriModel>();

            db.Insert(new UriModel { Id = 1, Link = new Uri("https://example.com/") });

            Assert.Equal("https://example.com/", db.ExecuteScalar<string>("SELECT Link FROM UriModel WHERE Id = 1"));
        }

        public class Rec
        {
            [PrimaryKey] public int Id { get; set; }
            public string Name { get; set; }
        }

        public class Typed
        {
            [PrimaryKey] public int Id { get; set; }
            public string Text { get; set; }
            public double Number { get; set; }
            public bool Flag { get; set; }
            public DateTime Moment { get; set; }
        }

        public class GuidModel
        {
            [PrimaryKey] public Guid Key { get; set; }
            public string Name { get; set; }
        }

        public enum Color { Red = 0, Green = 1, Blue = 2 }

        public class EnumModel
        {
            [PrimaryKey] public int Id { get; set; }
            public Color Num { get; set; }
            [EnumPolicy(EnumPolicyAttribute.Policies.AsText)] public Color Txt { get; set; }
        }

        public class UriModel
        {
            [PrimaryKey] public int Id { get; set; }
            public Uri Link { get; set; }
        }
    }
}
