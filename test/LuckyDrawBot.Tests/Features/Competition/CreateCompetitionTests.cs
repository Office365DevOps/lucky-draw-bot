using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using LuckyDrawBot.Tests.Infrastructure;
using Microsoft.Bot.Schema;
using Xunit;
using Xunit.Abstractions;

namespace LuckyDrawBot.Tests.Features.Competition
{
    public class CreateCompetitionTests : BaseTest
    {
        public CreateCompetitionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WhenSpecifyOnlyGiftNameAndCount_SendTextToBot_CompetitionIsCreatedAndPosted()
        {
            var giftName = "a gift name";
            var winnerCount = 6;

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var text = $"<at>bot name</at>{giftName}, {winnerCount}";
                var response = await client.SendTeamsText(text);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var createdMessages = server.Assert().GetCreatedMessages();
                createdMessages.Should().HaveCount(1);
                var heroCard = createdMessages[0].Activity.Attachments[0].Content as HeroCard;
                heroCard.Title.Should().StartWith(giftName);
                var openCompetitions = server.Assert().GetOpenCompetitions();
                openCompetitions.Should().HaveCount(1);
                openCompetitions[0].Gift.Should().StartWith(giftName);
                openCompetitions[0].WinnerCount.Should().Be(winnerCount);
            }
        }
    }
}
