using ADatabaseMigrator.ScriptLoading.EmbeddedResources;
using ADatabaseMigrator.ScriptLoading.EmbeddedResources.Versioning;

namespace ADatabaseMigrator.Tests;

public class EmbeddedResourceTests
{
    [Theory]
    [InlineData("Scripts.Migrations")]
    [InlineData("Scripts.MigrationsAlternative")]
    public async Task Scipts_have_expected_version_based_order(string migrationNamespace)
    {
        var scriptLoader = new EmbeddedResourceScriptLoader(configure => configure
            .UsingAssemblyFromType<EmbeddedResourceTests>()
                .AddNamespaces<VersionFromPathVersionLoader>(MigrationScriptRunType.RunOnce, migrationNamespace)
                .AddNamespaces<VersionFromAssemblyVersionLoader>(MigrationScriptRunType.RunIfChanged, "Scripts.RunIfChanged")
                .AddNamespaces<VersionFromAssemblyVersionLoader>(MigrationScriptRunType.RunIfChanged, "Scripts.RunAlways"));

        var scripts = await scriptLoader.Load();

        await Verify(scripts)
            .ScrubMember<MigrationScript>(x => x.Script)
            .ScrubLinesWithReplace(x => x.Replace(migrationNamespace, "{MigrationPath}"))
            .DisableRequireUniquePrefix(/* Allow all tests to use the same verification */);
    }
}
