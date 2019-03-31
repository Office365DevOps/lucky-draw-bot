using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.AspNetCore.Builder
{
    public static class ApplicationBuilderExtensions
    {
        public static void UseApiServices(this IApplicationBuilder app, IConfiguration configuration, IHostingEnvironment env)
        {
            if (env.IsDevelopment() || env.IsTest())
            {
                app.UseDeveloperExceptionPage();
            }

            if (env.IsDevelopment() || env.IsTest() || env.IsStaging())
            {
                var routePrefix = string.Empty;
                app.UseSwagger(c =>
                {
                    c.PreSerializeFilters.Add((swaggerDoc, httpReq) => swaggerDoc.Host = $"{httpReq.Host.Value}{routePrefix}");
                });
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint($"{routePrefix}/swagger/v1/swagger.json", Assembly.GetEntryAssembly().GetName().Name + " API V1");
                });
            }

            if (env.IsStaging() || env.IsProduction())
            {
                app.UseHsts();
            }

            app.UseCors(builder => {
                builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
            app.UseMvc();
        }
    }
}
