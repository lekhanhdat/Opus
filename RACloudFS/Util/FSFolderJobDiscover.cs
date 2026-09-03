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
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using Records.FS.Reclassify;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RACloudFS.Util
{
    public class FSFolderJobDiscover : IDisposable
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(FSFolderJobDiscover));
        private ExplorerDao _explorerDao = new ExplorerDao();
        private IRMReportManager mReportManger;
        public IRMReportManager ReportManager
        {
            get
            {
                if (mReportManger == null)
                {
                    mReportManger = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManger;
            }
        }

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

        public void Dispose()
        {
            if (_explorerDao != null)
            {
                _explorerDao.Dispose();
            }
        }

        public Task<List<Record>> ProcessFilesAsync(Guid folderId)
        {
            //List<Record> tempList = new List<Record>();
            //bool hasNext = true;
            //string pageIndex = string.Empty;
            //var pateSize = 500;
            //List<Record> datas = new List<Record>();
            //while (hasNext)
            //{
            //    Tuple<IEnumerable<Record>, string> result = _explorerDao.QueryByPage(o =>
            //    o.SourceFlag == (int)SourceFlag.FileSystem
            //    && o.RecordStatus == (int)RMRecordStatus.Active
            //    && o.ParentId == folderId
            //    && o.NodeType == (int)RMNodeLevel.FSFile, pateSize, pageIndex);
            //    hasNext = !string.IsNullOrEmpty(result.Item2);
            //    pageIndex = result.Item2;
            //    datas = result.Item1.ToList();
            //    tempList.AddRange(datas);
            //    ReportManager.IncreaseBase(datas.Count());
            //    logger.Debug($"Got {datas.Count()} children under the folder");
            //}

            return GetFilesV2Async(new List<Guid>() { folderId });
        }

        private async Task<List<Record>> GetFilesV2Async(List<Guid> parentIds)
        {
            ExplorerPagingInfo pageInfo;
            var explorerQueryV2Dto = GetQueryDto(parentIds);
            List<Record> records = new List<Record>();
            do
            {
                var result = await ExplorerQueryService.QueryDataListWithoutTotalAsync(explorerQueryV2Dto);
                if (result != null && result.Datas != null && result.Datas.Count > 0)
                {
                    var tempRecords = _explorerDao.GetRecordByIds(result.Datas.Select(r => r.Id).ToList());
                    records.AddRange(tempRecords);
                    ReportManager.IncreaseBase(tempRecords.Count);
                    logger.Debug($"Got {tempRecords.Count} children under the folder");
                }
                pageInfo = result?.PagingInfo;
            }
            while (pageInfo != null && pageInfo.HasNextPage);
            logger.Debug($"Total file count under folder is {records.Count}");
            return records;
        }

        private ExplorerFilterOptionV2 AssembleFilterOption(SourceFlag sourceFlag, List<Guid> parentIds)
        {
            var sourceFlags = new List<SourceFlag>() { SourceFlag.FileSystem };
            var nodeTypes = new List<RMNodeLevel>() { RMNodeLevel.FSFile };
            var rmRecordStatus = new List<RMRecordStatus>() { RMRecordStatus.Active };
            return new ExplorerFilterOptionV2()
            {
                SourceFlags = sourceFlags,
                NodeTypes = nodeTypes,
                Status = rmRecordStatus,
                ParentIds = parentIds
            };
        }

        private ExplorerQueryV2Dto GetQueryDto(List<Guid> parentIds)
        {
            return new ExplorerQueryV2Dto()
            {
                QueryOption = new ExplorerQueryOptionV2()
                {
                    FilterOption = AssembleFilterOption(SourceFlag.FileSystem, parentIds)
                },
                PagingInfo = new AvePoint.RA.Contract.RMWeb.ExplorerPagingInfo()
                {
                    PageIndex = "",
                    PageSize = 500
                }
            };
        }

        public List<Record> ProcessSubFolders(string folderPath)
        {
            List<Record> tempList = new List<Record>();
            bool hasNext = true;
            string pageIndex = string.Empty;
            var pateSize = 5000;
            List<Record> datas = new List<Record>();
            folderPath = folderPath.Replace('/','\\');  //Path.Combine  斜线不统一
            var parentFullPath = folderPath;
            if (!parentFullPath.EndsWith("\\"))
            {
                parentFullPath += "\\";
            }
            while (hasNext)
            {
                Tuple<IEnumerable<Record>, string> result = _explorerDao.QueryByPage(o =>
                o.SourceFlag == (int)SourceFlag.FileSystem
                && (o.DirPath.Contains(parentFullPath) || o.DirPath == folderPath)
                && o.NodeType == (int)RMNodeLevel.FSFolder, pateSize, pageIndex);
                hasNext = !string.IsNullOrEmpty(result.Item2);
                pageIndex = result.Item2;
                datas = result.Item1.ToList();
                tempList.AddRange(datas);
                ReportManager.IncreaseBase(datas.Count());
                logger.Debug($"Got {datas.Count()} children under the folder");
            }

            return tempList;
        }
    }
}
