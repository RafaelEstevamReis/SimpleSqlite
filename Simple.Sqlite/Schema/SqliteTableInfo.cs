namespace Simple.Sqlite;

public class SqliteTableInfo
{
    /// <summary>
    /// Column Id
    /// </summary>
    public long Cid { get; set; }
    /// <summary>
    /// Column Name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    /// <summary>
    /// Column data type if given, else ''
    /// </summary>
    public string Type { get; set; } = string.Empty;
    /// <summary>
    /// Whether or not the column can be NULL
    /// </summary>
    public bool NotNull { get; set; }
    /// <summary>
    /// The default value for the column
    /// </summary>
    public string? Dflt_value { get; set; }
    /// <summary>
    /// Either zero for columns that are not part of the primary key, or the 1-based index of the column within the primary key
    /// </summary>
    public int Pk {  get; set; }
}
