using System.Reactive.Subjects;

namespace Genesys.Client.Notifications.Responses
{
    public class PongResponse
    {
        private const string ChannelMetadata = "channel.metadata";
        internal static bool TryHandle(WebsocketMessage response, ISubject<PongResponse> subject)
        {
            if (response.TopicName() == ChannelMetadata && response.Message()?.ToLower() == "pong")
            {
                subject.OnNext(new PongResponse());
                return true;
            }
            return false;
        }
    }
}
