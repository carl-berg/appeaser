using System;
using System.Collections.Generic;
using System.Reflection;

namespace Appeaser.Microsoft.DependencyInjection
{
    internal class TypeScanner
    {
        private readonly IEnumerable<Assembly> _assemblies;

        public TypeScanner(IEnumerable<Assembly> assemblies)
        {
            _assemblies = assemblies;
        }

        public IEnumerable<ResolvedHandler> ResolveOpenTypes(Type openType)
        {
            foreach (var assembly in _assemblies)
            {
                foreach (var type in assembly.DefinedTypes)
                {
                    foreach (var @interface in type.GetInterfaces())
                    {
                        if (@interface.IsGenericType && openType.IsAssignableFrom(@interface.GetGenericTypeDefinition()))
                        {
                            yield return new ResolvedHandler(@interface, type.AsType());
                        }
                    }
                }
            }
        }

        internal class ResolvedHandler
        {
            public ResolvedHandler(Type @interface, Type handler)
            {
                InterfaceType = @interface;
                HandlerType = handler;
            }

            public Type InterfaceType { get; }
            public Type HandlerType { get; }
        }
    }
}
