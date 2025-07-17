using System;

namespace Genesys.Client.Notifications.Responses
{
    public interface IChannelResponse
    {
        string ChannelId { get; }
        DateTime Date { get; }

    }
}
