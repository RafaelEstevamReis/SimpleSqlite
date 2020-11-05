using Simple.Sqlite;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            SqliteDB db = new SqliteDB("myStuff.db");
            Console.WriteLine($"Database is at {db.DatabaseFileName}");

            // Create a DB Schema
            var table = SimpleTableSchema.BuildFromType<MyData>("MyInfo");
            if (db.CreateTables(new[] { table }) > 0)
            {
                Console.WriteLine($"A table was created");
            }

            var d = new MyData()
            {
                MyId = (int)DateTimeOffset.Now.ToUnixTimeSeconds(),
                MyName = "My name is bob",
                MyBirthDate = DateTime.Now,
                MyDecimalValue = 123.4M,
                MyDoubleValue = 456.7
            };
            Console.WriteLine($"New data isnerted: Id={d.MyId}");
            db.InsertInto("MyInfo", d);

            // get all data
            var allData = db.ExecuteQuery<MyData>("SELECT * FROM MyInfo", null).ToArray();

            Console.WriteLine("All data:");
            foreach (var rowData in allData)
            {
                Console.WriteLine($" > {rowData.MyId} {rowData.MyName}");
            }
        }
    }
    public class MyData
    {
        public int MyId { get; set; }
        public string MyName { get; set; }
        public DateTime MyBirthDate { get; set; }
        public decimal MyDecimalValue { get; set; }
        public double MyDoubleValue { get; set; }
    }
}
