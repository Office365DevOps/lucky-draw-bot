using AdaptiveCards;
using LuckyDrawBot.Controllers;
using LuckyDrawBot.Services;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;

namespace LuckyDrawBot.Handlers
{
    public class ComposeEditAgainHandler
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IActivityBuilder _activityBuilder;

        public ComposeEditAgainHandler(IDateTimeService dateTimeService, IActivityBuilder activityBuilder)
        {
            _dateTimeService = dateTimeService;
            _activityBuilder = activityBuilder;
        }

        public async Task<MessagingExtensionActionResponse> Handle(Activity activity)
        {
            var editForm = GetEditForm(activity);

            var localPlannedDrawTime = _dateTimeService.UtcNow.AddHours(2).ToOffset(activity.GetOffset());
            if (!string.IsNullOrEmpty(editForm.PlannedDrawTimeLocalDate) && !string.IsNullOrEmpty(editForm.PlannedDrawTimeLocalTime))
            {
                var dateTime = DateTime.Parse(editForm.PlannedDrawTimeLocalDate + "T" + editForm.PlannedDrawTimeLocalTime, CultureInfo.InvariantCulture);
                localPlannedDrawTime = new DateTimeOffset(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0, activity.GetOffset());
            }

            var card = _activityBuilder.CreateComposeEditForm(editForm.Gift, int.Parse(editForm.WinnerCount), editForm.GiftImageUrl, localPlannedDrawTime, string.Empty, activity.Locale);
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

        private CompetitionEditForm GetEditForm(Activity activity)
        {
            var value = (JObject)activity.Value;
            var previewActivities = value.GetValue("botActivityPreview").ToObject<Activity[]>();
            var previewCard = ((JObject)previewActivities[0].Attachments[0].Content).ToObject<AdaptiveCard>();
            var luckyDrawDataTextBlock = previewCard.Body.Find(x => x.Id == "LuckyDrawData") as AdaptiveTextBlock;
            var editFormJson = luckyDrawDataTextBlock.Text;
            var editForm = JsonSerializer.Deserialize<CompetitionEditForm>(editFormJson);
            return editForm;
        }
    }
}
