using System.Reactive.Subjects;

namespace Genesys.Client.Notifications.Responses
{
    public class PongResponse
    {
        private const string ChannelMetadata = "channel.metadata";
        internal static bool TryHandle(GenesysMessage message, ISubject<PongResponse> subject)
        {
            if (message.TopicName() == ChannelMetadata && message.Message()?.ToLower() == "pong")
            {
                subject.OnNext(new PongResponse());
                return true;
            }
            return false;
        }
    }
}
