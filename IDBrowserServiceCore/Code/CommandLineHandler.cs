using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IDBrowserServiceCore.Settings;
using Microsoft.Extensions.Configuration;
using Serilog.Events;

namespace IDBrowserServiceCore.Code
{
    public static class CommandLineHandler
    {
        public static IConfiguration Configuration { get; } = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

        public static void Handle(string[] args)
        {
            IDBrowserConfiguration configuration = new IDBrowserConfiguration();
            Configuration.Bind(configuration);

            if (args[0] == nameof(ConsoleFunctions.TranscodeAllVideos))
            {
                if (args.Count() < 3)
                {
                    Console.WriteLine("Please specify the site name, video size (e.g. Hd480, Hd720, Hd1080), number of FFmpeg instances (optional, default 2, max 12) and log level (optional, default Error, possible values Verbose, Debug, Information, Warning, Error and Fatal) to transcode videos.");
                }
                else
                {
                    string strSiteName = args[1];
                    string strVideoSize = args[2];
                    int taskCount = 2;
                    Serilog.Events.LogEventLevel logLevel = LogEventLevel.Error;

                    if (args.Count() > 3)
                    {
                        taskCount = Math.Min(12, Math.Max(1, int.Parse(args[3])));
                    }

                    if (args.Count() > 4)
                    {
                        logLevel = (LogEventLevel)Enum.Parse(typeof(LogEventLevel), args[4], true);
                    }

                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                    Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
                    {
                        cancellationTokenSource.Cancel();
                        e.Cancel = true;
                    };

                    ConsoleFunctions.TranscodeAllVideos(configuration, cancellationTokenSource.Token, strSiteName,
                        strVideoSize, logLevel, taskCount);
                    Console.WriteLine("Transcoding complete");
                }
            }
            else if (args[0] == nameof(ConsoleFunctions.GenerateThumbnails))
            {
                if (args.Count() < 2)
                {
                    Console.WriteLine("Please specify the site name");
                }
                else
                {
                    string strSiteName = args[1];

                    Console.WriteLine("Please enter optional from datetime: ");
                    DateTime.TryParse(Console.ReadLine(), out DateTime fromDateTime);

                    Console.WriteLine("Please enter optional to datetime: ");
                    DateTime.TryParse(Console.ReadLine(), out DateTime toDateTime);

                    Console.WriteLine("Please enter optional file filter: ");
                    string strFileFilter = Console.ReadLine();

                    Console.WriteLine("Please enter optional image guid: ");
                    string strImageGuid = Console.ReadLine();

                    Console.WriteLine("Please enter 'true' when thumbnails should be overwritten: ");
                    bool.TryParse(Console.ReadLine(), out bool overwrite);

                    Console.WriteLine("Generating thumbnails...");
                    ConsoleFunctions.GenerateThumbnails(configuration, strSiteName, fromDateTime,
                        toDateTime, strFileFilter, strImageGuid, overwrite);
                }
            }
            else
            {
                Console.WriteLine("Command not recognized. Possible Commands:");
                Console.WriteLine("TranscodeAllVideos");
            }
        }
    }
}
