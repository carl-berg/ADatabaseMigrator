using System.Reflection;

namespace ADatabaseMigrator.ScriptLoading.EmbeddedResources;

public interface IEmbeddedResourceBuilder
{
    IEmbeddedResourceAssemblyBuilder UsingAssembly(Assembly assembly);
    IEmbeddedResourceAssemblyBuilder UsingAssemblyFromType<T>() where T : class;
}
