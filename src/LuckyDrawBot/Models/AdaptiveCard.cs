using System;
using System.Collections.Generic;
using Microsoft.Bot.Schema;

namespace LuckyDrawBot.Models
{
    public enum AdaptiveTextSize
    {
        Default = 0,
        Small = 1,
        Medium = 2,
        Large = 3,
        ExtraLarge = 4
    }

    public class AdaptiveCard
    {
        public class AdaptiveBodyItem
        {
            public string Type { get; set; }
            public string Text { get; set; }
            public AdaptiveTextSize Size { get; set; }
        }

        public const string ContentType = "application/vnd.microsoft.card.adaptive";

        public string Type { get; } = "AdaptiveCard";

        public string Version { get; } = "1.0";

        public List<AdaptiveBodyItem> Body { get; set; } = new List<AdaptiveBodyItem>();
    }
}
