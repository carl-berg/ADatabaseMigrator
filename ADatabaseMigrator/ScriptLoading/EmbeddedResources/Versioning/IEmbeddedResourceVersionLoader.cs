using System;
using System.Reflection;

namespace ADatabaseMigrator.ScriptLoading.EmbeddedResources.Versioning;

public interface IEmbeddedResourceVersionLoader
{
    IComparable GetVersion(Assembly assembly, string embeddedResourceName, string rootNamespace, string fileName);
}
