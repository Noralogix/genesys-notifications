namespace Genesys.Client.Notifications
{
    public class GenesysConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Environment { get; set; }
        public int ChannelExpiresHours { get; set; } = 24;
    }
}
