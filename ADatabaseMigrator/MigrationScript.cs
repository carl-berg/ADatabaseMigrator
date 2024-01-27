using ADatabaseMigrator.Core;
using System;

namespace ADatabaseMigrator;

public class MigrationScript : IMigrationScript
{
    public MigrationScript(string name, MigrationScriptRunType runType, IComparable version, string script, string scriptHash)
    {
        Name = name;
        RunType = runType;
        Version = version;
        Script = script;
        ScriptHash = scriptHash;
    }

    /// <inheritdoc/>
    public string Script { get; }
    public string ScriptHash { get; }
    public string Name { get; }
    public MigrationScriptRunType RunType { get; }
    public IComparable Version { get; }
}
