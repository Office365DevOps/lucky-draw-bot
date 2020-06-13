using System;
using System.Net;
using System.Threading.Tasks;
using LuckyDrawBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

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

        [HttpPost("competitions/{competitionId}/draw")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> DrawForCompetition([FromRoute]Guid competitionId)
        {
            await Draw(competitionId);
            return Ok();
        }

        [HttpPost("competitions/draw")]
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
            if (competition == null)
            {
                // Sometimes, the competitions was drawn but posting ResultActivity failed.
                // So, when the timer service invokes the "draw" endpoint again, the competition is in "Closed" list already.
                // Get the competition from "Closed" list, and then try to post ResultActivity to Teams again.
                _logger.LogWarning("The competition has already been drawn and closed. Try to get it from Closed competition list. {competitionId}", competitionId);
                competition = await _competitionService.GetCompetition(competitionId);
            }

            if (string.IsNullOrEmpty(competition.MainActivityId))
            {
                _logger.LogWarning("The competition has not generated main activity. Skip this competition. {competitionId}", competitionId);
                return false;
            }

            if (!string.IsNullOrEmpty(competition.ResultActivityId))
            {
                _logger.LogWarning("The competition has been drawn and the ResultActivity has already been posted. Skip the duplicate posting. {competitionId}", competitionId);
                return true;
            }

            using (var botClient = _botClientFactory.CreateBotClient(competition.ServiceUrl))
            {
                var updatedMainActivity = _activityBuilder.CreateMainActivity(competition);
                await botClient.UpdateActivityAsync(competition.ChannelId, competition.MainActivityId, updatedMainActivity);

                var resultActivity = _activityBuilder.CreateResultActivity(competition);
                var resultMessage = await botClient.SendToConversationAsync(resultActivity);
                await _competitionService.UpdateResultActivity(competition.Id, resultMessage.Id);
            }
            return true;
        }
    }
}
