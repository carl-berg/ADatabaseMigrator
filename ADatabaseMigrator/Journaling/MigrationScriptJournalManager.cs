﻿using ADatabaseMigrator.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace ADatabaseMigrator.Journaling;

public class MigrationScriptJournalManager(DbConnection _connection) : IMigrationJournalManager<MigrationJournal, Migration, MigrationScript>
{
    public const string JournalTableName = "SchemaVersionJournal";

    public string AddJournalScript(MigrationScript script) =>
        $"""
        INSERT INTO dbo.{JournalTableName}(Version, Name, Applied, Hash, Type)
        VALUES(
            '{script.Version}',
            '{script.Name}',
            GETUTCDATE(),
            '{script.ScriptHash}',
            '{script.RunType}')
        """;

    public async virtual Task<MigrationJournal> Load(CancellationToken? cancellationToken = default)
    {
        await CreateJournalTableIfNotExists(cancellationToken);
        using var command = _connection.CreateCommand();
        command.CommandText =
            $"""
            SELECT Version, Name, Applied, Hash, Type 
            FROM dbo.{JournalTableName}
            ORDER BY Applied ASC
            """;

        using var reader = await command.ExecuteReaderAsync(cancellationToken ?? CancellationToken.None);
        var journalEntries = new List<Migration>();

        while (await reader.ReadAsync(cancellationToken ?? CancellationToken.None).ConfigureAwait(false))
        {
            journalEntries.Add(new(
                version: reader.GetString(0),
                name: reader.GetString(1),
                applied: DateTime.SpecifyKind(reader.GetDateTime(2), DateTimeKind.Utc),
                scriptHash: reader.GetString(3),
                runType: reader.GetString(4)
            ));
        }

        return new MigrationJournal(journalEntries);
    }

    public virtual async Task CreateJournalTableIfNotExists(CancellationToken? cancellationToken = default)
    {
        using var command = _connection.CreateCommand();
        command.CommandText =
        $"""
        IF OBJECT_ID(N'dbo.{JournalTableName}', N'U') IS NULL BEGIN
            CREATE TABLE dbo.{JournalTableName} (
                [Id] int IDENTITY(1,1) NOT NULL CONSTRAINT PK_{JournalTableName}_Id PRIMARY KEY,
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
}
