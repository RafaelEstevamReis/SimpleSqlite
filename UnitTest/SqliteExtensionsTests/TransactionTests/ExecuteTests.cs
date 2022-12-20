using Simple.Sqlite;
using Xunit;

namespace UnitTest.SqliteExtensionsTests.TransactionTests
{
    public class ExecuteTests
    {
        [Fact]
        public void TransactionTests_ExecuteWithTransacations()
        {
            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables()
              .Add<MyTestClass>()
              .Commit();

            // Read table: 0 rows
            int countFirst = db.ExecuteScalar<int>("SELECT COUNT(*) FROM MyTestClass");
            Assert.Equal(0, countFirst);
            // open transaction
            using (var tr = db.BeginTransaction())
            {
                // insert in tr
                tr.Execute("INSERT INTO MyTestClass (Id) VALUES (1) ");
                // read in tr: 1 row
                int countSecond = tr.ExecuteScalar<int>("SELECT COUNT(*) FROM MyTestClass");
                Assert.Equal(1, countSecond);

                tr.Rollback();
            }
            // read out of transaction: 0 rows
            int countThird = db.ExecuteScalar<int>("SELECT COUNT(*) FROM MyTestClass");
            Assert.Equal(0, countThird);

        }
        public class MyTestClass
        {
            public int Id { get; set; }
        }
    }
}
