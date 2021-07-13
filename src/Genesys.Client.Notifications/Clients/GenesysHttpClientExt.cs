using System.Linq;
using System.Threading.Tasks;

namespace Genesys.Client.Notifications.Clients
{
    public static class GenesysHttpClientExt
    {
        public static async Task<Channel> GetOrCreateChannelAsync(this GenesysHttpClient _http)
            => await GetLastChanngelAsync(_http, GetChanngelQuery.OAuthClient)
                ?? await _http.CreateChannelAsync();

        public static async Task<Channel> GetLastChanngelAsync(this GenesysHttpClient _http, GetChanngelQuery query)
            => (await _http.GetChannelsAsync(query))
            .Where(ch => ch.Expires.HasValue).OrderByDescending(ch => ch.Expires).FirstOrDefault();
    }
}
