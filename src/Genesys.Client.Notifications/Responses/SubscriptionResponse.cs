using System;
using System.Reactive.Subjects;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Genesys.Client.Notifications.Responses
{
    internal static class SubscriptionResponse
    {
        private static Lazy<JsonSerializerOptions> _options = new Lazy<JsonSerializerOptions>(() =>
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            return options;
        });

        internal static bool TryHandle(GenesysMessage message, ISubject<object> subject, IGenesysTopicSubscriptions topics)
        {
            var topicName = message.TopicName();
            var topicType = topics.GetTopicType(topicName);
            if (topicType == null) return false;
            if (topicType.BaseType == typeof(NotificationData<string>))
            {
                var data = Activator.CreateInstance(topicType);
                var ndata = (NotificationData<string>)data;
                ndata.EventBody = message.Raw;
                ndata.TopicName = message.TopicName();
                ndata.Version = message.Version();
                subject.OnNext(data);
            }
            else if (topicType.IsGenericType && topicType.GetGenericTypeDefinition() == typeof(NotificationData<>))
            {
                var data = JsonSerializer.Deserialize(message.Raw, topicType, _options.Value);
                if (data != null) subject.OnNext(data);
            }
            else
            {
                var body = message.EventBody();
                var data = JsonSerializer.Deserialize(body.Value.GetRawText(), topicType, _options.Value);
                if (data != null) subject.OnNext(data);
            }
            return true;
        }
    }
}
