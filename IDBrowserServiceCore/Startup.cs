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
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSerilogRequestLogging();

            try
            {
                IEnumerable<IConfigurationSection> sites = Configuration.GetSection("Sites").GetChildren();

                foreach (IConfigurationSection site in sites)
                {
                    //https://www.strathweb.com/2017/04/running-multiple-independent-asp-net-core-pipelines-side-by-side-in-the-same-application/
                    app.UseBranchWithServices("/" + site.Key,
                        services =>
                        {
                            services
                                .AddMvc(option => option.EnableEndpointRouting = false)
                                .AddXmlSerializerFormatters();

                            services.AddSingleton<IConfiguration>(Configuration);
                            services.Configure<ServiceSettings>(site.GetSection("ServiceSettings"));

                            var strDbType = site["ConnectionStrings:DBType"];
                            var strConnection = site["ConnectionStrings:IDImager"];
                            var strConnectionThumbs = site["ConnectionStrings:IDImagerThumbs"];

                            services.AddDbContextPool<IDImagerDB>(options => StaticFunctions
                                .SetDbContextOptions(options, strDbType, strConnection));
                            services.AddDbContextPool<IDImagerThumbsDB>(options => StaticFunctions
                                .SetDbContextOptions(options, strDbType, strConnectionThumbs));
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
