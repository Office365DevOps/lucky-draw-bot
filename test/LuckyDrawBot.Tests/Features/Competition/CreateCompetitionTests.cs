using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using FluentAssertions;
using LuckyDrawBot.Models;
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
            var plannedDrawTime = "2029-1-9 14:32";
            var giftImageUrl = "http://jpg.com/01.jpg";

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var text = $"<at>bot name</at>{giftName},{winnerCount},{plannedDrawTime},{giftImageUrl}";
                var response = await client.SendTeamsText(text);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var openCompetitions = server.Assert().GetOpenCompetitions();
                openCompetitions.Should().HaveCount(1);
                openCompetitions[0].Status.Should().Be(CompetitionStatus.Active);
                openCompetitions[0].Gift.Should().StartWith(giftName);
                openCompetitions[0].WinnerCount.Should().Be(winnerCount);
                openCompetitions[0].PlannedDrawTime.Should().Be(DateTimeOffset.Parse(plannedDrawTime + "Z"));
                openCompetitions[0].GiftImageUrl.Should().Be(giftImageUrl);
                var createdMessages = server.Assert().GetCreatedMessages();
                createdMessages.Should().HaveCount(1);
                var heroCard = createdMessages[0].Activity.Attachments[0].Content as HeroCard;
                heroCard.Title.Should().StartWith(giftName);
                heroCard.Images.Should().HaveCount(1);
                heroCard.Images[0].Url.Should().Be(giftImageUrl);
                heroCard.Buttons.Should().HaveCount(2);
                heroCard.Buttons[0].Type.Should().Be("invoke");
                heroCard.Buttons[0].Value.Should().BeEquivalentTo(new InvokeActionData { UserAction = InvokeActionType.Join, CompetitionId = openCompetitions[0].Id });
                heroCard.Buttons[1].Type.Should().Be("invoke");
                heroCard.Buttons[1].Value.Should().BeEquivalentTo(new InvokeActionData { Type = InvokeActionData.TypeTaskFetch, UserAction = InvokeActionType.ViewDetail, CompetitionId = openCompetitions[0].Id });
            }
        }

        [Fact]
        public async Task WhenSpecifyOnlyGiftNameAndWinnerCount_SendTextToBot_DefaultValuesAreUsedForOtherParameters()
        {
            var utcNow = DateTimeOffset.Parse("2019-02-03T13:22:33Z");
            var giftName = "a gift name";
            var winnerCount = 6;
            var defaultPlannedDrawTime = utcNow.AddMinutes(1);
            var defaultGiftImageUrl = string.Empty;

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                server.Arrange().SetUtcNow(utcNow);

                var text = $"<at>bot name</at>{giftName},{winnerCount}";
                var response = await client.SendTeamsText(text);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var openCompetitions = server.Assert().GetOpenCompetitions();
                openCompetitions.Should().HaveCount(1);
                openCompetitions[0].Gift.Should().StartWith(giftName);
                openCompetitions[0].WinnerCount.Should().Be(winnerCount);
                openCompetitions[0].PlannedDrawTime.Should().Be(defaultPlannedDrawTime);
                openCompetitions[0].GiftImageUrl.Should().Be(defaultGiftImageUrl);
            }
        }

        [Theory]
        [InlineData("2019-9-8", "en-us", 0, "2019-09-08T00:00:00Z")]
        [InlineData("2019-9-8", "en-us", 10, "2019-09-07T14:00:00Z")]
        [InlineData("2019-9-8", "en-us", -6, "2019-09-08T06:00:00Z")]
        [InlineData("2019-9-8 12:34:56", "en-us", 0, "2019-09-08T12:34:56Z")]
        [InlineData("5/6/2020", "en-us", 0, "2020-05-06T00:00:00Z")]
        [InlineData("5/6/2020", "en-au", 0, "2020-06-05T00:00:00Z")]
        public async Task WhenGivenDifferentPlannedDrawTime_SendTextToBot_PlannedDrawTimeIsParseCorrectly(
            string inputPlannedDrawTime,
            string locale,
            double offsetHours,
            string expectedPlannedDrawTime)
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                server.Arrange().SetUtcNow(new DateTimeOffset(2019, 8, 8, 0, 0, 0, TimeSpan.Zero));
                var text = $"<at>bot name</at>free coffee,1,{inputPlannedDrawTime}";
                var response = await client.SendTeamsText(text, locale: locale, offsetHours: offsetHours);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var openCompetitions = server.Assert().GetOpenCompetitions();
                openCompetitions.Should().HaveCount(1);
                openCompetitions[0].PlannedDrawTime.Should().Be(DateTimeOffset.Parse(expectedPlannedDrawTime));
            }
        }

        [Theory]
        [InlineData("8m", "2019-09-08T00:08:00Z")]
        [InlineData("8.25min", "2019-09-08T00:08:15Z")]
        [InlineData("2minutes", "2019-09-08T00:02:00Z")]
        [InlineData("3 分钟", "2019-09-08T00:03:00Z")]
        [InlineData("1.5h", "2019-09-08T01:30:00Z")]
        [InlineData("2.5 hrs", "2019-09-08T02:30:00Z")]
        [InlineData("4hours", "2019-09-08T04:00:00Z")]
        [InlineData("8小时", "2019-09-08T08:00:00Z")]
        public async Task WhenPlannedDrawTimeIsGivenAsDurationText_SendTextToBot_PlannedDrawTimeIsParseCorrectly(
            string inputPlannedDrawTime, string expectedPlannedDrawTime)
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                server.Arrange().SetUtcNow(DateTimeOffset.Parse("2019-09-08T00:00:00Z"));

                var text = $"<at>bot name</at>free coffee,1,{inputPlannedDrawTime}";
                var response = await client.SendTeamsText(text);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var openCompetitions = server.Assert().GetOpenCompetitions();
                openCompetitions.Should().HaveCount(1);
                openCompetitions[0].PlannedDrawTime.Should().Be(DateTimeOffset.Parse(expectedPlannedDrawTime));
            }
        }

        [Fact]
        public async Task WhenUseChineseSeparator_SendTextToBot_TextIsParsedCorrectly()
        {
            var giftName = "gift name";
            var winnerCount = 6;
            var plannedDrawTime = "2029-1-9 14:32";
            var giftImageUrl = "http://jpg.com/01.jpg";

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var text = $"<at>bot name</at> {giftName}，{winnerCount}，{plannedDrawTime}，{giftImageUrl}";
                var response = await client.SendTeamsText(text);

                var openCompetitions = server.Assert().GetOpenCompetitions();
                openCompetitions.Should().HaveCount(1);
                openCompetitions[0].Gift.Should().StartWith(giftName);
                openCompetitions[0].WinnerCount.Should().Be(winnerCount);
                openCompetitions[0].PlannedDrawTime.Should().Be(DateTimeOffset.Parse(plannedDrawTime + "Z"));
                openCompetitions[0].GiftImageUrl.Should().Be(giftImageUrl);
            }
        }

        [Theory]
        [InlineData("only one parameter")]
        [InlineData("giftName,wrong number")]
        [InlineData("giftName,1,wrong hours")]
        [InlineData("giftName,1,wrong minutes")]
        [InlineData("giftName,1,wrong date")]
        public async Task WhenTextDoesNotComplyWithRequiredFormat_SendTextToBot_ReplyInvalidCommandMessage(string wrongFormatText)
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var text = $"<at>bot name</at>" + wrongFormatText;
                var response = await client.SendTeamsText(text);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var createdMessages = server.Assert().GetCreatedMessages();
                createdMessages.Should().HaveCount(1);
                createdMessages[0].Activity.Text.Should().StartWith("I'm sorry");
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task WhenWinnerCountIsLessThanOne_SendTextToBot_ReplyErrorMessageAboutWinnerCount(int winnerCount)
        {
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var text = $"<at>bot name</at>giftName,{winnerCount}";
                var response = await client.SendTeamsText(text);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var createdMessages = server.Assert().GetCreatedMessages();
                createdMessages.Should().HaveCount(1);
                createdMessages[0].Activity.Text.Should().Contain("must be bigger than 0");
            }
        }

        [Fact]
        public async Task WhenPlannedDrawTimeIsNotFutureTime_SendTextToBot_ReplyErrorMessageAboutPlannedDrawTime()
        {
            var oneSecondAgo = DateTimeOffset.UtcNow.AddSeconds(-1);
            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var text = $"<at>bot name</at>giftName,1,{oneSecondAgo.ToString("yyyy-MM-dd HH:mm")}";
                var response = await client.SendTeamsText(text);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var createdMessages = server.Assert().GetCreatedMessages();
                createdMessages.Should().HaveCount(1);
                createdMessages[0].Activity.Text.Should().Contain("must be future time");
            }
        }
    }
}
