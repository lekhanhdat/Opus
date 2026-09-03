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
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.Discovery.Office365.License;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.WIF;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Controllers.Discovery.Office365
{
    [RMApiAuthorize(RMDiscoveryPermissionMasks.AccessAll, preferred: false)]
    public class RMDiscoveryOffice365DuplicationDataApiController : BaseApiController
    {
        private readonly IRALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOffice365DuplicationDataApiController));
        private readonly IRMDiscoveryOffice365ExportJobService _exportJobService = PlatformWindsorManager.GetService<IRMDiscoveryOffice365ExportJobService>();
        private readonly IRMDiscoveryOffice365TenantConfigurationDao _configurationDao = new RMDiscoveryOffice365TenantConfigurationDao();
        private readonly IRMDiscoveryOffice365OptimizationService _optimizationService = PlatformWindsorManager.GetService<IRMDiscoveryOffice365OptimizationService>();
        private static readonly HashSet<RMDiscoveryJobStatus> ProcessingJobStatuses = new HashSet<RMDiscoveryJobStatus>
        {
            RMDiscoveryJobStatus.Preparing,
            RMDiscoveryJobStatus.Waiting,
            RMDiscoveryJobStatus.Pending,
            RMDiscoveryJobStatus.Running,
            RMDiscoveryJobStatus.Completing,
        };

        [HttpPost]
        public async Task<RAReturnMessage> ExportDuplicationReport([FromBody] string o365TenantId)
        {
            var runningJobResult = await GetRunningJobResultAsync();
            if (runningJobResult != null) return runningJobResult;

            return await _exportJobService.ExportDuplicationReportAsync(o365TenantId);
        }

        [HttpPost]
        public async Task<RAReturnMessage> CleanupDiscoveryDuplication()
        {
            var result = new RAReturnMessage { MessageType = RAMessageType.Successful };
            RMDiscoveryOffice365RuleInfoDao ruleInfoDao = new RMDiscoveryOffice365RuleInfoDao();
            try
            {
                if(!await RMDiscoveryOffice365LicenseHelper.IsAllowedToCleanupDiscoveryDuplicationDataAsync())
                {
                    _logger.Warn("The current license does not allow to cleanup discovery duplication data.");
                    throw new InvalidOperationException(I18NEntity.GetString("RM_FA_Discovery_RunJobFailed"));
                }

                if (Request.Form["CleanupInfo"] is StringValues cleanupInfo && string.IsNullOrWhiteSpace(cleanupInfo))
                {
                    _logger.Warn("CleanupInfo is required for cleanup discovery duplication data, but it is missing in the request.");
                    throw new InvalidOperationException(I18NEntity.GetString("RM_FA_Discovery_RunJobFailed"));
                }

                if (Request.Form["O365TenantId"] is StringValues o365TenantIdStr && !Guid.TryParse(o365TenantIdStr, out var o365TenantId))
                {
                    _logger.Warn("Cleanup discovery duplication data failed due to invalid O365TenantId: {0}", o365TenantIdStr);
                    throw new InvalidOperationException(I18NEntity.GetString("RM_FA_Discovery_RunJobFailed"));
                }

                if (!await ruleInfoDao.CheckExistingRuleByAnalyzeMethodsAsync(true, RMDiscoveryRuleAnalyseMethod.DuplicatedDocument))
                {
                    _logger.Warn("No duplication rule exists for the tenant {0}.", o365TenantId);
                    throw new InvalidOperationException(I18NEntity.GetString("RM_FA_Discovery_ExportDuplicationReport_RunJobFailed_NoDuplicationRule"));
                }

                var runningJobResult = await GetRunningJobResultAsync();
                if (runningJobResult != null) return runningJobResult;

                var tempRoot = Path.Combine("DiscoveryDuplicationReport", $"Report_{DateTime.UtcNow.Ticks.ToString()}");
                var zipFilePath = tempRoot + JobMonitorConstants.ZIP;
                Directory.CreateDirectory(tempRoot);
                await SaveUploadedFilesAsync(tempRoot);
                ZipUtil.ZipFolder(tempRoot, zipFilePath, Encoding.UTF8);
                Directory.Delete(tempRoot, recursive: true);
                await _exportJobService.UploadDuplicationReportToBlobAsync(zipFilePath);
                System.IO.File.Delete(zipFilePath);

                await _configurationDao.AddOrUpdateAsync(o365TenantId, RMDiscoveryO365TenantConfigurationType.DuplicationReportConfiguration, cleanupInfo);
                _logger.Info("Upload duplication data zip file to blob storage successfully.");
                await _optimizationService.RunCleanUpDuplicateDataJob(cleanupInfo, o365TenantIdStr);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to run cleanup discovery duplication job.", ex);
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = ex.Message;
                if (!I18NEntity.HasKey(ex.Message))
                {
                    result.ErrorMessage = I18NEntity.GetString("RM_FA_Discovery_RunJobFailed");
                }
            }
            return result;
        }

        private async Task<RAReturnMessage> GetRunningJobResultAsync()
        {
            var jobDao = new RMDiscoveryOffice365JobDao();
            var (hasJob, jobInfo) = await jobDao.TryGetLatestMainJobAsync();

            if (!hasJob || jobInfo == null || !ProcessingJobStatuses.Contains(jobInfo.Status)) return null;

            _logger.Warn($"The discovery job is in processing status: {jobInfo.Status}, the requested operation cannot proceed.");
            var key = "RM_FA_Discovery_ExportDuplicationReport_RunJobFailed_IsRunningJobDiscovey";
            var rawMessage = $"{key}{I18NEntity.Separator}{jobInfo.Status}";
            Response.StatusCode = 403;
            return new RAReturnMessage { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetMultiStringWithSeparator(rawMessage) };
        }

        private async Task SaveUploadedFilesAsync(string folderPath)
        {
            var files = Request.Form.Files;
            if (files == null || files.Count == 0)
                throw new InvalidOperationException("No files uploaded for duplication data.");
            var allowedExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".csv" };
            foreach (var file in files)
            {
                _logger.Info("Receive file for import duplication data: {0}", file.FileName);
                var extension = Path.GetExtension(file.FileName);
                if (!allowedExtensions.Contains(extension))
                {
                    throw new InvalidOperationException($"Unsupported file extension: {extension}");
                }
                var tempFilePath = SecurityUtils.SafeCombinePath(folderPath, file.FileName);
                await using var stream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await file.CopyToAsync(stream);
            }
        }
    }
}
