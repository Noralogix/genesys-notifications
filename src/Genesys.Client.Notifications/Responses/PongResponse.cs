using System;
using System.Reactive.Subjects;

namespace Genesys.Client.Notifications.Responses
{
    public class PongResponse : IChannelResponse
    {
        private const string ChannelMetadata = "channel.metadata";
        public PongResponse(string channelId)
        {
            ChannelId = channelId;
            Date = DateTime.UtcNow;
        }
        public DateTime Date { get; }
        public string ChannelId { get; }

        internal static bool TryHandle(GenesysMessage message, ISubject<PongResponse> subject)
        {
            if (message.TopicName() == ChannelMetadata && message.Message()?.ToLower() == "pong")
            {
                subject.OnNext(new PongResponse(message.ChannelId));
                return true;
            }
            return false;
        }
    }
}
