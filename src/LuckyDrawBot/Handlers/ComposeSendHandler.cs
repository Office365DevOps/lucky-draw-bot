using AdaptiveCards;
using LuckyDrawBot.Controllers;
using LuckyDrawBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using System;
using System.Text.Json;
using System.Threading.Tasks;

namespace LuckyDrawBot.Handlers
{
    public class ComposeSendHandler
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IActivityBuilder _activityBuilder;
        private readonly IBotClientFactory _botClientFactory;
        private readonly ICompetitionService _competitionService;
        private readonly ITimerService _timerService;

        public ComposeSendHandler(IDateTimeService dateTimeService, IActivityBuilder activityBuilder, IBotClientFactory botClientFactory, ICompetitionService competitionService, ITimerService timerService)
        {
            _dateTimeService = dateTimeService;
            _activityBuilder = activityBuilder;
            _botClientFactory = botClientFactory;
            _competitionService = competitionService;
            _timerService = timerService;
        }

        public async Task Handle(Activity activity, IUrlHelper urlHelper)
        {
            var editForm = GetEditForm(activity);

            var offset = activity.GetOffset();
            var date = DateTimeOffset.Parse(editForm.PlannedDrawTimeLocalDate);
            var time = DateTimeOffset.Parse(editForm.PlannedDrawTimeLocalTime);
            var plannedDrawTime = new DateTimeOffset(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, 0, offset).ToUniversalTime();

            var channelData = activity.GetChannelData<TeamsChannelData>();
            var isGroupChat = channelData.Channel == null;
            var teamId = isGroupChat ? string.Empty : channelData.Team.Id;
            var channelId = isGroupChat ? activity.Conversation.Id : channelData.Channel.Id;
            var competition = await _competitionService.CreateActiveCompetition(
                                                               activity.ServiceUrl,
                                                               Guid.Parse(channelData.Tenant.Id),
                                                               teamId,
                                                               channelId,
                                                               plannedDrawTime,
                                                               activity.Locale,
                                                               offset.TotalHours,
                                                               editForm.Gift,
                                                               editForm.GiftImageUrl,
                                                               int.Parse(editForm.WinnerCount),
                                                               activity.From.Name,
                                                               activity.From.AadObjectId);

            var mainActivity = _activityBuilder.CreateMainActivity(competition);
            using (var botClient = _botClientFactory.CreateBotClient(activity.ServiceUrl))
            {
                var mainMessage = await botClient.SendToConversationAsync(mainActivity);
                await _competitionService.UpdateMainActivity(competition.Id, mainMessage.Id);
            }

            await _timerService.AddScheduledHttpRequest(
                competition.PlannedDrawTime,
                "POST",
                urlHelper.Action(nameof(CompetitionsController.DrawForCompetition), "Competitions", new { competitionId = competition.Id }));
        }

        private CompetitionEditForm GetEditForm(Activity activity)
        {
            var value = (JObject)activity.Value;
            var previewActivities = value.GetValue("botActivityPreview").ToObject<Activity[]>();
            var previewCard = ((JObject)previewActivities[0].Attachments[0].Content).ToObject<AdaptiveCard>();
            var luckyDrawDataTextBlock = previewCard.Body.Find(x => x.Id == "LuckyDrawData") as AdaptiveTextBlock;
            var editFormJson = luckyDrawDataTextBlock.Text;
            var editForm = JsonSerializer.Deserialize<CompetitionEditForm>(editFormJson);
            return editForm;
        }
    }
}
