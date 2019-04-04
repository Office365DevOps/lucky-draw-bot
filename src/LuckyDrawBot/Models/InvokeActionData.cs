using System;

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
        public string Type { get; set; }
        public InvokeActionType UserAction { get; set; }
        public Guid CompetitionId { get; set; }
    }
}