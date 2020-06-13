using LuckyDrawBot.Services;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;

namespace LuckyDrawBot.Handlers
{
    public class CommandHelpHandler
    {
        private readonly IBotClientFactory _botClientFactory;
        private readonly ILocalizationFactory _localizationFactory;

        public CommandHelpHandler(IBotClientFactory botClientFactory, ILocalizationFactory localizationFactory)
        {
            _botClientFactory = botClientFactory;
            _localizationFactory = localizationFactory;
        }

        public async Task Handle(Activity activity)
        {
            var localization = _localizationFactory.Create(activity.Locale);
            var help = activity.CreateReply(localization["Help.Message"]);
            using (var botClient = _botClientFactory.CreateBotClient(activity.ServiceUrl))
            {
                await botClient.SendToConversationAsync(help);
            }
        }
    }
}
