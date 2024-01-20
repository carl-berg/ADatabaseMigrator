using System.Threading.Tasks;

namespace ADatabaseMigrator.Core
{
    public interface IMigrationsJournalLoader<TMigrationJournal, TMigration>
        where TMigrationJournal : MigrationJournal<TMigration>
        where TMigration : IMigration
    {
        /// <summary>
        /// Load migration journal (information about already migrated scripts)
        /// </summary>
        Task<TMigrationJournal> Load();
    }
}
