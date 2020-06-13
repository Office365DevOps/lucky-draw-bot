using LuckyDrawBot.Controllers;
using LuckyDrawBot.Services;
using Microsoft.Bot.Schema;
using Newtonsoft.Json.Linq;
using System;
using System.Globalization;
using System.Text.Json;
using System.Threading.Tasks;

namespace LuckyDrawBot.Handlers
{
    public class ActionSaveDraftCompetitionHandler
    {
        private readonly ICompetitionService _competitionService;

        public ActionSaveDraftCompetitionHandler(ICompetitionService competitionService)
        {
            _competitionService = competitionService;
        }

        public async Task Handle(Activity activity)
        {
            var invokeActionData = activity.GetInvokeActionData();
            var data = ((JObject)activity.Value).GetValue("data");
            var editForm = JsonSerializer.Deserialize<CompetitionEditForm>(Newtonsoft.Json.JsonConvert.SerializeObject(data));

            // Teams/AdaptiveCards BUG: https://github.com/Microsoft/AdaptiveCards/issues/2644
            // DateInput does not post back the value in non-English situation.
            // Workaround: use TextInput instead and validate user's input against "yyyy-MM-dd" format
            if (!DateTimeOffset.TryParseExact(editForm.PlannedDrawTimeLocalDate, "yyyy-MM-dd", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out DateTimeOffset dummy))
            {
                var existingCompetition = await _competitionService.GetCompetition(invokeActionData.CompetitionId);
                editForm.PlannedDrawTimeLocalDate = existingCompetition.PlannedDrawTime.ToString("yyyy-MM-dd");
            }

            var offset = activity.GetOffset();
            var date = DateTimeOffset.Parse(editForm.PlannedDrawTimeLocalDate);
            var time = DateTimeOffset.Parse(editForm.PlannedDrawTimeLocalTime);
            var plannedDrawTime = new DateTimeOffset(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, 0, offset).ToUniversalTime();

            await _competitionService.UpdateGift(invokeActionData.CompetitionId, plannedDrawTime, editForm.Gift, editForm.GiftImageUrl, editForm.WinnerCount);
        }
    }
}
