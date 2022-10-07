using IDBrowserServiceCore.Jobs;
using IDBrowserServiceCore.Settings;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using System;

namespace IDBrowserServiceCore.Installers
{
    /// <summary>
    /// Installer for cron scheduler.
    /// </summary>
    public static class CronSchedulerInstaller
    {
        private static int _schedulerCounter;

        /// <summary>
        /// Add's cron scheduler services.
        /// </summary>
        /// <param name="services">IServiceCollection</param>
        /// <param name="serviceSettings">ServiceSettings</param>
        /// <returns>IServiceCollection</returns>
        public static IServiceCollection AddCronSchedulerServices(this IServiceCollection services,
            ServiceSettings serviceSettings, string name)
        {
            _schedulerCounter += 1;

            services.AddQuartz(q =>
            {
                q.SchedulerName = $"Scheduler_{name}";

                q.UseMicrosoftDependencyInjectionJobFactory();

                if (serviceSettings.EnableDatabaseCache)
                {
                    q.AddJobAndTrigger<UpdateDatabaseCacheJob>(serviceSettings.CronJobs.UpdateDatabaseCacheJob, name);
                }
            });

            // ASP.NET Core hosting
            services.AddQuartzHostedService(options =>
            {
                // when shutting down we shutdown all jobs.
                options.WaitForJobsToComplete = false;
            });

            return services;
        }

        /// <summary>
        /// Add's a cron job.
        /// </summary>
        /// <typeparam name="T">IJob</typeparam>
        /// <param name="quartz">IServiceCollectionQuartzConfigurator</param>
        /// <param name="cronSchedule">Cron schedule</param>
        /// <exception cref="Exception"></exception>
        public static void AddJobAndTrigger<T>(this IServiceCollectionQuartzConfigurator quartz, string cronSchedule, string name) where T : IJob
        {
            // Use the name of the IJob as the appsettings.json key
            string jobName = $"{typeof(T).Name}_" + name;

            // Some minor validation
            if (string.IsNullOrEmpty(cronSchedule))
            {
                throw new Exception($"No Quartz.NET Cron schedule found for job in configuration");
            }

            // register the job as before
            var jobKey = new JobKey(jobName);
            quartz.AddJob<T>(opts => opts.WithIdentity(jobKey));

            quartz.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity(jobName + "-cron-trigger")
                .StartAt(DateTimeOffset.Now.AddMinutes(5))
                .WithCronSchedule(cronSchedule)); // use the schedule from configuration

            // Single time trigger after app startup
            quartz.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity(jobName + "-startup-trigger")
                .StartAt(DateTimeOffset.Now.AddSeconds(_schedulerCounter * 60))
                .WithSimpleSchedule(x => x.WithIntervalInSeconds(0)));
        }
    }
}
