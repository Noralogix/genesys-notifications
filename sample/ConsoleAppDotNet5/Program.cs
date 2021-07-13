using Genesys.Client.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace ConsoleAppNet5
{
    class Program
    {
        const string appConfigName = "appsettings.devhb.json";
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

            try
            {
                using (var topics = new GenesysTopics(genesysConfig, logger))
                {
                    using (var notifications = await topics
                        //.Add<QueueConversationEmailEventTopicEmailConversation>("v2.routing.queues.{id}.conversations.emails", pcIds.Queues)
                        .Add<QueueConversationEmailEventTopicEmailConversation>("v2.routing.queues.{id}.conversations", pcIds.Queues)
                        //.Add<EvaluationQualityV2TopicEvaluationV2>("v2.quality.evaluations")
                        //.Add<NotificationData<PresenceEventUserPresence>>("v2.users.{id}.presence", pcIds.Users)
                        .CreateAsync())
                    {
                        notifications.Streams.Domain
                            .OfType<QueueConversationEmailEventTopicEmailConversation>()
                            .Subscribe(e =>
                            {
                                Console.WriteLine("[{0}] {1}", DateTime.Now, e.Id);
                                foreach (var item in e.Participants)
                                {
                                    Console.WriteLine(item);
                                }
                            });

                        notifications.Streams.Domain
                            .OfType<NotificationData<PresenceEventUserPresence>>()
                            .Where(p => p.TopicName.Contains("b16b0516") && p.EventBody.PresenceDefinition?.SystemPresence == "AVAILABLE")
                            .Subscribe(p => Console.WriteLine("[{0}] Taras-{1} AVAILABLE with filtering.", DateTime.Now, p.UserId()));

                        notifications.Streams.Domain
                            .OfType<NotificationData<PresenceEventUserPresence>>()
                            .Select(p => new PurecloudUserPresence(p.UserId(), p.EventBody))
                            .Subscribe(up => Console.WriteLine("[{0}] User-{1} {2}", DateTime.Now, up.Id, up.Presence));

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
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }

    public class PurecloudDataIds
    {
        public string[] Queues { get; set; }
        public string[] Users { get; set; }
    }

    public static class NotificationDataExt
    {
        public static string UserId(this INotificationData notificationData) => notificationData.TopicName.Split(".")[2].Substring(0, 5);
    }

    public class PurecloudUserPresence
    {
        public string Id { get; }
        public string Presence { get; }
        public PurecloudUserPresence(string userId, PresenceEventUserPresence userPresence) => (Id, Presence) = (userId, userPresence.PresenceDefinition.SystemPresence);
    }
}
