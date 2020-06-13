using HealthChecks.UI.Client;
using LuckyDrawBot.Handlers;
using LuckyDrawBot.Models;
using LuckyDrawBot.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LuckyDrawBot
{
    public class Startup
    {
        private readonly IWebHostEnvironment _env;

        public Startup(IConfiguration configuration, IWebHostEnvironment env)
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
                .AddAssemblyVersion();

            services.AddSingleton(Configuration.GetSection("Bot").Get<BotSettings>());
            services.Configure<LuckyDrawDataTablesSettings>(Configuration.GetSection("DataTable"));
            services.AddSingleton<IDateTimeService, DateTimeService>();
            services.AddSingleton<IRandomService, RandomService>();
            services.AddSingleton<ITimerService, TimerService>();
            services.AddSingleton<IBotClientFactory, BotClientFactory>();
            services.AddSingleton<ICompetitionRepositoryService, CompetitionRepositoryService>();
            services.AddSingleton<ICompetitionService, CompetitionService>();
            services.AddSingleton<IActivityBuilder, ActivityBuilder>();
            services.AddSingleton<ILocalizationFactory, LocalizationFactory>();
            services.AddSingleton<IBotValidator, BotValidator>();

            services.AddSingleton<BotHandlers>();
            services.AddTransient<NonTeamsChannelHandler>();
            services.AddTransient<HelpCommandHandler>();
            services.AddTransient<AddedToNewChannelHandler>();
            services.AddTransient<UnknownCommandHandler>();
            services.AddTransient<CreateDraftCompetitionCommandHandler>();
            services.AddTransient<CreateCompetitionCommandHandler>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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
