using ADatabaseMigrator.ScriptLoading.EmbeddedResources;
using ADatabaseMigrator.ScriptLoading.EmbeddedResources.Versioning;

namespace ADatabaseMigrator.Tests;

public class EmbeddedResourceTests
{
    [Fact]
    public async Task TestSomething()
    {
        var scriptLoader = new EmbeddedResourceScriptLoader(configure => configure
            .UsingAssemblyFromType<EmbeddedResourceTests>()
                .AddNamespaces<VersionFromPathVersionLoader>(MigrationScriptRunType.RunOnce, "Scripts.Migrations")
                .AddNamespaces<VersionFromAssemblyVersionLoader>(MigrationScriptRunType.RunIfChanged, "Scripts.RunIfChanged")
                .AddNamespaces<VersionFromAssemblyVersionLoader>(MigrationScriptRunType.RunIfChanged, "Scripts.RunAlways"));

        var scripts = await scriptLoader.Load();

        await Verify(scripts.Select(x => new { x.Name, x.Version }));
    }
}
