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
    public class CompetitionsController : ControllerBase
    {
        private readonly ILogger<MessagesController> _logger;
        private readonly IBotClientFactory _botClientFactory;
        private readonly ICompetitionService _competitionService;
        private readonly IActivityBuilder _activityBuilder;

        public CompetitionsController(ILogger<MessagesController> logger, IBotClientFactory botClientFactory, ICompetitionService competitionService, IActivityBuilder activityBuilder)
        {
            _logger = logger;
            _botClientFactory = botClientFactory;
            _competitionService = competitionService;
            _activityBuilder = activityBuilder;
        }

        [HttpPost]
        [Route("competitions/{competitionId}/draw")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> DrawForCompetition([FromRoute]Guid competitionId)
        {
            await Draw(competitionId);
            return Ok();
        }

        [HttpPost]
        [Route("competitions/draw")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> DrawForAllCompetitions()
        {
            var competitionIds = await _competitionService.GetToBeDrawnCompetitionIds();
            foreach (var competitionId in competitionIds)
            {
                await Draw(competitionId);
            }
            return Ok(competitionIds);
        }

        private async Task<bool> Draw(Guid competitionId)
        {
            var competition = await _competitionService.Draw(competitionId);
            if (string.IsNullOrEmpty(competition.MainActivityId))
            {
                _logger.LogWarning("The competition has not generated main activity. Skip this competition. {competitionId}", competitionId);
                return false;
            }

            var resultActivity = _activityBuilder.CreateResultActivity(competition);

            using (var botClient = _botClientFactory.CreateBotClient(competition.ServiceUrl))
            {
                var resultMessage = await botClient.SendToConversationAsync(resultActivity);
                await _competitionService.UpdateResultActivity(competition.Id, resultMessage.Id);
            }
            return true;
        }
    }
}
