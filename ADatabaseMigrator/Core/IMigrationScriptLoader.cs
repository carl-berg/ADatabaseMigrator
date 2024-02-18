using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ADatabaseMigrator.Core;

public interface IMigrationScriptLoader<TMigrationScript> where TMigrationScript : IMigrationScript
{
    /// <summary>
    /// Load all migration scripts
    /// </summary>
    Task<IReadOnlyList<TMigrationScript>> Load(CancellationToken? cancellationToken = default);
}
