using LuckyDrawBot.Models;
using LuckyDrawBot.Services;
using Microsoft.Bot.Schema;
using System.Threading.Tasks;

namespace LuckyDrawBot.Handlers
{
    public class ActionEditDraftCompetitionHandler
    {
        private readonly ICompetitionService _competitionService;
        private readonly IActivityBuilder _activityBuilder;

        public ActionEditDraftCompetitionHandler(ICompetitionService competitionService, IActivityBuilder activityBuilder)
        {
            _competitionService = competitionService;
            _activityBuilder = activityBuilder;
        }

        public async Task<TaskModuleTaskInfoResponse> Handle(Activity activity)
        {
            var invokeActionData = activity.GetInvokeActionData();
            var competition = await _competitionService.GetCompetition(invokeActionData.CompetitionId);
            var canEdit = competition.CreatorAadObjectId == activity.From.AadObjectId;
            if (canEdit)
            {
                return _activityBuilder.CreateDraftCompetitionEditTaskInfoResponse(competition, string.Empty, activity.Locale);
            }
            else
            {
                return _activityBuilder.CreateEditNotAllowedTaskInfoResponse(competition);
            }
        }
    }
}
