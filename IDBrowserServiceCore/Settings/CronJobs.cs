namespace IDBrowserServiceCore.Settings
{
    /// <summary>
    /// Cron job settings
    /// </summary>
    public class CronJobs
    {
        /// <summary>
        /// Cron expression to update database cache. (Default is every 15 Minutes "0 */15 * * * ?")
        /// Cache is only updated if new image properties are added/deleted or images get a new image property assigned or removed.
        /// </summary>
        public string UpdateDatabaseCacheJob { get; set; }

        /// <summary>
        /// Set's default settings.
        /// </summary>
        public CronJobs()
        {
            // Default values
            UpdateDatabaseCacheJob = "0 */15 * * * ?";
        }
    }
}
