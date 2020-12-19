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
            db.CreateTables()
              .Add<MyData>()
              .Commit();

            var d = new MyData()
            {
                MyId = (int)DateTimeOffset.Now.ToUnixTimeSeconds(),
                MyName = "My name is bob",
                MyBirthDate = DateTime.Now,
                MyDecimalValue = 123.4M,
                MyDoubleValue = 456.7,
                MyFloatValue = 789.3f
            };
            Console.WriteLine($"New data inserted: Id={d.MyId}");
            db.Insert(d);

            // get all data
            var allData = db.GetAll<MyData>();

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
        public float MyFloatValue { get; set; }
    }
}
