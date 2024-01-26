using ADatabaseMigrator.Core;

namespace ADatabaseMigrator
{
    public class Migrator : MigratorBase<Migration, MigrationScript, Journal>
    {
        public Migrator(
            IMigrationScriptLoader<MigrationScript> scriptLoader,
            IMigrationsJournalLoader<Journal, Migration> journalLoader,
            IMigrationScriptRunner<MigrationScript> scriptRunner) : base(scriptLoader, journalLoader, scriptRunner) { }

        protected override bool ShouldRunScript(MigrationScript script, Journal journal) => script switch
        {
            { RunType: MigrationScriptRunType.RunAlways } => true,
            { RunType: MigrationScriptRunType.RunIfChanged } => journal.HasChanged(script),
            { RunType: MigrationScriptRunType.RunOnce } => journal.Contains(script) is false,
            _ => throw new System.Exception($"Migration script {script.Name} has unknown runtype '{script.RunType}'")
        };
    }
}
