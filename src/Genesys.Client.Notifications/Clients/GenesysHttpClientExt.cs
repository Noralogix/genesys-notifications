using System.Linq;
using System.Threading.Tasks;

namespace Genesys.Client.Notifications.Clients
{
    public static class GenesysHttpClientExt
    {
        public static async Task<Channel> GetOrCreateChannelAsync(this GenesysHttpClient _http, GenesysAuthTokenInfo authToken)
            => await GetLastChanngelAsync(_http, authToken, GetChanngelQuery.OAuthClient)
                ?? await _http.CreateChannelAsync(authToken);

        public static async Task<Channel> GetLastChanngelAsync(this GenesysHttpClient _http, GenesysAuthTokenInfo authToken, GetChanngelQuery query)
            => (await _http.GetChannelsAsync(authToken, query))
            .Where(ch => ch.Expires.HasValue).OrderByDescending(ch => ch.Expires).FirstOrDefault();
    }
}
