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
        private readonly IGenesysConfig _config;

        public GenesysHttpClient(IGenesysConfig config) : this(config, new HttpClient())
        {
        }

        public GenesysHttpClient(IGenesysConfig config, HttpClient http)
        {
            _http = http;
            _config = config;
        }

        /// <summary>
        /// The list of existing channels
        /// </summary>
        /// <param name="authToken"></param>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<Channel[]> GetChannelsAsync(GetChanngelQuery query)
        {
            var authToken = await GetTokenAsync();

            var path = $"/api/v2/notifications/channels?includechannels={query}";
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.{_config.Environment}" + path);
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

        public async Task<Channel> CreateChannelAsync()
        {
            var authToken = await GetTokenAsync();
            var path = "/api/v2/notifications/channels";
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.{_config.Environment}" + path);
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

        public async Task CreateSubscriptionsAsync(Channel channel, IEnumerable<string> topics)
        {
            var authToken = await GetTokenAsync();

            var path = $"/api/v2/notifications/channels/{channel.Id}/subscriptions";
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.{_config.Environment}" + path);
            var data = string.Join(",", topics.Select(t => "{" + $"\"id\":\"{t}\"" + "}").ToArray()).TrimEnd(',');
            var body = $"[{data}]";
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken.AccessToken);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            await SendApiRequestAsync(request, "create channel subscriptions");
        }

        public async Task UpdateSubscriptionsAsync(Channel channel, IEnumerable<string> topics)
        {
            var authToken = await GetTokenAsync();

            var path = $"/api/v2/notifications/channels/{channel.Id}/subscriptions";
            var request = new HttpRequestMessage(HttpMethod.Put, $"https://api.{_config.Environment}" + path);
            var data = string.Join(",", topics.Select(t => "{" + $"\"id\":\"{t}\"" + "}").ToArray()).TrimEnd(',');
            var body = $"[{data}]";
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken.AccessToken);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            await SendApiRequestAsync(request, "updated channel subscriptions");
        }

        public async Task<GenesysAuthTokenInfo> GetTokenAsync()
        {
            if (_config is GenesysConfig) return await GetTokenClientCredentialsAsync((GenesysConfig)_config);
            else if (_config is GenesysAuthTokenConfig) return (_config as GenesysAuthTokenConfig).Token;
            throw new NotImplementedException();
        }

        public async Task<GenesysAuthTokenInfo> GetTokenClientCredentialsAsync(GenesysConfig config)
        {
            var path = "/oauth/token";

            var basicAuth = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                .GetBytes($"{config.ClientId}:{config.ClientSecret}"));
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://login.{config.Environment}" + path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);
            var content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
            });
            request.Content = content;
            var response = await _http.SendAsync(request);

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
            var response = await _http.SendAsync(request);
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
            _http.Dispose();
        }
    }
}
