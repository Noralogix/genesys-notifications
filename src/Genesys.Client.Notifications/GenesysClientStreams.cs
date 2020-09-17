using Genesys.Client.Notifications.Responses;
using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Genesys.Client.Notifications
{
    public class GenesysClientStreams
    {
        internal readonly Subject<object> SubscriptionsSubject = new Subject<object>();
        internal readonly Subject<HeartbeatResponse> HeartbeatsSubject = new Subject<HeartbeatResponse>();
        internal readonly Subject<SocketClosingResponse> SocketClosingSubject = new Subject<SocketClosingResponse>();
        internal readonly Subject<PongResponse> PongSubject = new Subject<PongResponse>();
        internal readonly Subject<string> UnhandledMessageSubject = new Subject<string>();


        public IObservable<object> SubscriptionsStream => SubscriptionsSubject.AsObservable();
        public IObservable<HeartbeatResponse> HeartbeatsStream => HeartbeatsSubject.AsObservable();
        public IObservable<SocketClosingResponse> SocketClosingStream => SocketClosingSubject.AsObservable();
        public IObservable<PongResponse> PongStream => PongSubject.AsObservable();
        
        /// <summary>
        /// Stream of all raw unhandled messages (that are not yet implemented)
        /// </summary>
        public IObservable<string> UnhandledMessageStream => UnhandledMessageSubject.AsObservable();
    }
}
