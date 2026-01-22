namespace Simple.Sqlite;

using Simple.DatabaseWrapper.Attributes;

internal class nsConfig
{
    [PrimaryKey]
    public string Id { get; set; } = default!;

    public object? Value { get; set; }

}
