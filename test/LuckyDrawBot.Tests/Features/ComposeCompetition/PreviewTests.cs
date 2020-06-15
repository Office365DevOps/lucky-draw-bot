using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AdaptiveCards;
using FluentAssertions;
using LuckyDrawBot.Controllers;
using LuckyDrawBot.Tests.Infrastructure;
using LuckyDrawBot.Tests.Models;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Schema.Teams;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace LuckyDrawBot.Tests.Features.ComposeCompetition
{
    public class PreviewTests : BaseTest
    {
        public PreviewTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidInput_Preview_ReturnsBotPreviewWithCorrectValues()
        {
            var gift = Guid.NewGuid().ToString();
            var winnerCount = "56";
            var giftImageUrl = "https://www.some.com/image";
            var plannedDrawTimeLocal = DateTimeOffset.UtcNow.AddDays(1);
            var plannedDrawTimeLocalDate = plannedDrawTimeLocal.ToString("yyyy-MM-dd");
            var plannedDrawTimeLocalTime = plannedDrawTimeLocal.ToString("HH:mm");

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
                        Data = new
                        {
                            UserAction = "Preview",
                            Gift = gift,
                            WinnerCount = winnerCount,
                            PlannedDrawTimeLocalDate = plannedDrawTimeLocalDate,
                            PlannedDrawTimeLocalTime = plannedDrawTimeLocalTime,
                            GiftImageUrl = giftImageUrl
                        }
                    },
                    Locale = "en-us",
                    LocalTimestamp = new DateTimeOffset(2018, 1, 1, 1, 1, 1, 1, TimeSpan.FromHours(5.5))
                };
                var response = await client.SendActivity(activity);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var actionResponse = await response.Content.ReadAsWithNewtonsoftJsonAsync<MessagingExtensionActionResponse>();
                actionResponse.ComposeExtension.Type.Should().Be("botMessagePreview");
                var card = ((JObject)actionResponse.ComposeExtension.ActivityPreview.Attachments[0].Content).ToObject<AdaptiveCard>();
                ((AdaptiveTextBlock)card.Body[0]).Text.Should().Be(gift);
                ((AdaptiveTextBlock)card.Body[1]).Text.Should().Contain(winnerCount);
                ((AdaptiveTextBlock)card.Body[1]).Text.Should().Contain(DateTimeOffset.Parse(plannedDrawTimeLocalDate + " " + plannedDrawTimeLocalTime).ToString("f", CultureInfo.GetCultureInfo("en-us")));
                ((AdaptiveImage)card.Body[3]).UrlString.Should().Be(giftImageUrl);
                var hiddenEditFormData = (AdaptiveTextBlock)card.Body[4];
                hiddenEditFormData.Id.Should().Be("LuckyDrawData");
                hiddenEditFormData.IsVisible.Should().BeFalse();
                var editForm = new CompetitionEditForm
                {
                    Gift = gift,
                    GiftImageUrl = giftImageUrl,
                    WinnerCount = winnerCount,
                    PlannedDrawTimeLocalDate = plannedDrawTimeLocalDate,
                    PlannedDrawTimeLocalTime = plannedDrawTimeLocalTime,
                };
                hiddenEditFormData.Text.Should().Be(System.Text.Json.JsonSerializer.Serialize(editForm));
            }
        }

        [Fact]
        public async Task GivenInvalidInput_Preview_ReturnsErrorMessage()
        {
            var gift = string.Empty;
            var winnerCount = "0";
            var giftImageUrl = "non-http://abc.com";
            var plannedDrawTimeLocal = DateTimeOffset.UtcNow.AddMinutes(-1);
            var plannedDrawTimeLocalDate = plannedDrawTimeLocal.ToString("yyyy-MM-dd");
            var plannedDrawTimeLocalTime = plannedDrawTimeLocal.ToString("HH:mm");

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
                        Data = new
                        {
                            UserAction = "Preview",
                            Gift = gift,
                            WinnerCount = winnerCount,
                            PlannedDrawTimeLocalDate = plannedDrawTimeLocalDate,
                            PlannedDrawTimeLocalTime = plannedDrawTimeLocalTime,
                            GiftImageUrl = giftImageUrl
                        }
                    },
                    Locale = "en-us",
                    LocalTimestamp = new DateTimeOffset(2018, 1, 1, 1, 1, 1, 1, TimeSpan.Zero)
                };
                var response = await client.SendActivity(activity);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var actionResponse = await response.Content.ReadAsWithNewtonsoftJsonAsync<MessagingExtensionActionResponseForContinue>();
                var card = ((JObject)actionResponse.Task.Value.Card.Content).ToObject<AdaptiveCard>();
                var errorTextBlock = card.Body[0] as AdaptiveTextBlock;
                errorTextBlock.Color.Should().Be(AdaptiveTextColor.Attention);
                errorTextBlock.Text.Should().Contain("specify the prize");
                errorTextBlock.Text.Should().Contain("number of prizes");
                errorTextBlock.Text.Should().Contain("draw time");
                errorTextBlock.Text.Should().Contain("URL of prize image");
            }
        }
    }
}
