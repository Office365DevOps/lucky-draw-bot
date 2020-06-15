using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace System.Net.Http
{
    public static class HttpContentExtensions
    {
        public static async Task<T> ReadAsAsync<T>(this HttpContent content)
        {
            var stream = await content.ReadAsStreamAsync();
            return await JsonSerializer.DeserializeAsync<T>(stream);
        }

        public static async Task<T> ReadAsWithNewtonsoftJsonAsync<T>(this HttpContent content)
        {
            var stream = await content.ReadAsStreamAsync();
            using (var reader = new StreamReader(stream))
            {
                var jsonString = await reader.ReadToEndAsync();
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonString);
            }
        }
    }
}
