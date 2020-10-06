using System;
using System.Linq;
using System.Collections.Generic;

namespace Genesys.Client.Notifications
{
    public class GenesysTopicSubscriptions
    {
        private const int topicsLimit = 1000;
        public Uri Uri => new Uri(Channel.ConnectUri);

        internal Dictionary<string, Type> Items { get; }
        internal IEnumerable<string> Topics => Items.Keys;
        public string ChannelId => Channel.Id;
        public Channel Channel { get; }
        public DateTime Expires { get; set; }
        private readonly int _expiresHours;
        public GenesysTopicSubscriptions(Channel channel, Dictionary<string, Type> topicSubscriptions, int expiresHours)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (topicSubscriptions == null) throw new ArgumentNullException(nameof(topicSubscriptions));
            if (topicSubscriptions.Count == 0) throw new ArgumentException("Empty topic subscriptions.");
            if (topicSubscriptions.Count > topicsLimit) throw new ArgumentException("Topics limit exceeded.");
            if (expiresHours <= 0) throw new ArgumentException("Expires hours should be more than 0.");

            _expiresHours = expiresHours;
            Channel = channel;
            UpdateExpires();
            Items = topicSubscriptions;
        }

        public bool IsExpired => Expires <= DateTime.UtcNow;

        public void UpdateExpires()
        {
            Expires = DateTime.UtcNow.AddHours(_expiresHours).AddMinutes(-1);
        }
    }
}
