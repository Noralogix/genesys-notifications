using System;
using System.Linq;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Genesys.Client.Notifications.Clients
{
    public enum GetChanngelQuery
    {
        Token, OAuthClient
    }

    public class GenesysHttpClient : IDisposable
    {
        private readonly HttpClient _http;
        public IGenesysConfig Config { get; private set; }

        private DateTime? _tokenExpires;
        private GenesysAuthTokenInfo _token;

        private Func<HttpClient> _httpFactory;
        public DateTime? TokenExpires => _tokenExpires;

        private HttpClient Http => _httpFactory != null ? _httpFactory() : _http;

        public GenesysHttpClient(IGenesysConfig config, Func<HttpClient> httpFactory)
        {
            Config = config;
            _httpFactory = httpFactory;
        }

        public GenesysHttpClient(IGenesysConfig config) : this(config, new HttpClient())
        {
        }

        public GenesysHttpClient(IGenesysConfig config, HttpClient http)
        {
            _http = http;
            Config = config;
        }

        /// <summary>
        /// The list of existing channels
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<Channel[]> GetChannelsAsync(GetChanngelQuery query)
        {
            var authToken = await GetTokenAsync();

            var path = $"/api/v2/notifications/channels?includechannels={query}";
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.{Config.Environment}" + path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken.AccessToken);
            var response = await SendApiRequestAsync(request, "get channels"); ;
            try
            {
                var responseContent = await response.Content.ReadAsStreamAsync();
                var channels = await JsonSerializer.DeserializeAsync<ChannelsList>(responseContent);
                return channels?.Items ?? new Channel[] { };
            }
            catch (Exception ex)
            {
                throw new GenesysException($"Channels Deserialize: " + ex.Message);
            }
        }

        /// <summary>
        /// Create a new channel. There is a limit of 20 channels per user/app combination. 
        /// Creating a 21st channel will remove the channel with oldest last used date. 
        /// Channels without an active connection will be removed first.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="GenesysException"></exception>
        public virtual async Task<Channel> CreateChannelAsync()
        {
            var authToken = await GetTokenAsync();
            var path = "/api/v2/notifications/channels";
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.{Config.Environment}" + path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken.AccessToken);
            var response = await SendApiRequestAsync(request, "create channel");
            try
            {
                var responseContent = await response.Content.ReadAsStreamAsync();
                var channel = await JsonSerializer.DeserializeAsync<Channel>(responseContent);
                return channel;
            }
            catch (Exception ex)
            {
                throw new GenesysException($"Channel Deserialize: " + ex.Message);
            }
        }

        /// <summary>
        /// Add a list of subscriptions to the existing list of subscriptions
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="topics"></param>
        /// <returns></returns>
        public virtual async Task AddSubscriptionsAsync(Channel channel, IEnumerable<string> topics)
        {
            var authToken = await GetTokenAsync();

            var path = $"/api/v2/notifications/channels/{channel.Id}/subscriptions";
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.{Config.Environment}" + path);
            var data = string.Join(",", topics.Select(t => "{" + $"\"id\":\"{t}\"" + "}").ToArray()).TrimEnd(',');
            var body = $"[{data}]";
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken.AccessToken);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            await SendApiRequestAsync(request, "create channel subscriptions");
        }

        /// <summary>
        /// Verify a channel still exists and is valid. Returns a 200 OK if channel exists, and a 404 Not Found if it doesn't
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public async Task VerifyChannelExists(Channel channel)
        {
            var authToken = await GetTokenAsync();
            var path = $"/api/v2/notifications/channels/{channel.Id}";
            var request = new HttpRequestMessage(HttpMethod.Head, $"https://api.{Config.Environment}" + path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken.AccessToken);
            await SendApiRequestAsync(request, "Verify a channel still exists");
        }

        public async Task RemoveAllSubscriptionsAsync(Channel channel)
        {
            var authToken = await GetTokenAsync();
            var path = $"/api/v2/notifications/channels/{channel.Id}/subscriptions";
            var request = new HttpRequestMessage(HttpMethod.Delete, $"https://api.{Config.Environment}" + path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken.AccessToken);
            await SendApiRequestAsync(request, "remove all channel subscriptions");
        }

        /// <summary>
        /// Replace the current list of subscriptions with a new list 
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="topics"></param>
        /// <returns></returns>
        public async Task ReplaceSubscriptionsAsync(Channel channel, IEnumerable<string> topics)
        {
            var authToken = await GetTokenAsync();

            var path = $"/api/v2/notifications/channels/{channel.Id}/subscriptions";
            var request = new HttpRequestMessage(HttpMethod.Put, $"https://api.{Config.Environment}" + path);
            var data = string.Join(",", topics.Select(t => "{" + $"\"id\":\"{t}\"" + "}").ToArray()).TrimEnd(',');
            var body = $"[{data}]";
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken.AccessToken);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            await SendApiRequestAsync(request, "Updated channel subscriptions");
        }

        public async Task<GenesysAuthTokenInfo> GetTokenAsync(bool reload = false)
        {
            var dt = DateTime.UtcNow;
            GenesysAuthTokenInfo newToken = null;
            if (reload || _token == null || _tokenExpires.HasValue && _tokenExpires.Value.AddMinutes(-2) <= dt)
            {
                newToken = await GetTokenClientCredentialsAsync();
                _token = newToken;
                _tokenExpires = dt.AddSeconds(newToken.ExpiresIn ?? 1000);
            }
            return newToken ?? _token;
        }

        public async Task<GenesysAuthTokenInfo> GetTokenClientCredentialsAsync()
        {
            var path = "/oauth/token";

            var basicAuth = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                .GetBytes($"{Config.ClientId}:{Config.ClientSecret}"));
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://login.{Config.Environment}" + path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);
            var content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
            });
            request.Content = content;
            var response = await Http.SendAsync(request);

            try
            {
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStreamAsync();
                var authTokenInfo = await JsonSerializer.DeserializeAsync<GenesysAuthTokenInfo>(responseContent);
                return authTokenInfo;
            }
            catch (HttpRequestException ex)
            {
                int statusCode = (int)response.StatusCode;
                throw new Exception($"Error calling Genesys API PostToken. Status Code {statusCode}.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Get Genesys token error.", ex);
            }
        }

        private async Task<HttpResponseMessage> SendApiRequestAsync(HttpRequestMessage request, string name)
        {
            var response = await Http.SendAsync(request);
            try
            {
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException ex)
            {
                int statusCode = (int)response.StatusCode;
                if (statusCode >= 400)
                {
                    var exMessage = ex.Message;
                    string responseContent = null;
                    try
                    {
                        responseContent = await response.Content.ReadAsStringAsync();
                        exMessage = responseContent;
                    }
                    catch
                    {
                    }
                    throw new GenesysException(statusCode, responseContent, $"Error calling '{name}': {exMessage}", request.Headers.ToDictionary(rh => rh.Key, rh => rh.Value?.FirstOrDefault()));
                }
                else
                    throw new GenesysException(statusCode, $"Error calling '{name}': " + ex.Message);
            }
            catch (Exception ex)
            {
                throw new GenesysException($"Error calling {name}: " + ex.Message);
            }

            return response;
        }

        public void Dispose()
        {
            Http?.Dispose();
        }
    }
}
