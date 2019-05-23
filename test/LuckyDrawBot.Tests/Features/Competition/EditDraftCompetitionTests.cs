using System;
using System.Collections.Generic;
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
using static LuckyDrawBot.Controllers.MessagesController;
using static LuckyDrawBot.Services.CompetitionRepositoryService;

namespace LuckyDrawBot.Tests.Features.Competition
{
    public class EditDraftCompetitionTests : BaseTest
    {
        public EditDraftCompetitionTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task WhenDraftCompetitionIsCreated_InvokeEditAction_EditCompetitionTaskInfoIsGenerated()
        {
            var competition = new OpenCompetitionEntity(Guid.NewGuid())
            {
                Locale = "en-US",
                Gift = string.Empty,
                Status = CompetitionStatus.Draft,
                Competitors = new List<Competitor>(),
                WinnerAadObjectIds = new List<string>(),
                CreatorAadObjectId = Guid.NewGuid().ToString()
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var arrangement = server.Arrange();
                await arrangement.GetOpenCompetitions().InsertOrReplace(competition);

                var response = await client.SendTeamsTaskFetch(
                    new InvokeActionData { UserAction = InvokeActionType.EditDraft, CompetitionId = competition.Id },
                    from: new ChannelAccount { AadObjectId = competition.CreatorAadObjectId });

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TaskModuleTaskInfoResponse>();
                result.Task?.Value?.Card?.Content.Should().NotBeNull();
                var card = ((JObject)result.Task.Value.Card.Content).ToObject<AdaptiveCard>();
                card.Actions.Should().HaveCount(2);
                card.Actions[0].Title.Should().Be("Save");
                card.Actions[1].Title.Should().Be("Start");
            }
        }

        [Fact]
        public async Task WhenDraftCompetitionIsCreatedByOthers_InvokeEditAction_ReturnsNotAllowedEdit()
        {
            var currentUserAadObjectId = Guid.NewGuid().ToString();
            var creatorAadObjectId = Guid.NewGuid().ToString();
            var competition = new OpenCompetitionEntity(Guid.NewGuid())
            {
                Locale = "en-US",
                Gift = string.Empty,
                Status = CompetitionStatus.Draft,
                Competitors = new List<Competitor>(),
                WinnerAadObjectIds = new List<string>(),
                CreatorAadObjectId = creatorAadObjectId
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var arrangement = server.Arrange();
                await arrangement.GetOpenCompetitions().InsertOrReplace(competition);

                var response = await client.SendTeamsTaskFetch(
                    new InvokeActionData { UserAction = InvokeActionType.EditDraft, CompetitionId = competition.Id },
                    from: new ChannelAccount { AadObjectId = currentUserAadObjectId });

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var result = await response.Content.ReadAsAsync<TaskModuleTaskInfoResponse>();
                result.Task?.Value?.Card?.Content.Should().NotBeNull();
                var card = ((JObject)result.Task.Value.Card.Content).ToObject<AdaptiveCard>();
                card.Body.Should().HaveCount(1);
                card.Body[0].Type.Should().Be("TextBlock");
            }
        }

        [Fact]
        public async Task WhenDraftCompetitionIsEdited_InvokeSaveAction_CompetitionIsUpdated()
        {
            var competition = new OpenCompetitionEntity(Guid.NewGuid())
            {
                Locale = "en-US",
                Gift = string.Empty,
                Status = CompetitionStatus.Draft,
                Competitors = new List<Competitor>(),
                WinnerAadObjectIds = new List<string>()
            };
            var editActionData = new SaveCompetitionInvokeActionData
            {
                UserAction = InvokeActionType.SaveDraft,
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

                var response = await client.SendTeamsTaskFetch(editActionData);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var openCompetitions = server.Assert().GetOpenCompetitions();
                openCompetitions.Should().HaveCount(1);
                openCompetitions[0].Gift.Should().Be(editActionData.Gift);
                openCompetitions[0].GiftImageUrl.Should().Be(editActionData.GiftImageUrl);
                openCompetitions[0].WinnerCount.Should().Be(editActionData.WinnerCount);
                openCompetitions[0].PlannedDrawTime.Should().Be(DateTimeOffset.Parse($"{editActionData.PlannedDrawTimeLocalDate}T{editActionData.PlannedDrawTimeLocalTime}Z"));
            }
        }

        public class SaveCompetitionInvokeActionData : InvokeActionData
        {
            public string Gift { get; set; }
            public string GiftImageUrl { get; set; }
            public int WinnerCount { get; set; }
            public string PlannedDrawTimeLocalDate { get; set; }
            public string PlannedDrawTimeLocalTime { get; set; }
        }
    }
}