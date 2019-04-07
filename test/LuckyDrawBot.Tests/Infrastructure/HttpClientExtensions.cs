using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LuckyDrawBot.Models;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json;

namespace System.Net.Http
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> SendTeamsText(
            this HttpClient httpClient,
            string text,
            string locale = null,
            double? offsetHours = null)
        {
            var activity = new Activity
            {
                ServiceUrl = "https://service-url.com",
                ChannelId = "msteams",
                Type = "message",
                Text = text,
                Locale = locale ?? "en-us",
                LocalTimestamp = offsetHours.HasValue ? new DateTimeOffset(2018, 1, 1, 1, 1, 1, 1, TimeSpan.FromHours(offsetHours.Value)) : (DateTimeOffset?)null,
                From = new ChannelAccount("id", "name"),
                Recipient = new ChannelAccount("bot id", "bot name"),
                Conversation = new ConversationAccount(isGroup: true, id: "conv id", name: "conv name"),
                ChannelData = new TeamsChannelData
                {
                    Tenant = new TenantInfo { Id = Guid.NewGuid().ToString() },
                    Team = new TeamInfo { Id = Guid.NewGuid().ToString() },
                    Channel = new ChannelInfo { Id = Guid.NewGuid().ToString() },
                }
            };

            return await httpClient.SendActivity(activity);
        }

        public static async Task<HttpResponseMessage> SendTeamsInvoke(
            this HttpClient httpClient,
            InvokeActionData invokeValue,
            ChannelAccount from = null)
        {
            var activity = new Activity
            {
                ServiceUrl = "https://service-url.com",
                ChannelId = "msteams",
                Type = "invoke",
                Value = invokeValue,
                From = from ?? new ChannelAccount("id", "name")
            };

            return await httpClient.SendActivity(activity);
        }

        public static async Task<HttpResponseMessage> SendActivity(this HttpClient httpClient, Activity activity)
        {
            var requestBody = new StringContent(JsonConvert.SerializeObject(activity), Encoding.UTF8, "application/json");
            return await httpClient.PostAsync("messages", requestBody);
        }
    }
}