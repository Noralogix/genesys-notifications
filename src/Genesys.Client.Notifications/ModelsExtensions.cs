using System;
using System.Collections.Generic;
using System.Text;

namespace Genesys.Client.Notifications
{
    public static class ModelsExtensions
    {
        public static bool IsExpired(this Channel channel)
         => channel != null && channel.Expires.HasValue && channel.Expires.Value <= DateTime.UtcNow;
    }
}
