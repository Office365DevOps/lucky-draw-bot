using LuckyDrawBot.Controllers;
using LuckyDrawBot.Models;
using LuckyDrawBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LuckyDrawBot.Handlers
{
    public class ActionActivateCompetitionHandler
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IBotClientFactory _botClientFactory;
        private readonly ILocalizationFactory _localizationFactory;
        private readonly ICompetitionService _competitionService;
        private readonly IActivityBuilder _activityBuilder;
        private readonly ITimerService _timerService;

        public ActionActivateCompetitionHandler(IDateTimeService dateTimeService, IBotClientFactory botClientFactory, ILocalizationFactory localizationFactory, ICompetitionService competitionService, IActivityBuilder activityBuilder, ITimerService timerService)
        {
            _dateTimeService = dateTimeService;
            _botClientFactory = botClientFactory;
            _localizationFactory = localizationFactory;
            _competitionService = competitionService;
            _activityBuilder = activityBuilder;
            _timerService = timerService;
        }

        public async Task<TaskModuleResponse> Handle(Activity activity, IUrlHelper urlHelper)
        {
            var invokeActionData = activity.GetInvokeActionData();
            var competition = await _competitionService.GetCompetition(invokeActionData.CompetitionId);
            var errorMessage = CanActivateCompetition(competition);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return _activityBuilder.CreateDraftCompetitionEditTaskInfoResponse(competition, errorMessage, activity.Locale);
            }

            competition = await _competitionService.ActivateCompetition(competition.Id);

            var mainActivity = _activityBuilder.CreateMainActivity(competition);
            using (var botClient = _botClientFactory.CreateBotClient(activity.ServiceUrl))
            {
                await botClient.UpdateActivityAsync(competition.ChannelId, competition.MainActivityId, mainActivity);
            }

            await _timerService.AddScheduledHttpRequest(
                competition.PlannedDrawTime,
                "POST",
                urlHelper.Action(nameof(CompetitionsController.DrawForCompetition), "Competitions", new { competitionId = competition.Id }));
            return null;
        }

        private string CanActivateCompetition(Competition competition)
        {
            var localization = _localizationFactory.Create(competition.Locale);
            var errors = new List<string>();
            if (string.IsNullOrEmpty(competition.Gift))
            {
                errors.Add(localization["EditCompetition.Form.Gift.Invalid"]);
            }
            if (competition.WinnerCount <= 0)
            {
                errors.Add(localization["EditCompetition.Form.WinnerCount.Invalid"]);
            }
            if (competition.PlannedDrawTime < _dateTimeService.UtcNow)
            {
                errors.Add(localization["EditCompetition.Form.PlannedDrawTime.Invalid"]);
            }
            var giftImageUrl = competition.GiftImageUrl;
            if (!string.IsNullOrEmpty(giftImageUrl)
                && !giftImageUrl.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                && !giftImageUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
            {
                errors.Add(localization["EditCompetition.Form.GiftImageUrl.Invalid"]);
            }
            return string.Join(' ', errors);
        }

    }
}
