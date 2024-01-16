using IDBrowserServiceCore.Data;
using IDBrowserServiceCore.Data.IDImager;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IDBrowserServiceCore.Services
{
    /// <summary>
    /// Database cache service
    /// </summary>
    public class DatabaseCache : IDatabaseCache
    {
        private readonly IDImagerDB _db;
        private readonly IMemoryCache _memoryCache;

        private int IdPropCount { get; set; }
        private int IdCatalogItemDefinitionCount { get; set; }

        /// <summary>
        /// v_PropCategory Cache
        /// </summary>
        public List<ImageProperty> VPropCategoryCache
        { 
            get
            {
                return _memoryCache.Get<List<ImageProperty>>(nameof(VPropCategoryCache));
            }
            set
            {
                _memoryCache.Set(nameof(VPropCategoryCache), value);
            }
        }

        /// <summary>
        /// v_prop Cache
        /// </summary>
        public List<ImageProperty> VPropCache
        {
            get
            {
                return _memoryCache.Get<List<ImageProperty>>(nameof(VPropCache));
            }
            set
            {
                _memoryCache.Set(nameof(VPropCache), value);
            }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="db">IDImagerDB</param>
        /// <param name="memoryCache">IMemoryCache</param>
        public DatabaseCache(IDImagerDB db, IMemoryCache memoryCache)
        {
            _db = db;
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Checks if the cache needs updating.
        /// </summary>
        public async Task CheckAndUpdateCacheAsync()
        {
            int currentIdPropCount = await _db.idProp.CountAsync();
            int currentidCatalogItemDefinitionCount = await _db.idCatalogItemDefinition.CountAsync();

            if (VPropCategoryCache == null
                || VPropCache == null
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

                VPropCategoryCache = vPropCategory;
                VPropCache = vprop;

                IdPropCount = currentIdPropCount;
                IdCatalogItemDefinitionCount = currentidCatalogItemDefinitionCount;
            }
        }
    }
}
