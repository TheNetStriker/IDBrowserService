using IDBrowserServiceCore.Code;
using IDBrowserServiceCore.Data;
using IDBrowserServiceCore.Data.IDImager;
using IDBrowserServiceCore.Data.IDImagerThumbs;
using IDBrowserServiceCore.Data.PostgresHelpers;
using IDBrowserServiceCore.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Extensions.Hosting;
using System;
using System.Collections.Generic;

namespace IDBrowserServiceCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostEnvironment env, ILogger<Startup> logger)
        {
            IDBrowserConfiguration configuration = new IDBrowserConfiguration();
            Configuration.Bind(configuration);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();

            if (configuration.UseResponseCompression)
                app.UseResponseCompression();

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "IDBrowserServiceCore V1");
            });

            try
            {
                IEnumerable<IConfigurationSection> sites = Configuration.GetSection("Sites").GetChildren();

                foreach (KeyValuePair<string, SiteSettings> siteKeyValuePair in configuration.Sites)
                {
                    string strSiteName = siteKeyValuePair.Key;
                    SiteSettings siteSettings = siteKeyValuePair.Value;
                    ConnectionStringSettings connectionStringSettings = siteSettings.ConnectionStrings;
                    ServiceSettings serviceSettings = siteSettings.ServiceSettings;

                    //https://www.strathweb.com/2017/04/running-multiple-independent-asp-net-core-pipelines-side-by-side-in-the-same-application/
                    app.UseBranchWithServices("/" + strSiteName,
                        services =>
                        {
                            services
                                .AddMvc(option => option.EnableEndpointRouting = false)
                                .AddXmlSerializerFormatters();

                            services.AddSingleton<ServiceSettings>(serviceSettings);

                            // Register the Swagger generator, defining 1 or more Swagger documents
                            services.AddSwaggerGen(c =>
                            {
                                c.SwaggerDoc("v1", new OpenApiInfo { Title = "IDBrowserServiceCore", Version = "v1" });
                            });

                            if (configuration.UseResponseCompression)
                                services.AddResponseCompression();

                            services.AddDbContextPool<IDImagerDB>(options => StaticFunctions
                                .SetDbContextOptions(options, connectionStringSettings.IDImager, connectionStringSettings.IDImager));
                            services.AddDbContextPool<IDImagerThumbsDB>(options => StaticFunctions
                                .SetDbContextOptions(options, connectionStringSettings.IDImager, connectionStringSettings.IDImagerThumbs));
                        },
                        appBuilder =>
                        {
                            appBuilder.UseMvc();
                        });
                }

                app.Run(async c =>
                {
                    await c.Response.WriteAsync("Service online!");
                });
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
        }
    }
}
