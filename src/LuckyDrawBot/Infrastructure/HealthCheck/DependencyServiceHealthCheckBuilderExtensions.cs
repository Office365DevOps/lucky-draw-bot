using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyServiceHealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddDependencyService(this IHealthChecksBuilder builder, IConfiguration configurationRoot, string httpClientName)
        {
            var dependencyServiceBaseUrl = configurationRoot.GetValue<string>($"HttpClientFactory:{httpClientName}:BaseAddress");
            if (!string.IsNullOrEmpty(dependencyServiceBaseUrl))
            {
                builder.AddUrlGroup(new Uri(dependencyServiceBaseUrl + "/healthcheck"), $"{httpClientName} service", HealthStatus.Unhealthy);
            }
            return builder;
        }
    }
}
