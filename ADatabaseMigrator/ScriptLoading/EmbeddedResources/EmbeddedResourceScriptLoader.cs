using ADatabaseMigrator.Core;
using ADatabaseMigrator.Hashing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ADatabaseMigrator.ScriptLoading.EmbeddedResources;

public class EmbeddedResourceScriptLoader : IMigrationScriptLoader<MigrationScript>, IEmbeddedResourceBuilder
{
    public EmbeddedResourceScriptLoader(IScriptHasher scriptHasher, Action<IEmbeddedResourceBuilder>? configure = null)
    {
        configure?.Invoke(this);
        ScriptHasher = scriptHasher;
    }

    protected List<AssemblyEmbeddedResources> Resources { get; } = new();
    protected IScriptHasher ScriptHasher { get; }

    public virtual async Task<IReadOnlyList<MigrationScript>> Load(CancellationToken? cancellationToken = default)
    {
        var scripts = new List<MigrationScript>();
        foreach (var assemblyResources in Resources)
        {
            var assemblyRootNamespace = assemblyResources.Assembly.GetName().Name;
            var embeddedResourceNames = assemblyResources.Assembly.GetManifestResourceNames();
            foreach (var (runType, versionLoader, @namespace) in assemblyResources)
            {
                var namespaceScripts = new List<MigrationScript>();
                var currentNamespaceRoot = $"{assemblyRootNamespace}.{@namespace}";
                foreach (var embeddedResource in embeddedResourceNames.Where(name => name.StartsWith($"{currentNamespaceRoot}.")))
                {
                    var stream = assemblyResources.Assembly.GetManifestResourceStream(embeddedResource);
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    var script = await reader.ReadToEndAsync();
                    var fileName = ExtractFilename(embeddedResource);
                    var version = versionLoader.GetVersion(assemblyResources.Assembly, embeddedResource, currentNamespaceRoot, fileName);
                    var hash = ScriptHasher.Hash(script);

                    namespaceScripts.Add(new MigrationScript(
                        name: embeddedResource,
                        runType: runType,
                        version: version,
                        script: script,
                        scriptHash: hash));
                }

                scripts.AddRange(namespaceScripts.OrderBy(x => x.Version).ThenBy(x => x.Name));
            }
        }

        return scripts;
    }

    protected string ExtractFilename(string embeddedResourceName)
    {
        var lastDirectoryIndex = Path.GetFileNameWithoutExtension(embeddedResourceName).LastIndexOf('.');
        return embeddedResourceName.Substring(lastDirectoryIndex + 1);
    }

    IEmbeddedResourceAssemblyBuilder IEmbeddedResourceBuilder.UsingAssembly(Assembly assembly)
    {
        var assemblyResources = new AssemblyEmbeddedResources(assembly);
        Resources.Add(assemblyResources);
        return new AssemblyBuilder(this, assemblyResources);
    }

    IEmbeddedResourceAssemblyBuilder IEmbeddedResourceBuilder.UsingAssemblyFromType<T>() where T : class 
        => ((IEmbeddedResourceBuilder)this).UsingAssembly(typeof(T).Assembly);
}
