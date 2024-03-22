using System.Threading;
using System.Threading.Tasks;

namespace ADatabaseMigrator.Core;

public interface IMigrationJournalLoader<TMigrationJournal, TMigration>
    where TMigrationJournal : IMigrationJournal<TMigration>
    where TMigration : IMigration
{
    /// <summary>
    /// Load migration journal (information about already migrated scripts)
    /// </summary>
    Task<TMigrationJournal> Load(CancellationToken? cancellationToken = default);
}
