using IDBrowserServiceCore.Code;
using Serilog;
using Serilog.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDBrowserServiceCore.Logging
{
    public static class LoggingExtensions
    {
        public static LoggerConfiguration WithEmptyEnricher(
            this LoggerEnrichmentConfiguration enrich)
        {
            if (enrich == null)
                throw new ArgumentNullException(nameof(enrich));

            return enrich.With<EmptyEnricher>();
        }
    }
}
