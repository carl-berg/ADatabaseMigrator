using ADatabaseMigrator.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ADatabaseMigrator.Journaling;

public class MigrationScriptJournalManager(DbConnection _connection) : IMigrationJournalManager<MigrationJournal, Migration, MigrationScript>
{
    public string AddJournalScript(MigrationScript script) =>
        $"""
        INSERT INTO dbo.SchemaVersionJournal(Version, Name, Applied, Hash, Type)
        VALUES(
            '{script.Version}',
            '{script.Name}',
            GETUTCDATE(),
            '{script.ScriptHash}',
            '{script.RunType}')
        """;

    public async Task<MigrationJournal> Load(CancellationToken? cancellationToken = default)
    {
        await CreateJournalTableIfNotExists(cancellationToken);
        using var command = _connection.CreateCommand();
        command.CommandText =
            """
            SELECT Version, Name, Applied, Hash, Type 
            FROM dbo.SchemaVersionJournal
            """;

        using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        var journalEntries = new List<(string Version, string Name, DateTime Applied, string ScriptHash, string RunType)>();

        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None).ConfigureAwait(false))
        {
            journalEntries.Add((
                reader.GetString(0),
                reader.GetString(1),
                DateTime.SpecifyKind(reader.GetDateTime(2), DateTimeKind.Utc),
                reader.GetString(3),
                reader.GetString(4)
            ));
        }

        return new MigrationJournal(journalEntries
            .Select(x => ParseMigration(x.Version, x.Name, x.Applied, x.ScriptHash, x.RunType))
            .ToList());
    }

    public virtual async Task CreateJournalTableIfNotExists(CancellationToken? cancellationToken = default)
    {
        using var command = _connection.CreateCommand();
        command.CommandText =
        """
        IF OBJECT_ID(N'dbo.SchemaVersionJournal', N'U') IS NULL BEGIN
            CREATE TABLE dbo.SchemaVersionJournal (
                [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT PK_SchemaVersionJournal_Id PRIMARY KEY,
                [Version] NVARCHAR(255) NOT NULL,                    
                [Name] NVARCHAR(255) NOT NULL,
                [Applied] DATETIME2 NOT NULL,
                [Hash] NVARCHAR(255) NOT NULL,
        	    [Type] NVARCHAR(255) NOT NULL,
            );
        END
        """;
        await command.ExecuteNonQueryAsync(cancellationToken ?? CancellationToken.None);
    }

    protected virtual Migration ParseMigration(string version, string name, DateTime applied, string scriptHash, string runType) =>
        new(
            name,
            Enum.TryParse<MigrationScriptRunType>(runType, out var parsedRunType)
                ? parsedRunType
                : throw new ArgumentException($"Migration {name} has a RunType '{runType}' which not a valid {nameof(MigrationScriptRunType)}"),
            version,
            applied,
            scriptHash);
}
