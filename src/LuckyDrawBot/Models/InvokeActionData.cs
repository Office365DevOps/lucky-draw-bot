using System;
using Newtonsoft.Json;

namespace LuckyDrawBot.Models
{
    public enum InvokeActionType
    {
        Unknown,
        ViewDetail,
        Join
    }

    public class InvokeActionData
    {
        public const string TypeTaskFetch = "task/fetch";

        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

        public InvokeActionType UserAction { get; set; }
        public Guid CompetitionId { get; set; }
    }
}