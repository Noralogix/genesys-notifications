using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Genesys.Client.Notifications
{
    public class Metadata
    {
        public Metadata() { }
        [JsonPropertyName("CorrelationId")] public string CorrelationId { get; set; }
    }
    public interface INotificationData
    {
        string TopicName { get; set; }
        string Version { get; set; }
        Metadata Metadata { get; set; }
    }
    public partial class NotificationData<T> : INotificationData
    {
        public NotificationData() { }
        [JsonPropertyName("topicName")] public string TopicName { get; set; }
        [JsonPropertyName("version")] public string Version { get; set; }
        [JsonPropertyName("eventBody")] public T EventBody { get; set; }
        [JsonPropertyName("metadata")] public Metadata Metadata { get; set; }
    }
    public partial class ChannelMetadataNotification
    {
        public ChannelMetadataNotification() { }
        [JsonPropertyName("message")] public string Message { get; set; }
    }
    public partial class Channel
    {
        [JsonPropertyName("connectUri")] public string ConnectUri { get; set; }
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("expires")] public DateTime? Expires { get; set; }
    }
    public partial class ChannelTopic
    {
        [JsonPropertyName("id")] public string Id { get; set; }
        [JsonPropertyName("selfUri")] public string SelfUri { get; private set; }
    }
    public partial class ChannelTopicEntityListing
    {
        [JsonPropertyName("entities")] public List<ChannelTopic> Entities { get; set; }
    }
    public class GenesysAuthTokenInfo
    {
        [JsonPropertyName("access_token")] public string AccessToken { get; set; }
        [JsonPropertyName("token_type")] public string TokenType { get; set; }
        [JsonPropertyName("expires_in")] public int? ExpiresIn { get; set; }
        [JsonPropertyName("error")] public string Error { get; set; }
        [JsonIgnore] public string Environment { get; set; }
    }
}
