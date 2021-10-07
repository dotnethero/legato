using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Legato.Bus.Extensions;
using Legato.CQRS;

namespace Legato.Bus.Azure.Extensions
{
    static class ReflectionExtensions
    {
        public static Type[] GetRoutedEvents(this AppDomain domain) =>
            domain
                .GetAssemblies()
                .SelectMany(GetRoutedEvents)
                .ToArray();

        public static Type[] GetRoutedCommands(this AppDomain domain) =>
            domain
                .GetAssemblies()
                .SelectMany(GetRoutedCommands)
                .ToArray();

        static IEnumerable<Type> GetRoutedCommands(Assembly assembly) => 
            assembly
                .GetTypes()
                .Where(t => t.IsAssignableTo(typeof(DomainCommand)) && t.IsRoutedTo());

        static IEnumerable<Type> GetRoutedEvents(Assembly assembly) => 
            assembly
                .GetTypes()
                .Where(t => t.IsAssignableTo(typeof(DomainEvent)) && t.IsRouted());
    }
}
