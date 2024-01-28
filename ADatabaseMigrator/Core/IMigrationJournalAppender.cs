namespace ADatabaseMigrator.Core;

public interface IMigrationJournalAppender<TMigrationScript>
    where TMigrationScript : IMigrationScript
{
    /// <summary>
    /// Creates a script to add journal
    /// </summary>
    string AddJournalScript(TMigrationScript script);
}
