namespace Simple.Sqlite;

using System;

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
    [Obsolete("Not supported on STRICT tables")]
    NUMERIC,
    /// <summary>
    /// Accepts any kind of data
    /// </summary>
    ANY,
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

public enum JournalMode
{
    /// <summary>
    /// The default mode. Rollback journals are deleted after each transaction
    /// </summary>
    DELETE,
    /// <summary>
    /// Write-Ahead Logging. Changes are written to a separate -wal file instead of the main database
    /// </summary>
    WAL,
    /// <summary>
    /// Similar to DELETE, but truncates the journal file to zero length instead of deleting it
    /// </summary>
    TRUNCATE,
    /// <summary>
    /// Overwrites the journal header with zeros but leaves the file on disk
    /// </summary>
    PERSIST,
    /// <summary>
    /// Stores the rollback journal in RAM rather than on disk
    /// </summary>
    MEMORY,
    /// <summary>
    /// Disables the rollback journal entirely
    /// </summary>
    OFF
}

public enum SynchronousMode
{
    /// <summary>
    /// With synchronous OFF (0), SQLite continues without syncing as soon as it has handed data off to the operating system. If the application running SQLite crashes, the data will be safe, but the database might become corrupted if the operating system crashes or the computer loses power before that data has been written to non-volatile storage. On the other hand, commits can be much faster with synchronous OFF.
    /// Setting synchronous to OFF is a good option when creating a new database from scratch, in a scenario where the process of creating the database can be repeated if a power loss occurs in the middle, and when performance is critical.
    /// </summary>
    OFF = 0,
    /// <summary>
    /// When synchronous is NORMAL (1), the SQLite database engine will still sync at the most critical moments, but less often than in FULL mode. There is a very small (though non-zero) chance that a power failure at just the wrong time could corrupt the database in journal_mode=DELETE on an older filesystem. WAL mode is safe from corruption with synchronous=NORMAL, and probably DELETE mode is safe too on modern filesystems. WAL mode is always consistent with synchronous=NORMAL, but WAL mode does lose durability. A transaction committed in WAL mode with synchronous=NORMAL might roll back following a power loss or system crash. Transactions are durable across application crashes regardless of the synchronous setting or journal mode.
    /// The synchronous = NORMAL setting provides the best balance between performance and safety for most applications running in WAL mode. You lose durability across power lose with synchronous NORMAL in WAL mode, but that is not important for most applications. Transactions are still atomic, consistent, and isolated, which are the most important characteristics in most use cases.
    /// </summary>
    NORMAL = 1,
    /// <summary>
    /// When synchronous is FULL (2), the SQLite database engine will use the xSync method of the VFS to ensure that all content is safely written to the disk surface prior to continuing. This ensures that an operating system crash or power failure will not corrupt the database.
    /// FULL is the default synchronous mode for a rollback journal. FULL is atomic, consistent, isolated, and durable (ACID) in WAL mode and is atomic, consistent, and isolated with a rollback journal. FULL might also be durable using a rollback journal, depending on the underlying filesystem.FULL is not necessarily durable across a power loss in rollback mode, so if durability is desired, it is best to set the synchronous mode to EXTRA.
    /// </summary>
    FULL = 2,
    /// <summary>
    /// EXTRA synchronous is like FULL with the addition that the directory containing a rollback journal is synced after that journal is unlinked to commit a transaction in DELETE mode. EXTRA provides additional durability if the commit is followed closely by a power loss. Without EXTRA, depending on the underlying filesystem, it is possible that a single transaction that commits right before a power loss might get rolled back upon reboot. The database will not go corrupt. But the last transaction might go missing, thus violating durability, if EXTRA is not set.
    /// EXTRA is no different from FULL in WAL mode.
    /// </summary>
    EXTRA = 3,
}