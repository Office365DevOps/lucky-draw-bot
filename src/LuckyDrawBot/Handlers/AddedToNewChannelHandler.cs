using LuckyDrawBot.Services;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;

namespace LuckyDrawBot.Handlers
{
    public class AddedToNewChannelHandler
    {
        private readonly IBotClientFactory _botClientFactory;

        public AddedToNewChannelHandler(IBotClientFactory botClientFactory)
        {
            _botClientFactory = botClientFactory;
        }

        public async Task Handle(Activity activity)
        {
            using (var botClient = _botClientFactory.CreateBotClient(activity.ServiceUrl))
            {
                // This incoming activity does not have locale information, so response welcome message in English
                var welcomeText = "Hi there, I'm LuckyDraw bot🎁. A teammate of yours recently added me to help your team create lucky draws.\r\n\r\n"
                                + "Quickstart guide\r\n\r\n"
                                + "* To create a lucky draw, type:\r\n\r\n"
                                + "  @LuckyDraw start\r\n\r\n"
                                + "* To find more about me, type:\r\n\r\n"
                                + "  @LuckyDraw help";
                var welcome = activity.CreateReply(welcomeText);
                await botClient.SendToConversationAsync(welcome);
            }
        }
    }
}
