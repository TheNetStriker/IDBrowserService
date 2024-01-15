using System.IO;
using IDBrowserServiceCore.Code;
using IDBrowserServiceCore.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using IDBrowserServiceCore.Data.IDImagerThumbs;
using IDBrowserServiceCore.Installers;
using IDBrowserServiceCore.Services;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using System.Collections.Generic;
using System.Reflection;
using System;
using IDBrowserServiceCore.Data.IDImager;

Environment.SetEnvironmentVariable("BASEDIR", AppDomain.CurrentDomain.BaseDirectory);

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

if (args.Length > 0 && !args[0].Equals("%LAUNCHER_ARGS%"))
{
    CommandLineHandler.Handle(args);
}
else
{
    IConfiguration Configuration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
        .AddJsonFile("sites.json", optional: true, reloadOnChange: true)
        .AddEnvironmentVariables()
        .Build();

    var builder = WebApplication.CreateBuilder(args);

    builder.WebHost.UseConfiguration(Configuration);
    builder.WebHost.UseUrls(Configuration.GetValue<string>(nameof(IDBrowserConfiguration.Urls)));
    builder.Host.UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
        .ReadFrom.Configuration(Configuration));

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }

    app.UseSerilogRequestLogging();

    IDBrowserConfiguration configuration = new();
    Configuration.Bind(configuration);
    
    try
    {
        if (configuration.Sites != null)
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

                        services.AddSingleton<IDatabaseCache, DatabaseCache>();
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

                        services.AddCronSchedulerServices(serviceSettings, strSiteName);
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
        }
    }
    catch (Exception ex)
    {
        Log.Error(ex.ToString());
    }

    app.MapGet("/", () => "Service online!");

    app.Run();
}