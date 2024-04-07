using ADatabaseMigrator.ScriptLoading.EmbeddedResources.Versioning;

namespace ADatabaseMigrator.ScriptLoading.EmbeddedResources;

public interface IEmbeddedResourceAssemblyBuilder : IEmbeddedResourceBuilder
{
    IEmbeddedResourceAssemblyBuilder AddNamespaces(string runtype, IEmbeddedResourceVersionLoader version, params string[] namespaces);
    IEmbeddedResourceAssemblyBuilder AddNamespaces<TVersionLoader>(string runtype, params string[] namespaces) where TVersionLoader : IEmbeddedResourceVersionLoader, new();
}
