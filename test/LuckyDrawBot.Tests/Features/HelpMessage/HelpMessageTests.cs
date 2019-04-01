using System;
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
        private class HealthCheckResponse
        {
            public string Status { get; set; }
        }

        public HelpMessageTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WhenEverythingIsGood_SendTextHelp_ReplyHelpMessage()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var response = await client.SendTeamsText("help");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var createdMessages = server.Assert().GetCreatedMessages();
                createdMessages.Should().HaveCount(1);
                createdMessages[0].Activity.Text.Should().StartWith("Hi there");
            }
        }
    }
}
