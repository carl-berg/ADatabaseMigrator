using ADatabaseMigrator.Core;

namespace ADatabaseMigrator
{
    public class MigrationScript : Migration, IMigrationScript
    {
        public MigrationScript(string id, string fileName, MigrationScriptRunType runType, string version, string script, string scriptHash) : base(id, runType, version, scriptHash)
        {
            FileName = fileName;
            Script = script;
        }

        public string FileName { get; }

        /// <inheritdoc/>
        public string Script { get; }
    }
}
