using System;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace LuckyDrawBot.Services
{
    public interface IBotClient : IDisposable
    {
        Task<ResourceResponse> SendToConversationAsync(Activity activity);
        Task<ResourceResponse> UpdateActivityAsync(string channelId, string replacedActivityId, Activity newActivity);
    }

    public class BotClient : IBotClient
    {
        private ConnectorClient _connector;

        public BotClient(string botServiceUrl, string botId, string botPassword, IDateTimeService dateTimeService)
        {
            MicrosoftAppCredentials.TrustServiceUrl(botServiceUrl, dateTimeService.UtcNow.AddDays(7).DateTime);
            _connector = new ConnectorClient(new Uri(botServiceUrl), botId, botPassword);
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
        IBotClient CreateBotClient(string botServiceUrl);
    }

    public class BotClientFactory : IBotClientFactory
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IConfiguration _configuration;

        public BotClientFactory(IDateTimeService dateTimeService, IConfiguration configuration)
        {
            _dateTimeService = dateTimeService;
            _configuration = configuration;
        }

        public IBotClient CreateBotClient(string botServiceUrl)
        {
            return new BotClient(
                botServiceUrl,
                _configuration.GetValue<string>("Bot:Id"),
                _configuration.GetValue<string>("Bot:Password"),
                _dateTimeService);
        }
    }
}