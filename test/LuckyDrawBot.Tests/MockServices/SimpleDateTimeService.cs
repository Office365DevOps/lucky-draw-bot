using System;
using LuckyDrawBot.Services;

namespace LuckyDrawBot.Tests.MockServices
{
    public class SimpleDateTimeService : IDateTimeService
    {
        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow;
    }
}