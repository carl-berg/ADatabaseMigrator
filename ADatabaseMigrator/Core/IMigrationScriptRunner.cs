using System.Threading;
using System.Threading.Tasks;

namespace ADatabaseMigrator.Core;

public interface IMigrationScriptRunner<TMigrationScript> where TMigrationScript : IMigrationScript
{
    /// <summary>
    /// Run migration script and append journal
    /// </summary>
    Task Run(TMigrationScript script, string appendJournalScript, CancellationToken cancellationToken);
}
