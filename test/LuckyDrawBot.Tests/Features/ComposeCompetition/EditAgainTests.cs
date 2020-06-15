using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using AdaptiveCards;
using FluentAssertions;
using LuckyDrawBot.Controllers;
using LuckyDrawBot.Tests.Infrastructure;
using LuckyDrawBot.Tests.Models;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace LuckyDrawBot.Tests.Features.ComposeCompetition
{
    public class EditAgainTests : BaseTest
    {
        public EditAgainTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidPreview_EditAgain_ReturnsEditFormWithCorrectInputValues()
        {
            var gift = Guid.NewGuid().ToString();
            var winnerCount = "56";
            var giftImageUrl = "https://www.some.com/image";
            var plannedDrawTimeLocal = DateTimeOffset.UtcNow;
            var plannedDrawTimeLocalDate = plannedDrawTimeLocal.ToString("yyyy-MM-dd");
            var plannedDrawTimeLocalTime = plannedDrawTimeLocal.ToString("HH:mm");
            var editForm = new CompetitionEditForm
            {
                Gift = gift,
                GiftImageUrl = giftImageUrl,
                WinnerCount = winnerCount,
                PlannedDrawTimeLocalDate = plannedDrawTimeLocalDate,
                PlannedDrawTimeLocalTime = plannedDrawTimeLocalTime,
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
                        BotMessagePreviewAction = "edit",
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
                    Locale = "en-us",
                    LocalTimestamp = new DateTimeOffset(2018, 1, 1, 1, 1, 1, 1, TimeSpan.FromHours(3.5))
                };
                var response = await client.SendActivity(activity);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var actionResponse = await response.Content.ReadAsWithNewtonsoftJsonAsync<MessagingExtensionActionResponseForContinue>();
                var card = ((JObject)actionResponse.Task.Value.Card.Content).ToObject<AdaptiveCard>();
                var giftTextInput = card.Body.FirstOrDefault(x => x.Id == "gift") as AdaptiveTextInput;
                giftTextInput.Should().NotBeNull();
                giftTextInput.Value.Should().Be(gift);
                var winnerCountTextInput = card.Body.FirstOrDefault(x => x.Id == "winnerCount") as AdaptiveNumberInput;
                winnerCountTextInput.Should().NotBeNull();
                winnerCountTextInput.Value.Should().Be(int.Parse(winnerCount));
                var columnSet = card.Body[5] as AdaptiveColumnSet;
                var dateInput = columnSet.Columns[0].Items[0] as AdaptiveDateInput;
                dateInput.Should().NotBeNull();
                dateInput.Value.Should().Be(plannedDrawTimeLocalDate);
                var timeInput = columnSet.Columns[1].Items[0] as AdaptiveTimeInput;
                timeInput.Should().NotBeNull();
                timeInput.Value.Should().Be(plannedDrawTimeLocalTime);
                var giftImageUrlTextInput = card.Body.FirstOrDefault(x => x.Id == "giftImageUrl") as AdaptiveTextInput;
                giftImageUrlTextInput.Should().NotBeNull();
                giftImageUrlTextInput.Value.Should().Be(giftImageUrl);
            }
        }
    }
}
