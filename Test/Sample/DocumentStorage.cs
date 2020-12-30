using System;
using System.Linq;
using Simple.Sqlite;

namespace Test.Sample
{
    public class DocumentStorage
    {
        public static void run()
        {
            NoSqliteStorage db = new NoSqliteStorage("myStuff.db");
            Console.WriteLine($"Database is at {db.DatabaseFileName}");

            var d = new MyData()
            {
                MyId = (int)DateTimeOffset.Now.ToUnixTimeSeconds(),
                MyName = "My name is bob",
                MyBirthDate = DateTime.Now,
                MyUID = Guid.NewGuid(),
                MyWebsite = new Uri("http://example.com"),
                MyDecimalValue = 123.4M,
                MyDoubleValue = 456.7,
                MyFloatValue = 789.3f,
                MyEnum = MyData.eIntEnum.Zero,
            };

            var id = d.MyUID;

            db.Store(id, d);
            var d2 = db.Retrieve<MyData>(id);
            var allKeys = db.GetAllKeys().ToArray();
            var allGuids = db.GetAllGuids().ToArray();
        }
    }
}
