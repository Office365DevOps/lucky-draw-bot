using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace System.Net.Http
{
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> SendTeamsText(this HttpClient httpClient, string text)
        {
            var activity = new Activity
            {
                Type = "message",
                Text = text,
                Locale = "en-us",
                From = new ChannelAccount("id", "name"),
                Recipient = new ChannelAccount("bot id", "bot name"),
                Conversation = new ConversationAccount(isGroup: true, id: "conv id", name: "conv name")
            };
            var requestBody = new StringContent(JsonConvert.SerializeObject(activity), Encoding.UTF8, "application/json");
            return await httpClient.PostAsync("message", requestBody);
        }
    }
}