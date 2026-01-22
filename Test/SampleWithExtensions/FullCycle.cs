using System;
using System.Linq;
using Simple.Sqlite;

namespace Test.SampleWithExtensions
{
    public class FullCycle
    {
        public static void run()
        {
            // The factory can be used in two ways:
            // 1. Simple get the connection from a file (static)
            // 2. Using DI with a Factory instance
            // this example uses [1]

            using var cnn = ConnectionFactory.CreateConnection("myExtendedStuff.db");
            Console.WriteLine($"Database is at {cnn.DatabasFilePath}");

            // Create a DB Schema
            var result = cnn.CreateTables()
                            .Add<MyData>()
                            .EditTable(t => ((Table)t).AsStrict = false)
                            .Commit();
            if (result.Length > 0 && result[0].WasTableCreated)
            {
                Console.WriteLine("A new table was created!");
            }

            var d = new MyData()
            {
                MyId = (int)DateTimeOffset.Now.ToUnixTimeSeconds(),
                MyName = "My name is bob",
                MyBirthDate = DateTime.Now,
                MyTimeSpan = TimeSpan.FromSeconds(5),
                MyUID = Guid.Empty, // When empty, a new Guid will be generated
                MyWebsite = new Uri("http://example.com"),
                MyFavColor = System.Drawing.Color.FromArgb(101, 102, 103, 104),
                MyDecimalValue = 123.4M,
                MyDoubleValue = 456.7,
                MyFloatValue = 789.3f,
                MyEnum = MyData.eIntEnum.Zero,
                MyTextEnum = MyData.eTextEnum.Oranges
            };

            Console.WriteLine($"New data to insert: Id={d.MyId} MyUID={d.MyUID}");
            cnn.Insert(d);

            // get all data
            var allData = cnn.GetAll<MyData>();

            Console.WriteLine("All data:");
            foreach (var rowData in allData)
            {
                Console.WriteLine($" > {rowData.MyId} {rowData.MyName} {rowData.MyUID}");
            }
            
            //get "bob" data
            var bobs = cnn.Query<MyData>("SELECT * FROM MyData WHERE MyName LIKE @name ", new { name = "%bob%" });
            Console.WriteLine("All bob data:");
            foreach (var rowData in bobs)
            {
                Console.WriteLine($" > {rowData.MyId} {rowData.MyName} {rowData.MyUID}");
            }

            // change frst bob
            var firstBob = bobs.First();
            firstBob.MyName = "Changed bob";

            cnn.Insert(firstBob, OnConflict.Replace);
            // show all data again
            Console.WriteLine("All data:");
            foreach (var rowData in allData)
            {
                Console.WriteLine($" > {rowData.MyId} {rowData.MyName} {rowData.MyUID}");
            }

            cnn.Dispose();
        }
    }
}
