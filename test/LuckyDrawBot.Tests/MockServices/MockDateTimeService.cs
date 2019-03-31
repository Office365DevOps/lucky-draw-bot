using System;
using LuckyDrawBot.Services;

namespace LuckyDrawBot.Tests.MockServices
{
    public class MockDateTimeService : IDateTimeService
    {
        public DateTimeOffset UtcNow { get; set; } = DateTimeOffset.UtcNow;
    }
}