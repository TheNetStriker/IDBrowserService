using IDBrowserServiceCore.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using IDBrowserServiceCore.Data.IDImager;
using IDBrowserServiceCore.Data.IDImagerThumbs;
using IDBrowserServiceCore.Code;
using System.Collections.Generic;
using IDBrowserServiceCore.Settings;
using Microsoft.EntityFrameworkCore.Storage;
using IDBrowserServiceCore.Data.PostgresHelpers;
using System;

namespace IDBrowserServiceCore
{
    public class Startup
    {
        private ILogger log;

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.AddLog4Net();
            log = loggerFactory.CreateLogger("Startup");

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
                            .AddMvc()
                            .AddXmlSerializerFormatters();

                        services
                            .AddSingleton<IConfiguration>(Configuration);

                        services.Configure<ServiceSettings>(site.GetSection("ServiceSettings"));

                        var strDbType = site["ConnectionStrings:DBType"];

                        if (strDbType.Equals("MsSql"))
                        {
                            var strConnection = site["ConnectionStrings:IDImager"];
                            services.AddDbContextPool<IDImagerDB>(options => options.UseSqlServer(strConnection));

                            var strConnectionThumbs = site["ConnectionStrings:IDImagerThumbs"];
                            services.AddDbContextPool<IDImagerThumbsDB>(options => options.UseSqlServer(strConnectionThumbs));
                        }
                        else if (strDbType.Equals("Postgres"))
                        {
                            var strConnection = site["ConnectionStrings:IDImager"];
                            services.AddDbContextPool<IDImagerDB>(options => options
                                .UseNpgsql(strConnection)
                                .ReplaceService<ISqlGenerationHelper, PostgresSqlGenerationHelper>());

                            var strConnectionThumbs = site["ConnectionStrings:IDImagerThumbs"];
                            services.AddDbContextPool<IDImagerThumbsDB>(options => options
                                .UseNpgsql(strConnectionThumbs)
                                .ReplaceService<ISqlGenerationHelper, PostgresSqlGenerationHelper>());
                        }
                        else
                        {
                            throw new System.Exception("DBType not supported, supported type are 'MsSql' and 'Postgres'.");
                        }
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
                log.LogError(ex.ToString());
            }
        }
    }
}
