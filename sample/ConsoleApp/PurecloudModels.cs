using Genesys.Client.Notifications;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;

namespace ConsoleApp
{
    public class QueueConversationChatEventTopicChatMediaParticipant
    {
        public string Id { get; set; }
        public Dictionary<string, string> Attributes { get; set; }
        public string Provider { get; set; }
        //
        // Summary:
        //     Gets or Sets Purpose
        //[DataMember(Name = "purpose", EmitDefaultValue = false)]
        public string Purpose { get; set; }

        ////
        //// Summary:
        ////     Gets or Sets EndTime
        //[DataMember(Name = "endTime", EmitDefaultValue = false)]
        public DateTime? EndTime { get; set; }
        ////
        //// Summary:
        ////     Gets or Sets StartTime
        //[DataMember(Name = "startTime", EmitDefaultValue = false)]
        public DateTime? StartTime { get; set; }
        //
        // Summary:
        //     Gets or Sets Name
        //[DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets State
        /// </summary>
        [JsonPropertyName("state")]
        public StateEnum? State { get; set; }
        /// <summary>
        /// Gets or Sets Direction
        /// </summary>
        [JsonPropertyName("direction")]
        public DirectionEnum? Direction { get; set; }
        /// <summary>
        /// Gets or Sets DisconnectType
        /// </summary>
        [JsonPropertyName("disconnectType")]
        public DisconnectTypeEnum? DisconnectType { get; set; }
    }

    public class QueueConversationChatEventTopicChatConversation
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("participants")]
        public List<QueueConversationChatEventTopicChatMediaParticipant> Participants { get; set; }
    }

    /// <summary>
    /// Gets or Sets State
    /// </summary>
    [JsonConverter(typeof(JsonEnumConverter<StateEnum>))]
    public enum StateEnum
    {
        /// <summary>
        /// Your SDK version is out of date and an unknown enum value was encountered. 
        /// Please upgrade the SDK using the command "Upgrade-Package PureCloudApiSdk" 
        /// in the Package Manager Console
        /// </summary>
        [EnumMember(Value = "OUTDATED_SDK_VERSION")]
        OutdatedSdkVersion,

        /// <summary>
        /// Enum Alerting for "alerting"
        /// </summary>
        [EnumMember(Value = "alerting")]
        Alerting,

        /// <summary>
        /// Enum Dialing for "dialing"
        /// </summary>
        [EnumMember(Value = "dialing")]
        Dialing,

        /// <summary>
        /// Enum Contacting for "contacting"
        /// </summary>
        [EnumMember(Value = "contacting")]
        Contacting,

        /// <summary>
        /// Enum Offering for "offering"
        /// </summary>
        [EnumMember(Value = "offering")]
        Offering,

        /// <summary>
        /// Enum Connected for "connected"
        /// </summary>
        [EnumMember(Value = "connected")]
        Connected,

        /// <summary>
        /// Enum Disconnected for "disconnected"
        /// </summary>
        [EnumMember(Value = "disconnected")]
        Disconnected,

        /// <summary>
        /// Enum Terminated for "terminated"
        /// </summary>
        [EnumMember(Value = "terminated")]
        Terminated,

        /// <summary>
        /// Enum Converting for "converting"
        /// </summary>
        [EnumMember(Value = "converting")]
        Converting,

        /// <summary>
        /// Enum Uploading for "uploading"
        /// </summary>
        [EnumMember(Value = "uploading")]
        Uploading,

        /// <summary>
        /// Enum Transmitting for "transmitting"
        /// </summary>
        [EnumMember(Value = "transmitting")]
        Transmitting,

        /// <summary>
        /// Enum Scheduled for "scheduled"
        /// </summary>
        [EnumMember(Value = "scheduled")]
        Scheduled,

        /// <summary>
        /// Enum None for "none"
        /// </summary>
        [EnumMember(Value = "none")]
        None
    }

    /// <summary>
    /// Gets or Sets Direction
    /// </summary>
    [JsonConverter(typeof(JsonEnumConverter<DirectionEnum>))]
    public enum DirectionEnum
    {
        /// <summary>
        /// Your SDK version is out of date and an unknown enum value was encountered. 
        /// Please upgrade the SDK using the command "Upgrade-Package PureCloudApiSdk" 
        /// in the Package Manager Console
        /// </summary>
        [EnumMember(Value = "OUTDATED_SDK_VERSION")]
        OutdatedSdkVersion,

        /// <summary>
        /// Enum Inbound for "inbound"
        /// </summary>
        [EnumMember(Value = "inbound")]
        Inbound,

        /// <summary>
        /// Enum Outbound for "outbound"
        /// </summary>
        [EnumMember(Value = "outbound")]
        Outbound
    }

    /// <summary>
    /// Gets or Sets DisconnectType
    /// </summary>
    [JsonConverter(typeof(JsonEnumConverter<DisconnectTypeEnum>))]
    public enum DisconnectTypeEnum
    {
        /// <summary>
        /// Your SDK version is out of date and an unknown enum value was encountered. 
        /// Please upgrade the SDK using the command "Upgrade-Package PureCloudApiSdk" 
        /// in the Package Manager Console
        /// </summary>
        [EnumMember(Value = "OUTDATED_SDK_VERSION")]
        OutdatedSdkVersion,

        /// <summary>
        /// Enum Endpoint for "endpoint"
        /// </summary>
        [EnumMember(Value = "endpoint")]
        Endpoint,

        /// <summary>
        /// Enum Client for "client"
        /// </summary>
        [EnumMember(Value = "client")]
        Client,

        /// <summary>
        /// Enum System for "system"
        /// </summary>
        [EnumMember(Value = "system")]
        System,

        /// <summary>
        /// Enum Transfer for "transfer"
        /// </summary>
        [EnumMember(Value = "transfer")]
        Transfer,

        /// <summary>
        /// Enum Timeout for "timeout"
        /// </summary>
        [EnumMember(Value = "timeout")]
        Timeout,

        /// <summary>
        /// Enum Transferconference for "transfer.conference"
        /// </summary>
        [EnumMember(Value = "transfer.conference")]
        Transferconference,

        /// <summary>
        /// Enum Transferconsult for "transfer.consult"
        /// </summary>
        [EnumMember(Value = "transfer.consult")]
        Transferconsult,

        /// <summary>
        /// Enum Transferforward for "transfer.forward"
        /// </summary>
        [EnumMember(Value = "transfer.forward")]
        Transferforward,

        /// <summary>
        /// Enum Transfernoanswer for "transfer.noanswer"
        /// </summary>
        [EnumMember(Value = "transfer.noanswer")]
        Transfernoanswer,

        /// <summary>
        /// Enum Transfernotavailable for "transfer.notavailable"
        /// </summary>
        [EnumMember(Value = "transfer.notavailable")]
        Transfernotavailable,

        /// <summary>
        /// Enum Transportfailure for "transport.failure"
        /// </summary>
        [EnumMember(Value = "transport.failure")]
        Transportfailure,

        /// <summary>
        /// Enum Error for "error"
        /// </summary>
        [EnumMember(Value = "error")]
        Error,

        /// <summary>
        /// Enum Peer for "peer"
        /// </summary>
        [EnumMember(Value = "peer")]
        Peer,

        /// <summary>
        /// Enum Other for "other"
        /// </summary>
        [EnumMember(Value = "other")]
        Other,

        /// <summary>
        /// Enum Spam for "spam"
        /// </summary>
        [EnumMember(Value = "spam")]
        Spam,

        /// <summary>
        /// Enum Uncallable for "uncallable"
        /// </summary>
        [EnumMember(Value = "uncallable")]
        Uncallable
    }

    /// <summary>
    /// Gets or Sets FlaggedReason
    /// </summary>
    [JsonConverter(typeof(JsonEnumConverter<FlaggedReasonEnum>))]
    public enum FlaggedReasonEnum
    {
        /// <summary>
        /// Your SDK version is out of date and an unknown enum value was encountered. 
        /// Please upgrade the SDK using the command "Upgrade-Package PureCloudApiSdk" 
        /// in the Package Manager Console
        /// </summary>
        [EnumMember(Value = "OUTDATED_SDK_VERSION")]
        OutdatedSdkVersion,

        /// <summary>
        /// Enum General for "general"
        /// </summary>
        [EnumMember(Value = "general")]
        General
    }

    public partial class PresenceEventOrganizationPresence : IEquatable<PresenceEventOrganizationPresence>
    {
        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [JsonPropertyName("id")] public string Id { get; set; }



        /// <summary>
        /// Gets or Sets SystemPresence
        /// </summary>
        [JsonPropertyName("systemPresence")] public string SystemPresence { get; set; }


        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class PresenceEventOrganizationPresence {\n");

            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  SystemPresence: ").Append(SystemPresence).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            return this.Equals(obj as PresenceEventOrganizationPresence);
        }

        /// <summary>
        /// Returns true if PresenceEventOrganizationPresence instances are equal
        /// </summary>
        /// <param name="other">Instance of PresenceEventOrganizationPresence to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(PresenceEventOrganizationPresence other)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            if (other == null)
                return false;

            return true &&
                (
                    this.Id == other.Id ||
                    this.Id != null &&
                    this.Id.Equals(other.Id)
                ) &&
                (
                    this.SystemPresence == other.SystemPresence ||
                    this.SystemPresence != null &&
                    this.SystemPresence.Equals(other.SystemPresence)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            // credit: http://stackoverflow.com/a/263416/677735
            unchecked // Overflow is fine, just wrap
            {
                int hash = 41;
                // Suitable nullity checks etc, of course :)

                if (this.Id != null)
                    hash = hash * 59 + this.Id.GetHashCode();

                if (this.SystemPresence != null)
                    hash = hash * 59 + this.SystemPresence.GetHashCode();

                return hash;
            }
        }
    }

    public partial class PresenceEventUserPresence : IEquatable<PresenceEventUserPresence>
    {
        [JsonPropertyName("source")]
        public string Source { get; set; }
        [JsonPropertyName("presenceDefinition")]
        public PresenceEventOrganizationPresence PresenceDefinition { get; set; }

        [JsonPropertyName("primary")] public bool? Primary { get; set; }
        [JsonPropertyName("message")] public string Message { get; set; }
        [JsonPropertyName("modifiedDate")] public DateTime? ModifiedDate { get; set; }


        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class PresenceEventUserPresence {\n");

            sb.Append("  Source: ").Append(Source).Append("\n");
            sb.Append("  PresenceDefinition: ").Append(PresenceDefinition).Append("\n");
            sb.Append("  Primary: ").Append(Primary).Append("\n");
            sb.Append("  Message: ").Append(Message).Append("\n");
            sb.Append("  ModifiedDate: ").Append(ModifiedDate).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="obj">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object obj)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            return this.Equals(obj as PresenceEventUserPresence);
        }

        /// <summary>
        /// Returns true if PresenceEventUserPresence instances are equal
        /// </summary>
        /// <param name="other">Instance of PresenceEventUserPresence to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(PresenceEventUserPresence other)
        {
            // credit: http://stackoverflow.com/a/10454552/677735
            if (other == null)
                return false;

            return true &&
                (
                    this.Source == other.Source ||
                    this.Source != null &&
                    this.Source.Equals(other.Source)
                ) &&
                (
                    this.PresenceDefinition == other.PresenceDefinition ||
                    this.PresenceDefinition != null &&
                    this.PresenceDefinition.Equals(other.PresenceDefinition)
                ) &&
                (
                    this.Primary == other.Primary ||
                    this.Primary != null &&
                    this.Primary.Equals(other.Primary)
                ) &&
                (
                    this.Message == other.Message ||
                    this.Message != null &&
                    this.Message.Equals(other.Message)
                ) &&
                (
                    this.ModifiedDate == other.ModifiedDate ||
                    this.ModifiedDate != null &&
                    this.ModifiedDate.Equals(other.ModifiedDate)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            // credit: http://stackoverflow.com/a/263416/677735
            unchecked // Overflow is fine, just wrap
            {
                int hash = 41;
                // Suitable nullity checks etc, of course :)

                if (this.Source != null)
                    hash = hash * 59 + this.Source.GetHashCode();

                if (this.PresenceDefinition != null)
                    hash = hash * 59 + this.PresenceDefinition.GetHashCode();

                if (this.Primary != null)
                    hash = hash * 59 + this.Primary.GetHashCode();

                if (this.Message != null)
                    hash = hash * 59 + this.Message.GetHashCode();

                if (this.ModifiedDate != null)
                    hash = hash * 59 + this.ModifiedDate.GetHashCode();

                return hash;
            }
        }
    }

}
