using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LuckyDrawBot.Handlers;
using LuckyDrawBot.Models;
using LuckyDrawBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LuckyDrawBot.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class MessagesController : ControllerBase
    {
        private readonly ILogger<MessagesController> _logger;
        private readonly IBotValidator _botValidator;
        private readonly IBotClientFactory _botClientFactory;
        private readonly ICompetitionService _competitionService;
        private readonly IActivityBuilder _activityBuilder;
        private readonly ITimerService _timerService;
        private readonly IDateTimeService _dateTimeService;
        private readonly ILocalizationFactory _localizationFactory;
        private readonly BotHandlers _handlers;

        public MessagesController(
            ILogger<MessagesController> logger,
            IBotValidator botValidator,
            IBotClientFactory botClientFactory,
            ICompetitionService competitionService,
            IActivityBuilder activityBuilder,
            ITimerService timerService,
            IDateTimeService dateTimeService,
            ILocalizationFactory localizationFactory,
            BotHandlers handlers)
        {
            _logger = logger;
            _botValidator = botValidator;
            _botClientFactory = botClientFactory;
            _competitionService = competitionService;
            _activityBuilder = activityBuilder;
            _timerService = timerService;
            _dateTimeService = dateTimeService;
            _localizationFactory = localizationFactory;
            _handlers = handlers;
        }

        [HttpPost("messages")]
        [ProducesResponseType((int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMessage()
        {
            // Get the incoming activity
            Activity activity;
            using (var streamReader = new StreamReader(Request.Body))
            {
                var bodyString = await streamReader.ReadToEndAsync();
                activity = JsonConvert.DeserializeObject<Activity>(bodyString);
            }

            _logger.LogInformation($"ChannelId:{activity.ChannelId} Type:{activity.Type} Action:{activity.Action} ValueType:{activity.ValueType} Value:{activity.Value}");
            _logger.LogInformation("Input activity: {activity}", JsonConvert.SerializeObject(activity));

            // Authenticate the request
            var (isAuthenticated, authenticationErrorMessage) = await _botValidator.Validate(Request);
            if (!isAuthenticated)
            {
                return Unauthorized(authenticationErrorMessage);
            }
            _logger.LogInformation($"Authentication check succeeded.");

            // Validate the channel of the incoming activity
            if (activity.ChannelId != "msteams")
            {
                await _handlers.NonTeamsChannel.Handle(activity);
                return Ok();
            }

            // Handle the activity
            if (activity.Type == ActivityTypes.Invoke)
            {
                InvokeActionData invokeActionData;
                var value = (JObject)activity.Value;
                if (value.ContainsKey("data"))
                {
                    var data = value.GetValue("data");
                    invokeActionData = JsonConvert.DeserializeObject<InvokeActionData>(JsonConvert.SerializeObject(data));
                }
                else
                {
                    invokeActionData = JsonConvert.DeserializeObject<InvokeActionData>(JsonConvert.SerializeObject(activity.Value));
                }

                switch (invokeActionData.UserAction)
                {
                    case InvokeActionType.Join:
                        await HandleJoinCompetitionAction(invokeActionData, activity);
                        return Ok();
                    case InvokeActionType.ViewDetail:
                        var competitionDetailResponse = await HandleViewCompetitionDetailAction(invokeActionData);
                        return OkWithNewtonsoftJson(competitionDetailResponse);
                    case InvokeActionType.EditDraft:
                        var editDraftCompetitionResponse = await HandleEditDraftCompetitionAction(invokeActionData, activity);
                        return OkWithNewtonsoftJson(editDraftCompetitionResponse);
                    case InvokeActionType.SaveDraft:
                        await HandleSaveDraftCompetitionAction(invokeActionData, activity);
                        return Ok();
                    case InvokeActionType.ActivateCompetition:
                        var activateCompetitionResponse = await HandleActivateCompetitionAction(invokeActionData, activity);
                        return OkWithNewtonsoftJson(activateCompetitionResponse);
                    default:
                        throw new Exception("Unknown invoke action type: " + invokeActionData.UserAction);
                }
            }
            else if (activity.Type == ActivityTypes.Message)
            {
                var text = activity.GetTrimmedText();
                if (text.Equals("help", StringComparison.InvariantCultureIgnoreCase))
                {
                    await _handlers.HelpCommand.Handle(activity);
                    return Ok();
                }

                if (text.Equals("start", StringComparison.InvariantCultureIgnoreCase))
                {
                    await _handlers.CreateDraftCompetition.Handle(activity);
                    return Ok();
                }

                if (_handlers.CreateCompetitionCommand.CanHandle(activity))
                {
                    await _handlers.CreateCompetitionCommand.Handle(activity, this.Url);
                }
                else
                {
                    await _handlers.UnknownCommand.Handle(activity);
                }

                return Ok();
            }
            else if (activity.Type == ActivityTypes.ConversationUpdate)
            {
                var botSelf = activity.Recipient.Id;
                if ((activity.MembersAdded != null)
                    && (activity.MembersAdded.Count > 0)
                    && (activity.MembersAdded[0].Id == botSelf))
                {
                    await _handlers.AddedToNewChannel.Handle(activity);
                    return Ok();
                }
            }

            return Ok();
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

        private async Task<TaskModuleTaskInfoResponse> HandleViewCompetitionDetailAction(InvokeActionData invokeActionData)
        {
            var competition = await _competitionService.GetCompetition(invokeActionData.CompetitionId);
            var taskInfoResponse = _activityBuilder.CreateCompetitionDetailTaskInfoResponse(competition);
            return taskInfoResponse;
        }

        private async Task<TaskModuleTaskInfoResponse> HandleEditDraftCompetitionAction(InvokeActionData invokeActionData, Activity activity)
        {
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

        private async Task HandleSaveDraftCompetitionAction(InvokeActionData invokeActionData, Activity activity)
        {
            var data = ((JObject)activity.Value).GetValue("data");
            var editForm = JsonConvert.DeserializeObject<CompetitionEditForm>(JsonConvert.SerializeObject(data));

            // Teams/AdaptiveCards BUG: https://github.com/Microsoft/AdaptiveCards/issues/2644
            // DateInput does not post back the value in non-English situation.
            // Workaround: use TextInput instead and validate user's input against "yyyy-MM-dd" format
            if (!DateTimeOffset.TryParseExact(editForm.PlannedDrawTimeLocalDate, "yyyy-MM-dd", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out DateTimeOffset dummy))
            {
                var existingCompetition = await _competitionService.GetCompetition(invokeActionData.CompetitionId);
                editForm.PlannedDrawTimeLocalDate = existingCompetition.PlannedDrawTime.ToString("yyyy-MM-dd");
            }

            var offset = activity.LocalTimestamp.HasValue ? activity.LocalTimestamp.Value.Offset : TimeSpan.Zero;
            var date = DateTimeOffset.Parse(editForm.PlannedDrawTimeLocalDate);
            var time = DateTimeOffset.Parse(editForm.PlannedDrawTimeLocalTime);
            var plannedDrawTime = new DateTimeOffset(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, 0, offset).ToUniversalTime();

            await _competitionService.UpdateGift(invokeActionData.CompetitionId, plannedDrawTime, editForm.Gift, editForm.GiftImageUrl, editForm.WinnerCount);
        }

        private async Task<TaskModuleTaskInfoResponse> HandleActivateCompetitionAction(InvokeActionData invokeActionData, Activity activity)
        {
            await HandleSaveDraftCompetitionAction(invokeActionData, activity);
            var competition = await _competitionService.GetCompetition(invokeActionData.CompetitionId);
            var errorMessage = CanActivateCompetition(competition);
            if (!string.IsNullOrEmpty(errorMessage))
            {
                return _activityBuilder.CreateDraftCompetitionEditTaskInfoResponse(competition, errorMessage, activity.Locale);
            }

            competition = await _competitionService.ActivateCompetition(competition.Id);

            var mainActivity = _activityBuilder.CreateMainActivity(competition);
            using (var botClient = _botClientFactory.CreateBotClient(activity.ServiceUrl))
            {
                await botClient.UpdateActivityAsync(competition.ChannelId, competition.MainActivityId, mainActivity);
            }

            await _timerService.AddScheduledHttpRequest(
                competition.PlannedDrawTime,
                "POST",
                Url.Action(nameof(CompetitionsController.DrawForCompetition), "Competitions", new { competitionId = competition.Id }));
            return null;
        }

        private string CanActivateCompetition(Competition competition)
        {
            var localization = _localizationFactory.Create(competition.Locale);
            var errors = new List<string>();
            if (string.IsNullOrEmpty(competition.Gift))
            {
                errors.Add(localization["EditCompetition.Form.Gift.Invalid"]);
            }
            if (competition.WinnerCount <= 0)
            {
                errors.Add(localization["EditCompetition.Form.WinnerCount.Invalid"]);
            }
            if (competition.PlannedDrawTime < _dateTimeService.UtcNow)
            {
                errors.Add(localization["EditCompetition.Form.PlannedDrawTime.Invalid"]);
            }
            var giftImageUrl = competition.GiftImageUrl;
            if (!string.IsNullOrEmpty(giftImageUrl)
                && !giftImageUrl.StartsWith("http://", StringComparison.InvariantCultureIgnoreCase)
                && !giftImageUrl.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
            {
                errors.Add(localization["EditCompetition.Form.GiftImageUrl.Invalid"]);
            }
            return string.Join(' ', errors);
        }

        private IActionResult OkWithNewtonsoftJson(object value)
        {
            if (value == null)
            {
                return NoContent();
            }

            var json = JsonConvert.SerializeObject(value);
            return Content(json, "application/json", Encoding.UTF8);
        }
    }
}
