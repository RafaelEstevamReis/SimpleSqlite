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

public class SqliteTableList
{
    /// <summary>
    /// the schema in which the table or view appears (for example "main" or "temp")
    /// </summary>
    public string schema { get; set; } = string.Empty;
    /// <summary>
    /// the name of the table or view
    /// </summary>
    public string name { get; set; } = string.Empty;
    /// <summary>
    /// the type of object - one of "table", "view", "shadow" (for shadow tables), or "virtual" for virtual tables
    /// </summary>
    public string type { get; set; } = string.Empty;
    /// <summary>
    /// the number of columns in the table, including generated columns and hidden columns
    /// </summary>
    public long ncol { get; set; }
    /// <summary>
    /// if the table is a WITHOUT ROWID table or 0 if is not
    /// </summary>
    public bool wr { get; set; }
    /// <summary>
    /// 1 if the table is a STRICT table or 0 if it is not
    /// </summary>
    public bool strict { get; set; }

}