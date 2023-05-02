You can query asynchronous Genesys Cloud event streams using LINQ operators. Thus you can filter, project, aggregate, compose and perform time-based operations on multiple events easily by using these standard LINQ operators.

C# .NET Standard 2.0 lightweight implementation of the client Genesys Notifications websocket API with default 24 hours reconnection and handling socket closing notification.
- Serialization based on System.Text.Json. 
- Logging functionality based on Microsoft.Extensions.Logging.  
- User friendly with reactive model programming.
```{"language":"csharp"}
using Genesys.Client.Notifications;
```
```{"language":"csharp"}
static async Task Main(string[] args)
{            
    using (var topics = new GenesysTopics(genesysConfig, logger))
    {
        using (var notifications = await topics
            .Add<QueueConversationEmailEventTopicEmailConversation>("v2.routing.queues.{id}.conversations.emails", pcIds.Queues)
            .Add<PresenceEventUserPresence>("v2.users.{id}.presence", pcIds.Users)
            .CreateAsync())
        {
            notifications.Streams.Domain
                .OfType<QueueConversationEmailEventTopicEmailConversation>()
                .Subscribe(e => Console.WriteLine("{0} {1}", e.Id, e.Participants.Count));
            notifications.Streams.Domain
                .OfType<PresenceEventUserPresence>()
                .Subscribe(p => Console.WriteLine("Presence {0}", p?.PresenceDefinition?.SystemPresence));
            notifications.Streams.Domain
                .Subscribe(obj => Console.WriteLine(obj.ToString()));
            notifications.Streams.Pong
                .Subscribe(_ => Console.WriteLine("Pong"));
            notifications.Streams.Heartbeats
                .Subscribe(_ => Console.WriteLine("Heart beat"));
            notifications.Streams.SocketClosing
                .Subscribe(_ => Console.WriteLine("Socket closing"));
            notifications.Ping();
            Console.ReadLine();
        }
    }
}
```