using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Net.Http;
using LuckyDrawBot.Tests.Infrastructure;
using LuckyDrawBot.Services;
using LuckyDrawBot.Tests.MockServices;
using System.Collections.Generic;
using static LuckyDrawBot.Services.CompetitionRepositoryService;
using LuckyDrawBot.Infrastructure.Database;
using LuckyDrawBot.Tests.Infrastructure.Database;

namespace LuckyDrawBot.Tests
{
    public static class ServerArrangementExtensions
    {
        public static DependencyServiceHttpHandler GetTimerServiceHandler(this ServerArrangement arrangement)
        {
            return arrangement.GetHttpHandler("Timer");
        }

        public static void SetUtcNow(this ServerArrangement arrangement, DateTimeOffset utcNow)
        {
            var mockService = arrangement.MainServices.GetRequiredService<IDateTimeService>() as SimpleDateTimeService;
            mockService.UtcNow = utcNow;
        }

        public static InMemoryDataTable<LuckyDrawDataTablesSettings, OpenCompetitionEntity> GetOpenCompetitions(this ServerArrangement arrangement)
        {
            var openCompetitions = arrangement.MainServices.GetRequiredService<IDataTable<LuckyDrawDataTablesSettings, OpenCompetitionEntity>>() as InMemoryDataTable<LuckyDrawDataTablesSettings, OpenCompetitionEntity>;
            return openCompetitions;
        }

    }
}
