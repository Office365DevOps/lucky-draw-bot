using Microsoft.Extensions.DependencyInjection;
using LuckyDrawBot.Tests.Infrastructure;
using LuckyDrawBot.Services;
using LuckyDrawBot.Tests.MockServices;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Moq;
using System.Net.Http;
using MaxKagamine.Moq.HttpClient;
using System.Net;

namespace LuckyDrawBot.Tests
{
    public static class ServerFixtureConfigurations
    {
        public static ServerFixtureConfiguration Default { get; } = new ServerFixtureConfiguration()
        {
            StartupType = typeof(Startup),
            DependencyServices = new List<DependencyServiceConfiguration>
            {
                new DependencyServiceConfiguration
                {
                    Name = "Timer",
                    Setup = serviceHandler =>
                    {
                        serviceHandler.HttpHandler.SetupRequest(HttpMethod.Post, serviceHandler.BaseUrl).ReturnsResponse(HttpStatusCode.OK);
                    }
                }
            },
            MainServicePostConfigureServices = (services) =>
            {
                services.ReplaceSingleton<IDateTimeService, SimpleDateTimeService>();
                services.ReplaceSingleton<IBotClientFactory, SimpleBotClientFactory>();
            },
            MainServicePostAppConfiguration = (configuration, testContext) =>
            {
                var settings = new Dictionary<string, string>
                {
                    ["Bot:Id"] = Guid.NewGuid().ToString(),
                    ["ServicePublicBaseUrl"] = "https://service.com"
                };
                configuration.AddInMemoryCollection(settings);
            }
        };
    }
}
