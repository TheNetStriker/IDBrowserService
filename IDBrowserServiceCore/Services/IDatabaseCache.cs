using IDBrowserServiceCore.Data;
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
        /// v_PropCategory Cache
        /// </summary>
        public List<ImageProperty> VPropCategoryCache { get; set; }

        /// <summary>
        /// v_prop Cache
        /// </summary>
        public List<ImageProperty> VPropCache { get; set; }

        /// <summary>
        /// Checks if the cache needs updating.
        /// </summary>
        public Task CheckAndUpdateCacheAsync();
    }
}
