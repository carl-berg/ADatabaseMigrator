using ADatabaseMigrator.Core;
using System;

namespace ADatabaseMigrator;

public class Migration : IMigration
{
    public Migration(string name, MigrationScriptRunType runType, IComparable version, string scriptHash)
    {
        Name = name;
        Version = version;
        RunType = runType;
        ScriptHash = scriptHash;
    }

    /// <inheritdoc/>
    public string Name { get; }

    /// <summary>
    /// Script version
    /// </summary>
    public IComparable Version { get; }

    public MigrationScriptRunType RunType { get; }

    /// <summary>
    /// Hash string that can be used to determine if two scripts have identical script content
    /// </summary>
    public string ScriptHash { get; }
}
