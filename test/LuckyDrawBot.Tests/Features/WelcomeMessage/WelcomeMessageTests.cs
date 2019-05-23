using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using LuckyDrawBot.Tests.Infrastructure;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Xunit;
using Xunit.Abstractions;

namespace LuckyDrawBot.Tests.Features.WelcomeMessage
{
    public class WelcomeMessageTests : BaseTest
    {
        public WelcomeMessageTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WhenEverythingIsGood_BotIsAddedIntoChannel_ReplyWelcomeMessage()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var botChannelAccountId = Guid.NewGuid().ToString();
                var activity = new Activity
                {
                    ServiceUrl = "https://service-url.com",
                    ChannelId = "msteams",
                    Type = ActivityTypes.ConversationUpdate,
                    Locale = "en-us",
                    From = new ChannelAccount("id", "name"),
                    Recipient = new ChannelAccount(botChannelAccountId, "bot name"),
                    MembersAdded = new List<ChannelAccount> { new ChannelAccount { Id = botChannelAccountId }},
                    Conversation = new ConversationAccount(isGroup: true, id: "conv id", name: "conv name"),
                    ChannelData = new TeamsChannelData
                    {
                        Tenant = new TenantInfo { Id = Guid.NewGuid().ToString() },
                        Team = new TeamInfo { Id = Guid.NewGuid().ToString() },
                        Channel = new ChannelInfo { Id = Guid.NewGuid().ToString() },
                    }
                };
                var response = await client.SendActivity(activity);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var createdMessages = server.Assert().GetCreatedMessages();
                createdMessages.Should().HaveCount(1);
                createdMessages[0].Activity.Text.Should().StartWith("Hi there, I'm LuckyDraw bot");
            }
        }
    }
}
