using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

// REUSE-IgnoreStart
namespace TdLib
{
    /// <summary>
    /// Autogenerated TDLib APIs
    /// </summary>
    public static partial class TdApi
    {
        /// <summary>
        /// Reports a false deletion of a message by aggressive anti-spam checks; requires administrator rights in the supergroup. Can be called only for messages from chatEventMessageDeleted with can_report_anti_spam_false_positive == true
        /// </summary>
        public class ReportSupergroupAntiSpamFalsePositive : Function<Ok>
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "reportSupergroupAntiSpamFalsePositive";

            /// <summary>
            /// Extra data attached to the function
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Supergroup identifier
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("supergroup_id")]
            public long SupergroupId { get; set; }

            /// <summary>
            /// Identifier of the erroneously deleted message from chatEventMessageDeleted
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("message_id")]
            public long MessageId { get; set; }
        }

        /// <summary>
        /// Reports a false deletion of a message by aggressive anti-spam checks; requires administrator rights in the supergroup. Can be called only for messages from chatEventMessageDeleted with can_report_anti_spam_false_positive == true
        /// </summary>
        public static Task<Ok> ReportSupergroupAntiSpamFalsePositiveAsync(
            this Client client, long supergroupId = default, long messageId = default)
        {
            return client.ExecuteAsync(new ReportSupergroupAntiSpamFalsePositive
            {
                SupergroupId = supergroupId, MessageId = messageId
            });
        }
    }
}
// REUSE-IgnoreEnd