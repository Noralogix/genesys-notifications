using Genesys.Client.Notifications;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Reactive.Linq;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace ConsoleApp
{
    public class PurecloudDataIds
    {
        public string[] Queues { get; set; }
        public string[] Users { get; set; }
    }

    class Program
    {
        const string appConfigName = "appsettings.dev.json";

        static async Task Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(appConfigName, optional: false, reloadOnChange: true);
            var configurations = builder.Build();
            var genesysConfig = configurations.Get<GenesysConfig>();
            var pcIds = configurations.Get<PurecloudDataIds>();
            var loggerFactory = LoggerFactory.Create(builder => builder.AddConfiguration(configurations.GetSection("Logging")).AddConsole());

            var logger = loggerFactory.CreateLogger("Genesys");

            using (var topics = new GenesysTopics(genesysConfig, logger))
            {
                using (var notifications = await topics
                    .Add<QueueConversationChatEventTopicChatConversation>("v2.routing.queues.{id}.conversations.emails", pcIds.Queues)
                    .Add<PresenceEventUserPresence>("v2.users.{id}.presence", pcIds.Users)
                    .CreateAsync())
                {
                    notifications.Streams.Domain
                        .OfType<QueueConversationChatEventTopicChatConversation>()
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
    }
}
