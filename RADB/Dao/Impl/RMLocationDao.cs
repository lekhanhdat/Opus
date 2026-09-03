/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */
using AvePoint.RA.DB.Model;
using AvePoint.RA.Contract.LocationManagement;
using System.Collections.Generic;
using System.Linq;
using System;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.RMWeb.Tree;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.I18N.Core;
using System.Data.SqlClient;
using AvePoint.RA.Common.Util;
using System.Threading.Tasks;
using AvePoint.RA.Common;
using AvePoint.GCommon.Utility;
using System.Data.Entity;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMLocationDao : BaseDao<RMLocation>, IRMLocationDao
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMLocationDao));
        private static readonly object lockCreateLocation = new object();
        private const double Difference = 1e-6;

        private IExplorerDao mExplorerDao;
        private IRMScheduleDao ScheduleDao => PlatformWindsorManager.GetService<IRMScheduleDao>();
        public IExplorerDao ExplorerDao
        {
            get
            {
                if (mExplorerDao == null)
                {
                    mExplorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return mExplorerDao;
            }
        }

        public IRMTemplateDao RMTemplateDao { get; set; }

        public RMLocation GetLocationById(int id)
        {
            RMLocation result = null;
            try
            {
                using (var context = GetNewContext())
                {
                    result = context.RMLocation.AsQueryable().Where(t => t.Id == id && !t.IsRemoved).FirstOrDefault();
                    if (result != null && result.UniqueId != Guid.Empty)
                    {
                        result.PathForDisplay = GetLocationPath(result.DirPath);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in get location by id, reanson : {ex.ToString()}.");
                throw;
            }
            return result;
        }

        public RMLocation GetLocationWitPathById(int id, bool isLoadPath)
        {
            RMLocation result = null;
            try
            {
                using (var context = GetNewContext())
                {
                    result = context.RMLocation.AsQueryable().Where(t => t.Id == id && !t.IsRemoved).FirstOrDefault();
                    if (isLoadPath && result != null && result.UniqueId != Guid.Empty)
                    {
                        result.PathForDisplay = GetLocationPath(result.DirPath);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in get location by id, reanson : {ex.ToString()}.");
                throw;
            }
            return result;
        }

        public Dictionary<int, RMLocation> GetLocationByIDs(IEnumerable<int> ids)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    return context.RMLocation.AsQueryable()
                        .Where(t => !t.IsRemoved && ids.Contains(t.Id))
                        .ToDictionary(t => t.Id);
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in get location by id, reanson : {ex.ToString()}.");
                throw;
            }
        }
        public RMLocation GetLocationByUniqueId(Guid uniqueId, bool isReplaceI18NKey = true)
        {
            RMLocation result = null;
            try
            {
                using (var context = GetNewContext())
                {
                    result = context.RMLocation.AsQueryable().Where(t => t.UniqueId == uniqueId && !t.IsRemoved).FirstOrDefault();
                    if (result != null && result.UniqueId != Guid.Empty)
                    {
                        result.PathForDisplay = GetLocationPath(result.DirPath, isReplaceI18NKey);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in get location by unique id, reason : {ex.ToString()}.");
                throw;
            }
            return result;
        }

        public List<RMLocation> GetLocationByUniqueIds(List<Guid> uniqueId)
        {
            List<RMLocation> result = null;
            try
            {
                using (var context = GetNewContext())
                {
                    result = context.RMLocation.AsQueryable().Where(t => uniqueId.Contains(t.UniqueId) && !t.IsRemoved).ToList();
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in get location by unique id, reason : {ex.ToString()}.");
                throw;
            }
            return result;
        }

        public List<RMLocation> LoadRootNode(int pageIndex, int pageCount)
        {
            List<RMLocation> locations = new List<RMLocation>();
            try
            {
                locations.Add(GetRootLocation());
            }
            catch (Exception ex)
            {
                logger.Error($"Error in LoadRootNode, reason : {ex.ToString()}.");
            }
            return locations;
        }

        public RMLocation GetRootLocation()
        {
            this.UpgradeBottomLocationAssociation();
            using (var context = GetNewContext())
            {
                var rootLocation = context.RMLocation.FirstOrDefault(t => t.NodeType == (int)RMNodeLevel.PhysicalRootLocation && !t.IsRemoved);
                if (rootLocation == null)
                {
                    logger.Info("init data in location table.");
                    RMLocation temp = new RMLocation();
                    temp.ParentId = 0;
                    temp.UniqueId = Guid.NewGuid();
                    temp.Name = "RM_SPS_Location_RootNode";
                    temp.NodeType = (int)RMNodeLevel.PhysicalRootLocation;
                    temp.DirPath = "";
                    context.RMLocation.Add(temp);
                    var tempResult = context.SaveChanges();
                    if (tempResult > 0)
                    {
                        rootLocation = temp;
                    }
                }
                if (rootLocation != null)
                {
                    rootLocation.Name = rootLocation.Name == "RM_SPS_Location_RootNode"? I18N.Core.I18NEntity.GetString(rootLocation.Name) : rootLocation.Name;
                }
                return rootLocation;
            }
        }
        public void UpgradeBottomLocationAssociation()
        {
            using (var ctx = GetNewContext())
            {
                var allBottomLoationIds = ctx.RMLocation.Where(t => t.NodeType == (int)RMNodeLevel.PhysicalBottomLocation && !t.IsRemoved).Select(lo => lo.UniqueId).ToList();
                var bottomLocationassociationIds = ctx.RMLocationSuiteAssociation.Where(a => allBottomLoationIds.Contains(a.LocationUniqueId)).Select(a => a.LocationUniqueId).ToList();
                var needInitBottomLocationAssociation = allBottomLoationIds.Where(ml => !bottomLocationassociationIds.Contains(ml)).ToList();
                if (needInitBottomLocationAssociation.Count > 0)
                {
                    logger.Info("need initial bottom location association");
                    foreach (var bottom in needInitBottomLocationAssociation)
                    {
                        ctx.RMLocationSuiteAssociation.Add(new RMLocationSuiteAssociation()
                        {
                            LocationUniqueId = bottom,
                            SuiteUniqueId = new Guid(DefaultSuiteIds.RECORD_SUITE_DEFAULT_BOX_SUITE_ID)
                        });
                        ctx.RMLocationSuiteAssociation.Add(new RMLocationSuiteAssociation()
                        {
                            LocationUniqueId = bottom,
                            SuiteUniqueId = new Guid(DefaultSuiteIds.RECORD_SUITE_DEFAULT_FOLDER_SUITE_ID)
                        });
                    }
                    if (ctx.SaveChanges() > 0)
                    {
                        logger.Info("initial bottom location association successfully");
                    }
                }
            }
        }

        public RMLocationProfileNode GetLocationChildren(RMLocationProfileNode node)
        {
            if (node.NodeType == (int)RMNodeLevel.PhysicalRootLocation)
            {
                var root = Convert2ProfileNode(GetRootLocation(), true);
                root.Expanded = true;
                root.Loaded = true;
                root.PagerIndex = node.PagerIndex;
                root.PagerSize = node.PagerSize;
                node = root;
            } 
            else
            {
                int n = 0;
                node.ChildStates = GetChildIDsOrderByName(int.Parse(node.Id)).ToDictionary(i => i.ToString(), i => new List<int> { n++ });
                node.ChildrenCount = node.ChildStates.Count;
            }
            node.Children = new List<RMLocationProfileNode>();
            using (var context = GetNewContext())
            {
                int nodeId = int.Parse(node.Id);
                var subLocations = context.RMLocation.AsQueryable().Where(a => a.ParentId == nodeId && !a.IsRemoved)
                    .OrderBy(p => p.Name).Skip(node.PagerIndex * node.PagerSize).Take(node.PagerSize);
                foreach (var subLocation in subLocations)
                {
                    node.Children.Add(Convert2ProfileNode(subLocation, true, node.Checked == true));
                }
            }

            return node;
        }

        public RMLocationProfileNode GetRootLocationChildrenWithPermission(RMLocationProfileNode node, List<Guid> topLocationIds)
        {

            var root = Convert2ProfileNode(GetRootLocation(), true, topLocationIds: topLocationIds, needCheckPermission: true);
            root.Expanded = true;
            root.Loaded = true;
            root.PagerIndex = node.PagerIndex;
            root.PagerSize = node.PagerSize;
            node = root;

            node.Children = new List<RMLocationProfileNode>();
            using (var context = GetNewContext())
            {
                int nodeId = int.Parse(node.Id);
                var subLocations = context.RMLocation.AsQueryable().Where(a => a.ParentId == nodeId && !a.IsRemoved && topLocationIds.Contains(a.UniqueId))
                    .OrderBy(p => p.Name).Skip(node.PagerIndex * node.PagerSize).Take(node.PagerSize);
                foreach (var subLocation in subLocations)
                {
                    node.Children.Add(Convert2ProfileNode(subLocation, true, node.Checked == true));
                }
            }

            return node;
        }

        #region For TRIM Import
        public List<RMLocation> GetAllLocations()
        {
            using (var context = GetNewContext())
            {
                //int rootLevel = (int)RMNodeType.PhysicalRootLocation;
                var tempLocations = context.RMLocation.Where(r => !r.IsRemoved).ToList();
                return tempLocations;
            }
        }
        public List<Guid> GetLocationUniqueIds()
        {
            using (var context = GetNewContext())
            {
                var tempLocations = context.RMLocation.Where(r => !r.IsRemoved).Select(r => r.UniqueId).ToList();
                return tempLocations;
            }
        }

        #endregion
        public List<RMLocation> GetSubLocationByParentId(int parentId, int pageIndex, int pageCount, List<int> userAndGroupIds = null)
        {
            var subLocations = new List<RMLocation>();
            try
            {
                using (var context = GetNewContext())
                {
                    SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                    if (userAndGroupIds?.Count > 0)
                    {
                        List<SqlParameter> parameters = null;
                        string filterSql = $@"select * from {context.SchemaName}.RMLocations  as r where r.ParentId = @parentId and r.IsRemoved = 0 and
                                    ((cast(r.Id as nvarchar(36)) in (select a.Scope from {context.SchemaName}.RMScopePermissions as a) and cast(r.Id as nvarchar(36)) in (select p.Scope from {context.SchemaName}.RMScopePermissions as p where p.Id in (select ac.ScopePermission from {context.SchemaName}.RMScopeAccountMappings as ac where ac.Account in {DatabaseUtility.BuildInClause(userAndGroupIds, out parameters)})))
                                    or (cast(r.Id as nvarchar(36))  not in  (select a.Scope from {context.SchemaName}.RMScopePermissions as a)))";
                        parameters.Add(new SqlParameter("parentId", parentId));
                        subLocations = context.Database.SqlQuery<RMLocation>(filterSql, parameters.ToArray()).OrderBy(p => p.Name).Skip(pageIndex * pageCount).Take(pageCount).ToList();
                    }
                    else
                    {
                        subLocations = context.RMLocation.AsQueryable().Where(a => a.ParentId == parentId && !a.IsRemoved).OrderBy(p => p.Name).Skip(pageIndex * pageCount).Take(pageCount).ToList();
                    }
                    if (subLocations != null)
                    {
                        foreach (var tempSubLocation in subLocations)
                        {
                            tempSubLocation.RMLocationSuiteAssociationIds = context.RMLocationSuiteAssociation.Where(location => location.LocationUniqueId == tempSubLocation.UniqueId).Select(lo => lo.SuiteUniqueId).ToList();
                            //Location Path...
                            tempSubLocation.PathForDisplay = GetLocationPath(tempSubLocation.DirPath);

                            //Available Space...
                            if (Convert.ToInt32(tempSubLocation.AvailableSpace) != 0)
                            {
                                tempSubLocation.AvailableSpace = Math.Round(tempSubLocation.AvailableSpace, 2);
                            }
                            //SubLocation Count...
                            //tempSubLocation.SubLocationCount = CountSubLocation(tempSubLocation.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in GetSubLocationByParentId, reason : {ex.ToString()}.");
            }
            return subLocations;
        }

        public List<RMLocation> GetTopLocationByParentIdAndId(int parentId, int pageIndex, int pageCount, List<Guid> locationIds = null)
        {
            var subLocations = new List<RMLocation>();
            try
            {
                using (var context = GetNewContext())
                {
                    SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                    if (locationIds.Count > 0)
                    {
                        subLocations = context.RMLocation.AsQueryable().Where(a => a.ParentId == parentId && locationIds.Contains(a.UniqueId) && !a.IsRemoved).OrderBy(p => p.Name).Skip(pageIndex * pageCount).Take(pageCount).ToList();
                    }
                    else
                    {
                        subLocations = context.RMLocation.AsQueryable().Where(a => a.ParentId == parentId && !a.IsRemoved).OrderBy(p => p.Name).Skip(pageIndex * pageCount).Take(pageCount).ToList();
                    }
                    if (subLocations != null)
                    {
                        foreach (var tempSubLocation in subLocations)
                        {
                            tempSubLocation.RMLocationSuiteAssociationIds = context.RMLocationSuiteAssociation.Where(location => location.LocationUniqueId == tempSubLocation.UniqueId).Select(lo => lo.SuiteUniqueId).ToList();
                            tempSubLocation.PathForDisplay = GetLocationPath(tempSubLocation.DirPath);

                            if (Convert.ToInt32(tempSubLocation.AvailableSpace) != 0)
                            {
                                tempSubLocation.AvailableSpace = Math.Round(tempSubLocation.AvailableSpace, 2);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in GetSubLocationByParentId, reason : {ex.ToString()}.");
            }
            return subLocations;
        }

        public Dictionary<int, string> GetLocationIdNameMapping()
        {
            using (var context = GetNewContext())
            {
                return context.RMLocation.AsQueryable().Select(a => new { Id = a.Id, Name = a.Name }).ToDictionary(l => l.Id, z => z.Name);
            }
        }

        public IEnumerable<RMLocation> GetLocationBottomByLocationIds(IEnumerable<Guid> locationIds)
        {
            using(var context = GetNewContext())
            {
                return context.RMLocation.Where(a => !a.IsRemoved && a.NodeType == (int)RMNodeType.PhysicalBottomLocation && 
                                            locationIds.Any() && locationIds.Contains(a.UniqueId)).ToList();
            }
        }

        public IEnumerable<RMLocation> GetLocationNormalByIds(List<string> ids)
        {
            using (var context = GetNewContext())
            {
                return context.RMLocation.AsEnumerable()
                    .Where(a => !a.IsRemoved && a.NodeType == (int)RMNodeType.PhysicalNormalLocation && ids.Contains(a.Id.ToString())).ToList();
            }
        }

        public async Task<List<RMLocation>> GetAllTopLocation()
        {
            using var context = GetNewContext();
            var rootLocationId = await GetRootLocationId(context);
            return await context.RMLocation.Where(_ => !_.IsRemoved && _.ParentId == rootLocationId).ToListAsync();
        }

        public async Task<List<Guid>> GetAllTopLocationIds()
        {
            using var context = GetNewContext();
            var rootLocationId = await GetRootLocationId(context);
            return await context.RMLocation.Where(_ => !_.IsRemoved && _.ParentId == rootLocationId).Select(_ => _.UniqueId).ToListAsync();
        }

        private async Task<int> GetRootLocationId(Core.RMDbContext context)
        {
            return await context.RMLocation.Where(l => !l.IsRemoved && l.NodeType == (int)RMNodeType.PhysicalRootLocation).Select(l => l.Id).FirstOrDefaultAsync();
        }

        public List<Guid> LoadAllLocationIdUnderTopLocation(List<Guid> topLocationIds)
        {
            try
            {
                using var context = GetNewContext();

                var topLocationPaths = context.RMLocation
                     .Where(x => !x.IsRemoved && topLocationIds.Contains(x.UniqueId))
                     .Select(x => new
                     {
                         x.DirPath,
                         x.Id
                     })
                     .AsEnumerable()
                     .Select(x => $"{x.DirPath}{x.Id}")
                     .ToList();

                var result = context.RMLocation
                    .Where(x => !x.IsRemoved &&
                                topLocationPaths.Any(p => x.DirPath.StartsWith(p)))
                    .Select(x => x.UniqueId)
                    .Distinct()
                    .ToList();
                result.AddRange(topLocationIds);
                return result;
            }
            catch(Exception e)
            {
                logger.Error($"Load all location id under top location have errors: {e}");
                return new();
            }
        }

        public List<Guid> LoadAllLocationBottomIdUnderTopLocation(List<Guid> topLocationIds)
        {
            try
            {
                using var context = GetNewContext();

                var topLocationPaths = context.RMLocation
                     .Where(x => !x.IsRemoved && topLocationIds.Contains(x.UniqueId))
                     .Select(x => new
                     {
                         x.DirPath,
                         x.Id
                     })
                     .AsEnumerable()
                     .Select(x => $"{x.DirPath}{x.Id}")
                     .ToList();

                var result = context.RMLocation
                    .Where(x => !x.IsRemoved &&
                                (topLocationPaths.Any(p => x.DirPath.StartsWith(p)) || topLocationIds.Contains(x.UniqueId))
                                && x.NodeType == (int)RMNodeType.PhysicalBottomLocation)
                    .Select(x => x.UniqueId)
                    .Distinct()
                    .ToList();
                return result;
            }
            catch(Exception e)
            {
                logger.Error($"Load all location bottom id under top location have errors: {e}");
                return new();
            }
        }

        public List<string> LoadLocationPathByLocationIds(List<Guid> locationIds)
        {
            using var context = GetNewContext();
            return context.RMLocation
                .Where(x => !x.IsRemoved && locationIds.Contains(x.UniqueId))
                .AsEnumerable()
                .Select(x => GetLocationPath(x))
                .ToList();
        }

        public Guid LoadTopLocationIdBySubLocation(Guid locationId)
        {
            try
            {
                using var context = GetNewContext();
                var currentLocation = context.RMLocation
                    .Where(x => !x.IsRemoved && x.UniqueId == locationId)
                    .Select(x => new { x.DirPath, x.NodeType, x.UniqueId })
                    .FirstOrDefault();
                if (string.IsNullOrEmpty(currentLocation.DirPath))
                    return Guid.Empty;
                var splitPaths = currentLocation.DirPath.TrimEnd('/').Split('/');
                if (splitPaths.Length == 1)
                    return currentLocation.UniqueId;
                var topLocationIdStr = splitPaths.Length >= 2 ? splitPaths[1] : splitPaths[0];
                if (int.TryParse(topLocationIdStr, out int topLocationId))
                {
                    var topLocationGuidId = context.RMLocation
                        .Where(x => !x.IsRemoved && x.Id == topLocationId)
                        .Select(x => x.UniqueId)
                        .FirstOrDefault();
                    return topLocationGuidId;
                }
                return Guid.Empty;
            }
            catch(Exception e)
            {
                logger.Error($"Load top location id by sub location have errors: {e}");
                return Guid.Empty;
            }
        }

        public Guid GetLocationUniqueIdById(int id)
        {
            try
            {
                using var context = GetNewContext();
                return context.RMLocation.Where(l => l.Id == id).Select(l => l.UniqueId).FirstOrDefault();
            }
            catch(Exception e)
            {
                logger.Error($"Load location unique Id have errors: {e}");
                return Guid.Empty;
            }
        }

        private string GetLocationPath(RMLocation location, bool isSkipRoot = true)
        {
            string result = string.Empty;
            if(isSkipRoot && string.IsNullOrEmpty(location.DirPath)) 
            {
                return location.Name;
            }
            string dirPath = location.DirPath + location.Id;
            if (!string.IsNullOrEmpty(dirPath))
            {
                var LocationIDNameMapping = GetLocationIdNameMapping(); 
                dirPath = dirPath.TrimEnd('/');
                List<string> locationIds = dirPath.Split('/').ToList();
                int i = isSkipRoot ? 1 : 0;
                for (i = 1; i < locationIds.Count; i++)
                {
                    var tempPath = string.Empty;
                    if (LocationIDNameMapping.TryGetValue(Convert.ToInt32(locationIds[i]), out tempPath))
                    {
                        if (i == 1)
                        {
                            result = tempPath;
                        }
                        else
                        {
                            result += "/" + tempPath;
                        }
                    }
                    else
                    {
                        logger.Warn($"Cannot get location : {locationIds[i]} in db.");
                        throw new Exception($"Cannot get location by Path");
                    }
                }
            }
            return result;
        }

        public string GetLocationPath(string dirPath, bool isReplaceI18NKey = true)
        {
            var result = string.Empty;
            if (!string.IsNullOrEmpty(dirPath))
            {
                var LocationIDNameMapping = GetLocationIdNameMapping();
                dirPath = dirPath.TrimEnd('/');
                List<string> locationIds = dirPath.Split('/').ToList();
                for (int i = 0; i < locationIds.Count; i++)
                {
                    var tempPath = string.Empty;
                    if (LocationIDNameMapping.TryGetValue(Convert.ToInt32(locationIds[i]), out tempPath))
                    {
                        if (i == 0)
                        {
                            result = tempPath == "RM_SPS_Location_RootNode" && isReplaceI18NKey ? I18N.Core.I18NEntity.GetString(tempPath) : tempPath;
                        }
                        else
                        {
                            result = result + "/" + tempPath;
                        }
                    }
                    else
                    {
                        logger.Warn($"Cannot get location : {locationIds[i]} in db.");
                        throw new Exception($"Cannot get location by Path");
                    }
                }
            }
            return result;
        }

        public List<RMLocationProfileNode> GetSubLocationByParentId(int parentId)
        {
            List<RMLocation> subLocations = GetAllSubLocationByParentId(parentId);
            return subLocations.Select(el => Convert2ProfileNode(el)).ToList();
        }
        public List<RMLocation> GetAllSubLocationByParentId(int parentId)
        {
            var subLocations = new List<RMLocation>();
            try
            {
                using (var context = GetNewContext())
                {
                    subLocations = context.RMLocation.AsQueryable().Where(a => a.ParentId == parentId && !a.IsRemoved).OrderBy(p => p.Name).ToList();
                    if (subLocations != null)
                    {
                        foreach (var tempSubLocation in subLocations)
                        {
                            if (Convert.ToInt32(tempSubLocation.AvailableSpace) != 0)
                            {
                                tempSubLocation.AvailableSpace = Math.Round(tempSubLocation.AvailableSpace, 2);
                            }
                            tempSubLocation.SubLocationCount = CountSubLocation(tempSubLocation.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in GetAllSubLocationByParentId, reason : {ex.ToString()}.");
            }
            return subLocations;
        }

        public List<RMLocation> GetAllSubLocationByParentIdAndUniqueIds(int parentId, List<Guid> locationIds)
        {
            var subLocations = new List<RMLocation>();
            try
            {
                if (locationIds == null || locationIds.Count == 0) return subLocations;
                using (var context = GetNewContext())
                {
                    subLocations = context.RMLocation.AsQueryable().Where(a => a.ParentId == parentId && locationIds.Contains(a.UniqueId) && !a.IsRemoved).OrderBy(p => p.Name).ToList();
                    if (subLocations != null)
                    {
                        foreach (var tempSubLocation in subLocations)
                        {
                            if (Convert.ToInt32(tempSubLocation.AvailableSpace) != 0)
                            {
                                tempSubLocation.AvailableSpace = Math.Round(tempSubLocation.AvailableSpace, 2);
                            }
                            tempSubLocation.SubLocationCount = CountSubLocation(tempSubLocation.Id);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in GetAllSubLocationByParentId, reason : {ex.ToString()}.");
            }
            return subLocations;
        }

        public bool HasSubLocation(int id)
        {
            bool hasChild = false;
            try
            {
                using (var context = GetNewContext())
                {
                    var subLocations = context.RMLocation.AsQueryable().Where(a => a.ParentId == id && !a.IsRemoved).Take(1);
                    if (subLocations != null)
                    {
                        hasChild = subLocations.Count() > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in HasSubLocation, reason : {ex.ToString()}.");
            }
            return hasChild;
        }

        public async Task<RMLocation> RenameLocationAsync(int locationId, string name, bool ensureConflict)
        {
            RMLocation result = null;
            try
            {
                using (var context = GetNewContext())
                {
                    result = context.RMLocation.AsQueryable().Where(t => t.Id == locationId).First();
                    if (result.IsRemoved)
                    {
                        throw new Exception("Location is invalied.");
                    }
                    if (ensureConflict && HasSameName(locationId, name, result.ParentId))
                    {
                        throw new Exception("Location has same name.");
                    }
                    result.Name = name;
                    result.ModifiedTime = DateTime.UtcNow.Ticks;
                    result.ModifiedUserId = TenantLocalValue.LogonUserId;
                    await UpdateAsync(result);
                    result.SubLocationCount = CountSubLocation(locationId);
                    result.RMLocationSuiteAssociationIds = context.RMLocationSuiteAssociation.Where(location => location.LocationUniqueId == result.UniqueId).Select(lo => lo.SuiteUniqueId).ToList();
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occur when RenameLocation, reason : {ex.ToString()}.");
                throw;
            }
            return result;
        }

        public RMLocation CreateLocation(string name, int parentId)
        {
            RMLocation result = null;
            try
            {
                using (var context = GetNewContext())
                {
                    var pLocation = context.RMLocation.AsQueryable().Where(t => t.Id == parentId).First();
                    if (pLocation.IsRemoved)
                    {
                        throw new Exception("Parent location is invalied.");
                    }
                    var tempLocation = new RMLocation();
                    tempLocation.UniqueId = Guid.NewGuid();
                    tempLocation.ParentId = parentId;
                    tempLocation.Name = name;
                    tempLocation.NodeType = (int)RMNodeLevel.PhysicalNormalLocation;
                    tempLocation.DirPath = pLocation.DirPath + pLocation.Id.ToString() + "/";
                    var createdTime = DateTime.UtcNow.Ticks;
                    tempLocation.CreatedUserId = TenantLocalValue.LogonUserId;
                    tempLocation.CreatedTime = createdTime;
                    tempLocation.ModifiedUserId = TenantLocalValue.LogonUserId;
                    tempLocation.ModifiedTime = createdTime;

                    lock (lockCreateLocation)
                    {
                        if (HasSameName(name, parentId))
                        {
                            throw new Exception("Location has same name.");
                        }
                        context.RMLocation.Add(tempLocation);
                        context.SaveChanges();
                        result = context.RMLocation.AsQueryable().Where(t => t.ParentId == parentId && !t.IsRemoved && t.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in CreateLocation, reason : {ex.ToString()}.");
                throw;
            }
            return result;
        }

        public async Task<bool> DeleteLocationAsync(int locationId)
        {
            var result = false;
            try
            {
                using (var context = GetNewContext())
                {
                    var cLocations = context.RMLocation.AsQueryable().Where(t => t.ParentId == locationId && !t.IsRemoved);
                    if (cLocations != null && cLocations.Count() > 0)
                    {
                        logger.Warn("The location has children location associated, cannot be deleted now.");
                    }
                    else
                    {
                        var location = context.RMLocation.AsQueryable().Where(t => t.Id == locationId && !t.IsRemoved).FirstOrDefault();
                        if (location != null)
                        {
                            var locationParentUniqueId = GetLocationById(location.ParentId).UniqueId;
                            var scheduleProfileId = $"{locationParentUniqueId}|{location.UniqueId}";
                            var deleteScheduleInfos = ScheduleDao.GetScheduleByProfileId(scheduleProfileId);
                            foreach (var scheduleInfo in deleteScheduleInfos)
                            {
                                scheduleInfo.IsRemoved = true;
                                ScheduleDao.UpdateScheduleAsync(scheduleInfo);
                            }                          
                            location.IsRemoved = true;
                            await this.UpdateAsync(location);
                            context.SaveChanges();
                            result = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in DeleteLocation, reason : {ex.ToString()}.");
                throw;
            }
            return result;
        }

        public RMLocation GetLocationsBySearch(string locationStr)
        {
            RMLocation result = null;
            try
            {
                using (var context = GetNewContext())
                {
                    var tempLocations = context.RMLocation.AsQueryable().Where(r => r.Name.Contains(locationStr) && !r.IsRemoved).ToList();
                    if (tempLocations != null && tempLocations.Count > 0)
                    {
                        //Root Location
                        result = context.RMLocation.Where(r => r.NodeType == (int)RMNodeLevel.PhysicalRootLocation && !r.IsRemoved).FirstOrDefault();
                        result.Name = result.Name == "RM_SPS_Location_RootNode" ? I18NEntity.GetString(result.Name) : result.Name;
                        result.SubLocationCount = CountSubLocation(result.Id);
                        result.SubLocations = new List<RMLocation>();

                        //Build Location Tree
                        //Sub Location
                        //Renew Path
                        foreach (var temp in tempLocations)
                        {
                            temp.DirPath = temp.DirPath + temp.Id.ToString();
                            List<string> locationIds = temp.DirPath.Split('/').ToList();
                            var tempLocation = new RMLocation();
                            for (int i = 1; i < locationIds.Count; i++)
                            {
                                int subLocationId = Convert.ToInt32(locationIds[i]);
                                var subLocation = context.RMLocation.AsQueryable().Where(r => r.Id.Equals(subLocationId)).FirstOrDefault();
                                subLocation.SubLocationCount = CountSubLocation(subLocationId);
                                if (i == 1)
                                {
                                    tempLocation = BuildLocationTree(result, subLocation);
                                }
                                else
                                {
                                    tempLocation = BuildLocationTree(tempLocation, subLocation);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in GetLocationsBySearch, reason : {ex.ToString()}.");
            }
            return result;
        }

        public RMLocationProfileNode SearchLocationTree(string searchKey)
        {
            RMLocationProfileNode treeRoot = null;
            try
            {
                using (var context = GetNewContext())
                {
                    //Root Location
                    RMLocation root = context.RMLocation.Where(r => r.NodeType == (int)RMNodeLevel.PhysicalRootLocation && !r.IsRemoved).FirstOrDefault();
                    root.Name = root.Name == "RM_SPS_Location_RootNode" ? I18NEntity.GetString(root.Name) : root.Name;
                    treeRoot = Convert2ProfileNode(root);

                    var matchLocations = context.RMLocation.AsQueryable().Where(r => r.Name.Contains(searchKey) && !r.IsRemoved)
                        .ToList().Select(a => Convert2ProfileNode(a));
                    var loadedLocations = matchLocations.ToDictionary(a => a.Id.ToString());
                    foreach (var temp in matchLocations)
                    {
                        temp.DirPath = temp.DirPath + temp.Id.ToString();
                        var locationIds = temp.DirPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                        if(locationIds[0] != treeRoot.Id)
                        {
                            continue;
                        }
                        RMLocationProfileNode tempParent = treeRoot;
                        RMLocationProfileNode tempChild = null;
                        for (int i = 1; i < locationIds.Length; i++)
                        {
                            var sId = locationIds[i];
                            if(!loadedLocations.TryGetValue(sId, out tempChild))
                            {
                                int iId = Convert.ToInt32(sId);
                                var subLocation = context.RMLocation.AsQueryable().FirstOrDefault(r => r.Id.Equals(iId));
                                if(subLocation == null)
                                {
                                    //数据错误，有节点缺失
                                    break;
                                }
                                tempChild = loadedLocations[sId] = Convert2ProfileNode(subLocation);
                            }

                            if(tempParent.ChildStates == null)
                            {
                                tempParent.ChildStates = new Dictionary<string, List<int>>();
                            } 
                            if(!tempParent.ChildStates.ContainsKey(sId))
                            {
                                tempParent.ChildStates[sId] = new List<int> { tempParent.ChildrenCount };
                                tempParent.Children.Add(tempChild);
                                tempParent.ChildrenCount = tempParent.Children.Count;
                                tempParent.HasChildren = true;
                                tempParent.Expanded = true;
                                tempParent.Loaded = true;
                            }

                            tempParent = tempChild;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in SearchLocationTree, reason : {ex.ToString()}.");
            }
            return treeRoot;
        }

        public async Task<RMLocation> SaveLocationSettingAsync(LocationInfo locationSetting)
        {
            RMLocation result = null;
            try
            {
                using (var context = GetNewContext())
                {
                    var tempLocation = context.RMLocation.AsQueryable().Where(r => r.Id.Equals(locationSetting.LocationId) && !r.IsRemoved).FirstOrDefault();
                    if (tempLocation != null)
                    {
                        tempLocation.NodeType = locationSetting.NodeType;
                        if (locationSetting.Description?.Length <= 1000)
                        {
                            tempLocation.Description = locationSetting.Description;
                        }
                        if (Math.Abs(tempLocation.AvailableSpace - locationSetting.AvailableSpace) <= Difference)
                        {
                            if (Convert.ToInt32(tempLocation.AvailableSpace) != 0)
                            {
                                tempLocation.AvailableSpace = Math.Round(tempLocation.AvailableSpace, 2);
                            }
                        }
                        else
                        {
                            tempLocation.AvailableSpace = locationSetting.AvailableSpace;
                        }
                        tempLocation.ModifiedUserId = TenantLocalValue.LogonUserId;
                        tempLocation.ModifiedTime = DateTime.UtcNow.Ticks;
                        await this.UpdateAsync(tempLocation);
                        result = tempLocation;

                        if (locationSetting.NodeType == (int)RMNodeLevel.PhysicalBottomLocation)
                        {
                            var allSuites = context.Suite.ToList();
                            var locationAssociations = context.RMLocationSuiteAssociation.Where(l => l.LocationUniqueId == tempLocation.UniqueId).ToList();
                            var locationAssociationIds = locationAssociations.Select(a => a.SuiteUniqueId).ToList();
                            foreach (var suite in allSuites)
                            {
                                var existInDbAssociation = locationAssociationIds.Contains(suite.UniqueId);
                                var existInDtoAssocition = locationSetting.AssociationSuites.Contains(suite.UniqueId);
                                if (!existInDbAssociation && existInDtoAssocition)
                                {
                                    context.RMLocationSuiteAssociation.Add(new RMLocationSuiteAssociation()
                                    {
                                        LocationUniqueId = tempLocation.UniqueId,
                                        SuiteUniqueId = suite.UniqueId
                                    });
                                    context.SaveChanges();
                                }
                                else if (existInDbAssociation && !existInDtoAssocition)
                                {
                                    var templates = RMTemplateDao.GetAllSubTemplateBySuiteId(suite.UniqueId);
                                    List<int> allSubTemplates = templates.Where(t => t.Type != TemplateType.Records).Select(t => t.Id).ToList();
                                    if (allSubTemplates.Count > 0 && ExplorerDao.QueryByPage(d => allSubTemplates.Contains(d.TemplateId) && d.LocationId == tempLocation.UniqueId && d.RecordStatus != (int)Contract.Explorer.RMRecordStatus.RMDeleted && d.RecordStatus != (int)Contract.Explorer.RMRecordStatus.MoveOverwrite, 1).Item1.Any())
                                    {
                                        throw new CancelSuiteAssociationInUsingExcetion(I18NEntity.GetString("RM_LM_CancelSuiteAssociationInUsingExcetionMessage", I18NEntity.GetString(suite.Name)));
                                    }
                                    else
                                    {
                                        var removeEntity = locationAssociations.FirstOrDefault(l => l.SuiteUniqueId == suite.UniqueId);
                                        context.RMLocationSuiteAssociation.Remove(removeEntity);
                                        context.SaveChanges();
                                    }
                                }
                            }
                        }
                        else
                        {
                            //validate exist data in outer
                            var associations = context.RMLocationSuiteAssociation.Where(l => l.LocationUniqueId == tempLocation.UniqueId).ToList();
                            context.RMLocationSuiteAssociation.RemoveRange(associations);
                        }
                        result.RMLocationSuiteAssociationIds = locationSetting.AssociationSuites;
                    }
                }
            }
            catch (CancelSuiteAssociationInUsingExcetion)
            {
                throw;
            }
            catch (Exception ex)
            {
                logger.Error($"Error in SaveLocationSetting, reason : {ex.ToString()}.");
            }
            return result;
        }

        /// <summary>
        /// 组装Id和Name的FullPath
        /// </summary>
        /// <param name="uniqueId"></param>
        /// <returns></returns>
        public RMLocation GetLocationInfo(Guid uniqueId)
        {
            RMLocation result = null;
            try
            {
                using (var context = GetNewContext())
                {
                    result = context.RMLocation.AsQueryable().Where(t => t.UniqueId == uniqueId && !t.IsRemoved).FirstOrDefault();
                    if (result != null && result.UniqueId != Guid.Empty)
                    {
                        result.PathForDisplay = $"{GetLocationPath(result.DirPath)}/{result.Name}/";
                        result.DirPath = $"{result.DirPath}{result.Id}/";
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An Error occured when GetLocationIdFullPath, reason : {ex.ToString()}.");
                throw;
            }
            return result;
        }

        public List<RMLocation> GetLocationInfos(IEnumerable<Guid> uniqueIds)
        {
            using(var context = GetNewContext())
            {
                return context.RMLocation.Where(item => uniqueIds.Contains(item.UniqueId)).ToList();
            }
        }

        #region Private Zone
        public RMLocation GetByName(string name, int parentId)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    return context.RMLocation.AsQueryable().FirstOrDefault(t => t.ParentId == parentId && !t.IsRemoved && t.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                }
            }
            catch
            {
                return null;
            }
        }

        public bool HasSameName(string name, int parentId)
        {
            bool result = false;
            try
            {
                using (var context = GetNewContext())
                {
                    var locations = context.RMLocation.AsQueryable().Where(t => t.ParentId == parentId && !t.IsRemoved && t.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                    if (locations != null && locations.Count() > 0)
                    {
                        result = true;
                    }
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }

        private bool HasSameName(int locationId, string name, int parentId)
        {
            bool result = false;
            try
            {
                using (var context = GetNewContext())
                {
                    var locations = context.RMLocation.AsQueryable().Where(t => t.ParentId == parentId && !t.IsRemoved && !t.Id.Equals(locationId) && t.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase));
                    if (locations != null && locations.Count() > 0)
                    {
                        result = true;
                    }
                }
            }
            catch
            {
                result = false;
            }
            return result;
        }

        public int CountSubLocation(int parentId)
        {
            var result = 0;
            try
            {
                using (var context = GetNewContext())
                {
                    result = context.RMLocation.AsQueryable().Where(a => a.ParentId == parentId && !a.IsRemoved).Count();
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in CountSubLocation, reason : {ex.ToString()}.");
            }
            return result;
        }
        public int CountSubLocation(int parentId, List<int> userAndGroupIds)
        {
            var result = 0;
            try
            {
                using (var context = GetNewContext())
                {
                    SecurityUtils.SanitizeSQLSchemaName(context.SchemaName);
                    if (userAndGroupIds != null)
                    {
                        List<SqlParameter> parameters = null;
                        string filterSqlCount = $@"select count(*) from {context.SchemaName}.RMLocations  as r where r.ParentId = @parentId and r.IsRemoved = 0 and
                                    ((cast(r.Id as nvarchar(36)) in (select a.Scope from {context.SchemaName}.RMScopePermissions as a) and cast(r.Id as nvarchar(36)) in (select p.Scope from {context.SchemaName}.RMScopePermissions as p where p.Id in (select ac.ScopePermission from {context.SchemaName}.RMScopeAccountMappings as ac where ac.Account in {DatabaseUtility.BuildInClause(userAndGroupIds, out parameters)})))
                                    or (cast(r.Id as nvarchar(36))  not in  (select a.Scope from {context.SchemaName}.RMScopePermissions as a)))";
                        parameters.Add(new SqlParameter("parentId", parentId));
                        result = context.Database.SqlQuery<int>(filterSqlCount, parameters.ToArray()).FirstOrDefault();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in CountSubLocation, reason : {ex.ToString()}.");
            }
            return result;
        }

        public int CountSubLocationByLocationIds(int parentId, List<Guid> locationIds)
        {
            var result = 0;
            try
            {
                using (var context = GetNewContext())
                {
                    if (locationIds.Count > 0)
                    {
                        result = context.RMLocation.AsQueryable().Where(a => a.ParentId == parentId && locationIds.Contains(a.UniqueId) && !a.IsRemoved).Count();
                    }
                    else
                    {
                        result = context.RMLocation.AsQueryable().Where(a => a.ParentId == parentId && !a.IsRemoved).Count();

                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in CountSubLocationByLocationId, reason : {ex.ToString()}.");
            }
            return result;
        }

        public List<int> GetChildIDsOrderByName(int parentId, List<int> userAndGroupIds = null, List<Guid> topLocationIds = null, bool needCheckPermission = false)
        {
            try
            {
                using (var context = GetNewContext())
                {
                    if (userAndGroupIds != null && userAndGroupIds.Count > 0)
                    {
                        List<SqlParameter> parameters = null;
                        string filterSql = $@"select * from {context.SchemaName}.RMLocations  as r where r.ParentId = @parentId and r.IsRemoved = 0 and
                                    ((cast(r.Id as nvarchar(36)) in (select a.Scope from {context.SchemaName}.RMScopePermissions as a) and cast(r.Id as nvarchar(36)) in (select p.Scope from {context.SchemaName}.RMScopePermissions as p where p.Id in (select ac.ScopePermission from {context.SchemaName}.RMScopeAccountMappings as ac where ac.Account in {DatabaseUtility.BuildInClause(userAndGroupIds, out parameters)})))
                                    or (cast(r.Id as nvarchar(36))  not in  (select a.Scope from {context.SchemaName}.RMScopePermissions as a)))";
                        parameters.Add(new SqlParameter("parentId", parentId));
                        var ids = context.Database.SqlQuery<RMLocation>(filterSql, parameters.ToArray()).OrderBy(p => p.Name).Select(a => a.Id).ToList(); ;
                        return ids;
                    }
                    else
                    {
                        if (needCheckPermission)
                        {
                            return context.RMLocation.AsQueryable()
                                .Where(a => a.ParentId == parentId && !a.IsRemoved && topLocationIds.Contains(a.UniqueId))
                                .OrderBy(p => p.Name)
                                .Select(a => a.Id).ToList();
                        }
                        //subLocations = context.RMLocation.AsQueryable().Where(a => a.ParentId == parentId && !a.IsRemoved).OrderBy(p => p.Name).Skip(pageIndex * pageCount).Take(pageCount).ToList();
                        var ids = context.RMLocation.AsQueryable()
                        .Where(a => a.ParentId == parentId && !a.IsRemoved)
                        .OrderBy(p => p.Name)
                        .Select(a => a.Id).ToList();
                        return ids;
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error($"Error in GetChildIDs, reason : {ex.ToString()}.");
                throw ex;
            }
        }

        public List<Guid> GetLocationSuiteAssociationIds(Guid uniqueId)
        {
            using (var context = GetNewContext())
            {
                return context.RMLocationSuiteAssociation.Where(lo => lo.LocationUniqueId == uniqueId).Select(lo => lo.SuiteUniqueId).ToList();
            }
        }

        private RMLocation BuildLocationTree(RMLocation location, RMLocation subLocation)
        {
            if (location.SubLocations == null)
            {
                location.SubLocations = new List<RMLocation>();
            }
            if (location.SubLocations.AsQueryable().Where(t => t.Id.Equals(subLocation.Id)).FirstOrDefault() == null)
            {
                location.SubLocations.Add(subLocation);
            }
            return subLocation;
        }

        public RMLocationProfileNode Convert2ProfileNode(int locationId, bool widthChildIDs = false, bool isChecked = false, List<Guid> topLocationIds = null, bool needCheckPermission = false)
        {
            var location = GetLocationById(locationId);
            return Convert2ProfileNode(location, widthChildIDs, isChecked, topLocationIds, needCheckPermission);
        }

        private RMLocationProfileNode Convert2ProfileNode(RMLocation location, bool widthChildIDs = false, bool isChecked = false, List<Guid> topLocationIds = null, bool needCheckPermission = false)
        {
            RMLocationProfileNode node = new RMLocationProfileNode() {
            
                Id = location.Id.ToString(),
                ParentId = location.ParentId.ToString(),
                AvailableSpace = location.AvailableSpace,
                DirPath = location.DirPath,
                Name = location.Name,
                NodeType = location.NodeType,
                PagerIndex = 0,
                PagerSize = 10,
                Children = new List<RMLocationProfileNode>(),
                UniqueId = location.UniqueId,
                Checked = isChecked
            };
            if (widthChildIDs && location.NodeType != (int)RMNodeType.PhysicalBottomLocation)
            {
                var ids = GetChildIDsOrderByName(location.Id,topLocationIds: topLocationIds, needCheckPermission: needCheckPermission);
                node.ChildStates = new Dictionary<string, List<int>>();
                for (int i = 0; i < ids.Count; i++)
                {
                    node.ChildStates[ids[i].ToString()] = isChecked ? new List<int>() { i, 1 } : new List<int>() { i };
                }
                node.ChildrenCount = node.ChildStates.Count;
                node.HasChildren = node.ChildrenCount > 0;
            }
            return node;
        }
        #endregion
    }
}
