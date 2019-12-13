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
using Microsoft.AspNetCore.ResponseCompression;
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
using System.IO;
using System.Linq;
using System.Reflection;

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

            try
            {
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

                            if (configuration.UseSwagger)
                            {
                                //Register the Swagger generator, defining 1 or more Swagger documents
                                services.AddSwaggerGen(c =>
                                {
                                    c.SwaggerDoc("v1", new OpenApiInfo 
                                    { 
                                        Title = $"IDBrowserService Core Site \"{strSiteName}\"", 
                                        Version = "v1",
                                        Description = "Webservice for IDBrowser Android app."
                                    });

                                    // Set the comments path for the Swagger JSON and UI.
                                    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                                    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                                    if (File.Exists(xmlPath))
                                        c.IncludeXmlComments(xmlPath);
                                });
                            }

                            if (configuration.UseResponseCompression)
                                services.AddResponseCompression(options =>
                                {
                                    options.Providers.Add<BrotliCompressionProvider>();
                                    options.Providers.Add<GzipCompressionProvider>();
                                    options.EnableForHttps = true;
                                });

                            services.AddDbContextPool<IDImagerDB>(options => StaticFunctions
                                .SetDbContextOptions(options, connectionStringSettings.DBType, connectionStringSettings.IDImager));
                            services.AddDbContextPool<IDImagerThumbsDB>(options => StaticFunctions
                                .SetDbContextOptions(options, connectionStringSettings.DBType, connectionStringSettings.IDImagerThumbs));
                        },
                        appBuilder =>
                        {
                            if (configuration.UseResponseCompression)
                                appBuilder.UseResponseCompression();

                            appBuilder.UseMvc();

                            if (configuration.UseSwagger)
                            {
                                // Enable middleware to serve generated Swagger as a JSON endpoint.
                                appBuilder.UseSwagger();

                                // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
                                // specifying the Swagger JSON endpoint.
                                appBuilder.UseSwaggerUI(c =>
                                {
                                    c.SwaggerEndpoint($"/{strSiteName}/swagger/v1/swagger.json", "IDBrowserServiceCore V1");
                                });
                            }
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
