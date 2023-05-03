using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genesys.Client.Notifications.Clients
{
    public class GenesysWebsocketClient : IDisposable
    {
        private const string messagePing = "{\"message\":\"ping\"}";
        private readonly List<GenesysWebsocketChannelClient> _websockets;
        private readonly IGenesysChannels _channels;
        private readonly Action<GenesysChannelResponseMessage> _handleMessage;        
        private string _name;
        private TimeSpan? _reconnectTimeout;
        private TimeSpan? _errorReconnectTimeout;
        public GenesysWebsocketClient(IGenesysChannels channels, Action<GenesysChannelResponseMessage> handleMessage)
        {
            _channels = channels;
            _handleMessage = handleMessage;
            _websockets = new List<GenesysWebsocketChannelClient>();
            foreach (var item in channels.Channels)
            {
                var ws = new GenesysWebsocketChannelClient(item, _handleMessage);
                _websockets.Add(ws);
            }
        }
        public void Ping() => _websockets.ForEach(i => i.Send(messagePing));
        public string Name
        {
            set
            {
                _name = value;
                _websockets.ForEach(i => i.Name = $"{value}-{_websockets.IndexOf(i) + 1}");
            }
        }

        public TimeSpan? ReconnectTimeout
        {
            set
            {
                _reconnectTimeout = value;
                _websockets.ForEach(i => i.ReconnectTimeout = value);
            }
        }
        public TimeSpan? ErrorReconnectTimeout
        {
            set
            {
                _errorReconnectTimeout = value;
                _websockets.ForEach(i => i.ErrorReconnectTimeout = value);
            }
        }

        public Task Reconnect(string id) => _websockets.FirstOrDefault(i => i.Id == id)?.Reconnect();

        public Task StartOrFail() => Task.WhenAll(_websockets.Select(ws => ws.StartOrFail()));
        public async Task ReconnectOrFail()
        {
            var wsList = _websockets.ToArray();
            for (int i = 0; i < _channels.Channels.Count; i++)
            {
                var channel = _channels.Channels[i];
                if (i < wsList.Length) await wsList[i].ReconnectOrFail(channel);
                else
                {
                    var ws = new GenesysWebsocketChannelClient(channel, _handleMessage);
                    ws.Name = $"{_name}-{i + 1}";
                    ws.ErrorReconnectTimeout = _errorReconnectTimeout;
                    ws.ReconnectTimeout = _reconnectTimeout;
                    await ws.StartOrFail();
                    _websockets.Add(ws);
                }
            }
        }
        public void Dispose()
        {
            _websockets.ForEach(i => i.Close());
            _websockets.Clear();
        }
    }
}
