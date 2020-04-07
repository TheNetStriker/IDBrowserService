using IDBrowserServiceCore.Code;
using IDBrowserServiceCore.Settings;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using System;
using System.IO;
using System.Linq;

namespace IDBrowserServiceCore
{
    public class Program
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("sites.json", optional: true, reloadOnChange: true)
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
            // BASEDIR variable for Serilog path
            Environment.SetEnvironmentVariable("BASEDIR", AppDomain.CurrentDomain.BaseDirectory);

            return WebHost.CreateDefaultBuilder(args)
                .UseConfiguration(Configuration)
                .UseIISIntegration()
                .UseUrls(Configuration.GetValue<string>(nameof(IDBrowserConfiguration.Urls)))
                .UseStartup<Startup>()
                .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                    .ReadFrom.Configuration(Configuration));
        }
    }
}
