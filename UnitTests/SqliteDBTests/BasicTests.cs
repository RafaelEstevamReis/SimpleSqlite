using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace UnitTests.SqliteDBTests
{
    public class BasicTests
    {
        [Fact]
        public void SqliteDB_TestInMemoryDBOpen()
        {
            var db = Helpers.TestDBBuilder.Create();
            Assert.Equal(10, db.ExecuteScalar<int>("SELECT 10", null));
        }
    }
}
