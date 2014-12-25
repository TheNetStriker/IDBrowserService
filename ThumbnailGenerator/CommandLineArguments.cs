using CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ThumbnailGenerator
{
    class CommandLineArguments
    {
        [Option(null, "from", HelpText = "Check from created date")]
        public DateTime FromDateTime { get; set; }

        [Option(null, "to", HelpText = "Check to created date")]
        public DateTime ToDateTime { get; set; }

        [Option(null, "filter", HelpText = "File filter")]
        public String FileFilter { get; set; }

        [Option(null, "imageguid", HelpText = "Generate specific image guid thumbnails")]
        public String ImageGuid { get; set; }

        [Option(null, "overwrite", HelpText = "Overwrite existring preview pictures")]
        public Boolean Overwrite { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var usage = new StringBuilder();
            usage.AppendLine("ThumbnailGenerator");
            return usage.ToString();
        }
    }
}
