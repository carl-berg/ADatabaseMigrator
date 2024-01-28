namespace ADatabaseMigrator.Core;

public interface IMigrationJournalManager<TMigrationJournal, TMigration, TMigrationScript> :
    IMigrationJournalLoader<TMigrationJournal, TMigration>,
    IMigrationJournalAppender<TMigrationScript>
        where TMigrationJournal : IMigrationJournal<TMigration>
        where TMigration : IMigration
        where TMigrationScript : IMigrationScript
{
}
