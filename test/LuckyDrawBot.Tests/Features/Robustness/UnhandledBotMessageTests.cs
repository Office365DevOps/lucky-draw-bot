using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using LuckyDrawBot.Tests.Infrastructure;
using Microsoft.Bot.Schema;
using Xunit;
using Xunit.Abstractions;

namespace LuckyDrawBot.Tests.Features.HealthCheck
{
    public class UnhandledBotMessageTests : BaseTest
    {
        public UnhandledBotMessageTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WhenMessageComesFromNonMSTeamsChannel_ProcessMessage_ReturnOk()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var activity = new Activity
                {
                    ChannelId = "non msteams",
                    From = new ChannelAccount("id", "name"),
                    Conversation = new ConversationAccount(isGroup: true, id: "conv id", name: "conv name"),
                };

                var response = await client.SendActivity(activity);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var createdMessages = server.Assert().GetCreatedMessages();
                createdMessages.Should().HaveCount(1);
                createdMessages[0].Activity.Text.Should().StartWith("Sorry");
            }
        }

        [Fact]
        public async Task WhenIncomingActivityTypeIsNotMessageOrInvoke_ProcessMessage_ReturnOk()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var activity = new Activity
                {
                    ChannelId = "msteams",
                    Type = "not a message"
                };

                var response = await client.SendActivity(activity);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
            }
        }
    }
}
