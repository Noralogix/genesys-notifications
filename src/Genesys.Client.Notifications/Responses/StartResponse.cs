using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

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
                Expires = subscriptions.Expires,
                ChannelId = subscriptions.ChannelId,
                Topics = subscriptions.Items.Keys.ToArray()
            };
    }
}
