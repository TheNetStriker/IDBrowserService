using IDBrowserServiceCore.Code;
using IDBrowserServiceCore.Settings;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace IDBrowserServiceCore
{
    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        public static void Main(string[] args)
        {
            if (args.Count() > 0 && !args[0].Equals("%LAUNCHER_ARGS%"))
            {
                CommandLineHandler.Handle(args);
            }
            else
            {
                CreateWebHostBuilder(args).Build().Run();
            }
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            return WebHost.CreateDefaultBuilder(args)
                .ConfigureKestrel(serverOptions =>
                {
                    serverOptions.ListenAnyIP(5000);
                })
                .UseIISIntegration()
                .UseUrls(Configuration.GetValue<string>("Urls"))
                .UseStartup<Startup>()
                .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                    .ReadFrom.Configuration(Configuration)
                    .Enrich.FromLogContext());
        }
    }
}
