using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Simple.DatabaseWrapper.Attributes;
using Simple.Sqlite;

namespace Test.Sample
{
    public class IntPrimaryKeyExample
    {
        public static void run()
        {
            var db = new SqliteDB("myStuff.db");
            Console.WriteLine($"Database is at {db.DatabaseFileName}");
           
            // Create a DB Schema
            db.CreateTables()
              .Add<IntSample>()
              .Commit();
            // reset before sample
            db.ExecuteNonQuery("DELETE FROM IntSample");

            var bobAccount = new IntSample()
            {
                Id = 0, // will be generated
                Name = "bob"
            };
            //When using INT PrimaryKeys, the column becomes the sqlite's _rowid_
            var id = db.InsertOrReplace(bobAccount);
            Console.WriteLine($"Inserted with ID {id}");
            // bobAccount.Id is still zero

            // update bob
            bobAccount = db.Get<IntSample>(id);
            Console.WriteLine($"Name is still `{bobAccount.Name}`");
            Console.WriteLine($"Previous inserted ID was: `{id}` and  bobAccount.Id: `{bobAccount.Id}`");

            bobAccount.Name = "Bob's account";
            var id2 = db.InsertOrReplace(bobAccount);

            foreach (var bobs in db.GetAll<IntSample>())
            {
                Console.WriteLine($"Id: {bobs.Id} Name: {bobs.Name}");
            }

        }

        public class IntSample
        {
            [PrimaryKey]
            public int Id { get; set; }
            public string Name { get; set; }
        }
    }
}