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
        /// <summary>
        /// Contains information about a Telegram Business account
        /// </summary>
        public partial class BusinessInfo : Object
        {
            /// <summary>
            /// Data type for serialization
            /// </summary>
            [JsonProperty("@type")]
            public override string DataType { get; set; } = "businessInfo";

            /// <summary>
            /// Extra data attached to the object
            /// </summary>
            [JsonProperty("@extra")]
            public override string Extra { get; set; }

            /// <summary>
            /// Location of the business; may be null if none
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("location")]
            public BusinessLocation Location { get; set; }

            /// <summary>
            /// Opening hours of the business; may be null if none. The hours are guaranteed to be valid and has already been split by week days
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("opening_hours")]
            public BusinessOpeningHours OpeningHours { get; set; }

            /// <summary>
            /// Opening hours of the business in the local time; may be null if none. The hours are guaranteed to be valid and has already been split by week days.
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("local_opening_hours")]
            public BusinessOpeningHours LocalOpeningHours { get; set; }

            /// <summary>
            /// Time left before the business will open the next time, in seconds; 0 if unknown. An updateUserFullInfo update is not triggered when value of this field changes
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("next_open_in")]
            public int NextOpenIn { get; set; }

            /// <summary>
            /// Time left before the business will close the next time, in seconds; 0 if unknown. An updateUserFullInfo update is not triggered when value of this field changes
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("next_close_in")]
            public int NextCloseIn { get; set; }

            /// <summary>
            /// The greeting message; may be null if none or the Business account is not of the current user
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("greeting_message_settings")]
            public BusinessGreetingMessageSettings GreetingMessageSettings { get; set; }

            /// <summary>
            /// The away message; may be null if none or the Business account is not of the current user
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("away_message_settings")]
            public BusinessAwayMessageSettings AwayMessageSettings { get; set; }

            /// <summary>
            /// Information about start page of the account; may be null if none
            /// </summary>
            [JsonConverter(typeof(Converter))]
            [JsonProperty("start_page")]
            public BusinessStartPage StartPage { get; set; }
        }
    }
}
// REUSE-IgnoreEnd