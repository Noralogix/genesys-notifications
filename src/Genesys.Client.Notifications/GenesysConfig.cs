namespace Genesys.Client.Notifications
{
    public interface IGenesysConfig
    {
        string ClientId { get; set; }
        string ClientSecret { get; set; }
        string Environment { get; set; }
        bool Reconnect { get; set; }
    }

    public class GenesysConfig : IGenesysConfig
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string Environment { get; set; }
        public bool Reconnect { get; set; } = true;
    }
}
