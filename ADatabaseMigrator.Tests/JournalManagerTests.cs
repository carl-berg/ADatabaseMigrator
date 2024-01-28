﻿using ADatabaseMigrator.Tests.Core;
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
        await journalManager.CreateJournalTableIfNotExists();

        // Verify SchemaVersionJournal table exists (and can be queried)
        await Should.NotThrowAsync(connection.QuerySingleAsync<int>("SELECT COUNT(1) FROM SchemaVersionJournal"));
    }

    [Fact]
    public async Task Test_Add_Journal_Scripts_Journal()
    {
        using var connection = Fixture.CreateNewConnection();
        var journalManager = new MigrationScriptJournalManager(connection);
        await journalManager.CreateJournalTableIfNotExists();

        // Ensure schema version journal is empty
        await connection.ExecuteAsync("DELETE FROM SchemaVersionJournal");

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

        await Verify(journalEntries);
    }

    [Fact]
    public async Task Test_Load_Scripts_Journal()
    {
        using var connection = Fixture.CreateNewConnection();
        var journalManager = new MigrationScriptJournalManager(connection);
        await journalManager.CreateJournalTableIfNotExists();

        // Ensure schema version journal is empty
        await connection.ExecuteAsync("DELETE FROM SchemaVersionJournal");

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

        var journal = await journalManager.Load();

        await Verify(journal);
    }

    private record JournalEntry(string Name, string Version, DateTime Applied, string Type, string Hash);
}
