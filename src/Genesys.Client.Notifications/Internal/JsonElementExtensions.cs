using System.Text.Json;

namespace Genesys.Client.Notifications.Internal
{
    internal static class JsonElementExtensions
    {
        public static JsonElement? GetPropertyOrNull(this JsonElement element, string propertyName) =>
            element.TryGetProperty(propertyName, out var result) ? result : (JsonElement?)null;
    }
}
