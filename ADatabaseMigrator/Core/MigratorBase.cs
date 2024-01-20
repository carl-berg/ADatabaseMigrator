using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ADatabaseMigrator.Core
{
    public abstract class MigratorBase<TMigration, TMigrationScript, TMigrationJournal>
        where TMigration : IMigration
        where TMigrationScript : IMigrationScript
        where TMigrationJournal : MigrationJournal<TMigration>
    {
        public MigratorBase(
            IMigrationScriptLoader<TMigrationScript> scriptLoader,
            IMigrationsJournalLoader<TMigrationJournal, TMigration> journalLoader,
            IMigrationScriptRunner<TMigrationScript> scriptRunner)
        {
            ScriptLoader = scriptLoader;
            JournalLoader = journalLoader;
            ScriptRunner = scriptRunner;
        }

        protected IMigrationScriptLoader<TMigrationScript> ScriptLoader { get; }
        protected IMigrationsJournalLoader<TMigrationJournal, TMigration> JournalLoader { get; }
        protected IMigrationScriptRunner<TMigrationScript> ScriptRunner { get; }

        public virtual async Task Migrate(CancellationToken cancellationToken)
        {
            var allMigrations = await ScriptLoader.Load();
            var journal = await JournalLoader.Load();
            var scriptsToRun = allMigrations.Where(x => ShouldRunScript(x, journal));
            await RunScripts(scriptsToRun, cancellationToken);
        }

        protected abstract bool ShouldRunScript(TMigrationScript script, TMigrationJournal journal);

        protected virtual async Task RunScripts(IEnumerable<TMigrationScript> scripts, CancellationToken cancellationToken)
        {
            foreach (var script in scripts)
            {
                await ScriptRunner.Run(script, cancellationToken);
            }
        }
    }
}
