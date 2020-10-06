using Websocket.Client;

namespace Genesys.Client.Notifications.Clients
{
    public class GenesysWebsocketClient : WebsocketClient
    {
        public GenesysWebsocketClient(IGenesysTopicSubscriptions topicSubscriptions)
            : base(new System.Uri(topicSubscriptions.ChannelURI))
        {
        }

        public void Ping()
        {
            string message = "{\"message\":\"ping\"}";
            Send(message);
        }
    }
}
