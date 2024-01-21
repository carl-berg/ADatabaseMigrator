using ADatabaseMigrator.Core;

namespace ADatabaseMigrator
{
    public class Migration : IMigration
    {
        public Migration(string id, MigrationScriptRunType runType, string version, string? scriptHash)
        {
            Id = id;
            Version = version;
            RunType = runType;
            ScriptHash = scriptHash;
        }

        /// <inheritdoc/>
        public string Id { get; }

        /// <summary>
        /// Script version
        /// </summary>
        public string Version { get; }

        public MigrationScriptRunType RunType { get; }

        /// <summary>
        /// Hash string that can be used to determine if two scripts have identical script content
        /// </summary>
        public string? ScriptHash { get; }
    }
}
