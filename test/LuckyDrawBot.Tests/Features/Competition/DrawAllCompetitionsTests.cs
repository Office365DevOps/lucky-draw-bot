using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using LuckyDrawBot.Models;
using LuckyDrawBot.Tests.Infrastructure;
using Microsoft.Bot.Schema;
using Xunit;
using Xunit.Abstractions;
using static LuckyDrawBot.Services.CompetitionRepositoryService;

namespace LuckyDrawBot.Tests.Features.Competition
{
    public class DrawAllCompetitionsTests : BaseTest
    {
        public DrawAllCompetitionsTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WhenThereAreTwoCompetitions_DrawAllCompetitions_AllCompetitionsAreCompleted()
        {
            var competition1 = new OpenCompetitionEntity(Guid.NewGuid())
            {
                MainActivityId = "main activity id 1",
                Locale = "en-US",
                OffsetHours = 8,
                Gift = "gift name 1",
                Status = CompetitionStatus.Active,
                Competitors = new List<Competitor>(),
                WinnerCount = 1,
                WinnerAadObjectIds = new List<string>()
            };
            var competition2 = new OpenCompetitionEntity(Guid.NewGuid())
            {
                MainActivityId = "main activity id 2",
                Locale = "en-US",
                OffsetHours = 8,
                Gift = "gift name 2",
                Status = CompetitionStatus.Active,
                Competitors = new List<Competitor>(),
                WinnerCount = 2,
                WinnerAadObjectIds = new List<string>()
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var openCompetitionsArrangement = server.Arrange().GetOpenCompetitions();
                await openCompetitionsArrangement.InsertOrReplace(competition1);
                await openCompetitionsArrangement.InsertOrReplace(competition2);

                var response = await client.PostAsync($"competitions/draw", new StringContent(string.Empty));

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var assertion = server.Assert();
                assertion.GetUpdatedMessages().Should().HaveCount(2);
                assertion.GetCreatedMessages().Should().HaveCount(2);
                assertion.GetOpenCompetitions().Should().HaveCount(0);
                assertion.GetClosedCompetitions().Should().HaveCount(2);
            }
        }

    }
}
