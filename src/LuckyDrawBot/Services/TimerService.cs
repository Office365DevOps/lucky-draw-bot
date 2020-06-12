using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace LuckyDrawBot.Services
{
    public interface ITimerService
    {
        Task AddScheduledHttpRequest(DateTimeOffset invokeTime, string httpMethod, string httpUrl);
    }

    public class TimerService : ITimerService
    {
        private class AddScheduledJobRequest
        {
            public string CallbackTime { get; set; }
            public string CallbackMethod { get; set; }
            public string CallbackUrl { get; set; }
        }

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly Uri _callbackBaseUrl;

        public TimerService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
        {
            _callbackBaseUrl = new Uri(configuration.GetValue<string>("ServicePublicBaseUrl"));
            _httpClientFactory = httpClientFactory;
        }

        public async Task AddScheduledHttpRequest(DateTimeOffset invokeTime, string httpMethod, string httpRelativeUrl)
        {
            using (var client = _httpClientFactory.CreateClient("Timer"))
            {
                var requestBody = new AddScheduledJobRequest
                {
                    CallbackTime = invokeTime.ToString("u"),
                    CallbackMethod = httpMethod,
                    CallbackUrl = new Uri(_callbackBaseUrl, httpRelativeUrl).ToString()
                };
                var json = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(string.Empty, content);
                response.EnsureSuccessStatusCode();
            }
        }
    }
}
