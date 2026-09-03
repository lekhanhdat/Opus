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
using AvePoint.RA.Common.Aos;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.ArchiverCommon;
using Newtonsoft.Json;
using RAGoogle.Archive.Common;
using RAGoogle.Common;
using RAGoogle.Extension;
using RAGoogle.Models;
using RAGoogle.RecordsDisposal.Action.Archive.Data;
using RAGoogle.Services;
using RAGoogle.Util;

namespace RAGoogle.RecordsDisposal.Action.ExportOnly
{
    internal class ExportOnlyController : BaseBackupController
    {
        #region properties

        private IRALogger _logger = RALogger.GetInstance(typeof(ExportOnlyController));
        private GoogleExportBeforeArcInfo _googleExportBefArcInfo = null;

        #endregion

        public ExportOnlyController(GoogleConfiguration configuration, GoogleExportBeforeArcInfo googleExportBeforeArcInfo) : base(configuration)
        {
            _googleExportBefArcInfo = googleExportBeforeArcInfo;
        }

        public override async Task Process(GoogleItemData item)
        {
            List<string> filePaths = [];
            try
            {
                if (GoogleConstant.NotSupportedMimeType.Contains(item.MimeType))
                {
                    item.AddToExportSummaryReportsByGoogleItem(mConfiguration.ActionApproveReports, JobDetailsStatus.Failed, mConfiguration.CurrentRule.Name, I18NResource.UnsupportFile);
                }
                else if (item.Level == AvePoint.RA.Contract.RMWeb.Tree.Base.RMNodeLevel.GoogleFolder)
                {
                    if (_googleExportBefArcInfo is { GoogleExport: not null, GoogleExportPathGenerator: not null })
                    {
                        GoogleFolderExport folderExport = new GoogleFolderExport() { Configuration = mConfiguration };
                        folderExport.GoogleExportBeforeArcInfo = _googleExportBefArcInfo;
                        folderExport.VaultExport(item);
                    }
                    else
                    {
                        item.AddToExportSummaryReportsByGoogleItem(mConfiguration.ActionApproveReports, JobDetailsStatus.Failed, mConfiguration.CurrentRule.Name, I18NEntity.GetString("StorageOptimization_GoogleNARAExportConfigFileDeserializeException"));
                    }
                }
                else
                {
                    var service = await GetDriveService(item);
                    var downloadManagement = new ExportFileDownloadManagement(mConfiguration, service);
                    var fileVersions = await downloadManagement.DownloadFileWithVersionsAsync(item);
                    filePaths = downloadManagement.GetFilePaths();
                    foreach (var fileVersion in fileVersions)
                    {
                        if (_googleExportBefArcInfo is { GoogleExport: not null, GoogleExportPathGenerator: not null })
                        {
                            GoogleItemExport googleItemExport = new GoogleItemExport() { Configuration = mConfiguration };
                            googleItemExport.GoogleExportBeforeArcInfo = _googleExportBefArcInfo;
                            googleItemExport.VaultExport(fileVersion);
                            UpdateItemVersionDetails(fileVersion, item);
                            item.AddToExportSummaryReportsByGoogleItem(mConfiguration.ActionApproveReports, JobDetailsStatus.Successful, mConfiguration.CurrentRule.Name, string.Empty);    
                        }
                        else
                        {
                            item.AddToExportSummaryReportsByGoogleItem(mConfiguration.ActionApproveReports, JobDetailsStatus.Failed, mConfiguration.CurrentRule.Name, I18NEntity.GetString("StorageOptimization_GoogleNARAExportConfigFileDeserializeException"));
                        }
                    }
                }
            }
            catch (JobStopException)
            {
                _logger.Warn("the job has stopped.");
                throw new JobStopException("the job has stopped");
            }
            catch (Exception ex)
            {
                string message = I18NResource.ExportItemFailed;
                _logger.Error($"An error occurred while export item [{item.Id}]. Error: {ex}");
                item.AddToExportSummaryReportsByGoogleItem(mConfiguration.ActionApproveReports, JobDetailsStatus.Failed, mConfiguration.CurrentRule.Name, message);
            }
            finally
            {
                foreach (var filePath in filePaths)
                {
                    try
                    {
                        File.Delete(filePath);
                        _logger.Info($"Delete temp file {filePath} Successful");
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn("Error in delete temp file :{0},Reason :{1}", filePath, ex.ToString());
                    }
                }
            }
        }

        private async Task<GoogleDriveService> GetDriveService(GoogleItemData item)
        {
            using (GoogleDriveService driveService = new GoogleDriveService(mConfiguration.AppProfile))
                try
                {
                    var googleApp = mConfiguration.AppProfile;
                    if (googleApp.TenantId != item.TenantId)
                    {
                        googleApp = RMAosApiClient.GetGoogleAppProfile(TenantLocalValue.LogonGroupId, item.TenantId, true);
                    }
                    if (!string.IsNullOrEmpty(item.DriveId))
                    {
                        string memberEmail = item.MemberEmail;
                        return new GoogleDriveService(googleApp, memberEmail);
                    }

                    return new GoogleDriveService(googleApp, item.DriveName);
                }
                catch (Exception ex)
                {
                    _logger.Error($"An error occurred while get service[{item.Id}]. Error: {ex}");
                    throw;
                }
        }

        public override async Task ProcessArchiveReport(ArchiveApproveReport item, BackupNodeParameters nodeParameters)
        {
            mArchiveItem = item;
            if (item.JsonMeta.IsNotNullOrEmpty())
            {
                mGoogleItem = JsonConvert.DeserializeObject<GoogleItemData>(item.JsonMeta) ?? null;
            }
            if (mGoogleItem != null)
            {
                await Process(mGoogleItem);
            }
        }

        private void UpdateItemVersionDetails(DownloadedFileInfo fileVersion, GoogleItemData item)
        {
            if (fileVersion.IsCurrentVersion) return;
            item.RelativePath = $"{fileVersion.RelativePath}:{fileVersion.VersionName}";
            item.Size = fileVersion.Size;
            item.Level = RMNodeLevel.ItemVersion;
        }
    }
}
