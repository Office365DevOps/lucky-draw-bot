using LuckyDrawBot.Models;
using LuckyDrawBot.Services;
using Microsoft.Bot.Schema;
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

        public async Task<TaskModuleTaskInfoResponse> Handle(Activity activity)
        {
            var localPlannedDrawTime = _dateTimeService.UtcNow.AddHours(2).ToOffset(TimeSpan.FromHours(activity.GetOffset().TotalHours));
            var card = _activityBuilder.CreateComposeEditForm(string.Empty, 1, string.Empty, localPlannedDrawTime, string.Empty, activity.Locale);
            var taskInfo = new TaskModuleTaskInfo
            {
                Type = "continue",
                Value = new TaskModuleTaskInfo.TaskInfoValue
                {
                    Title = string.Empty,
                    Card = card
                }
            };

            var response = new TaskModuleTaskInfoResponse { Task = taskInfo };
            return await Task.FromResult(response);
        }
    }
}
