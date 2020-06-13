using System;
using System.Text.Json;
using LuckyDrawBot.Models;
using Newtonsoft.Json.Linq;

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

        public static InvokeActionData GetInvokeActionData(this Activity activity)
        {
            if (activity.Type != ActivityTypes.Invoke)
            {
                throw new Exception($"Cannot get InvokeActionData, as the type of activity is not 'Invoke'. The type is '{activity.Type}'");
            }

            var value = (JObject)activity.Value;
            if (value.ContainsKey("data"))
            {
                var data = value.GetValue("data");
                return JsonSerializer.Deserialize<InvokeActionData>(Newtonsoft.Json.JsonConvert.SerializeObject(data));
            }
            else
            {
                return JsonSerializer.Deserialize<InvokeActionData>(Newtonsoft.Json.JsonConvert.SerializeObject(activity.Value));
            }
        }
    }
}