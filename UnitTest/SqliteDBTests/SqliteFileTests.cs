using Simple.Sqlite;
using System;
using System.Data.SQLite;
using System.IO;
using Xunit;

namespace UnitTest.SqliteDBTests
{
    public class SqliteFileTests : IDisposable
    {
        private readonly FileInfo file;

        public SqliteFileTests()
        {
            file = new FileInfo($"demoDb_{Guid.NewGuid()}.db");
        }
        protected SqliteDB GetDb() => new SqliteDB(file.FullName);

        [Fact]
        public void SqliteDB_TestConnection()
        {
            var db = GetDb();
            Assert.Equal(1, db.ExecuteScalar<int>("SELECT 1", null));
        }

        public void Dispose()
        {
            //if (file.Exists) file.Delete();
            if (File.Exists(file.FullName)) file.Delete();
        }
    }
}
