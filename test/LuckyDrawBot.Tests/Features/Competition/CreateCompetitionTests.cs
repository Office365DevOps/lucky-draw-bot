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
        public async Task WhenSpecifyAllParameters_SendTextToBot_CompetitionIsCreatedAndPosted()
        {
            var giftName = "a gift name";
            var winnerCount = 6;
            var plannedDrawTime = "2019-1-9 14:32";
            var giftImageUrl = "http://jpg.com/01.jpg";

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var text = $"<at>bot name</at>{giftName},{winnerCount},{plannedDrawTime},{giftImageUrl}";
                var response = await client.SendTeamsText(text);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var createdMessages = server.Assert().GetCreatedMessages();
                createdMessages.Should().HaveCount(1);
                var heroCard = createdMessages[0].Activity.Attachments[0].Content as HeroCard;
                heroCard.Title.Should().StartWith(giftName);
                heroCard.Images.Should().HaveCount(1);
                heroCard.Images[0].Url.Should().Be(giftImageUrl);
                var openCompetitions = server.Assert().GetOpenCompetitions();
                openCompetitions.Should().HaveCount(1);
                openCompetitions[0].Gift.Should().StartWith(giftName);
                openCompetitions[0].WinnerCount.Should().Be(winnerCount);
                openCompetitions[0].PlannedDrawTime.Should().Be(DateTimeOffset.Parse(plannedDrawTime));
                openCompetitions[0].GiftImageUrl.Should().Be(giftImageUrl);
            }
        }
    }
}
