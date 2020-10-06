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
            Console.WriteLine("Genesys Client Notifications");

            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(appConfigName, optional: false, reloadOnChange: true);
            var configurations = builder.Build();
            var genesysConfig = configurations.Get<GenesysConfig>();
            var pcIds = configurations.Get<PurecloudDataIds>();

            var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder
                    .AddConfiguration(configurations.GetSection("Logging"))
                    .AddConsole();
            });

            var logger = loggerFactory.CreateLogger("Genesys");

            var topics = pcIds.Queues.Select(id => new { name = $"v2.routing.queues.{id}.conversations.emails", type = typeof(QueueConversationChatEventTopicChatConversation) }).ToList();
            topics.AddRange(pcIds.Users.Select(id => new { name = $"v2.users.{id}.presence", type = typeof(PresenceEventUserPresence) }));

            var genesys = new GenesysNotifications(genesysConfig, logger);

            await genesys.StartAsync(topics.ToDictionary(t => t.name, t => t.type));

            var emails = genesys.Streams.Domain.OfType<QueueConversationChatEventTopicChatConversation>()
                .Subscribe(e =>
                {
                    Console.WriteLine("{0} {1}", e.Id, e.Participants.Count);
                });

            var presence = genesys.Streams.Domain.OfType<PresenceEventUserPresence>()
                .Subscribe(p =>
                {
                    Console.WriteLine("Presence {0}", p?.PresenceDefinition?.SystemPresence);
                });

            genesys.Streams.Pong.Subscribe(_ =>
            {
                Console.WriteLine("Pong");
            });
            genesys.Streams.Heartbeats.Subscribe(_ =>
            {
                Console.WriteLine("Heart beat");
                genesys.Ping();
            });
            genesys.Streams.SocketClosing.Subscribe(_ => Console.WriteLine("Socket closing"));

            genesys.Ping();
            genesys.Ping();

            Console.ReadLine();

            Console.WriteLine("End");
        }
    }
}
