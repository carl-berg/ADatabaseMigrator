using ADatabaseMigrator.Hashing;
using ADatabaseMigrator.ScriptLoading.EmbeddedResources;
using ADatabaseMigrator.ScriptLoading.EmbeddedResources.Versioning;
using ADatabaseMigrator.Tests.Core;
using Dapper;

namespace ADatabaseMigrator.Tests;

public class MigratorTests(DatabaseFixture fixture) : DatabaseTest(fixture)
{
    [Fact]
    public async Task Test_Migration()
    {
        using var connection = Fixture.CreateNewConnection();

        var migrator = new Migrator(
            scriptLoader: new EmbeddedResourceScriptLoader(new MD5ScriptHasher(), config => config
                .UsingAssemblyFromType<ScriptLoaderTests>()
                    .AddNamespaces<VersionFromPathVersionLoader>(MigrationScriptRunType.RunOnce, "Scripts.Migrations")
                    .AddNamespaces<VersionFromAssemblyVersionLoader>(MigrationScriptRunType.RunIfChanged, "Scripts.RunIfChanged")
                    .AddNamespaces<VersionFromAssemblyVersionLoader>(MigrationScriptRunType.RunAlways, "Scripts.RunAlways")),
            journalManager: new MigrationScriptJournalManager(connection),
            scriptRunner: new MigrationScriptRunner(connection));

        await migrator.Migrate(CancellationToken.None);

        var tables = await connection.QueryAsync<string>("SELECT table_name FROM INFORMATION_SCHEMA.TABLES");
        var journal = await connection.QueryAsync<SchemaVersionJournalDto>("SELECT Version, Name, Hash FROM SchemaVersionJournal");
        var runLogEntries = await connection.QuerySingleAsync<int>("SELECT COUNT(1) FROM RunLog");

        await Verify(new { tables, journal, runLogEntries })
            .DontScrubGuids(/* Verify mistakes hashes for guids and scrubs them */);
    }

    private record SchemaVersionJournalDto(string Version, string Name, string Hash);
}
