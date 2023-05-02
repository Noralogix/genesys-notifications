using Genesys.Client.Notifications;

namespace ConsoleAppDotNet6
{
    public record EvaluationQualityV2TopicEvaluationV2();
    public record PresenceEventOrganizationPresence(string Id, string SystemPresence);
    public record PresenceEventUserPresence(PresenceEventOrganizationPresence PresenceDefinition, bool? Primary, string Message, DateTime? ModifiedDate);

    public record QueueConversationEmailEventTopicEmailMediaParticipant(string Id
        , Dictionary<string, string> Attributes
        , string Purpose
        , DateTime? EndTime
        , DateTime? StartTime
        , string Name
        , StateEnum? State
        , DirectionEnum? Direction);

    public record QueueConversationEmailEventTopicEmailConversation(string Id
        , string Name
        , QueueConversationEmailEventTopicEmailMediaParticipant[] Participants);

    public enum DirectionEnum
    {
        OutdatedSdkVersion,
        Inbound,
        Outbound
    }
    public enum StateEnum
    {
        OutdatedSdkVersion,
        Alerting,
        Dialing,
        Contacting,
        Offering,
        Connected,
        Disconnected,
        Terminated,
        Converting,
        Uploading,
        Transmitting,
        Scheduled,
        None
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
