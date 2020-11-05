namespace Simple.Sqlite
{
    public enum SqliteType
    {
        INTEGER,
        TEXT,
        BLOB,
        REAL,
        NUMERIC, // Include Bool, Date and Datetime
    }
    public enum SqliteCollate
    {
        None, //Omitted
        Binary,
        Nocase,
        RTtrim,
    }
}
