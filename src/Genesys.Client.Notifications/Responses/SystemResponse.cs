using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Genesys.Client.Notifications.Responses
{
    public class SystemResponse
    {
        private static Lazy<JsonSerializerOptions> _options = new Lazy<JsonSerializerOptions>(() =>
        {
            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            };
            options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            return options;
        });

        public enum TopicType
        {
            no_longer_subscribed, subscription_changed, token_revoked
        }
        public enum TopicReason
        {
            another_channel_subscribed, user_tokens_revoked
        }
        public string ChannelId { get; set; }
        public TopicType SystemTopicType { get; set; }
        public string CorrelationId { get; set; }
        public string OrganizationId { get; set; }
        public string UserId { get; set; }
        public string OauthClientId { get; set; }        
        public TopicReason Reason { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }

        private const string no_longer_subscribed = "v2.system.no_longer_subscribed";
        private const string subscription_changed = "v2.system.subscription_changed";
        private const string token_revoked = "v2.system.token_revoked";

        private static string[] systemTopics = new string[] { no_longer_subscribed, subscription_changed, token_revoked };

        internal static bool TryHandle(GenesysMessage message, ISubject<SystemResponse> subject)
        {
            var topic = message.TopicName()?.ToLower();
            if (string.IsNullOrEmpty(topic)) return false;
            if (topic.Contains(no_longer_subscribed)
                || topic.Contains(subscription_changed)
                || topic.Contains(token_revoked))
            {
                var data = JsonSerializer.Deserialize<SystemResponse>(message.Raw, _options.Value);
                data.Date = DateTime.UtcNow;
                subject.OnNext(data);
                return true;
            }
            return false;
        }
    }
}
