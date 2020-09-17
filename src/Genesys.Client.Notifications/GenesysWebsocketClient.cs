using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Reactive.Linq;
using Websocket.Client;
using Genesys.Client.Notifications.Responses;

namespace Genesys.Client.Notifications
{
    public class GenesysWebsocketClient : IDisposable
    {        
        private readonly Subject<object> SubscriptionsSubject = new Subject<object>();
        private readonly Subject<HeartbeatResponse> HeartbeatsSubject = new Subject<HeartbeatResponse>();
        private readonly Subject<SocketClosingResponse> SocketClosingSubject = new Subject<SocketClosingResponse>();
        private readonly Subject<PongResponse> PongSubject = new Subject<PongResponse>();        

        public IObservable<object> SubscriptionsStream => SubscriptionsSubject.AsObservable();
        public IObservable<HeartbeatResponse> HeartbeatsStream => HeartbeatsSubject.AsObservable();
        public IObservable<SocketClosingResponse> SocketClosingStream => SocketClosingSubject.AsObservable();
        public IObservable<PongResponse> PongStream => PongSubject.AsObservable();


        private readonly WebsocketClient _websocket;
        private readonly Dictionary<string, Type> _subscriptions;

        private readonly IDisposable _messageReceivedSubscription;

        public GenesysWebsocketClient(Uri uri, Dictionary<string, Type> subscriptions)
        {
            _websocket = new WebsocketClient(uri);
            _subscriptions = subscriptions;
            _messageReceivedSubscription = _websocket.MessageReceived.Subscribe(HandleMessage);
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

                //if (!string.IsNullOrWhiteSpace(messageSafe))
                //    Streams.UnhandledMessageSubject.OnNext(messageSafe);
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
            var response = WebsocketMessage.Parse(msg);
            return

                PongResponse.TryHandle(response, PongSubject) ||
                HeartbeatResponse.TryHandle(response, HeartbeatsSubject) ||
                SocketClosingResponse.TryHandle(response, SocketClosingSubject) ||
                SubscriptionResponse.TryHandle(response, SubscriptionsSubject, _subscriptions);
        }
        public void Dispose()
        {
            _websocket.Dispose();
            _messageReceivedSubscription?.Dispose();
        }
    }
}
