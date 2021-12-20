using Genesys.Client.Notifications.Clients;
using Genesys.Client.Notifications.Responses;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Websocket.Client;
using Websocket.Client.Models;

namespace Genesys.Client.Notifications
{
    public class GenesysNotifications : IDisposable
    {
        private readonly ILogger _logger;

        private GenesysWebsocketClient _websocket;

        private readonly IDisposable _messageReceivedSubscription;
        private readonly IDisposable _disconnectionHappenedSubscription;
        private readonly IDisposable _reconnectionHappenedSubscription;

        private readonly IGenesysTopicSubscriptions _subscriptions;
        public GenesysNotifications(IGenesysTopicSubscriptions subscriptions, ILogger logger)
        {
            _subscriptions = subscriptions;
            _websocket = new GenesysWebsocketClient(_subscriptions);
            _websocket.Name = "GenesysCloudWS";
            _websocket.ReconnectTimeout = TimeSpan.FromSeconds(30);
            _websocket.ErrorReconnectTimeout = TimeSpan.FromSeconds(30);

            _disconnectionHappenedSubscription = _websocket.DisconnectionHappened.Subscribe(OnDisconnectionHappened);
            _reconnectionHappenedSubscription = _websocket.ReconnectionHappened.Subscribe(OnReconnectionHappened);
            _messageReceivedSubscription = _websocket.MessageReceived.Subscribe(HandleMessage);
            _logger = logger;
        }

        private void OnReconnectionHappened(ReconnectionInfo info)
        {
            _logger.LogInformation("Reconnection happened {Type}", info.Type);
        }
        private void OnDisconnectionHappened(DisconnectionInfo info)
        {
            _logger.LogError(info.Exception, "Disconnection happened {Type} {CloseStatus} {CloseStatusDescription}", info.Type, info.CloseStatus, info.CloseStatusDescription);
        }

        public async Task StartAsync()
        {
            await _websocket.Start();
            OnStart();
            _logger.LogDebug("Genesys notifications {ChannelId}", _subscriptions.ChannelId);

            var restartNotifications = Observable.Range(1, 1)
                .RepeatWhen(observable => observable.Delay(_subscriptions.Expires))
                .Select(_ => Observable.FromAsync(async () =>
                {
                    if (_subscriptions.IsExpired)
                    {
                        _logger.LogDebug("WebSocket expired {ChannelId}", _subscriptions.ChannelId);
                        var result = await _subscriptions.UpdateExpiresAsync();
                        if (result.IsNewChannel) _websocket.Url = new Uri(result.Channel.ConnectUri);
                        if (result.IsUpdated)
                        {
                            _logger.LogDebug("WebSocket restarted {ChannelId}", _subscriptions.ChannelId);
                            OnStart();
                        }    
                    }
                }))
                .Concat()
                .Subscribe(_ =>
                {
                });
            Streams.SocketClosing.Subscribe(async _ =>
            {
                _logger.LogDebug("Socket closing and start reconnect {ChannelId}", _subscriptions.ChannelId);
                await _websocket.Reconnect();                
            });
        }

        /// <summary>
        /// Provided message streams
        /// </summary>
        public GenesysClientStreams Streams { get; } = new GenesysClientStreams();

        public void Ping() => _websocket?.Ping();

        private void HandleMessage(ResponseMessage message)
        {
            try
            {
                bool handled;
                var messageSafe = (message.Text ?? string.Empty).Trim();

                if (messageSafe.StartsWith("{"))
                {
                    handled = HandleObjectMessage(messageSafe);
                    if (handled)
                        return;
                }

                handled = HandleRawMessage(messageSafe);
                if (handled)
                    return;

                if (!string.IsNullOrWhiteSpace(messageSafe))
                    Streams.UnhandledMessageSubject.OnNext(messageSafe);

                _logger.LogWarning("Unhandled response {ChannelId} {message}", _subscriptions.ChannelId, messageSafe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while receiving websocket message {ChannelId}", _subscriptions.ChannelId);
            }
        }

        private bool HandleRawMessage(string msg)
        {
            return true;
        }
        private bool HandleObjectMessage(string msg)
        {
            var gmessage = GenesysMessage.Parse(msg);
            return
                PongResponse.TryHandle(gmessage, Streams.PongSubject) ||
                HeartbeatResponse.TryHandle(gmessage, Streams.HeartbeatsSubject) ||
                SocketClosingResponse.TryHandle(gmessage, Streams.SocketClosingSubject) ||
                SubscriptionResponse.TryHandle(gmessage, Streams.SubscriptionsSubject, _subscriptions);
        }

        public void Dispose()
        {
            _messageReceivedSubscription.Dispose();
            _disconnectionHappenedSubscription.Dispose();
            _reconnectionHappenedSubscription.Dispose();
            _websocket.Dispose();
            Streams.Dispose();
        }

        private void OnStart() => Streams.StartSubject.OnNext(StartResponse.Create(_subscriptions));
    }
}
