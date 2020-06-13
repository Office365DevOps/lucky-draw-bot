using System;

namespace Microsoft.Bot.Schema
{
    public static class ActivityExtensions
    {
        public static string GetTrimmedText(this Activity activity)
        {
            const string MentionBotEndingFlag = "</at>";
            var text = activity.Text;
            if (text.IndexOf(MentionBotEndingFlag) < 0)
            {
                return null;
            }
            text = text.Substring(text.IndexOf(MentionBotEndingFlag) + MentionBotEndingFlag.Length);
            text = text.Trim();
            return text;
        }

        public static TimeSpan GetOffset(this Activity activity)
        {
            return activity.LocalTimestamp.HasValue ? activity.LocalTimestamp.Value.Offset : TimeSpan.Zero;
        }
    }
}