namespace Simple.Sqlite
{
    /// <summary>
    /// Represents a Sqlite internal type
    /// </summary>
    public enum SqliteType
    {
        /// <summary>
        /// Numeric without decimal places
        /// </summary>
        INTEGER,
        /// <summary>
        /// Sqlite text
        /// </summary>
        TEXT,
        /// <summary>
        /// Binary data or anythng (sqlite is dynamic typed)
        /// </summary>
        BLOB,
        /// <summary>
        /// Numeric with decimal places
        /// </summary>
        REAL,
        /// <summary>
        /// Numeric value. Includes Bool, Date and Datetime
        /// </summary>
        NUMERIC,
    }
}
