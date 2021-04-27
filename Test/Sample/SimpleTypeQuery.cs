using System;
using System.Linq;
using Simple.Sqlite;

namespace Test.Sample
{
    public class SimpleTypeQuery
    {
        public static void run()
        {
            SqliteDB db = new SqliteDB("myStuff.db");
            Console.WriteLine($"Database is at {db.DatabaseFileName}");

            // Create a DB Schema
            db.CreateTables()
              .Add<MyData>()
              .Commit();

            db.InsertInto(new
            {
                MyUID = Guid.NewGuid(),
                MyName = "Test"
            }, 
            OnConflict.Ignore, 
            tableName: "MyData");

            for (int i = 0; i < 5; i++)
            {
                db.Insert(new MyData()
                {
                    MyId = i * 3,
                    MyUID = Guid.NewGuid()
                });
            }
            var allInts = db.ExecuteQuery<int>("SELECT MyId FROM MyData", null).ToArray();
        }
    }
}
