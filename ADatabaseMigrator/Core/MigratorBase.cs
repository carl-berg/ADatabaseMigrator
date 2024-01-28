using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ADatabaseMigrator.Core;

public abstract class MigratorBase<TMigration, TMigrationScript, TMigrationJournal>
    where TMigration : IMigration
    where TMigrationScript : IMigrationScript
    where TMigrationJournal : IMigrationJournal<TMigration>
{
    public MigratorBase(
        IMigrationScriptLoader<TMigrationScript> scriptLoader,
        IMigrationJournalManager<TMigrationJournal, TMigration, TMigrationScript> journalManager,
        IMigrationScriptRunner<TMigrationScript> scriptRunner)
    {
        ScriptLoader = scriptLoader;
        JournalManager = journalManager;
        ScriptRunner = scriptRunner;
    }

    protected IMigrationScriptLoader<TMigrationScript> ScriptLoader { get; }
    protected IMigrationJournalManager<TMigrationJournal, TMigration, TMigrationScript> JournalManager { get; }
    protected IMigrationScriptRunner<TMigrationScript> ScriptRunner { get; }

    public virtual async Task Migrate(CancellationToken cancellationToken)
    {
        var allMigrations = await ScriptLoader.Load();
        var journal = await JournalManager.Load();
        var scriptsToRun = allMigrations.Where(x => ShouldRunScript(x, journal));
        await RunScripts(scriptsToRun, cancellationToken);
    }

    protected abstract bool ShouldRunScript(TMigrationScript script, TMigrationJournal journal);

    protected virtual async Task RunScripts(IEnumerable<TMigrationScript> migrationScripts, CancellationToken cancellationToken)
    {
        foreach (var migrationScript in migrationScripts)
        {
            var appendJournalScript = JournalManager.AddJournalScript(migrationScript);
            await ScriptRunner.Run(migrationScript, appendJournalScript, cancellationToken);
        }
    }
}
