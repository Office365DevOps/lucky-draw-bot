using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Net.Http;
using LuckyDrawBot.Tests.Infrastructure;
using LuckyDrawBot.Services;
using LuckyDrawBot.Tests.MockServices;
using System.Collections.Generic;
using LuckyDrawBot.Infrastructure.Database;
using static LuckyDrawBot.Services.CompetitionRepositoryService;
using LuckyDrawBot.Tests.Infrastructure.Database;

namespace LuckyDrawBot.Tests
{
    public static class ServerAssertionExtensions
    {
        public static List<CreatedMessage> GetCreatedMessages(this ServerAssertion assertion)
        {
            var factory = assertion.MainServices.GetRequiredService<IBotClientFactory>() as SimpleBotClientFactory;
            return factory.CreatedMessages;
        }

        public static List<UpdatedMessage> GetUpdatedMessages(this ServerAssertion assertion)
        {
            var factory = assertion.MainServices.GetRequiredService<IBotClientFactory>() as SimpleBotClientFactory;
            return factory.UpdatedMessages;
        }

        public static IReadOnlyList<OpenCompetitionEntity> GetOpenCompetitions(this ServerAssertion assertion)
        {
            var openCompetitions = assertion.MainServices.GetRequiredService<IDataTable<LuckyDrawDataTablesSettings, OpenCompetitionEntity>>() as InMemoryDataTable<LuckyDrawDataTablesSettings, OpenCompetitionEntity>;
            return openCompetitions.AllEntities;
        }

        public static IReadOnlyList<ClosedCompetitionEntity> GetClosedCompetitions(this ServerAssertion assertion)
        {
            var closedCompetitions = assertion.MainServices.GetRequiredService<IDataTable<LuckyDrawDataTablesSettings, ClosedCompetitionEntity>>() as InMemoryDataTable<LuckyDrawDataTablesSettings, ClosedCompetitionEntity>;
            return closedCompetitions.AllEntities;
        }
    }
}
