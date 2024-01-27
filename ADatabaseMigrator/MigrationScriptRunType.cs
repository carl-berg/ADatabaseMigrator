namespace ADatabaseMigrator;

public enum MigrationScriptRunType
{
    RunOnce = 1,
    RunIfChanged = 2,
    RunAlways = 3,
}
