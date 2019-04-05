using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using LuckyDrawBot.Models;
using LuckyDrawBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LuckyDrawBot.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class MessagesController : ControllerBase
    {
        private class CreateCompetitionParameters
        {
            public string Gift { get; set; }
            public string GiftImageUrl { get; set; }
            public int WinnerCount { get; set; }
            public DateTimeOffset PlannedDrawTime { get; set; }
            public string Description { get; set; }
        }

        private readonly ILogger<MessagesController> _logger;
        private readonly IBotClientFactory _botClientFactory;
        private readonly ICompetitionService _competitionService;
        private readonly IActivityBuilder _activityBuilder;
        private readonly ITimerService _timerService;
        private readonly IDateTimeService _dateTimeService;

        public MessagesController(ILogger<MessagesController> logger, IBotClientFactory botClientFactory, ICompetitionService competitionService, IActivityBuilder activityBuilder, ITimerService timerService, IDateTimeService dateTimeService)
        {
            _logger = logger;
            _botClientFactory = botClientFactory;
            _competitionService = competitionService;
            _activityBuilder = activityBuilder;
            _timerService = timerService;
            _dateTimeService = dateTimeService;
        }

        [HttpPost]
        [Route("messages")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMessage([FromBody]Activity activity)
        {
            _logger.LogInformation($"ChannelId:{activity.ChannelId} Type:{activity.Type} Action:{activity.Action} ValueType:{activity.ValueType} Value:{activity.Value}");

            if (activity.ChannelId != "msteams")
            {
                return Ok();
            }

            if (activity.Type == ActivityTypes.Invoke)
            {
                var invokeActionData = JsonConvert.DeserializeObject<InvokeActionData>(JsonConvert.SerializeObject(activity.Value));
                switch(invokeActionData.UserAction)
                {
                    // case InvokeActionType.ViewDetail:
                    //     var taskEnvelope = new {
                    //         task = new {
                    //             type = "continue",
                    //             value = new {
                    //                 title = "abc",
                    //                 height = 500,
                    //                 width = 400,
                    //                 url = "https://www.ngrok.io"
                    //             }
                    //         }
                    //     };
                    //     var json = JsonConvert.SerializeObject(taskEnvelope);
                    //     _logger.LogWarning(json);
                    //     return Ok(taskEnvelope);
                    case InvokeActionType.Join:
                        await HandleJoinCompetitionAction(invokeActionData, activity);
                        return Ok();
                    default:
                        throw new Exception("Unknown invoke action type: " + activity.Type);
                }
            }
            else if (activity.Type == ActivityTypes.Message)
            {
                var succeeded = await HandleCompetitionInitialization(activity);
                if (!succeeded)
                {
                    await HandleDisplayHelp(activity);
                    return Ok();
                }
            }
            return Ok();
        }

        private async Task<bool> HandleCompetitionInitialization(Activity activity)
        {
            var parameters = ParseCreateCompetitionParameters(activity);
            if (parameters == null)
            {
                return false;
            }

            var channelData = activity.GetChannelData<TeamsChannelData>();
            var competition = await _competitionService.Create(
                                                               activity.ServiceUrl,
                                                               Guid.Parse(channelData.Tenant.Id),
                                                               channelData.Team.Id,
                                                               channelData.Channel.Id,
                                                               parameters.PlannedDrawTime,
                                                               activity.Locale,
                                                               parameters.Gift,
                                                               parameters.GiftImageUrl,
                                                               parameters.Description,
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
                Url.Action(nameof(CompetitionsController.DrawForCompetition), "Competitions", new { competitionId = competition.Id }));

            return true;
        }

        private async Task HandleJoinCompetitionAction(InvokeActionData invokeActionData, Activity activity)
        {
            var competition = await _competitionService.AddCompetitor(invokeActionData.CompetitionId, activity.From.AadObjectId, activity.From.Name);
            var updatedActivity = _activityBuilder.CreateMainActivity(competition);
            using (var botClient = _botClientFactory.CreateBotClient(activity.ServiceUrl))
            {
                await botClient.UpdateActivityAsync(competition.ChannelId, competition.MainActivityId, updatedActivity);
            }
        }

        private async Task HandleDisplayHelp(Activity activity)
        {
            var help = activity.CreateReply(
                "Hi there, To start a lucky draw type something like <b>@luckydraw secret gift, 1h</b>. Want more? here is the cheat sheet:<br/>"
                + "@luckydraw [gift name], [draw time], [the number of gifts], [the url of gift url]<br>"
            );
            using (var botClient = _botClientFactory.CreateBotClient(activity.ServiceUrl))
            {
                await botClient.SendToConversationAsync(help);
            }
        }

        private CreateCompetitionParameters ParseCreateCompetitionParameters(Activity activity)
        {
            const string MentionBotEndingFlag = "</at>";
            var text = activity.Text;
            if (text.IndexOf(MentionBotEndingFlag) < 0)
            {
                return null;
            }
            text = text.Substring(text.IndexOf(MentionBotEndingFlag) + MentionBotEndingFlag.Length);

            var parts = text.Split(',').Select(p => p.Trim()).ToArray();
            if (parts.Length < 2)
            {
                return null;
            }

            var gift = parts[0].Trim();
            var winnerCount = int.Parse(parts[1]);
            var offset = activity.LocalTimestamp.HasValue ? activity.LocalTimestamp.Value.Offset : TimeSpan.Zero;
            DateTimeOffset plannedDrawTime;
            if (parts.Length > 2)
            {
                var time = DateTimeOffset.Parse(parts[2].Trim(), CultureInfo.GetCultureInfo(activity.Locale));
                plannedDrawTime = new DateTimeOffset(time.Year, time.Month, time.Day, time.Hour, time.Minute, time.Second, 0, offset).ToUniversalTime();
            }
            else
            {
                plannedDrawTime = _dateTimeService.UtcNow.AddMinutes(1);
            }
            var giftImageUrl = parts.Length > 3 ? parts[3].Trim() : string.Empty;

            var plannedDrawTimeString = plannedDrawTime.ToOffset(offset).ToString("f", CultureInfo.GetCultureInfo(activity.Locale));
            return new CreateCompetitionParameters
            {
                Gift = gift,
                GiftImageUrl = giftImageUrl,
                WinnerCount = winnerCount,
                PlannedDrawTime = plannedDrawTime,
                Description = $"{winnerCount} prize(s). Time: {plannedDrawTimeString}"
            };
        }
    }
}
