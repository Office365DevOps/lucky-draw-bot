using System;

namespace LuckyDrawBot.Models
{
    public class Competitor
    {
        public string AadObjectId { get; set; }
        public string Name { get; set; }
        public DateTimeOffset JoinTime { get; set; }
    }
}