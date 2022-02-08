using System;

namespace Genesys.Client.Notifications.Responses
{
    public class StartResponse
    {
        public Channel Channel { get; set; }
        public string[] Topics { get; set; }

        internal static StartResponse Create(IGenesysTopicSubscriptions subscriptions)
            => new StartResponse
            {
                Channel = subscriptions.Channel,
                Topics = subscriptions.GetTopics(),
            };
    }
}
