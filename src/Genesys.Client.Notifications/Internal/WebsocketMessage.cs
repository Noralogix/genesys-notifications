using Genesys.Client.Notifications.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace Genesys.Client.Notifications.Responses
{
    internal partial class WebsocketMessage
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

        public string EventBody() => _root
            .GetPropertyOrNull("eventBody")?
            .GetString();

        public WebsocketMessage(JsonElement root, string raw)
        {
            _root = root;
            _raw = raw;
        }
    }

    internal partial class WebsocketMessage
    {
        public static WebsocketMessage Parse(string raw) => new WebsocketMessage(
            Json.TryParse(raw) ?? throw new Exception("Genesys WebSocket message is broken. Cant parse json."), raw
        );
    }
}
