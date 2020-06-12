using System.Reflection;
using LuckyDrawBot.Infrastructure.Azure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace LuckyDrawBot
{
    public static class Program
    {
        public static void Main()
        {
            CreateWebHostBuilder(null).Build().Run();
        }

        public static IHostBuilder CreateWebHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .ConfigureServices(services =>
                        {
                            services.AddApplicationInsightsTelemetry();
                        })
                        .ConfigureAppConfiguration((context, config) =>
                        {
                            var keyVaultName = config.Build().GetValue<string>("KeyVaultName");
                            if (string.IsNullOrEmpty(keyVaultName))
                            {
                                return;
                            }

                            var assemblyName = Assembly.GetEntryAssembly().GetName();
                            var prefix = $"{assemblyName.Name}--{assemblyName.Version.Major}";

                            var azureServiceTokenProvider = new AzureServiceTokenProvider();
                            var keyVaultClient = new KeyVaultClient(new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback));
                            var keyVaultConfigBuilder = new ConfigurationBuilder();
                            keyVaultConfigBuilder.AddAzureKeyVault(
                                $"https://{keyVaultName}.vault.azure.net/",
                                keyVaultClient,
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
                        .UseStartup<Startup>();
                });
    }
}
