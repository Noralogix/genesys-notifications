using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text.Json;

namespace Genesys.Client.Notifications.Responses
{
    internal static class SubscriptionResponse
    {
        internal static bool TryHandle(GenesysMessage message, ISubject<object> subject, GenesysTopicSubscriptions topics)
        {
            var topicName = message.TopicName();
            if (topics.Items.ContainsKey(topicName))
            {
                var body = message.EventBody();
                var data = JsonSerializer.Deserialize(body.Value.GetRawText(), topics.Items[topicName]);
                if (data != null) subject.OnNext(data);
                return true;
            }
            return false;
        }
    }
}
