
using System;
using System.Collections.Generic;
using System.Linq;
using Mono.Cecil;

namespace RealmWeaver
{
    public class WeaverAssemblyResolver : BaseAssemblyResolver
    {
        private readonly IDictionary<string, string> _appDomainAssemblyLocations;
        private readonly IDictionary<string, AssemblyDefinition> _cache;

        public WeaverAssemblyResolver()
        {
            _appDomainAssemblyLocations = new Dictionary<string, string>();
            _cache = new Dictionary<string, AssemblyDefinition>();

            var domain = AppDomain.CurrentDomain;

            var assemblies = domain.GetAssemblies();
            foreach (var assembly in domain.GetAssemblies().Where(a => !a.ReflectionOnly && !a.IsDynamic))
            {

                _appDomainAssemblyLocations[assembly.FullName] = assembly.Location;
                AddSearchDirectory(System.IO.Path.GetDirectoryName(assembly.Location));
            }
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name)
        {
            var assemblyDef = FindAssemblyDefinition(name.FullName, null);

            if (assemblyDef == null)
            {
                assemblyDef = base.Resolve(name);
                _cache[name.FullName] = assemblyDef;
            }

            return assemblyDef;
        }

        public override AssemblyDefinition Resolve(AssemblyNameReference name, ReaderParameters parameters)
        {
            var assemblyDef = FindAssemblyDefinition(name.FullName, parameters);

            if (assemblyDef == null)
            {
                assemblyDef = base.Resolve(name, parameters);
                _cache[name.FullName] = assemblyDef;
            }

            return assemblyDef;
        }

        private AssemblyDefinition FindAssemblyDefinition(string fullName, ReaderParameters parameters)
        {
            if (_cache.TryGetValue(fullName, out var assemblyDefinition))
            {
                return assemblyDefinition;
            }

            if (_appDomainAssemblyLocations.TryGetValue(fullName, out var location))
            {
                assemblyDefinition = parameters != null ? AssemblyDefinition.ReadAssembly(location, parameters) : AssemblyDefinition.ReadAssembly(location);

                _cache[fullName] = assemblyDefinition;

                return assemblyDefinition;
            }

            return null;
        }
    }
}
