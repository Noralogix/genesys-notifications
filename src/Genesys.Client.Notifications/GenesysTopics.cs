using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Genesys.Client.Notifications.Clients;

namespace Genesys.Client.Notifications
{
    public interface IGenesysChannels
    {
        List<Channel> Channels { get; }
        DateTime Expires { get; }
    }
    public interface IGenesysTopicSubscriptions : IGenesysChannels
    {
        Task UpdateAsync();
        IGenesysConfig Config { get; }
        Type GetTopicType(string topicName);
        string[] GetTopics();
    }

    public class GenesysTopics : IGenesysTopicSubscriptions, IDisposable
    {
        private const int _topicsAmountPerChannel = 1000;
        public Dictionary<string, Type> Items { get; } = new Dictionary<string, Type>();
        public List<string> _topics = new List<string>();
        public DateTime Expires => Channels.Min(c => c.Expires);
        public List<Channel> Channels { get; } = new List<Channel>();
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
        public GenesysTopics Add<TTopic>(string topic, string[] ids = null) => Add(topic, typeof(TTopic), ids);

        /// <summary>
        /// Non typed subscribtion
        /// </summary>
        /// <param name="topic">Genesys Cloud Topic</param>
        /// <param name="ids">Genesys Cloud id's</param>
        /// <returns></returns>
        public GenesysTopics Add(string topic, string[] ids = null)
        {
            if (ids == null)
            {
                if (!Items.ContainsKey(topic)) _topics.Add(topic);
            }
            else
                foreach (var topicWithId in ids.Select(id => topic.Replace("{id}", id)))
                {
                    if (!_topics.Contains(topicWithId))
                        _topics.Add(topicWithId);
                }
            return this;
        }

        /// <summary>
        /// Available topics https://developer.genesys.cloud/api/rest/v2/notifications/available_topics        
        /// </summary>
        /// <param name="topic">Genesys Cloud Topic</param>
        /// <param name="topicType">Topic Type</param>
        /// <param name="ids">Genesys Cloud id's</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public GenesysTopics Add(string topic, Type topicType, string[] ids = null)
        {
            if (ids == null)
            {
                if (!Items.ContainsKey(topic)) Items.Add(topic, topicType);
            }
            else
            {
                foreach (var topicWithId in ids.Select(id => topic.Replace("{id}", id)))
                {
                    if (!Items.ContainsKey(topicWithId)) Items.Add(topicWithId, topicType);
                }
            }
            return this;
        }

        public async Task UpdateAsync<TTopic>(string topic, string[] ids = null)
        {
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
            await UpdateAsync();
        }


        public virtual async Task<GenesysNotifications> CreateAsync(string name = "GenesysCloudWS")
        {
            _logger.LogDebug("Genesys notifications topics {TopicsCount}", Items.Count);

            foreach (var items in Items.Keys.Chunk(_topicsAmountPerChannel))
            {
                var channel = await _http.CreateChannelAsync();
                Channels.Add(channel);
                await _http.AddSubscriptionsAsync(channel, items);
            }
            _notifications = new GenesysNotifications(this, _logger, name);
            await _notifications.StartAsync();
            return _notifications;
        }

        public virtual async Task UpdateAsync()
        {
            await UpdateAsync(true, false);
        }

        private async Task UpdateAsync(bool handleUnauthorize = false, bool reloadToken = false)
        {
            try
            {
                Channels.Clear();
                foreach (var items in Items.Keys.Chunk(1000))
                {
                    var channel = await _http.CreateChannelAsync();
                    Channels.Add(channel);
                    await _http.AddSubscriptionsAsync(channel, items);
                }
            }
            catch (GenesysException gex)
            {
                if (handleUnauthorize && gex.ErrorCode == 401)
                {
                    await UpdateAsync(handleUnauthorize: false, reloadToken: true);
                }
                else
                    throw gex;
            }
        }
        public Func<string, Type> TopicTypeResolver { get; set; } = _ => null;
        public Type GetTopicType(string topicName) => Items.ContainsKey(topicName) ? Items[topicName] : TopicTypeResolver(topicName);
        public string[] GetTopics() => Items.Keys.ToArray();
        public void Dispose()
        {
            _http.Dispose();
        }
    }
}
