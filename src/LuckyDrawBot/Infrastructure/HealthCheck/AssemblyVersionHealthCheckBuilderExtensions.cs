using Microsoft.Extensions.Diagnostics.HealthChecks;
using LuckyDrawBot.Infrastructure.HealthCheck;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class AssemblyVersionHealthCheckBuilderExtensions
    {
        public static IHealthChecksBuilder AddAssemblyVersion(this IHealthChecksBuilder builder)
        {
            return builder.Add(new HealthCheckRegistration("Assembly Version", sp => new AssemblyVersionHealthCheck(), HealthStatus.Healthy, new string[0]));
        }
    }
}