using ADatabaseMigrator.Core;
using System;
using System.Collections.Generic;
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

        public Task<IReadOnlyList<MigrationScript>> Load()
        {
            var scripts = new List<MigrationScript>();
            foreach (var assemblyResources in Resources)
            {
                var embeddedResourceNames = assemblyResources.Assembly.GetManifestResourceNames();
                foreach (var (runtype, @namespace) in assemblyResources)
                {
                    foreach (var embeddedResource in embeddedResourceNames.Where(x => x.StartsWith(@namespace)))
                    {
                        //TODO: Extract version
                        //TODO: Extract string contents
                        //TODO: Extract name
                        //TODO: Extract hash
                        //TODO: Add migration script to list
                    }
                }
            }

            throw new NotImplementedException("TODO");
        }

        public IEmbeddedResourceAssemblyBuilder UsingAssembly(Assembly assembly)
        {
            var assemblyResources = new AssemblyEmbeddedResources(assembly);
            Resources.Add(assemblyResources);
            return new AssemblyBuilder(this, assemblyResources);
        }

        public IEmbeddedResourceAssemblyBuilder UsingAssemblyFromType<T>() where T : class => UsingAssembly(typeof(T).Assembly);

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
