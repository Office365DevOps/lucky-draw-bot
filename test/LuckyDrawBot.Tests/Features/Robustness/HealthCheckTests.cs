using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using LuckyDrawBot.Tests.Infrastructure;
using Xunit;
using Xunit.Abstractions;

namespace LuckyDrawBot.Tests.Features.HealthCheck
{
    public class HealthCheckTests : BaseTest
    {
        private class HealthCheckResponse
        {
            public string Status { get; set; }
        }

        public HealthCheckTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WhenEverythingWorks_GetHealthCheck_ReturnHealthy()
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var response = await client.GetAsync("healthcheck");

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<HealthCheckResponse>();
                result.Status.Should().Be("Healthy");
            }
        }
    }
}
