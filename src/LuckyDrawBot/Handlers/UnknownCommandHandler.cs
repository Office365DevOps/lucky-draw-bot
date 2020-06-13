using LuckyDrawBot.Services;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;

namespace LuckyDrawBot.Handlers
{
    public class UnknownCommandHandler
    {
        private readonly IBotClientFactory _botClientFactory;
        private readonly ILocalizationFactory _localizationFactory;

        public UnknownCommandHandler(IBotClientFactory botClientFactory, ILocalizationFactory localizationFactory)
        {
            _botClientFactory = botClientFactory;
            _localizationFactory = localizationFactory;
        }

        public async Task Handle(Activity activity)
        {
            var localization = _localizationFactory.Create(activity.Locale);
            var unknownCommandReply = activity.CreateReply(localization["UnknownCommand.Message"]);
            using (var botClient = _botClientFactory.CreateBotClient(activity.ServiceUrl))
            {
                await botClient.SendToConversationAsync(unknownCommandReply);
            }
        }
    }
}
