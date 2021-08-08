using Simple.Sqlite;
using Xunit;

namespace UnitTest.ConfigTests
{
    public class BaseTests
    {
        [Fact]
        public void Config_InMemory_BaseTests()
        {
            var memDb = SqliteDB.CreateInMemory();
            var cfg = ConfigurationDB.FromDB(memDb);

            Assert.Equal(":Empty:", cfg.GetConfig("test", "cat", ":Empty:"));

            cfg.SetConfig("test", "cat", "myValue");
            Assert.Equal("myValue", cfg.GetConfig("test", "cat", ":Empty:"));
        }

        [Fact]
        public void Config_InMemory_Nulls()
        {
            var memDb = SqliteDB.CreateInMemory();
            var cfg = ConfigurationDB.FromDB(memDb);

            Assert.Equal(":Empty:", cfg.GetConfig("null", "cat", ":Empty:"));

            cfg.SetConfig<string>("null", "cat", null);
            Assert.Null(cfg.GetConfig("null", "cat", ":Empty:"));
        }
    }
}
