using LuckyDrawBot.Infrastructure.Database;
using LuckyDrawBot.Tests.Infrastructure.Database;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Contrib.HttpClient;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace LuckyDrawBot.Tests.Infrastructure
{
    public class MainServiceStartup<TOriginalStartup> where TOriginalStartup : class
    {
        private readonly TOriginalStartup _startup;

        public MainServiceStartup(IServiceProvider serviceProvider)
        {
            _startup = serviceProvider.CreateInstance<TOriginalStartup>();
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InvokeMethod(
                _startup,
                "ConfigureServices",
                new Dictionary<Type, object>() { [typeof(IServiceCollection)] = services });

            var assembly = typeof(TOriginalStartup).Assembly;
            services.AddApplicationPart(assembly);

            var serverFixtureConfiguration = serviceProvider.GetRequiredService<ServerFixtureConfiguration>();

            var httpHandler = new Mock<HttpMessageHandler>();
            services.AddSingleton(httpHandler);
            var httpClientFactory = httpHandler.CreateClientFactory();
            services.AddSingleton(typeof(IHttpClientFactory), httpClientFactory);
            var mockHttpClientFactory = Mock.Get(httpClientFactory);
            foreach (var dependencyService in serverFixtureConfiguration.DependencyServices)
            {
                var baseUrl = $"https://{dependencyService.Name}.com";
                mockHttpClientFactory
                    .Setup(x => x.CreateClient(dependencyService.Name))
                    .Returns(() =>
                    {
                        var client = httpHandler.CreateClient();
                        client.BaseAddress = new Uri(baseUrl);
                        return client;
                    });
                if (dependencyService.Setup != null)
                {
                    var handler = new DependencyServiceHttpHandler(httpHandler, baseUrl);
                    dependencyService.Setup(handler);
                }
            }

            services.ReplaceSingleton(typeof(IDataTable<,>), typeof(InMemoryDataTable<,>));

            serverFixtureConfiguration.MainServicePostConfigureServices?.Invoke(services);
        }

        public void Configure(IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            serviceProvider.InvokeMethod(
                _startup,
                "Configure",
                new Dictionary<Type, object>() { [typeof(IApplicationBuilder)] = app });

            var serverFixtureConfiguration = serviceProvider.GetRequiredService<ServerFixtureConfiguration>();
            serverFixtureConfiguration.MainServicePostConfigure?.Invoke(app, serviceProvider);
        }

    }

}
