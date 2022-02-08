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
        Task<UpdateExpiresResult> UpdateExpiresAsync();
        Channel Channel { get; }
        IGenesysConfig Config { get; }
        Type GetTopicType(string topicName);
        string[] GetTopics();
    }

    public class GenesysTopics : IGenesysTopicSubscriptions, IDisposable
    {
        public Dictionary<string, Type> Items { get; } = new Dictionary<string, Type>();
        public List<string> _topics = new List<string>();
        public Channel Channel { get; private set; }
        public IGenesysConfig Config { get; private set; }

        private GenesysNotifications _notifications;

        private readonly ILogger _logger;
        private readonly GenesysHttpClient _http;

        public GenesysTopics(IGenesysConfig config, ILogger logger)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _http = new GenesysHttpClient(config);
        }

        public GenesysTopics(GenesysHttpClient client, ILogger logger)
        {
            _http = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            Config = client.Config;
        }

        public GenesysTopics(IGenesysConfig config, ILogger logger, System.Net.Http.HttpClient httpClient)
        {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _http = new GenesysHttpClient(config, httpClient ?? throw new ArgumentNullException(nameof(httpClient)));
        }

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

        public async Task AddAsync<TTopic>(string topic, string[] ids = null)
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
            await _http.ReplaceSubscriptionsAsync(Channel, Items.Keys);
        }

        public virtual async Task<GenesysNotifications> CreateAsync(string name = "GenesysCloudWS")
        {
            Channel = await _http.CreateChannelAsync();

            await _http.AddSubscriptionsAsync(Channel, Items.Keys);
            _notifications = new GenesysNotifications(this, _logger, name);
            await _notifications.StartAsync();
            return _notifications;
        }

        public virtual async Task<UpdateExpiresResult> UpdateExpiresAsync()
        {
            if ((await _http.ChannelExistsAsync(Channel)))
            {
                await _http.ReplaceSubscriptionsAsync(Channel, Items.Keys);
                Channel.Expires = DateTime.UtcNow.AddHours(24).AddMinutes(-1);
                return new UpdateExpiresResult(Channel.ConnectUri);
            }
            else
            {
                Channel = await _http.CreateChannelAsync();
                await _http.AddSubscriptionsAsync(Channel, Items.Keys);
                return new UpdateExpiresResult(Channel.ConnectUri);
            }
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
        public UpdateExpiresResult(string connectUri)
        {
            ConnectUri = connectUri;
        }
        public string ConnectUri { get; private set; }
    }
}
