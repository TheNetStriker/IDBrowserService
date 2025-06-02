using IDBrowserServiceCore.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace IDBrowserServiceCore.Services
{
    /// <summary>
    /// Database cache service interface
    /// </summary>
    public interface IDatabaseCache
    {
        /// <summary>
        /// Get v_PropCategory Cache
        /// </summary>
        public ValueTask<List<ImageProperty>> GetVPropCategoryCacheAsync();

        /// <summary>
        /// Get v_prop Cache
        /// </summary>
        public ValueTask<List<ImageProperty>> GetVPropCacheAsync();

        /// <summary>
        /// Set v_PropCategory Cache
        /// </summary>
        public ValueTask SetVPropCategoryCacheAsync(List<ImageProperty> value);

        /// <summary>
        /// Set v_prop Cache
        /// </summary>
        public ValueTask SetVPropCacheAsync(List<ImageProperty> value);

        /// <summary>
        /// Checks if the cache needs updating.
        /// </summary>
        /// <returns>Task</returns>
        public Task CheckAndUpdateCacheAsync();
    }
}
