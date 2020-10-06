using Genesys.Client.Notifications.Clients;
using Genesys.Client.Notifications.Responses;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Websocket.Client;

namespace Genesys.Client.Notifications
{
    public class GenesysNotifications : IDisposable
    {
        private readonly ILogger _logger;

        private GenesysWebsocketClient _websocket;

        private IDisposable _messageReceivedSubscription;
        private readonly IGenesysTopicSubscriptions _subscriptions;
        public GenesysNotifications(IGenesysTopicSubscriptions subscriptions, ILogger logger)
        {
            _subscriptions = subscriptions;
            _websocket = new GenesysWebsocketClient(_subscriptions);
            _messageReceivedSubscription = _websocket.MessageReceived.Subscribe(HandleMessage);
            _logger = logger;
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
                    try
                    {
                        if (_subscriptions.IsExpired)
                        {
                            await _subscriptions.UpdateExpiresAsync();
                            _logger.LogDebug("WebSocket restarted {ChannelId}", _subscriptions.ChannelId);
                            OnStart();
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(LogLevel.Error, ex, "Can't restart websocket {ChannelId}", _subscriptions.ChannelId);
                    }
                }))
                .Concat()
                .Subscribe(_ =>
                {
                });
            Streams.SocketClosing.Subscribe(async _ =>
            {                
                await _websocket.Reconnect();
                _logger.LogDebug("WebSocket closed and reconnected {ChannelId}", _subscriptions.ChannelId);
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
                _logger.Log(LogLevel.Error, ex, "Exception while receiving websocket message {ChannelId}", _subscriptions.ChannelId);
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
            _websocket.Dispose();
            Streams.Dispose();
        }

        private void OnStart() => Streams.StartSubject.OnNext(StartResponse.Create(_subscriptions));
    }
}
