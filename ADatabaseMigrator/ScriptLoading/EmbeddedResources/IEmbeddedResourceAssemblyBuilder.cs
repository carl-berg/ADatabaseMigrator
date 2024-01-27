using ADatabaseMigrator.ScriptLoading.EmbeddedResources.Versioning;

namespace ADatabaseMigrator.ScriptLoading.EmbeddedResources
{
    public interface IEmbeddedResourceAssemblyBuilder : IEmbeddedResourceBuilder
    {
        IEmbeddedResourceAssemblyBuilder AddNamespaces(MigrationScriptRunType runtype, IEmbeddedResourceVersionLoader version, params string[] namespaces);
        IEmbeddedResourceAssemblyBuilder AddNamespaces<TVersionLoader>(MigrationScriptRunType runtype, params string[] namespaces) where TVersionLoader : IEmbeddedResourceVersionLoader, new();
    }
}
