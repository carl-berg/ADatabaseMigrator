using ADatabaseMigrator.Hashing;
using ADatabaseMigrator.Journaling;
using ADatabaseMigrator.ScriptLoading.EmbeddedResources;
using ADatabaseMigrator.ScriptLoading.EmbeddedResources.Versioning;
using ADatabaseMigrator.Tests.GalacticWasteManagement.Core;
using GalacticWasteManagement;
using GalacticWasteManagement.Logging;
using GalacticWasteManagement.SqlServer;
using Shouldly;

namespace ADatabaseMigrator.Tests.GalacticWasteManagement;

public class GalacticWasteCompatibilityTests(DatabaseFixture fixture) : DatabaseTest(fixture)
{
    [Fact]
    public async Task Test_Migration_With_Existing_GWM_Migrations_In_SchemaVersionJournal()
    {
        // Run migrations using GWM
        var galacticWasteManager = GalacticWasteManager
            .Create<GalacticWasteCompatibilityTests>(Fixture.ConnectionString, cfg => cfg.ScriptParser = new MsSql150ScriptParser());
        await galacticWasteManager.Update("LiveField");

        using var connection = Fixture.CreateNewConnection();

        // Configure and run ADatabaseMigrator using GalacticWasteMigrationScriptJournalManager
        var migrator = new Migrator(new EmbeddedResourceScriptLoader(new MD5ScriptHasher(), config => config
                .UsingAssemblyFromType<GalacticWasteCompatibilityTests>()
                    .AddNamespaces<VersionFromPathVersionLoader>(MigrationScriptRunType.RunOnce, "Scripts.Migrations")
                    .AddNamespaces<VersionFromAssemblyVersionLoader>(MigrationScriptRunType.RunIfChanged, "Scripts.RunIfChanged")
                    .AddNamespaces<VersionFromAssemblyVersionLoader>(MigrationScriptRunType.RunAlways, "Scripts.RunAlways")),
            new GalacticWasteMigrationScriptJournalManager(connection), // <-- Custom journal manager for GWM compatibility
            new MigrationScriptRunner(connection));

        var result = await migrator.Migrate(CancellationToken.None);

        // Only the RunAalways migration has been run
        result.ShouldHaveSingleItem().ShouldSatisfyAllConditions(
            migration => migration.RunType.ShouldBe(MigrationScriptRunType.RunAlways),
            migration => migration.Name.ShouldEndWith("001_RunLog.sql"));
    }

    private class TestLogger : ILogger
    {
        public void Log(string message, string type)
        {

        }
    }
}
