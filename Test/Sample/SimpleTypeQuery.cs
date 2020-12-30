using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
