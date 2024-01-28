using ADatabaseMigrator.Core;
using System;

namespace ADatabaseMigrator;

public class Migration : IMigration
{
    public Migration(string name, MigrationScriptRunType runType, IComparable version, DateTime applied, string scriptHash)
    {
        Name = name;
        Version = version;
        RunType = runType;
        Applied = applied;
        ScriptHash = scriptHash;
    }

    /// <inheritdoc/>
    public string Name { get; }

    /// <summary>
    /// Script version
    /// </summary>
    public IComparable Version { get; }

    /// <summary>
    /// Applied timestamp (UTC)
    /// </summary>
    public DateTime Applied { get; }

    public MigrationScriptRunType RunType { get; }

    /// <summary>
    /// Hash string that can be used to determine if two scripts have identical script content
    /// </summary>
    public string ScriptHash { get; }
}
