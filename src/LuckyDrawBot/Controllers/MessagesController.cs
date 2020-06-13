using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using LuckyDrawBot.Handlers;
using LuckyDrawBot.Models;
using LuckyDrawBot.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace LuckyDrawBot.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class MessagesController : ControllerBase
    {
        private readonly ILogger<MessagesController> _logger;
        private readonly IBotValidator _botValidator;
        private readonly BotHandlers _handlers;

        public MessagesController(
            ILogger<MessagesController> logger,
            IBotValidator botValidator,
            BotHandlers handlers)
        {
            _logger = logger;
            _botValidator = botValidator;
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
                InvokeActionData invokeActionData = activity.GetInvokeActionData();

                switch (invokeActionData.UserAction)
                {
                    case InvokeActionType.Join:
                        await _handlers.ActionJoinCompetition.Handle(activity);
                        return Ok();
                    case InvokeActionType.ViewDetail:
                        var competitionDetailResponse = await _handlers.ActionViewCompetitionDetail.Handle(activity);
                        return OkWithNewtonsoftJson(competitionDetailResponse);
                    case InvokeActionType.EditDraft:
                        var editDraftCompetitionResponse = await _handlers.ActionEditDraftCompetition.Handle(activity);
                        return OkWithNewtonsoftJson(editDraftCompetitionResponse);
                    case InvokeActionType.SaveDraft:
                        await _handlers.ActionSaveDraftCompetition.Handle(activity);
                        return Ok();
                    case InvokeActionType.ActivateCompetition:
                        await _handlers.ActionSaveDraftCompetition.Handle(activity);
                        var activateCompetitionResponse = await _handlers.ActionActivateCompetition.Handle(activity, this.Url);
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
                    await _handlers.CommandHelp.Handle(activity);
                    return Ok();
                }

                if (text.Equals("start", StringComparison.InvariantCultureIgnoreCase))
                {
                    await _handlers.CommandCreateDraftCompetition.Handle(activity);
                    return Ok();
                }

                if (_handlers.CommandCreateCompetition.CanHandle(activity))
                {
                    await _handlers.CommandCreateCompetition.Handle(activity, this.Url);
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
