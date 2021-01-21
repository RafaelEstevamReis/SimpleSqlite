using Simple.Sqlite.Attributes;

namespace Simple.Sqlite
{
    internal class nsConfig
    {
        [PrimaryKey]
        public string Id { get; set; }

        public object Value { get; set; }

    }
}
