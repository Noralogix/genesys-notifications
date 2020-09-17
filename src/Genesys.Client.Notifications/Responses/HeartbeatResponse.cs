using System.Reactive.Subjects;

namespace Genesys.Client.Notifications.Responses
{
    public class HeartbeatResponse
    {
        private const string ChannelMetadata = "channel.metadata";
        private const string Message = "websocket heartbeat";
        internal static bool TryHandle(WebsocketMessage response, ISubject<HeartbeatResponse> subject)
        {
            if (response.TopicName() == ChannelMetadata && response.Message()?.ToLower() == Message)
            {
                subject.OnNext(new HeartbeatResponse());
                return true;
            }
            return false;
        }
    }
}
