using Xunit;

namespace ADatabaseMigrator.Tests;

public class EmbeddedResourceTests
{
    [Fact]
    public async Task TestSomething()
    {
        var scriptLoader = new EmbeddedResourceScriptLoader(configure => configure
            .UsingAssemblyFromType<EmbeddedResourceTests>()
                .AddNamespaces(MigrationScriptRunType.RunOnce, "Scripts.Migrations")
                .AddNamespaces(MigrationScriptRunType.RunIfChanged, "Scripts.RunIfChanged")
                .AddNamespaces(MigrationScriptRunType.RunIfChanged, "Scripts.RunAlways"));

        var scripts = await scriptLoader.Load();
    }

}
