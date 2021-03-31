using System;
using System.Collections.Generic;

namespace ConsoleAppNet5
{
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
}
