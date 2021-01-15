using System;
using System.Linq;
using Simple.Sqlite;

namespace Test.Sample
{
    public class FullCycle
    {
        public static void run()
        {
            SqliteDB db = new SqliteDB("myStuff.db");
            Console.WriteLine($"Database is at {db.DatabaseFileName}");

            // Create a DB Schema
            db.CreateTables()
              .Add<MyData>()
              .Commit();

            var d = new MyData()
            {
                MyId = (int)DateTimeOffset.Now.ToUnixTimeSeconds(),
                MyName = "My name is bob",
                MyBirthDate = DateTime.Now,
                MyUID = Guid.NewGuid(),
                MyWebsite = new Uri("http://example.com"),
                MyFavColor = System.Drawing.Color.FromArgb(101, 102, 103, 104),
                MyDecimalValue = 123.4M,
                MyDoubleValue = 456.7,
                MyFloatValue = 789.3f,
                MyEnum = MyData.eIntEnum.Zero,
            };
            Console.WriteLine($"New data to insert: Id={d.MyId} MyUID={d.MyUID}");
            db.Insert(d);

            // get all data
            var allData = db.GetAll<MyData>();

            Console.WriteLine("All data:");
            foreach (var rowData in allData)
            {
                Console.WriteLine($" > {rowData.MyId} {rowData.MyName} {rowData.MyUID}");
            }

            //get "bob" data
            var bobs = db.ExecuteQuery<MyData>("SELECT * FROM MyData WHERE MyName LIKE @name ", new { name = "%bob%" });
            Console.WriteLine("All bob data:");
            foreach (var rowData in bobs)
            {
                Console.WriteLine($" > {rowData.MyId} {rowData.MyName} {rowData.MyUID}");
            }

            // change frst bob
            var firstBob = bobs.First();
            firstBob.MyName = "Changed bob";
            db.InsertOrReplace(firstBob);
            // show all data again
            Console.WriteLine("All data:");
            foreach (var rowData in allData)
            {
                Console.WriteLine($" > {rowData.MyId} {rowData.MyName} {rowData.MyUID}");
            }
        }
    }
}
