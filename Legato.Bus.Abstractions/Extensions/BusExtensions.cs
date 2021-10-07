using System;
using System.Reflection;
using Legato.Bus.Attributes;
using Legato.CQRS;

namespace Legato.Bus.Extensions
{
    public static class BusExtensions
    {
        public static bool IsRouted<T>(this T data) 
            where T : DomainEvent =>
            data
                .GetType()
                .IsRouted();

        public static bool IsRouted(this Type type) =>
            type.GetCustomAttribute(typeof(RoutedAttribute)) is not null;

        public static bool IsRoutedTo<T>(this T data)
            where T : DomainCommand =>
            data
                .GetType()
                .IsRoutedTo();

        public static bool IsRoutedTo(this Type type) =>
            type.GetCustomAttribute(typeof(RoutedToAttribute)) is not null;
    }
}