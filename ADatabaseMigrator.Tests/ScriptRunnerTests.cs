using ADatabaseMigrator.Hashing;
using ADatabaseMigrator.ScriptLoading.EmbeddedResources;
using ADatabaseMigrator.ScriptLoading.EmbeddedResources.Versioning;
using ADatabaseMigrator.Tests.Core;

namespace ADatabaseMigrator.Tests;

public class ScriptRunnerTests(DatabaseFixture fixture) : DatabaseTest(fixture)
{
    [Fact]
    public async Task Test_ScriptRunner_Execution()
    {
        //TODO: Replace these scripts with inline sql scripts so this test doesn't depend on how embedded script loader functionality
        var scriptLoader = new EmbeddedResourceScriptLoader(new MD5ScriptHasher(), configure => configure
            .UsingAssemblyFromType<EmbeddedResourceTests>()
                .AddNamespaces<VersionFromPathVersionLoader>(MigrationScriptRunType.RunOnce, "Scripts.Migrations")
                .AddNamespaces<VersionFromAssemblyVersionLoader>(MigrationScriptRunType.RunIfChanged, "Scripts.RunIfChanged")
                .AddNamespaces<VersionFromAssemblyVersionLoader>(MigrationScriptRunType.RunAlways, "Scripts.RunAlways"));

        using var connection = Fixture.CreateNewConnection();

        var scriptRunner = new MigrationScriptRunner(connection);

        var scripts = await scriptLoader.Load();

        foreach (var script in scripts)
        {
            await scriptRunner.Run(script, CancellationToken.None);
        }

        //TODO: Verify scripts were executed

        //TODO: Verify journals were inserted
    }

    public Task Test_Individual_Transaction_Handling()
    {
        // TODO: Test that an inidividual script failure doesn't roll back already inserted scripts
        return Task.CompletedTask;
    }

    public Task Test_Enlisted_Transaction_Handling()
    {
        // TODO: Test that an inidividual script failure roll back all already inserted scripts
        return Task.CompletedTask;
    }
}
