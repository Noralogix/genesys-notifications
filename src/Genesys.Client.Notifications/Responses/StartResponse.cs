using System;

namespace Genesys.Client.Notifications.Responses
{
    public class StartResponse
    {
        public DateTime Expires { get; set; }
        public string ChannelId { get; set; }
        public string[] Topics { get; set; }

        internal static StartResponse Create(IGenesysTopicSubscriptions subscriptions)
            => new StartResponse
            {
                Expires = subscriptions.Channel.Expires,
                ChannelId = subscriptions.Channel.Id,
                Topics = subscriptions.GetTopics()
            };
    }
}
