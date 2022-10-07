using IDBrowserServiceCore.Services;
using Microsoft.Extensions.Logging;
using Quartz;
using System;
using System.Threading.Tasks;

namespace IDBrowserServiceCore.Jobs
{
    /// <summary>
    /// Update database cache job
    /// </summary>
    [DisallowConcurrentExecution]
    public class UpdateDatabaseCacheJob : IJob
    {
        private readonly ILogger<UpdateDatabaseCacheJob> logger;
        private readonly IDatabaseCache databaseCache;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="logger">Logger</param>
        /// <param name="databaseCache">Database cache</param>
        public UpdateDatabaseCacheJob(ILogger<UpdateDatabaseCacheJob> logger, IDatabaseCache databaseCache)
        {
            this.logger = logger;
            this.databaseCache = databaseCache;
        }

        /// <summary>
        /// Job execution
        /// </summary>
        /// <param name="context">IJobExecutionContext</param>
        /// <returns>Task</returns>
        public async Task Execute(IJobExecutionContext context)
        {
            try
            {
                logger.LogInformation($"Job {nameof(UpdateDatabaseCacheJob)} started by trigger: {context.Trigger.Key.Name}");
                await databaseCache.CheckAndUpdateCacheAsync();
                logger.LogInformation($"Job {nameof(UpdateDatabaseCacheJob)} finished by trigger: {context.Trigger.Key.Name}");
            }
            catch (Exception ex)
            {
                logger.LogError(ex.ToString());
            }
        }
    }
}
