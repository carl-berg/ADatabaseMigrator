using ADatabaseMigrator.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ADatabaseMigrator.ScriptLoading.EmbeddedResources
{
    public class EmbeddedResourceScriptLoader : IMigrationScriptLoader<MigrationScript>, IEmbeddedResourceBuilder
    {
        public EmbeddedResourceScriptLoader(Action<IEmbeddedResourceBuilder>? configure = null) => configure?.Invoke(this);

        protected List<AssemblyEmbeddedResources> Resources { get; } = new();

        public async Task<IReadOnlyList<MigrationScript>> Load()
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
                        var info = assemblyResources.Assembly.GetManifestResourceInfo(embeddedResource);
                        var stream = assemblyResources.Assembly.GetManifestResourceStream(embeddedResource);
                        using var reader = new StreamReader(stream, Encoding.UTF8);
                        var script = await reader.ReadToEndAsync();
                        var lastDirectoryIndex = Path.GetFileNameWithoutExtension(embeddedResource).LastIndexOf('.');
                        var fileName = embeddedResource.Substring(lastDirectoryIndex + 1);
                        var version = versionLoader.GetVersion(assemblyResources.Assembly, embeddedResource, currentNamespaceRoot, fileName);

                        namespaceScripts.Add(new MigrationScript(
                            name: embeddedResource,
                            runType: runType,
                            version: version,
                            script: script,
                            scriptHash: string.Empty));

                        //TODO: Extract hash
                    }

                    scripts.AddRange(namespaceScripts.OrderBy(x => x.Version).ThenBy(x => x.Name));
                }
            }

            return scripts;
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
}
