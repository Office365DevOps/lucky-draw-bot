using LuckyDrawBot.Models;
using LuckyDrawBot.Services;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;

namespace LuckyDrawBot.Handlers
{
    public class ActionViewCompetitionDetailHandler
    {
        private readonly ICompetitionService _competitionService;
        private readonly IActivityBuilder _activityBuilder;

        public ActionViewCompetitionDetailHandler(ICompetitionService competitionService, IActivityBuilder activityBuilder)
        {
            _competitionService = competitionService;
            _activityBuilder = activityBuilder;
        }

        public async Task<TaskModuleTaskInfoResponse> Handle(Activity activity)
        {
            var invokeActionData = activity.GetInvokeActionData();
            var competition = await _competitionService.GetCompetition(invokeActionData.CompetitionId);
            var taskInfoResponse = _activityBuilder.CreateCompetitionDetailTaskInfoResponse(competition);
            return taskInfoResponse;
        }
    }
}
