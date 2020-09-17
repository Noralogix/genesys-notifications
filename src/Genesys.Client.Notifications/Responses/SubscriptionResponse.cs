using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text.Json;

namespace Genesys.Client.Notifications.Responses
{
    internal static class SubscriptionResponse
    {
        internal static bool TryHandle(GenesysMessage message, ISubject<object> subject, Dictionary<string, Type> topics)
        {
            var topicName = message.TopicName();
            if (topics.ContainsKey(topicName))
            {
                var data = JsonSerializer.Deserialize(message.EventBody(), topics[topicName]);
                if (data != null) subject.OnNext(data);
                return true;
            }
            return false;
        }
    }
}
