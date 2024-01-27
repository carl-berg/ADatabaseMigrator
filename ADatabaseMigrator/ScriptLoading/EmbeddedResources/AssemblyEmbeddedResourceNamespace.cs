using ADatabaseMigrator.ScriptLoading.EmbeddedResources.Versioning;

namespace ADatabaseMigrator.ScriptLoading.EmbeddedResources
{
    public class AssemblyEmbeddedResourceNamespace(MigrationScriptRunType runType, IEmbeddedResourceVersionLoader versionLoader, string @namespace)
    {
        public MigrationScriptRunType RunType { get; } = runType;
        public IEmbeddedResourceVersionLoader VersionLoader { get; } = versionLoader;
        public string Namespace { get; } = @namespace;

        public void Deconstruct(out MigrationScriptRunType runType, out IEmbeddedResourceVersionLoader versionLoader, out string @namespace)
        {
            runType = RunType;
            versionLoader = VersionLoader;
            @namespace = Namespace;
        }
    }
}
