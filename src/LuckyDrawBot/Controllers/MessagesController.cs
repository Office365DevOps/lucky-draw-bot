using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private readonly ILogger<MessagesController> _logger;
        private readonly IBotClientFactory _botClientFactory;
        private readonly ICompetitionService _competitionService;
        private readonly IActivityBuilder _activityBuilder;

        public MessagesController(ILogger<MessagesController> logger, IBotClientFactory botClientFactory, ICompetitionService competitionService, IActivityBuilder activityBuilder)
        {
            _logger = logger;
            _botClientFactory = botClientFactory;
            _competitionService = competitionService;
            _activityBuilder = activityBuilder;
        }

        [HttpPost]
        [Route("message")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMessage([FromBody]Activity activity)
        {
            _logger.LogInformation($"Type:{activity.Type} Action:{activity.Action} ValueType:{activity.ValueType} Value:{activity.Value}");

            if (activity.Type == ActivityTypes.Invoke)
            {
                var invokeActionData = JsonConvert.DeserializeObject<InvokeActionData>(JsonConvert.SerializeObject(activity.Value));
                switch(invokeActionData.Type)
                {
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

        [HttpGet]
        [Route("draw")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> Draw()
        {
            var competitionIds = await _competitionService.GetToBeDrawnCompetitionIds();
            foreach (var competitionId in competitionIds)
            {
                var competition = await _competitionService.Draw(competitionId);
                var resultActivity = _activityBuilder.CreateResultActivity(competition);

                using (var botClient = _botClientFactory.CreateBotClient(competition.ServiceUrl))
                {
                    var resultMessage = await botClient.SendToConversationAsync(resultActivity);
                    await _competitionService.UpdateResultActivity(competition.Id, resultMessage.Id);
                }
            }
            return Ok(competitionIds);
        }

        private async Task<bool> HandleCompetitionInitialization(Activity activity)
        {
            var parts = activity.Text.Split(',').Select(p => p.Trim()).ToArray();
            if (parts.Length < 2)
            {
                return false;
            }

            var gift = parts[0].Substring(parts[0].IndexOf(' ') + 1);

            var channelData = activity.GetChannelData<TeamsChannelData>();
            var competition = await _competitionService.Create(
                                                               activity.ServiceUrl,
                                                               Guid.Parse(channelData.Tenant.Id),
                                                               channelData.Team.Id,
                                                               channelData.Channel.Id,
                                                               DateTimeOffset.UtcNow,
                                                               activity.Locale,
                                                               gift,
                                                               "detail terms",
                                                               1,
                                                               activity.From.Name,
                                                               activity.From.AadObjectId);
            var mainActivity = _activityBuilder.CreateMainActivity(competition);
            using (var botClient = _botClientFactory.CreateBotClient(activity.ServiceUrl))
            {
                var mainMessage = await botClient.SendToConversationAsync(mainActivity);
                await _competitionService.UpdateMainActivity(competition.Id, mainMessage.Id);
            }

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

    }
}
