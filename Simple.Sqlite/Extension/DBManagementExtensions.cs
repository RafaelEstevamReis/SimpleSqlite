namespace Simple.Sqlite;

using System.Linq;

/// <summary>
/// Extension for managing a database
/// </summary>
public static class DBManagementExtensions
{
    /// <summary>
    /// Vacuums current database
    /// </summary>
    public static void Vacuum(this ISqliteConnection source)
    {
        source.Execute("VACUUM");
    }
    /// <summary>
    /// Vacuums current database into a New File
    /// </summary>
    /// <param name="source">Source sqlite connection to be used</param>
    /// <param name="destinationFilePath">New File to be Vacuum to. If a file already exists the command will fail with an error</param>
    public static void VacuumIntoFile(this ISqliteConnection source, string destinationFilePath)
    {
        source.Execute($"VACUUM INTO '{destinationFilePath}'");
    }

    /// <summary>
    /// Sets current database Journal mode
    /// </summary>
    public static void SetJournalMode(this ISqliteConnection source, JournalMode journalMode)
    {
        source.Execute($"PRAGMA journal_mode = {journalMode};");
    }

    /// <summary>
    /// Sets current database Synchronous mode
    /// </summary>
    public static void SetSchemaSynchronous(this ISqliteConnection source, SynchronousMode synchronousMode)
    {
        source.Execute($"PRAGMA synchronous = {synchronousMode};");
    }

    public static void Optimize(this ISqliteConnection source)
    {
        source.Execute("PRAGMA optimize;");
    }

    /// <summary>
    /// Checks current database integrity
    /// </summary>
    public static string[] IntegrityCheck(this ISqliteConnection source)
    {
        return source.Query<string>("PRAGMA integrity_check;").ToArray();
    }

    /// <summary>
    /// Checks quickly current database integrity. 
    /// Uniqueness and index matching are not checked.
    /// </summary>
    public static string[] IntegrityCheckQuick(this ISqliteConnection source)
    {
        return source.Query<string>("PRAGMA quick_check;").ToArray();
    }

}
