using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace LuckyDrawBot.Services
{
    public interface IBotValidator
    {
        Task<(bool IsAuthenticated, string ErrorMessage)> Validate(HttpRequest request);
    }

    public class BotValidator : IBotValidator
    {
        private readonly IConfiguration _configuration;

        public BotValidator(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<(bool IsAuthenticated, string ErrorMessage)> Validate(HttpRequest request)
        {
            if (!request.Headers.TryGetValue("Authorization", out StringValues authorizationHeader))
            {
                return (false, "Missing 'Authorization' header");
            }

            var credential = new SimpleCredentialProvider(
                _configuration.GetValue<string>("Bot:Id"),
                _configuration.GetValue<string>("Bot:Password"));
            var claimsIdentity = await JwtTokenValidation.ValidateAuthHeader(authorizationHeader, credential, null, null);
            if (!claimsIdentity.IsAuthenticated)
            {
                return (false, "The request fails to pass auth check.");
            }

            return (true, string.Empty);
        }
    }
}