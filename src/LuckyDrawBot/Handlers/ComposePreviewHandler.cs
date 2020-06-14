using LuckyDrawBot.Controllers;
using LuckyDrawBot.Models;
using LuckyDrawBot.Services;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;

namespace LuckyDrawBot.Handlers
{
    public class ComposePreviewHandler
    {
        private readonly IDateTimeService _dateTimeService;
        private readonly IActivityBuilder _activityBuilder;
        private readonly ILocalizationFactory _localizationFactory;

        public ComposePreviewHandler(IDateTimeService dateTimeService, IActivityBuilder activityBuilder, ILocalizationFactory localizationFactory)
        {
            _dateTimeService = dateTimeService;
            _activityBuilder = activityBuilder;
            _localizationFactory = localizationFactory;
        }

        public async Task<TaskModuleTaskInfoResponse> Handle(Activity activity)
        {
            var data = ((JObject)activity.Value).GetValue("data");
            var editForm = JsonSerializer.Deserialize<CompetitionEditForm>(Newtonsoft.Json.JsonConvert.SerializeObject(data));

            // Teams/AdaptiveCards BUG: https://github.com/Microsoft/AdaptiveCards/issues/2644
            // DateInput does not post back the value in non-English situation.
            // Workaround: use TextInput instead and validate user's input against "yyyy-MM-dd" format
            if (!DateTimeOffset.TryParseExact(editForm.PlannedDrawTimeLocalDate, "yyyy-MM-dd", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out DateTimeOffset dummy))
            {
                var defaultPlannedDrawTimeLocal = _dateTimeService.UtcNow.AddHours(2).ToOffset(TimeSpan.FromHours(activity.GetOffset().TotalHours));
                editForm.PlannedDrawTimeLocalDate = defaultPlannedDrawTimeLocal.ToString("yyyy-MM-dd");
            }

            var offset = activity.GetOffset();
            var date = DateTimeOffset.Parse(editForm.PlannedDrawTimeLocalDate);
            var time = DateTimeOffset.Parse(editForm.PlannedDrawTimeLocalTime);
            var plannedDrawTime = new DateTimeOffset(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, 0, offset).ToUniversalTime();
            var localPlannedDrawTime = plannedDrawTime.ToOffset(TimeSpan.FromHours(activity.GetOffset().TotalHours));

            var errorMessage = CanPreviewCompetition(editForm.Gift, int.Parse(editForm.WinnerCount), plannedDrawTime, editForm.GiftImageUrl, activity.Locale);
            var card = _activityBuilder.CreateComposeEditForm(editForm.Gift, int.Parse(editForm.WinnerCount), editForm.GiftImageUrl, localPlannedDrawTime, errorMessage, activity.Locale);
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

        private string CanPreviewCompetition(string gift, int winnerCount, DateTimeOffset plannedDrawTime, string giftImageUrl, string locale)
        {
            var localization = _localizationFactory.Create(locale);
            var errors = new List<string>();
            if (string.IsNullOrEmpty(gift))
            {
                errors.Add(localization["EditCompetition.Form.Gift.Invalid"]);
            }
            if (winnerCount <= 0)
            {
                errors.Add(localization["EditCompetition.Form.WinnerCount.Invalid"]);
            }
            if (plannedDrawTime < _dateTimeService.UtcNow)
            {
                errors.Add(localization["EditCompetition.Form.PlannedDrawTime.Invalid"]);
            }
            if (!string.IsNullOrEmpty(giftImageUrl)
                && !giftImageUrl.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                && !giftImageUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
            {
                errors.Add(localization["EditCompetition.Form.GiftImageUrl.Invalid"]);
            }
            return string.Join(' ', errors);
        }
    }
}
