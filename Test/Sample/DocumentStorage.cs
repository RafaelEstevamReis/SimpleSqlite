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



        }
    }
}
