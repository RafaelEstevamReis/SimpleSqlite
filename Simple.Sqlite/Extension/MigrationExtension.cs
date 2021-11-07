using Simple.DatabaseWrapper.Interfaces;

namespace Simple.Sqlite.Extension
{
    public static class MigrationExtension
    {
        public static ITableMapper CreateTables(this ISqliteConnection Connection) => new TableMapperNew(Connection);
    }
}
