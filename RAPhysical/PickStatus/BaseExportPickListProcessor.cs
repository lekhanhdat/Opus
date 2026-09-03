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
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.PickStatus
{
    public abstract class BaseExportPickListProcessor : BasePickStatusProcessor
    {
        protected IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        protected IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        protected ILocationManagementService LocationManagementService => PlatformWindsorManager.GetService<ILocationManagementService>();

        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();
        protected static readonly int CountOfOneSheet = 63375;
        protected static readonly int QueryDBGroupCount = 100;
        protected int CurrentIndex = 0;
        protected string FileNamePrefix;
        protected string SheetName;
        protected int SheetIndex = 0;
        protected string FolderPath;
        protected string FullPath;
        protected RMDownloadDataInfo DownCenterInfo;
        protected List<BaseRecordDto> sheetList = new List<BaseRecordDto>();
        protected bool CreateEmptyFile = true;
        public BaseExportPickListProcessor(JobType jobType, string jobId) : base(jobType, jobId)
        {
            var mainJobId = jobId.Split('_')[0];
            DownCenterInfo = DownloadDataInfoDao.GetDownloadDataInfosByStatus(new List<int>() { (int)DownloadContentJobStatus.Wait }).Where(item => item.JobId == mainJobId).FirstOrDefault();
        }
        protected override async Task PrepareProcessAsync()
        {
            var generalSetting = await GeneralSettingService.GetGeneralSettingAsync();
            var nowDateTimeStr = GeneralSettingService.ConvertTiksToDateTime(generalSetting, DateTime.UtcNow.Ticks, false).DataTime.ToString("yyyyMMddhhmmss");
            var fileName = FileNamePrefix + "_" + nowDateTimeStr;
            FolderPath = GetTempFolder() + Path.DirectorySeparatorChar + fileName + Guid.NewGuid();
            FullPath = AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(FolderPath, fileName + ".xlsx");

            DownCenterInfo.JobStatus = (int)DownloadContentJobStatus.InProgress;
            await DownloadDataInfoDao.UpdateAsync(DownCenterInfo);
        }

        protected async Task UploadBlobAsync(string folderPath, string jobId)
        {
            AvePoint.GCommon.ZipUtil.ZipFolder(folderPath, folderPath + ".zip", Encoding.UTF8);
            var customId = TenantLocalValue.LogonGroupId;
            var blobName = Path.Combine(customId, jobId + ".zip");
            var retryFailed = false;
            try
            {
                await Retryer.RetryAsync(() =>
                {
                    blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, folderPath + ".zip");
                    logger.Info($"Upload pick list report success");
                    return Task.CompletedTask;
                });
            }
            catch
            {
                retryFailed = true;
                logger.Error($"Upload pick list report failed");
            }
            if (retryFailed)
            {
                return;
            }

            logger.Info($"finish to upload blob name:{blobName}");
        }

        protected abstract string GetTempFolder();

        protected string GetPhysicalObjectFullPath(BaseRecordDto oPhy, bool isReplaceI18NKey = true)
        {
            var path = new StringBuilder();
            try
            {
                if (oPhy != null)
                {
                    path.Append(LocationManagementService.GetLocationPathById(oPhy.LocationId, isReplaceI18NKey));
                }

                if (oPhy.Ancestors != null) return GetPhysicalLocationFullPathByAncestors(oPhy, path.ToString(), ExplorerDao); //new format data

                //old format data
                if (oPhy.NodeType != (int)RMNodeType.PhyBox)
                {
                    if (oPhy.BoxId != Guid.Empty)
                    {
                        var parentBox = ExplorerDao.QueryAll(r => r.Id == oPhy.BoxId).FirstOrDefault();
                        path.Append($"/{parentBox?.LeafName}");
                    }
                    if (oPhy.NodeType == (int)RMNodeType.PhyRecord)
                    {
                        var parentFile = ExplorerDao.QueryAll(r => r.Id == oPhy.FileId).FirstOrDefault();
                        path.Append($"/{parentFile?.LeafName}");
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Get PhysicalObject FullPath by id: [{oPhy.Id}], error: [{ex.ToString()}]");
            }
            return path.ToString();
        }

        protected static string GetPhysicalLocationFullPathByAncestors(BaseRecordDto record, string locationPath, IExplorerDao ExplorerDao)
        {
            if (record.Ancestors == null || record.Ancestors.Count == 1) return locationPath;
            Guid[] ancestors = new Guid[record.Ancestors.Count - 1];
            record.Ancestors.CopyTo(1, ancestors, 0, record.Ancestors.Count - 1);//first one is location id,  do not need it
            var path = new StringBuilder(locationPath);
            var dic = ExplorerDao.QueryAll(o => Enumerable.Contains(ancestors, o.Id)).Select(o => new { o.Id, o.LeafName }).ToDictionary(o => o.Id);
            foreach (var r in ancestors)
            {
                path.Append($"/{dic[r].LeafName}");
            }

            return path.ToString();
        }

    }
}
