using ConsoleAppDotNet6;
using Genesys.Client.Notifications.Clients;
using Genesys.Client.Notifications;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Reactive.Linq;

var appConfigName = "appsettings.dev.json";
var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile(appConfigName, optional: false, reloadOnChange: true);
var configurations = builder.Build();
var genesysConfig = configurations.Get<GenesysConfig>();
var appConfig = configurations.Get<PurecloudAppConfig>();
var loggerFactory = LoggerFactory.Create(builder => builder.AddConfiguration(configurations.GetSection("Logging")).AddConsole());

var logger = loggerFactory.CreateLogger("Genesys");
var client = new GenesysHttpClient(genesysConfig);
var ds = new DataService();
try
{
    //var users = await AzStorage.LoadDataAsync(appConfig, AzStorage.PurecloudUsers);
    //var usersIds = users.Select(u => u.Id).ToArray();
    var usersIds = appConfig.Users;
    //var queues = await AzStorage.LoadDataAsync(appConfig, AzStorage.PurecloudQueues);
    //var queuesIds = queues.Select(u => u.Id).ToArray();


    //logger.LogInformation("User count {UCount}. Queues count {QCount}", users.Count, queues.Count);
    //logger.LogInformation("Queues count {QCount}", queues.Count);
    logger.LogInformation("Users count {Users}", usersIds.Count());
    using (var topics = new GenesysTopics(client, logger))
    {
        using (var notifications = await topics
            //.Add<QueueConversationEmailEventTopicEmailConversation>("v2.routing.queues.{id}.conversations.emails", queuesIds)
            //.Add<QueueConversationCallEventTopicCallConversation>("v2.routing.queues.{id}.conversations.calls", queuesIds)
            //.Add<EvaluationQualityV2TopicEvaluationV2>("v2.quality.evaluations")
            .Add<NotificationData<AnalyticsUsersAggregates>>("v2.analytics.users.{id}.aggregates", usersIds)
            //.Add<NotificationData<AnalyticsUsersAggregates>>("v2.analytics.queues.{id}.observations", queuesIds)
            //.Add<NotificationData<PresenceEventUserPresence>>("v2.users.{id}.presence", usersIds)
            .CreateAsync())
        {
            notifications.Streams.Domain
                .OfType<QueueConversationCallEventTopicCallConversation>()
                .Subscribe(e =>
                {
                    ds.Add(e);
                    Console.WriteLine("[{0}] {1}", DateTime.Now, e.Id);
                    foreach (var item in e.Participants)
                    {
                        Console.WriteLine(item);
                    }
                });

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
                .OfType<NotificationData<AnalyticsUsersAggregates>>()
                .Subscribe(e =>
                {
                    Console.WriteLine("[{0}] {1}", DateTime.Now, e.TopicName);
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
            notifications.Streams.System
                .Subscribe(s => Console.WriteLine("System {0}", s.SystemTopicType, s.Reason, s.ChannelId, s.UserId));
            notifications.Ping();

            //var t = Task.Run(async () =>
            //{
            //    var channel = await client.CreateChannelAsync();
            //    for (int i = 0; i < 1000; i++)
            //    {
            //        channel = await client.CreateChannelAsync();
            //        await Task.Delay(TimeSpan.FromSeconds(5));
            //    }
            //});

            //var t2 = Task.Run(async () =>
            //{
            //    var client = new GenesysHttpClient(genesysConfig);                            
            //    for (int i = 0; i < 1000; i++)
            //    {
            //        var channel = await client.GetTokenClientCredentialsAsync();
            //        await Task.Delay(TimeSpan.FromSeconds(5));
            //    }
            //});

            //var t3 = Task.Run(async () =>
            //{
            //    for (int i = 0; i < 1000; i++)
            //    {
            //        await Task.Delay(TimeSpan.FromSeconds(5));
            //        notifications.Ping();
            //    }
            //});

            Console.ReadLine();
        }
    }
}
catch (Exception ex)
{
    Console.WriteLine(ex.Message);
}
Console.ReadLine();