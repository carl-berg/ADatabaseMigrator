using ADatabaseMigrator.Core;

namespace ADatabaseMigrator;

public class Migrator : MigratorBase<Migration, MigrationScript, MigrationJournal>
{
    public Migrator(
        IMigrationScriptLoader<MigrationScript> scriptLoader,
        IMigrationJournalManager<MigrationJournal, Migration, MigrationScript> journalManager,
        IMigrationScriptRunner<MigrationScript> scriptRunner) : base(scriptLoader, journalManager, scriptRunner) { }

    protected override bool ShouldRunScript(MigrationScript script, MigrationJournal journal) => script switch
    {
        { RunType: MigrationScriptRunType.RunAlways } => true,
        { RunType: MigrationScriptRunType.RunIfChanged } => journal.HasChanged(script),
        { RunType: MigrationScriptRunType.RunOnce } => journal.Contains(script) is false,
        _ => false
    };
}
