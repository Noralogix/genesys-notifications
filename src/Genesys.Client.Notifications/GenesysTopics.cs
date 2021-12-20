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

        private GenesysNotifications _notifications;

        private readonly IGenesysConfig _config;
        private readonly ILogger _logger;
        private readonly GenesysHttpClient _http;

        public GenesysTopics(GenesysConfig config, ILogger logger)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _http = new GenesysHttpClient(config);
            Expires = DateTime.UtcNow.AddHours(config.ChannelExpiresHours).AddMinutes(-1);
        }

        public GenesysTopics(GenesysConfig config, ILogger logger, System.Net.Http.HttpClient httpClient)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _http = new GenesysHttpClient(config, httpClient ?? throw new ArgumentNullException(nameof(httpClient)));
            Expires = DateTime.UtcNow.AddHours(config.ChannelExpiresHours).AddMinutes(-1);
        }

        public GenesysTopics(GenesysAuthTokenConfig config, ILogger logger, System.Net.Http.HttpClient httpClient)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _http = new GenesysHttpClient(config, httpClient ?? throw new ArgumentNullException(nameof(httpClient)));
            Expires = DateTime.UtcNow.AddHours(config.ChannelExpiresHours).AddMinutes(-1);
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

        /// <summary>
        /// Available topics https://developer.genesys.cloud/api/rest/v2/notifications/available_topics
        /// </summary>
        /// <typeparam name="TTopic">Topic Type</typeparam>
        /// <param name="topic">Genesys Cloud Topic</param>
        /// <param name="ids">Genesys Cloud id's</param>
        /// <returns></returns>
        public GenesysTopics Add<TTopic>(string topic, string[] ids = null)
        {
            if (_notifications != null) throw new Exception("Cant add topic. Already subscribed.");
            if (ids == null)
            {
                Items.Add(topic, typeof(TTopic));
            }
            else
            {
                foreach (var topicWithId in ids.Select(id => topic.Replace("{id}", id)))
                {
                    if (!Items.ContainsKey(topicWithId))
                        Items.Add(topicWithId, typeof(TTopic));
                }
            }
            return this;
        }

        public async Task UpdateAsync<TTopic>(string topic, string[] ids = null)
        {
            if (_notifications == null) throw new Exception("Cant update topic. Channel have not created.");
            if (ids == null)
            {
                Items.Add(topic, typeof(TTopic));
            }
            else
            {
                foreach (var topicWithId in ids.Select(id => topic.Replace("{id}", id)))
                {
                    if (!Items.ContainsKey(topicWithId))
                        Items.Add(topicWithId, typeof(TTopic));
                }
            }            
            await _http.UpdateSubscriptionsAsync(Channel, Items.Keys);
        }

        public virtual async Task<GenesysNotifications> CreateAsync()
        {
            Channel = await _http.CreateChannelAsync();

            await _http.CreateSubscriptionsAsync(Channel, Items.Keys);
            _notifications = new GenesysNotifications(this, _logger);
            await _notifications.StartAsync();
            return _notifications;
        }

        public bool IsExpired => Expires <= DateTime.UtcNow;

        public virtual async Task UpdateExpiresAsync()
        {
            try
            {
                Expires = DateTime.UtcNow.AddHours(_config.ChannelExpiresHours).AddMinutes(-1);
                await _http.UpdateSubscriptionsAsync(Channel, Items.Keys);
            }
            catch (GenesysException gex)
            {
                if (gex.ErrorCode == 400)
                {
                    var error = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(gex.ErrorContent);
                    _logger.LogWarning("Unable to update channel subscriptions {Message} {Status} {Code} {ContextId}", error.Message, error.Status, error.Code, error.ContextId);
                    if (error.IsNotificationChannelIdError())
                    {
                        Channel = await _http.CreateChannelAsync();
                        await _http.CreateSubscriptionsAsync(Channel, Items.Keys);
                    }
                    else
                        throw;
                }
                else
                    throw;
            }
            catch
            {
                throw;
            }
        }

        public void Dispose()
        {
            _http.Dispose();
        }
    }

    public class UpdateExpiresResult
    {
        public bool IsNewChannel { get; set; }
    }
}
