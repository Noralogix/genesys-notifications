namespace Genesys.Client.Notifications
{
    public interface IGenesysConfig
    {
        string Environment { get; set; }
        int ChannelExpiresHours { get; set; }
    }

    public class GenesysAuthTokenConfig : IGenesysConfig
    {
        public GenesysAuthTokenInfo Token { get; set; }
        public string Environment { get; set; }
        public int ChannelExpiresHours { get; set; } = 24;
    }

    public class GenesysConfig : IGenesysConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Environment { get; set; }
        public int ChannelExpiresHours { get; set; } = 24;
    }
}
