using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;

namespace LuckyDrawBot.Controllers
{
    [ApiController]
    [Produces("application/json")]
    public class MessagesController : ControllerBase
    {
        public MessagesController()
        {
        }

        [HttpPost]
        [Route("message")]
        [ProducesResponseType(typeof(Activity), (int)HttpStatusCode.OK)]
        public async Task<IActionResult> GetMessage([FromBody]Activity activity)
        {
            MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl, DateTime.Now.AddDays(7));
            var connector = new ConnectorClient(new Uri(activity.ServiceUrl), "20128cb3-5809-4b4f-a32d-b9929e67238c", ".l!c}F4xt8=*{%x{I6wZ7Ek");
            var reply = activity.CreateReply();
            reply.Attachments = new List<Attachment>
            {
                new Attachment
                {
                    ContentType = HeroCard.ContentType,
                    Content = new HeroCard()
                    {
                        Title = "Superhero",
                        Subtitle = "An incredible hero",
                        Text = "Microsoft Teams",
                        Images = new List<CardImage>()
                        {
                            new CardImage()
                            {
                                Url = "https://github.com/tony-xia/microsoft-teams-templates/raw/master/images/steak.jpg"
                            }
                        },
                        Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Type = "openUrl",
                                Title = "Visit",
                                Value = "http://www.microsoft.com"
                            }
                        }
                    }
                }
            };
            //var msgToUpdate = await connector.Conversations.SendToConversationAsync(reply);
            var msgToUpdate = await connector.Conversations.ReplyToActivityAsync(reply);
            await Task.Delay(2000);
            Activity updatedReply = activity.CreateReply();
            updatedReply.Attachments = new List<Attachment>
            {
                new Attachment
                {
                    ContentType = HeroCard.ContentType,
                    Content = new HeroCard()
                    {
                        Title = "This is an updated message",
                        Subtitle = "An incredible hero",
                        Text = "Microsoft Teams",
                        Images = new List<CardImage>()
                        {
                            new CardImage()
                            {
                                Url = "https://github.com/tony-xia/microsoft-teams-templates/raw/master/images/cbd_after_sunset.jpg"
                            }
                        },
                        Buttons = new List<CardAction>()
                        {
                            new CardAction()
                            {
                                Type = "openUrl",
                                Title = "Visit",
                                Value = "http://www.microsoft.com"
                            }
                        }
                    }
                }
            };
            await connector.Conversations.UpdateActivityAsync(reply.Conversation.Id, msgToUpdate.Id, updatedReply);

            // var userId = activity.From.Id;
            // var botId = activity.Recipient.Id;
            // var botName = activity.Recipient.Name;
            // var channelData = activity.GetChannelData<TeamsChannelData>();
            // var connectorClient = new ConnectorClient(new Uri(activity.ServiceUrl), "20128cb3-5809-4b4f-a32d-b9929e67238c", ".l!c}F4xt8=*{%x{I6wZ7Ek");
            // var parameters = new ConversationParameters
            // {
            //     Bot = new ChannelAccount(botId, botName),
            //     Members = new ChannelAccount[] { new ChannelAccount(userId) },
            //     ChannelData = new TeamsChannelData
            //     {
            //         Tenant = channelData.Tenant,
            //         Team = channelData.Team,
            //         Channel = channelData.Channel,
            //     }
            // };
            // var conversationResource = await connectorClient.Conversations.CreateConversationAsync(parameters);
            // var message = Activity.CreateMessageActivity();
            // message.From = new ChannelAccount(botId, botName);
            // message.Conversation = new ConversationAccount(id: conversationResource.Id.ToString());
            // message.Text = "new start";
            // await connectorClient.Conversations.SendToConversationAsync((Activity)message);
            return Ok();
        }

    }
}
