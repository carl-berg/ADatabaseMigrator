using System;
using System.IO;
using System.Reflection;

namespace ADatabaseMigrator.ScriptLoading.EmbeddedResources.Versioning
{
    public class VersionFromPathVersionLoader : IEmbeddedResourceVersionLoader
    {
        public IComparable GetVersion(Assembly assembly, string embeddedResourceName, string rootNamespace, string fileName)
        {
            var lastDirectoryIndex = Path.GetFileNameWithoutExtension(embeddedResourceName).LastIndexOf('.');

            var versionPart = embeddedResourceName
                .Remove(lastDirectoryIndex)
                .Substring(rootNamespace.Length + 1)
                .Replace("_", "");

            return Version.TryParse(versionPart, out var match)
                ? match
                : throw new Exception($"Cannot extract a version from embedded resource {embeddedResourceName}");
        }
    }
}
