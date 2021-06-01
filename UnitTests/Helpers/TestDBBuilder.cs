using Simple.Sqlite;

namespace UnitTests.Helpers
{
    public static class TestDBBuilder
    {
        public static readonly string IN_MEMORY = ":memory:";

        public static SqliteDB Create() => new SqliteDB(IN_MEMORY);
    }
}
