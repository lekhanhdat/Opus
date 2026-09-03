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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.AzureBlobStorage;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using RAExportCommon;
using RAManualApproval.ExportAction.ExportTermAndRule;
using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;

namespace RAManualApproval.DataInfoProcessingCenter
{
    public class DataInfoProcessing
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(ExportTermAndRuleExportProcessor));

        private static readonly IDownloadDataInfoDao _downloadDataInfoDao = PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private static readonly RMRetryer Retryer = RMRetryerBuilder.CreateBuilder().Build();

        private string FullPath { get; set; }

        private string FolderPath { get; set; }

        private string JobId { get; set; }

        public static List<RMDownloadDataInfo> GetDownloadDataInfoStatus(List<int> status)
        {
            return _downloadDataInfoDao.GetDownloadDataInfosByStatus(status);
        }

        public static void UpdateDownloadDataStatus(RMDownloadDataInfo downCenterInfo, DownloadContentJobStatus downloadStatus)
        {
            using (new PerformanceScope("Update download data ", $"Download data status is {downloadStatus}"))
            {
                downCenterInfo.JobStatus = (int)downloadStatus;
                var success = _downloadDataInfoDao.UpdateDownloadInfo(downCenterInfo);
                if (success)
                {
                    _logger.Info($"Update download file status to {downloadStatus} finished.");
                }
                else
                {
                    _logger.Info($"Update download file status to {downloadStatus} failed, retry update.");
                    success = _downloadDataInfoDao.UpdateDownloadInfo(downCenterInfo);
                    var status = success ? "finished" : "failed";
                    _logger.Info($"Update retry download file {status}.");
                }
            }
        }

        public static async Task<FileInfo> UploadBlobAsync(string folderPath, string jobId)
        {
            using (new PerformanceScope("Upload blob to azure storage", "", true))
            {
                AvePoint.GCommon.ZipUtil.ZipFolder(folderPath, folderPath + ".zip", Encoding.UTF8);
                var customId = TenantLocalValue.LogonGroupId;
                var blobName = SecurityUtils.SafeCombinePath(customId, jobId + ".zip");
                try
                {
                    await Retryer.RetryAsync(() =>
                    {
                        blobName = DownloadCenterUtility.UploadStorageForDownloadCenter(blobName, folderPath + ".zip");
                        _logger.Info($"Upload report profile details success");
                        return Task.CompletedTask;
                    });
                }
                catch (Exception e)
                {
                    _logger.Error($"Upload report profile details failed,error is :{e}");
                    throw;
                }

                _logger.Info($"finish to upload blob name:{blobName}");
                return new FileInfo(folderPath + ".zip");
            }
        }

        public static async Task<string> GetExportFileName(string i18NExportNamne)
        {
            DateTime nowTime = DateTime.UtcNow;
            var nowTimeStr = (await GeneralSettingService.ConvertTiksToDateTimeAsync(nowTime.Ticks, false)).DataTime.ToString(AveDateTimeUtility.DATETYPE022);
            return I18NEntity.GetString(i18NExportNamne) + "_" + nowTimeStr;
        }
    }
}
