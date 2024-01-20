using ADatabaseMigrator.Core;

namespace ADataMigrator.GalacticWaste
{
    public class Migration : IMigration
    {
        public Migration(string id, MigrationScriptRunType runType, string? scriptHash)
        {
            Id = id;
            RunType = runType;
            ScriptHash = scriptHash;
        }

        /// <inheritdoc/>
        public string Id { get; }

        public MigrationScriptRunType RunType { get; }

        /// <summary>
        /// Hash string that can be used to determine if two scripts have identical script content
        /// </summary>
        public string? ScriptHash { get; }
    }
}
