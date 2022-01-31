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
        Task<UpdateExpiresResult> UpdateExpiresAsync();
        DateTime Expires { get; }
        bool IsExpired { get; }
        IGenesysConfig Config { get; }
        Type GetTopicType(string topicName);
        string[] GetTopics();
    }

    public class GenesysTopics : IGenesysTopicSubscriptions, IDisposable
    {
        private const int limit = 1000;
        public Dictionary<string, Type> Items { get; } = new Dictionary<string, Type>();
        public List<string> _topics = new List<string>();

        public string ChannelId => Channel?.Id;
        public string ChannelURI => Channel?.ConnectUri;
        public Channel Channel { get; private set; }
        public DateTime Expires { get; private set; }
        public IGenesysConfig Config { get; private set; }

        private GenesysNotifications _notifications;

        private readonly ILogger _logger;
        private readonly GenesysHttpClient _http;

        public GenesysTopics(GenesysConfig config, ILogger logger)
        {
            Config = Config;
            Expires = DateTime.UtcNow.AddHours(config.ChannelExpiresHours).AddMinutes(-1);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _http = new GenesysHttpClient(config ?? throw new ArgumentNullException(nameof(config)));
        }

        public GenesysTopics(GenesysHttpClient genesysClient, ILogger logger, int channelExpiresHours = 23)
        {
            Config = Config;
            Expires = DateTime.UtcNow.AddHours(channelExpiresHours).AddMinutes(-1);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _http = genesysClient;
        }

        public GenesysTopics(GenesysConfig config, ILogger logger, System.Net.Http.HttpClient httpClient, int topicsCapacity = 100)
        {
            Items = new Dictionary<string, Type>(topicsCapacity);
            Config = Config;
            Expires = DateTime.UtcNow.AddHours(config.ChannelExpiresHours).AddMinutes(-1);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _http = new GenesysHttpClient(config ?? throw new ArgumentNullException(nameof(config))
                , httpClient ?? throw new ArgumentNullException(nameof(httpClient)));
        }

        public GenesysTopics(GenesysAuthTokenConfig config, ILogger logger)
        {
            Config = Config;
            Expires = DateTime.UtcNow.AddHours(config.ChannelExpiresHours).AddMinutes(-1);

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _http = new GenesysHttpClient(config ?? throw new ArgumentNullException(nameof(config)));
        }

        public GenesysTopics(GenesysAuthTokenConfig config, ILogger logger, System.Net.Http.HttpClient httpClient)
        {
            Config = Config;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _http = new GenesysHttpClient(config ?? throw new ArgumentNullException(nameof(config))
                , httpClient ?? throw new ArgumentNullException(nameof(httpClient)));
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

        /// <summary>
        /// Non typed subscribtion
        /// </summary>
        /// <param name="topic">Genesys Cloud Topic</param>
        /// <param name="ids">Genesys Cloud id's</param>
        /// <returns></returns>
        public GenesysTopics Add(string topic, string[] ids = null)
        {
            if (_notifications != null) throw new Exception("Cant add topic. Already subscribed.");
            if (ids == null) _topics.Add(topic);
            else
                foreach (var topicWithId in ids.Select(id => topic.Replace("{id}", id)))
                {
                    if (!_topics.Contains(topicWithId))
                        _topics.Add(topicWithId);
                }
            return this;
        }

        public Task UpdateAsync(string topic, string[] ids = null)
        {
            if (_notifications == null) throw new Exception("Cant update topic. Channel have not created.");
            return Task.CompletedTask;
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

        public virtual async Task<GenesysNotifications> CreateAsync(string name = "GenesysCloudWS")
        {
            Channel = await _http.CreateChannelAsync();

            await _http.CreateSubscriptionsAsync(Channel, Items.Keys);
            _notifications = new GenesysNotifications(this, _logger, name);
            await _notifications.StartAsync();
            return _notifications;
        }

        public bool IsExpired => Expires <= DateTime.UtcNow;

        public virtual async Task<UpdateExpiresResult> UpdateExpiresAsync()
        {
            try
            {                
                await _http.UpdateSubscriptionsAsync(Channel, Items.Keys);
                Expires = DateTime.UtcNow.AddHours(_http.Config.ChannelExpiresHours).AddMinutes(-1);
                return new UpdateExpiresResult
                {
                    IsNewChannel = false,
                    IsUpdated = true,
                };
            }
            catch (GenesysException gex)
            {
                if (gex.ErrorCode == 400)
                {
                    var error = System.Text.Json.JsonSerializer.Deserialize<ErrorMessage>(gex.ErrorContent);
                    if (error.IsNotificationChannelIdError())
                    {
                        _logger.LogWarning("Unable to update channel subscriptions {Message} {Status} {Code} {ContextId}", error.Message, error.Status, error.Code, error.ContextId);
                        Channel = await _http.CreateChannelAsync();
                        Expires = Channel.Expires ??  DateTime.UtcNow.AddHours(_http.Config.ChannelExpiresHours).AddMinutes(-1);
                        await _http.CreateSubscriptionsAsync(Channel, Items.Keys);
                        return new UpdateExpiresResult
                        {
                            IsNewChannel = true,
                            IsUpdated = true,
                            Channel = Channel
                        };
                    }
                }
                _logger.LogError(gex, "Can't update channel subscriptions {ChannelId}", Channel.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Can't update channel subscriptions {ChannelId}", Channel.Id);
            }
            return new UpdateExpiresResult
            {
                IsNewChannel = false,
                IsUpdated = false
            };
        }

        public void Dispose()
        {
            _http.Dispose();
        }

        public Type GetTopicType(string topicName) => Items.ContainsKey(topicName) ? Items[topicName] : null;
        public string[] GetTopics() => Items.Keys.ToArray();
    }

    public class UpdateExpiresResult
    {
        public bool IsUpdated { get; set; }
        public bool IsNewChannel { get; set; }
        public Channel Channel { get; set; }
    }
}
