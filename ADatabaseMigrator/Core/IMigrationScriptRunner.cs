using System.Threading;
using System.Threading.Tasks;

namespace ADatabaseMigrator.Core
{
    public interface IMigrationScriptRunner<TMigrationScript> where TMigrationScript : IMigrationScript
    {
        /// <summary>
        /// Run migration script and update journal
        /// </summary>
        Task Run(TMigrationScript script, CancellationToken cancellationToken);
    }
}
