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
    public class JoinCompetitionTests : BaseTest
    {
        public JoinCompetitionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WhenCompetitionIsNotCompleted_JoinCompetition_SucceedToJoin()
        {
            var utcNow = DateTimeOffset.UtcNow;
            var competition = new OpenCompetitionEntity(Guid.NewGuid())
            {
                MainActivityId = "main activity id",
                PlannedDrawTime = DateTimeOffset.UtcNow.AddMinutes(-1),
                Locale = "en-US",
                OffsetHours = 8,
                Gift = "gift name",
                IsCompleted = false,
                Competitors = new List<Competitor>()
            };
            var userAccount = new ChannelAccount { Name = "user name", AadObjectId = "user aad object id" };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var arrangement = server.Arrange();
                arrangement.SetUtcNow(utcNow);
                await arrangement.GetOpenCompetitions().InsertOrReplace(competition);

                var response = await client.SendTeamsInvoke(new InvokeActionData { UserAction = InvokeActionType.Join, CompetitionId = competition.Id }, userAccount);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var updatedMessages = server.Assert().GetUpdatedMessages();
                updatedMessages.Should().HaveCount(1);
                updatedMessages[0].ReplacedActivityId.Should().Be(competition.MainActivityId);
                var heroCard = updatedMessages[0].NewActivity.Attachments[0].Content as HeroCard;
                heroCard.Text.Should().Contain(userAccount.Name);
                var openCompetitions = server.Assert().GetOpenCompetitions();
                openCompetitions.Should().HaveCount(1);
                openCompetitions[0].Competitors.Should().HaveCount(1);
                openCompetitions[0].Competitors[0].Name.Should().Be(userAccount.Name);
                openCompetitions[0].Competitors[0].AadObjectId.Should().Be(userAccount.AadObjectId);
                openCompetitions[0].Competitors[0].JoinTime.Should().Be(utcNow);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public async Task WhenCompetitionHasAlreadyHadSomeCompetitions_JoinCompetition_SucceedToJoin(int existingCompetitorCount)
        {
            var utcNow = DateTimeOffset.UtcNow;
            var competition = new OpenCompetitionEntity(Guid.NewGuid())
            {
                MainActivityId = "main activity id",
                PlannedDrawTime = DateTimeOffset.UtcNow.AddMinutes(-1),
                Locale = "en-US",
                OffsetHours = 8,
                Gift = "gift name",
                IsCompleted = false,
                Competitors = new List<Competitor>()
            };
            for (int i = 0; i < existingCompetitorCount; i++)
            {
                competition.Competitors.Add(new Competitor
                {
                    Name = $"existing user name {i}",
                    AadObjectId = $"existing user aad object id {i}",
                    JoinTime = utcNow.AddSeconds(-i)
                });
            }
            var userAccount = new ChannelAccount { Name = "new user name", AadObjectId = "new user aad object id" };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var arrangement = server.Arrange();
                arrangement.SetUtcNow(utcNow);
                await arrangement.GetOpenCompetitions().InsertOrReplace(competition);

                var text = $"<at>bot name</at>" + "wrongFormatText";
                var response = await client.SendTeamsInvoke(new InvokeActionData { UserAction = InvokeActionType.Join, CompetitionId = competition.Id }, userAccount);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var updatedMessages = server.Assert().GetUpdatedMessages();
                updatedMessages.Should().HaveCount(1);
                var openCompetitions = server.Assert().GetOpenCompetitions();
                openCompetitions.Should().HaveCount(1);
                openCompetitions[0].Competitors.Should().HaveCount(existingCompetitorCount + 1);
                var lastCompetitor = openCompetitions[0].Competitors.Last();
                lastCompetitor.Name.Should().Be(userAccount.Name);
                lastCompetitor.AadObjectId.Should().Be(userAccount.AadObjectId);
                lastCompetitor.JoinTime.Should().Be(utcNow);
            }
        }
    }
}
