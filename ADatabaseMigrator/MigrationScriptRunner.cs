using ADatabaseMigrator.Core;
using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace ADatabaseMigrator;

public class MigrationScriptRunner(
    DbConnection _connection, 
    DbTransaction? _transaction = null) : IMigrationScriptRunner<MigrationScript>
{
    public virtual async Task Run(MigrationScript migrationScript, string appendJournalScript, CancellationToken cancellationToken)
    {
        try
        { 
            if (_transaction is { })
            {
                // Enlisting in provided transaction, caller handles committing
                await ExecuteScript(migrationScript.Name, migrationScript.Script, _transaction);
                await ExecuteScript(migrationScript.Name, appendJournalScript, _transaction);
            }
            else
            {
                // No transaction provided, we create and commit our own for this script
                using var transaction = _connection.BeginTransaction();
                await ExecuteScript(migrationScript.Name, migrationScript.Script, transaction);
                await ExecuteScript(migrationScript.Name, appendJournalScript, transaction);
                transaction.Commit();
            }
        }
        catch (Exception ex)
        {
            throw new ScriptExecutionException(migrationScript, ex);
        }
    }

    protected virtual async Task ExecuteScript(string scriptName, string script, DbTransaction transaction)
    {
        using var command = _connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = script;
        await command.ExecuteNonQueryAsync();
    }

    public class ScriptExecutionException : Exception
    {
        public ScriptExecutionException(MigrationScript script, Exception innerException) 
            : base($"Script {script.Name} could not be executed.", innerException)
        {
            Data["MigrationScript"] = script;
        }
    }
}