using Genesys.Client.Notifications.Responses;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Genesys.Client.Notifications
{
    public class GenesysClientStreams : IDisposable
    {
        internal readonly Subject<object> SubscriptionsSubject = new Subject<object>();
        internal readonly Subject<HeartbeatResponse> HeartbeatsSubject = new Subject<HeartbeatResponse>();
        internal readonly Subject<SocketClosingResponse> SocketClosingSubject = new Subject<SocketClosingResponse>();
        internal readonly Subject<PongResponse> PongSubject = new Subject<PongResponse>();
        internal readonly Subject<string> UnhandledMessageSubject = new Subject<string>();
        internal readonly Subject<StartResponse> StartSubject = new Subject<StartResponse>();        

        public IObservable<object> Domain => SubscriptionsSubject.AsObservable();
        public IObservable<HeartbeatResponse> Heartbeats => HeartbeatsSubject.AsObservable();
        public IObservable<SocketClosingResponse> SocketClosing => SocketClosingSubject.AsObservable();
        public IObservable<PongResponse> Pong => PongSubject.AsObservable();
        public IObservable<StartResponse> Start => StartSubject.AsObservable();
        /// <summary>
        /// Stream of all raw unhandled messages (that are not yet implemented)
        /// </summary>
        public IObservable<string> UnhandledMessageStream => UnhandledMessageSubject.AsObservable();

        public void Dispose()
        {
            SubscriptionsSubject.Dispose();
            HeartbeatsSubject.Dispose();
            SocketClosingSubject.Dispose();
            PongSubject.Dispose();
            UnhandledMessageSubject.Dispose();
            StartSubject.Dispose();
        }
    }
}
