using Genesys.Client.Notifications.Internal;
using System;
using System.Text.Json;

namespace Genesys.Client.Notifications.Responses
{
    internal partial class GenesysMessage
    {
        private readonly JsonElement _root;
        private readonly string _raw;
        public string TopicName() => _root
            .GetPropertyOrNull("topicName")?
            .GetString();

        public string Message() => _root
            .GetPropertyOrNull("eventBody")?
            .GetPropertyOrNull("message")?
            .GetString();

        public JsonElement? EventBody() => _root
            .GetPropertyOrNull("eventBody");

        public GenesysMessage(JsonElement root, string raw)
        {
            _root = root;
            _raw = raw;
        }
    }

    internal partial class GenesysMessage
    {
        public static GenesysMessage Parse(string raw) => new GenesysMessage(
            Json.TryParse(raw) ?? throw new Exception("Genesys WebSocket message is broken. Cant parse json."), raw
        );
    }
}
