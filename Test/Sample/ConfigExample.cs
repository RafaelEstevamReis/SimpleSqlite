using System;
using Simple.Sqlite;

namespace Test.Sample
{
    public class ConfigExample
    {
        public static void run()
        {
            ConfigurationDB db = new ConfigurationDB("myStuff.db");
            Console.WriteLine($"Database is at {db.DatabaseFileName}");

            var guid = Guid.NewGuid();
            showExample<Guid>(db, "Guid", "MyGuid", "General.UI", guid, Guid.Empty);

            var name = "MyNameText";
            showExample<string>(db, "String", "MyName", "General.UI", name, string.Empty);



            /*
             
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
             */

        }

        private static void showExample<T>(ConfigurationDB db, string typeName, string key, string category, T valueToInsert, T defaultValue)
        {
            // force delete due to multiple executions
            db.RemoveConfig(key, category);

            Console.WriteLine($"Current {typeName}: {db.ReadConfig(key, category, defaultValue)}");
            Console.WriteLine($"Saving a new {typeName}: {valueToInsert}");
            db.SetConfig(key, category, valueToInsert);
            Console.WriteLine($"Current {typeName}: {db.ReadConfig(key, category, defaultValue)}");
        }
    }
}
