using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

namespace Legato.Bus.Azure.Extensions
{
    class TimeSpanConverter : JsonConverter<TimeSpan>
    {
        public override TimeSpan Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return TimeSpan.Parse(reader.GetString() ?? "00:00:00");
        }

        public override void Write(Utf8JsonWriter writer, TimeSpan value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }

    static class JsonExtensions
    {
        public static JsonSerializerOptions DefaultOptions => 
            new()
            {
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
                Converters =
                {
                    new TimeSpanConverter()
                }
            };

        public static string Serialize<T>(T obj) =>
            JsonSerializer.Serialize(obj, DefaultOptions);

        public static string Serialize<T>(T obj, Type type) =>
            JsonSerializer.Serialize(obj, type, DefaultOptions);

        public static T Deserialize<T>(string json) =>
            JsonSerializer.Deserialize<T>(json, DefaultOptions);

        public static object Deserialize(string json, Type consumedType) => 
            JsonSerializer.Deserialize(json, consumedType);
    }
}
