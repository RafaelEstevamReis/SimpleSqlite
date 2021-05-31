using Simple.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace UnitTests.Helpers
{
    public static class TestDBBuilder
    {
        public static readonly string IN_MEMORY = "FullUri=file:instance.db?mode=memory&cache=shared";

        public static SqliteDB Create() => new SqliteDB(":memory:");
    }
}
