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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.ExportDiscoveryProfile;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using RAManualApproval.DataInfoProcessingCenter;
using RAManualApproval.ExportAction.ExportTermAndRule;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.RA.Contract.Discovery.DiscoveryExtension;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.Contract.Exceptions;

namespace RAManualApproval.Discover.ExportDiscoveryProfile
{
    public class ExportDiscoveryProfileProcessor
    {
        private static readonly RALogger _logger = RALogger.GetInstance(typeof(ExportTermAndRuleExportProcessor));
        private static readonly IDownloadDataInfoDao DownloadDataInfoDao = PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private IRMDiscoveryOffice365ProfileService DiscoveryOffice365ProfileService => PlatformWindsorManager.GetService<IRMDiscoveryOffice365ProfileService>();
        private string FileName { get; set; }

        private string FolderPath { get; set; }

        private string JobId { get; set; }

        private string ProfileId{ get; set; }

        private Guid O365TenantId { get; set; }

        private string DiscoveryType { get; set; }

        private bool IsDescending { get; set; }

        private RMDownloadDataInfo DownloadDataInfo { get; set; }

        public ExportDiscoveryProfileProcessor(string jobId, string o365TenantId, string discoveryType, string profileId, bool isDesc)
        {
            this.JobId = jobId;
            this.DiscoveryType = discoveryType;
            this.O365TenantId = new Guid(o365TenantId);
            this.ProfileId = profileId;
            this.IsDescending = isDesc;
            ExportDiscoveryProfileJobManager.Init(JobId, JobType.DiscoveryExportO365Profile);         
        }

        public async Task RunAsync()
        {
            var profileName = string.Empty;
            AvePoint.RA.Contract.Discovery.Model.Profile.RMDiscoveryProfileDataInfo profile = new ();
            try
            {
                DownloadDataInfo = DataInfoProcessing.GetDownloadDataInfoStatus(new List<int>() { (int)DownloadContentJobStatus.Wait }).Where(item => item.JobId == JobId).First();
                profile = await DiscoveryOffice365ProfileService.GetProfileInfoByIdAsync(O365TenantId, new Guid(ProfileId), DiscoveryType);     
                DataInfoProcessing.UpdateDownloadDataStatus(DownloadDataInfo, DownloadContentJobStatus.InProgress);

                var validProfileName = string.Empty;
                if (profile.IsBuildIn)
                {
                    validProfileName = DiscoveryType + " " + I18NEntity.GetString(profile.Name).ToValidName();
                }
                else
                {
                    validProfileName = I18NEntity.GetString(profile.Name).ToValidName();
                }
                FileName = await DataInfoProcessing.GetExportFileName(validProfileName) + "_" + profile.Id;
                FolderPath = JobReportUtility.GetDownloadDiscoveryExportReportTempleFolder("Temple") + Path.DirectorySeparatorChar + FileName;

                await DiscoveryOffice365ProfileService.GenerateExportProfileAsync(new ExportDiscoveryProfileParam
                {
                    FileName = this.FileName,
                    FolderPath = this.FolderPath,
                    DiscoveryType = this.DiscoveryType,
                    ProfileId = this.ProfileId,
                    O365TenantId = this.O365TenantId,
                    JobId = this.JobId,
                    SortBy = profile.SortBy,
                    IsDescending = this.IsDescending,
                    PageIndex = 0,
                    PageSize = 5000,
                }, profile);

                _logger.Info($"[Export] Generate export profile successfully");
                var fileInfo = await DataInfoProcessing.UploadBlobAsync(FolderPath, JobId);
                
                if (fileInfo != null)
                {
                    DownloadDataInfo.FileSize = fileInfo.Length;
                }
                DownloadDataInfo.BlobSasUri = await DownloadCenterUtility.GenerateSasUri();

                var successJobDetail = profile.GenerateExportProfileActionJobDetail(JobDetailsStatus.Successful, string.Empty);
                ExportDiscoveryProfileJobManager.HasSucceedDetail = true;
                DataInfoProcessing.UpdateDownloadDataStatus(DownloadDataInfo, DownloadContentJobStatus.Finished);
                ExportDiscoveryProfileJobManager.RecordJobDetail(successJobDetail);
            }
            catch (JobStopException)
            {
                _logger.Warn($"Export Discovery Data for Microsoft 365 has been stopped by user");
                throw;
            }
            catch (Exception ex)
            {
                var errorMessenger = ex.Message.Contains("Sequence contains no elements") ? $"{I18NEntity.GetString("RM_JS_JM_DiscoveryExportProfileNotFound")}" :  ex.Message;
                var failJobDetail = profile.GenerateExportProfileActionJobDetail(JobDetailsStatus.Failed, errorMessenger);               
                ExportDiscoveryProfileJobManager.HasFailedDetail = true;
                ExportDiscoveryProfileJobManager.JobComment = errorMessenger;
                DataInfoProcessing.UpdateDownloadDataStatus(DownloadDataInfo, DownloadContentJobStatus.Failed);
                ExportDiscoveryProfileJobManager.RecordJobDetail(failJobDetail);
                _logger.Error($"Export Discovery Data for Microsoft 365 failed , {ex}");
            }
            finally
            {
                ExportDiscoveryProfileJobManager.SetJobFinished();
                PerformanceMonitor.WritePerformanceResult();
            }
        }
    }
}
