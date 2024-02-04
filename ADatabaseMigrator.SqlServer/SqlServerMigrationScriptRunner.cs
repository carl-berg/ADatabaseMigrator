using Microsoft.SqlServer.Management.SqlParser.Parser;
using System.Data.Common;
using System.Linq;
using System.Threading.Tasks;

namespace ADatabaseMigrator.SqlServer
{
    /// <summary>
    /// Script runner that parses scripts based on sql server execution (supports batching)
    /// </summary>
    public class SqlServerMigrationScriptRunner(DbConnection _connection, DbTransaction? _transaction = null, ParseOptions? _parseOptions = null) : MigrationScriptRunner(_connection, _transaction)
    {
        protected override async Task ExecuteScript(string scriptName, string script, DbTransaction transaction)
        {
            var result = _parseOptions is null
                ? Parser.Parse(script) 
                : Parser.Parse(script, _parseOptions);

            if (result.Errors.Any())
            {
                throw new ScriptParsingException(scriptName, result);
            }
            else
            {
                foreach (var batch in result.Script.Batches.Where(x => x is { Sql.Length: > 0 }))
                {
                    await base.ExecuteScript(scriptName, batch.Sql, transaction);
                }
            }
        }
    }
}
