using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using Websocket.Client;
using Genesys.Client.Notifications.Responses;

namespace Genesys.Client.Notifications
{
    public class GenesysWebsocketClient : IDisposable
    {
        private readonly WebsocketClient _websocket;
        private readonly Dictionary<string, Type> _subscriptions;

        private readonly IDisposable _messageReceivedSubscription;

        public GenesysWebsocketClient(Uri uri, Dictionary<string, Type> subscriptions)
        {
            _websocket = new WebsocketClient(uri);
            _subscriptions = subscriptions;
            _messageReceivedSubscription = _websocket.MessageReceived.Subscribe(HandleMessage);
        }

        /// <summary>
        /// Provided message streams
        /// </summary>
        public GenesysClientStreams Streams { get; } = new GenesysClientStreams();

        public void Ping()
        {
            string message = "{'message':'ping'}";
            _websocket.Send(message);
        }

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

                //Log.Warn(L($"Unhandled response:  '{messageSafe}'"));
            }
            catch (Exception ex)
            {
                //Log.Error(e, L("Exception while receiving message"));
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
            _websocket.Dispose();
            _messageReceivedSubscription?.Dispose();
        }
    }
}
