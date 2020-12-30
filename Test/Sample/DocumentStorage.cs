using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.Sqlite;

namespace Test.Sample
{
    public class DocumentStorage
    {
        public static void run()
        {
            NoSqliteDoc db = new NoSqliteDoc("myStuff.db");
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

            string id = d.MyUID.ToString();

            db.Store(id, d);

            var d2 = db.Retrieve<MyData>(id);
            d2 = d2;

            var allKeys = db.GetAllKeys().ToArray();
        }
    }
}
