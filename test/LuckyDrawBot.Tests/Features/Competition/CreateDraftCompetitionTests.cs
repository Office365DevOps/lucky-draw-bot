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
    public class CreateDraftCompetitionTests : BaseTest
    {
        public CreateDraftCompetitionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WhenInputTextStart_SendTextToBot_DraftCompetitionIsCreatedAndPosted()
        {
            var utcNow = DateTimeOffset.UtcNow;

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                server.Arrange().SetUtcNow(utcNow);

                var text = $"<at>bot name</at> start";
                var response = await client.SendTeamsText(text);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var openCompetitions = server.Assert().GetOpenCompetitions();
                openCompetitions.Should().HaveCount(1);
                openCompetitions[0].Status.Should().Be(CompetitionStatus.Draft);
                openCompetitions[0].Gift.Should().Be(string.Empty);
                openCompetitions[0].WinnerCount.Should().Be(1);
                openCompetitions[0].PlannedDrawTime.Should().Be(utcNow.AddHours(2));
                openCompetitions[0].GiftImageUrl.Should().Be(string.Empty);
                var createdMessages = server.Assert().GetCreatedMessages();
                createdMessages.Should().HaveCount(1);
                var heroCard = createdMessages[0].Activity.Attachments[0].Content as HeroCard;
                heroCard.Buttons.Should().HaveCount(1);
                heroCard.Buttons[0].Type.Should().Be("invoke");
                heroCard.Buttons[0].Value.Should().BeEquivalentTo(new InvokeActionData { Type = InvokeActionData.TypeTaskFetch, UserAction = InvokeActionType.EditDraft, CompetitionId = openCompetitions[0].Id });
            }
        }
    }
}