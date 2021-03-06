﻿using Microsoft.Extensions.DependencyInjection;
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

        public static InMemoryDataTable<LuckyDrawDataTablesSettings, ClosedCompetitionEntity> GetClosedCompetitions(this ServerArrangement arrangement)
        {
            var closedCompetitions = arrangement.MainServices.GetRequiredService<IDataTable<LuckyDrawDataTablesSettings, ClosedCompetitionEntity>>() as InMemoryDataTable<LuckyDrawDataTablesSettings, ClosedCompetitionEntity>;
            return closedCompetitions;
        }

    }
}
