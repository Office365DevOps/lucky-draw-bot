using System;
using System.Globalization;

namespace LuckyDrawBot.Controllers
{
    public class CompetitionEditForm
    {
        public string Gift { get; set; }
        public string GiftImageUrl { get; set; }
        public string WinnerCount { get; set; }
        public string PlannedDrawTimeLocalDate { get; set; }
        public string PlannedDrawTimeLocalTime { get; set; }

        public DateTimeOffset GetPlannedDrawTime(TimeSpan localTimeOffset, DateTimeOffset defaultUtcPlannedDrawTime)
        {
            // Teams/AdaptiveCards BUG: https://github.com/Microsoft/AdaptiveCards/issues/2644
            // DateInput does not post back the value in non-English situation.
            // Workaround: use TextInput instead and validate user's input against "yyyy-MM-dd" format
            if (!DateTimeOffset.TryParseExact(this.PlannedDrawTimeLocalDate, "yyyy-MM-dd", CultureInfo.InvariantCulture.DateTimeFormat, DateTimeStyles.None, out DateTimeOffset dummy))
            {
                var defaultLocalPlannedDrawTime = defaultUtcPlannedDrawTime.ToOffset(TimeSpan.FromHours(localTimeOffset.TotalHours));
                this.PlannedDrawTimeLocalDate = defaultLocalPlannedDrawTime.ToString("yyyy-MM-dd");
            }

            var date = DateTimeOffset.Parse(this.PlannedDrawTimeLocalDate);
            var time = DateTimeOffset.Parse(this.PlannedDrawTimeLocalTime);
            var plannedDrawTime = new DateTimeOffset(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, 0, localTimeOffset).ToUniversalTime();
            return plannedDrawTime;
        }
    }
}
