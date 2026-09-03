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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.SharePoint.RMExplorer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace RAGlobalSearch.Discover
{
    public class GlobalSearchDiscover
    {
        protected AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(GlobalSearchDiscover));
        private IExplorerQueryService mExplorerQueryService;
        public IExplorerQueryService ExplorerQueryService
        {
            get
            {
                if (mExplorerQueryService == null)
                {
                    mExplorerQueryService = (IExplorerQueryService)PlatformWindsorManager.GetService(typeof(IExplorerQueryService));
                }
                return mExplorerQueryService;
            }
        }

        private ExplorerQueryV3Dto explorerQueryV3Dto;
        private string mUserId;
        private GlobalSearchAction mAction;
        private SourceFlag mFlag;
        private int mNodeLevel;
        public bool DiscoverFinish;

        public GlobalSearchDiscover(GlobalSearchActionDto dto)
        {
            explorerQueryV3Dto = dto.FilterInfo;
            mUserId = dto.UserId;
            TenantLocalValue.LogonUserId = mUserId;
            mAction = dto.Action;
            mFlag = (SourceFlag)dto.SourceFlag;
        }
        public void Run()
        {
            string tenantId = TenantLocalValue.LogonGroupId;
            string email = TenantLocalValue.LogonUserEmail;
            System.Threading.Tasks.Task t = System.Threading.Tasks.Task.Factory.StartNew(() => DoDiscover(tenantId, email, mUserId, mAction, mFlag));
            //Thread t = new Thread(DoDiscover);
            // t.IsBackground = true;
            // t.Start();
        }

        private void DoDiscover(string tenantId, string email, string userId, GlobalSearchAction action, SourceFlag flag)
        {
            try
            {
                TenantLocalValue.LogonGroupId = tenantId;
                TenantLocalValue.LogonUserEmail = email;
                TenantLocalValue.LogonUserId = userId;
                logger.Info($"Current tenant id:{tenantId}, user id:{userId}, action:{action.ToString()}, source flag:{flag.ToString()}");
                ExplorerPagingInfo pageInfo;
                do
                {
                    var result = ExplorerQueryService.QueryDataListWithoutTotalAsync(explorerQueryV3Dto).Result;
                    if (result != null && result.Datas != null && result.Datas.Count > 0)
                    {
                        var nodeTypes = result.Datas.Select(n => n.NodeType).Distinct().ToList();
                        string nodeTypesStr = string.Join(",", nodeTypes);
                        logger.Info($"Discover got {result.Datas.Count} items. NodeTypes:{nodeTypesStr}");
                        mNodeLevel = nodeTypes[0];
                        var filteredData = FilterResult(action, flag, result.Datas);
                        logger.Info($"Filtered data count:{filteredData.Count}");
                        if (filteredData.Count > 0)
                        {
                            GlobalSearchCache.Instance.DiscoverCache.AddBatch(filteredData);
                        }
                    }
                    pageInfo = result?.PagingInfo;
                }
                while (pageInfo != null && pageInfo.HasNextPage);
                DiscoverFinish = true;
                logger.Info("Discover finished.");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while discovering. Error:{e.ToString()}");
                DiscoverFinish = true;
            }
        }

        private List<BaseRecordDto> FilterResult(GlobalSearchAction action, SourceFlag flag, List<BaseRecordDto> data)
        {
            List<BaseRecordDto> result = new List<BaseRecordDto>();
            switch (flag)
            {
                case SourceFlag.SharePoint:
                    result = FilterSPData(action, data);
                    break;
                case SourceFlag.Exchange:
                    result = FilterEXOData(action, data);
                    break;
                case SourceFlag.FileSystem:
                    result = FilterFSData(action, data);
                    break;
                case SourceFlag.Physical:
                    result = FilterPhysicalData(action, data);
                    break;
                case SourceFlag.OneDrive:
                    result = FilterOneDriveData(action, data);
                    break;
                case SourceFlag.AzureFileShare:
                    result = FilterAzureFileShareData(action, data);
                    break;
                case SourceFlag.Box:
                    result = FilterBoxData(action, data);
                    break;
                case SourceFlag.Google:
                    result = FilterGoogleData(action, data);
                    break;         
                case SourceFlag.Teams:
                    result = FilterTeamsData(action, data);
                    break;
                case var f when (int)f >= 1000:
                    result = FilterCustomizeConnectorData(action, data);
                    break;
            }
            return result;
        }

        private List<BaseRecordDto> FilterGoogleData(GlobalSearchAction action, List<BaseRecordDto> data)
        {
            List<BaseRecordDto> result = new List<BaseRecordDto>();
            switch (action)
            {
                case GlobalSearchAction.Reclassify:
                    if (mNodeLevel == (int)RMNodeLevel.GoogleFolder)
                    {
                        result = data.Where(r => r.NodeType == (int)RMNodeLevel.GoogleFolder && 
                                      (r.RecordStatus != (int)RMRecordStatus.Destroyed && r.RecordStatus != (int)RMRecordStatus.RMDeleted)).ToList();
                    }
                    else
                    {
                        result = data.Where(r => r.NodeType == (int)RMNodeLevel.GoogleFile &&
                                      (r.RecordStatus != (int)RMRecordStatus.Destroyed && r.RecordStatus != (int)RMRecordStatus.RMDeleted)).ToList();
                    }
                    break;
            }
            return result;
        }

        private List<BaseRecordDto> FilterSPData(GlobalSearchAction action, List<BaseRecordDto> data)
        {
            List<BaseRecordDto> result = new List<BaseRecordDto>();
            switch (action)
            {
                case GlobalSearchAction.MoveTo:
                    result = data.Where(r => r.NodeType == (int)RMNodeLevel.Item && r.ExtensionForFile != "SharePoint Item" && r.RecordStatus != (int)RMRecordStatus.Archived).ToList();
                    break;
                case GlobalSearchAction.DeclareRecords:
                    result = data.Where(r => r.NodeType == (int)RMNodeLevel.Item && r.DeclareAsRecord == false && r.RecordStatus != (int)RMRecordStatus.Archived).ToList();
                    break;
                case GlobalSearchAction.UnDeclareRecords:
                    result = data.Where(r => r.NodeType == (int)RMNodeLevel.Item && r.DeclareAsRecord == true && r.RecordStatus != (int)RMRecordStatus.Archived).ToList();
                    break;
                case GlobalSearchAction.AddRecordLabel:
                    result = data.Where(r => r.NodeType == (int)RMNodeLevel.Item && r.LockedByRecordLabel == false && r.RecordStatus != (int)RMRecordStatus.Archived).ToList();
                    break;
                case GlobalSearchAction.RemoveRecordLabel:
                    result = data.Where(r => r.NodeType == (int)RMNodeLevel.Item && r.LockedByRecordLabel == true && r.RecordStatus != (int)RMRecordStatus.Archived).ToList();
                    break;
                case GlobalSearchAction.Reclassify:
                    var allowReclassifyNodeTypes = new List<int> { (int)RMNodeLevel.Item, (int)RMNodeLevel.Folder };
                    result = data.Where(r => allowReclassifyNodeTypes.Contains(r.NodeType) && r.RecordStatus != (int)RMRecordStatus.Archived).ToList();
                    break;
            }
            return result;
        }
        private List<BaseRecordDto> FilterEXOData(GlobalSearchAction action, List<BaseRecordDto> data)
        {
            List<BaseRecordDto> result = new List<BaseRecordDto>();
            switch (action)
            {
                case GlobalSearchAction.Reclassify:
                    result = data.Where(r => r.NodeType == (int)NodeLevel.ExchangeOnlineItem).ToList();
                    break;
            }
            return result;
        }
        private List<BaseRecordDto> FilterFSData(GlobalSearchAction action, List<BaseRecordDto> data)
        {
            List<BaseRecordDto> result = new List<BaseRecordDto>();
            switch (action)
            {
                case GlobalSearchAction.Reclassify:
                    result = data.Where(r => r.NodeType == (int)NodeLevel.FSFile
                    || r.NodeType == (int)NodeLevel.FSFolder).ToList();
                    break;
            }
            return result;
        }
        private List<BaseRecordDto> FilterPhysicalData(GlobalSearchAction action, List<BaseRecordDto> data)
        {
            List<BaseRecordDto> result = new List<BaseRecordDto>();
            switch (action)
            {
                case GlobalSearchAction.AccessControl:
                    result = data.Where(r => r.NodeType == (int)RMNodeLevel.PhysicalBox || r.NodeType == (int)RMNodeLevel.PhysicalFile || r.NodeType == (int)RMNodeLevel.PhysicalCustom).ToList();
                    break;
                case GlobalSearchAction.Reclassify:
                    result = data.Where(r => r.NodeType == (int)RMNodeLevel.PhysicalBox || r.NodeType == (int)RMNodeLevel.PhysicalFile).ToList();
                    break;
                case GlobalSearchAction.PhysicalBulkUpdate:
                    result = data.Where(r => r.RecordStatus != (int)RMRecordStatus.Destroyed).ToList();
                    break;
            }
            return result;
        }
        private List<BaseRecordDto> FilterOneDriveData(GlobalSearchAction action, List<BaseRecordDto> data)
        {
            List<BaseRecordDto> result = new List<BaseRecordDto>();
            switch (action)
            {
                case GlobalSearchAction.MoveTo:
                    result = data.Where(r => r.NodeType == (int)RMNodeLevel.Item && r.ExtensionForFile != "SharePoint Item").ToList();
                    break;
                case GlobalSearchAction.DeclareRecords:
                    result = data.Where(r => r.NodeType == (int)RMNodeLevel.Item && r.DeclareAsRecord == false).ToList();
                    break;
                case GlobalSearchAction.UnDeclareRecords:
                    result = data.Where(r => r.NodeType == (int)RMNodeLevel.Item && r.DeclareAsRecord == true).ToList();
                    break;
                case GlobalSearchAction.AddRecordLabel:
                    result = data.Where(r => r.NodeType == (int)RMNodeLevel.Item && r.LockedByRecordLabel == false).ToList();
                    break;
                case GlobalSearchAction.RemoveRecordLabel:
                    result = data.Where(r => r.NodeType == (int)RMNodeLevel.Item && r.LockedByRecordLabel == true).ToList();
                    break;
                case GlobalSearchAction.Reclassify:
                    var allowReclassifyNodeTypes = new List<int> { (int)RMNodeLevel.Item, (int)RMNodeLevel.Folder };
                    result = data.Where(r => allowReclassifyNodeTypes.Contains(r.NodeType)).ToList();
                    break;
            }
            return result;
        }
        private List<BaseRecordDto> FilterAzureFileShareData(GlobalSearchAction action, List<BaseRecordDto> data)
        {
            switch(action)
            {
                case GlobalSearchAction.Reclassify:
                    return data.Where(item => item.NodeType == (int)RMNodeLevel.AzureFileShareFile).ToList();
            }

            return new List<BaseRecordDto>();
        }

        private List<BaseRecordDto> FilterBoxData(GlobalSearchAction action, List<BaseRecordDto> data)
        {
            switch (action)
            {
                //Todo: include BoxFolder level for reclassify job in the future.
                case GlobalSearchAction.Reclassify:
                    return data.Where(item => item.NodeType == (int)RMNodeLevel.BoxFile).ToList();
            }

            return new List<BaseRecordDto>();
        }

        private List<BaseRecordDto> FilterCustomizeConnectorData(GlobalSearchAction action, List<BaseRecordDto> data)
        {
            switch (action)
            {
                case GlobalSearchAction.Reclassify:
                    return data.Where(item => item.NodeType == (int)RMNodeLevel.CustomizeConnectorItem).ToList();
            }

            return new List<BaseRecordDto>();
        }

        private List<BaseRecordDto> FilterTeamsData(GlobalSearchAction action, List<BaseRecordDto> data)
        {
            List<BaseRecordDto> result = new List<BaseRecordDto>();
            switch (action)
            {
                case GlobalSearchAction.MoveTo:
                    result = data.Where(r => r.NodeType == (int)RMNodeLevel.Item && r.ExtensionForFile != "SharePoint Item" && r.RecordStatus != (int)RMRecordStatus.Archived).ToList();
                    break;
                case GlobalSearchAction.DeclareRecords:
                    result = data.Where(r => r.NodeType == (int)RMNodeLevel.Item && r.DeclareAsRecord == false && r.RecordStatus != (int)RMRecordStatus.Archived).ToList();
                    break;
                case GlobalSearchAction.UnDeclareRecords:
                    result = data.Where(r => r.NodeType == (int)RMNodeLevel.Item && r.DeclareAsRecord == true && r.RecordStatus != (int)RMRecordStatus.Archived).ToList();
                    break;
                case GlobalSearchAction.AddRecordLabel:
                    result = data.Where(r => r.NodeType == (int)RMNodeLevel.Item && r.LockedByRecordLabel == false && r.RecordStatus != (int)RMRecordStatus.Archived).ToList();
                    break;
                case GlobalSearchAction.RemoveRecordLabel:
                    result = data.Where(r => r.NodeType == (int)RMNodeLevel.Item && r.LockedByRecordLabel == true && r.RecordStatus != (int)RMRecordStatus.Archived).ToList();
                    break;
                case GlobalSearchAction.Reclassify:
                    var allowReclassifyNodeTypes = new List<int> { (int)RMNodeLevel.Item, (int)RMNodeLevel.Folder };
                    result = data.Where(r => allowReclassifyNodeTypes.Contains(r.NodeType) && r.RecordStatus != (int)RMRecordStatus.Archived).ToList();
                    break;
            }
            return result;
        }
    }
}
