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
    /// <summary>
    /// OnConflict clause options
    /// </summary>
    public enum OnConflict
    {
        /// <summary>
        /// When an applicable constraint violation occurs, the ROLLBACK resolution algorithm aborts the current SQL statement 
        /// with an SQLITE_CONSTRAINT error and rolls back the current transaction. 
        /// If no transaction is active (other than the implied transaction that is created on every command) then the ROLLBACK 
        /// resolution algorithm works the same as the ABORT algorithm
        /// </summary>
        RollBack,
        /// <summary>
        /// When an applicable constraint violation occurs, 
        /// the ABORT resolution algorithm aborts the current SQL statement with an SQLITE_CONSTRAINT error and backs out any 
        /// changes made by the current SQL statement; 
        /// but changes caused by prior SQL statements within the same transaction are preserved and the transaction remains active. 
        /// This is the default behavior and the behavior specified by the SQL standard
        /// </summary>
        Abort,
        /// <summary>
        /// When an applicable constraint violation occurs, the FAIL resolution algorithm aborts the current SQL statement with an SQLITE_CONSTRAINT error. 
        /// But the FAIL resolution does not back out prior changes of the SQL statement that failed nor does it end the transaction. 
        /// For example, if an UPDATE statement encountered a constraint violation on the 100th row that it attempts to update, 
        /// then the first 99 row changes are preserved but changes to rows 100 and beyond never occur.
        /// The FAIL behavior only works for uniqueness, NOT NULL, and CHECK constraints. A foreign key constraint violation causes an ABORT
        /// </summary>
        Fail,
        /// <summary>
        /// When an applicable constraint violation occurs, 
        /// the IGNORE resolution algorithm skips the one row that contains the constraint violation and continues processing 
        /// subsequent rows of the SQL statement as if nothing went wrong. 
        /// Other rows before and after the row that contained the constraint violation are inserted or updated normally. 
        /// No error is returned for uniqueness, NOT NULL, and UNIQUE constraint errors when the IGNORE conflict resolution algorithm is used. 
        /// However, the IGNORE conflict resolution algorithm works like ABORT for foreign key constraint errors
        /// </summary>
        Ignore,
        /// <summary>
        /// When a UNIQUE or PRIMARY KEY constraint violation occurs, 
        /// the REPLACE algorithm deletes pre-existing rows that are causing the constraint violation prior to inserting or 
        /// updating the current row and the command continues executing normally. 
        /// If a NOT NULL constraint violation occurs, the REPLACE conflict resolution replaces the NULL value with the default value for that column, 
        /// or if the column has no default value, then the ABORT algorithm is used. If a CHECK constraint or foreign key constraint violation occurs, 
        /// the REPLACE conflict resolution algorithm works like ABORT
        /// </summary>
        Replace
    }
}
