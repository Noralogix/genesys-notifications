using System;
using System.Reactive.Subjects;

namespace Genesys.Client.Notifications.Responses
{
    public class SocketClosingResponse
    {
        public string ChannelId { get; set; }
        public DateTime Date { get; } = DateTime.UtcNow;

        private const string SocketClosing = "v2.system.socket_closing";
        internal static bool TryHandle(GenesysMessage message, ISubject<SocketClosingResponse> subject)
        {
            if (message.TopicName() == SocketClosing)
            {
                subject.OnNext(new SocketClosingResponse() { ChannelId = message.ChannelId });
                return true;
            }
            return false;
        }
    }
}
