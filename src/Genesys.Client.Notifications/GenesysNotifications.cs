using Genesys.Client.Notifications.Clients;
using Genesys.Client.Notifications.Responses;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Websocket.Client;

namespace Genesys.Client.Notifications
{
    public class GenesysNotifications : IDisposable
    {
        private readonly GenesysHttpClient _http;
        private readonly ILogger _logger;

        private GenesysTopicSubscriptions _subscriptions;
        private GenesysWebsocketClient _websocket;
        private GenesysAuthTokenInfo _token;

        private IDisposable _messageReceivedSubscription;

        public GenesysNotifications(GenesysConfig config, ILogger logger)
        {
            _http = new GenesysHttpClient(config);
            _logger = logger;
        }

        public async Task StartAsync(Dictionary<string, Type> topicSubscriptions, int expiresHours = 24)
        {
            _token = await _http.GetTokenAsync();
            var channel = await _http.CreateChannelAsync(_token);            
            _subscriptions = new GenesysTopicSubscriptions(channel, topicSubscriptions, expiresHours);
            _websocket = new GenesysWebsocketClient(_subscriptions);
            
            _messageReceivedSubscription = _websocket.MessageReceived.Subscribe(HandleMessage);

            await _websocket.Start();            
            await _http.CreateSubscriptionsAsync(_token, channel, _subscriptions.Topics);

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
                            _token = await _http.GetTokenAsync();
                            await _http.UpdateSubscriptionsAsync(_token, _subscriptions.Channel, _subscriptions.Topics);
                            _subscriptions.UpdateExpires();
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
            _http.Dispose();
            _messageReceivedSubscription.Dispose();
            _websocket.Dispose();
            Streams.Dispose();
        }

        private async Task<Channel> GetOrCreateChannelAsync(GenesysAuthTokenInfo authToken)
            => await GetLastChanngelAsync(authToken, GetChanngelQuery.OAuthClient)
                ?? await _http.CreateChannelAsync(authToken);

        private async Task<Channel> GetLastChanngelAsync(GenesysAuthTokenInfo authToken, GetChanngelQuery query)
            => (await _http.GetChannelsAsync(authToken, query))
            .Where(ch => ch.Expires.HasValue).OrderByDescending(ch => ch.Expires).FirstOrDefault();

        private void OnStart() => Streams.StartSubject.OnNext(StartResponse.Create(_subscriptions));
    }
}
