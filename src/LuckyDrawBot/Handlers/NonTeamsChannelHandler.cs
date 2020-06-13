using LuckyDrawBot.Services;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;

namespace LuckyDrawBot.Handlers
{
    public class NonTeamsChannelHandler
    {
        private readonly IBotClientFactory _botClientFactory;

        public NonTeamsChannelHandler(IBotClientFactory botClientFactory)
        {
            _botClientFactory = botClientFactory;
        }

        public async Task Handle(Activity activity)
        {
            var reply = activity.CreateReply("Sorry, I work for Microsoft Teams only.");
            using (var botClient = _botClientFactory.CreateBotClient(activity.ServiceUrl))
            {
                await botClient.SendToConversationAsync(reply);
            }
        }
    }
}
