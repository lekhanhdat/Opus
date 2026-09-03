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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Global.Explorer;
using AvePoint.RA.Contract.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using AvePoint.GCommon;

namespace RAFileSystem.SharePoint.GlobalSearch.Discover
{
    public class GlobalSearchDiscover
    {
        protected AveLogger logger = AveLogger.GetInstance(typeof(GlobalSearchDiscover));
        private GlobalSearchAction mAction;
        //private SourceFlag mFlag;
        private string mJobId;      
        public bool DiscoverFinish;


        public GlobalSearchDiscover(string jobId, GlobalSearchAction action)
        {
            //logger.Info($"Query info:{SerializerHelper.SerializeByDataContractSerializer(dto.FilterInfo)}");          
            mAction = action;
            mJobId = jobId;          
        }
        public void Run()
        {

            System.Threading.Tasks.Task t = System.Threading.Tasks.Task.Factory.StartNew(() => DoDiscover(mAction, mJobId));
            //Thread t = new Thread(DoDiscover);
            // t.IsBackground = true;
            // t.Start();
        }

        private void DoDiscover(GlobalSearchAction action, string jobId)
        {
            try
            {

                logger.Info($"Start to query data for global seach. Action:{action.ToString().LogBase64()}");
                ExplorerPagingInfo mPageInfo = new ExplorerPagingInfo()
                {
                    HasNextPage = true,
                    PageIndex = "",
                    PageSize = 100
                };
                GlobalSearchQueryDto dto = new GlobalSearchQueryDto()
                {
                    JobId = jobId,
                    PageInfo = mPageInfo
                };
                do
                {
                    var result = HybridApiClient.Instance.QueryDataForGlobalSearch(dto);
                    if (result != null && result.Data != null && result.Data.Count > 0)
                    {
                        var nodeTypes = result.Data.Select(n => n.NodeType).Distinct().ToList();
                        string nodeTypesStr = string.Join(",", nodeTypes);
                        logger.Info($"Discover got {result.Data.Count} items. NodeTypes:{nodeTypesStr.LogBase64()}");
                        var filteredData = FilterResult(action, result.Data);
                        logger.Info($"Filtered data count:{filteredData.Count}");
                        if (filteredData.Count > 0)
                        {
                            GlobalSearchCache.Instance.DiscoverCache.AddBatch(filteredData);
                        }
                    }
                    mPageInfo = result.PageInfo;
                    dto.PageInfo = mPageInfo;
                }
                while (mPageInfo != null && mPageInfo.HasNextPage);
                DiscoverFinish = true;
                logger.Info("Discover finished.");
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while discovering. Error:{e.ToString()}");
                DiscoverFinish = true;
            }
        }

        private List<RecordDto> FilterResult(GlobalSearchAction action, List<RecordDto> data)
        {
            List<RecordDto> result = new List<RecordDto>();
            // switch (flag)
            {
                //case SourceFlag.SharePoint:
                //    result = FilterSPData(action, data);
                //    break;
                //case SourceFlag.Exchange:
                //    result = FilterEXOData(action, data);
                //    break;
                //case SourceFlag.FileSystem:
                //    result = FilterFSData(action, data);
                //    break;
                //case SourceFlag.Physical:
                //    result = FilterPhysicalData(action, data);
                //    break;.
                //case SourceFlag.SharePointOnPrem:
                result = FilterSPOnPremData(action, data);
                //break;
            }
            return result;
        }

        private List<RecordDto> FilterSPOnPremData(GlobalSearchAction action, List<RecordDto> data)
        {
            List<RecordDto> result = new List<RecordDto>();
            switch (action)
            {
                //case GlobalSearchAction.MoveTo:
                //    result = data.Where(r => r.NodeType == (int)RMNodeLevel.Item && r.ExtensionForFile != "SharePoint Item").ToList();
                //    break;
                case GlobalSearchAction.DeclareRecords:
                    result = data.Where(r => r.NodeType == 500 && r.DeclareAsRecord == false).ToList();
                    break;
                case GlobalSearchAction.UnDeclareRecords:
                    result = data.Where(r => r.NodeType == 500 && r.DeclareAsRecord == true).ToList();
                    break;
                case GlobalSearchAction.Reclassify:
                    result = data.Where(r => r.NodeType == 500).ToList();
                    break;
            }
            return result;
        }

        //private List<RecordDto> FilterSPData(GlobalSearchAction action, List<RecordDto> data)
        //{
        //    List<RecordDto> result = new List<RecordDto>();
        //    switch (action)
        //    {
        //        case GlobalSearchAction.MoveTo:
        //            result = data.Where(r => r.NodeType == (int)RMNodeLevel.Item && r.ExtensionForFile != "SharePoint Item").ToList();
        //            break;
        //        case GlobalSearchAction.DeclareRecords:
        //            result = data.Where(r => r.NodeType == (int)RMNodeLevel.Item && r.DeclareAsRecord == false).ToList();
        //            break;
        //        case GlobalSearchAction.UnDeclareRecords:
        //            result = data.Where(r => r.NodeType == (int)RMNodeLevel.Item && r.DeclareAsRecord == true).ToList();
        //            break;
        //        case GlobalSearchAction.Reclassify:
        //            result = data.Where(r => r.NodeType == (int)RMNodeLevel.Item).ToList();
        //            break;
        //    }
        //    return result;
        //}
        //private List<RecordDto> FilterEXOData(GlobalSearchAction action, List<RecordDto> data)
        //{
        //    List<RecordDto> result = new List<RecordDto>();
        //    switch (action)
        //    {
        //        case GlobalSearchAction.Reclassify:
        //            result = data.Where(r => r.NodeType == (int)NodeLevel.ExchangeOnlineItem).ToList();
        //            break;
        //    }
        //    return result;
        //}
        //private List<RecordDto> FilterFSData(GlobalSearchAction action, List<RecordDto> data)
        //{
        //    List<RecordDto> result = new List<RecordDto>();
        //    switch (action)
        //    {
        //        case GlobalSearchAction.Reclassify:
        //            result = data.Where(r => r.NodeType == (int)NodeLevel.FSFile
        //            || r.NodeType == (int)NodeLevel.FSFolder).ToList();
        //            break;
        //    }
        //    return result;
        //}
        //private List<RecordDto> FilterPhysicalData(GlobalSearchAction action, List<RecordDto> data)
        //{
        //    List<RecordDto> result = new List<RecordDto>();
        //    switch (action)
        //    {
        //        case GlobalSearchAction.AccessControl:
        //        case GlobalSearchAction.Reclassify:
        //            result = data.Where(r => r.NodeType == (int)RMNodeLevel.PhysicalBox || r.NodeType == (int)RMNodeLevel.PhysicalFile).ToList();
        //            break;
        //    }
        //    return result;
        //}

    }
}
