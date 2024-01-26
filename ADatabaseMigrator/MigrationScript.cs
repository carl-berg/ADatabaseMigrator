using ADatabaseMigrator.Core;
using System;

namespace ADatabaseMigrator
{
    public class MigrationScript : Migration, IMigrationScript
    {
        public MigrationScript(string name, MigrationScriptRunType runType, IComparable version, string script, string scriptHash) : base(name, runType, version, scriptHash)
        {
            Script = script;
        }

        /// <inheritdoc/>
        public string Script { get; }
    }
}
