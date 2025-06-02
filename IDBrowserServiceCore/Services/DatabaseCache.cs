using IDBrowserServiceCore.Data;
using IDBrowserServiceCore.Data.IDImager;
using IDBrowserServiceCore.Settings;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ZiggyCreatures.Caching.Fusion;

namespace IDBrowserServiceCore.Services
{
    /// <summary>
    /// Database cache service
    /// </summary>
    public class DatabaseCache : IDatabaseCache
    {
        private readonly IDImagerDB _db;
        private readonly IFusionCache _cache;

        private readonly string _vPropCategoryCacheKey;
        private readonly string _vPropCacheKey;
        private readonly FusionCacheEntryOptions _entryOptions;

        private int IdPropCount { get; set; }
        private int IdCatalogItemDefinitionCount { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">IDImagerDB</param>
        /// <param name="cache">IFusionCache</param>
        /// <param name="siteSettings">SiteSettings</param>
        public DatabaseCache(IDImagerDB db, IFusionCache cache, SiteSettings siteSettings)
        {
            _db = db;
            _cache = cache;

            _vPropCategoryCacheKey = $"{siteSettings.SiteName}:VPropCategoryCache";
            _vPropCacheKey = $"{siteSettings.SiteName}:VPropCache";

            _entryOptions = new FusionCacheEntryOptions
            {
                Duration = siteSettings.ServiceSettings.CronJobs.UpdateDatabaseCache_MemoryCacheExpiration,
                DistributedCacheDuration = siteSettings.ServiceSettings.CronJobs.UpdateDatabaseCache_DistributedCacheExpiration,
            };
        }

        /// <summary>
        /// Checks if the cache needs updating.
        /// </summary>
        public async Task CheckAndUpdateCacheAsync()
        {
            int currentIdPropCount = await _db.idProp.CountAsync();
            int currentidCatalogItemDefinitionCount = await _db.idCatalogItemDefinition.CountAsync();

            List<ImageProperty> vPropCategoryCache = await GetVPropCategoryCacheAsync();

            List<ImageProperty> vPropCache = await GetVPropCacheAsync();

            if (vPropCategoryCache == null
                || vPropCache == null
                || IdPropCount != currentIdPropCount
                || IdCatalogItemDefinitionCount != currentidCatalogItemDefinitionCount)
            {
                var queryVPropCategorySource = from tbl in _db.v_PropCategory
                                               where !tbl.CategoryName.Equals("Internal")
                                               orderby tbl.CategoryName
                                               select new ImageProperty
                                               {
                                                   GUID = tbl.GUID,
                                                   Name = tbl.CategoryName,
                                                   ImageCount = tbl.idPhotoCount,
                                                   SubPropertyCount = _db.idProp.Where(x => x.ParentGUID == tbl.GUID).Count()
                                               };

                var vPropCategory = await queryVPropCategorySource.ToListAsync();

                var queryVProp = from tbl in _db.v_prop
                                 orderby tbl.PropName
                                 select new ImageProperty
                                 {
                                     GUID = tbl.GUID,
                                     ParentGUID = tbl.ParentGUID,
                                     Name = tbl.PropName,
                                     ImageCount = tbl.idPhotoCount,
                                     SubPropertyCount = _db.idProp.Where(x => x.ParentGUID == tbl.GUID).Count()
                                 };

                var vprop = await queryVProp.ToListAsync();

                await SetVPropCategoryCacheAsync(vPropCategory);
                await SetVPropCacheAsync(vprop);

                IdPropCount = currentIdPropCount;
                IdCatalogItemDefinitionCount = currentidCatalogItemDefinitionCount;
            }
        }

        public ValueTask<List<ImageProperty>> GetVPropCategoryCacheAsync()
        {
            return _cache.GetOrDefaultAsync<List<ImageProperty>>(_vPropCategoryCacheKey, null);
        }

        public ValueTask<List<ImageProperty>> GetVPropCacheAsync()
        {
            return _cache.GetOrDefaultAsync<List<ImageProperty>>(_vPropCacheKey, null);
        }

        public ValueTask SetVPropCategoryCacheAsync(List<ImageProperty> value)
        {
            return _cache.SetAsync(_vPropCategoryCacheKey, value, _entryOptions);
        }

        public ValueTask SetVPropCacheAsync(List<ImageProperty> value)
        {
            return _cache.SetAsync(_vPropCacheKey, value, _entryOptions);
        }
    }
}
