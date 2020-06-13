using LuckyDrawBot.Services;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using System;
using System.Threading.Tasks;

namespace LuckyDrawBot.Handlers
{
    public class CommandCreateDraftCompetitionHandler
    {
        private readonly IBotClientFactory _botClientFactory;
        private readonly ICompetitionService _competitionService;
        private readonly IActivityBuilder _activityBuilder;

        public CommandCreateDraftCompetitionHandler(IBotClientFactory botClientFactory, ICompetitionService competitionService, IActivityBuilder activityBuilder)
        {
            _botClientFactory = botClientFactory;
            _competitionService = competitionService;
            _activityBuilder = activityBuilder;
        }

        public async Task Handle(Activity activity)
        {
            var channelData = activity.GetChannelData<TeamsChannelData>();
            var draftCompetition = await _competitionService.CreateDraftCompetition(
                                                            activity.ServiceUrl,
                                                            Guid.Parse(channelData.Tenant.Id),
                                                            channelData.Team.Id,
                                                            channelData.Channel.Id,
                                                            activity.Locale,
                                                            activity.GetOffset().TotalHours,
                                                            activity.From.Name,
                                                            activity.From.AadObjectId);

            var activityForDraft = _activityBuilder.CreateMainActivity(draftCompetition);
            using (var botClient = _botClientFactory.CreateBotClient(activity.ServiceUrl))
            {
                var mainMessage = await botClient.SendToConversationAsync(activityForDraft);
                await _competitionService.UpdateMainActivity(draftCompetition.Id, mainMessage.Id);
            }
        }
    }
}
