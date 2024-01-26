using ADatabaseMigrator.Core;
using ADatabaseMigrator.ScriptLoading.EmbeddedResources.Versioning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static ADatabaseMigrator.ScriptLoading.EmbeddedResources.EmbeddedResourceScriptLoader;

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
                    var currentNamespaceRoot = $"{assemblyRootNamespace}.{@namespace}";
                    foreach (var embeddedResource in embeddedResourceNames.Where(name => name.StartsWith(currentNamespaceRoot)))
                    {
                        var info = assemblyResources.Assembly.GetManifestResourceInfo(embeddedResource);
                        var stream = assemblyResources.Assembly.GetManifestResourceStream(embeddedResource);
                        using var reader = new StreamReader(stream, Encoding.UTF8);
                        var script = await reader.ReadToEndAsync();
                        var lastDirectoryIndex = Path.GetFileNameWithoutExtension(embeddedResource).LastIndexOf('.');
                        var fileName = embeddedResource.Substring(lastDirectoryIndex + 1);
                        var version = versionLoader.GetVersion(assemblyResources.Assembly, embeddedResource, currentNamespaceRoot, fileName);

                        scripts.Add(new MigrationScript(
                            name: embeddedResource,
                            runType: runType,
                            version: version,
                            script: script,
                            scriptHash: string.Empty));

                        //TODO: Extract hash
                    }
                }
            }

            return scripts.OrderBy(x => x.Version).ThenBy(x => x.Name).ToList();
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
            public IEmbeddedResourceAssemblyBuilder AddNamespaces(MigrationScriptRunType runType, IEmbeddedResourceVersionLoader versionLoader, params string[] namespaces)
            {
                foreach (var @namespace in namespaces)
                {
                    _resources.Add(new(runType, versionLoader, @namespace));
                }

                return this;
            }

            public IEmbeddedResourceAssemblyBuilder AddNamespaces<TVersionLoader>(MigrationScriptRunType runtype, params string[] namespaces) where TVersionLoader : IEmbeddedResourceVersionLoader, new()
                => AddNamespaces(runtype, new TVersionLoader(), namespaces);

            public IEmbeddedResourceAssemblyBuilder UsingAssembly(Assembly assembly) => _root.UsingAssembly(assembly);
            public IEmbeddedResourceAssemblyBuilder UsingAssemblyFromType<T>() where T : class => _root.UsingAssemblyFromType<T>();
        }

        public class AssemblyEmbeddedResources(Assembly assembly) : List<AssemblyEmbeddedResourceNamespace>
        {
            public Assembly Assembly { get; } = assembly;
        }

        public class AssemblyEmbeddedResourceNamespace(MigrationScriptRunType runType, IEmbeddedResourceVersionLoader versionLoader, string @namespace)
        {
            public MigrationScriptRunType RunType { get; } = runType;
            public IEmbeddedResourceVersionLoader VersionLoader { get; } = versionLoader;
            public string Namespace { get; } = @namespace;

            public void Deconstruct(out MigrationScriptRunType runType, out IEmbeddedResourceVersionLoader versionLoader, out string @namespace)
            {
                runType = RunType;
                versionLoader = VersionLoader;
                @namespace = Namespace;
            }
        }

        public interface IEmbeddedResourceBuilder
        {
            IEmbeddedResourceAssemblyBuilder UsingAssembly(Assembly assembly);
            IEmbeddedResourceAssemblyBuilder UsingAssemblyFromType<T>() where T : class;
        }

        public interface IEmbeddedResourceAssemblyBuilder : IEmbeddedResourceBuilder
        {
            IEmbeddedResourceAssemblyBuilder AddNamespaces(MigrationScriptRunType runtype, IEmbeddedResourceVersionLoader version, params string[] namespaces);
            IEmbeddedResourceAssemblyBuilder AddNamespaces<TVersionLoader>(MigrationScriptRunType runtype, params string[] namespaces) where TVersionLoader : IEmbeddedResourceVersionLoader, new();
        }
    }
}
