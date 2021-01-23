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

            var myFloat = 3.14f;
            showExample<float>(db, "Float", "MyName", "General.UI", myFloat, 0);

            var myDate = DateTime.Now;
            showExample<DateTime>(db, "DateTime", "myDate", "General.UI", myDate, DateTime.MinValue);

            var myColor = System.Drawing.Color.FromArgb(101, 102, 103, 104);
            showExample<System.Drawing.Color>(db, "Color", "myColor", "General.UI", myColor, System.Drawing.Color.FromArgb(0, 0, 0, 0));

            var myTimespan = TimeSpan.FromSeconds(1234);
            showExample<TimeSpan>(db, "TimeSpan", "myTimespan", "General.UI", myTimespan, TimeSpan.Zero);

            var mByteArray = new byte[]{ 101, 102, 103, 104 };
            showExample<byte[]>(db, "Byte[]", "mByteArray", "General.UI", mByteArray, new byte[0]);
        }

        private static void showExample<T>(ConfigurationDB db, string typeName, string key, string category, T valueToInsert, T defaultValue)
        {
            // force delete due to multiple executions
            db.RemoveConfig(key, category);
            Console.WriteLine($"---{typeName} example---");
            Console.WriteLine($"Current {typeName}: {db.GetConfig(key, category, defaultValue)}");
            Console.WriteLine($"Saving a new {typeName}: {valueToInsert}");
            db.SetConfig(key, category, valueToInsert);
            Console.WriteLine($"Current {typeName}: {db.GetConfig(key, category, defaultValue)}");
        }
    }
}
