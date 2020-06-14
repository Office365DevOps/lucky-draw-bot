using LuckyDrawBot.Services;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using System;
using System.Threading.Tasks;

namespace LuckyDrawBot.Handlers
{
    public class ComposeStartFormHandler
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IActivityBuilder _activityBuilder;

        public ComposeStartFormHandler(IDateTimeService dateTimeService, IActivityBuilder activityBuilder)
        {
            _dateTimeService = dateTimeService;
            _activityBuilder = activityBuilder;
        }

        public async Task<MessagingExtensionActionResponse> Handle(Activity activity)
        {
            var localPlannedDrawTime = _dateTimeService.UtcNow.AddHours(2).ToOffset(TimeSpan.FromHours(activity.GetOffset().TotalHours));
            var card = _activityBuilder.CreateComposeEditForm(string.Empty, 1, string.Empty, localPlannedDrawTime, string.Empty, activity.Locale);
            var taskInfo = new TaskModuleContinueResponse
            {
                Type = "continue",
                Value = new TaskModuleTaskInfo
                {
                    Title = string.Empty,
                    Card = card
                }
            };

            var response = new MessagingExtensionActionResponse { Task = taskInfo };
            return await Task.FromResult(response);
        }
    }
}
