namespace Simple.Sqlite.Extension;

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

}
