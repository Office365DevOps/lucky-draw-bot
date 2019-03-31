using System;

namespace LuckyDrawBot.Tests.Infrastructure
{
    public class ServerAssertion
    {
        public IServiceProvider MainServices { get; }

        public ServerAssertion(IServiceProvider mainServices)
        {
            MainServices = mainServices;
        }

    }
}
