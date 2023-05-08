using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleAppDotNet6
{   
    public record QueueConversationCallEventTopicCallConversation(string Id
        , string Name
        , QueueConversationCallEventTopicCallMediaParticipant[] Participants);

    public record QueueConversationCallEventTopicCallMediaParticipant(
        string Purpose,
        StateEnum? State,
        StateEnum? InitialState,
        DisconnectTypeEnum? DisconnectType,
        DateTime? StartTime,
        DateTime? ConnectedTime,
        DateTime? EndTime,
        DateTime? StartHoldTime,
        DateTime? StartAcwTime,
        DateTime? EndAcwTime,
        string Id,
        string Name,
        string Address,
        DirectionEnum Direction,        
        bool? Held,
        bool? WrapupRequired,
        string WrapupPrompt,
        Dictionary<string, string> Attributes,
        int? WrapupTimeoutMs,
        bool? WrapupSkipped,
        int? AlertingTimeoutMs,
        bool? Muted,
        bool? Confined,
        bool? Recording,
        string Ani,
        string Dnis,
        ConversationUriReference User,
        ConversationUriReference ExternalContact,
        ConversationUriReference ExternalOrganization,
        QueueConversationEmailEventTopicWrapup Wrapup,
        ConversationUriReference Queue);

    public record ConversationUriReference(string Id, string Name);
    public record QueueConversationEventTopicWrapup(string Code, string Notes, List<string> Tags, int? DurationSeconds, DateTime? EndTime, object AdditionalProperties);
    public record QueueConversationEmailEventTopicWrapup(string Code, string Notes, List<string> Tags, int? DurationSeconds, DateTime? EndTime, object AdditionalProperties);

    #region Sub models

    public enum DisconnectTypeEnum
    {
        //
        // Summary:
        //     Your SDK version is out of date and an unknown enum value was encountered. Please
        //     upgrade the SDK using the command "Upgrade-Package PureCloudApiSdk" in the Package
        //     Manager Console
        OutdatedSdkVersion = 0,
        //
        // Summary:
        //     Enum Endpoint for "endpoint"
        Endpoint = 1,
        //
        // Summary:
        //     Enum Client for "client"
        Client = 2,
        //
        // Summary:
        //     Enum System for "system"
        System = 3,
        //
        // Summary:
        //     Enum Transfer for "transfer"
        Transfer = 4,
        //
        // Summary:
        //     Enum Timeout for "timeout"
        Timeout = 5,
        //
        // Summary:
        //     Enum Transferconference for "transfer.conference"
        Transferconference = 6,
        //
        // Summary:
        //     Enum Transferconsult for "transfer.consult"
        Transferconsult = 7,
        //
        // Summary:
        //     Enum Transferforward for "transfer.forward"
        Transferforward = 8,
        //
        // Summary:
        //     Enum Transfernoanswer for "transfer.noanswer"
        Transfernoanswer = 9,
        //
        // Summary:
        //     Enum Transfernotavailable for "transfer.notavailable"
        Transfernotavailable = 10,
        //
        // Summary:
        //     Enum Transportfailure for "transport.failure"
        Transportfailure = 11,
        //
        // Summary:
        //     Enum Error for "error"
        Error = 12,
        //
        // Summary:
        //     Enum Peer for "peer"
        Peer = 13,
        //
        // Summary:
        //     Enum Other for "other"
        Other = 14,
        //
        // Summary:
        //     Enum Spam for "spam"
        Spam = 15,
        //
        // Summary:
        //     Enum Uncallable for "uncallable"
        Uncallable = 16
    }

    public enum AfterCallWorkStateEnum
    {
        OutdatedSdkVersion,
        Unknown,
        Skipped,
        Pending,
        Complete,
        Notapplicable
    }

    public enum ScreenRecordingStateEnum
    {
        OutdatedSdkVersion,
        Requested,
        Active,
        Paused,
        Stopped,
        Error,
        Timeout
    }

    public enum RecordingStateEnum
    {
        OutdatedSdkVersion,
        None,
        Active,
        Paused
    }

    public enum TypeEnum
    {
        OutdatedSdkVersion,
        Q850,
        Sip
    }

    public record ParticipantUser(string Id, string Name);
    public record ParticipantExternalContact(string Id, string Name);
    public record ParticipantExternalOrganization(string Id, string Name);

    #endregion 
}
