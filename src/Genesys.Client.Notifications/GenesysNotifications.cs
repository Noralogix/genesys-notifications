using Genesys.Client.Notifications.Clients;
using Genesys.Client.Notifications.Responses;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Websocket.Client;

namespace Genesys.Client.Notifications
{
    public class GenesysNotifications : IDisposable
    {
        private readonly ILogger _logger;

        private readonly GenesysWebsocketClient _websocket;

        private readonly IDisposable _messageReceivedSubscription;
        private readonly IDisposable _socketClosingSubscription;
        private readonly IDisposable _heartbeatsSubscription;

        private IDisposable _requiresReconnectSubscription;

        private long _lastHeartbeatsHit = -1;

        private readonly IGenesysTopicSubscriptions _subscriptions;
        public GenesysNotifications(IGenesysTopicSubscriptions subscriptions, ILogger logger, string name)
        {
            _ = subscriptions ?? throw new ArgumentNullException(nameof(subscriptions));
            _ = subscriptions.Channel ?? throw new ArgumentNullException(nameof(subscriptions.Channel));
            _ = subscriptions.Channel.ConnectUri ?? throw new ArgumentNullException(nameof(subscriptions.Channel.ConnectUri));

            _logger = logger;


            _subscriptions = subscriptions;
            
            _websocket = new GenesysWebsocketClient(_subscriptions);
            _websocket.Name = name;
            _websocket.ReconnectTimeout = TimeSpan.FromSeconds(30);
            _websocket.ErrorReconnectTimeout = TimeSpan.FromSeconds(30);


            _messageReceivedSubscription = _websocket.MessageReceived.Subscribe(HandleMessage);
            _heartbeatsSubscription = Streams.Heartbeats.Subscribe(_ => Interlocked.Exchange(ref _lastHeartbeatsHit, DateTime.UtcNow.Ticks));
            _socketClosingSubscription = Streams.SocketClosing.Subscribe(async _ =>
            {
                _logger.LogDebug("Socket closing and start reconnect {ChannelId}", _subscriptions.Channel.Id);
                await _websocket.Reconnect();
            });
        }

        public async Task ReconnectAsync(string url)
        {
            _websocket.Url = new Uri(url);
            await _websocket.Reconnect();
        }

        public bool RequiresReconnect(DateTime dt)
        {
            if (_subscriptions.Channel.Expires <= dt) return true;
            var lastHeartbeatsHit = Interlocked.Read(ref _lastHeartbeatsHit);
            if (lastHeartbeatsHit == -1) return false;
            var dtHeartbeats = new DateTime(lastHeartbeatsHit);
            if (dtHeartbeats.AddMinutes(1) < dt)
            {
                Interlocked.Exchange(ref _lastHeartbeatsHit, -1);
                return true;
            }
            return false;
        }

        public async Task StartAsync()
        {
            await _websocket.Start();            
            OnStart();            
            _logger.LogDebug("Genesys notifications {ChannelId}", _subscriptions.Channel.Id);
            if (_subscriptions.Config.Reconnect)
            {
                _requiresReconnectSubscription = Observable.Range(1, 1)
                    .RepeatWhen(observable => observable.Delay(TimeSpan.FromMinutes(2)))
                    .Select(_ => Observable.FromAsync(async () =>
                    {
                        try
                        {
                            if (RequiresReconnect(DateTime.UtcNow))
                            {
                                _logger.LogDebug("Expired");
                                await UpdateSubscriptionsAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Expired");
                        }
                    }))
                    .Concat()
                    .Subscribe(_ =>
                    {
                    });
            }
        }

        public async Task UpdateSubscriptionsAsync()
        {
            try
            {
                var result = await _subscriptions.UpdateExpiresAsync();
                _websocket.Url = new Uri(result.ConnectUri);
                await _websocket.Reconnect();
                OnStart();
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Can't update subscriptions");
            }
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

                _logger.LogWarning("Unhandled response {ChannelId} {message}", _subscriptions.Channel.Id, messageSafe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while receiving websocket message {ChannelId}", _subscriptions.Channel.Id);
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
            _socketClosingSubscription.Dispose();
            _heartbeatsSubscription.Dispose();
            _requiresReconnectSubscription?.Dispose();
            _messageReceivedSubscription.Dispose();
            _websocket.Dispose();
            Streams.Dispose();
        }

        private void OnStart() => Streams.StartSubject.OnNext(StartResponse.Create(_subscriptions));
    }
}
