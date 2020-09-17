using System;
using System.Collections.Generic;

namespace Genesys.Client.Notifications
{
    public class GenesysTopicSubscriptions
    {
        private const int topicsLimit = 1000;
        public Uri Uri { get; private set; }        
        internal Dictionary<string, Type> Items { get; } = new Dictionary<string, Type>();
    }
}
