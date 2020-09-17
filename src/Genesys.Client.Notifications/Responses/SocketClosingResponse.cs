using System.Reactive.Subjects;

namespace Genesys.Client.Notifications.Responses
{
    public class SocketClosingResponse
    {
        private const string SocketClosing = "v2.system.socket_closing";
        internal static bool TryHandle(WebsocketMessage response, ISubject<SocketClosingResponse> subject)
        {
            if (response.TopicName() == SocketClosing)
            {
                subject.OnNext(new SocketClosingResponse());
                return true;
            }
            return false;
        }
    }
}
