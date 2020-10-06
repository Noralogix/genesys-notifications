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
        private readonly GenesysConfig _config;

        public GenesysHttpClient(GenesysConfig config) : this(config, new HttpClient())
        {
        }

        public GenesysHttpClient(GenesysConfig config, HttpClient http)
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
        public async Task<Channel[]> GetChannelsAsync(GenesysAuthTokenInfo authToken, GetChanngelQuery query)
        {
            var path = $"/api/v2/notifications/channels?includechannels={query}";
            var request = new HttpRequestMessage(HttpMethod.Get, $"https://api.{_config.Environment}" + path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken.AccessToken);
            var response = await _http.SendAsync(request);
            try
            {
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStreamAsync();
                var channels = await JsonSerializer.DeserializeAsync<ChannelsList>(responseContent);
                return channels?.Items ?? new Channel[] { };
            }
            catch (HttpRequestException ex)
            {
                int statusCode = (int)response.StatusCode;
                throw new Exception($"Genesys API Error GetChanngelAsync. Status Code {statusCode}.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"GetChanngelAsync Error", ex);
            }
        }

        public async Task<Channel> CreateChannelAsync(GenesysAuthTokenInfo authToken)
        {
            var path = "/api/v2/notifications/channels";
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.{_config.Environment}" + path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken.AccessToken);
            var response = await _http.SendAsync(request);
            try
            {
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStreamAsync();
                var channel = await JsonSerializer.DeserializeAsync<Channel>(responseContent);
                return channel;
            }
            catch (HttpRequestException ex)
            {
                int statusCode = (int)response.StatusCode;
                throw new Exception($"Genesys API Error CreateChanngel. Status Code {statusCode}.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"CreateChanngel Error", ex);
            }
        }

        public async Task CreateSubscriptionsAsync(GenesysAuthTokenInfo authToken, Channel channel, IEnumerable<string> topics)
        {
            var path = $"/api/v2/notifications/channels/{channel.Id}/subscriptions";
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://api.{_config.Environment}" + path);
            var data = string.Join(",", topics.Select(t => "{" + $"\"id\":\"{t}\"" + "}").ToArray()).TrimEnd(',');
            var body = $"[{data}]";
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken.AccessToken);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task UpdateSubscriptionsAsync(GenesysAuthTokenInfo authToken, Channel channel, IEnumerable<string> topics)
        {
            var path = $"/api/v2/notifications/channels/{channel.Id}/subscriptions";
            var request = new HttpRequestMessage(HttpMethod.Put, $"https://api.{_config.Environment}" + path);
            var data = string.Join(",", topics.Select(t => "{" + $"\"id\":\"{t}\"" + "}").ToArray()).TrimEnd(',');
            var body = $"[{data}]";
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken.AccessToken);
            request.Content = new StringContent(body, Encoding.UTF8, "application/json");
            var response = await _http.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        public async Task<GenesysAuthTokenInfo> GetTokenAsync()
        {
            var path = "/oauth/token";

            var basicAuth = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                .GetBytes($"{_config.ClientId}:{_config.ClientSecret}"));
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://login.{_config.Environment}" + path);
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

        public void Dispose()
        {
            _http.Dispose();
        }
    }
}
