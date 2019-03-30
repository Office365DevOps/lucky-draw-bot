using System;

namespace LuckyDrawBot.Models
{
    public enum InvokeActionType
    {
        Join
    }

    public class InvokeActionData
    {
        public InvokeActionType Type { get; set; }
        public Guid CompetitionId { get; set; }
    }
}