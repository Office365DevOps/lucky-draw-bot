using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace LuckyDrawBot.Services
{
    public interface ITeamsAuthProvider
    {
        bool Validate(HttpRequest request, out string errorMessage);
    }

    public class TeamsAuthProvider : ITeamsAuthProvider
    {
        private readonly ILogger<TeamsAuthProvider> _logger;
        private readonly string _securityToken;

        public TeamsAuthProvider(ILogger<TeamsAuthProvider> logger, IConfiguration configuration)
        {
            _logger = logger;
            _securityToken = configuration.GetValue<string>("TeamsAppSecurityToken");
        }

        public bool Validate(HttpRequest request, out string errorMessage)
        {
            request.Body.Seek(0, SeekOrigin.Begin);
            string messageContent = new StreamReader(request.Body).ReadToEnd();
            var authenticationHeaderValue = request.Headers["Authorization"];

            if (authenticationHeaderValue.Count <= 0)
            {
                errorMessage = "Authentication header not present on request.";
                return false;
            }

            if (!authenticationHeaderValue[0].StartsWith("HMAC"))
            {
                errorMessage = "Incorrect authorization header scheme.";
                return false;
            }

            if (string.IsNullOrEmpty(messageContent))
            {
                errorMessage = "Unable to validate authentication header for messages with empty body.";
                return false;
            }

            string providedHmacValue = authenticationHeaderValue[0].Substring("HMAC".Length).Trim();
            string calculatedHmacValue = null;
            try
            {
                byte[] serializedPayloadBytes = Encoding.UTF8.GetBytes(messageContent);

                byte[] keyBytes = Convert.FromBase64String(_securityToken);
                using (HMACSHA256 hmacSHA256 = new HMACSHA256(keyBytes))
                {
                    byte[] hashBytes = hmacSHA256.ComputeHash(serializedPayloadBytes);
                    calculatedHmacValue = Convert.ToBase64String(hashBytes);
                }

                if (string.Equals(providedHmacValue, calculatedHmacValue))
                {
                    errorMessage = string.Empty;
                    return true;
                }
                else
                {
                    errorMessage = string.Format(
                        "AuthHeaderValueMismatch. Expected:'{0}' Provided:'{1}'",
                        calculatedHmacValue,
                        providedHmacValue);
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Exception occurred while verifying HMAC on the incoming request.");
                errorMessage = "Exception thrown while verifying MAC on incoming request.";
                return false;
            }
        }
    }
}
