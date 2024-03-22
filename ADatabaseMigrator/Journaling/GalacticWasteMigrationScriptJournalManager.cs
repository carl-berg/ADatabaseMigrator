using System;
using System.Data.Common;

namespace ADatabaseMigrator.Journaling;

/// <summary>
/// Migration script journal manager compatible with GalacticWasteManagement. 
/// Use this JournalManager if you are have a database with existing GWM migrations.
/// If starting from scratch, just use <see cref="MigrationScriptJournalManager"/>.
/// </summary>
public class GalacticWasteMigrationScriptJournalManager(DbConnection connection) : MigrationScriptJournalManager(connection)
{
    protected override Migration ParseMigration(string version, string name, DateTime applied, string scriptHash, string runType)
        => base.ParseMigration(version, name, applied, scriptHash, ParseGalacticWasteRunType(runType));

    protected virtual string ParseGalacticWasteRunType(string runType) => runType switch
    {
        "Migration" => nameof(MigrationScriptRunType.RunOnce),
        _ => runType
    };
}
