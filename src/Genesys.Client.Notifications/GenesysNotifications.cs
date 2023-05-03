using Genesys.Client.Notifications.Clients;
using Genesys.Client.Notifications.Responses;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Genesys.Client.Notifications
{
    public class GenesysNotifications : IDisposable
    {
        private readonly ILogger _logger;

        private readonly GenesysWebsocketClient _websocket;
        private readonly IDisposable _socketClosingSubscription;
        private readonly IDisposable _heartbeatsSubscription;

        private bool? _requiresReconnect = null;
        private DateTime? _lastHeartbeats = null;

        private const int heartbeatDelayMinutes = 1;
        private static TimeSpan _heartbeatsBuffer = TimeSpan.FromMinutes(2);
        private static TimeSpan _heartbeatsPeriod = TimeSpan.FromMinutes(1);

        private readonly IGenesysTopicSubscriptions _subscriptions;
        public GenesysNotifications(IGenesysTopicSubscriptions subscriptions, ILogger logger, string name)
        {
            _ = subscriptions ?? throw new ArgumentNullException(nameof(subscriptions));
            _ = subscriptions.Channels ?? throw new ArgumentException(nameof(subscriptions.Channels));

            _logger = logger;
            _subscriptions = subscriptions;

            _websocket = new GenesysWebsocketClient(_subscriptions, HandleMessage);
            _websocket.Name = name;
            _websocket.ReconnectTimeout = TimeSpan.FromSeconds(30);
            _websocket.ErrorReconnectTimeout = TimeSpan.FromSeconds(30);

            var checkHeartbeats = Observable.Interval(_heartbeatsPeriod);

            _heartbeatsSubscription = checkHeartbeats.CombineLatest(Streams.Heartbeats, (_, h) => h)
                .Buffer(_heartbeatsBuffer)
                .Select(items => Observable.FromAsync(() => RequiresReconnect(items)))
                .Concat()
                .Subscribe(_ => { }, (error) => _logger.LogError(error, "Internal Heartbeats"));
            _socketClosingSubscription = Streams.SocketClosing.Subscribe(async args =>
            {
                _logger.LogDebug("Socket closing and start reconnect {ChannelId}", args.ChannelId);
                await _websocket.Reconnect(args.ChannelId);
            });
        }

        private async Task RequiresReconnect(IList<HeartbeatResponse> heartbeats)
        {
            var date = DateTime.UtcNow;
            var result = heartbeats.GroupBy(x => x.ChannelId)
                 .Select(g => new { ChannelId = g.Key, LatestDate = g.Max(x => x.Date) })
                 .ToList();
            if (result.Count > 0) _lastHeartbeats = result.Min(i => i.LatestDate);
            var requiresReconnect = result.Count < _subscriptions.Channels.Count
                || _lastHeartbeats.HasValue && _lastHeartbeats.Value.AddMinutes(heartbeatDelayMinutes) < date;
            _requiresReconnect = requiresReconnect;

            if (requiresReconnect && _subscriptions.Config.Reconnect)
            {
                _logger.LogDebug("Expired Heartbeats {Heartbeats}", _lastHeartbeats);
                await UpdateSubscriptionsAsync();
            }
        }

        public (bool reconnect, DateTime? dtHeartbeats) RequiresReconnect(DateTime dt)
        {
            if (_subscriptions.Expires <= dt) return (true, null);
            return (_requiresReconnect ?? false, _lastHeartbeats);
        }

        public async Task StartAsync()
        {
            OnStart();
            _logger.LogDebug("Genesys notifications starting {ChannelsCount}  {Expires}", _subscriptions.Channels.Count, _subscriptions.Expires);
            await _websocket.StartOrFail();
            _logger.LogDebug("Genesys notifications started");
        }

        public async Task UpdateSubscriptionsAsync()
        {
            try
            {
                await _subscriptions.UpdateExpiresAsync();
                await _websocket.ReconnectOrFail();
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

        private void HandleMessage(GenesysChannelResponseMessage gc)
        {
            var message = gc.Message;
            try
            {
                bool handled;
                var messageSafe = (message.Text ?? string.Empty).Trim();

                if (messageSafe.StartsWith("{"))
                {
                    handled = HandleObjectMessage(messageSafe, gc.ChannelId);
                    if (handled)
                        return;
                }

                handled = HandleRawMessage(messageSafe);
                if (handled)
                    return;

                if (!string.IsNullOrWhiteSpace(messageSafe))
                    Streams.UnhandledMessageSubject.OnNext(messageSafe);

                _logger.LogWarning("Unhandled response {message}", messageSafe);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while receiving websocket message");
            }
        }

        private bool HandleRawMessage(string msg)
        {
            return true;
        }
        private bool HandleObjectMessage(string msg, string channelId)
        {
            var gmessage = GenesysMessage.Parse(msg, channelId);
            return
                PongResponse.TryHandle(gmessage, Streams.PongSubject) ||
                HeartbeatResponse.TryHandle(gmessage, Streams.HeartbeatsSubject) ||
                SocketClosingResponse.TryHandle(gmessage, Streams.SocketClosingSubject) ||
                SystemResponse.TryHandle(gmessage, Streams.SystemSubject) ||
                SubscriptionResponse.TryHandle(gmessage, Streams.SubscriptionsSubject, _subscriptions);
        }

        public void Dispose()
        {
            try
            {
                _socketClosingSubscription.Dispose();
                _heartbeatsSubscription.Dispose();
                Streams.Dispose();
                _websocket.Dispose();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cant dispose");
            }
        }

        private void OnStart() => _subscriptions.Channels.ForEach(ch => Streams.StartSubject.OnNext(StartResponse.Create(ch)));
    }
}
