using Simple.Sqlite;
using Simple.Sqlite.Extension;
using System;
using System.Linq;
using Xunit;

namespace UnitTest.SqliteExtensionsTests.GuidTests
{
    public class GuidReadWriteTests
    {
        [Fact]
        public void GuidTests_ReadWrite()
        {
            ConnectionFactory.HandleGuidAsByteArray = true;

            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables()
              .Add<MyGuidClass>()
              .Commit();

            var guid = Guid.NewGuid();
            // Write GUID, read Bytes
            db.Insert(new MyGuidClass() { Guid = guid });
            var c2 = db.Query<MyByteClass>("SELECT * FROM MyGuidClass", null).FirstOrDefault();

            Assert.Equal(guid, new Guid(c2.Guid));
        }



        public class MyGuidClass
        {
            public Guid Guid { get; set; }
        }
        public class MyByteClass
        {
            public byte[] Guid { get; set; }
        }

    }
}
