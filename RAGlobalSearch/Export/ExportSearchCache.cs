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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.CommonUtil;
using System.Reflection;

namespace RAGlobalSearch.Export
{
    public class ExportSearchCache
    {
        protected AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public static ILocationManagementService LocationManagementService => PlatformWindsorManager.GetService<ILocationManagementService>();
        public ExportSearchCache() { }

        private IExplorerDao _explorerDao = new ExplorerDao();

        private Dictionary<Guid, string> NodeNameCache = new();

        private string GetPhysicalLocationFullPathByAncestors(BaseRecordDto record, string locationPath)
        {
            var currentRecordId = record.NodeId;
            try
            {
                var homeLocationPath = locationPath;
                List<string> parentNames = new();
                if (record.Ancestors != null)
                {
                    var parentIds = record.Ancestors.Skip(1).ToList(); //except location id
                    if (parentIds.Count > 0)
                    {
                        #region Cache the parent node ids to be queried.
                        List<Guid> needQueryIds = new();
                        foreach (var parentId in parentIds)
                        {
                            if (!NodeNameCache.ContainsKey(parentId))
                            {
                                needQueryIds.Add(parentId);
                            }
                        }
                        #endregion
                        #region Add the parent nodes path that are not in the cache.
                        if (needQueryIds.Count > 0)
                        {
                            var dic = _explorerDao.QueryAll(o => needQueryIds.Contains(o.Id)).Select(o => new { o.Id, o.LeafName }).ToDictionary(o => o.Id);
                            needQueryIds.ForEach((id) =>
                            {
                                NodeNameCache.Add(id, dic[id].LeafName);
                            });
                        }
                        #endregion
                        parentNames = GetParentNames(parentIds);
                    }
                    #region If the current node is not of type PhysicalRecord and add it to the NodePathCache.
                    if (record.NodeType < (int)RMNodeLevel.PhysicalRecord && !NodeNameCache.ContainsKey(currentRecordId))
                    {
                        NodeNameCache.Add(currentRecordId, record.LeafName);
                    }
                    #endregion
                }
                return parentNames.Count > 0 ? $"{homeLocationPath}/{string.Join("/", parentNames)}" : homeLocationPath;
            }
            catch (Exception ex)
            {
                logger.Error($"An error while get physical node home location path, id:{currentRecordId}, message: {ex}");
                return string.Empty;
            }
        }

        private string GetLocationNodePath(Guid locationId)
        {
            if (!NodeNameCache.ContainsKey(locationId))
            {
                var locationPath = LocationManagementService.GetLocationPathById(locationId);
                NodeNameCache.Add(locationId, locationPath); //cache location path
                return locationPath;
            }
            return NodeNameCache[locationId];
        }

        private List<string> GetParentNames(List<Guid> parentIds)
        {
            List<string> names = new();
            foreach (var parentId in parentIds)
            {
                if (NodeNameCache.ContainsKey(parentId))
                {
                    names.Add(NodeNameCache[parentId]);
                }
            }
            return names;
        }

        public string GetPhyNodeHomeLocation(BaseRecordDto record)
        {
            try
            {
                var locationPath = GetLocationNodePath(record.LocationId);
                if (record.Ancestors != null) return GetPhysicalLocationFullPathByAncestors(record, locationPath); //new format data

                #region old format data to do
                StringBuilder path = new();
                path.Append(locationPath);
                if (record.NodeType != (int)RMNodeType.PhyBox)
                {
                    if (record.BoxId != Guid.Empty)
                    {
                        var parentBox = _explorerDao.QueryAll(r => r.Id == record.BoxId).FirstOrDefault();
                        path.Append($"/{parentBox?.LeafName}");
                    }
                    if (record.NodeType == (int)RMNodeType.PhyRecord)
                    {
                        var parentFile = _explorerDao.QueryAll(r => r.Id == record.FileId).FirstOrDefault();
                        path.Append($"/{parentFile?.LeafName}");
                    }
                }
                return path.ToString();
                #endregion
            }
            catch (Exception ex)
            {
                logger.Error($"An error while GetPhyNodeHomeLocation, id: {record.NodeId}, message: {ex}");
                return string.Empty;
            }
        }
    }
}
