using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Sqlite
{
    public class ConfigurationDB
    {
        private SqliteDB internalDb;
        /// <summary>
        /// Database file full path
        /// </summary>
        public string DatabaseFileName => internalDb.DatabaseFileName;
        /// <summary>
        /// Creates a new instance
        /// </summary>
        public ConfigurationDB(string fileName)
        {
            internalDb = new SqliteDB(fileName);
            createConfigTable();
        }

        private void createConfigTable()
        {
            internalDb.ExecuteNonQuery(
@"CREATE TABLE IF NOT EXISTS nsConfig (
    Id         TEXT NOT NULL,
    Value    BLOB,
    PRIMARY KEY(Id)
);");
        }

        public void SetConfig<T>(string ConfigKey, string ConfigCategory, T Value)
        {
            internalDb.InsertOrReplace(new nsConfig()
            {
                Id = buildKey(ConfigKey, ConfigCategory),
                Value = Value
            });
        }

        public T ReadConfig<T>(string ConfigKey, string ConfigCategory, T Default)
        {
            var values = internalDb.ExecuteQuery<T>("SELECT Value FROM nsConfig WHERE Id = @id",
                                                   new { id = buildKey(ConfigKey, ConfigCategory) })
                                   .ToArray();
            if (values.Length == 0) return Default;
            return values[0];
        }

        public void RemoveConfig(string ConfigKey, string ConfigCategory)
        {
            internalDb.ExecuteNonQuery("DELETE FROM nsConfig WHERE Id = @id",
                                       new { id = buildKey(ConfigKey, ConfigCategory) });
        }

        private static string buildKey(string ConfigKey, string ConfigCategory)
        {
            if (string.IsNullOrEmpty(ConfigKey))
            {
                throw new ArgumentException($"'{nameof(ConfigKey)}' cannot be null or empty", nameof(ConfigKey));
            }

            if (ConfigCategory is null)
            {
                ConfigCategory = string.Empty;
            }

            return $"{ConfigCategory}::{ConfigKey}";
        }

    }
}
