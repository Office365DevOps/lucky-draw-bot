using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using AdaptiveCards;
using FluentAssertions;
using LuckyDrawBot.Tests.Infrastructure;
using LuckyDrawBot.Tests.Models;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using Xunit;
using Xunit.Abstractions;

namespace LuckyDrawBot.Tests.Features.ComposeCompetition
{
    public class StartFormTests : BaseTest
    {
        public StartFormTests(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public async Task GivenValidInput_StartForm_ReturnsEditForm()
        {
            var utcNow = DateTimeOffset.UtcNow;
            var offset = TimeSpan.FromHours(3.5);

            using (var server = CreateServerFixture(ServerFixtureConfigurations.Default))
            using (var client = server.CreateClient())
            {
                server.Arrange().SetUtcNow(utcNow);

                var activity = new Activity
                {
                    Name = "composeExtension/fetchTask",
                    ServiceUrl = "https://service-url.com",
                    ChannelId = "msteams",
                    Type = ActivityTypes.Invoke,
                    Value = new { CommandId = "create" },
                    Locale = "en-us",
                    LocalTimestamp = new DateTimeOffset(2018, 1, 1, 1, 1, 1, 1, offset)
                };
                var response = await client.SendActivity(activity);

                response.StatusCode.Should().Be(HttpStatusCode.OK);
                var actionResponse = await response.Content.ReadAsWithNewtonsoftJsonAsync<MessagingExtensionActionResponseForContinue>();
                var card = ((JObject)actionResponse.Task.Value.Card.Content).ToObject<AdaptiveCard>();
                var giftTextInput = card.Body.FirstOrDefault(x => x.Id == "gift") as AdaptiveTextInput;
                giftTextInput.Should().NotBeNull();
                giftTextInput.Value.Should().BeEmpty();
                var winnerCountTextInput = card.Body.FirstOrDefault(x => x.Id == "winnerCount") as AdaptiveNumberInput;
                winnerCountTextInput.Should().NotBeNull();
                winnerCountTextInput.Value.Should().Be(1);
                var columnSet = card.Body[5] as AdaptiveColumnSet;
                var dateInput = columnSet.Columns[0].Items[0] as AdaptiveDateInput;
                dateInput.Should().NotBeNull();
                dateInput.Value.Should().Be(utcNow.ToOffset(offset).AddHours(2).ToString("yyyy-MM-dd"));
                var timeInput = columnSet.Columns[1].Items[0] as AdaptiveTimeInput;
                timeInput.Should().NotBeNull();
                timeInput.Value.Should().Be(utcNow.ToOffset(offset).AddHours(2).ToString("HH:mm"));
                var giftImageUrlTextInput = card.Body.FirstOrDefault(x => x.Id == "giftImageUrl") as AdaptiveTextInput;
                giftImageUrlTextInput.Should().NotBeNull();
                giftImageUrlTextInput.Value.Should().BeEmpty();
            }
        }
    }
}
