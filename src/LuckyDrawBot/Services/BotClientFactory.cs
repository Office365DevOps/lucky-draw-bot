using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LuckyDrawBot.Services
{
    public interface IBotClient : IDisposable
    {
        Task<ResourceResponse> SendToConversationAsync(Activity activity);
        Task<ResourceResponse> UpdateActivityAsync(string channelId, string replacedActivityId, Activity newActivity);
    }

    public class BotClient : IBotClient
    {
        private ILogger _logger;
        private ConnectorClient _connector;

        public BotClient(ILogger logger, string botServiceUrl, string botId, string botPassword, IDateTimeService dateTimeService)
        {
            _logger = logger;
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
            var retry = 0;
            List<Exception> exceptions = new List<Exception>();
            while (retry < 3)
            {
                try
                {
                    retry++;
                    return await _connector.Conversations.SendToConversationAsync(activity);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to invoke bot's SendToConversationAsync() method. {retry} {activity}", retry, activity);
                    exceptions.Add(ex);
                }
            }
            throw new AggregateException("Failed to invoke bot's SendToConversationAsync() method.", exceptions);
        }

        public async Task<ResourceResponse> UpdateActivityAsync(string channelId, string replacedActivityId, Activity newActivity)
        {
            var retry = 0;
            List<Exception> exceptions = new List<Exception>();
            while (retry < 3)
            {
                try
                {
                    retry++;
                    return await _connector.Conversations.UpdateActivityAsync(channelId, replacedActivityId, newActivity);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to invoke bot's UpdateActivityAsync() method. {retry} {channelId} {replacedActivityId} {newActivity}", retry, channelId, replacedActivityId, newActivity);
                    exceptions.Add(ex);
                }
            }
            throw new AggregateException("Failed to invoke bot's UpdateActivityAsync() method.", exceptions);
        }
    }

    public interface IBotClientFactory
    {
        IBotClient CreateBotClient(string botServiceUrl);
    }

    public class BotClientFactory : IBotClientFactory
    {
        private ILogger<BotClientFactory> _logger;
        private readonly IDateTimeService _dateTimeService;
        private readonly IConfiguration _configuration;

        public BotClientFactory(ILogger<BotClientFactory> logger, IDateTimeService dateTimeService, IConfiguration configuration)
        {
            _logger = logger;
            _dateTimeService = dateTimeService;
            _configuration = configuration;
        }

        public IBotClient CreateBotClient(string botServiceUrl)
        {
            return new BotClient(
                _logger,
                botServiceUrl,
                _configuration.GetValue<string>("Bot:Id"),
                _configuration.GetValue<string>("Bot:Password"),
                _dateTimeService);
        }
    }
}