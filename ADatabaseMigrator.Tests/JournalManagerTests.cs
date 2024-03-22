using ADatabaseMigrator.Journaling;
using ADatabaseMigrator.Tests.Core;
using Dapper;
using Shouldly;

namespace ADatabaseMigrator.Tests;

public class JournalManagerTests(DatabaseFixture fixture) : DatabaseTest(fixture)
{
    [Fact]
    public async Task Test_Create_Version_Journal_Table()
    {
        using var connection = Fixture.CreateNewConnection();
        var journalManager = new MigrationScriptJournalManager(connection);
        await journalManager.CreateJournalTableIfNotExists(CancellationToken.None);

        // Verify SchemaVersionJournal table exists (and can be queried)
        await Should.NotThrowAsync(connection.QuerySingleAsync<int>("SELECT COUNT(1) FROM SchemaVersionJournal"));
    }

    [Fact]
    public async Task Test_Add_Journal_Scripts_Journal()
    {
        using var connection = Fixture.CreateNewConnection();
        var journalManager = new MigrationScriptJournalManager(connection);
        await journalManager.CreateJournalTableIfNotExists(CancellationToken.None);

        await connection.ExecuteAsync(journalManager.AddJournalScript(new MigrationScript(
            name: "Script_1", 
            runType: MigrationScriptRunType.RunOnce,
            version: "1.0.0",
            script: string.Empty,
            scriptHash: "hash_1")));

        await connection.ExecuteAsync(journalManager.AddJournalScript(new MigrationScript(
            name: "Script_2",
            runType: MigrationScriptRunType.RunOnce,
            version: "1.0.0",
            script: string.Empty,
            scriptHash: "hash_2")));

        // Verify journal entries exist
        var journalEntries = await connection.QueryAsync<JournalEntry>(
            "SELECT Name, Version, Applied, Type, Hash FROM SchemaVersionJournal");

        await Verify(journalEntries)
            .ScrubMember<JournalEntry>(x => x.Applied);
    }

    [Fact]
    public async Task Test_Load_Scripts_Journal()
    {
        using var connection = Fixture.CreateNewConnection();
        var journalManager = new MigrationScriptJournalManager(connection);
        await journalManager.CreateJournalTableIfNotExists(CancellationToken.None);

        await connection.ExecuteAsync(journalManager.AddJournalScript(new MigrationScript(
            name: "Script_1",
            runType: MigrationScriptRunType.RunOnce,
            version: "1.0.0",
            script: string.Empty,
            scriptHash: "hash_1")));

        await connection.ExecuteAsync(journalManager.AddJournalScript(new MigrationScript(
            name: "Script_2",
            runType: MigrationScriptRunType.RunOnce,
            version: "1.0.0",
            script: string.Empty,
            scriptHash: "hash_2")));

        var journal = await journalManager.Load(CancellationToken.None);

        await Verify(journal)
            .ScrubMember<Migration>(x => x.Applied);

        // Verify utc dates
        foreach(var entry in journal)
        {
            entry.Applied.Kind.ShouldBe(DateTimeKind.Utc);
        }
    }

    [Fact]
    public async Task Test_Load_Scripts_Containing_Duplicates()
    {
        using var connection = Fixture.CreateNewConnection();
        var journalManager = new MigrationScriptJournalManager(connection);
        await journalManager.CreateJournalTableIfNotExists(CancellationToken.None);

        await connection.ExecuteAsync(journalManager.AddJournalScript(new MigrationScript(
            name: "Script_1",
            runType: MigrationScriptRunType.RunIfChanged,
            version: "1.0.0",
            script: string.Empty,
            scriptHash: "hash_1")));

        await connection.ExecuteAsync(journalManager.AddJournalScript(new MigrationScript(
            name: "Script_1",
            runType: MigrationScriptRunType.RunOnce,
            version: "1.0.1",
            script: string.Empty,
            scriptHash: "hash_2")));

        var journal = await journalManager.Load(CancellationToken.None);

        journal.ShouldAllBe(x => x.Name == "Script_1");
        journal.Count().ShouldBe(2);
    }

    private record JournalEntry(string Name, string Version, DateTime Applied, string Type, string Hash);
}
