using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LuckyDrawBot.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Schema;

namespace LuckyDrawBot.Tests.MockServices
{
    public class SimpleBotValidator : IBotValidator
    {
        public bool IsAuthenticated { get; set; } = true;

        public async Task<(bool IsAuthenticated, string ErrorMessage)> Validate(HttpRequest request)
        {
            var result = (IsAuthenticated, IsAuthenticated ? string.Empty : "Failed to pass authentication check");
            return await Task.FromResult(result);
        }
    }
}