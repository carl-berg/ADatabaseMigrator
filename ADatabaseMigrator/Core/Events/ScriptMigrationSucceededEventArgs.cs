using System;

namespace ADatabaseMigrator.Core.Events;

public class ScriptMigrationSucceededEventArgs<TMigrationScript>(TMigrationScript script) : EventArgs where TMigrationScript : IMigrationScript
{
    public TMigrationScript Script { get; } = script;
}
