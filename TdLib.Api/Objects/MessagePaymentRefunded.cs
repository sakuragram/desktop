using System;
using Newtonsoft.Json;

// REUSE-IgnoreStart
namespace TdLib
{
    /// <summary>
    /// Autogenerated TDLib APIs
    /// </summary>
    public static partial class TdApi
    {
        public partial class MessageContent : Object
        {
            /// <summary>
            /// A payment has been refunded
            /// </summary>
            public class MessagePaymentRefunded : MessageContent
            {
                /// <summary>
                /// Data type for serialization
                /// </summary>
                [JsonProperty("@type")]
                public override string DataType { get; set; } = "messagePaymentRefunded";

                /// <summary>
                /// Extra data attached to the message
                /// </summary>
                [JsonProperty("@extra")]
                public override string Extra { get; set; }

                /// <summary>
                /// Identifier of the previous owner of the Telegram Stars that refunds them
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("owner_id")]
                public MessageSender OwnerId { get; set; }

                /// <summary>
                /// Currency for the price of the product
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("currency")]
                public string Currency { get; set; }

                /// <summary>
                /// Total price for the product, in the smallest units of the currency
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("total_amount")]
                public long TotalAmount { get; set; }

                /// <summary>
                /// Invoice payload; only for bots
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("invoice_payload")]
                public byte[] InvoicePayload { get; set; }

                /// <summary>
                /// Telegram payment identifier
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("telegram_payment_charge_id")]
                public string TelegramPaymentChargeId { get; set; }

                /// <summary>
                /// Provider payment identifier
                /// </summary>
                [JsonConverter(typeof(Converter))]
                [JsonProperty("provider_payment_charge_id")]
                public string ProviderPaymentChargeId { get; set; }
            }
        }
    }
}
// REUSE-IgnoreEnd