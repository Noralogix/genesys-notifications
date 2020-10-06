using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Genesys.Client.Notifications.Clients;

namespace Genesys.Client.Notifications
{
    public interface IGenesysTopicSubscriptions
    {
        string ChannelURI { get; }
        string ChannelId { get; }
        int ExpiresHours { get; }
        Task UpdateExpiresAsync();
        DateTime Expires { get; }
        bool IsExpired { get; }
        Dictionary<string, Type> Items { get; }
    }

    public class GenesysTopics : IGenesysTopicSubscriptions, IDisposable
    {
        private const int limit = 1000;        
        public Dictionary<string, Type> Items { get; } = new Dictionary<string, Type>();
        public string ChannelId => Channel?.Id;
        public string ChannelURI => Channel?.ConnectUri;
        public Channel Channel { get; private set; }
        public DateTime Expires { get; private set; }
        public int ExpiresHours => _config.ChannelExpiresHours;

        private GenesysNotifications _notifications;

        private readonly GenesysConfig _config;
        private readonly ILogger _logger;
        private readonly GenesysHttpClient _http;

        public GenesysTopics(GenesysConfig config, ILogger logger)
        {
            _config = config;
            _http = new GenesysHttpClient(config);
            _logger = logger;
            Expires = DateTime.UtcNow.AddHours(ExpiresHours).AddMinutes(-1);
        }

        //public GenesysTopicSubscriptions(Channel channel, Dictionary<string, Type> topicSubscriptions, int expiresHours)
        //{
        //    if (channel == null) throw new ArgumentNullException(nameof(channel));
        //    if (topicSubscriptions == null) throw new ArgumentNullException(nameof(topicSubscriptions));
        //    if (topicSubscriptions.Count == 0) throw new ArgumentException("Empty topic subscriptions.");
        //    if (topicSubscriptions.Count > topicsLimit) throw new ArgumentException("Topics limit exceeded.");
        //    if (expiresHours <= 0) throw new ArgumentException("Expires hours should be more than 0.");

        //    Channel = channel;
        //    UpdateExpiresAsync();
        //    Items = topicSubscriptions;
        //}        

        public GenesysTopics Add<T>(string topic, string[] ids)
        {
            if (_notifications != null) throw new Exception("Cant add topic. Already subscribed.");
            foreach (var topicWithId in ids.Select(id => topic.Replace("{id}", id)))
            {
                Items.Add(topicWithId, typeof(T));
            }
            return this;
        }

        public async Task<GenesysNotifications> CreateAsync()
        {
            var token = await _http.GetTokenAsync();
            Channel = await _http.CreateChannelAsync(token);

            await _http.CreateSubscriptionsAsync(token, Channel, Items.Keys);
            _notifications = new GenesysNotifications(this, _logger);
            await _notifications.StartAsync();
            return _notifications;
        }

        public bool IsExpired => Expires <= DateTime.UtcNow;

        public async Task UpdateExpiresAsync()
        {
            Expires = DateTime.UtcNow.AddHours(ExpiresHours).AddMinutes(-1);
            var token = await _http.GetTokenAsync();
            await _http.UpdateSubscriptionsAsync(token, Channel, Items.Keys);
        }

        public void Dispose()
        {
            _http.Dispose();
        }
    }
}
