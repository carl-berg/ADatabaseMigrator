using ADatabaseMigrator.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using static ADatabaseMigrator.EmbeddedResourceScriptLoader;

namespace ADatabaseMigrator
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
                foreach (var (runtype, @namespace) in assemblyResources)
                {
                    foreach (var embeddedResource in embeddedResourceNames.Where(name => name.StartsWith($"{assemblyRootNamespace}.{@namespace}")))
                    {
                        var info = assemblyResources.Assembly.GetManifestResourceInfo(embeddedResource);
                        var stream = assemblyResources.Assembly.GetManifestResourceStream(embeddedResource);
                        using var reader = new StreamReader(stream);
                        var script = await reader.ReadToEndAsync();
                        var lastDirectoryIndex = Path.GetFileNameWithoutExtension(embeddedResource).LastIndexOf('.');
                        var fileName = embeddedResource.Substring(lastDirectoryIndex + 1);
                        scripts.Add(new MigrationScript(
                            id: embeddedResource,
                            fileName: fileName,
                            runType: runtype,
                            version: string.Empty,
                            script: script,
                            scriptHash: string.Empty));

                        // TODO: Extract version
                        // not super clear how to do this... maybe exclude the filename and the namespace part, remove underscores and try to regex a version out of what's left
                        // but i think this logic should probably be extracted as its own dependency so you can plug in your own to handle weird scenarios
                        // maybe this versioner dependency should be set by namespace inclusion since versioning is not needed for RunAlways/RunIfChanged

                        //TODO: Extract hash
                    }
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

        IEmbeddedResourceAssemblyBuilder IEmbeddedResourceBuilder.UsingAssemblyFromType<T>() where T : class => ((IEmbeddedResourceBuilder)this).UsingAssembly(typeof(T).Assembly);

        internal class AssemblyBuilder(IEmbeddedResourceBuilder _root, AssemblyEmbeddedResources _resources) : IEmbeddedResourceAssemblyBuilder
        {
            public IEmbeddedResourceAssemblyBuilder AddNamespaces(MigrationScriptRunType runType, params string[] namespaces)
            {
                foreach (var @namespace in namespaces)
                {
                    _resources.Add((runType, @namespace));
                }

                return this;
            }

            public IEmbeddedResourceAssemblyBuilder UsingAssembly(Assembly assembly) => _root.UsingAssembly(assembly);
            public IEmbeddedResourceAssemblyBuilder UsingAssemblyFromType<T>() where T : class => _root.UsingAssemblyFromType<T>();
        }

        public class AssemblyEmbeddedResources : List<(MigrationScriptRunType Runtype, string Namespace)>
        {
            public AssemblyEmbeddedResources(Assembly assembly) => Assembly = assembly;

            public Assembly Assembly { get; }
        }

        public interface IEmbeddedResourceBuilder
        {
            IEmbeddedResourceAssemblyBuilder UsingAssembly(Assembly assembly);
            IEmbeddedResourceAssemblyBuilder UsingAssemblyFromType<T>() where T : class;
        }

        public interface IEmbeddedResourceAssemblyBuilder : IEmbeddedResourceBuilder
        {
            IEmbeddedResourceAssemblyBuilder AddNamespaces(MigrationScriptRunType runtype, params string[] namespaces);
        }
    }
}
