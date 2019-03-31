using System.Reflection;
using LuckyDrawBot.Infrastructure.Azure;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;

namespace LuckyDrawBot
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    var keyVaultConfig = config.Build().GetSection("Azure:KeyVault").Get<KeyVaultConfig>();
                    if(keyVaultConfig == null || string.IsNullOrEmpty(keyVaultConfig.KeyVaultName))
                    {
                        return;
                    }

                    var assemblyName = Assembly.GetEntryAssembly().GetName();
                    var prefix = $"{assemblyName.Name}--{assemblyName.Version.Major}";
                    var keyVaultConfigBuilder = new ConfigurationBuilder();
                    keyVaultConfigBuilder.AddAzureKeyVault(
                        $"https://{keyVaultConfig.KeyVaultName}.vault.azure.net/",
                        keyVaultConfig.ClientId,
                        keyVaultConfig.ClientSecret,
                        new PrefixKeyVaultSecretManager(prefix));

                    config.AddConfiguration(keyVaultConfigBuilder.Build());
                })
                .UseSerilog((context, logger) =>
                {
                    var applicationInsightsKey = context.Configuration.GetValue<string>("ApplicationInsights:InstrumentationKey");
                    logger.Enrich.FromLogContext()
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                        .WriteTo.ApplicationInsights(applicationInsightsKey, TelemetryConverter.Events)
                        .WriteTo.Console();
                })
                .UseAzureAppServices()
                .UseApplicationInsights()
                .UseStartup<Startup>();
    }
}
