using System;
using System.Collections.Generic;

namespace LuckyDrawBot.Models
{
    public class Competition
    {
        public Guid Id { get; set; }
        public string ServiceUrl { get; set; }
        public Guid TenantId { get; set; }
        public string TeamId { get; set; }
        public string ChannelId { get; set; }
        public string MainActivityId { get; set; }
        public string ResultActivityId { get; set; }
        public DateTimeOffset CreatedTime { get; set; }
        public DateTimeOffset PlannedDrawTime { get; set; }
        public DateTimeOffset? ActualDrawTime { get; set; }
        public string Locale { get; set; }
        public double OffsetHours { get; set; }
        public string Gift { get; set; }
        public string GiftImageUrl { get; set; }
        public int WinnerCount { get; set; }
        public CompetitionStatus Status { get; set; }
        public string CreatorName { get; set; }
        public string CreatorAadObjectId { get; set; }
        public List<string> WinnerAadObjectIds { get; set; }
        public List<Competitor> Competitors { get; set; }
    }
}