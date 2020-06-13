using FluentAssertions;
using Moq.Contrib.HttpClient;
using Moq.Language;
using Moq.Language.Flow;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace LuckyDrawBot.Tests.Infrastructure
{
    public static class MockHttpMessageHandlerExtensions
    {
        public static IReturnsResult<HttpMessageHandler> ReturnsJson<T>(this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup, T content)
        {
            return setup.ReturnsResponse(JsonSerializer.Serialize(content), "application/json");
        }

        public static IReturnsResult<HttpMessageHandler> ReturnsJson<T>(this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup, HttpStatusCode statusCode, T content)
        {
            return setup.ReturnsResponse(statusCode, JsonSerializer.Serialize(content), "application/json");
        }

        public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsJson<T>(this ISetupSequentialResult<Task<HttpResponseMessage>> setup, T content)
        {
            return setup.ReturnsResponse(JsonSerializer.Serialize(content), "application/json");
        }

        public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsJson<T>(this ISetupSequentialResult<Task<HttpResponseMessage>> setup, HttpStatusCode statusCode, T content)
        {
            return setup.ReturnsResponse(statusCode, JsonSerializer.Serialize(content), "application/json");
        }


        public static IReturnsResult<HttpMessageHandler> ReturnsJsonUsingNewtonsoft<T>(this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup, T content)
        {
            return setup.ReturnsResponse(Newtonsoft.Json.JsonConvert.SerializeObject(content), "application/json");
        }

        public static IReturnsResult<HttpMessageHandler> ReturnsJsonUsingNewtonsoft<T>(this ISetup<HttpMessageHandler, Task<HttpResponseMessage>> setup, HttpStatusCode statusCode, T content)
        {
            return setup.ReturnsResponse(statusCode, Newtonsoft.Json.JsonConvert.SerializeObject(content), "application/json");
        }

        public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsJsonUsingNewtonsoft<T>(this ISetupSequentialResult<Task<HttpResponseMessage>> setup, T content)
        {
            return setup.ReturnsResponse(Newtonsoft.Json.JsonConvert.SerializeObject(content), "application/json");
        }

        public static ISetupSequentialResult<Task<HttpResponseMessage>> ReturnsJsonUsingNewtonsoft<T>(this ISetupSequentialResult<Task<HttpResponseMessage>> setup, HttpStatusCode statusCode, T content)
        {
            return setup.ReturnsResponse(statusCode, Newtonsoft.Json.JsonConvert.SerializeObject(content), "application/json");
        }

        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(this DependencyServiceHttpHandler handler, HttpMethod method, string requestUrl)
        {
            return handler.HttpHandler.SetupRequest(method, $"{handler.BaseUrl}/{requestUrl}");
        }

        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequest(this DependencyServiceHttpHandler handler, HttpMethod method, string requestUrl, Func<HttpRequestMessage, Task<bool>> match)
        {
            return handler.HttpHandler.SetupRequest(method, $"{handler.BaseUrl}/{requestUrl}", match);
        }

        public static ISetupSequentialResult<Task<HttpResponseMessage>> SetupRequestSequence(this DependencyServiceHttpHandler handler, HttpMethod method, string requestUrl)
        {
            return handler.HttpHandler.SetupRequestSequence(method, $"{handler.BaseUrl}/{requestUrl}");
        }

        public static ISetupSequentialResult<Task<HttpResponseMessage>> SetupRequestSequence(this DependencyServiceHttpHandler handler, HttpMethod method, string requestUrl, Func<HttpRequestMessage, Task<bool>> match)
        {
            return handler.HttpHandler.SetupRequestSequence(method, $"{handler.BaseUrl}/{requestUrl}", match);
        }

        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequestWithExpectedBody<TRequestJsonType>(this DependencyServiceHttpHandler handler, HttpMethod method, string requestUrl, TRequestJsonType expectedRequestBody)
        {
            return handler.SetupRequest(method, requestUrl, async(message) =>
            {
                var stringContent = await message.Content.ReadAsStringAsync();
                var requestBody = JsonSerializer.Deserialize<TRequestJsonType>(stringContent);
                requestBody.Should().BeEquivalentTo(expectedRequestBody);

                return true;
            });
        }

        public static ISetup<HttpMessageHandler, Task<HttpResponseMessage>> SetupRequestWithExpectedFileContent(this DependencyServiceHttpHandler handler, HttpMethod method, string requestUrl, string fileParameterName, byte[] expectedFileBytes)
        {
            return handler.SetupRequest(method, requestUrl, async(message) =>
            {
                message.Content.GetType().Should().Be(typeof(MultipartFormDataContent));
                var multipartContent = (MultipartFormDataContent)message.Content;
                var filePart = multipartContent.FirstOrDefault(part => part.Headers.ContentDisposition.Name == fileParameterName);
                filePart.Should().NotBeNull();
                var bytes = await filePart.ReadAsByteArrayAsync();
                bytes.Should().BeEquivalentTo(expectedFileBytes);

                return true;
            });
        }
    }
}
