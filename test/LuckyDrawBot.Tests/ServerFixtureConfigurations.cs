using Microsoft.Extensions.DependencyInjection;
using LuckyDrawBot.Tests.Infrastructure;
using LuckyDrawBot.Services;
using LuckyDrawBot.Tests.MockServices;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace LuckyDrawBot.Tests
{
    public static class ServerFixtureConfigurations
    {
        public static ServerFixtureConfiguration Default { get; } = new ServerFixtureConfiguration()
        {
            StartupType = typeof(Startup),
            MainServicePostConfigureServices = (services) =>
            {
                services.ReplaceSingleton<IDateTimeService, MockDateTimeService>();
                services.ReplaceSingleton<IBotClientFactory, MockBotClientFactory>();
            },
            MainServicePostAppConfiguration = (configuration, testContext) =>
            {
                var settings = new Dictionary<string, string>
                {
                    ["Bot:Id"] = Guid.NewGuid().ToString()
                };
                configuration.AddInMemoryCollection(settings);
            }
        };
    }
}
