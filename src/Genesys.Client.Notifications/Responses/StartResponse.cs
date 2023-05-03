using System;

namespace Genesys.Client.Notifications.Responses
{
    public class StartResponse
    {
        public Channel Channel { get; set; }
        public string[] Topics { get; set; }

        internal static StartResponse Create(Channel channel)
            => new StartResponse
            {
                Channel = channel,
                Topics = new string[] { },
            };
    }
}
