using System;
using System.Text.Json;

namespace Genesys.Client.Notifications.Internal
{
    internal static class JsonElementExtensions
    {
        public static JsonElement? GetPropertyOrNull(this JsonElement element, string propertyName) =>
            element.TryGetProperty(propertyName, out var result) ? result : (JsonElement?)null;

        //public static object ToObject(this JsonElement element, Type returnType, JsonSerializerOptions options = null)
        //{

        //    var bufferWriter = new ArrayBufferWriter();
        //    using (var writer = new Utf8JsonWriter(bufferWriter))
        //    {
        //        element.WriteTo(writer);
        //    }

            //    return JsonSerializer.Deserialize(bufferWriter.WrittenSpan, returnType, options);
            //}
        }
}
