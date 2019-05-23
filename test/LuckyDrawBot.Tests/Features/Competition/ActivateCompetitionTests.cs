using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AdaptiveCards;
using FluentAssertions;
using LuckyDrawBot.Models;
using LuckyDrawBot.Tests.Infrastructure;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;
using static LuckyDrawBot.Services.CompetitionRepositoryService;

namespace LuckyDrawBot.Tests.Features.Competition
{
    public class ActivateCompetitionTests : BaseTest
    {
        public ActivateCompetitionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WhenAllCompetitionPropertiesAreValid_InvokeActivationAction_CompetitionIsActivated()
        {
            var competition = new OpenCompetitionEntity(Guid.NewGuid())
            {
                Locale = "en-US",
                Gift = string.Empty,
                Status = CompetitionStatus.Draft,
                Competitors = new List<Competitor>(),
                WinnerAadObjectIds = new List<string>()
            };
            var activateActionData = new ActivateCompetitionInvokeActionData
            {
                UserAction = InvokeActionType.ActivateCompetition,
                CompetitionId = competition.Id,
                Gift = "new gift name",
                GiftImageUrl = "http://www.abc.com/new.png",
                WinnerCount = 988,
                PlannedDrawTimeLocalDate = "2028-06-18",
                PlannedDrawTimeLocalTime = "18:28"
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var arrangement = server.Arrange();
                await arrangement.GetOpenCompetitions().InsertOrReplace(competition);

                var response = await client.SendTeamsTaskFetch(activateActionData);

                response.StatusCode.Should().Be(HttpStatusCode.NoContent);
                var openCompetitions = server.Assert().GetOpenCompetitions();
                openCompetitions.Should().HaveCount(1);
                openCompetitions[0].Status.Should().Be(CompetitionStatus.Active);
                openCompetitions[0].Gift.Should().Be(activateActionData.Gift);
                openCompetitions[0].GiftImageUrl.Should().Be(activateActionData.GiftImageUrl);
                openCompetitions[0].WinnerCount.Should().Be(activateActionData.WinnerCount);
                openCompetitions[0].PlannedDrawTime.Should().Be(DateTimeOffset.Parse($"{activateActionData.PlannedDrawTimeLocalDate}T{activateActionData.PlannedDrawTimeLocalTime}Z"));
                var updatedMessages = server.Assert().GetUpdatedMessages();
                updatedMessages.Should().HaveCount(1);
                var heroCard = updatedMessages[0].NewActivity.Attachments[0].Content as HeroCard;
                heroCard.Buttons.Should().HaveCount(2);
                heroCard.Buttons[0].Type.Should().Be("invoke");
                heroCard.Buttons[0].Value.Should().BeEquivalentTo(new InvokeActionData { UserAction = InvokeActionType.Join, CompetitionId = openCompetitions[0].Id });
                heroCard.Buttons[1].Type.Should().Be("invoke");
                heroCard.Buttons[1].Value.Should().BeEquivalentTo(new InvokeActionData { Type = InvokeActionData.TypeTaskFetch, UserAction = InvokeActionType.ViewDetail, CompetitionId = openCompetitions[0].Id });
            }
        }

        [Theory]
        [InlineData("", 10, "2028-06-18", "18:28", "http://www.abc.com/new.png", "You must specify the prize.")]
        [InlineData("giftname", 0, "2028-06-18", "18:28", "http://www.abc.com/new.png", "The number of prizes must be bigger than 0.")]
        [InlineData("giftname", 1, "2008-01-10", "18:28", "http://www.abc.com/new.png", "The draw time must be future time.")]
        [InlineData("giftname", 1, "2028-06-18", "18:28", "abc", "The URL of prize image must start with 'http://' or 'https://'.")]
        [InlineData("", 0, "2028-06-18", "18:28", "http://www.abc.com/new.png", "You must specify the prize. The number of prizes must be bigger than 0.")]
        public async Task WhenPropertyIsInvalid_InvokeActivationAction_ErrorMessageIsReturned(
            string gift,
            int winnerCount,
            string plannedDrawLocalDate,
            string plannedDrawLocalTime,
            string giftImageUrl,
            string errorText)
        {
            var competition = new OpenCompetitionEntity(Guid.NewGuid())
            {
                Locale = "en-US",
                Gift = string.Empty,
                Status = CompetitionStatus.Draft,
                Competitors = new List<Competitor>(),
                WinnerAadObjectIds = new List<string>()
            };
            var activateActionData = new ActivateCompetitionInvokeActionData
            {
                UserAction = InvokeActionType.ActivateCompetition,
                CompetitionId = competition.Id,
                Gift = gift,
                GiftImageUrl = giftImageUrl,
                WinnerCount = winnerCount,
                PlannedDrawTimeLocalDate = plannedDrawLocalDate,
                PlannedDrawTimeLocalTime = plannedDrawLocalTime
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var arrangement = server.Arrange();
                await arrangement.GetOpenCompetitions().InsertOrReplace(competition);

                var response = await client.SendTeamsTaskFetch(activateActionData);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TaskModuleTaskInfoResponse>();
                result.Task?.Value?.Card?.Content.Should().NotBeNull();
                var cardBody = ((JObject)result.Task.Value.Card.Content).GetValue("body");
                var errorTextBlock = ((JObject)cardBody.First()).ToObject<AdaptiveTextBlock>();
                errorTextBlock.Color.Should().Be(AdaptiveTextColor.Attention);
                errorTextBlock.Text.Should().Be(errorText);
            }
        }

        public class ActivateCompetitionInvokeActionData : InvokeActionData
        {
            public string Gift { get; set; }
            public string GiftImageUrl { get; set; }
            public int WinnerCount { get; set; }
            public string PlannedDrawTimeLocalDate { get; set; }
            public string PlannedDrawTimeLocalTime { get; set; }
        }
    }
}