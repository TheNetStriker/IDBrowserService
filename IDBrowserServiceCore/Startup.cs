using IDBrowserServiceCore.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using IDBrowserServiceCore.Data.IDImager;
using IDBrowserServiceCore.Data.IDImagerThumbs;

namespace IDBrowserServiceCore
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            StaticFunctions.Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddMvc()
                .AddXmlSerializerFormatters();
            services
                .AddSingleton<IConfiguration>(Configuration);

            var strConnection = Configuration["ConnectionStrings:IDImager"];
            services.AddDbContextPool<IDImagerDB>(options => options.UseSqlServer(strConnection));

            var strConnectionThumbs = Configuration["ConnectionStrings:IDImagerThumbs"];
            services.AddDbContextPool<IDImagerThumbsDB>(options => options.UseSqlServer(strConnectionThumbs));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.AddLog4Net();
            app.UseMvc();
        }
    }
}
