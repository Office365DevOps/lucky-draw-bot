using System.Net.Http;
using Moq;

namespace LuckyDrawBot.Tests.Infrastructure
{
    public class DependencyServiceHttpHandler
    {
        public Mock<HttpMessageHandler> HttpHandler { get; }
        public string BaseUrl { get; }

        public DependencyServiceHttpHandler(Mock<HttpMessageHandler> httpHandler, string baseUrl)
        {
            HttpHandler = httpHandler;
            BaseUrl = baseUrl;
        }
    }
}
