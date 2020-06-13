using LuckyDrawBot.Controllers;
using LuckyDrawBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace LuckyDrawBot.Handlers
{
    public class CreateCompetitionCommandHandler
    {
        private const char ChineseCommaCharacter = '，';
        private readonly IDateTimeService _dateTimeService;
        private readonly IBotClientFactory _botClientFactory;
        private readonly ILocalizationFactory _localizationFactory;
        private readonly ICompetitionService _competitionService;
        private readonly IActivityBuilder _activityBuilder;
        private readonly ITimerService _timerService;

        public CreateCompetitionCommandHandler(IDateTimeService dateTimeService, IBotClientFactory botClientFactory, ILocalizationFactory localizationFactory, ICompetitionService competitionService, IActivityBuilder activityBuilder, ITimerService timerService)
        {
            _dateTimeService = dateTimeService;
            _botClientFactory = botClientFactory;
            _localizationFactory = localizationFactory;
            _competitionService = competitionService;
            _activityBuilder = activityBuilder;
            _timerService = timerService;
        }

        public bool CanHandle(Activity activity)
        {
            var parameters = ParseCreateCompetitionParameters(activity);
            return (parameters != null);
        }

        public async Task Handle(Activity activity, IUrlHelper urlHelper)
        {
            var parameters = ParseCreateCompetitionParameters(activity);

            var channelData = activity.GetChannelData<TeamsChannelData>();
            if (parameters.WinnerCount <= 0)
            {
                var localization = _localizationFactory.Create(activity.Locale);
                var invalidCommandReply = activity.CreateReply(localization["InvalidCommand.WinnerCountLessThanOne"]);
                using (var botClient = _botClientFactory.CreateBotClient(activity.ServiceUrl))
                {
                    await botClient.SendToConversationAsync(invalidCommandReply);
                }
                return;
            }

            if (parameters.PlannedDrawTime < _dateTimeService.UtcNow)
            {
                var localization = _localizationFactory.Create(activity.Locale);
                var invalidCommandReply = activity.CreateReply(localization["InvalidCommand.PlannedDrawTimeNotFuture"]);
                using (var botClient = _botClientFactory.CreateBotClient(activity.ServiceUrl))
                {
                    await botClient.SendToConversationAsync(invalidCommandReply);
                }
                return;
            }

            var competition = await _competitionService.CreateActiveCompetition(
                                                               activity.ServiceUrl,
                                                               Guid.Parse(channelData.Tenant.Id),
                                                               channelData.Team.Id,
                                                               channelData.Channel.Id,
                                                               parameters.PlannedDrawTime,
                                                               activity.Locale,
                                                               parameters.OffsetHours,
                                                               parameters.Gift,
                                                               parameters.GiftImageUrl,
                                                               parameters.WinnerCount,
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

        private CreateCompetitionParameters ParseCreateCompetitionParameters(Activity activity)
        {
            var text = activity.GetTrimmedText();

            var offset = activity.LocalTimestamp.HasValue ? activity.LocalTimestamp.Value.Offset : TimeSpan.Zero;

            var parts = text.Split(',', ChineseCommaCharacter).Select(p => p.Trim()).ToArray();
            if (parts.Length < 2)
            {
                return null;
            }

            var gift = parts[0].Trim();
            int winnerCount;
            if (!int.TryParse(parts[1], out winnerCount))
            {
                return null;
            }
            DateTimeOffset plannedDrawTime;
            if (parts.Length > 2)
            {
                var timeString = parts[2].Trim();
                if (TryParseTimeDuration(timeString, out TimeSpan duration))
                {
                    plannedDrawTime = _dateTimeService.UtcNow.Add(duration);
                }
                else
                {
                    DateTimeOffset time;
                    if (!DateTimeOffset.TryParse(timeString, CultureInfo.GetCultureInfo(activity.Locale), DateTimeStyles.None, out time))
                    {
                        return null;
                    }
                    plannedDrawTime = new DateTimeOffset(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, 0, offset).ToUniversalTime();
                }
            }
            else
            {
                plannedDrawTime = _dateTimeService.UtcNow.AddMinutes(1);
            }
            var giftImageUrl = parts.Length > 3 ? parts[3].Trim() : string.Empty;

            return new CreateCompetitionParameters
            {
                Gift = gift,
                GiftImageUrl = giftImageUrl,
                WinnerCount = winnerCount,
                PlannedDrawTime = plannedDrawTime,
                OffsetHours = offset.TotalHours
            };
        }

        // We will leverage LUIS to parse the input time
        private bool TryParseTimeDuration(string time, out TimeSpan duration)
        {
            var minutePostfixes = new string[] { "m", "min", "mins", "minute", "minutes", "分钟" };
            var hourPostfixes = new string[] { "h", "hr", "hrs", "hour", "hours", "小时" };

            foreach (var minutePostfix in minutePostfixes)
            {
                if (time.EndsWith(minutePostfix, StringComparison.InvariantCultureIgnoreCase)
                    && double.TryParse(time.Substring(0, time.Length - minutePostfix.Length), out double minutes))
                {
                    duration = TimeSpan.FromMinutes(minutes);
                    return true;
                }
            }
            foreach (var hourPostfix in hourPostfixes)
            {
                if (time.EndsWith(hourPostfix, StringComparison.InvariantCultureIgnoreCase)
                    && double.TryParse(time.Substring(0, time.Length - hourPostfix.Length), out double hours))
                {
                    duration = TimeSpan.FromHours(hours);
                    return true;
                }
            }
            duration = TimeSpan.Zero;
            return false;
        }

        private class CreateCompetitionParameters
        {
            public string Gift { get; set; }
            public string GiftImageUrl { get; set; }
            public int WinnerCount { get; set; }
            public DateTimeOffset PlannedDrawTime { get; set; }
            public double OffsetHours { get; set; }
        }
    }
}
