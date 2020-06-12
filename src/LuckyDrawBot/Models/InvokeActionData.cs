using System;
using System.Text.Json.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LuckyDrawBot.Models
{
    [Newtonsoft.Json.JsonConverter(typeof(StringEnumConverter))]
    public enum InvokeActionType
    {
        Unknown = 0,
        ViewDetail = 1,
        Join = 2,
        EditDraft = 3,
        SaveDraft = 4,
        ActivateCompetition = 5
    }

    public class InvokeActionData
    {
        public const string TypeTaskFetch = "task/fetch";

        [JsonProperty(PropertyName = "type")]
        [JsonPropertyName("type")]
        public string Type { get; set; }

        public InvokeActionType UserAction { get; set; }
        public Guid CompetitionId { get; set; }
    }
}