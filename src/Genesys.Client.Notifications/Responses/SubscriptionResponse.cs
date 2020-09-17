using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text.Json;

namespace Genesys.Client.Notifications.Responses
{
    internal static class SubscriptionResponse
    {
        internal static bool TryHandle(WebsocketMessage response, ISubject<object> subject, Dictionary<string, Type> subscriptions)
        {
            var topicName = response.TopicName();
            if (subscriptions.ContainsKey(topicName))
            {
                var data = JsonSerializer.Deserialize(response.EventBody(), subscriptions[topicName]);
                if (data != null) subject.OnNext(data);
                return true;
            }
            return false;
        }
    }
}
