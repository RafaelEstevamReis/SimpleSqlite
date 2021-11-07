using System;

namespace Simple.Sqlite.Extension
{
    public static class ExecuteExtension
    {
        public static int Execute(this ISqliteConnection connection, string text, object parameters = null)
        {
            using var cmd = connection.connection.CreateCommand();

            cmd.CommandText = text;
            HelperFunctions.fillParameters(cmd, parameters, connection.typeCollection);

            return cmd.ExecuteNonQuery();
        }

        public static T ExecuteScalar<T>(this ISqliteConnection connection, string text, object parameters = null)
        {
            using var cmd = connection.connection.CreateCommand();

            cmd.CommandText = text;
            HelperFunctions.fillParameters(cmd, parameters, connection.typeCollection);

            var obj = cmd.ExecuteScalar();

            // In SQLite DateTime is returned as STRING after aggregate operations
            if (typeof(T) == typeof(DateTime))
            {
                if (DateTime.TryParse(obj.ToString(), out DateTime dt))
                {
                    return (T)(object)dt;
                }
                return default;
            }
            return (T)Convert.ChangeType(obj, typeof(T));
        }
    }
}
