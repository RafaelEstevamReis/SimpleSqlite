using Simple.DatabaseWrapper.Helpers;
using System.Collections.Generic;

namespace Simple.Sqlite.Extension
{
    public static class QueryExtension
    {
        public static IEnumerable<T> Query<T>(this ISqliteConnection connection, string text, object parameters)
        {
            var typeT = typeof(T);
            using var cmd = connection.connection.CreateCommand();

            cmd.CommandText = text;
            HelperFunctions.fillParameters(cmd, parameters, connection.typeCollection);

            using var reader = cmd.ExecuteReader();

            if (!reader.HasRows)
            {
                yield break;
            }

            var colNames = HelperFunctions.getSchemaColumns(reader);
            bool isSimple = typeT.CheckIfSimpleType();
            while (reader.Read())
            {
                // build new
                if (isSimple)
                {
                    yield return (T)TypeMapper.ReadValue(reader, typeT, 0);
                }
                else
                {
                    yield return TypeMapper.MapObject<T>(colNames, reader, connection.typeCollection);
                }
            }
        }
    }
}
