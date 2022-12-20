using Simple.DatabaseWrapper.Attributes;
using Simple.Sqlite;
using System;
using System.Linq;
using Xunit;

namespace UnitTest.SqliteExtensionsTests.GuidTests
{
    public class GuidReadWriteTests
    {
        [Fact]
        public void GuidTests_ReadWrite_GuidToByte()
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
        [Fact]
        public void GuidTests_ReadWrite_ByteToGuid_DefTrue()
        {
            ConnectionFactory.HandleGuidAsByteArray = true;

            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables()
              .Add<MyByteClass>()
              .Commit();

            var guid = Guid.NewGuid();
            // Write GUID, read Bytes
            db.Insert(new MyByteClass() { Guid = guid.ToByteArray() });
            var c2 = db.Query<MyGuidClass>("SELECT * FROM MyByteClass", null).FirstOrDefault();

            Assert.Equal(guid, c2.Guid);
        }

        [Fact]
        public void GuidTests_ReadWrite_ByteToGuid_DefFalse()
        {
            ConnectionFactory.HandleGuidAsByteArray = false;

            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables()
              .Add<MyByteClass>()
              .Commit();

            var guid = Guid.NewGuid();
            // Write GUID, read Bytes
            db.Insert(new MyByteClass() { Guid = guid.ToByteArray() });
            var c2 = db.Query<MyGuidClass>("SELECT * FROM MyByteClass", null).FirstOrDefault();

            Assert.Equal(guid, c2.Guid);
        }
        [Fact]
        public void GuidTests_ReadWrite_GuidToByte_DefFalse()
        {
            ConnectionFactory.HandleGuidAsByteArray = false;

            using var db = ConnectionFactory.CreateInMemory();
            db.CreateTables()
              .Add<MyByteClass>()
              .Commit();

            var guid = Guid.NewGuid();
            // Write GUID, read Bytes
            db.Insert(new MyByteClass() { Guid = guid.ToByteArray() });
            var c2 = db.Query<MyGuidClass>("SELECT * FROM MyByteClass", null).FirstOrDefault();

            Assert.Equal(guid, c2.Guid);
        }

        public class MyGuidClass
        {
            [PrimaryKey]
            public Guid Guid { get; set; }
        }
        public class MyByteClass
        {
            public byte[] Guid { get; set; }
        }

    }
}
