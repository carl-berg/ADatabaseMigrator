using Microsoft.SqlServer.Management.SqlParser.Parser;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ADatabaseMigrator.SqlServer
{
    public class ScriptParsingException : Exception
    {
        public ScriptParsingException(string scriptName, ParseResult result) 
            : base($"Failed to parse script '{scriptName}':{Environment.NewLine}{FormatErrors(result.Errors)}")
        {
            Data["script_name"] = scriptName;
            Data["script_sql"] = result.Script.Sql;
        }

        private static string FormatErrors(IEnumerable<ErrorBase> errors) => string.Join(Environment.NewLine, errors.Select(x => $"\t-{x.Message}"));
    }
}
