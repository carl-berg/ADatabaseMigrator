using ADatabaseMigrator.Hashing;
using ADatabaseMigrator.ScriptLoading.EmbeddedResources;
using ADatabaseMigrator.ScriptLoading.EmbeddedResources.Versioning;
using ADatabaseMigrator.Tests.Core;

namespace ADatabaseMigrator.Tests;

public class MigratorTests(DatabaseFixture fixture) : DatabaseTest(fixture)
{
    [Fact]
    public async Task Test_Migration()
    {
        using var connection = Fixture.CreateNewConnection();

        //TODO: Ensure database is empty

        var migrator = new Migrator(
            scriptLoader: new EmbeddedResourceScriptLoader(new MD5ScriptHasher(), config => config
                .UsingAssemblyFromType<ScriptLoaderTests>()
                    .AddNamespaces<VersionFromPathVersionLoader>(MigrationScriptRunType.RunOnce, "Scripts.Migrations")
                    .AddNamespaces<VersionFromAssemblyVersionLoader>(MigrationScriptRunType.RunIfChanged, "Scripts.RunIfChanged")
                    .AddNamespaces<VersionFromAssemblyVersionLoader>(MigrationScriptRunType.RunAlways, "Scripts.RunAlways")),
            journalManager: new MigrationScriptJournalManager(connection),
            scriptRunner: new MigrationScriptRunner(connection));

        await migrator.Migrate(CancellationToken.None);

        //TODO: Verify migration outcome
    }
}
