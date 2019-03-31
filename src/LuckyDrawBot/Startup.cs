using HealthChecks.UI.Client;
using LuckyDrawBot.Models;
using LuckyDrawBot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuckyDrawBot
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;

        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApiServices(Configuration, _env);

            services
                .AddHealthChecks()
                .AddDependencyService(Configuration, "LuckyDrawBot")
                .AddAssemblyVersion();

            services.AddSingleton(Configuration.GetSection("Bot").Get<BotSettings>());
            services.AddSingleton<IDateTimeService, DateTimeService>();
            services.AddSingleton<IRandomService, RandomService>();
            services.AddSingleton<ICompetitionRepositoryService, CompetitionRepositoryService>();
            services.AddSingleton<ICompetitionService, CompetitionService>();
            services.AddSingleton<IActivityBuilder, ActivityBuilder>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseHealthChecks("/healthcheck", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });

            app.UseApiServices(Configuration, _env);
        }
    }
}
