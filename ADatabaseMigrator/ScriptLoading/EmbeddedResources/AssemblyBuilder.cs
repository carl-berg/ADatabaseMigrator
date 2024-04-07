using ADatabaseMigrator.ScriptLoading.EmbeddedResources.Versioning;
using System.Reflection;

namespace ADatabaseMigrator.ScriptLoading.EmbeddedResources;

internal class AssemblyBuilder(IEmbeddedResourceBuilder _root, AssemblyEmbeddedResources _resources) : IEmbeddedResourceAssemblyBuilder
{
    public IEmbeddedResourceAssemblyBuilder AddNamespaces(string runType, IEmbeddedResourceVersionLoader versionLoader, params string[] namespaces)
    {
        foreach (var @namespace in namespaces)
        {
            _resources.Add(new(runType, versionLoader, @namespace));
        }

        return this;
    }

    public IEmbeddedResourceAssemblyBuilder AddNamespaces<TVersionLoader>(string runtype, params string[] namespaces) where TVersionLoader : IEmbeddedResourceVersionLoader, new()
        => AddNamespaces(runtype, new TVersionLoader(), namespaces);

    public IEmbeddedResourceAssemblyBuilder UsingAssembly(Assembly assembly) => _root.UsingAssembly(assembly);
    public IEmbeddedResourceAssemblyBuilder UsingAssemblyFromType<T>() where T : class => _root.UsingAssemblyFromType<T>();
}
