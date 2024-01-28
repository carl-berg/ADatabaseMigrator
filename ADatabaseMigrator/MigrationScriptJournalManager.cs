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
                Applied = DateTime.Parse(reader["Applied"].ToString(), CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal),
                ScriptHash = reader.GetString(3),
                RunType = reader.GetString(4),
            });
        }

        return new MigrationJournal(journalEntries
            .Select(x => x.ToMigration())
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

    private class MigrationJournalEntry
    {
        public string Version { get; set; }
        public string Name { get; set; }
        public DateTime Applied { get; set; }
        public string RunType { get; set; }
        public string ScriptHash { get; set; }

        public Migration ToMigration() => new(
            Name,
            (MigrationScriptRunType)Enum.Parse(typeof(MigrationScriptRunType), RunType),
            Version,
            Applied,
            ScriptHash);
    }
}
