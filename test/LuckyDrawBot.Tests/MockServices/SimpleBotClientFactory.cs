using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LuckyDrawBot.Services;
using Microsoft.Bot.Schema;

namespace LuckyDrawBot.Tests.MockServices
{
    public class SimpleBotClient : IBotClient
    {
        private readonly SimpleBotClientFactory _factory;

        public SimpleBotClient(SimpleBotClientFactory factory)
        {
            _factory = factory;
        }

        public void Dispose()
        {
        }

        public async Task<ResourceResponse> SendToConversationAsync(Activity activity)
        {
            var response = new ResourceResponse(Guid.NewGuid().ToString());
            _factory.CreatedMessages.Add(new CreatedMessage
            {
                Activity = activity,
                Response = response
            });
            return await Task.FromResult(response);
        }

        public async Task<ResourceResponse> UpdateActivityAsync(string channelId, string replacedActivityId, Activity newActivity)
        {
            var response = new ResourceResponse(Guid.NewGuid().ToString());
            _factory.UpdatedMessages.Add(new UpdatedMessage
            {
                ChannelId = channelId,
                ReplacedActivityId = replacedActivityId,
                NewActivity = newActivity,
                Response = response
            });
            return await Task.FromResult(response);
        }
    }

    public class CreatedMessage
    {
        public Activity Activity { get; set; }
        public ResourceResponse Response { get; set; }
    }

    public class UpdatedMessage
    {
        public string ChannelId { get; set; }
        public string ReplacedActivityId { get; set; }
        public Activity NewActivity { get; set; }
        public ResourceResponse Response { get; set; }
    }

    public class SimpleBotClientFactory : IBotClientFactory
    {
        public List<CreatedMessage> CreatedMessages { get; } = new List<CreatedMessage>();
        public List<UpdatedMessage> UpdatedMessages { get; } = new List<UpdatedMessage>();

        public IBotClient CreateBotClient(string botServiceUrl)
        {
            return new SimpleBotClient(this);
        }
    }
}