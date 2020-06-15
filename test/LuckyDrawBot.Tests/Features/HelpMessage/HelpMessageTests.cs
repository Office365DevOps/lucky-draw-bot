using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using LuckyDrawBot.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace LuckyDrawBot.Tests.Features.HelpMessage
{
    public class HelpMessageTests : BaseTest
    {
        public HelpMessageTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WhenEverythingIsGood_SendTextHelp_ReplyHelpMessage()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var response = await client.SendTeamsText("<at>bot name</at>help");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var createdMessages = server.Assert().GetCreatedMessages();
                createdMessages.Should().HaveCount(1);
                createdMessages[0].Activity.Text.Should().StartWith("Hi there, To start a lucky draw");
            }
        }
    }
}
