using Microsoft.Extensions.DependencyInjection;
using Moq;
using System;
using System.Net.Http;
using LuckyDrawBot.Tests.Infrastructure;
using LuckyDrawBot.Services;
using LuckyDrawBot.Tests.MockServices;
using System.Collections.Generic;

namespace LuckyDrawBot.Tests
{
    public static class ServerAssertionExtensions
    {
        public static List<CreatedMessage> GetCreatedMessages(this ServerAssertion assertion)
        {
            var factory = assertion.MainServices.GetRequiredService<IBotClientFactory>() as MockBotClientFactory;
            return factory.CreatedMessages;
        }

        public static List<UpdatedMessage> GetUpdatedMessages(this ServerAssertion assertion)
        {
            var factory = assertion.MainServices.GetRequiredService<IBotClientFactory>() as MockBotClientFactory;
            return factory.UpdatedMessages;
        }
    }
}
