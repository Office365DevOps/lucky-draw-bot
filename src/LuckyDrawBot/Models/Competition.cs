using System;
using System.Collections.Generic;

namespace LuckyDrawBot.Models
{
    public class Competition
    {
        public Guid Id { get; set; }
        public Guid TenantId { get; set; }
        public string TeamId { get; set; }
        public string ChannelId { get; set; }
        public string MainActivityId { get; set; }
        public string ResultActivityId { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset PlannedDrawTime { get; set; }
        public DateTimeOffset ActualDrawTime { get; set; }
        public string Locale { get; set; }
        public string Gift { get; set; }
        public string Description { get; set; }
        public bool IsCompleted { get; set; }
        public string CreatorName { get; set; }
        public Guid CreatorAadObject { get; set; }
        public Guid WinnerAadObjectId { get; set; }
        public List<Competitor> Competitors { get; set; }
    }
}