using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

namespace LuckyDrawBot.Services
{
    public class BotClient : IDisposable
    {
        private ConnectorClient _connector;

        public BotClient(string botServiceUrl, IDateTimeService dateTimeService)
        {
            MicrosoftAppCredentials.TrustServiceUrl(botServiceUrl, dateTimeService.UtcNow.AddDays(7).DateTime);
            _connector = new ConnectorClient(new Uri(botServiceUrl), "20128cb3-5809-4b4f-a32d-b9929e67238c", ".l!c}F4xt8=*{%x{I6wZ7Ek");
        }

        public void Dispose()
        {
            if (_connector != null)
            {
                _connector.Dispose();
                _connector = null;
            }
        }

        public async Task<ResourceResponse> SendToConversationAsync(Activity activity)
        {
            return await _connector.Conversations.SendToConversationAsync(activity);
        }

        public async Task<ResourceResponse> UpdateActivityAsync(string channelId, string replacedActivityId, Activity newActivity)
        {
            return await _connector.Conversations.UpdateActivityAsync(channelId, replacedActivityId, newActivity);
        }
    }

    public interface IBotClientFactory
    {
        BotClient CreateBotClient(string botServiceUrl);
    }

    public class BotClientFactory : IBotClientFactory
    {
        private readonly IDateTimeService _dateTimeService;

        public BotClientFactory(IDateTimeService dateTimeService)
        {
            _dateTimeService = dateTimeService;
        }

        public BotClient CreateBotClient(string botServiceUrl)
        {
            return new BotClient(botServiceUrl, _dateTimeService);
        }
    }
}