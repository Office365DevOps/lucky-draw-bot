using LuckyDrawBot.Controllers;
using LuckyDrawBot.Services;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

        public async Task<object> Handle(Activity activity)
        {
            var data = ((JObject)activity.Value).GetValue("data");
            var editForm = JsonSerializer.Deserialize<CompetitionEditForm>(Newtonsoft.Json.JsonConvert.SerializeObject(data));
            var plannedDrawTime = editForm.GetPlannedDrawTime(activity.GetOffset(), _dateTimeService.UtcNow.AddHours(2));
            var localPlannedDrawTime = plannedDrawTime.ToOffset(activity.GetOffset());

            var errorMessage = CanPreviewCompetition(editForm.Gift, int.Parse(editForm.WinnerCount), plannedDrawTime, editForm.GiftImageUrl, activity.Locale);
            if (string.IsNullOrEmpty(errorMessage))
            {
                var card = _activityBuilder.CreatePreviewCard(editForm, localPlannedDrawTime, activity.Locale);
                var preview = MessageFactory.Attachment(card) as Activity;
                preview.Value = editForm;
                var response = new MessagingExtensionActionResponse
                {
                    ComposeExtension = new MessagingExtensionResult
                    {
                        Type = "botMessagePreview",
                        ActivityPreview = preview
                    }
                };
                return await Task.FromResult(response);
            }
            else
            {
                var card = _activityBuilder.CreateComposeEditForm(editForm.Gift, int.Parse(editForm.WinnerCount), editForm.GiftImageUrl, localPlannedDrawTime, errorMessage, activity.Locale);
                var taskInfo = new TaskModuleContinueResponse
                {
                    Type = "continue",
                    Value = new Microsoft.Bot.Schema.Teams.TaskModuleTaskInfo
                    {
                        Title = string.Empty,
                        Width = "medium",
                        Card = card
                    }
                };
                var response = new MessagingExtensionActionResponse { Task = taskInfo };
                return await Task.FromResult(response);
            }
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
