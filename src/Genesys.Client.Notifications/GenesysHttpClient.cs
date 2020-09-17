using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;

namespace Genesys.Client.Notifications
{
    public class GenesysHttpClient : IDisposable
    {
        private readonly HttpClient _httpClient;

        public GenesysHttpClient() : this(new HttpClient())
        {
        }

        public GenesysHttpClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<Channel> CreateChanngelAsync(GenesysAuthTokenInfo authToken)
        {
            return Task.FromResult(new Channel());
        }
                   
        public Task AddSubscriptionsAsync(GenesysAuthTokenInfo authToken, Channel channel, List<(string, Type)> subscriptions)
        {
            return Task.CompletedTask;
        }

        public async Task<GenesysAuthTokenInfo> GetTokenAsync(string environment, string clientId, string clientSecret)
        {
            var path = "/oauth/token";

            var basicAuth = Convert.ToBase64String(Encoding.GetEncoding("ISO-8859-1")
                .GetBytes($"{clientId}:{clientSecret}"));
            var request = new HttpRequestMessage(HttpMethod.Post, $"https://login.{environment}" + path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", basicAuth);
            var content = new FormUrlEncodedContent(new[]
            {
                    new KeyValuePair<string, string>("grant_type", "client_credentials"),
            });
            request.Content = content;
            var response = await _httpClient.SendAsync(request);

            try
            {
                response.EnsureSuccessStatusCode();
                var responseContent = await response.Content.ReadAsStreamAsync();
                var authTokenInfo = await JsonSerializer.DeserializeAsync<GenesysAuthTokenInfo>(responseContent);
                authTokenInfo.Environment = environment;
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
            _httpClient.Dispose();
        }
    }
}
