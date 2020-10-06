using Websocket.Client;

namespace Genesys.Client.Notifications.Clients
{
    public class GenesysWebsocketClient : WebsocketClient
    {        
        public GenesysWebsocketClient(GenesysTopicSubscriptions topicSubscriptions)
            : base(topicSubscriptions.Uri)
        {
        }

        public void Ping()
        {
            string message = "{\"message\":\"ping\"}";
            Send(message);
        }
    }
}
