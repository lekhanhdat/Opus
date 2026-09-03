using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using static Google.Apis.Drive.v3.Data.File.ImageMediaMetadataData;


namespace AvePoint.RA.RAPhysical.Export
{
    public class ExportHoldsRecordsCache
    {
        protected AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public static ILocationManagementService LocationManagementService => PlatformWindsorManager.GetService<ILocationManagementService>();
        public static IRMScopeDao RMScopeDao = (IRMScopeDao)PlatformWindsorManager.GetService(typeof(IRMScopeDao));

        private readonly Dictionary<Guid, string> mNodeNameCache = new Dictionary<Guid, string>();
        public ExportHoldsRecordsCache() { }
        public string GetLocationNodePath(Guid locationId)
        {
            if (!mNodeNameCache.ContainsKey(locationId))
            {
                var locationPath = LocationManagementService.GetLocationPathById(locationId);
                mNodeNameCache.Add(locationId, locationPath ?? string.Empty);
                logger.Debug($"[CACHE] Caching Id: {locationId}, value {locationPath}.");

                return locationPath;
            }
            return mNodeNameCache[locationId];
        }
        public string GetScopeFullPath(Guid scopeId, string aveSiteId)
        {
            if (mNodeNameCache.ContainsKey(scopeId))
            {
                return mNodeNameCache[scopeId];
            }
            var scope = RMScopeDao.GetScopeInfoByIds(new List<Guid>() { scopeId }).Values?.FirstOrDefault();
            if (scope != null)
            {
                mNodeNameCache.Add(scopeId, scope.FullPath ?? string.Empty);
                logger.Debug($"[CACHE] Caching Scope Id from DB: {scopeId} value {scope.FullPath}");
                return mNodeNameCache[scopeId];
            }

            SharePointSettingUtility SPUtility = new SharePointSettingUtility();
            var site = SPUtility.GetRemoteSiteCollection(aveSiteId.ToString());

            if (site != null)
            {
                var newScope = new RMScope()
                {
                    FullPath = site.url,
                    ScopeId = scopeId,
                    ScopeName = site.Name,
                    IsRemoved = false,
                };
                RMScopeDao.AddOrUpateSiteScope(newScope);
                mNodeNameCache.Add(scopeId, site.url ?? string.Empty);
                logger.Debug($"[CACHE] Caching Scope Id from SP Site: {scopeId} value {site.url}");
                return mNodeNameCache[scopeId];
            }

            return string.Empty;
        }
        public void Clear()
        {
            try
            {
                logger.Debug($"[CACHE] Clearing {mNodeNameCache.Count} cached from memory.");
                mNodeNameCache?.Clear();
                logger.Debug($"[CACHE] {mNodeNameCache.Count} records");
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while clearing ExportHoldsRecordsCache: {ex.Message}");
            }
        }
    }
}
