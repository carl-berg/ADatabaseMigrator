using ADatabaseMigrator.Core;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace ADatabaseMigrator;

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

    public async Task<MigrationJournal> Load()
    {
        await CreateJournalTableIfNotExists();
        using var command = _connection.CreateCommand();
        command.CommandText =
            """
            SELECT Version, Name, Applied, Hash, Type 
            FROM dbo.SchemaVersionJournal
            """;

        using var reader = await command.ExecuteReaderAsync();
        var journalEntries = new List<MigrationJournalEntry>();

        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            journalEntries.Add(new MigrationJournalEntry
            {
                Version = reader.GetString(0),
                Name = reader.GetString(1),
                Applied = reader[nameof(MigrationJournalEntry.Applied)].ToString(),
                ScriptHash = reader.GetString(3),
                RunType = reader.GetString(4),
            });
        }

        return new MigrationJournal(journalEntries
            .Select(x => x.ToMigration(ParseDate, ParseRunType))
            .ToList());
    }

    public virtual async Task CreateJournalTableIfNotExists()
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
        await command.ExecuteNonQueryAsync();
    }

    protected virtual DateTime ParseDate(string value) => DateTime.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal);
    protected virtual MigrationScriptRunType ParseRunType(string value) => (MigrationScriptRunType)Enum.Parse(typeof(MigrationScriptRunType), value);

    private class MigrationJournalEntry
    {
        public string Version { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Applied { get; set; } = default!;
        public string RunType { get; set; } = default!;
        public string ScriptHash { get; set; } = default!;

        public Migration ToMigration(Func<string, DateTime> dateParser, Func<string, MigrationScriptRunType> runTypeParser) => new(
            Name,
            runTypeParser(RunType),
            Version,
            dateParser(Applied),
            ScriptHash);
    }
}
