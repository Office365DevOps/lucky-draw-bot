using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Net.Http;
using LuckyDrawBot.Tests.Infrastructure;
using LuckyDrawBot.Services;
using LuckyDrawBot.Tests.MockServices;

namespace LuckyDrawBot.Tests
{
    public static class ServerArrangementExtensions
    {
        public static DependencyServiceHttpHandler GetTimerServiceHandler(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler("Timer");
        }

        public static void SetUtcNow(this ServerArrangement arrangement, DateTime utcNow)
        {
            var mockService = arrangement.MainServices.GetRequiredService<IDateTimeService>() as MockDateTimeService;
            mockService.UtcNow = utcNow;
        }
    }
}
