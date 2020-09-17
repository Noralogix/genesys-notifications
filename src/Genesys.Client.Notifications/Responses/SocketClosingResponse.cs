using System.Reactive.Subjects;

namespace Genesys.Client.Notifications.Responses
{
    public class SocketClosingResponse
    {
        private const string SocketClosing = "v2.system.socket_closing";
        internal static bool TryHandle(GenesysMessage message, ISubject<SocketClosingResponse> subject)
        {
            if (message.TopicName() == SocketClosing)
            {
                subject.OnNext(new SocketClosingResponse());
                return true;
            }
            return false;
        }
    }
}
