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
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.AddLog4Net();

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

                    var strConnection = site["ConnectionStrings:IDImager"];
                    services.AddDbContextPool<IDImagerDB>(options => options.UseSqlServer(strConnection));

                    var strConnectionThumbs = site["ConnectionStrings:IDImagerThumbs"];
                    services.AddDbContextPool<IDImagerThumbsDB>(options => options.UseSqlServer(strConnectionThumbs));
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
    }
}
