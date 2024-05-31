using ADatabaseMigrator.Core.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static ADatabaseMigrator.MigrationScriptRunner;

namespace ADatabaseMigrator.Core;

public abstract class MigratorBase<TMigration, TMigrationScript, TMigrationJournal>(
    IMigrationScriptLoader<TMigrationScript> scriptLoader,
    IMigrationJournalManager<TMigrationJournal, TMigration, TMigrationScript> journalManager,
    IMigrationScriptRunner<TMigrationScript> scriptRunner)
    where TMigration : IMigration
    where TMigrationScript : IMigrationScript
    where TMigrationJournal : IMigrationJournal<TMigration>
{
    public event EventHandler<ScriptMigrationSucceededEventArgs<TMigrationScript>>? ScriptMigrationSucceeded;
    public event EventHandler<ScriptMigrationFailedEventArgs<TMigrationScript>>? ScriptMigrationFailed;

    protected IMigrationScriptLoader<TMigrationScript> ScriptLoader { get; } = scriptLoader;
    protected IMigrationJournalManager<TMigrationJournal, TMigration, TMigrationScript> JournalManager { get; } = journalManager;
    protected IMigrationScriptRunner<TMigrationScript> ScriptRunner { get; } = scriptRunner;

    public virtual async Task<IReadOnlyList<TMigrationScript>> Migrate(CancellationToken? cancellationToken = default)
    {
        var allMigrations = await ScriptLoader.Load(cancellationToken);
        var journal = await JournalManager.Load(cancellationToken);
        var scriptsToRun = allMigrations.Where(x => ShouldRunScript(x, journal)).ToList();
        await RunScripts(scriptsToRun, cancellationToken);
        return scriptsToRun;
    }

    protected abstract bool ShouldRunScript(TMigrationScript script, TMigrationJournal journal);

    protected virtual async Task RunScripts(IEnumerable<TMigrationScript> migrationScripts, CancellationToken? cancellationToken = default)
    {
        foreach (var migrationScript in migrationScripts)
        {
            var appendJournalScript = JournalManager.AddJournalScript(migrationScript);
            try
            {
                await ScriptRunner.Run(migrationScript, appendJournalScript, cancellationToken);
                ScriptMigrationSucceeded?.Invoke(this, new ScriptMigrationSucceededEventArgs<TMigrationScript>(migrationScript));
            }
            catch (ScriptExecutionException ex)
            {
                ScriptMigrationFailed?.Invoke(this, new ScriptMigrationFailedEventArgs<TMigrationScript>(migrationScript, ex));
                throw;
            }
        }
    }


}
