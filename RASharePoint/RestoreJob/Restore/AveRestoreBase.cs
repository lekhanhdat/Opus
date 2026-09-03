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
using AvePoint.GCommon.Contract.GranularBackup.Object;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.FileTransfer;
using AvePoint.RA.Common.GraphApi.Tenant;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.RestoreJob.Restore;
using AvePoint.RA.SharePoint.RestoreJob.Restore.Content;
using AvePoint.Wrapper.Common;
using Polly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.Item.Restore
{
    public abstract class AveRestoreBase : IDisposable
    {
        public static readonly AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        protected Exception mError;

        public IAveRestoreStream RestoreStream { get; set; }
        public ItemRestoreConfig Config { get; set; }
        public AveItemRestoreContentReader ContentReader { get; set; }
        public IFileReceiver FileReceiver { get; set; }
        public char ReplaceType { get; set; }
        public RestoreTreeNode RestoreTree { get; set; }

        public JobReportImps Report { get; set; }
        public bool IsEnduserRestore { get; set; }
        public bool IsForceDeleteStub { get; set; }
        public bool SetNowAsRestoreFileModifyTime { get; set; }
        public bool ThrowExceptionWhenRestoreItemCTAndFields { get; set; }
        public string OopStubUrl { get; set; }
        public string PossiblyStubType { get; set; }
        public DestinationSPOLocationInfo DestInfo { get; set; }
        public bool IsRestoreToSPO { get; set; }
        public bool IsAdvancedRestore { get; set; }
        public RestoreSettingAndTree RestoreSettingAndTree { get; set; }
        public Exception Error { get { return mError; } set { mError = value; } }

        private RMGraphTenantManager mGraphManager;
        public RMGraphTenantManager GraphManager
        {
            get
            {
                if (mGraphManager == null)
                {
                    mGraphManager = new RMGraphTenantManager(RestoreSettingAndTree.Setting.DestDto.TenantId);
                }
                return mGraphManager;
            }
        }

        protected virtual void AddReport(AveRestoreReportDto reportDto)
        {
            try
            {
                if (IsRestoreToSPO)
                {
                    if (reportDto.Type != AveConstants.TYPE_SITE.ToString()
                        && reportDto.Type != AveConstants.TYPE_LIST.ToString()
                        && reportDto.Type != AveConstants.TYPE_LISTITEM.ToString()
                        && reportDto.Type != AveConstants.TYPE_DOCUMENT.ToString()
                        && reportDto.Type != AveConstants.TYPE_ATTACHMENTS.ToString()
                        && reportDto.Type != AveConstants.TYPE_LISTITEMVERSION.ToString())
                    {
                        return;
                    }

                    if (reportDto.Status != RestoreStatus.Failed 
                        && (reportDto.Type == AveConstants.TYPE_SITE.ToString() || reportDto.Type == AveConstants.TYPE_LIST.ToString()))
                    {
                        reportDto.Size = 0;
                        reportDto.Status = RestoreStatus.Skipped;
                    }
                }
                else if (IsAdvancedRestore)
                {
                    if (reportDto.Type != AveConstants.TYPE_SITE.ToString()
                        && reportDto.Type != AveConstants.TYPE_WEB.ToString()
                        && reportDto.Type != AveConstants.TYPE_LIST.ToString()
                        && reportDto.Type != AveConstants.TYPE_FOLDER.ToString()
                        && reportDto.Type != AveConstants.TYPE_DOCUMENT.ToString()
                        && reportDto.Type != AveConstants.TYPE_LISTITEM.ToString()
                        && reportDto.Type != AveConstants.TYPE_VERSION.ToString())
                    {
                        return;
                    }
                    if (reportDto.Status != RestoreStatus.Failed
                        && (reportDto.Type == AveConstants.TYPE_SITE.ToString() || reportDto.Type == AveConstants.TYPE_WEB.ToString() || reportDto.Type == AveConstants.TYPE_LIST.ToString() || reportDto.Type == AveConstants.TYPE_FOLDER.ToString()))
                    {
                        reportDto.Size = 0;
                        reportDto.Status = RestoreStatus.Skipped;
                    }
                }

                Action writeReport = () => WriteReport(reportDto);
                if (Config?.ContainerReportTracker != null && IsContainerReportType(reportDto.Type))
                {
                    // Keep ownership of a container at the first batch that successfully writes its report.
                    Config.ContainerReportTracker.TryExecuteOnce(reportDto, writeReport);
                }
                else
                {
                    writeReport();
                }
            }
            catch (Exception e)
            {
                log.Warn(@"Looks up a localized string similar to An error occurred while adding restore report. Path: {0}, type: {1} {2}", reportDto.Title, reportDto.Type, e);
            }
        }

        private void WriteReport(AveRestoreReportDto reportDto)
        {
            Report.ReportManager.Increase();
            Report.AddRestoreReport(reportDto.SourcePath, reportDto.Size, (int)reportDto.Status, reportDto.Type, 0, reportDto.Path, reportDto.ErrorMessage, reportDto.ConflictResolution, reportDto.StartTime, Config.IsUsingMigrationImportJob, reportDto.PathMD5, reportDto.DestinationUrl);
        }

        private static bool IsContainerReportType(string reportType)
        {
            return reportType == AveConstants.TYPE_SITE.ToString()
                || reportType == AveConstants.TYPE_WEB.ToString()
                || reportType == AveConstants.TYPE_LIST.ToString()
                || reportType == AveConstants.TYPE_FOLDER.ToString();
        }

        protected virtual void AddVirtualReport(AveRestoreReportDto reportDto)
        {
            try
            {
                Report.ReportManager.Increase();
                this.Report.AddRestoreReport(reportDto.SourcePath, reportDto.Size, (int)reportDto.Status, reportDto.Type, 0, reportDto.Path, reportDto.ErrorMessage, reportDto.ConflictResolution, reportDto.StartTime, Config.IsUsingMigrationImportJob, reportDto.PathMD5);
            }
            catch (Exception e)
            {
                log.Warn(@"Looks up a localized string similar to An error occurred while adding virtual restore report. Path: {0}, type: {1} {2}", reportDto.Title, reportDto.Type, e);
            }
        }

        protected void AddReport(IEnumerable<AveRestoreReportDto> reportDtos)
        {
            foreach (var reportDto in reportDtos)
            {
                AddReport(reportDto);
            }
        }
        public static AveRestoreBase CreateInstance(ItemRestoreConfig config, BackupLevel restoreLevel, ProductVersion productVersion)
        {
            if (config.EnableMigrationImportJob &&
                (!WrapperConfiguration.WrapperConfigurationForBPOS.HasItemLevelNode || WrapperConfiguration.WrapperConfigurationForBPOS.IsSearchAllRestore || !WrapperConfiguration.WrapperConfigurationForBPOS.IsEndUserRestore)
            )
            {
                log.Info("EnableMigrationImportJob is true.");
                return new AveMigrationRestore();
            }
            switch (restoreLevel)
            {
                case BackupLevel.Item:
                    return new AveItemRestore();
                //return new AveItemMultiThreadRestore();
                case BackupLevel.SiteCollection:
                    return new AveSiteRestore(productVersion);
                case BackupLevel.Site:
                    return new AveWebRestore(productVersion);
            }
            return null;
        }

        protected virtual void WaitForItems(bool isEndOfJob)
        {
        }

        protected virtual void IsItemHasDepedenciesList()
        {
        }

        public virtual void PostProcess()
        {
            WaitForItems(true);
        }

        protected void CheckDtoType(char dtoType)
        {
            if (dtoType != AveConstants.TYPE_DOCUMENT
                && dtoType != AveConstants.TYPE_LISTITEM
                && dtoType != AveConstants.TYPE_ATTACHMENTS)
            {
                WaitForItems(false);
                if (WrapperConfiguration.WrapperConfigurationForBPOS.ArchiverRestoreVersionMapping != null
    && WrapperConfiguration.WrapperConfigurationForBPOS.ArchiverRestoreVersionMapping.Count > 0)
                {
                    log.Info($"CheckDtoType clear ArchiverRestoreVersionMapping,Count:{WrapperConfiguration.WrapperConfigurationForBPOS.ArchiverRestoreVersionMapping.Count}.");
                    WrapperConfiguration.WrapperConfigurationForBPOS.ArchiverRestoreVersionMapping.Clear();
                }
            }
        }

        public virtual void ProcessForOpus()
        {
            var notShowNamesType = new char[] 
            { 
                AveConstants.TYPE_DOCUMENT,
                AveConstants.TYPE_LISTITEMVERSION,
                AveConstants.TYPE_LISTITEM,
                AveConstants.TYPE_ATTACHMENTS
            };
            using (AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.Process"))
            {
                try
                {
                    log.Info("Looks up a localized string similar to Begin restoring....");
                    Init();
                    RestoreContentDto dto;  
                    while ((dto = ContentReader.MoveNext()) != null)
                    {
                        var srcUrl = notShowNamesType.Contains(dto.Type) ? dto?.ItemPathMd5 : dto?.SrcUrl;
                        log.Info(@"Looks up a localized string similar to The restore content is received from media. Name: {0}. SrcName: {1}. Type: {2}.", dto.UniqueId, srcUrl, dto?.Type);
                        RestoreStream.Reset();
                        if (!dto.ReplaceType.Equals('\0'))
                        {
                            ReplaceType = dto.ReplaceType;
                        }
                        try
                        {
                            using (new CheckJobStopScope()) { }
                            CheckDtoType(dto.Type);
                            switch (dto.Type)
                            {
                                case AveConstants.TYPE_SITE:
                                    RestoreSite(dto);
                                    break;

                                case AveConstants.TYPE_WEB:
                                    RestoreWeb(dto);
                                    break;

                                case AveConstants.TYPE_PROJECT:
                                    log.Warn("Pwa data is not supported in DocAveOnline.");
                                    //RestoreProject(dto);
                                    break;
                                case AveConstants.TYPE_APP:
                                    RestoreApp(dto);
                                    break;

                                case AveConstants.TYPE_LIST:
                                    if (dto.IsMyProfileList)
                                    {
                                        RestoreMyProfileList(dto);
                                    }
                                    else
                                    {
                                        RestoreList(dto);
                                        IsItemHasDepedenciesList(); //switch thread mode for special list type
                                    }
                                    break;

                                case AveConstants.TYPE_FOLDER:
                                    RestoreFolder(dto);
                                    break;

                                case AveConstants.TYPE_DOCUMENT:
                                case AveConstants.TYPE_LISTITEM:
                                case AveConstants.TYPE_ATTACHMENTS:
                                case AveConstants.TYPE_VERSION:
                                case AveConstants.TYPE_LISTITEMVERSION:
                                    RestoreItem(dto);
                                    break;

                                default:
                                    log.Warn(@"Looks up a localized string similar to Unknown object type: {0}.", dto.Type);
                                    break;
                            }
                        }
                        catch (JobStopException ex)
                        {
                            log.Warn("job is stopped by manual");
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Error(@"Looks up a localized string similar to An error occurred while doing the restore job. Type: {0},exception:{1}", dto.Type, e);
                            mError = e;
                        }
                    }
                }
                catch (JobStopException ex)
                {
                    log.Warn("job is stopped by manual");
                    throw;
                }
                catch (Exception e)
                {
                    log.Error(@"Looks up a localized string similar to An error occurred while receiving backup data from media.{0}", e);
                    Report.HasErrorNode = true;
                    if (e.Message.Contains("Cannot find the index with the path"))
                    {
                        Report.summaryComments = "RM_JM_RestoreFaild_IndexNotExsit_ErrorMessage";
                    }
                    else if (e.Message.Contains("This site has the maximum number of lists and libraries"))
                    {
                        Report.summaryComments = "RM_JM_RestoreFaild_OutOfListCountLimit_ErrorMessage";
                    }
                    mError = e;
                }
                finally
                {
                    PostProcess();
                }
            }
        }
        public void Process()
        {
            if (IsEnduserRestore && !string.IsNullOrEmpty(OopStubUrl))
            {
                ProcessForEndUser();
            }
            else
            {
                ProcessForOpus();
            }
        }
        public void ProcessForEndUser()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.Process"))
            {
                try
                {
                    log.Info("Looks up a localized string similar to Begin restoring....");
                    Init();
                    RestoreContentDto dto;
                    while ((dto = ContentReader.MoveNext()) != null)
                    {
                        log.Info(@"Looks up a localized string similar to The restore content is received from media. Name: {0}. SrcName: {1}. Type: {2}.", dto.UniqueId, dto?.SrcUrl, dto?.Type);
                        RestoreStream.Reset();
                        if (!dto.ReplaceType.Equals('\0'))
                        {
                            ReplaceType = dto.ReplaceType;
                        }
                        try
                        {
                            using (new CheckJobStopScope()) { }
                            CheckDtoType(dto.Type);
                            switch (dto.Type)
                            {
                                case AveConstants.TYPE_SITE:
                                    RestoreSite(new RestoreContentDto() { Type = 'E',SrcUrl = dto.SrcUrl });
                                    RestoreWeb(new RestoreContentDto() { Type = 'W' });
                                    break;
                                case AveConstants.TYPE_LIST:
                                    RestoreList(new RestoreContentDto() { Type = 'L' });
                                    RestoreFolder(new RestoreContentDto() { Type = 'F' });
                                    break;
                                case AveConstants.TYPE_DOCUMENT:
                                case AveConstants.TYPE_LISTITEM:
                                case AveConstants.TYPE_ATTACHMENTS:
                                case AveConstants.TYPE_VERSION:
                                case AveConstants.TYPE_LISTITEMVERSION:
                                    dto.SiteUrl = WebUtility.UrlDecode(OopStubUrl);
                                    dto.OopSourceUrl = dto.SrcUrl;
                                    dto.SrcUrl = OopStubUrl;
                                    RestoreItem(dto);
                                    break;

                                default:
                                    log.Warn(@"Looks up a localized string similar to Unknown object type: {0}.", dto.Type);
                                    break;
                            }
                        }
                        catch (JobStopException ex)
                        {
                            log.Warn("job is stopped by manual");
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Error(@"Looks up a localized string similar to An error occurred while doing the restore job. Type: {0},exception:{1}", dto.Type, e);
                            mError = e;
                        }
                    }
                }
                catch (JobStopException ex)
                {
                    log.Warn("job is stopped by manual");
                    throw;
                }
                catch (Exception e)
                {
                    log.Error(@"Looks up a localized string similar to An error occurred while receiving backup data from media.{0}", e);
                    mError = e;
                }
                finally
                {
                    PostProcess();
                }
            }
        }

        public async Task ProcessForM365ArchiveAsync()
        {
            using AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.Process");
            try
            {
                log.Info("Looks up a localized string similar to Begin restoring....");
                Init();
                var treeRoot = RestoreSettingAndTree?.Tree[0];
                if (treeRoot is not null)
                {
                    await ProcessTreeNodeAsync(treeRoot);
                }
            }
            catch (JobStopException)
            {
                log.Warn("job is stopped by manual");
                throw;
            }
            catch (Exception e)
            {
                log.Error(@"Looks up a localized string similar to An error occurred while receiving backup data from media.{0}", e);
                mError = e;
            }
            finally
            {
                log.Info("Looks up a localized string similar to End restoring....");
            }
        }

        private async Task ProcessTreeNodeAsync(SPTreeNodeDto treeNode, string rootUrl = "")
        {
            log.Info(@"Looks up a localized string similar to The restore content is received from media. Name: {0}. SrcName: {1}. Type: {2}.", treeNode.ID, treeNode.FullPath, treeNode.Level);
            try
            {
                using (new CheckJobStopScope()) { }
                var sourcePath = rootUrl + treeNode.FullPath;
                switch (treeNode.Level)
                {
                    case GCommon.Contract.Tree.Object.NodeLevel.SiteCollection:
                        var uri = new Uri(treeNode.FullPath);
                        rootUrl = uri.GetLeftPart(UriPartial.Authority);
                        AddReport(new AveRestoreReportDto
                        {
                            Status = RestoreStatus.Skipped,
                            Type = AveConstants.TYPE_SITE.ToString(),
                            Size = 0,
                            SourcePath = sourcePath,
                            Path = sourcePath,
                        });
                        break;

                    case GCommon.Contract.Tree.Object.NodeLevel.Site:
                        AddReport(new AveRestoreReportDto
                        {
                            Status = RestoreStatus.Skipped,
                            Type = AveConstants.TYPE_WEB.ToString(),
                            Size = 0,
                            SourcePath = sourcePath,
                            Path = sourcePath,
                        });
                        break;

                    case GCommon.Contract.Tree.Object.NodeLevel.List:
                        AddReport(new AveRestoreReportDto
                        {
                            Status = RestoreStatus.Skipped,
                            Type = AveConstants.TYPE_LIST.ToString(),
                            Size = 0,
                            SourcePath = sourcePath,
                            Path = sourcePath,
                        });
                        break;

                    case GCommon.Contract.Tree.Object.NodeLevel.Folder:
                        AddReport(new AveRestoreReportDto
                        {
                            Status = RestoreStatus.Skipped,
                            Type = AveConstants.TYPE_FOLDER.ToString(),
                            Size = 0,
                            SourcePath = sourcePath,
                            Path = sourcePath,
                        });
                        break;

                    case GCommon.Contract.Tree.Object.NodeLevel.Document:
                    case GCommon.Contract.Tree.Object.NodeLevel.Item:
                    case GCommon.Contract.Tree.Object.NodeLevel.ItemVersion:
                        await ProcessItemAsync(treeNode, rootUrl);
                        break;

                    default:
                        log.Warn(@"Looks up a localized string similar to Unknown object type: {0}.", treeNode.Level);
                        break;
                }
            }
            catch (JobStopException)
            {
                log.Warn("job is stopped by manual");
                throw;
            }
            catch (Exception ex)
            {
                log.Error(@"Looks up a localized string similar to An error occurred while doing the restore job. Type: {0},exception:{1}", treeNode.Level, ex);
                mError = ex;
            }
            if (treeNode.Children is not null)
            {
                foreach (var child in treeNode.Children)
                {
                    await ProcessTreeNodeAsync(child, rootUrl);
                }
            }
        }

        private async Task ProcessItemAsync(SPTreeNodeDto treeNode, string rootUrl)
        {
            var itemType = treeNode.Level switch
            {
                GCommon.Contract.Tree.Object.NodeLevel.Document => AveConstants.TYPE_DOCUMENT.ToString(),
                GCommon.Contract.Tree.Object.NodeLevel.Item => AveConstants.TYPE_LISTITEM.ToString(),
                GCommon.Contract.Tree.Object.NodeLevel.ItemVersion => AveConstants.TYPE_VERSION.ToString(),
                _ => AveConstants.TYPE_DOCUMENT.ToString(),
            };
            var sourcePath = rootUrl + treeNode.FullPath;
            try
            {
                var extensions = treeNode.Extension.Split('|');
                var siteId = extensions[0];
                var listId = extensions[1];
                var rowId = int.Parse(extensions[2]);
                await SetToUnArchiveStatusAsync(siteId, listId, rowId);
                AddReport(new AveRestoreReportDto
                {
                    Status = RestoreStatus.Success,
                    Type = itemType,
                    Size = treeNode.Size,
                    SourcePath = sourcePath,
                    Path = sourcePath,
                });
            }
            catch (Exception ex)
            {
                log.Error(@"Looks up a localized string similar to An error occurred while setting the item to unarchive status. Type: {0},exception:{1}", treeNode.Level, ex);
                mError = ex;
                AddReport(new AveRestoreReportDto
                {
                    Status = RestoreStatus.Failed,
                    Type = itemType,
                    Size = treeNode.Size,
                    SourcePath = sourcePath,
                    Path = sourcePath,
                });
            }
        }

        private async Task SetToUnArchiveStatusAsync(string siteId, string listId, int rowId)
        {
            await Policy
                .Handle<HttpRequestException>(ex => ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(3, retryTimes => TimeSpan.FromSeconds(retryTimes * 5))
                .ExecuteAsync(async () => await GraphManager.SetItemToUnarchiveStatusAsync(siteId, listId, rowId));
        }

        #region Need OverRide in sub Class

        public virtual void Init()
        {
            RestoreStream = new WrapperRestoreStreamV2(new FileReceiverWrapper(FileReceiver));
            ContentReader = new AveItemRestoreContentReader(RestoreStream, Config, RestoreTree);
        }

        public virtual void RestoreSite(RestoreContentDto aveSiteDto)
        {
        }

        public virtual void RestoreWeb(RestoreContentDto aveWebDto)
        {
        }

        public virtual void RestoreMyProfileList(RestoreContentDto aveListDto)
        {
        }

        public virtual void RestoreList(RestoreContentDto aveListDto)
        {
        }

        public virtual void RestoreFolder(RestoreContentDto aveFolderDto)
        {
        }

        public virtual void RestoreItem(RestoreContentDto aveItemDto)
        {
        }

        public virtual void RestoreApp(RestoreContentDto aveAppDto)
        {
        }

        public virtual void RestoreProject(RestoreContentDto projectDto)
        {
        }

        #endregion


        public virtual void Dispose()
        {
            log.Info("Looks up a localized string similar to The Restore Base has disposed..");
            try
            {
                //AveItemRestorePauseResume.SendCloseResponse();
                FileReceiver.Close(this.mError == null ? string.Empty : this.mError.Message);
            }
            catch (Exception e)
            {
                log.Error(@"Looks up a localized string similar to An error occurred while closing file receiver.{0}", e.ToString());
                AveRestoreReportDto reportDto = new AveRestoreReportDto();
                reportDto.Status = RestoreStatus.Failed;
                //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(e,RestoreReportKey.Item_DisposeError.ToString(), RestoreReportResource.Item_DisposeError, e.Message);
                AddReport(reportDto);
            }
            RestoreResultInfo resultInfo = null;
            if (this.mError != null)
            {
                //这里如果给一个默认的key，如果出现的异常没有key就能显示默认key的国际化内容
                string errorMessage = AveWrapperHandleErrorMessage.GetFormateErrorMessage(this.mError, string.Empty, this.mError.Message);
                //if (this.mError is PauseProcessException)
                //{
                //    resultInfo = new RestoreResultInfo() { JobStatus = JobStatus.Stopped };
                //    Report.Finish(resultInfo, errorMessage);
                //    return;
                //}
                if (!this.mError.Message.StartsWith("Error because of insufficient", StringComparison.OrdinalIgnoreCase))
                {
                    if (this.mError is AvePoint.GCommon.Network.BlockQueueSyncException ||
                        this.mError is AvePoint.GCommon.Network.ClosedWithErrorException ||
                        this.mError is AvePoint.GCommon.Network.NetworkBrokenException)
                    {
                        resultInfo = new RestoreResultInfo() { JobStatus = JobStatus.Failed };
                        //resultInfo.AddARestoreError(RestoreReportResource.Item_MediaError, RestoreReportKey.Item_MediaError.ToString(), new string[] { });
                    }
                }
                if (resultInfo == null)
                {
                    // resultInfo = new RestoreResultInfo(JobStatus.Failed, errorMessage);
                    resultInfo = new RestoreResultInfo() { JobStatus = JobStatus.Failed };
                    resultInfo.AddARestoreError(this.mError.Message, null, new string[] { });
                }
                //Report.Finish(resultInfo, errorMessage);
            }
            else
            {
                // Report.Finish(JobStatus.Finished, null);
                resultInfo = new RestoreResultInfo() { JobStatus = JobStatus.Finished };
                //Report.Finish(resultInfo,string.Empty);
            }
        }
    }
}