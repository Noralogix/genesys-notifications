using System;
using System.Reactive.Subjects;

namespace Genesys.Client.Notifications.Responses
{
    public class HeartbeatResponse
    {
        public DateTime Date { get; } = DateTime.UtcNow;
        public string ChannelId { get; set; }
        private const string ChannelMetadata = "channel.metadata";
        private const string Message = "websocket heartbeat";
        internal static bool TryHandle(GenesysMessage message, ISubject<HeartbeatResponse> subject)
        {
            if (message.TopicName() == ChannelMetadata && message.Message()?.ToLower() == Message)
            {
                subject.OnNext(new HeartbeatResponse() { ChannelId = message.ChannelId });
                return true;
            }
            return false;
        }
    }
}
