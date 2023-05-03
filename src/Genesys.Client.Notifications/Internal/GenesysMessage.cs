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

        public string Version() => _root
            .GetPropertyOrNull("version")?
            .GetString();

        public string Message() => _root
            .GetPropertyOrNull("eventBody")?
            .GetPropertyOrNull("message")?
            .GetString();

        public JsonElement? EventBody() => _root
            .GetPropertyOrNull("eventBody");

        public string Raw => _raw;

        public string ChannelId { get; }
        public GenesysMessage(string channelId, JsonElement root, string raw)
        {
            _root = root;
            _raw = raw;
            ChannelId = channelId;
        }
    }

    internal partial class GenesysMessage
    {
        public static GenesysMessage Parse(string raw, string channelId) => new GenesysMessage(channelId,
            Json.TryParse(raw) ?? throw new Exception("Genesys WebSocket message is broken. Cant parse json."), raw
        );
    }
}
