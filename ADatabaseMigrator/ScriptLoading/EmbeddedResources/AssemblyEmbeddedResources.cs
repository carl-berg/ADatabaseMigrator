using System.Collections.Generic;
using System.Reflection;

namespace ADatabaseMigrator.ScriptLoading.EmbeddedResources
{
    public class AssemblyEmbeddedResources(Assembly assembly) : List<AssemblyEmbeddedResourceNamespace>
    {
        public Assembly Assembly { get; } = assembly;
    }
}
