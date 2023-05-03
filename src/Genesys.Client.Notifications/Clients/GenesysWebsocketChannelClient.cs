using System;
using System.Threading.Tasks;
using Websocket.Client;

namespace Genesys.Client.Notifications.Clients
{
    public class GenesysWebsocketChannelClient : WebsocketClient
    {
        private Channel _channel;
        public string Id => _channel.Id;
        private readonly IDisposable _messageReceivedSubscription;
        public GenesysWebsocketChannelClient(Channel channel, Action<GenesysChannelResponseMessage> handleMessage)
            : base(new System.Uri(channel.ConnectUri))
        {
            _channel = channel;
            _messageReceivedSubscription = MessageReceived.Subscribe(m => handleMessage(new GenesysChannelResponseMessage(m, Id)));
        }

        public Task ReconnectOrFail(Channel channel)
        {
            _channel = channel;
            Url = new Uri(channel.ConnectUri);
            return ReconnectOrFail();
        }

        public Task Reconnect(Channel channel)
        {
            _channel = channel;
            Url = new Uri(channel.ConnectUri);
            return Reconnect();
        }

        public void Close()
        {
            _messageReceivedSubscription.Dispose();
            Dispose();
        }
    }

    public class GenesysChannelResponseMessage
    {
        public ResponseMessage Message { get; }
        public string ChannelId { get; }
        public GenesysChannelResponseMessage(ResponseMessage message, string channelId)
        {
            Message = message;
            ChannelId = channelId;
        }
    }
}
