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
    public class DrawCompetitionTests : BaseTest
    {
        public DrawCompetitionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WhenCompetitionHasOneCompetitor_DrawCompetition_CompetitorWinsGift()
        {
            var utcNow = DateTimeOffset.UtcNow;
            var competition = new OpenCompetitionEntity(Guid.NewGuid())
            {
                MainActivityId = "main activity id",
                Locale = "en-US",
                OffsetHours = 8,
                Gift = "gift name",
                IsCompleted = false,
                Competitors = new List<Competitor> { new Competitor { Name = "user name", AadObjectId = "user aad object id" } },
                WinnerCount = 1,
                WinnerAadObjectIds = new List<string>()
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var arrangement = server.Arrange();
                arrangement.SetUtcNow(utcNow);
                await arrangement.GetOpenCompetitions().InsertOrReplace(competition);

                var response = await client.PostAsync($"competitions/{competition.Id}/draw", new StringContent(string.Empty));

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                // The main activity should be updated
                var updatedMessages = server.Assert().GetUpdatedMessages();
                updatedMessages.Should().HaveCount(1);
                updatedMessages[0].ReplacedActivityId.Should().Be(competition.MainActivityId);
                var updatedMessageHeroCard = updatedMessages[0].NewActivity.Attachments[0].Content as HeroCard;
                updatedMessageHeroCard.Buttons.Should().HaveCount(1);
                // The result activity should be created
                var createdMessages = server.Assert().GetCreatedMessages();
                createdMessages.Should().HaveCount(1);
                var createdMessageHeroCard = createdMessages[0].Activity.Attachments[0].Content as HeroCard;
                createdMessageHeroCard.Title.Should().Contain(competition.Competitors[0].Name);
                // Competition data in database should be updated
                var openCompetitions = server.Assert().GetOpenCompetitions();
                openCompetitions.Should().HaveCount(0);
                var completedCompetitions = server.Assert().GetClosedCompetitions();
                completedCompetitions.Should().HaveCount(1);
                completedCompetitions[0].IsCompleted.Should().BeTrue();
                completedCompetitions[0].ActualDrawTime.Should().Be(utcNow);
                completedCompetitions[0].WinnerAadObjectIds.Should().HaveCount(1);
                completedCompetitions[0].WinnerAadObjectIds[0].Should().Be(competition.Competitors[0].AadObjectId);
            }
        }

        [Fact]
        public async Task WhenNoCompetitor_DrawCompetition_NoWinner()
        {
            var utcNow = DateTimeOffset.UtcNow;
            var competition = new OpenCompetitionEntity(Guid.NewGuid())
            {
                MainActivityId = "main activity id",
                Locale = "en-US",
                OffsetHours = 8,
                Gift = "gift name",
                IsCompleted = false,
                Competitors = new List<Competitor>(),
                WinnerCount = 1,
                WinnerAadObjectIds = new List<string>()
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var arrangement = server.Arrange();
                arrangement.SetUtcNow(utcNow);
                await arrangement.GetOpenCompetitions().InsertOrReplace(competition);

                var response = await client.PostAsync($"competitions/{competition.Id}/draw", new StringContent(string.Empty));

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var completedCompetitions = server.Assert().GetClosedCompetitions();
                completedCompetitions.Should().HaveCount(1);
                completedCompetitions[0].WinnerAadObjectIds.Should().HaveCount(0);
            }
        }
    }
}
