using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using IDBrowserServiceCore.Settings;
using Microsoft.Extensions.Configuration;

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

            if (args[0] == nameof(StaticFunctions.TranscodeAllVideos))
            {
                if (args.Count() < 3)
                {
                    Console.WriteLine("Please specify the site name and video size to transcode videos. (e.g. Hd480, Hd720, Hd1080)");
                }
                else
                {
                    string strSiteName = args[1];
                    string strVideoSize = args[2];

                    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

                    Console.CancelKeyPress += (object sender, ConsoleCancelEventArgs e) =>
                    {
                        cancellationTokenSource.Cancel();
                        e.Cancel = true;
                    };

                    Console.WriteLine("Transcoding videos...");
                    StaticFunctions.TranscodeAllVideos(configuration, cancellationTokenSource.Token, strSiteName, strVideoSize);
                }
            }
            else if (args[0] == nameof(StaticFunctions.GenerateThumbnails))
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
                    StaticFunctions.GenerateThumbnails(configuration, strSiteName, fromDateTime,
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
