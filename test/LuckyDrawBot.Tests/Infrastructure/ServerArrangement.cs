using System;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace LuckyDrawBot.Tests.Infrastructure
{
    public class ServerArrangement
    {
        public IServiceProvider MainServices { get; }

        public ServerArrangement(IServiceProvider mainServices)
        {
            MainServices = mainServices;
        }

        public DependencyServiceHttpHandler GetHttpHandler(string dependencyServiceName)
        {
            var httpHandler = MainServices.GetRequiredService<Mock<HttpMessageHandler>>();
            return new DependencyServiceHttpHandler(httpHandler, $"https://{dependencyServiceName}.com");
        }

    }
}
