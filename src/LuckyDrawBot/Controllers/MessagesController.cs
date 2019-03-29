using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Microsoft.Extensions.Logging;

namespace LuckyDrawBot.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class MessagesController : ControllerBase
    {
        private readonly ILogger<MessagesController> _logger;

        public MessagesController(ILogger<MessagesController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("message")]
        [ProducesResponseType(typeof(Activity), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMessage([FromBody]Activity activity)
        {
            _logger.LogWarning($"Type:{activity.Type} Action:{activity.Action} ValueType:{activity.ValueType} Value:{activity.Value}");

            MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl, DateTime.Now.AddDays(7));

            var channelData = activity.GetChannelData<TeamsChannelData>();
            var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl), "20128cb3-5809-4b4f-a32d-b9929e67238c", ".l!c}F4xt8=*{%x{I6wZ7Ek");

            var rootActivity = Activity.CreateMessageActivity() as Activity;
            rootActivity.From = new ChannelAccount("20128cb3-5809-4b4f-a32d-b9929e67238c", "a name");
            rootActivity.Conversation = new ConversationAccount(id: channelData.Channel.Id);
            rootActivity.Attachments = new List<Attachment>
            {
                new Attachment
                {
                    ContentType = HeroCard.ContentType,
                    Content = new HeroCard()
                    {
                        Title = "Yummy Steak",
                        Subtitle = "",
                        Text = "",
                        Images = new List<CardImage>()
                        {
                            new CardImage()
                            {
                                Url = "https://github.com/tony-xia/microsoft-teams-templates/raw/master/images/steak.jpg"
                            }
                        }
                    }
                }
            };
            var rootMessage = await connectorClient.Conversations.SendToConversationAsync((Activity)rootActivity);

            await UpdateActivity("1 person", connectorClient, channelData.Channel.Id, rootMessage.Id);
            await UpdateActivity("5 persons", connectorClient, channelData.Channel.Id, rootMessage.Id);
            await UpdateActivity("100 persons", connectorClient, channelData.Channel.Id, rootMessage.Id);

            await Task.Delay(3000);
            var resultActivity = Activity.CreateMessageActivity() as Activity;
            resultActivity.From = new ChannelAccount("20128cb3-5809-4b4f-a32d-b9929e67238c", "a name");
            resultActivity.Conversation = new ConversationAccount(id: channelData.Channel.Id);
            resultActivity.Attachments = new List<Attachment>
            {
                new Attachment
                {
                    ContentType = HeroCard.ContentType,
                    Content = new HeroCard()
                    {
                        Title = "Our winners are:",
                        Subtitle = "Ares & Tony",
                        Images = new List<CardImage>()
                        {
                            new CardImage()
                            {
                                Url = "https://serverpress.com/wp-content/uploads/2015/12/congrats-gif-2.gif"
                            }
                        },
                        Buttons = new List<CardAction>
                        {
                            new CardAction
                            {
                                Title = "title",
                                Text = "text",
                                Type = "invoke",
                                Value = "{\"abc\":1}"
                            }
                        }
                    }
                }
            };
            var resultMessage = await connectorClient.Conversations.SendToConversationAsync((Activity)resultActivity);
            return Ok();
        }


        private async Task UpdateActivity(string text, ConnectorClient connectorClient, string channelId, string activityId)
        {
            await Task.Delay(1500);
            var updatedActivity = Activity.CreateMessageActivity() as Activity;
            updatedActivity.Attachments = new List<Attachment>
            {
                new Attachment
                {
                    ContentType = HeroCard.ContentType,
                    Content = new HeroCard()
                    {
                        Title = "Yummy Steak - " + text,
                        Subtitle = "",
                        Text = "",
                        Images = new List<CardImage>()
                        {
                            new CardImage()
                            {
                                Url = "https://github.com/tony-xia/microsoft-teams-templates/raw/master/images/steak.jpg"
                            }
                        }
                    }
                }
            };
            await connectorClient.Conversations.UpdateActivityAsync(channelId, activityId, updatedActivity);
        }

    }
}
