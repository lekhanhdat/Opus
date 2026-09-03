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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using Media.Service.ArchiverBackup.Statistics;
using Newtonsoft.Json;
using RAGoogle.Archive;
using RAGoogle.Archive.Common;
using RAGoogle.Archive.Wrapper;
using RAGoogle.Common;
using RAGoogle.Extension;
using RAGoogle.Models;
using RAGoogle.Models.GoogleObjectModel;
using RAGoogle.RecordsDisposal.Action.Archive.Data;
using RAGoogle.Util;

namespace RAGoogle.RecordsDisposal.Action.Archive
{
    internal class ArchiveController(GoogleConfiguration configuration) : BaseBackupController(configuration)
    {
        #region properties

        private IRALogger _logger = RALogger.GetInstance(typeof(ArchiveController));
        private readonly string anyonePermissionId = "anyoneWithLink";
        private readonly string anyonePermissionName = "Anyone with the link";
        private BackupInfoSender aveSender { get; set; }
        private CacheNode cacheNode { get; set; }
        private string nodeType { get; set; }
        //private GoogleDriveHelper _googleDriveHelper = new GoogleDriveHelper(configuration.AppProfile);
        private AveGDrive aveGDrive { get; set; }
        private AveGDFolder aveGDFolder { get; set; }
        private string _tenantId
        {
            get
            {
                if (configuration?.AppProfile != null)
                    return configuration.AppProfile.TenantId;
                return _tenantId;
            }
            set
            {
                _tenantId = value;
            }
        }
        #endregion
        public override async Task ProcessArchiveReport(ArchiveApproveReport item, BackupNodeParameters nodeParameters)
        {
            mArchiveItem = item;
            aveSender = nodeParameters.Sender;
            cacheNode = nodeParameters.CacheNode;
            if (item.JsonMeta.IsNotNullOrEmpty())
            {
                mGoogleItem = JsonConvert.DeserializeObject<GoogleItemData>(item.JsonMeta) ?? new();
            }
            try
            {
                var nodeLevel = (NodeLevel)item.SPNodeLevel;
                switch (nodeLevel)
                {
                    case NodeLevel.GoogleMyDrive:
                        await BackupDriveAsync(nodeParameters);
                        break;
                    case NodeLevel.GoogleSharedDrive:
                        await BackupDriveAsync(nodeParameters, true);
                        break;
                    case NodeLevel.GoogleFolder:
                        await BackupFolderAsync(mGoogleItem, nodeParameters);
                        break;
                    case NodeLevel.GoogleFile:
                        await BackupFileAsync(mGoogleItem, nodeParameters);
                        break;
                    default:
                        throw new NotSupportedException($"Unsupported item level: {item.Level}");
                };
            }
            catch (JobStopException)
            {
                _logger.Warn("the job has stopped.");
                mConfiguration.ReportCenter.JobHasStopped = true;
                throw new JobStopException("the job has stopped");
            }
            catch (Exception ex)
            {
                string message = I18NResource.DeleteItemFailed;
                if (ex.Message.Contains(I18NResource.InvalidUserPermission))
                {
                    message = I18NResource.InvalidUserPermission;
                }
                if(mGoogleItem != null)
                {
                    _logger.Error($"An error occurred while deleting item [{mGoogleItem?.Id}]. Error: {ex}");
                    mConfiguration.ReportCenter?.RecordFailedCommon(mGoogleItem.GenerateDisposalActionJobDetail(I18NResource.RemoveAndDestroyAction, mConfiguration.CurrentRule.Name, message), (int)item.Level);
                }
            }
        }
        public override async Task Process(GoogleItemData item)
        {
            try
            {
            }
            catch (JobStopException)
            {
                _logger.Warn("the job has stopped.");
                throw new JobStopException("the job has stopped");
            }
            catch (Exception ex)
            {
                string message = I18NResource.DeleteItemFailed;
                if (ex.Message.Contains(I18NResource.InvalidUserPermission))
                {
                    message = I18NResource.InvalidUserPermission;
                }
                _logger.Error($"An error occurred while deleting item [{item.Id}]. Error: {ex}");
                mConfiguration.ReportCenter?.RecordFailedCommon(item.GenerateDisposalActionJobDetail(I18NResource.RemoveAndDestroyAction, mConfiguration.CurrentRule.Name, message), (int)item.Level);
            }
        }

        private async Task BackupFileAsync(GoogleItemData item, BackupNodeParameters nodeParameters)
        {
            var status = JobDetailsStatus.Successful;
            var cacheFilePaths = new List<string>();
            try
            {
                using (PerformanceScope pc = new PerformanceScope("ArchiveController.BackupFileAsync"))
                {
                    _logger.Info($"Start file backup {item.Id}.");
                    if (GoogleConstant.UnsupportedRestoreMimeType.Contains(item.MimeType))
                    {
                        cacheNode.DoDelete = false;
                        cacheNode.BackupStatus = FileHeaderStatus.Failed;
                        _logger.Info($"Don't support backup {item.Id}, type:{item.MimeType}.");
                        mArchiveItem.AddToReportsByArchiveApproveReport(mConfiguration.ActionApproveReports, ActionTab.Backup, JobDetailsStatus.Failed, mArchiveItem.DocumentSize /*aveSender.BackupStream.StreamTransfered*/, mConfiguration.CurrentRule?.Name, I18NResource.UnsupportFile);
                        return;
                    }
                    var aveGDFile = new AveGDFile(mConfiguration.AppProfile, mGoogleDriveInfo, GoogleActionType.Backup);
                    aveGDFile.ItemData = item;
                    var fileProxy = await aveGDFile.BackupSelf(item);
                    (var fileVersions, cacheFilePaths) = await aveGDFile.FileVersionsDownloadedAsync();
                    
                    
                    var fileName = item.Name;
                    foreach (var versionItem in fileVersions)
                    {
                        SOGDriveArchiverJobInfoStatistics.Instance.AccumulationItemsSize(versionItem.Size ?? 0, versionItem.DriveName);
                        try
                        {
                            if (nodeParameters.ExportBeforeArcInfo is { GoogleExport: not null, GoogleExportPathGenerator: not null })
                            {
                                GoogleItemExport googleItemExport = new GoogleItemExport() { Configuration = mConfiguration };
                                googleItemExport.GoogleExportBeforeArcInfo = nodeParameters.ExportBeforeArcInfo;
                                googleItemExport.VaultExport(versionItem);
                                item.AddToExportSummaryReportsByGoogleItem(mConfiguration.ActionApproveReports, JobDetailsStatus.Successful, mConfiguration.CurrentRule.Name, string.Empty);
                            }
                        }
                        catch(Exception ex)
                        {
                            _logger.Error($"An error occurred while export item [{item.Id}]. Error: {ex}");
                            item.AddToExportSummaryReportsByGoogleItem(mConfiguration.ActionApproveReports, JobDetailsStatus.Failed, mConfiguration.CurrentRule.Name, I18NResource.ExportItemFailed);
                            throw;
                        }
                        if (item.ParentId == "root")
                        {
                            item.ParentId = aveGDrive.DriveProxy.Id;
                        }
                        item.CreatedBy = fileProxy.Owners.IsNotNullOrEmpty() ? fileProxy.Owners.First()?.EmailAddress ?? string.Empty : string.Empty;
                        item.ModifiedBy = fileProxy.LastModifyingUser?.EmailAddress ?? string.Empty;
                        item.MemberEmail = item.CreatedBy.IsNullOrEmpty() ? item.ModifiedBy.IsNullOrEmpty() ? aveGDrive.ServiceAdminUser : item.ModifiedBy : item.CreatedBy;

                        if (!versionItem.IsCurrentVersion)
                        {
                            mArchiveItem.CacheNodeType = (int)GoogleCacheNodeType.ItemVersion;
                            versionItem.FileName = string.Format("{0}:{1}", fileName, versionItem.VersionName);
                            mArchiveItem.FullPath = item.RelativePath.HandleRelativePathWithFileVersion(item.Name, versionItem.FileName);
                            aveSender.BackupGoogleFileVersionHeader(item, versionItem, mConfiguration.CurrentRule.Name, versionItem.VersionName);
                        }
                        else
                        {
                            aveSender.BackupGoogleFileHeader(item, versionItem, mConfiguration.CurrentRule.Name);
                        }

                        aveSender.BackupStream.SetStreamTransfered(0);
                        

                        var fileHeader = aveSender.BackupHeader(item.RelativePath);
                        if (versionItem.IsCurrentVersion)
                        {
                            cacheNode.FileHeader = fileHeader;
                        }

                        var stream = aveSender.BackupStream;

                        stream.BeginWriteMetadata();
                        try
                        {
                            aveGDFile.ExportFileMetaData(stream, versionItem);

                            _logger.Info("Start to export Google File permission info.");
                            aveGDFile.ExportFilePermission(stream);
                        }
                        catch (Exception ex)
                        {
                            status = JobDetailsStatus.Failed;
                            mConfiguration.ReportCenter.SummaryComments = ex.Message;
                            _logger.Error("Backup Google File Metadata Error: {0}", ex.ToString());
                            cacheNode.DoDelete = false;
                            cacheNode.BackupStatus = FileHeaderStatus.Failed;
                            throw;
                        }
                        finally
                        {
                            aveSender.BackupStream.EndWriteMetadata();
                            aveGDFile.ExportContent(stream, versionItem.LocalPath);
                            aveSender.BackupTail(status == JobDetailsStatus.Successful);
                            _logger.Info($"End BackupFileAsync {item.Id}, version {versionItem.VersionName}.");
                            mArchiveItem.AddToReportsByArchiveApproveReport(mConfiguration.ActionApproveReports, ActionTab.Backup, status, versionItem.Size ?? mArchiveItem.DocumentSize, mConfiguration.CurrentRule?.Name, mConfiguration.ReportCenter.SummaryComments);
                        }

                    }
                    cacheNode.DoDelete = true;
                    cacheNode.BackupStatus = FileHeaderStatus.Success;

                }
            }
            catch (JobStopException)
            {
                mConfiguration.ReportCenter.JobHasStopped = true;
                _logger.Warn("The job has stopped.");
                cacheNode.DoDelete = false;
                cacheNode.BackupStatus = FileHeaderStatus.Failed;
                throw new JobStopException("The job has stopped.");
            }
            catch (Exception ex)
            {
                mConfiguration.ReportCenter.SummaryComments = ex.Message;
                cacheNode.DoDelete = false;
                cacheNode.BackupStatus = FileHeaderStatus.Failed;
                mArchiveItem.AddToReportsByArchiveApproveReport(mConfiguration.ActionApproveReports, ActionTab.Backup, JobDetailsStatus.Failed, mArchiveItem.DocumentSize/*aveSender.BackupStream.StreamTransfered*/, mConfiguration.CurrentRule?.Name, mConfiguration.ReportCenter.SummaryComments);
                _logger.Error($"BackupFileAsync error:{ex.Message.ToString()}");
                throw;
            }
            finally
            {
                foreach (var filePath in cacheFilePaths)
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
        private async Task BackupFolderAsync(GoogleItemData item, BackupNodeParameters nodeParameters)
        {
            _logger.Info($"Start to backup Google Folder: {item.Id}");
            bool hasBackupHeader = false;
            JobDetailsStatus status = JobDetailsStatus.Successful;
            try
            {
                if (nodeParameters.ExportBeforeArcInfo is { GoogleExport: not null, GoogleExportPathGenerator: not null })
                {
                    GoogleFolderExport folderExport = new GoogleFolderExport() { Configuration = mConfiguration };
                    folderExport.GoogleExportBeforeArcInfo = nodeParameters.ExportBeforeArcInfo;
                    folderExport.VaultExport(item);
                }
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to handle export action, item id:{item.Id}, exception:{ex}");
            }
            try
            {
                using (PerformanceScope pc = new PerformanceScope("ArchiveController.BackupFolderAsync"))
                {
                    this.aveGDFolder = new AveGDFolder(mConfiguration.AppProfile, mGoogleDriveInfo, mConfiguration.ReportCenter, GoogleActionType.Backup);
                    aveGDFolder.ItemData = item;
                    var folderProxy = await aveGDFolder.BackupSelf(item);
                    if(item.ParentId == "root")
                    {
                        item.ParentId = aveGDrive.DriveProxy.Id;
                    }
                    item.CreatedBy = folderProxy.Owners.IsNotNullOrEmpty() ? folderProxy.Owners.First()?.EmailAddress ?? string.Empty : string.Empty;
                    item.ModifiedBy = folderProxy.LastModifyingUser?.EmailAddress ?? string.Empty;
                    item.MemberEmail = item.CreatedBy ?? item.ModifiedBy ?? aveGDrive.ServiceAdminUser;

                    aveSender.BackupGoogleFolderHeader(item, mConfiguration.CurrentRule.Name, nodeType);
                    cacheNode.FileHeader = aveSender.BackupHeader(item.Path);

                    
                    hasBackupHeader = true;
                    aveSender.BackupStream.BeginWriteMetadata();
                    try
                    {
                        _logger.Info("Start to export folder basic info.");
                        aveGDFolder.ExportFolderBasicInfo(aveSender.BackupStream, item);

                        _logger.Info("Start to export folder permission info.");
                        aveGDFolder.ExportFolderPermissionsInfo(aveSender.BackupStream);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Backup Google Drive Metadata Error: {0}", ex.ToString());
                        status = JobDetailsStatus.Failed;
                        cacheNode.DoDelete = false;
                        throw;
                    }
                    finally
                    {
                        aveSender.BackupStream.EndWriteMetadata();
                        aveSender.BackupStream.FlushMetadata(0);
                        if (hasBackupHeader)
                        {
                            aveSender.BackupTail(status == JobDetailsStatus.Successful);
                        }
                        else
                        {
                            _logger.Warn($"Backup Google Folder:{item.Path} does not backup header so skip BackupTail.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("Backup google folder error: {0}", e.ToString());
                mConfiguration.ReportCenter.SummaryComments = e.Message.ToString();
                status = JobDetailsStatus.Failed;
                cacheNode.DoDelete = false;
                throw;
            }
            finally
            {
                _logger.Info($"Archiver level: {item.Level.ToString()}");
                mArchiveItem.AddToReportsByArchiveApproveReport(mConfiguration.ActionApproveReports, ActionTab.Backup, status, 0, mConfiguration.CurrentRule?.Name, mConfiguration.ReportCenter.SummaryComments);
            }
        }

        private async Task BackupDriveAsync(BackupNodeParameters nodeParameters, bool isShareDrive = false)
        {
            _logger.Info("Start dive backup...");
            var aveSender = nodeParameters.Sender;
            JobDetailsStatus status = JobDetailsStatus.Successful;
            var currentNode = nodeParameters.CacheNode;
            bool hasBackupHeader = false;
            try
            {
                using (PerformanceScope pc = new PerformanceScope("ArchiveController.BackupDriveAsync"))
                {

                    this.aveGDrive = new AveGDrive(mConfiguration.AppProfile, mGoogleDriveInfo, GoogleActionType.Backup);
                    var (drive, memberObjects) = await aveGDrive.BackupDriveAndDriveMember(mConfiguration.SelectedNode);
                    if (isShareDrive)
                    {
                        aveSender.BackupDriveHeader(mArchiveItem, drive);
                    }
                    else
                    {
                        aveSender.BackupMyDriveHeader(mArchiveItem, mConfiguration.SelectedNode.ObjectId, mConfiguration.SelectedNode.DisplayName, drive.Id);
                    }

                    currentNode.FileHeader = aveSender.BackupHeader(nodeParameters.Node.FullPath);
                    hasBackupHeader = true;
                    var stream = aveSender.BackupStream;
                    stream.BeginWriteMetadata();
                    try
                    {
                        _logger.Info("Start to export Google Drive basic info.");
                        aveGDrive.ExportBasicInfo(stream);
                        if (isShareDrive)
                        {
                            _logger.Info("Start to export Google Drive setting info.");
                            aveGDrive.ExportSetting(stream);
                            _logger.Info("Start to export Google Drive members info.");
                            aveGDrive.ExportMembers(stream);
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Backup Google Drive Metadata Error: {0}", ex.ToString());
                        status = JobDetailsStatus.Failed;
                        currentNode.DoDelete = false;
                        throw;
                    }
                    finally
                    {
                        aveSender.BackupStream.EndWriteMetadata();
                        aveSender.BackupStream.FlushMetadata(0);
                        if (hasBackupHeader)
                        {
                            aveSender.BackupTail(status == JobDetailsStatus.Successful);
                        }
                        else
                        {
                            _logger.Warn($"Backup Google Drive:{nodeParameters.Node.FullPath} does not backup header so skip BackupTail.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Backup Google Drive Error: {0}", ex.ToString());
                mConfiguration.ReportCenter.SummaryComments = ex.Message.ToString();
                status = JobDetailsStatus.Failed;
                currentNode.DoDelete = false;
                throw;
            }
            finally
            {
                mArchiveItem.AddToReportsByArchiveApproveReport(mConfiguration.ActionApproveReports, ActionTab.Backup, status, 0, mConfiguration.CurrentRule?.Name, mConfiguration.ReportCenter.SummaryComments);
            }
        }

        private async Task<GDPermissionList> GetItemPermisisonsObject(GoogleItemData item)
        {
            //aveGDrive = new AveGDrive(mConfiguration.AppProfile, mGoogleDriveInfo);
            var service = this.aveGDrive.DriveService;// await GetDriveService(item);
            var permissions = await service.GetPermissionsByIdAsync(item.Id);

            var folderPermissions = permissions.Select(x => new PermissionInfo
            {
                Id = x.Id,
                DisplayName = x.Id == anyonePermissionId ? anyonePermissionName : x.DisplayName,
                AllowFileDiscovery = x.AllowFileDiscovery,
                Type = x.Type,
                Deleted = x.Deleted,
                EmailAddress = x.EmailAddress,
                ExpirationTimeRaw = x.ExpirationTimeRaw,
                ExpirationTime = x.ExpirationTimeDateTimeOffset == null ? 0 : x.ExpirationTimeDateTimeOffset.Value.Ticks,
                PhotoLink = x.PhotoLink,
                Role = x.Role,
                Domain = x.Domain,
            }).ToList();

            return new GDPermissionList
            {
                Permissions = folderPermissions
            };
        }
    }
}
