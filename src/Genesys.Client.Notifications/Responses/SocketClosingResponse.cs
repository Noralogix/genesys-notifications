using System;
using System.Reactive.Subjects;

namespace Genesys.Client.Notifications.Responses
{
    public class SocketClosingResponse : IChannelResponse
    {
        private const string SocketClosing = "v2.system.socket_closing";
        public SocketClosingResponse(string channelId)
        {
            ChannelId = channelId;
            Date = DateTime.UtcNow;
        }
        public string ChannelId { get; }
        public DateTime Date { get; }
        internal static bool TryHandle(GenesysMessage message, ISubject<SocketClosingResponse> subject)
        {
            if (message.TopicName() == SocketClosing)
            {
                subject.OnNext(new SocketClosingResponse(message.ChannelId));
                return true;
            }
            return false;
        }
    }
}
