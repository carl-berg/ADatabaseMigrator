using System;
using static ADatabaseMigrator.MigrationScriptRunner;

namespace ADatabaseMigrator.Core.Events;

public class ScriptMigrationFailedEventArgs<TMigrationScript>(TMigrationScript script, ScriptExecutionException exception) : EventArgs where TMigrationScript : IMigrationScript
{
    public TMigrationScript Script { get; } = script;
    public ScriptExecutionException Exception { get; } = exception;
}
