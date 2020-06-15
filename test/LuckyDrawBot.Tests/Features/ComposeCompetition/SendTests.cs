using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AdaptiveCards;
using FluentAssertions;
using LuckyDrawBot.Controllers;
using LuckyDrawBot.Models;
using LuckyDrawBot.Tests.Infrastructure;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Xunit;
using Xunit.Abstractions;

namespace LuckyDrawBot.Tests.Features.ComposeCompetition
{
    public class SendTests : BaseTest
    {
        public SendTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidInput_Send_CreatesLuckyDrawMainActivity()
        {
            var offset = TimeSpan.FromHours(9.5);
            var utcNow = DateTimeOffset.UtcNow;
            var plannedDrawTime = new DateTimeOffset(utcNow.Year, utcNow.Month, utcNow.Day, utcNow.Hour, utcNow.Minute, 0, TimeSpan.Zero);
            var plannedDrawTimeLocal = plannedDrawTime.ToOffset(offset);
            var editForm = new CompetitionEditForm
            {
                Gift = Guid.NewGuid().ToString(),
                GiftImageUrl = "https://www.some.com/image",
                WinnerCount = "56",
                PlannedDrawTimeLocalDate = plannedDrawTimeLocal.ToString("yyyy-MM-dd"),
                PlannedDrawTimeLocalTime = plannedDrawTimeLocal.ToString("HH:mm"),
            };

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                var activity = new Activity
                {
                    Name = "composeExtension/submitAction",
                    ServiceUrl = "https://service-url.com",
                    ChannelId = "msteams",
                    Type = ActivityTypes.Invoke,
                    Value = new
                    {
                        CommandId = "create",
                        BotMessagePreviewAction = "send",
                        BotActivityPreview = new []
                        {
                            new Activity
                            {
                                Attachments = new []
                                {
                                    new Attachment
                                    {
                                        Content = new AdaptiveCard("1.0")
                                        {
                                            Body = new List<AdaptiveElement>
                                            {
                                                new AdaptiveTextBlock
                                                {
                                                    Id = "LuckyDrawData",
                                                    Text = JsonSerializer.Serialize(editForm)
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    },
                    From = new ChannelAccount("id", "name"),
                    Conversation = new ConversationAccount(isGroup: true, id: "conv id", name: "conv name"),
                    ChannelData = new TeamsChannelData
                    {
                        Tenant = new TenantInfo { Id = Guid.NewGuid().ToString() },
                        Team = new TeamInfo { Id = Guid.NewGuid().ToString() },
                        Channel = new ChannelInfo { Id = Guid.NewGuid().ToString() }
                    },
                    Locale = "en-us",
                    LocalTimestamp = new DateTimeOffset(2018, 1, 1, 1, 1, 1, 1, offset)
                };

                var response = await client.SendActivity(activity);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var openCompetitions = server.Assert().GetOpenCompetitions();
                openCompetitions.Should().HaveCount(1);
                openCompetitions[0].Status.Should().Be(CompetitionStatus.Active);
                openCompetitions[0].Gift.Should().Be(editForm.Gift);
                openCompetitions[0].GiftImageUrl.Should().Be(editForm.GiftImageUrl);
                openCompetitions[0].WinnerCount.Should().Be(int.Parse(editForm.WinnerCount));
                openCompetitions[0].PlannedDrawTime.Should().Be(plannedDrawTime);
                var createdMessages = server.Assert().GetCreatedMessages();
                createdMessages.Should().HaveCount(1);
                var heroCard = createdMessages[0].Activity.Attachments[0].Content as HeroCard;
                heroCard.Buttons.Should().HaveCount(2);
                heroCard.Buttons[0].Type.Should().Be("invoke");
                heroCard.Buttons[0].Value.Should().BeEquivalentTo(new InvokeActionData { UserAction = InvokeActionType.Join, CompetitionId = openCompetitions[0].Id });
                heroCard.Buttons[1].Type.Should().Be("invoke");
                heroCard.Buttons[1].Value.Should().BeEquivalentTo(new InvokeActionData { Type = InvokeActionData.TypeTaskFetch, UserAction = InvokeActionType.ViewDetail, CompetitionId = openCompetitions[0].Id });
            }
        }

    }
}
