using System;
using System.Reflection;

namespace ADatabaseMigrator.ScriptLoading.EmbeddedResources.Versioning
{
    public class VersionFromAssemblyVersionLoader : IEmbeddedResourceVersionLoader
    {
        public IComparable GetVersion(Assembly assembly, string embeddedResourceName, string rootNamespace, string fileName) => assembly.GetName().Version;
    }
}
