using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Simple.Sqlite
{
    /// <summary>
    /// Represents a easy to use configuration set
    /// </summary>
    public class ConfigurationDB
    {
        /// <summary>
        /// Exposes the internal database engine
        /// </summary>
        internal protected SqliteDB internalDb;

        /// <summary>
        /// Database file full path
        /// </summary>
        public string DatabaseFileName => internalDb.DatabaseFileName;

        /// <summary>
        /// Creates a new instance
        /// </summary>
        public ConfigurationDB(string fileName)
            : this(new SqliteDB(fileName))
        { }
        private ConfigurationDB(SqliteDB internalDb)
        {
            this.internalDb = internalDb;
            createConfigTable();
        }

        private void createConfigTable()
        {
            internalDb.Execute(
@"CREATE TABLE IF NOT EXISTS nsConfig (
    Id         TEXT NOT NULL,
    Value    BLOB,
    PRIMARY KEY(Id)
);");
        }
        /// <summary>
        /// Sets a configuration value
        /// </summary>
        /// <typeparam name="T">Type of the configuration value</typeparam>
        /// <param name="ConfigKey">Key to locate the configration in the category</param>
        /// <param name="ConfigCategory">Category of the configuration</param>
        /// <param name="Value">Value to be stored</param>
        public void SetConfig<T>(string ConfigKey, string ConfigCategory, T Value)
        {
            internalDb.InsertOrReplace(new nsConfig()
            {
                Id = buildKey(ConfigKey, ConfigCategory),
                Value = Value
            });
        }

        /// <summary>
        /// [DEPRECATED] Use 'GetConfig' instead
        /// </summary>
        [Obsolete("Use 'GetConfig' instead")]
        public T ReadConfig<T>(string ConfigKey, string ConfigCategory, T Default) => GetConfig(ConfigKey, ConfigCategory, Default);
        /// <summary>
        /// Gets a configuration value
        /// </summary>
        /// <typeparam name="T">Type of the configuration value</typeparam>
        /// <param name="ConfigKey">Key to locate the configration in the category</param>
        /// <param name="ConfigCategory">Category of the configuration</param>
        /// <param name="Default">Default value if none exists</param>
        /// <returns>Saved value</returns>
        public T GetConfig<T>(string ConfigKey, string ConfigCategory, T Default)
        {
            var values = internalDb.Query<T>("SELECT Value FROM nsConfig WHERE Id = @id",
                                                   new { id = buildKey(ConfigKey, ConfigCategory) })
                                   .ToArray();
            if (values.Length == 0) return Default;
            return values[0];
        }
        /// <summary>
        /// Remove a stored item
        /// </summary>
        /// <param name="ConfigKey">Key to locate the configration in the category</param>
        /// <param name="ConfigCategory">Category of the configuration</param>
        public void RemoveConfig(string ConfigKey, string ConfigCategory)
        {
            internalDb.Execute("DELETE FROM nsConfig WHERE Id = @id",
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

        /// <summary>
        /// Create a new instance based on an existing ConfigurationDB
        /// </summary>
        public static ConfigurationDB FromDB(ConfigurationDB cfg)
        {
            return new ConfigurationDB(cfg.internalDb);
        }
#if !NET40
        /// <summary>
        /// Create a new instance based on an existing NoSqliteStorage
        /// </summary>
        public static ConfigurationDB FromDB(NoSqliteStorage nss)
        {
            return new ConfigurationDB(nss.internalDb);
        }
#endif
        /// <summary>
        /// Create a new instance based on an existing SqliteDB
        /// </summary>
        public static ConfigurationDB FromDB(SqliteDB db)
        {
            return new ConfigurationDB(db);
        }
    }
}
