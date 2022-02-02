using System.Linq;
using System.Threading.Tasks;

namespace Genesys.Client.Notifications.Clients
{
    public static class GenesysHttpClientExt
    {
        public static bool IsNotificationChannelIdError(this ErrorMessage errorMessage)
            => errorMessage.Status == 400 && errorMessage.Code.Equals("notification.unable.to.get.channel.id", System.StringComparison.OrdinalIgnoreCase);
        public static async Task<Channel> GetOrCreateChannelAsync(this GenesysHttpClient _http)
            => await GetLastChanngelAsync(_http, GetChanngelQuery.OAuthClient)
                ?? await _http.CreateChannelAsync();

        public static async Task<Channel> GetLastChanngelAsync(this GenesysHttpClient _http, GetChanngelQuery query)
            => (await _http.GetChannelsAsync(query))
            .OrderByDescending(ch => ch.Expires).FirstOrDefault();
    }
}
