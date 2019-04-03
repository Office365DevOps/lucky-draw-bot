using System;
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
        private class HealthCheckResponse
        {
            public string Status { get; set; }
        }

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
                };

                var response = await client.SendActivity(activity);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
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
