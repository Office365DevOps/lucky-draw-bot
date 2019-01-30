using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;

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
            var reply = activity.CreateReply("abc");
            await connector.Conversations.ReplyToActivityAsync(reply);

            return Ok();
        }

    }
}
