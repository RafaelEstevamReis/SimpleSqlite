namespace Simple.Sqlite;

public class SqliteTableInfo
{
    /// <summary>
    /// Column Id
    /// </summary>
    public long cid { get; set; }
    /// <summary>
    /// Column Name
    /// </summary>
    public string name { get; set; } = string.Empty;
    /// <summary>
    /// Column data type if given, else ''
    /// </summary>
    public string type { get; set; } = string.Empty;
    /// <summary>
    /// Whether or not the column can be NULL
    /// </summary>
    public bool notnull { get; set; }
    /// <summary>
    /// The default value for the column
    /// </summary>
    public string? dflt_value { get; set; }
    /// <summary>
    /// Either zero for columns that are not part of the primary key, or the 1-based index of the column within the primary key
    /// </summary>
    public int pk {  get; set; }
}
