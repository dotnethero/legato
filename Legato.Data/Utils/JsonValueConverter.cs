using System;
using Legato.Common.Extensions;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Legato.Data.Utils
{
    class JsonValueConverter<T>: ValueConverter<T, string>
    {
        public JsonValueConverter() : base(
            data => JsonExtensions.Serialize(data),
            json => JsonExtensions.Deserialize<T>(json)) { }
    }

    class SystemTypeConverter : ValueConverter<Type, string>
    {
        public SystemTypeConverter() : base(
            type => type.AssemblyQualifiedName!,
            typeName => Type.GetType(typeName)!) { }
    }

    public static class PropertyBuilderExtensions
    {
        public static void HasJsonConversion<T>(this PropertyBuilder<T> builder) => 
            builder.HasConversion(new JsonValueConverter<T>());

        public static void HasSystemTypeConversion(this PropertyBuilder<Type> builder) =>
            builder.HasConversion(new SystemTypeConverter());
    }
}
