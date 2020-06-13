using LuckyDrawBot.Infrastructure;
using LuckyDrawBot.Infrastructure.Database;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        static ServiceCollectionExtensions()
        {
            // Workaround: https://github.com/dotnet/runtime/issues/31094#issuecomment-543342051
            var jsonSerializerOptions = (JsonSerializerOptions)typeof(JsonSerializerOptions)
                .GetField("s_defaultOptions", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                .GetValue(null);
            jsonSerializerOptions.PropertyNameCaseInsensitive = true;
            jsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            jsonSerializerOptions.IgnoreNullValues = true;
            jsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            jsonSerializerOptions.Converters.Add(new DateTimeConverter());
        }

        public static void AddApiServices(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment env)
        {
            JsonConvert.DefaultSettings = (() =>
            {
                var settings = new JsonSerializerSettings();
                settings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                settings.NullValueHandling = NullValueHandling.Ignore;
                settings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                settings.DateTimeZoneHandling = DateTimeZoneHandling.Utc;
                settings.Converters.Add(new StringEnumConverter { NamingStrategy = new CamelCaseNamingStrategy() });
                return settings;
            });

            services.AddMemoryCache();
            services.AddHttpContextAccessor();

            var httpClientConfiguration = configuration.GetSection("HttpClientFactory");
            foreach (var clientConfiguration in httpClientConfiguration.GetChildren())
            {
                var apiName = clientConfiguration.Key;
            services.AddHttpClient(apiName, (sv, client) =>
            {
                client.BaseAddress = new Uri(clientConfiguration["BaseAddress"]);
                var authenticationSection = clientConfiguration.GetSection("Authentication");
                    if (authenticationSection.GetChildren().Any())
                    {
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
                            authenticationSection["Scheme"],
                            authenticationSection["Parameter"]);
                    }
                });
            }

            services.AddCors();

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Latest)
                .AddJsonOptions(opt =>
                {
                    opt.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
                    opt.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                    opt.JsonSerializerOptions.IgnoreNullValues = true;
                    opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
                    opt.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
                });

            if (configuration.GetValue<bool>("EnableSwagger"))
            {
                services.AddSwaggerGen(options =>
                {
                    options.SwaggerDoc("v1", new OpenApiInfo { Title = Assembly.GetEntryAssembly().GetName().Name, Version = "1" });
                    options.EnableAnnotations();
                    options.CustomSchemaIds(x => x.Name);
                });
            }

            services.AddSingleton(typeof(IDataTable<,>), typeof(DataTable<,>));
        }

        public static void AddApplicationPart(this IServiceCollection services, Assembly assembly)
        {
            var managerService = services.FirstOrDefault(s => s.ServiceType == typeof(ApplicationPartManager));
            if (managerService != null)
            {
                var applicationParts = ((ApplicationPartManager)managerService.ImplementationInstance).ApplicationParts;
                var exists = applicationParts.Any(p => (p is AssemblyPart) && ((AssemblyPart)p).Assembly == assembly);
                if (!exists)
                {
                    var part = new AssemblyPart(assembly);
                    applicationParts.Add(part);
                }
            }
        }

        public static IServiceCollection ReplaceSingleton<TService, TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService
        {
            var service = services.First(s => s.ServiceType == typeof(TService));
            services.Remove(service);
            return services.AddSingleton<TService, TImplementation>();
        }

        public static IServiceCollection ReplaceSingleton(this IServiceCollection services, Type serviceType, Type implementationType)
        {
            var service = services.First(s => s.ServiceType == serviceType);
            services.Remove(service);
            return services.AddSingleton(serviceType, implementationType);
        }

        public static IServiceCollection ReplaceSingleton(this IServiceCollection services, Type serviceType, object implementationInstance)
        {
            var service = services.First(s => s.ServiceType == serviceType);
            services.Remove(service);
            return services.AddSingleton(serviceType, implementationInstance);
        }

        public static IServiceCollection ReplaceScoped<TService>(this IServiceCollection services, Func<IServiceProvider, TService> implementationFactory) where TService : class
        {
            var service = services.First(s => s.ServiceType == typeof(TService));
            services.Remove(service);
            return services.AddScoped(implementationFactory);
        }

        public static IServiceCollection ReplaceScoped<TService, TImplementation>(this IServiceCollection services) where TService : class where TImplementation : class, TService
        {
            var service = services.First(s => s.ServiceType == typeof(TService));
            services.Remove(service);
            return services.AddScoped<TService, TImplementation>();
        }

    }
}
