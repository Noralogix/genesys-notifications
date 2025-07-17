using System;
using System.Reactive.Subjects;

namespace Genesys.Client.Notifications.Responses
{
    public class HeartbeatResponse : IChannelResponse
    {
        private const string ChannelMetadata = "channel.metadata";
        private const string Message = "websocket heartbeat";
        public HeartbeatResponse(string channelId)
        {
            ChannelId = channelId;
            Date = DateTime.UtcNow;
        }
        public DateTime Date { get; }
        public string ChannelId { get; }

        internal static bool TryHandle(GenesysMessage message, ISubject<HeartbeatResponse> subject)
        {
            if (message.TopicName() == ChannelMetadata && message.Message()?.ToLower() == Message)
            {
                subject.OnNext(new HeartbeatResponse(message.ChannelId));
                return true;
            }
            return false;
        }
    }
}
