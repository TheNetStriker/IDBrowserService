using IDBrowserServiceCore.Data;
using IDBrowserServiceCore.Data.IDImager;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IServiceScopeFactory _scopeFactory;

        private int IdPropCount { get; set; }
        private int IdCatalogItemDefinitionCount { get; set; }

        /// <summary>
        /// v_PropCategory Cache
        /// </summary>
        public List<ImageProperty> VPropCategoryCache { get; set; }

        /// <summary>
        /// v_prop Cache
        /// </summary>
        public List<ImageProperty> VPropCache { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="scopeFactory">IServiceScopeFactory</param>
        public DatabaseCache(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Checks if the cache needs updating.
        /// </summary>
        public async Task CheckAndUpdateCacheAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IDImagerDB>();

            int currentIdPropCount = await db.idProp.CountAsync();
            int currentidCatalogItemDefinitionCount = await db.idCatalogItemDefinition.CountAsync();

            if (VPropCategoryCache == null
                || VPropCache == null
                || IdPropCount != currentIdPropCount
                || IdCatalogItemDefinitionCount != currentidCatalogItemDefinitionCount)
            {
                var queryVPropCategorySource = from tbl in db.v_PropCategory
                                               where !tbl.CategoryName.Equals("Internal")
                                               orderby tbl.CategoryName
                                               select new ImageProperty
                                               {
                                                   GUID = tbl.GUID,
                                                   Name = tbl.CategoryName,
                                                   ImageCount = tbl.idPhotoCount,
                                                   SubPropertyCount = db.idProp.Where(x => x.ParentGUID == tbl.GUID).Count()
                                               };

                var vPropCategory = await queryVPropCategorySource.ToListAsync();

                var queryVProp = from tbl in db.v_prop
                                 orderby tbl.PropName
                                 select new ImageProperty
                                 {
                                     GUID = tbl.GUID,
                                     ParentGUID = tbl.ParentGUID,
                                     Name = tbl.PropName,
                                     ImageCount = tbl.idPhotoCount,
                                     SubPropertyCount = db.idProp.Where(x => x.ParentGUID == tbl.GUID).Count()
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
