using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseApiServices(this IApplicationBuilder app, IConfiguration configuration, IWebHostEnvironment env)
        {
            app.UseHsts();

            if (app.ApplicationServices.GetService(typeof(Serilog.Extensions.Hosting.DiagnosticContext)) != null)
            {
                app.UseSerilogRequestLogging();
            }

            if (configuration.GetValue<bool>("EnableSwagger"))
            {
                var routePrefix = string.Empty;
                app.UseSwagger(c =>
                {
                    c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                    {
                        swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer { Url = $"{httpReq.Scheme}://{httpReq.Host.Value}{routePrefix}" } };
                    });
                });
                app.UseSwaggerUI(options =>
                {
                    var assemblyName = Assembly.GetEntryAssembly().GetName().Name;
                    options.DocumentTitle = $"{assemblyName} - {env.EnvironmentName}";
                    options.SwaggerEndpoint($"{routePrefix}/swagger/v1/swagger.json", assemblyName + " API V1");

                    if (configuration.GetValue<string>("Auth0:ClientId") != null)
                    {
                        var clientId = configuration.GetValue<string>("Auth0:ClientId");
                        options.OAuthClientId(clientId);
                        var audience = configuration.GetValue<string>("Auth0:Audience");
                        options.OAuthAdditionalQueryStringParams(new Dictionary<string, string> { { "audience", audience } });
                    }
                });
            }

            app.UseCors(builder => {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
