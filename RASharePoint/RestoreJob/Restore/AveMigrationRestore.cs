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
using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Item.Common;
using AvePoint.Item.Restore;
using AvePoint.ObjectModel.Common.Cache;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Common.Utility;
using AvePoint.Wrapper.Restore;
using HSMAzureCommon;
using HSMCommon;
using HSMCommon.DeploymentXML;
using LS.SPWorkflowProcessor;
using Media.Service.ArchiverBackup.Restore;
using Microsoft365.SharePoint;
using RAArchiverCommon;
using RAGoogle.Archive.Wrapper;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace AvePoint.RA.SharePoint.RestoreJob.Restore
{
    public class DictionaryEntryComparer : IEqualityComparer<DictionaryEntry>
    {
        public bool Equals(DictionaryEntry x, DictionaryEntry y)
        {
            return string.Equals(x.Name, y.Name, StringComparison.OrdinalIgnoreCase);
        }
        public int GetHashCode(DictionaryEntry obj)
        {
            return obj.Name.ToUpper().GetHashCode();
        }
    }
    public class SPMHSConstant
    {
        public const string MANIFEST_XML_NAME = "Manifest.xml";
        public const string EXPORTSETTINGS_XML_NAME = "ExportSettings.xml";
        public const string LOOKUPLISTSMAP_XML_NAME = "LookupListMap.xml";
        public const string REQUIREMENTS_XML_NAME = "Requirements.xml";
        public const string ROOTOBJECTMAP_XML_NAME = "RootObjectMap.xml";
        public const string SYSTEMDATA_XML_NAME = "SystemData.xml";
        public const string USETGROUP_XML_NAME = "UserGroup.xml";
        public const string VIEWFORMSLIST_XML_NAME = "ViewFormsList.xml";

        static AveLogger logger = AveLogger.GetInstance(typeof(SPMHSConstant));
        public static Int32 FileValue = 0;
        public static Int32 PackageCountCapacity = 250;

        //Temp use Int Type
        public static Int32 PackageSizeCapacity = 250 * 1024;

        private static Dictionary<CultureLCID, string> nFileNameCultures =
    new Dictionary<CultureLCID, string>();

        private static string LcidCultureValue(Dictionary<CultureLCID, string> valueCollection, CultureLCID id)
        {
            string tempValue = string.Empty;
            try
            {
                if (valueCollection != null && valueCollection.Count > 0)
                {
                    if (valueCollection.TryGetValue(id, out tempValue))
                    {
                        return tempValue;
                    }
                }
            }
            catch (Exception el)
            {
                logger.Error("An error occurred while getting lcid value,details:{0}.", el.ToString());
                tempValue = string.Empty;
            }
            return tempValue;
        }

        private static string newFileName = null;
        private static CultureLCID lcid = CultureLCID.USA;
        public static string NEWFILENAME
        {
            get
            {
                if (string.IsNullOrEmpty(newFileName))
                {
                    newFileName = LcidCultureValue(nFileNameCultures, lcid);
                    //if (string.IsNullOrEmpty(newFileName))
                    //    newFileName = "New File Name";
                }
                return newFileName;
            }
        }
    }

    public enum CultureLCID
    {
        USA = 1033, GERMANY = 1031, JAPAN = 1041
    }

    public class HSMFolderObjectBasicInfo
    {
        public string Url;
        public Guid Id;
    }

    public class AveMigrationRestore : AveItemRestore
    {
        public static readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly object padlock = new object();

        AveSPFolder mAveListRootFolder => aveListRootFolder;
        AveSPFolder mAveFolder => aveFolder;
        AveSPList mAveList => AveList;
        AveSPWeb ParentAveWeb => AveWeb;
        AveSPSite ParentAveSite => AveSite;

        bool IsFreeContainer = true;
        
        List<string> CurrentPackageIdList { get; set; }

        Dictionary<string, string> mFileValueDic = new Dictionary<string, string>();

        HSMFolderObjectBasicInfo ParentFolderInfo { get; set; }

        protected ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, SPGenericObject>> mCacheSPFolderObjects = new ConcurrentDictionary<Guid, ConcurrentDictionary<Guid, SPGenericObject>>();
        protected Dictionary<Guid, Guid> ParentSPFolderCache { get; set; }
        protected Dictionary<string, string> ListFolderUrlMapping { get; set; }

        private readonly object mJobStatusLock = new object();

        private readonly object mMultiReceiverLock = new object();

        private ConcurrentDictionary<Guid, ImportJobResources> mAllJobStatus = new ConcurrentDictionary<Guid, ImportJobResources>();
        
        protected AveMultiReceiver multiReceiver = null;

        private List<ARRestoreFileInfo> mCurrentPackageList = [];
        private ConcurrentDictionary<string, List<MigrationRestoreVersionDto>> mVersionReports = [];

        ManifestPackageProcessor _manifestProcessor { get; set; }

        #region ---Initialize---

        private void InitTaskManager()
        {
            multiReceiver = new AveMultiReceiver(Config.MigrationImportJobCount);
            multiReceiver.scheduler.AddTask(new AveMutiEmpty(0, true));
        }
        #endregion

        #region ---Deploynt XML---
        #endregion

        #region---Add Import Job---
        /// <summary>
        /// Processes and finalizes a package, then creates import job if ready.
        /// Refactored to delegate package preparation to ManifestPackageProcessor.
        /// </summary>
        /// <param name="isLastPackage">Indicates if this is the last package</param>
        protected void ProcessPackage(bool isLastPackage = false)
        {
            if (mAveList != null && mAveList.SPList != null && _manifestProcessor != null)
            {
                // Delegate package finalization to ManifestPackageProcessor
                var packageStatus = _manifestProcessor.FinalizePackage(ParentAveWeb.SPWeb, isLastPackage);

                if (packageStatus == PackageStatus.Ready || packageStatus == PackageStatus.Error)
                {
                    // Package is ready - create import job
                    AddImportJobTask(
                        ParentAveSite.SPSite,
                        ParentAveWeb.SPWeb.ID,
                        mAveList.SPList.ID,
                        this.mAveList.Url,
                        true,
                        _manifestProcessor.TempContentPath,
                        _manifestProcessor.TempManifestPath,
                        _manifestProcessor.AzureInfo,
                        true,
                        _manifestProcessor.UploadFileHashDic,
                        _manifestProcessor.LastError ?? string.Empty
                    );

                    // reset upload file hash dic for the next package
                    _manifestProcessor.UploadFileHashDic = [];

                    // Reset container info for next package
                    if (!isLastPackage)
                    {
                        _manifestProcessor.ResetContainerInfo();
                        mLog.Info("Reset FreeContainer parameters. New container will be provisioned on next file upload.");
                    }
                }
                else if (packageStatus == PackageStatus.Empty)
                {
                    // Empty package - trigger empty list post action
                    EmptyListPostAction(mAveList.SPList.ID);
                }
                // If NotReady, no action needed - continue processing
            }
        }

        /// <summary>
        /// Legacy method maintained for backward compatibility.
        /// Delegates to ProcessPackage which uses the refactored architecture.
        /// </summary>
        [Obsolete("Use ProcessPackage instead. This method is maintained for backward compatibility.")]
        protected void SplitPackage(bool isLastPackage = false)
        {
            ProcessPackage(isLastPackage);
        }

        private void AddImportJobTask(IAveSite site, Guid webId, Guid listId, string listUrl, bool isLast, string dataContainerDir, string manifestContainerDir, WinAzure azureInfo, bool isEncryption, Dictionary<string, FileHash> uploadFileHashDic, string message = "")
        {
            mLog.Info("Adding Import Job for {0} with {1} files in HashDic", listUrl, uploadFileHashDic.Count);

            lock (mJobStatusLock)
            {
                mAllJobStatus[listId].JobCount++;
                mAllJobStatus[listId].AddJobsFinished = isLast;
            }
            WinAzure temAzure = new WinAzure();
            temAzure.AzureContainerManifestUri = azureInfo.AzureContainerManifestUri;
            temAzure.AzureContainerSourceUri = azureInfo.AzureContainerSourceUri;
            temAzure.AzureManifestContainerName = azureInfo.AzureManifestContainerName;
            temAzure.AzureQueueReportContainerName = azureInfo.AzureQueueReportContainerName;
            temAzure.AzureQueueReportUri = azureInfo.AzureQueueReportUri;
            temAzure.AzureSourceContainerName = azureInfo.AzureSourceContainerName;
            //temAzure.AzureConnectionString = azureInfo.AzureConnectionString;

            MutliImportParameter importParameter = new MutliImportParameter()
            {
                AzureInfo = temAzure,
                Site = site,
                WebId = webId,
                ListId = listId,
                ManifestContainerDir = manifestContainerDir,
                DataContainerDir = dataContainerDir,
                MigrationModuleType = MigrationModuleType.SPMigration,
                IsEncryption = isEncryption,
                IsNeedCheckSourceFilesUploaded = false,
                RetryMigrationJobTime = Config.MigrationImportJobTimeOutMinutes,
                CurrentRestoreFileIdsList = mCurrentPackageList,
                UploadFileHashDic = uploadFileHashDic
                //ExtraObjects = new Dictionary<string, object>() { { AveSPPropertyKey.SHAREDLINKS, new Dictionary<string, List<PostCacheObjectShareLink>>(CurrentPackageSharedLinks) } },
            };

            if (isOriginalSiteExist)
            {
                importParameter.IsOriginalSiteExist = true;
                importParameter.OriSite = oriAveSite.SPSite;
                importParameter.OriWebId = oriAveWeb.SPWeb.ID;
            }

            //importParameter.Report.ListUrl = listUrl;
            //importParameter.Report.ThreadIdentity = listUrl.Split('/').Last();
            //importParameter.Report.PackageName = manifestContainerDir;
            //importParameter.Report.Location = PathExtension.Combine(mConfig.JobDir, "PrimeReport.ave");
            //importParameter.GenerateMD5ForLargeFile = GlobalPreferenceSettings.GenerateMD5ForLargeFile;
            //importParameter.UploadTimeoutForLargeFile = GlobalPreferenceSettings.AzureFileUploadTimeoutHoursForLargeFile;
            //importParameter.UploadTimeout = GlobalPreferenceSettings.AzureFileUploadTimeoutHours;
            //importParameter.UploadRetryCount = GlobalPreferenceSettings.AzureFileUploadRetryCount;
            try
            {
                if (!string.IsNullOrEmpty(message))
                {
                    mAllJobStatus[listId].JobCount--;
                    SendErrorJobReport(message, importParameter);
                    return;
                }
                if (IsFreeContainer)
                {
                    // Use the already-provisioned FreeContainer parameters from ManifestPackageProcessor
                    // DO NOT create new container here - it would reset the URI and cause upload failures
                    importParameter.FCParameters = _manifestProcessor.GetFreeContainerParameters();
                    importParameter.IsFreeContainer = true;
                }

                AzureMultipleImport import = new AzureMultipleImport(importParameter, 1);

                PostActionDelegate postAction = new PostActionDelegate(ImportPostAction);
                import.updateErrorReportsEvent += new UpdateErrorReportsDelegate(this.UpdateErrorReports);
                import.sendJobReportEvent += new SendJobReportDelegate(this.SendJobReports);
                import.sendErrorJobReportEvent += new SendErrorJobReportDelegate(this.SendErrorJobReport);
                import.PostActionEvent = postAction;
                mLog.Info("The package ListUrl is {0}. Name is {1}.", listUrl, importParameter.AzureInfo.AzureManifestContainerName);
                lock (mMultiReceiverLock)
                {
                    multiReceiver.scheduler.AddTask(import);
                }
            }
            catch (Exception e)
            {
                mLog.Error($"Error occured while AddImportJobTask for list: {listUrl}, containerName: {importParameter?.AzureInfo?.AzureManifestContainerName}. Ex: {e}");

                mAllJobStatus[listId].JobCount--;
                SendErrorJobReport(e.Message, importParameter);
            }
            finally
            {
                mCurrentPackageList = [];
                CurrentPackageIdList?.Clear();
                //CurrentPackageSharedLinks?.Clear();
            }
        }
        public void ImportPostAction(MutliImportParameter multiImportParameter)
        {

        }
        private void UpdateErrorReports(Dictionary<string, AzureQueueMessage> ErrorItems, MutliImportParameter multiImportParameter)
        {
            using (new AvePerformanceScope("Event:UpdateErrorReports"))
            {
                if (ErrorItems.Count > 0)
                {
                    mLog.Error("ErrorItems Count:{0}", ErrorItems.Count);
                }
                using (IAveWeb mWeb = multiImportParameter.Site.OpenWeb(multiImportParameter.WebId))
                {
                    IAveList mList = mWeb.GetList(multiImportParameter.ListId);
                    foreach (var m in ErrorItems)
                    {
                        mLog.Error("Error File Url:{0}", m.Value.Url);
                        mLog.Error("Error Code:{0}", m.Value.ErrorCode);
                        mLog.Error("Message: {0}", m.Value.Message);
                        var reportMess = ArchiverCommonStaticMethod.GetMessageFromCallStack(m.Value.Message);
                        var reportStatus = JobDetailsStatus.Failed;
                        if (m.Value.ErrorCode == SPErrorCode.TP_E_DOCALREADYEXISTS.ToString()) // cannot get modified and editor from SPO error queueMessage
                        {
                            reportMess = "RM_RS_SkippedItemByIsSameItem";
                            reportStatus = JobDetailsStatus.Skipped;
                        }
                        else if (m.Value.ErrorCode == SPErrorCode.TP_E_INVALIDFILENAME.ToString()) // 429 too many request also can meet this error
                        {
                            reportMess = string.Format(I18NEntity.GetString("RM_JM_JD_ConvertStub_Comment_StubPathInvalidOrBusy"), m.Value.Url);
                        }
                        if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(m.Key))
                        {
                            mAllJobStatus[multiImportParameter.ListId].SetReportStatusAndMessage(m.Key, reportStatus, reportMess);
                        }
                        else
                        {
                            mAllJobStatus[multiImportParameter.ListId].SetReportStatusAndMessageByUrl(m.Value.Url, reportStatus, reportMess);
                            IAveFile restoreFile = mWeb.GetFile(m.Value.Url);
                            if (!restoreFile.Exists)
                            {
                                mLog.Warn("Restore file does not exist when UpdateErrorReports.FileUrl: {0}.", m.Value.Url);
                            }
                        }
                    }
                }
            }
        }

        private void SendJobReports(MutliImportParameter multiImportParameter, bool isImportJobCanceled)
        {
            //isImportJobCanceled = true;
            var errorItemIds = new List<string>();
            if (isImportJobCanceled)
            {
                mLog.Warn("Import Job Canceled");
                SendJobReportsForCanceledJob(multiImportParameter, ref errorItemIds);
            }
            else
            {
                List<ARMigrationRestoreFileInfo> mRRestoreFileInfos = new List<ARMigrationRestoreFileInfo>();
                using (new AvePerformanceScope("Event:SendJobReports"))
                {
                    using (IAveWeb mWeb = multiImportParameter.Site.OpenWeb(multiImportParameter.WebId))
                    {
                        IAveList mList = mWeb.GetList(multiImportParameter.ListId);
                        foreach (var info in multiImportParameter.CurrentRestoreFileIdsList)
                        {
                            if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(info.id) && mAllJobStatus[multiImportParameter.ListId].GetReport(info.id).Status == JobDetailsStatus.Failed)
                            {
                                Report.HasErrorNode = true;
                                errorItemIds.Add(info.id);
                                continue;
                            }
                            if (info is not ARMigrationRestoreFileInfo)
                            {
                                mLog.Warn($"the info {info.id}, {info.serverRelativeUrl} is not a valid migration restore file info.");
                                errorItemIds.Add(info.id);
                                continue;
                            }

                            mRRestoreFileInfos.Add((ARMigrationRestoreFileInfo)info);
                            if (mRRestoreFileInfos.Count >= 20)
                            {
                                BulkDeclareAndDelete(mWeb, mList, mRRestoreFileInfos, multiImportParameter, ref errorItemIds);
                                mRRestoreFileInfos.Clear();
                            }
                        }
                        if (mRRestoreFileInfos.Count != 0)
                        {
                            BulkDeclareAndDelete(mWeb, mList, mRRestoreFileInfos, multiImportParameter, ref errorItemIds);
                            mRRestoreFileInfos.Clear();
                        }

                        #region send report
                        mLog.Info($"Start internal send report in SendJobReports");
                        InternalSendReports(multiImportParameter, mWeb, mList);
                        mLog.Info($"End internal send report in SendJobReports");
                        #endregion
                    }

                }
            }

            if (multiImportParameter.IsOriginalSiteExist)
            {
                using (new AvePerformanceScope("Event:DeleteOriginalStubs"))
                {
                    BulkDeleteOriStub(multiImportParameter, errorItemIds);
                }
            }
        }

        private void BulkDeleteOriStub(MutliImportParameter multiImportParameter, List<string> errorItemIds)
        {
            using IAveWeb mOriWeb = multiImportParameter.OriSite.OpenWeb(multiImportParameter.OriWebId);
            // group multiImportParameter.CurrentRestoreFileIdsList by OriParentListId
            // foreach group, open List and deleteBatch Items by list OriStubRowId
            log.Info($"Begin BulkDelete Original Stubs. Total lists count: {multiImportParameter.CurrentRestoreFileIdsList.Count}, error item count: {errorItemIds.Count}");
            var groupedByList = multiImportParameter.CurrentRestoreFileIdsList
                .Where(info => !errorItemIds.Contains(info.id) && info is ARMigrationRestoreFileInfo aInfo && aInfo.NeedDeleteOriStub)
                .Cast<ARMigrationRestoreFileInfo>()
                .GroupBy(info => info.OriParentListId);
            // should be only one here...
            foreach (var listGroup in groupedByList)
            {
                Guid oriListId = listGroup.Key;
                if (oriListId == Guid.Empty) continue;

                try
                {
                    IAveList oriList = mOriWeb.GetList(oriListId);
                    var originalFiles = listGroup.ToHashSet();
                    using (new AvePerformanceScope("SP2013ArchiveBackUp.CleanupOriginalStub.BulkDelete"))
                    {
                        mLog.Info("Begin BulkDelete Original Stubs. ListId: {0}, ItemsCount: {1}.", oriListId, originalFiles.Count);
                        try
                        {
                            List<int> idsToDelete = originalFiles
                                .Select(x => x.OriStubRowId).ToList();

                            if (idsToDelete.Count > 0)
                            {
                                oriList.DeleteItemsByRowIds(idsToDelete);
                                mLog.Info("End BulkDelete Original Stubs successfully.");
                                foreach (var fileInfo in originalFiles)
                                {
                                    LinkFileCommon.DeleteStubFileRecord(multiImportParameter.OriSite.ID.ToString(), fileInfo.AveDocIdOriginal);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            mLog.Warn("Failed BulkDelete Original Stubs, falling back to One-by-one. Message: {0}", ex.Message);
                            foreach (var fileInfo in originalFiles)
                            {
                                using (new AvePerformanceScope("SP2013ArchiveBackUp.CleanupOriginalStub.OneByOne"))
                                {
                                    try
                                    {
                                        IAveFile oriFile = mOriWeb.GetFile(fileInfo.OriStubPath);
                                        if (oriFile != null && oriFile.Exists)
                                        {
                                            DeleteStubFile(oriFile);
                                            mLog.Info("Successfully deleted original stub one-by-one: {0}", fileInfo.OriStubPath.LogBase64());
                                            LinkFileCommon.DeleteStubFileRecord(multiImportParameter.OriSite.ID.ToString(), fileInfo.AveDocIdOriginal);
                                        }
                                        else
                                        {
                                            // the clean stub file tracking records in destroy stub job, not in this job
                                            mLog.Warn("Original file does not exist or already deleted: {0}", fileInfo.OriStubPath.LogBase64());
                                        }
                                    }
                                    catch (Exception oneEx)
                                    {
                                        mLog.Error("Error deleting original stub one-by-one. Path: {0}, Ex: {1}", fileInfo.OriStubPath, oneEx.Message);
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception listEx)
                {
                    mLog.Error("An error occurred while accessing Original List {0}. Exception: {1}", oriListId, listEx.Message);
                }
            }
        }

        private void SendJobReportsForCanceledJob(MutliImportParameter multiImportParameter, ref List<string> errorItemIds)
        {
            using (new AvePerformanceScope("Event:SendJobReportsForCanceledJob"))
            {
                using (IAveWeb mWeb = multiImportParameter.Site.OpenWeb(multiImportParameter.WebId))
                {
                    foreach (var info in multiImportParameter.CurrentRestoreFileIdsList)
                    {
                        if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(info.id) && mAllJobStatus[multiImportParameter.ListId].GetReport(info.id).Status == JobDetailsStatus.Failed)
                        {
                            Report.HasErrorNode = true;
                            //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, new Guid(info.id), 50000, multiImportParameter.JobId);
                            errorItemIds.Add(info.id);
                            continue;
                        }
                        try
                        {
                            if (info is not ARMigrationRestoreFileInfo migrationInfo)
                            {
                                mLog.Warn($"the info {info.id}, {info.serverRelativeUrl} is not a valid migration restore file info.");
                                errorItemIds.Add(info.id);
                                continue;
                            }

                            IAveFile restoredFile = mWeb.GetFile(migrationInfo.serverRelativeUrl);


                            if (!restoredFile.Exists)
                            {
                                mLog.Info($"The restore file not exists. will set failed,Id {migrationInfo.id}, Url {migrationInfo.serverRelativeUrl}");
                                Report.HasErrorNode = true;
                                if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(migrationInfo.id))
                                {
                                    mAllJobStatus[multiImportParameter.ListId].SetReportStatus(migrationInfo.id, JobDetailsStatus.Failed);
                                }
                                errorItemIds.Add(info.id);
                                continue;
                            }
                            else if (restoredFile.CheckedOutByUser != null)
                            {
                                restoredFile.CheckIn("");
                            }

                            IAveFile stubfile = mWeb.GetFile(migrationInfo.StubPath);
                            //Restored file exist and stub exist->Delete stub.
                            if (stubfile.Exists)
                            {
                                DeleteStubFile(stubfile);
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Warn("An error occur when post-processing item, error is :{0}", e.ToString());
                            errorItemIds.Add(info.id);
                            Report.HasErrorNode = true;
                        }
                    }

                    #region send report
                    mLog.Info($"Start internal send report in SendJobReportsForCanceledJob");
                    InternalSendReports(multiImportParameter, mWeb);
                    mLog.Info($"End internal send report in SendJobReportsForCanceledJob");
                    #endregion
                }
            }
        }

        private void InternalSendReports(MutliImportParameter multiImportParameter, IAveWeb web = null, IAveList list = null)
        {
            web ??= multiImportParameter.Site.OpenWeb(multiImportParameter.WebId);
            list ??= web.GetList(multiImportParameter.ListId);
            foreach (var info in multiImportParameter.CurrentRestoreFileIdsList)
            {
                if (NeedStopCurrentJob())
                {
                    return;
                }

                if (info is not ARMigrationRestoreFileInfo migrationInfo)
                {
                    mLog.Warn($"the info {info.id}, {info.serverRelativeUrl} is not a valid migration restore file info.");
                    continue;
                }

                if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(info.id))
                {
                    try
                    {
                        var report = mAllJobStatus[multiImportParameter.ListId].GetReport(info.id);

                        if (report is not MigrationRestoreFileDto migrationReport)
                        {
                            mLog.Warn($"the info {info.id}, {info.serverRelativeUrl} is not a valid migration restore file info.");
                            continue;
                        }

                        if (!migrationReport.VersionsReportDtos.IsNullOrEmpty())
                        {
                            foreach (var item in migrationReport.VersionsReportDtos)
                            {
                                item.Status = report.Status;
                                item.Message = report.Message;
                                AddReport(new AveRestoreReportDto()
                                {
                                    SourcePath = GetNodeFullPath(item.FileUrl),
                                    Size = item.Size,
                                    Status = (RestoreStatus)item.Status,
                                    Type = item.Type.ToString(),
                                    Path = "",
                                    ErrorMessage = item.Message,
                                    StartTime = item.StartTime,
                                    PathMD5 = item.Md5
                                });
                                UpdateStatistics4VersionReport(item, list);
                            }
                        }

                        AddReport(new AveRestoreReportDto()
                        {
                            SourcePath = GetNodeFullPath(report.FileUrl),
                            Size = report.Size,
                            Status = (RestoreStatus)report.Status,
                            Type = migrationReport.NodeType,
                            Path = migrationReport.Path,
                            ErrorMessage = report.Message,
                            StartTime = migrationReport.StartTime,
                            PathMD5 = migrationReport.Md5
                        });

                        if (!(list.ID == Guid.Empty && string.Compare(list.Title, AveConstants.SYSTEM_FOLDER, StringComparison.OrdinalIgnoreCase) == 0))
                        {
                            if (report.Status == (int)JobDetailsStatus.Successful)
                            {
                                if (migrationInfo.Type == AveConstants.TYPE_DOCUMENT && migrationInfo.name != null && !migrationInfo.name.Contains(":"))
                                {
                                    SOArchiverJobInfoStatistics.Instance.FileCurrentVersionCount++;
                                }
                                else if (migrationInfo.Type == AveConstants.TYPE_VERSION ||
                                    (migrationInfo.name != null && migrationInfo.name.Contains(":") && migrationInfo.Type == AveConstants.TYPE_DOCUMENT))
                                {
                                    SOArchiverJobInfoStatistics.Instance.FileHisVersionCount++;
                                }

                                if (migrationInfo.Type == AveConstants.TYPE_LISTITEM || migrationInfo.Type == AveConstants.TYPE_LISTITEMVERSION
                                    || migrationInfo.Type == AveConstants.TYPE_DOCUMENT || migrationInfo.Type == AveConstants.TYPE_VERSION)
                                {
                                    SOArchiverJobInfoStatistics.Instance.ItemAndVersionCountFotTelemetry++;
                                    SOArchiverJobInfoStatistics.Instance.ItemAndVersionExpireSumTime += SOArchiverJobInfoStatistics.Instance.MainJobStartTime - migrationInfo.ArchiveTime;
                                }
                                if (migrationInfo.Type == AveConstants.TYPE_LISTITEM || migrationInfo.Type == AveConstants.TYPE_LISTITEMVERSION)
                                {
                                    SOArchiverJobInfoStatistics.Instance.ItemSizeSumForTelemetry += ContractConstants.ITEMSIZEFORLICENSE;
                                    SOArchiverJobInfoStatistics.Instance.ItemCountForTelemetry++;
                                    SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(ContractConstants.ITEMSIZEFORLICENSE, report.FileUrl);
                                }
                                else
                                {
                                    if (migrationInfo.Type != AveConstants.TYPE_ATTACHMENTS)
                                    {
                                        RecordRestoredFile.InsertIntoTable(migrationInfo.StorageId, migrationInfo.RowKey, report.Md5, migrationInfo.BackUpJobId, report.FileUrl);
                                    }
                                    SOArchiverJobInfoStatistics.Instance.ItemSizeSumForTelemetry += report.Size;
                                    SOArchiverJobInfoStatistics.Instance.ItemCountForTelemetry++;
                                    SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(report.Size, report.FileUrl);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("An error occur when sending job report, error is :{0}", e.ToString());
                    }

                    mAllJobStatus[multiImportParameter.ListId].RemoveReports(info.id);
                    Report.UpdateProgress();
                }
            }
        }

        private void UpdateStatistics4VersionReport(MigrationRestoreVersionDto item, IAveList list)
        {
            if (!(list.ID == Guid.Empty && string.Compare(list.Title, AveConstants.SYSTEM_FOLDER, StringComparison.OrdinalIgnoreCase) == 0))
            {
                if (item.Status == (int)JobDetailsStatus.Successful)
                {
                    if (item.Type == AveConstants.TYPE_DOCUMENT && item.Name != null && !item.Name.Contains(":"))
                    {
                        SOArchiverJobInfoStatistics.Instance.FileCurrentVersionCount++;
                    }
                    else if (item.Type == AveConstants.TYPE_VERSION ||
                        (item.Name != null && item.Name.Contains(":") && item.Type == AveConstants.TYPE_DOCUMENT))
                    {
                        SOArchiverJobInfoStatistics.Instance.FileHisVersionCount++;
                    }

                    if (item.Type == AveConstants.TYPE_LISTITEM || item.Type == AveConstants.TYPE_LISTITEMVERSION
                        || item.Type == AveConstants.TYPE_DOCUMENT || item.Type == AveConstants.TYPE_VERSION)
                    {
                        SOArchiverJobInfoStatistics.Instance.ItemAndVersionCountFotTelemetry++;
                        SOArchiverJobInfoStatistics.Instance.ItemAndVersionExpireSumTime += SOArchiverJobInfoStatistics.Instance.MainJobStartTime - item.ArchiveTime;
                    }
                    if (item.Type == AveConstants.TYPE_LISTITEM || item.Type == AveConstants.TYPE_LISTITEMVERSION)
                    {
                        SOArchiverJobInfoStatistics.Instance.ItemSizeSumForTelemetry += ContractConstants.ITEMSIZEFORLICENSE;
                        SOArchiverJobInfoStatistics.Instance.ItemCountForTelemetry++;
                        SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(ContractConstants.ITEMSIZEFORLICENSE, item.FileUrl);
                    }
                    else
                    {
                        if (item.Type != AveConstants.TYPE_ATTACHMENTS)
                        {
                            RecordRestoredFile.InsertIntoTable(item.StorageId, item.RowKey, item.Md5, item.BackUpJobId, item.FileUrl);
                        }
                        SOArchiverJobInfoStatistics.Instance.ItemSizeSumForTelemetry += item.Size;
                        SOArchiverJobInfoStatistics.Instance.ItemCountForTelemetry++;
                        SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(item.Size, item.FileUrl);
                    }
                }
            }
        }

        public string GetNodeFullPath(string nodePath)
        {
            string nodeFullPath = string.Empty;
            if (nodePath.StartsWith(this.AveSite.SiteUrl, StringComparison.OrdinalIgnoreCase) || nodePath.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                nodeFullPath = nodePath;
            }
            else
            {
                nodeFullPath = this.AveSite.SiteUrl + "/" + nodePath.TrimStart('/');
            }
            return nodeFullPath;
        }

        // handle declare restored file and delete stub file
        // this method is opposite with BulkDeclareAndDelete in Archive job (declare stub and delete source file)
        private void BulkDeclareAndDelete(IAveWeb mWeb, IAveList mList, List<ARMigrationRestoreFileInfo> infos, MutliImportParameter multiImportParameter, ref List<string> errorItemIds)
        {
            try
            {
                //1.先remove掉没有成功生成的Stub数据，这种数据不需要删除源文件.同时也不需要对Declare文件执行Declare操作.
                List<BulkDeclareAndDeleteMigrationFileInfo> declareRecordList = [];
                List<BulkDeclareAndDeleteMigrationFileInfo> deleteStubList = [];

                foreach (ARMigrationRestoreFileInfo mRRestoreFileInfo in infos)
                {
                    BulkDeclareAndDeleteMigrationFileInfo bulkDeclareAndDeleteFileInfo = new()
                    {
                        mARRestoreFileInfo = mRRestoreFileInfo
                    };

                    if (mRRestoreFileInfo.NeedDeclareRecord)
                    {
                        using (new AvePerformanceScope("SP2013ArchiveBackUp.BulkDeclareAndDelete.GetRestoredItem"))
                        {
                            IAveFile restoredFile = mWeb.GetFile(mRRestoreFileInfo.serverRelativeUrl);
                            if (!restoredFile.Exists)
                            {
                                mLog.Warn("Restored file does not exist when BulkDeclareAndDelete.restoredFile: {0}.", mRRestoreFileInfo.serverRelativeUrl);
                                Report.HasErrorNode = true;
                                if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(mRRestoreFileInfo.id))
                                {
                                    mAllJobStatus[multiImportParameter.ListId].SetReportStatus(mRRestoreFileInfo.id, JobDetailsStatus.Failed);
                                }
                                errorItemIds.Add(mRRestoreFileInfo.id);
                                //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, new Guid(mRRestoreFileInfo.id), 50000, mRRestoreFileInfo.subjobid);
                                continue;
                            }
                            else if (restoredFile.CheckedOutByUser != null)
                            {
                                restoredFile.CheckIn("");
                            }
                            bulkDeclareAndDeleteFileInfo.restoredListItem = restoredFile.Item;
                            bulkDeclareAndDeleteFileInfo.restoredItemRowId = restoredFile.Item.ID;
                        }

                        declareRecordList.Add(bulkDeclareAndDeleteFileInfo);
                    }

                    if (mRRestoreFileInfo.NeedDeleteStub)
                    {
                        deleteStubList.Add(bulkDeclareAndDeleteFileInfo);
                    }
                }
                //2.如果勾选Declare，则对Stub文件执行Declare操作(先批量，出异常再one by one).
                //测试结果：批量Declare，Declare数据和非Declare数据同时存在，也可以批量Declare成功。

                //if (mConfig.IsOneDriverSite)
                if (declareRecordList.Count > 0)
                {
                    if (IsOnedrive(mWeb.Site.Url)) // need skip declare for onedrive
                    {
                        try
                        {
                            mLog.Warn("OneDrive site unable declare item as record, will update report message and status");
                            foreach (var bulkDeclareFileInfo in declareRecordList)
                            {
                                mAllJobStatus[multiImportParameter.ListId].SetReportMessage(bulkDeclareFileInfo.mARRestoreFileInfo.id, "RM_SO_OneDriveDeclareItem_ErrorMessage");
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Error($"Fail check or update onedrive stub file skip decalre, ex:{e}");
                        }
                    }
                    else
                    {
                        try
                        {
                            using (new AvePerformanceScope("SP2013ArchiveBackUp.BulkDeclareAndDelete.BulkDeclareRestoredItem"))
                            {
                                mLog.Info("Begin BulkDeclareRestoredItem. ItemsCount : {0}.", declareRecordList.Count);
                                mList.DeclareItemsByRowIds(declareRecordList.Select(x => x.restoredItemRowId).ToList());
                                mLog.Info("End BulkDeclareRestoredItem. ItemsCount : {0}.", declareRecordList.Count);
                            }
                        }
                        catch (Exception ex)
                        {
                            mLog.Warn("Failed DeclareItemsByRowIds and declare restored item one by one.Message:{0}", ex.ToString());
                            //one by one declare
                            foreach (var bulkDeclareAndDeleteFileInfo in declareRecordList)
                            {
                                using (new AvePerformanceScope("SP2013ArchiveBackUp.BulkDeclareAndDelete.DeclareItemOneByOne"))
                                {
                                    try
                                    {
                                        if (!ScheduleConfiguration.CheckisRecord(bulkDeclareAndDeleteFileInfo.restoredListItem))
                                        {
                                            Record.DeclareItemAsRecord(bulkDeclareAndDeleteFileInfo.restoredListItem);
                                            mLog.Info("Success declare file one by one.Items Url: {0}.", bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.serverRelativeUrl.LogBase64());
                                        }
                                    }
                                    catch (Exception exc)
                                    {
                                        mLog.Warn("Declare Item has some error when one by one declare. Detail : {0}.", exc.ToString());
                                        Report.HasErrorNode = true;
                                        bulkDeclareAndDeleteFileInfo.hasErrorNode = true;
                                        if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id))
                                        {
                                            mAllJobStatus[multiImportParameter.ListId].SetReportStatus(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id, JobDetailsStatus.Failed);
                                        }
                                        continue;
                                    }
                                }
                            }
                        }
                    }
                }
                //3.Stub操作完成后，删除源文件(先批量，出异常再one by one).
                //测试结果：批量删除会删除正常数据，特殊的数据删除不掉，比如五个数据，两个Declare，三个正常，那么正常的三个数据会删除掉，两个Declare文件删除不掉。
                if (deleteStubList.Count > 0)
                {
                    try
                    {
                        using (new AvePerformanceScope("SP2013ArchiveBackUp.BulkDeclareAndDelete.BulkDeleteStub"))
                        {
                            mLog.Info("Begin BulkDeleteStub. ItemsCount : {0}.", deleteStubList.Count);
                            try
                            {
                                StringBuilder builder = new StringBuilder();
                                foreach (var f in deleteStubList)
                                {
                                    builder.AppendFormat("{0},", f.mARRestoreFileInfo.rowid);
                                }
                                mLog.Info($"Begin BulkDeleteStub.Ids {builder.ToString()}");
                            }
                            catch (Exception e)
                            {
                                mLog.Error($"error occured when BulkDeclareAndDelete1,error:{e}");
                            }

                            mList.DeleteItemsByRowIds(deleteStubList.Select(x => x.mARRestoreFileInfo.rowid).ToList());
                            mLog.Info("End BulkDeleteStub. ItemsCount : {0}.", deleteStubList.Count);
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Failed DeleteItemsByRowIds and delete stub one by one.Message:{0}", ex.ToString());
                        //one by one delete
                        foreach (var bulkDeclareAndDeleteFileInfo in deleteStubList)
                        {
                            using (new AvePerformanceScope("SP2013ArchiveBackUp.BulkDeclareAndDelete.DeleteStubOneByOne"))
                            {
                                try
                                {
                                    IAveFile file = mWeb.GetFile(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.StubPath);
                                    if (file != null && !file.Exists)
                                    {
                                        mLog.Warn("Current stub already deleted in batch.Items Url: {0}.", bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.StubPath.LogBase64());
                                        continue;
                                    }
                                    else
                                    {
                                        DeleteStubFile(file);
                                        //SendDestructionReport(multiImportParameter.Site, bulkDeclareAndDeleteFileInfo);
                                    }
                                }
                                catch (Exception exc)
                                {
                                    log.Error($"Error in remove archive stub. stub name: {bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.StubPath}, Ex: {exc}.");
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Warn("An error occur when post-processing item, error is :{0}", e.ToString());
                Report.HasErrorNode = true;
            }
        }

        private bool IsOnedrive(string siteUrl)
        {
            var reg = new Regex(@"https://([^/]+?)-my\.(sharepoint[^/]*)(/.*)?");
            var matches = reg.Match(siteUrl);
            if (matches.Success)
            {
                mLog.Info($"Current site is onedrive site. Url:[{siteUrl}]");
            }
            return matches.Success;
        }

        private void SendErrorJobReport(string errorMessage, MutliImportParameter multiImportParameter)
        {
            try
            {
                //RemoveReports
                mLog.Error("an error occurred when running job. error is {0}", errorMessage);
                if (NeedStopCurrentJob())
                {
                    mLog.Info("this job has stopped");
                    return;
                }
                Report.HasErrorNode = true;
                Report.summaryComments = errorMessage;
                #region send report
                mLog.Info($"Start internal send report in SendErrorJobReport");
                InternalSendReports(multiImportParameter);
                mLog.Info($"End internal send report in SendErrorJobReport");
                #endregion
            }
            catch (Exception e)
            {
                mLog.Error("An error occurred set error job report, Exception: " + e.ToString());
            }
        }
        public void EmptyListPostAction(Guid listId)
        {
            //bool isLastJob = false;
            //if (mAllJobStatus.ContainsKey(listId))
            //{
            //    //only check all list job finished need lock
            //    lock (mAllJobStatus)
            //    {
            //        mAllJobStatus[listId].AddJobsFinished = true;
            //        if (mAllJobStatus[listId].JobCount == 0 && mAllJobStatus[listId].AddJobsFinished)
            //        {
            //            isLastJob = true;
            //        }
            //    }
            //    if (isLastJob)
            //    {
            //        SPMContextBase context = new SPMContextBase();
            //        context.OperationType = SPMOperationTypeEnum.Restore;
            //        context.PlatformType = base.SPMContext.PlatformType;
            //        Guid endListId = Guid.Empty;
            //        Guid webOldId = Guid.Empty;
            //        bool canUpdateListEnd = false;
            //        if (mAveList != null && mAveList.ListInfo != null && ParentAveWeb != null && ParentAveWeb.WebInfo != null)
            //        {
            //            endListId = mAveList.ListInfo.Id;
            //            webOldId = ParentAveWeb.WebInfo.OldWebId;
            //            canUpdateListEnd = true;
            //        }
            //        RestoreListCachedData();
            //        string folderUrl = "****************";
            //        lock (this.mReport)
            //        {
            //            List<SPMReportDto> reports = new List<SPMReportDto>();
            //            foreach (KeyValuePair<string, List<SPMReportDto>> re in mAllJobStatus[listId].ReportDto)
            //            {
            //                try
            //                {
            //                    if (re.Value[0].ObjectType == AveConstants.TYPE_FOLDER && re.Value[0].Status == JobReportDetailStatus.Failed)
            //                    {
            //                        folderUrl = re.Value[0].Path;
            //                    }
            //                    else if (re.Value[0].Path != null && re.Value[0].Path.StartsWith(folderUrl))
            //                    {
            //                        re.Value[0].Status = JobReportDetailStatus.Failed;
            //                        re.Value[0].Path = string.Empty;
            //                        re.Value[0].Operation = OperationType.None;
            //                        re.Value[0].Messages = new List<SPMInternatMessage>() { new SPMInternatMessage()
            //                        {
            //                            Key = "Migration_SharePoint_ImportParentObjectFailed",
            //                            Format = SPMReportResource.Migration_SharePoint_ImportParentObjectFailed,
            //                            Args = new string[] { re.Value[0].RelatedObjTitle.Replace('/', '\\') }
            //                        }};
            //                    }

            //                    if (re.Value[0].Status != JobReportDetailStatus.Success && re.Value[0].Status != JobReportDetailStatus.Exceptional)
            //                    {
            //                        re.Value[0].Size = 0;
            //                    }
            //                    context.ObjectTitle = re.Value[0].RelatedObjTitle.ToString();
            //                    context.ObjectType = SPMContextBase.GetObjectTypeFromChar(re.Value[0].ObjectType);
            //                    reports.AddRange(re.Value);
            //                }
            //                catch (Exception e)
            //                {

            //                    mLog.Warn("An error occurred while add report {0}", e.ToString());
            //                }
            //            }
            //            this.mReport.SendReportAndSummary(reports);

            //            if (canUpdateListEnd)
            //            {
            //                List<SPMReportDto> endReportDtos = new List<SPMReportDto>();
            //                SPMReportDto listEndReport = new SPMReportDto();
            //                listEndReport.ConfigurationType = "ListEnd";
            //                listEndReport.SourceParentListUniqueID = endListId;
            //                listEndReport.SourceSiteID = webOldId;
            //                endReportDtos.Add(listEndReport);
            //                mReport.SendReportAndSummary(endReportDtos);
            //            }

            //            if (mAllJobStatus.TryRemove(listId, out var resources))
            //            {
            //                resources = null;
            //            }
            //        }
            //    }
            //}
        }

        private void DeleteStubFile(IAveFile file)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("SP2013ArchiveBackUp.DeleteStubFile"))
            {
                try
                {
                    IAveListItem listItem = file.Item;
                    if (ScheduleConfiguration.CheckisRecord(listItem))
                    {
                        mLog.Info("This Stub File is Declare File.FileName:{0}", file.UniqueId);
                        Record.UndeclareItemAsRecord(listItem);
                    }
                    if (ArchiverCommonStaticMethod.IsHaveRecordLabel(listItem))
                    {
                        mLog.Info("This Stub File is locked by record label File.FileName:{0}", file.UniqueId);
                        listItem.SetComplianceTagOnBulkItems("");
                    }
                    try
                    {
                        file.Delete();
                        //mLog.Info("Delete the file successfully. {0}", file.ServerRelativeUrl);
                    }
                    catch (Exception e)
                    {
                        string errorMessage = e.ToString();
                        #region Error Code: 6404
                        if (errorMessage.Contains("Code d'erreur : 6404")//法语
                            || errorMessage.Contains(": 6404")
                            || errorMessage.Contains("Error Code: 6404")
                            || errorMessage.Contains("Код ошибки: 6404")//俄语
                            || errorMessage.Contains("Impossible de supprimer le fichier")
                            || errorMessage.Contains("Cannot remove file"))

                        {
                            mLog.Info("Delete file has 6404 error and need reget file.File url:{0}.", file.ServerRelativeUrl);
                            AveTaskRetryHelper retryHelper = new AveTaskRetryHelper(3, true);
                            retryHelper.ExecuteWithRetryMechanism(() =>
                            {
                                file = Config.ObjectModelFactory.CreateSite(file.ParentFolder.ParentList.ParentWeb.Site.Url).OpenWeb(file.ParentFolder.ParentList.ParentWeb.ID).GetFile(file.UniqueId, file.ServerRelativeUrl);
                                if (file.Exists)
                                {
                                    file.Delete();
                                }
                                else
                                {
                                    mLog.Info("Current file:{0} doesn't exist when Error Code: 6404.", file.ServerRelativeUrl);
                                }
                            });
                            mLog.Info("Delete 6404 error file success.File name:{0}", file.Name);
                        }
                        #endregion
                        #region check out/lock/label file.
                        if (e.InnerException != null
                            && (e.InnerException.Message.Contains("is checked out for editing by")
                            || e.InnerException.Message.Contains("est extrait pour modification par")//法语
                            || e.InnerException.Message.Contains("został wyewidencjonowany do edycji przez użytkownika")//波兰语
                            || e.InnerException.Message.Contains("извлечен для редактирования пользователем")//俄语
                            || e.InnerException.Message.Contains("Foi feito o check-out para edição do arquivo")
                            || e.InnerException.Message.Contains("Foi dada saída ao ficheiro")//葡萄牙语
                            || e.InnerException.Message.Contains("zur Bearbeitung ausgecheckt")//德语
                            ))
                        {
                            mLog.Info("Current file is check out file and Records check in and delete.FileName:{0}.", file.Name);
                            file.CheckIn("");
                            file.Delete();
                            mLog.Info("Delete check out document success.File name:{0}", file.Name);
                            return;
                        }
                        #endregion
                        #region Item cannot be deleted while on hold
                        if (e.InnerException != null
                            && (e.InnerException.Message.Contains("Item cannot be deleted while on hold.")))
                        {
                            mLog.Info("Current file is is hold file.FileName:{0}. will retry.", file.Name);
                            Thread.Sleep(500);
                            file.Delete();
                            mLog.Info("Delete hold file success.File name:{0}", file.Name);
                            return;
                        }
                        #endregion
                        else
                        {
                            file = file.ParentFolder.ParentList.ParentWeb.GetFile(file.UniqueId, file.ServerRelativeUrl);
                            file.Delete();
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLog.Debug("Stub File Delete Error: {0} error message: {1}", file.Name, ex.ToString());
                }
            }
        }

        private bool NeedStopCurrentJob()
        {
            try
            {
                using (new CheckJobStopScope()) { }
            }
            catch (JobStopException)
            {
                return true;
            }
            return false;
        }

        #endregion

        #region---Process Data---
   
        public bool ProcessFolderXML(RestoreContentDto aveFolderDto)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.RestoreFolder"))
            {
                if (!string.IsNullOrEmpty(targetSiteUrl))
                {
                    aveFolderDto = ConvertRestoreContentDtoForArchiverOOPRestore(aveFolderDto);
                }
                var reportDto = new AveRestoreReportDto { Type = aveFolderDto.Type.ToString(), Title = ReportAbsolutePath.GetTitle(aveFolderDto.Name) }; //Path = aveFolderDto.Name
                if (AveList != null && AveList.NeedContinue == false)
                {
                    //List Skipped,we should not add item\folder under the list to report.
                    return false;
                }
                if (AveList == null || this.aveListRootFolder == null)
                {
                    if (aveFolderDto.IsAppData)
                    {
                        return false;
                    }
                    reportDto.SourcePath = aveFolderDto.SrcUrl;
                    reportDto.Status = RestoreStatus.Skipped;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(RestoreReportKey.Item_CanNotFindFolderParent.ToString(), RestoreReportResource.Item_CanNotFindFolderParent, aveFolderDto.Name);
                    AddReport(reportDto);
                    return false;
                }
                bool? isFolderExist = null;
                try
                {
                    string parentPath = this.mListPath;
                    if (!aveFolderDto.Name.StartsWith(parentPath, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new AveException(@"Looks up a localized string similar to The folder does not belong to the current list. Folder Path: {0} List Path: {1}.", aveFolderDto.Name, this.mListPath);
                    }
                    string subPath = aveFolderDto.Name.Substring(parentPath.Length).TrimStart('\\');
                    string nameWithoutSpecialChar = AveConverter.DecodeSpecialChar(mListPath);
                    aveFolderDto.SrcName = aveFolderDto.SrcName.Replace(mListPath, nameWithoutSpecialChar);
                    reportDto.Title = reportDto.Title.Replace(mListPath, nameWithoutSpecialChar);
                    var currentFolder = aveFolder;
                    this.aveFolder = null;
                    this.aveFolder = GenerateFolder(currentFolder, subPath);
                    if (aveFolder.ParentFolder != null && (aveFolder.ParentFolder.SPFolder == null || !aveFolder.ParentFolder.SPFolder.Exists))
                    {
                        aveFolder.ParentFolder.InitSPFolder(true);
                    }
                    var securityRestoreOption = new SecurityRestoreOption()
                    {
                        IsIncludeShareLink = WrapperConfiguration.WrapperConfigurationForBPOS.IsIncludeShareLinks
                    };
                    GlobalRestoreOptionWorker.CheckFolderGlobalSetting(aveFolder, aveFolderDto, securityRestoreOption);
                    this.aveFolder.SetRestoreOption(aveFolderDto.RestoreOption);
                    aveFolder.ParentList.BackupListSetting();
                    this.aveFolder.RestoringItem.IsIncludingRecycleBinData = (Config).IncludingRecycleBinData;

                    if (string.IsNullOrEmpty(subPath)) //Restore to list Root Folder
                    {
                    }
                    else if (string.Compare(aveFolderDto.Name, "{System Folder}", StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        this.aveFolder.InitSPFolder();
                    }
                    else
                    {
                        var isVersion = false;
                        bool skipFolder = false;
                        bool skipFolderByTime = false;
                        string folderName = subPath.Contains("\\") ? subPath.Substring(subPath.LastIndexOf("\\") + 1) : subPath;
                        var folderUrl = mAveListRootFolder.ServerRelativeUrl + '/' + subPath;
                        AveMetadata metadata;
                        var docData = new Dictionary<string, object>();
                        //this.aveFolder.ParentList.BackupListSetting();
                        while ((metadata = RestoreStream.ReadMetadata()) != null)
                        {
                            switch (metadata.MetadataType)
                            {
                                case AveMetadataType.DocProperty:
                                    docData.Clear();
                                    metadata.GetMetadata(docData);
                                    #region User & Group Cache, we may need it in the future in item level

                                    AveMetadata userCacheMetadata = RestoreStream.TryReadMetadata(AveMetadataType.UserCache);
                                    if (userCacheMetadata != null)
                                    {
                                        var userList = userCacheMetadata.GetMetadata<AveUserList>();
                                        this.aveFolder.ParentList.ParentWeb.ParentSite.SPMembers.MultiThreadRestoreUsers(userList.Users, false, false, Config.ExcludeGroupWithoutPermissions);
                                    }

                                    AveMetadata groupCacheMetadata = RestoreStream.TryReadMetadata(AveMetadataType.GroupCache);
                                    if (groupCacheMetadata != null && WrapperConfiguration.WrapperConfigurationForBPOS.IsIncludeShareLinks)
                                    {
                                        AveGroupList groupList = groupCacheMetadata.GetMetadata<AveGroupList>();
                                        this.aveFolder.ParentList.ParentWeb.ParentSite.SPMembers.RestoreGroups(groupList.Groups, true, false);
                                    }

                                    #endregion
                                    try
                                    {
                                        //restore document MMS
                                        var metaData = RestoreStream.TryReadMetadata(AveMetadataType.MetadataService);
                                        if (metaData != null)
                                        {
                                            List<AveTermStoreInfo> termStoreInfos = metaData.GetMetadata<List<AveTermStoreInfo>>();
                                            this.aveFolder.ParentSite.MetadataService.Restore(termStoreInfos);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        log.Error("Failed restore document meta data, due to {0}", e);
                                    }
                                    AveMetadata userDataMetadata = RestoreStream.TryReadMetadata(AveMetadataType.DocData);
                                    var userData = new Dictionary<string, object>();
                                    if (userDataMetadata != null)
                                    {
                                        userDataMetadata.GetMetadata(userData);
                                    }
                                    AveMetadata dataJuntionMetadata = RestoreStream.TryReadMetadata(AveMetadataType.DocDataJunction);
                                    List<Dictionary<string, object>> dataJunction = null;
                                    if (dataJuntionMetadata != null)
                                    {
                                        dataJunction = dataJuntionMetadata.GetMetadata<List<Dictionary<string, object>>>();
                                    }
                                    #region Item Dependency
                                    ItemLevelRestoreItemCTAndFields(userData, dataJunction, aveFolder);
                                    #endregion
                                    if (this.aveFolder.CheckRestoreOption(AveRestoreMode.Replace) &&
                                        ReplaceType.Equals(AveConstants.TYPE_FOLDER))
                                    {
                                        bool exist = ReplaceWorker.DeleteFolder(AveList, this.aveFolder);
                                        NullableBooleanExtension.SetIfValueNotExist(ref isFolderExist, exist);
                                    }
                                    try
                                    {
                                        //this.aveFolder.RestoreSelf(data, userData, dataJunction);
                                        //using (var report = this.aveFolder.GetReport())
                                        //{
                                        //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveFolderDto));
                                        //}

                                        var currentSPFolderObject = new SPGenericObject();
                                        currentSPFolderObject.Id = docData["Id"].ToString();
                                        currentSPFolderObject.ObjectType = SPObjectType.SPFolder;
                                        currentSPFolderObject.ParentId = docData["PARENTID"].ToString();
                                        currentSPFolderObject.ParentWebId = ParentAveWeb.SPWeb.ID.ToString();
                                        currentSPFolderObject.ParentWebUrl = ParentAveWeb.SPWeb.ServerRelativeUrl;
                                        currentSPFolderObject.Url = folderUrl;
                                        var spFolder = new SPFolder()
                                        {
                                            Id = docData["Id"].ToString(),
                                            Url = mAveListRootFolder.SPFolder.Url + '/' + subPath,
                                            Name = folderName,
                                            ParentFolderId = docData["PARENTID"].ToString(),
                                            ParentWebId = ParentAveWeb.SPWeb.ID.ToString(),
                                            ParentWebUrl = ParentAveWeb.ServerRelativeUrl,
                                            ContainingDocumentLibrary = mAveList.SPList.ID.ToString(),
                                            TimeCreated = Convert.ToDateTime(docData["TimeCreated"]),
                                            TimeLastModified = Convert.ToDateTime(docData["TimeLastModified"]),
                                        };
                                        if (userData != null)
                                        {
                                            if (userData.ContainsKey("#tp_ID"))
                                            {
                                                spFolder.ListItemIntId = (int)userData["#tp_ID"];
                                            }
                                        }
                                        if (userData.ContainsKey(AveFieldNameCollection.HTMLFileType)
                                            && userData[AveFieldNameCollection.HTMLFileType] != null
                                            && userData[AveFieldNameCollection.HTMLFileType].ToString().Equals("Sharepoint.DocumentSet", StringComparison.OrdinalIgnoreCase))
                                        {
                                            userData[AveFieldNameCollection.HTMLFileType] = "Sharepoint.DocumentSet";
                                        }

                                        if (!skipFolder)
                                        {
                                            List<DictionaryEntry> properties = new List<DictionaryEntry>();

                                            if ((userData == null || userData.Count == 0) && (folderName.ToLower().EndsWith("_files") || folderName.ToLower().EndsWith("_file")))
                                            {
                                                InitFilesFolder(userData, docData["Id"].ToString());
                                            }

                                            //UserDataListItem
                                            //FolderVersiion，Version
                                            if ((userData != null && userData.Count > 0) || folderName.ToLower().EndsWith("_files") || folderName.ToLower().EndsWith("_file"))
                                            {
                                                //Try to find parent folder
                                                ParentFolderInfo = TryToFindParentFolderObject(folderUrl.GetParentUrl());
                                                docData["PARENTID"] = ParentFolderInfo.Id;
                                                SPListItem item = null;
                                                AveSPFolder aveFolder = new AveSPFolder(mAveListRootFolder, folderName); //new AveSPFolder(mAveListRootFolder, folderName, false);
                                                //aveFolder.SetRestoreOption(dto.RestoreOption);
                                                List <DictionaryEntry> mmsProperties = new List<DictionaryEntry>();
                                                using (new AvePerformanceScope("HSWorker.GenerateItemNode"))
                                                {
                                                    item = _manifestProcessor.GenerateSPListItem(docData, userData, dataJunction, ListItemDocType.Folder, false);
                                                }
                                                _manifestProcessor.CopySPListItem(item, spFolder);
                                                currentSPFolderObject.Item = spFolder;
                                                _manifestProcessor.SPObjectCollection.SPObject.Add(currentSPFolderObject);
                                                using (new AvePerformanceScope("HSWorker.ProcessListItemNode"))
                                                {
                                                    //ProcessListItemNode(aveFolder, docData, userData, docDataJunction, isVersion, item, reportDto.ObjectIdentity);
                                                }
                                                if (!isVersion)
                                                {
                                                    _manifestProcessor.SPObjectCollection.SPObject.Add(_manifestProcessor.SPObject);
                                                    try
                                                    {
                                                        reportDto.Path = this.mAveFolder.SPFolder.ParentWeb.Url.TrimStart('/') + "/" + spFolder.Url;
                                                    }
                                                    catch (Exception e)
                                                    {
                                                        mLog.Warn($"Failed to pre-init dto report. {e}");
                                                    }
                                                    //ProcessAlertXml(reportDtos, reportDto, aveFolder);
                                                }
                                            }
                                            else
                                            {
                                                currentSPFolderObject.Item = spFolder;
                                                _manifestProcessor.SPObjectCollection.SPObject.Add(currentSPFolderObject);
                                            }
                                            mCacheSPFolderObjects[mAveList.SPList.ID].TryAdd(currentSPFolderObject.Url.ToHashGuid(), currentSPFolderObject);
                                            ParentSPFolderCache.TryAdd(currentSPFolderObject.Url.ToHashGuid(), Guid.Parse(currentSPFolderObject.Id));
                                            return true;
                                        }
                                        else
                                        {
                                            //AveListItemConflictBaseInfo aveListItemConflictBaseInfo = mAveList.SPList.FoldersCollection[folderPath];
                                            //spFolder.TimeCreated = aveListItemConflictBaseInfo.TimeCreated;
                                            //spFolder.TimeLastModified = aveListItemConflictBaseInfo.Modified;
                                            //spFolder.Author = aveListItemConflictBaseInfo.Author.ToString();
                                            //spFolder.ModifiedBy = aveListItemConflictBaseInfo.Editor.ToString();
                                            //currentSPFolderObject.Item = spFolder;
                                            //if (!(isChannelFolder && GlobalPreferenceSettings.IsSkipChannelFolder))
                                            //{
                                            //    //mSPObjectCollection.SPObject.Add(currentSPFolderObject);
                                            //}
                                            //else
                                            //{
                                            //    isReportImmediate = true;
                                            //}
                                            //ParentSPFolderCache.TryAdd(currentSPFolderObject.Url.ToHashGuid(), Guid.Parse(currentSPFolderObject.Id));
                                            //return false;
                                        }
                                    }
                                    finally
                                    {
                                        NullableBooleanExtension.SetIfValueNotExist(ref isFolderExist, !this.aveFolder.IsNewCreated);
                                    }
                                    break;

                                case AveMetadataType.RoleAssignment:
                                    if (this.aveFolder.CheckRestoreOption(this.aveFolder.IsNewCreated, AveRestoreMode.RestoreSecurity) || GlobalRestoreOptionWorker.GlobalRestoreOption.ContainerSetting.CheckRestoreSecurityOnly())
                                    {
                                        log.Info("Begin restore FolderLevel RoleAssignment.");
                                        var roleAssignments = metadata.GetMetadata<List<AveRoleAssignmentInfo>>();
                                        AveObjectSecurity security = AveObjectSecurity.CreateInstance(this.aveFolder.AveSPItem);
                                        security.SourceHasUniqueRoleAssignment = aveFolder.AveSPItem.HasUniqueRoleAssignments;
                                        security.RestoreRoleAssignments(roleAssignments, securityRestoreOption);
                                        //using (var report = security.GetReport())
                                        //{
                                        //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveFolderDto));
                                        //}
                                    }
                                    break;

                                case AveMetadataType.DocImmedSubscriptions:
                                    if (this.aveFolder.CheckRestoreOption(this.aveFolder.IsNewCreated, AveRestoreMode.OverWrite))
                                    {
                                        var iAlertInfos = metadata.GetMetadata<List<Dictionary<string, object>>>();
                                        AveSPAlert alert = new AveSPFolderAlert(this.aveFolder);
                                        foreach (var iAlertInfo in iAlertInfos)
                                        {
                                            alert.RestoreAlert(iAlertInfo, false);
                                        }
                                        //using (var report = alert.GetReport())
                                        //{
                                        //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveFolderDto));
                                        //}
                                    }
                                    break;

                                case AveMetadataType.DocSchedSubscriptions:
                                    if (this.aveFolder.CheckRestoreOption(this.aveFolder.IsNewCreated, AveRestoreMode.OverWrite))
                                    {
                                        var sAlertInfos = metadata.GetMetadata<List<Dictionary<string, object>>>();
                                        AveSPAlert alert = new AveSPFolderAlert(this.aveFolder);
                                        foreach (var sAlertInfo in sAlertInfos)
                                        {
                                            alert.RestoreAlert(sAlertInfo, true);
                                        }
                                        //using (var report = alert.GetReport())
                                        //{
                                        //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveFolderDto));
                                        //}
                                    }
                                    break;
                                //#region Social Tag and Comment
                                //case AveMetadataType.SocialTag:
                                //    if (this.aveFolder.CheckRestoreOption(this.aveFolder.IsNewCreated, AveRestoreMode.OverWrite) &&
                                //        AveEnv.IsMoss)
                                //    {
                                //        List<AveSocialTagInfo> tagInfos = metadata.GetMetadata<List<AveSocialTagInfo>>();
                                //        AveSPSocialTag socialTags = new AveSPSocialTag(this.aveFolder.TagUrl, this.aveFolder.ParentSite);
                                //        socialTags.Restore(tagInfos);
                                //    }
                                //    break;

                                //case AveMetadataType.SocialComment:
                                //    if (this.aveFolder.CheckRestoreOption(this.aveFolder.IsNewCreated, AveRestoreMode.OverWrite) &&
                                //        AveEnv.IsMoss)
                                //    {
                                //        List<AveSocialCommentInfo> commentInfos = metadata.GetMetadata<List<AveSocialCommentInfo>>();
                                //        AveSPSocialComment socialComment = new AveSPSocialComment(this.aveFolder.TagUrl, this.aveFolder.ParentSite);
                                //        using (new AvePerformanceScope("GranularRestore.RestoreDocument.SocialComment"))
                                //        {
                                //            socialComment.Restore(commentInfos);
                                //        }
                                //    }
                                //    break;
                                //#endregion
                                case AveMetadataType.WorkflowInstance:
                                    if (this.aveFolder.CheckRestoreOption(this.aveFolder.IsNewCreated, AveRestoreMode.OverWrite))
                                    {
                                        var wfInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
                                        WFConflictResolution wfResolution = WFConflictResolution.Instance;
                                        foreach (var unit in wfInfo)
                                        {
                                            var wfAssociationUnit = SPWFInstanceUnit.Load(unit.AssociationUnit);
                                            wfResolution.HandleInstanceConflict(wfAssociationUnit, aveFolder.AveSPItem.SPListItem);
                                        }
                                        //using (var report = wfResolution.GetReport())
                                        //{
                                        //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), aveFolderDto));
                                        //}
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    reportDto.Path = AveWeb.SPWeb.Url + '/' + aveFolder.SPFolder.Url;
                    reportDto.Size = RestoreStream.CurrentNodeTransferedSize;
                    //log.Info(RestoreResource.Item_AIRRestoreFolderCurrentFolder, aveFolderDto.Name);
                }
                catch (AvePoint.Wrapper.Common.SkipException e)
                {
                    log.Warn("Skip this folder while restore folder Name:{0} ,Error:{1}", aveFolderDto.Name, e.Message);
                    reportDto.Status = RestoreStatus.Skipped;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(e, RestoreReportKey.Item_ItemSkipped.ToString(), RestoreReportResource.Item_ItemSkipped, aveFolder.Name, e.Message);
                    //this.aveFolder = null;
                }
                catch (AveSecurityTrimingException e)
                {
                    log.Warn("An error occurred while restore folder. Name:{0} ,Error:{1}", aveFolderDto.Name, e);
                    reportDto.Status = RestoreStatus.Skipped;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(e, RestoreReportKey.Item_SecurityFolderSkipped.ToString(), RestoreReportResource.Item_SecurityFolderSkipped, aveFolderDto.Name, e.Message);
                    this.aveFolder = null;
                }
                catch (TeamChannalFolderUpdateFailed e)
                {
                    log.Warn("An error occurred while restore folder,TeamChannalFolderUpdateFailed. Name:{0} ,Error:{1}", aveFolderDto.Name, e);
                    reportDto.Status = RestoreStatus.Skipped;
                    reportDto.ErrorMessage = "RM_RS_RestoreChannelFolderError";
                    //this.aveFolder = null;
                }
                catch (Exception e)
                {
                    log.Error(@"Looks up a localized string similar to An error occurred while restoring a folder. Path: {0} {1}.", aveFolderDto.Name, e);
                    reportDto.Status = RestoreStatus.Failed;
                    if (e.Message != null && e.Message.Contains("This item cannot be updated because it is locked as read-only"))
                    {
                        reportDto.ErrorMessage = "StorageOptimization13_SOARDeleteOfficeLockFile";
                    }
                    else if (e.Message != null && e.Message.Contains("The label that's applied to this item prevents it from being edited or deleted. Check the item's label for more details"))
                    {
                        reportDto.ErrorMessage = "StorageOptimization_SOARRecordManagerLabelDocumentDeleteFailed";
                    }
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(e, RestoreReportKey.Item_RestoreFolderErrorReport.ToString(), RestoreReportResource.Item_RestoreFolderErrorReport, aveFolderDto.Name, e.Message);
                    this.aveFolder = null;
                }
                reportDto.SourcePath = aveFolderDto.SrcUrl;
                if (isFolderExist == true && aveFolderDto.RestoreOption.mAveRestoreMode == AveRestoreMode.Default && reportDto.Status == RestoreStatus.Success)
                {
                    reportDto.Status = RestoreStatus.Skipped;
                }
                reportDto.SetOption(aveFolderDto.RestoreOption.mAveRestoreMode, isFolderExist, reportDto.Status);
                if (!AveList.IsSystemList)
                {
                    AddReport(reportDto);
                }
            }

            return true;
        }

        private void InitFilesFolder(Dictionary<string, object> userData, string Id)
        {
            if (!userData.ContainsKey("#tp_ID"))
            {
                userData["#tp_ID"] = 0;
            }
            if (!userData.ContainsKey("Id"))
            {
                userData["Id"] = Id;
            }
            if (!userData.ContainsKey("#tp_UIVersion"))
            {
                userData["#tp_UIVersion"] = 512;
            }
        }

        private HSMFolderObjectBasicInfo TryToFindParentFolderObject(string objectUrl)
        {
            string parentUrl = objectUrl;
            //Get parent folder object
            if (ParentSPFolderCache.TryGetValue(parentUrl.ToHashGuid(), out var folderId))
            {
                var folderCache = mCacheSPFolderObjects[mAveList.SPList.ID];
                List<SPGenericObject> parentFolderCache = new List<SPGenericObject>();
                while (parentUrl.Length > mAveListRootFolder.ServerRelativeUrl.Length)
                {
                    //Add all parent folder to current package
                    if (folderCache.TryGetValue(parentUrl.ToHashGuid(), out var folderObj) && !CurrentPackageIdList.Contains(folderObj.Id))
                    {
                        parentFolderCache.Add(folderObj);
                    }
                    parentUrl = parentUrl.GetParentUrl();
                }
                parentFolderCache.Reverse();
                foreach (var folder in parentFolderCache)
                {
                    CurrentPackageIdList.Add(folder.Id);
                    _manifestProcessor.SPObjectCollection.SPObject.Add(folder);
                }
                return new HSMFolderObjectBasicInfo() { Url = objectUrl, Id = folderId };
            }
            else
            {
                mLog.Warn($"Parent folder for {objectUrl} can not found");
            }
            return null;
        }

        private AveRestoreMode ProcessDocumentXML(AveSPDoc aveDoc, RestoreContentDto dto, AveRestoreReportDto reportDto, bool isVersion, ref bool? isDocumentExist, IAveRestoreStream restoreStream, ARMigrationRestoreFileInfo restoreFileInfo)
        {
            using (new AvePerformanceScope("ProcessDocumentXML"))
            {
                string aveDocNameOriginal = dto.Name;
                var restoreMode = aveDoc.RestoreOption.mAveRestoreMode;
                AveMetadata metadata;
                var docData = new Dictionary<string, object>();
                var securityRestoreOption = new SecurityRestoreOption()
                {
                    IsIncludeShareLink = WrapperConfiguration.WrapperConfigurationForBPOS.IsIncludeShareLinks
                };
                //if (!isVersion)
                //{
                //    aveFolder.RestoringItem.NeedSkipped = false;
                //}
                GlobalRestoreOptionWorker.CheckDocumentGlobalSetting(aveFolder, dto, securityRestoreOption);
                //Backup the setting of parent list and then change it for item restore.
                aveDoc.ParentFolder.ParentList.BackupListSetting();
                
                string key = Guid.NewGuid().ToString();
                while ((metadata = restoreStream.ReadMetadata()) != null)
                {
                    switch (metadata.MetadataType)
                    {
                        case AveMetadataType.DocProperty:
                            docData.Clear();
                            docData = metadata.GetMetadata<Dictionary<string, object>>();

                            #region User & Group Cache, we may need it in the future in item level
                            AveMetadata userCacheMetadata = restoreStream.TryReadMetadata(AveMetadataType.UserCache);
                            if (userCacheMetadata != null)
                            {
                                var userList = userCacheMetadata.GetMetadata<AveUserList>();
                                lock (LockerDispatcher.GetLocker("UserInfoLock"))
                                {
                                    foreach (AveUserInfo userInfo in userList.Users)
                                    {
                                        aveDoc.ParentSite.SPMembers.RestoreUser(userInfo, false, false, Config.ExcludeGroupWithoutPermissions);
                                    }
                                }
                            }
                            bool hasSensitivityLabels = false;
                            //if (docData.ContainsKey("_IpLabelId") && !string.IsNullOrEmpty(docData["_IpLabelId"].ToString()))
                            //{
                            //    log.Info("[SensitivityLabel]This file has Sensitivity Labels.");
                            //    hasSensitivityLabels = true;
                            //}
                            //else if (aveDoc.CheckIfHasSensitivityLabels())
                            //{
                            //    log.Info("[SensitivityLabel]This file in the sharepoint has Sensitivity Labels.");
                            //    hasSensitivityLabels = true;
                            //}
                            AveMetadata groupCacheMetadata = restoreStream.TryReadMetadata(AveMetadataType.GroupCache);
                            if (groupCacheMetadata != null && WrapperConfiguration.WrapperConfigurationForBPOS.IsIncludeShareLinks)
                            {
                                AveGroupList groupList = groupCacheMetadata.GetMetadata<AveGroupList>();
                                this.aveFolder.ParentList.ParentWeb.ParentSite.SPMembers.RestoreGroups(groupList.Groups, true, false);
                            }

                            #endregion

                            try
                            {
                                //restore document MMS
                                var metaData = restoreStream.TryReadMetadata(AveMetadataType.MetadataService);
                                if (metaData != null)
                                {
                                    List<AveTermStoreInfo> termStoreInfos = metaData.GetMetadata<List<AveTermStoreInfo>>();
                                    aveDoc.ParentSite.MetadataService.Restore(termStoreInfos);
                                }
                            }
                            catch (Exception e)
                            {
                                log.Error("Failed restore document meta data, due to {0}", e);
                            }

                            var userData = new Dictionary<string, object>();
                            var userDataMetadata = restoreStream.TryReadMetadata(AveMetadataType.DocData);
                            if (userDataMetadata != null)
                            {
                                userData = userDataMetadata.GetMetadata<Dictionary<string, object>>();
                            }

                            List<Dictionary<string, object>> dataJunction = null;
                            var dataJuntionMetadata = restoreStream.TryReadMetadata(AveMetadataType.DocDataJunction);
                            if (dataJuntionMetadata != null)
                            {
                                dataJunction = dataJuntionMetadata.GetMetadata<List<Dictionary<string, object>>>();
                            }

                            List<AveWebPartBaseInfo> webParts = null;
                            AveMetadata webpartMetadata = restoreStream.TryReadMetadata(AveMetadataType.DocWebPart);
                            if (webpartMetadata != null)
                            {
                                webParts = webpartMetadata.GetMetadata<List<AveWebPartBaseInfo>>();
                            }


                            if (ItemVersionFilter.EnableVersionFilter &&
                                !IsRelatedVersionsContainsThis(docData, aveDoc.AveSPItem, restoreStream))
                            {
                                throw new Wrapper.Common.SkipException("Looks up a localized string similar to The version is filtered out..");
                            }

                            #region Item Dependency
                            ItemLevelRestoreItemCTAndFields(userData, dataJunction, aveDoc);
                            #endregion

                            #region conflict resolution
                            if (aveDoc.CheckRestoreOption(AveRestoreMode.AppendANewVersion))
                            {
                                if (AddNewVersionForDuplicateItem(docData, aveDoc))
                                {
                                    //reportDto.Path = AveItemRestoreUtility.GetItemVersionString(aveDoc.Name, (int)data["UIVersion"]);
                                    this.aveFolder.RestoringItem.ResetNewItemValues(true, aveDoc.Name, aveDoc.Name);
                                    NullableBooleanExtension.SetIfValueNotExist(ref isDocumentExist, true);//Append a new version
                                }
                                else
                                {
                                    restoreMode = AveRestoreMode.Default;
                                }
                            }
                            if ((docData.ContainsKey("IsCurrentVersion") && (bool)docData["IsCurrentVersion"]) || userData.ContainsKey("#tp_IsCurrent") && (bool)userData["#tp_IsCurrent"])
                            {
                                reportDto.Title = aveDoc.Name;
                                reportDto.Path = ReportAbsolutePath.GetDocumentAP(AveSite.SiteUrl, AveSite.ServerRelativeUrl, aveFolder.SPFolder.ServerRelativeUrl, aveDoc.Name);
                                //migrationReportDto.Path = ReportAbsolutePath.GetDocumentAP(AveSite.SiteUrl, AveSite.ServerRelativeUrl, aveFolder.SPFolder.ServerRelativeUrl, aveDoc.Name);
                            }
                            else
                            {
                                reportDto.Title = aveDoc.Name + ":" + GetUIVersionString(Convert.ToInt32(docData["UIVersion"]));
                                reportDto.Path = ReportAbsolutePath.GetDocumentVersionAP(AveSite.SiteUrl, AveSite.ServerRelativeUrl, AveWeb.SPWeb.Url, Convert.ToInt32(docData["UIVersion"]), aveFolder.SPFolder.Url, aveFolder.SPFolder.ServerRelativeUrl, aveDoc.Name);
                                //migrationReportDto.Path = ReportAbsolutePath.GetDocumentVersionAP(AveSite.SiteUrl, AveSite.ServerRelativeUrl, AveWeb.SPWeb.Url, (int)docData["UIVersion"], aveFolder.SPFolder.Url, aveFolder.SPFolder.ServerRelativeUrl, aveDoc.Name);
                            }
                            #endregion
                            

                            var oldFileUniqueId = docData.TryGetValue("Id", out object oldIdObj) ? oldIdObj.ToString() : docData.TryGetValue("UniqueId", out object oldUniqueIdObj) ? oldUniqueIdObj.ToString() : Guid.Empty.ToString();
                            // Set a new unique id for restore file as default, will use the existing one in overwrite mode.
                            docData["Id"] = Guid.NewGuid(); 
                            var newFileUniqueId = docData["Id"].ToString();

                            AveRestoreResult result = AveRestoreResult.Normal;
                            try
                            {
                                if (SetNowAsRestoreFileModifyTime && userData.ContainsKey("Modified"))
                                {
                                    userData["Modified"] = DateTime.UtcNow;
                                }

                                _manifestProcessor.InitParentFolder(aveFolder);

                                if (_manifestProcessor.ItemUniqueIdMapping.TryGetValue(oldFileUniqueId, out Guid value))
                                {
                                    docData["Id"] = value;
                                }
                                else
                                {
                                    //Check Skip
                                    if (restoreMode == AveRestoreMode.Default)
                                    {
                                        if (aveFolder.RestoringItem.NeedSkipped || mAveList.SPList.TryGetCachedListItem(_manifestProcessor.GenerateWebRelativeUrl(aveDoc.Name), out _))
                                        {
                                            if (aveFolder.RestoringItem.NeedSkipped)
                                            {
                                                log.Debug($"File version skipped due to current version skipped");
                                            }
                                            else
                                            {
                                                log.Debug($"File exsits in the destination path, will skip the file. sourceId:{oldFileUniqueId}");
                                            }
                                            aveFolder.RestoringItem.NeedSkipped = true;
                                            aveFolder.RestoringItem.NeedSkippedReason = AvePoint.Wrapper.Resource.WrapperRestoreReportResource.Wrapper_SkippedItemByIsSameItem;
                                            aveFolder.RestoringItem.NeedSkippedKey = WrapperReportResourceKey.Wrapper_SkippedItemByIsSameItem.ToString();
                                            aveFolder.RestoringItem.ConfictType = ConfictType.Document;
                                            throw new AveRestoreException(AveRestoreResult.Omit, AveRestoreResult.Omit.ToString());
                                        }
                                        else
                                        {
                                            log.Debug($"File does not exsit in the destination path");
                                        }
                                    }
                                    else if (restoreMode == AveRestoreMode.OverWrite)
                                    {
                                        if (mAveList.SPList.TryGetCachedListItem(_manifestProcessor.GenerateWebRelativeUrl(aveDoc.Name), out var file))
                                        {
                                            if (!string.Equals(oldFileUniqueId, file.UniqueId.ToString()))
                                            {
                                                docData["Id"] = file.UniqueId;
                                                newFileUniqueId = file.UniqueId.ToString();
                                                log.Debug($"File exsits in the destination path, will use the destiantion unique id. sourceId:{oldFileUniqueId} targetId:{newFileUniqueId}");
                                            }
                                            else
                                            {
                                                log.Debug("File exsits in the destination path.");
                                            }
                                        }
                                    }
                                    else if (restoreMode == AveRestoreMode.Append)
                                    {
                                        log.Debug($"File exsits in the destination path, will append the file with new unique id. sourceId:{oldFileUniqueId}, targetId:{newFileUniqueId}");
                                    }

                                    _manifestProcessor.ItemUniqueIdMapping[oldFileUniqueId] = new Guid(newFileUniqueId);
                                }

                                if (ProcessContentData(restoreStream, key, docData, userData, isVersion))
                                {
                                    ConvertDocDataAndUserData(docData, userData);
                                    if (hasSensitivityLabels)
                                    {
                                        //if (ServiceAccountRequestForSensitivityLabel != null)
                                        //{
                                        //    // if has service account, we will use service account request to restore file.
                                        //    SensitivityLabelRestoreOption sensitivityLabelRestoreOption = new SensitivityLabelRestoreOption()
                                        //    {
                                        //        method = SensitivityLabelRestoreMethod.ServiceAccount,
                                        //        Request = ServiceAccountRequestForSensitivityLabel,
                                        //    };
                                        //    log.Info("[SensitivityLabel]Has service account, we will use service account request to restore file.");
                                        //    result = aveDoc.RestoreSelf(data, userData, dataJunction, webParts, sensitivityLabelRestoreOption);
                                        //}
                                        //else if (dto.RestoreOption.mAveItemRestoreOption.DELETE_ITEM)
                                        //{
                                        //    //aveItemDto.RestoreOption.mAveItemRestoreOption.DELETE_ITEM：true说明勾选了current version进行了还原
                                        //    //则current version不需要进行解密SensitivityLabel，直接还原即可把SensitivityLabel添加
                                        //    //对于version 则进行解密，否则还原会失败
                                        //    if ((data.ContainsKey("IsCurrentVersion") && (bool)data["IsCurrentVersion"]) || userData.ContainsKey("#tp_IsCurrent") && (bool)userData["#tp_IsCurrent"])
                                        //    {
                                        //        log.Info("[SensitivityLabel]current version RestoreSelf.");
                                        //        result = aveDoc.RestoreSelf(data, userData, dataJunction, webParts);
                                        //    }
                                        //    else if (AppProfileRequestForSensitivityLabel != null)
                                        //    {
                                        //        SensitivityLabelRestoreOption sensitivityLabelRestoreOption = new SensitivityLabelRestoreOption()
                                        //        {
                                        //            method = SensitivityLabelRestoreMethod.AppProfile,
                                        //            Request = AppProfileRequestForSensitivityLabel,
                                        //        };
                                        //        log.Info("[SensitivityLabel]Has AppProfile, we will use app request to restore file.");
                                        //        result = aveDoc.RestoreSelf(data, userData, dataJunction, webParts, sensitivityLabelRestoreOption);
                                        //    }
                                        //    else
                                        //    {
                                        //        result = aveDoc.RestoreSelf(data, userData, dataJunction, webParts);
                                        //    }
                                        //}
                                        //else
                                        //{
                                        //    result = aveDoc.RestoreSelf(data, userData, dataJunction, webParts);
                                        //}
                                    }
                                    else
                                    {
                                        //result = aveDoc.RestoreSelf(docData, userData, dataJunction, webParts);



                                        SPFile file = null;
                                        using (new AvePerformanceScope("HSWorker.GenerateFileNode"))
                                        {
                                            file = _manifestProcessor.GenerateSPFile(aveDoc.Name, key, mAveFolder.SPFolder.UniqueId.ToString(), docData, userData, isVersion);
                                            //file = GenerateFileNode(aveDoc, docData, userData, dataJunction, isVersion);
                                            restoreFileInfo.id = file.Id;
                                        }

                                        SPListItem item = null;
                                        List<DictionaryEntry> mmsProperties = new List<DictionaryEntry>();
                                        using (new AvePerformanceScope("HSWorker.GenerateItemNode"))
                                        {
                                            item = _manifestProcessor.GenerateSPListItem(docData, userData, dataJunction, ListItemDocType.File, isVersion, true);
                                            _manifestProcessor.CopySPListItem(item, file);
                                        }

                                        using (new AvePerformanceScope("HSWorker.GenerateItemNode"))
                                        {
                                            var spFieldCollection = _manifestProcessor.GenerateSPFieldCollection(mAveList, item, docData, userData, dataJunction, ListItemDocType.File, mmsProperties, isVersion, true);
                                            item.Items.Add(spFieldCollection);
                                        }

                                        //if (file.Properties != null && mmsProperties.Count > 0)
                                        //{
                                        //    foreach (DictionaryEntry pro in mmsProperties)
                                        //    {
                                        //        //if (!CheckKeyExist(file.Properties, pro.Name))
                                        //        //{
                                        //        //    file.Properties.Add(pro);
                                        //        //}
                                        //    }
                                        //}
                                        SPGenericObject SPFileObject;
                                        using (new AvePerformanceScope("HSWorker.ProcessFileObjectNode"))
                                        {
                                            SPFileObject = _manifestProcessor.ProcessFileObjectNode(docData, userData, dataJunction, isVersion, file);
                                        }
                                        SPGenericObject SPObject;
                                        using (new AvePerformanceScope("HSWorker.ProcessListItemNode"))
                                        {
                                            SPObject = _manifestProcessor.ProcessListItemNode(docData, userData, dataJunction, isVersion, item);
                                        }

                                        if (!isVersion)
                                        {
                                            _manifestProcessor.SPObjectCollection.SPObject.Add(SPFileObject);
                                            _manifestProcessor.SPObjectCollection.SPObject.Add(SPObject);

                                            //ProcessAlertXml(reportDtos, reportDto, aveDoc);
                                        }


                                        _manifestProcessor.Increase(restoreStream.ContentLength, !isVersion);

                                    }

                                    if (result == AveRestoreResult.Normal)
                                    {
                                        log.Info("IsRemoveTheStubAfterRestore and result is Normal, so RemoveArchiveStub.");
                                        RemoveArchiveStub(aveDoc, aveDocNameOriginal, dto.UniqueId.ToString(), dto.StubType, restoreFileInfo);
                                        restoreFileInfo.NeedDeleteStub = restoreFileInfo.rowid != 0 && !string.IsNullOrEmpty(restoreFileInfo.StubPath);
                                        restoreFileInfo.NeedDeclareRecord = userData.ContainsKey("_vti_ItemDeclaredRecord");
                                        restoreFileInfo.NeedDeleteOriStub = restoreFileInfo.OriStubRowId != 0 && !string.IsNullOrEmpty(restoreFileInfo.OriStubPath) && restoreFileInfo.OriParentListId != Guid.Empty;
                                    }
                                    else
                                    {
                                        log.Info($"IsRemoveTheStubAfterRestore and result is :{result.ToString()}, so skip RemoveArchiveStub.");
                                    }

                                }
                            }
                            catch (AveWarningException e)
                            {
                                throw new AvePoint.Wrapper.Common.SkipException(e.Message);
                            }
                            finally
                            {
                                if (result == AveRestoreResult.SkipRecycleBinData)
                                {
                                    throw new AvePoint.Wrapper.Common.SkipException("This item conflicts with recycle bin and conflict resolution is skip.");
                                }
                                if (aveDoc.ConflictWithDocument.HasValue && result != AveRestoreResult.SkipTheSameItem)
                                {
                                    NullableBooleanExtension.SetIfValueNotExist(ref isDocumentExist, aveDoc.ConflictWithDocument.Value);
                                }
                                if (this.aveFolder.RestoringItem.NeedSkipped && !GlobalRestoreOptionWorker.GlobalRestoreOption.ContentSetting.CheckRestoreSecurityOnly())
                                {
                                    // reset for new upcoming restore file
                                    if (!isVersion)
                                    {
                                        aveFolder.RestoringItem.NeedSkipped = false;
                                    }
                                    throw new AvePoint.Wrapper.Common.SkipException(this.aveFolder.RestoringItem.NeedSkippedKey, this.aveFolder.RestoringItem.NeedSkippedReason);
                                }
                            }
                            break;

                        case AveMetadataType.RoleAssignment:
                            //if (aveDoc.CheckRestoreOption(aveDoc.IsNewCreated, AveRestoreMode.RestoreSecurity) ||
                            //    GlobalRestoreOptionWorker.GlobalRestoreOption.ContentSetting.CheckRestoreSecurityOnly())
                            //{
                            if (!isVersion)
                            {
                                if (string.IsNullOrEmpty(OopStubUrl))
                                {
                                    log.Info("Begin restore DocumentLevel RoleAssignment.");
                                    var roleAssignments = metadata.GetMetadata<List<AveRoleAssignmentInfo>>();

                                    ProcessRoleAssignments(roleAssignments);

                                    //AveObjectSecurity security = AveObjectSecurity.CreateInstance(aveDoc.AveSPItem);
                                    //security.SourceHasUniqueRoleAssignment = aveDoc.AveSPItem.HasUniqueRoleAssignments;
                                    //security.RestoreRoleAssignments(roleAssignments, securityRestoreOption);
                                    //using (var report = security.GetReport())
                                    //{
                                    //    AddReport(AveRestoreReportDto.Parse(report.GetDetails(), dto));
                                    //}
                                }
                            }
                            //}
                            break;

                        //case AveMetadataType.DocImmedSubscriptions:
                        //    if (aveDoc.CheckRestoreOption(aveDoc.IsNewCreated, AveRestoreMode.OverWrite))
                        //    {
                        //        var iAlertInfos = metadata.GetMetadata<List<Dictionary<string, object>>>();
                        //        AveSPAlert alert = new AveSPDocAlert(aveDoc);
                        //        foreach (var iAlertInfo in iAlertInfos)
                        //        {
                        //            alert.RestoreAlert(iAlertInfo, false);
                        //        }
                        //        using (var report = alert.GetReport())
                        //        {
                        //            AddReport(AveRestoreReportDto.Parse(report.GetDetails(), dto));
                        //        }
                        //    }
                        //    break;

                        //case AveMetadataType.DocSchedSubscriptions:
                        //    if (aveDoc.CheckRestoreOption(aveDoc.IsNewCreated, AveRestoreMode.OverWrite))
                        //    {
                        //        var sAlertInfos = metadata.GetMetadata<List<Dictionary<string, object>>>();
                        //        AveSPAlert alert = new AveSPDocAlert(aveDoc);
                        //        foreach (var sAlertInfo in sAlertInfos)
                        //        {
                        //            alert.RestoreAlert(sAlertInfo, true);
                        //        }
                        //        using (var report = alert.GetReport())
                        //        {
                        //            AddReport(AveRestoreReportDto.Parse(report.GetDetails(), dto));
                        //        }
                        //    }
                        //    break;

                        //case AveMetadataType.WorkflowInstance:
                        //    if (aveDoc.CheckRestoreOption(aveDoc.IsNewCreated, AveRestoreMode.OverWrite))
                        //    {
                        //        var wfInfo = metadata.GetMetadata<List<AveWorkflowInfo>>();
                        //        WFConflictResolution wfResolution = WFConflictResolution.Instance;
                        //        foreach (var unit in wfInfo)
                        //        {
                        //            var wfAssociationUnit = SPWFInstanceUnit.Load(unit.AssociationUnit);
                        //            wfResolution.HandleInstanceConflict(wfAssociationUnit, aveDoc.AveSPItem.SPListItem);
                        //        }
                        //        using (var report = wfResolution.GetReport())
                        //        {
                        //            AddReport(AveRestoreReportDto.Parse(report.GetDetails(), dto));
                        //        }
                        //    }
                        //    break;
                        default:
                            break;
                    }
                }
                return restoreMode;
            }
            //aveDoc.DealSolution();
        }
        private void ConvertDocDataAndUserData(Dictionary<string, object> docData, Dictionary<string, object> userData)
        {
            if (docData == null || userData == null)
            {
                return;
            }

            TryAddValue("TimeCreated", "Created");
            TryAddValue("TimeLastModified", "Modified");
            TryAddValue("DoclibRowId", "#tp_ID");
            TryAddValue("UIVersion", "#tp_UIVersionString", ConvertUiVersionToString);
            TryAddValue("UIVersion", "#tp_UIVersion");
            void TryAddValue(string docKey, string userKey, Func<object, object> converter = null)
            {
                if (userData.ContainsKey(userKey))
                {
                    return;
                }

                if (!docData.TryGetValue(docKey, out var docValue) || docValue == null)
                {
                    return;
                }

                var valueToSet = converter == null ? docValue : converter(docValue);
                if (valueToSet != null)
                {
                    userData[userKey] = valueToSet;
                }
            }

            // SharePoint UI version uses a 512-based major/minor scheme.
            object ConvertUiVersionToString(object value)
            {
                if (!int.TryParse(value.ToString(), out var uiVersion))
                {
                    return null;
                }

                var major = uiVersion / 512;
                var minor = uiVersion % 512;
                return $"{major}.{minor}";
            }
        }
        protected bool ProcessContentData(IAveRestoreStream restoreStream, string key, Dictionary<string, object> docData, Dictionary<string, object> userData, bool isVersion, bool isAttachment = false)
        {
            // Generate temp file path and get file name (e.g., 1.dat, 2.dat)
            string datFilePath = _manifestProcessor.GenerateContentPath(key);
            string fileName = Path.GetFileName(datFilePath);

            try
            {
                // Step 1: Save stream to temp file
                if (!SaveStream(datFilePath, restoreStream))
                {
                    return false;
                }

                // Step 2: Upload file immediately to FreeContainer
                UploadFileToFreeContainer(datFilePath, fileName);

                // Step 3: Delete temp file to free disk space
                if (File.Exists(datFilePath))
                {
                    File.Delete(datFilePath);
                    mLog.Debug("Deleted temp file after upload: {0}", datFilePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                mLog.Error("Failed to process content data for key: {0}. Exception: {1}", key, ex.ToString());

                // Clean up temp file on error
                if (File.Exists(datFilePath))
                {
                    try { File.Delete(datFilePath); } catch { }
                }

                return false;
            }

            bool SaveStream(string filePath, IAveRestoreStream mReceiver)
            {
                using (new AvePerformanceScope("HSWorker.SaveStream"))
                {
                    try
                    {
                        if (File.Exists(filePath))
                        {
                            File.Delete(filePath);
                        }

                        bool isDamagedFile = false;
                        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            long length = mReceiver.ContentLength;
                            while (length > 0)
                            {
                                byte[] buffer = new byte[Int16.MaxValue * 2];
                                int readCount = mReceiver.ReadContent(buffer, 0, buffer.Length);
                                if (readCount <= 0)
                                {
                                    isDamagedFile = true;
                                    break;
                                }
                                fileStream.Write(buffer, 0, readCount);
                                length -= readCount;
                            }
                            fileStream.Flush();
                        }

                        if (isDamagedFile)
                        {
                            mLog.Debug("This is damaged file. Don't restore this file.");
                            if (File.Exists(filePath))
                            {
                                File.Delete(filePath);
                            }
                            return false;
                        }
                        return true;
                    }
                    catch (Exception e)
                    {
                        mLog.Error("An error occurred while saving stream. Exception: {0}.", e.ToString());
                        throw e;
                    }
                }
            }

            void UploadFileToFreeContainer(string filePath, string fileName)
            {
                using (new AvePerformanceScope("HSWorker.UploadFileToFreeContainer"))
                {
                    try
                    {
                        // Get FreeContainer parameters from manifest processor
                        var fcParams = _manifestProcessor.GetFreeContainerParameters();

                        // Create new FreeContainerManager instance for upload
                        var fcManager = new FreeContainerManager(fcParams);

                        // Create blob container client
                        var containerClient = new Azure.Storage.Blobs.BlobContainerClient(new Uri(fcParams.DataContainerUri));

                        // Upload the file (handles encryption and hash calculation)
                        FileInfo fileInfo = new FileInfo(filePath);
                        fcManager.UploadFile(fileInfo, containerClient, true, _manifestProcessor.UploadFileHashDic);

                        mLog.Debug("Successfully uploaded file to FreeContainer: {0}, size={1} bytes", fileName, fileInfo.Length);
                    }
                    catch (Exception e)
                    {
                        mLog.Error("Failed to upload file to FreeContainer: {0}. Exception: {1}", fileName, e.ToString());
                        throw;
                    }
                }
            }
        }
        protected void ProcessRoleAssignments(List<AveRoleAssignmentInfo> roleAssignmentsInfo)
        {
            if (roleAssignmentsInfo == null || roleAssignmentsInfo.Count == 0)
            {
                return;
            }

            var roleAssignment = _manifestProcessor.GenerateDeploymentRoleAssignments();

            try
            {
                //Group by principal id
                var groupedRoleAssignmentsInfo = roleAssignmentsInfo.Where(w => w.RoleId != 1073741833 && w.RoleId != AveConstants.LIMIT_ACCESS_ROLE_ID).GroupBy(i => i.PrincipalId);

                foreach (var roleAssignmentCollection in groupedRoleAssignmentsInfo)
                {
                    var newPrincipal = ParentAveSite.SPMembers.FindMember(roleAssignmentCollection.Key, true, false);

                    if (newPrincipal == null)
                    {
                        ////[LS]SharedLink group，PostAction
                        //AveGroupInfo groupInfo = ParentAveSite.SPMembers.UserAndDomainMapping.GetUserMapping(roleAssignmentCollection.Key) as AveGroupInfo;
                        //if (groupInfo != null && groupInfo.IsShareLink)
                        //{
                        //    if (WrapperConfiguration.RestoreSharingLink && this.mAveList != null)
                        //    {
                        //        if (!ShareLinkCache.Contains(groupInfo.ID))
                        //        {
                        //            ShareLinkCache.Add(groupInfo.ID);
                        //            var links = this.ParentAveWeb.RestoreSharingLink(groupInfo, ParentAveWeb.ServerRelativeUrl, mAveList.AveList, new Guid(objectId), -1, objectIdentity, true);
                        //            if (links != null && links.Count > 0)
                        //            {
                        //                sharedLinks.AddRange(links);
                        //            }
                        //        }
                        //        else
                        //        {
                        //            mLog.Info($"Skipped the duplicate shared link. PID:{groupInfo.ID}");
                        //        }
                        //    }
                        //    continue;
                        //}
                        //else
                        //{

                        //    //if (!ParentAveSite.SPMembers.SkippedUserCache.Contains(roleAssignmentCollection.Key))
                        //    //{
                        //    //    mLog.Log(AveLogLevel.WARN, string.Format("Cannot find one user/group with principal id. PrincipalId:{0}", roleAssignmentCollection.Key));
                        //    //    if (string.IsNullOrEmpty(ParentAveWeb.ParentSite.DefaultUser))
                        //    //    {
                        //    //        //ParentAveWeb.ParentSite.SPMembers.AddSecurityReport(roleAssignmentCollection.Key, AveConvert.ParseStringCol(roleAssignmentCollection.Select(i => i.RoleName)), AveStatus.Failed, item.ObjectIdentity);
                        //    //    }
                        //    //    else
                        //    //    {
                        //    //        //ParentAveWeb.ParentSite.SPMembers.AddSecurityReport(roleAssignmentCollection.Key, AveConvert.ParseStringCol(roleAssignmentCollection.Select(i => i.RoleName)), AveStatus.Skipped, item.ObjectIdentity);
                        //    //    }
                        //    //}
                        //    //else
                        //    //{
                        //        mLog.Log(AveLogLevel.INFO, string.Format("Skip user/group with principal id. PrincipalId:{0}", roleAssignmentCollection.Key));
                        //    //}
                        //    continue;
                        //}
                    }
                    else
                    {
                        int newPrincipalId = newPrincipal.ID;
                 
                        foreach (var roleAssignmentInfo in roleAssignmentCollection)
                        {
                            var status = AveStatus.Successful;
                            var reportMessage = string.Empty;

                            try
                            {
                                IAveRoleDefinition spRoleDefinition = ParentAveWeb.Security.GetRoleWithCache(roleAssignmentInfo.RoleId, ParentAveWeb);
                                if (spRoleDefinition == null)
                                {
                                    try
                                    {
                                        if (!string.IsNullOrEmpty(roleAssignmentInfo.RoleName))
                                        {
                                            roleAssignmentInfo.RoleName = ParentAveSite.GetNameByLanguageMapping(roleAssignmentInfo.RoleName, AveLanguageMappingType.PermissionMapping);
                                            spRoleDefinition = ParentAveWeb.SPWeb.RoleDefinitions[roleAssignmentInfo.RoleName];
                                        }
                                        else
                                        {
                                            spRoleDefinition = ParentAveWeb.SPWeb.RoleDefinitions.GetById(roleAssignmentInfo.RoleId);
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        //if (GlobalPreferenceSettings.EnableRestorePermissionLevelAsItem)
                                        //{
                                        //    var roleInfo = ParentAveWeb.GetRoleByName(roleAssignmentInfo.RoleName);
                                        //    if (roleInfo != null)
                                        //    {
                                        //        var newId = ParentAveWeb.Security.RestoreRole(roleInfo, ParentAveWeb, false);
                                        //        if (newId > 0)
                                        //        {
                                        //            ParentAveWeb.ParentSite.MappingManager.WebMappingManager.AddRoleDefinitionsCache(roleInfo.RoleId, newId);
                                        //            spRoleDefinition = ParentAveWeb.SPWeb.FirstUniqueRoleDefinitionWeb.RoleDefinitions[roleAssignmentInfo.RoleName];
                                        //        }
                                        //        else
                                        //        {
                                        //            mLog.Log(AveLogLevel.WARN, "Restore role failed when restore role assignment. RoleId:{0}, PrincipalId:{1}.", roleAssignmentInfo.RoleId, roleAssignmentInfo.PrincipalId);
                                        //            reportMessage = $"Cannot find [{roleAssignmentInfo.RoleName}] permission level in the target site.";
                                        //            status = AveStatus.Failed;
                                        //            continue;
                                        //        }
                                        //    }
                                        //    else
                                        //    {
                                        //        mLog.Log(AveLogLevel.WARN, "Cannot find role by role name. RoleId:{0}, PrincipalId:{1}.", roleAssignmentInfo.RoleId, roleAssignmentInfo.PrincipalId);
                                        //        reportMessage = $"Cannot find [{roleAssignmentInfo.RoleName}] permission level in the target site.";
                                        //        status = AveStatus.Failed;
                                        //        continue;
                                        //    }
                                        //}
                                        //else
                                        //{
                                        mLog.Log(AveLogLevel.WARN, "Cannot find role in role definition cache. RoleId:{0}, PrincipalId:{1}.Error message:{2}", roleAssignmentInfo.RoleId, roleAssignmentInfo.PrincipalId, e);
                                        reportMessage = $"Cannot find [{roleAssignmentInfo.RoleName}] permission level in the target site.";
                                        status = AveStatus.Failed;
                                        continue;
                                        //}
                                    }
                                }

                                _manifestProcessor.GenerateDeploymentAssignment(roleAssignment, newPrincipal, spRoleDefinition);

                            }
                            catch (Exception ex)
                            {
                                reportMessage = ex.Message;
                                status = AveStatus.Failed;
                            }
                            finally
                            {
                                //ParentAveWeb.ParentSite.SPMembers.AddSecurityReport(roleAssignmentCollection.Key, roleAssignmentInfo.RoleName, status, item.ObjectIdentity, reportMessage, key);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"An error occurred while restoring security of item, ex:{e.ToString()}");
            }
        }

        protected bool ProcessListItemXML(RestoreContentDto dto)
        {
            return true;
        }
        protected bool ProcessAttachmentXML(RestoreContentDto dto)
        {
            return true;
        }

        bool IsWithinFormFolder(AveSPFolder folder)
        {
            if (folder == null || mAveListRootFolder == null)
            {
                return false;
            }

            var currentFolder = folder;
            while (currentFolder != null)
            {
                // Check if we've reached the list root folder - stop traversing
                if (string.Equals(currentFolder.ServerRelativeUrl, mAveListRootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }

                // Check parent folder exists before accessing it
                if (currentFolder.ParentFolder == null)
                {
                    return false;
                }

                // Check if parent is the list root folder
                if (string.Equals(currentFolder.ParentFolder.ServerRelativeUrl, mAveListRootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                {
                    // This folder is a direct child of the list root - check if it's a system folder
                    if (currentFolder.Name.Equals("Forms", StringComparison.OrdinalIgnoreCase)
                        || currentFolder.Name.Equals("_t", StringComparison.OrdinalIgnoreCase)
                        || currentFolder.Name.Equals("_w", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    // Direct child of list root but not a system folder
                    return false;
                }

                currentFolder = currentFolder.ParentFolder;
            }
            return false;
        }

        bool IsPagesOrSystemFile(string fileName)
        {
            try
            {
                if (mAveFolder != null)
                {
                    if (!mAveFolder.IsWithinFormFolder.HasValue)
                    {
                        mAveFolder.IsWithinFormFolder = IsWithinFormFolder(mAveFolder);
                    }

                    if (mAveFolder.IsWithinFormFolder.Value)
                    {
                        return true;
                    }
                    else
                    {
                        return fileName.EndsWith(".aspx", StringComparison.OrdinalIgnoreCase)
                      || fileName.EndsWith(".onetoc2", StringComparison.OrdinalIgnoreCase)
                      || fileName.EndsWith(".one", StringComparison.OrdinalIgnoreCase)
                      || fileName.Equals("client_LocationBasedDefaults.html", StringComparison.OrdinalIgnoreCase);
                    }
                }
                else
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"Failed to check file is system file. fileName:{fileName}, ex:{e}");
                return true;
            }
        }

        public virtual void ProcessItem(RestoreContentDto aveItemDto)
        {
            IAveRestoreStream restoreStream = base.RestoreStream;
            var skipAddToMigration = false;
            var baseRestore = false;
            ARMigrationRestoreFileInfo restoreFileInfo = new();

            if (IsRestoreToSPO)
            {
                var destFolderPath = string.IsNullOrEmpty(DestInfo.FolderPath) ? DestInfo.ListPath : DestInfo.FolderPath;
                if (!isSelectedFolderProcessed)
                {
                    SetSourceFolderUrl(aveItemDto, false);
                }

                if (string.IsNullOrEmpty(targetFolderUrl))
                {
                    targetFolderUrl = WebUtil.MakeFullUrl(targetSiteUrl, destFolderPath);
                }

                InitTargetParentFolders(destFolderPath);
            }
            string oldUrl = aveItemDto.SrcUrl;
            string srcPathAppendName = string.Empty;
            string appendName = string.Empty;

            if (oldUrl.Contains('\\'))
            {
                oldUrl = oldUrl.Replace('\\', '/');
            }
            if (!string.IsNullOrEmpty(targetSiteUrl))
            {
                aveItemDto = ConvertRestoreContentDtoForArchiverOOPRestore(aveItemDto);
            }
            var reportDto = new AveRestoreReportDto 
            { 
                Type = aveItemDto.Type.ToString(),
                Title = aveItemDto.Name,
                StartTime = DateTime.UtcNow.Ticks,
                PathMD5 = aveItemDto.ItemPathMd5,
                DestinationUrl = string.Empty
            };//Path = aveItemDto.Name

            if (AveList != null && AveList.NeedContinue == false)
            {
                //List Skipped,we should not add item\folder under the list to report.
                return;
            }
            if (this.aveFolder == null)
            {
                if (aveItemDto.IsAppData)
                {
                    return;
                }
                if (aveItemDto.IsSelected)
                {
                    reportDto.Status = RestoreStatus.ContainerFailed;
                }
                else
                {
                    reportDto.Status = RestoreStatus.Skipped;
                }

                reportDto.SourcePath = aveItemDto.SrcUrl;
                
                //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(RestoreReportKey.Item_CanNotFindItemParent.ToString(), RestoreReportResource.Item_CanNotFindItemParent, aveItemDto.Name);
                AddReport(reportDto);
                return;
            }
            if (aveItemDto.IsFailed)
            {
                reportDto.SourcePath = aveItemDto.SrcUrl;
                reportDto.Status = RestoreStatus.Skipped;
                //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(RestoreReportKey.Item_SkipBackupFailedItem.ToString(), RestoreReportResource.Item_SkipBackupFailedItem, aveItemDto.Name);
                AddReport(reportDto);
                return;
            }
            if ((this.aveFolder.SPFolder == null && GlobalRestoreOptionWorker.GlobalRestoreOption.ContainerSetting == ContainerSetting.SecurityOnlyMerge)
                || (this.aveFolder.SPFolder == null && GlobalRestoreOptionWorker.GlobalRestoreOption.ContainerSetting == ContainerSetting.SecurityOnlyOverWrite))
            {
                reportDto.SourcePath = aveItemDto.SrcUrl;
                reportDto.Status = RestoreStatus.Skipped;
                //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(RestoreReportKey.Item_GlobalRestoreOptionWorkerSkip.ToString(), RestoreReportResource.Item_GlobalRestoreOptionWorkerSkip, aveItemDto.Name);
                AddReport(reportDto);
                return;
            }
            if (this.aveFolder.SPFolder == null || !aveFolder.SPFolder.Exists)
            {
                this.aveFolder.InitSPFolder(true);
            }
            this.aveFolder.RestoringItem.IsIncludingRecycleBinData = (Config).IncludingRecycleBinData;
            string realName = aveItemDto.Name;
            int index = realName.IndexOf(':');
            bool isVersion = false;
            if (index >= 0)
            {
                realName = realName.Substring(0, index);
                isVersion = true;
                reportDto.Version = aveItemDto.Name.Substring(index + 1);
            }
            //用于Report.Option
            bool? isItemExistInDestination = null;
            //SAAS-44975，特定用户SP中有文件，选择version还原skip掉
            //暂不支持这种case，如果支持再放开
            //RECO-21172 不带着current version使用skip或者overrite方式还原文件直接跳过
            if (aveItemDto.RestoreOption.mAveRestoreMode == AveRestoreMode.Default || aveItemDto.RestoreOption.mAveRestoreMode == AveRestoreMode.OverWrite)
            {
                //不做任何操作
                log.Info($"Skip restoring files using skip or overwrite without current version.");
            }
            else
            {
                ResetRestoreModeForArchiver(aveItemDto, isVersion);
            }
            AveRestoreMode restoreMode = aveItemDto.RestoreOption.mAveRestoreMode;
            try
            {
                using (AvePerformanceScope pcAll = new AvePerformanceScope("GranularRestore.RestoreItem"))
                {
                    switch (aveItemDto.Type)
                    {
                        case AveConstants.TYPE_DOCUMENT:
                        case AveConstants.TYPE_VERSION:
                            using (AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.RestoreItem.Document"))
                            {
                                if (!CheckSODataNeedRestore(restoreStream))
                                {
                                    throw new AvePoint.Wrapper.Common.SkipException("Looks up a localized string similar to This is a Storage Manager/Connector stub.If you want to restore it,please modify configration file..");
                                }
                                var aveDoc = new AveSPDoc(this.aveFolder, aveItemDto.Name);
                                aveDoc.SetStream(restoreStream);
                                aveDoc.SetRestoreOption(aveItemDto.RestoreOption);
                                var mode = ResetDocNameIfNeedAppend(aveDoc, realName, ref isItemExistInDestination, restoreStream);
                                aveDoc.RestoreOption.mAveRestoreMode = mode;
                                try
                                {
                                    if (mode == AveRestoreMode.Append && Item.Restore.AppendItemMapping.ContainsKeyAppendName(realName))
                                    {
                                        log.Info($"Processing appendName for document");
                                        appendName = Item.Restore.AppendItemMapping.GetValueAppendName(realName);
                                        log.Info($"Get append name: {appendName}");
                                        string dest = aveItemDto.SrcUrl;
                                        log.Info($"DestinationURl: {dest}");
                                        dest = dest.Replace('\\', '/');
                                        int lastSeparatorIndex = dest.LastIndexOf('/');
                                        string renamedPath = lastSeparatorIndex >= 0
                                                            ? dest[..(lastSeparatorIndex + 1)] + appendName
                                                            : appendName;
                                        srcPathAppendName = renamedPath;
                                        log.Info($"SrcPathWithAppendName: {srcPathAppendName}");
                                    }
                                }
                                catch (Exception e)
                                {
                                    mLog.Error($"Failed to reset doc name if need append. Id: {aveItemDto.Id}, Name: {aveItemDto.SrcUrl}, ex: {e}");
                                }
                                mLog.Info($"Start restore document. Id: {aveItemDto.Id}");
                                if (mAveList == null || (mAveList != null && (mAveList.IsSystemList || mAveList.IsSpecialList)) || IsPagesOrSystemFile(realName))
                                {
                                    mLog.Info($"Start the base restore document. Id: {aveItemDto.Id}");
                                    baseRestore = true;
                                    restoreMode = base.RestoreDocument(aveDoc, aveItemDto, reportDto, ref isItemExistInDestination, restoreStream);
                                }
                                else
                                {
                                    mLog.Info($"Start the migration restore document. Id: {aveItemDto.Id}");
                                    restoreMode = ProcessDocumentXML(aveDoc, aveItemDto, reportDto, isVersion, ref isItemExistInDestination, restoreStream, restoreFileInfo);
                                }
                                reportDto.Size = restoreStream.ContentLength;

                                if (!isVersion)
                                {
                                    mLog.Info($"Add migration restore document. Id: {aveItemDto.Id}, restoreFileInfoId: {restoreFileInfo.id}");
                                    restoreFileInfo.name = aveItemDto.Name;
                                    restoreFileInfo.serverRelativeUrl = aveDoc.ParentFolder.ServerRelativeUrl + "/" + aveDoc.Name;
                                    restoreFileInfo.size = restoreStream.ContentLength;
                                    restoreFileInfo.Type = aveItemDto.Type;
                                    restoreFileInfo.ArchiveTime = aveItemDto.ArchiveTime;
                                    restoreFileInfo.StorageId = aveItemDto.StorageId;
                                    restoreFileInfo.RowKey = aveItemDto.Id;
                                    restoreFileInfo.BackUpJobId = aveItemDto.BackUpJobId;
                                }
                                else
                                {
                                    mLog.Info($"Add migration restore version document. Id: {aveItemDto.Id}, restoreFileInfoId: {restoreFileInfo.id}");
                                    skipAddToMigration = true;
                                    if (string.IsNullOrEmpty(restoreFileInfo.id) || string.Equals(restoreFileInfo.id, Guid.Empty.ToString()))
                                    {
                                        mLog.Error($"the sp file id of current item is invalid. id: {restoreFileInfo.id}, path: {aveItemDto.SrcUrl}");
                                    }
                                    else
                                    {
                                        var vList = mVersionReports.GetOrAdd(restoreFileInfo.id, _ => []);
                                        lock (vList)
                                        {
                                            vList.Add(new MigrationRestoreVersionDto()
                                            {
                                                FileUrl = aveItemDto.SrcUrl,
                                                Size = reportDto.Size,
                                                Version = reportDto.Version,

                                                // for statistics
                                                Md5 = aveItemDto.ItemPathMd5,
                                                Name = aveItemDto.Name,
                                                ArchiveTime = aveItemDto.ArchiveTime,
                                                BackUpJobId = aveItemDto.BackUpJobId,
                                                RowKey = aveItemDto.Id,
                                                StorageId = aveItemDto.StorageId,
                                                Type = aveItemDto.Type,
                                                StartTime = reportDto.StartTime
                                            });
                                        }
                                    }
                                }
                            }
                            break;

                        case AveConstants.TYPE_LISTITEM:
                        case AveConstants.TYPE_LISTITEMVERSION:
                            using (AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.RestoreItem.ListItem"))
                            {
                                string tempName = aveItemDto.Name;
                                //For folder version
                                if (tempName.StartsWith(":", StringComparison.Ordinal))
                                {
                                    var folderVersion = new AveSPFolder(this.aveFolder.ParentFolder, this.aveFolder.SPFolder.Name);
                                    this.aveFolder.SetRestoreOption(aveItemDto.RestoreOption);
                                    AveMetadata metadata;
                                    var data = new Dictionary<string, object>();
                                    while ((metadata = restoreStream.ReadMetadata()) != null)
                                    {
                                        switch (metadata.MetadataType)
                                        {
                                            case AveMetadataType.DocProperty:
                                                data.Clear();
                                                metadata.GetMetadata(data);
                                                AveMetadata userDataMetadata = restoreStream.TryReadMetadata(AveMetadataType.DocData);
                                                var userData = new Dictionary<string, object>();
                                                if (userDataMetadata != null)
                                                {
                                                    userDataMetadata.GetMetadata(userData);
                                                }

                                                AveMetadata dataJuntionMetadata = restoreStream.TryReadMetadata(AveMetadataType.DocDataJunction);
                                                List<Dictionary<string, object>> dataJunction = new List<Dictionary<string, object>>();
                                                if (dataJuntionMetadata != null)
                                                {
                                                    dataJuntionMetadata.GetMetadata(dataJunction);
                                                }
                                                folderVersion.RestoreSelf(data, userData, dataJunction);
                                                int folderId = data["DoclibRowId"] is DBNull ? -1 : (int)data["DoclibRowId"];
                                                reportDto.Path = ReportAbsolutePath.GetFolderVersionAP(AveSite.SiteUrl, AveSite.ServerRelativeUrl, aveFolder.SPFolder.ServerRelativeUrl, this.AveList.SPList.DefaultDisplayFormUrl, folderId, (int)data["UIVersion"]);
                                                break;
                                        }
                                    }
                                }
                                else
                                {
                                    var aveListItem = new AveSPListItem(this.aveFolder, aveItemDto.Name);
                                    aveListItem.SetRestoreOption(aveItemDto.RestoreOption);
                                    //aveListItem.RestoreOption.mAveRestoreMode = ResetListItemNameIfNeedAppend(aveListItem, realName, ref isItemExistInDestination, restoreStream);
                                    //#region Rename report.Name if appended
                                    //if (!string.Equals(realName, aveListItem.Name, StringComparison.OrdinalIgnoreCase))
                                    //{
                                    //    reportDto.Path = reportDto.Path.Replace(realName, aveListItem.Name);
                                    //}
                                    //#endregion
                                    //todo++restoreMode = RestoreListItem(aveListItem, aveItemDto, reportDto, ref isItemExistInDestination, restoreStream);
                                }
                                reportDto.Size = restoreStream.ContentLength > 0 ? restoreStream.ContentLength : 1024 * 1024;
                            }
                            break;

                        case AveConstants.TYPE_ATTACHMENTS:
                            if (this.AveList == null)
                            {
                                log.Error("AveList is null when restore item for attachments type");
                                throw new ArgumentNullException(nameof(this.AveList));
                            }
                            this.AveList.DisableListVersionSettings();
                            using (AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.RestoreItem.Attachment"))
                            {
                                if (GlobalRestoreOptionWorker.GlobalRestoreOption.ContentSetting.CheckRestoreSecurityOnly())
                                {
                                    log.Log(AveLogLevel.INFO, "Looks up a localized string similar to Attachment will skip while restore security only..");
                                    throw new Wrapper.Common.SkipException("Looks up a localized string similar to Attachment will skip while restore security only..");
                                }
                                if (!CheckSODataNeedRestore(restoreStream)) //this.aveFolder.RestoringItem.NeedSkipped ||
                                {
                                    //throw new SkipException(RestoreResource.Item_SkipRestoreStub);
                                    throw new Wrapper.Common.SkipException(this.aveFolder.RestoringItem.NeedSkippedReason);
                                }
                                string attachmentName = aveItemDto.Name.Substring(aveItemDto.Name.IndexOf(':') + 1);
                                if (Item.Restore.AppendItemMapping.ContainsKeyAppendName(realName))
                                {
                                    aveItemDto.Name = Item.Restore.AppendItemMapping.GetValueAppendName(realName) + ":" + attachmentName;
                                }
                                var aveAtta = new AveSPAttachment(this.AveList, aveItemDto.Name);
                                aveAtta.SetStream(restoreStream);
                                aveAtta.SetRestoreOption(aveItemDto.RestoreOption);
                                //NullableBooleanExtension.SetIfValueNotExist(ref isItemExistInDestination, aveAtta.IsAttachmentExists());
                                //todo++RestoreAttachment(aveAtta, restoreStream);
                                isItemExistInDestination = false;
                                reportDto.Path = ReportAbsolutePath.GetAttachmentAP(AveList.Url, aveAtta.AttachmentInfo.RowId, attachmentName);
                                reportDto.Size = restoreStream.ContentLength;
                            }
                            break;
                    }
                    //log.Info(RestoreResource.Item_AIRRestoreItem, aveItemDto.Name, aveItemDto.Type);
                }
            }
            catch (AveSecurityTrimingException ex)
            {
                log.Warn(@"An error occurred while restore item. {0}", aveItemDto.Name, ex);
                reportDto.Status = RestoreStatus.Skipped;
                reportDto.ErrorMessage = ex.Message;
            }
            catch (Wrapper.Common.SkipException e)
            {
                log.Warn(@"Looks up a localized string similar to This object was skipped.Name:{0} Reason:{1}.", aveItemDto.Name, e);
                reportDto.Path = null;
                reportDto.Status = RestoreStatus.Skipped;
                reportDto.ErrorMessage = e.Message;
            }
            catch (Exception e)
            {
                if (reportDto?.Path?.EndsWith("Forms/Document Set/docsethomepage.aspx") == true)
                {
                    mLog.Warn(@$"docsethomepage.aspx is system file,error:{e}");
                    reportDto.Status = RestoreStatus.Skipped;
                }
                else if (aveFolder.RestoringItem.NeedSkipped)
                {
                    mLog.Warn(@"Looks up a localized string similar to This object was skipped.Name:{0} Reason:{1}.", aveItemDto.Name, e);
                    reportDto.Status = RestoreStatus.Skipped;
                    reportDto.ErrorMessage = e.Message;
                }
                else
                {
                    mLog.Log(EventSources.DocAveAgentService, Config.EventCategory, new EventIds.SharePoint.RestoreItemFailedEventMessage(aveItemDto.Name, e));
                    reportDto.Status = RestoreStatus.Failed;
                    if (e.Message != null)
                    {
                        if (e.Message.Contains("The label that's applied to this item prevents it from being edited or deleted. Check the item's label for more details"))
                        {
                            reportDto.ErrorMessage = "StorageOptimization_SOARRecordManagerLabelDocumentDeleteFailed";
                        }
                        else if (e.Message.Contains("This item cannot be updated because it is locked as read-only"))
                        {
                            reportDto.ErrorMessage = "StorageOptimization13_SOARDeleteOfficeLockFile";
                        }
                        else
                        {
                            reportDto.ErrorMessage = e.Message;
                        }
                    }
                }
                reportDto.Path = null;
            }
            if (!(isItemExistInDestination.HasValue && isItemExistInDestination.Value))
            {
                Item.Restore.AppendItemMapping.AddToMappingAppendVersion(realName, true);
            }
            reportDto.SetOption(restoreMode, isItemExistInDestination, reportDto.Status);
            if (IsEnduserRestore && !string.IsNullOrEmpty(OopStubUrl))
            {
                string resultUrl = OopStubUrl.Substring(0, OopStubUrl.LastIndexOf('.'));
                reportDto.SourcePath = resultUrl;
                reportDto.Path = aveItemDto.OopSourceUrl.Replace("\\", "/");
            }
            else
            {
                log.Info($"AveMigrationRestore, setting value last time");
                reportDto.SourcePath = aveItemDto.SrcUrl;
                reportDto.Path = string.Empty;
                if (!string.IsNullOrEmpty(srcPathAppendName))
                {
                    log.Info($"AveMigrationRestore, srcPathName is true");
                    reportDto.DestinationUrl = srcPathAppendName;
                    log.Info($"AveMigrationRestore, DestinationUrl: {reportDto.DestinationUrl}");
                    if (!string.IsNullOrEmpty(oldUrl))
                    {
                        reportDto.SourcePath = oldUrl;
                    }
                }
                else
                {
                    log.Info($"AveMigrationRestore, other option case");
                    string replacePathh = reportDto.SourcePath.Replace("\\", "/");
                    reportDto.DestinationUrl = replacePathh;
                    log.Info($"AveMigrationRestore, DestinationUrl: {reportDto.DestinationUrl}");
                    if (IsRestoreToSPO)
                    {
                        reportDto.SourcePath = oldUrl;
                    }
                }
            }

            if (reportDto.Status == RestoreStatus.Success && !baseRestore)
            {
                if (skipAddToMigration) return;
                if (string.IsNullOrEmpty(restoreFileInfo.id) || string.Equals(restoreFileInfo.id, Guid.Empty.ToString()))
                {
                    mLog.Error($"the sp file id of current item is invalid. id: {aveItemDto.Id}");
                    return;
                }

                mLog.Info($"Start add to migration report for document. id: {aveItemDto.Id}");

                if (mAllJobStatus[mAveList.Id].ContainsReport(restoreFileInfo.id))
                {
                    mAllJobStatus[mAveList.Id].AddOrUpdateVersionReport(restoreFileInfo.id, mVersionReports.GetOrAdd(restoreFileInfo.id, _ => []));
                }
                else
                {
                    MigrationRestoreFileDto migrationReportDto = new()
                    {
                        FileUrl = aveItemDto.SrcUrl,
                        Md5 = aveItemDto.ItemPathMd5,
                        NodeType = aveItemDto.Type.ToString(),
                        Size = reportDto.Size,
                        StartTime = reportDto.StartTime,
                        VersionsReportDtos = mVersionReports.GetOrAdd(restoreFileInfo.id, _ => []),
                    };

                    if (string.IsNullOrEmpty(restoreFileInfo.id) || string.Equals(restoreFileInfo.id, Guid.Empty.ToString()))
                    {
                        mLog.Warn($"the sp file id of current item is invalid. id: {restoreFileInfo.id}, path: {migrationReportDto.FileUrl}");
                        restoreFileInfo.id = Guid.NewGuid().ToString();
                    }

                    mAllJobStatus[mAveList.Id].AddReports(restoreFileInfo.id, migrationReportDto);
                    mVersionReports.TryRemove(restoreFileInfo.id, out _);
                }

                lock (padlock)
                {
                    mCurrentPackageList.Add(restoreFileInfo);
                }

                ProcessPackage();
                return;
            }

            CheckFileTail(reportDto, restoreStream);
            if (!AveList.IsSystemList)
            {
                AddReport(reportDto);
                if (reportDto.Status == (int)AvePoint.RA.Contract.RMWeb.JobMonitor.JobDetailsStatus.Successful)
                {
                    if (aveItemDto.Type == AveConstants.TYPE_DOCUMENT && aveItemDto.Name != null && !aveItemDto.Name.Contains(":"))
                    {
                        SOArchiverJobInfoStatistics.Instance.FileCurrentVersionCount++;
                    }
                    else if (aveItemDto.Type == AveConstants.TYPE_VERSION ||
                        (aveItemDto?.Name != null && aveItemDto.Name.Contains(":") && aveItemDto.Type == AveConstants.TYPE_DOCUMENT))
                    {
                        SOArchiverJobInfoStatistics.Instance.FileHisVersionCount++;
                    }

                    if (aveItemDto.Type == AveConstants.TYPE_LISTITEM || aveItemDto.Type == AveConstants.TYPE_LISTITEMVERSION
                        || aveItemDto.Type == AveConstants.TYPE_DOCUMENT || aveItemDto.Type == AveConstants.TYPE_VERSION)
                    {
                        SOArchiverJobInfoStatistics.Instance.ItemAndVersionCountFotTelemetry++;
                        SOArchiverJobInfoStatistics.Instance.ItemAndVersionExpireSumTime += SOArchiverJobInfoStatistics.Instance.MainJobStartTime - aveItemDto.ArchiveTime;
                    }
                    if (aveItemDto.Type == AveConstants.TYPE_LISTITEM || aveItemDto.Type == AveConstants.TYPE_LISTITEMVERSION)
                    {
                        SOArchiverJobInfoStatistics.Instance.ItemSizeSumForTelemetry += ContractConstants.ITEMSIZEFORLICENSE;
                        SOArchiverJobInfoStatistics.Instance.ItemCountForTelemetry++;
                        SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(ContractConstants.ITEMSIZEFORLICENSE, aveItemDto.SrcUrl);
                    }
                    else
                    {
                        if (aveItemDto.Type != AveConstants.TYPE_ATTACHMENTS)
                        {
                            RecordRestoredFile.InsertIntoTable(aveItemDto.StorageId, aveItemDto.Id, aveItemDto.ItemPathMd5, aveItemDto.BackUpJobId, reportDto.SourcePath);
                        }
                        SOArchiverJobInfoStatistics.Instance.ItemSizeSumForTelemetry += reportDto.Size;
                        SOArchiverJobInfoStatistics.Instance.ItemCountForTelemetry++;
                        SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(reportDto.Size, aveItemDto.SrcUrl);
                    }
                }
            }
        }
        #endregion
       

        private void ProcessListRootFolderXML()
        {
            //var listSettingInfo = AvailableResource.MetadataGenerator.GetData<AveListSettingInfo>(AveMetadataType.ListProperty.ToString());

            var mParentSPFolderObject = new SPGenericObject()
            {
                Id = mAveFolder.Id.ToString(),
                ObjectType = SPObjectType.SPFolder,
                ParentId = mAveFolder.SPFolder.ParentFolder.UniqueId.ToString(),
                ParentWebId = ParentAveWeb.SPWeb.ID.ToString(),
                ParentWebUrl = ParentAveWeb.SPWeb.ServerRelativeUrl,
                Url = mAveFolder.ServerRelativeUrl,
            };
            var spFolder = new SPFolder()
            {
                Id = mAveFolder.Id.ToString(),
                Url = mAveFolder.SPFolder.Url,
                Name = mAveFolder.SPFolder.Name,
                ParentFolderId = mAveFolder.SPFolder.ParentFolder.UniqueId.ToString(),
                ParentWebId = ParentAveWeb.SPWeb.ID.ToString(),
                ParentWebUrl = ParentAveWeb.ServerRelativeUrl,
                ContainingDocumentLibrary = mAveList.SPList.ID.ToString(),
            };
            if (listSettingInfo == null)
            {
                spFolder.TimeCreated = DateTime.Now;
                spFolder.TimeLastModified = DateTime.Now;
            }
            else
            {
                spFolder.TimeCreated = listSettingInfo.Created == null ? DateTime.Now : listSettingInfo.Created.Value;
                spFolder.TimeLastModified = listSettingInfo.LastModifiedTime == null ? DateTime.Now : listSettingInfo.LastModifiedTime.Value;
            }
            mParentSPFolderObject.Item = spFolder;
            mCacheSPFolderObjects[mAveList.SPList.ID] = new ConcurrentDictionary<Guid, SPGenericObject>();
            IAveFolder targetFolder = mAveList.SPList.GetFolder(mAveFolder.ServerRelativeUrl);
            if (targetFolder == null || !targetFolder.Exists)
            {
               _manifestProcessor.SPObjectCollection.SPObject.Add(mParentSPFolderObject);
            }
            mCacheSPFolderObjects[mAveList.SPList.ID].TryAdd(mParentSPFolderObject.Url.ToHashGuid(), mParentSPFolderObject);
            if (string.Equals(mAveList.RootFolder.ServerRelativeUrl, mAveFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                ParentSPFolderCache.TryAdd(mParentSPFolderObject.Url.ToHashGuid(), Guid.Parse(mParentSPFolderObject.Id));
            }
            else
            {
                ParentSPFolderCache.TryAdd(mAveFolder.ParentFolder.ServerRelativeUrl.ToHashGuid(), mAveFolder.ParentFolder.Id);
                ParentSPFolderCache.TryAdd(mAveFolder.ServerRelativeUrl.ToHashGuid(), mAveFolder.Id);
            }
            //ListFolderUrlMapping.TryAdd(mAveList.ListInfo.ServerRelativeUrl, mAveListRootFolder.ServerRelativeUrl);
 
            //if (!mAveList.IsNewCreated && mAveList.ListInfo != null)
            //{

            //    if (mAveList.ListInfo.BaseType == 1)
            //    {
            //        var tempCollection = mAveList.SPList.FileCollection;

            //    }
            //    else
            //    {
            //        var tempCollection = mAveList.SPList.TPGuidIDMapping;
            //    }
            //}
            //var tempFolder = mAveList.SPList.FoldersCollection;
        }

        private void InitListLevelObjects()
        {
   
            ImportJobResources importJobResources = new ImportJobResources();

            importJobResources.JobCount = 0;
            importJobResources.ListTitle = mAveList.SPList.Title;
            importJobResources.WebName = ParentAveWeb.Name;
            //importJobResources.listUrl = mAveList.Url;
            //importJobResources.listDefaultDisplayFormUrl = mAveList.SPList.DefaultDisplayFormUrl;
            //importJobResources.listTemplateType = mAveList.SPList.BaseTemplate;
            mAllJobStatus.TryAdd(mAveList.SPList.ID, importJobResources);


            var workPath = SecurityUtils.SafeCombinePath(BackgroundSettings.GetInstance().ArchiveTemp, TenantLocalValue.LogonGroupId, Config.SubJobId);
            _manifestProcessor = new ManifestPackageProcessor(ParentAveSite, ParentAveWeb, mAveList, workPath, Config);
            
            int aveListSqliteCacheTypes = (int)AveListCacheType.FileCollection;
            mAveList.InitSqliteCacheInfo(Config.SubJobId, aveListSqliteCacheTypes);
        }

        public override void ProcessForOpus()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.Process"))
            {
                try
                {
                    log.Info("Looks up a localized string similar to Begin restoring....");
                    Init();
                    Config.IsUsingMigrationImportJob = true;
                    InitTaskManager();
                    RestoreContentDto contentDto;
                    while ((contentDto = ContentReader.MoveNext()) != null)
                    {
                        log.Info(@"Looks up a localized string similar to The restore content is received from media. Name: {0}. SrcName: {1}. Type: {2}.", contentDto.UniqueId, contentDto?.SrcUrl, contentDto?.Type);
                        RestoreStream.Reset();
                        if (!contentDto.ReplaceType.Equals('\0'))
                        {
                            ReplaceType = contentDto.ReplaceType;
                        }
                        try
                        {
                            using (new CheckJobStopScope()) { }
                            CheckDtoType(contentDto.Type);
                            switch (contentDto.Type)
                            {
                                case AveConstants.TYPE_SITE:
                                    ProcessPackage(true);
                                    RestoreSite(contentDto);
                                    break;

                                case AveConstants.TYPE_WEB:
                                    ProcessPackage(true);
                                    RestoreWeb(contentDto);
                                    break;

                                case AveConstants.TYPE_PROJECT:
                                    log.Warn("Pwa data is not supported in DocAveOnline.");
                                    //RestoreProject(dto);
                                    break;
                                case AveConstants.TYPE_APP:
                                    ProcessPackage(true);
                                    RestoreApp(contentDto);
                                    break;

                                case AveConstants.TYPE_LIST:
                                    ProcessPackage(true);
                                    if (contentDto.IsMyProfileList)
                                    {
                                        RestoreMyProfileList(contentDto);
                                    }
                                    else
                                    {
                                        RestoreList(contentDto);
                                        IsItemHasDepedenciesList(); //switch thread mode for special list type
                                    }


                                    if (mAveList != null && mAveList.ListInfo != null && mAveList.SPList != null && !mAveList.IsSystemList && !mAveList.IsSpecialList)
                                    {
                                        InitListLevelObjects();
                                        if (!IsFreeContainer)
                                        {
                                            //InitAzureInfo(mConfig.AzureConifg);
                                        }

                                        using (new AvePerformanceScope("HSWorker.ProcessListRootFolderXML"))
                                        {
                                            //ProcessListRootFolderXML();
                                        }
                                        //mAveSite.MappingManager.SiteMappingManager.AddGuidToMappingCollection(mAveList.ListInfo.Id, mAveList.SPList.ID);
                                    }
                                    
                                    break;

                                case AveConstants.TYPE_FOLDER:
                                    using (new AvePerformanceScope("HSWorker.FolderXML"))
                                    {
                                        base.RestoreFolder(contentDto);
                                        ////Web root folderfolder
                                        //if (mAveList != null && mAveList.IsSystemList)
                                        //{
                                        //    base.RestoreFolder(contentDto);
                                        //    //this.mReport.SendReportAndSummary(reportDtos);
                                        //}
                                        //else
                                        //{
                                        //    if (ProcessFolderXML(contentDto))
                                        //    {
                                        //        _manifestProcessor.CurrentPackageCountCapacity += 1;
                                        //    }
                                        //    SplitPackage();
                                        //    //this.ObjOperator.CatchOperationOjb(contentDto, reportDtos[0], false);
                                        //}
                                    }
                                    break;

                                case AveConstants.TYPE_DOCUMENT:
                                case AveConstants.TYPE_VERSION:
                                    ProcessItem(contentDto);
                                    break;

                                case AveConstants.TYPE_LISTITEM:
                                case AveConstants.TYPE_LISTITEMVERSION:
                                case AveConstants.TYPE_ATTACHMENTS:
                                    base.RestoreItem(contentDto);
                                    break;

                                

                                //case AveConstants.TYPE_DOCUMENT:
                                //case AveConstants.TYPE_VERSION:
                                //    using (new AvePerformanceScope("HSWorker.DocumentXML"))
                                //    {
                                //        //Web root folderdocument
                                //        if (mAveList != null && mAveList.IsSystemList)
                                //        {
                                //            //base.RestoreDocument(contentDto, reportDtos);
                                //            //this.mReport.SendReportAndSummary(reportDtos);
                                //        }
                                //        else
                                //        {
                                //            if (ProcessDocumentXML(contentDto) && !contentDto.Name.Contains(":"))
                                //            {
                                //                _manifestProcessor.CurrentPackageCountCapacity += 1;
                                //            }
                                //            if (!contentDto.Name.Contains(":"))
                                //            {
                                //                SplitPackage();
                                //            }
                                //            //this.ObjOperator.CatchOperationOjb(contentDto, reportDtos[0], false);
                                //        }
                                //    }
                                //    break;

                                //case AveConstants.TYPE_LISTITEM:
                                //case AveConstants.TYPE_LISTITEMVERSION:
                                //    using (new AvePerformanceScope("HSWorker.ListItemXML"))
                                //    {
                                //        if (ProcessListItemXML(contentDto) && !contentDto.Name.Contains(":"))
                                //        {
                                //            _manifestProcessor.CurrentPackageCountCapacity += 1;
                                //        }
                                //        if (!contentDto.Name.Contains(":"))
                                //        {
                                //            SplitPackage();
                                //        }
                                //        //this.ObjOperator.CatchOperationOjb(contentDto, reportDtos[0], false);
                                //    }
                                //    break;
                                //case AveConstants.TYPE_ATTACHMENTS:
                                //    using (new AvePerformanceScope("HSWorker.AttachmentXML"))
                                //    {
                                //        ProcessAttachmentXML(contentDto);
                                //        //this.ObjOperator.CatchOperationOjb(contentDto, reportDtos[0], false);
                                //    }
                                //    break;

                                default:
                                    log.Warn(@"Looks up a localized string similar to Unknown object type: {0}.", contentDto.Type);
                                    break;
                            }
                        }
                        catch (JobStopException)
                        {
                            log.Warn("job is stopped by manual");
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Error(@"Looks up a localized string similar to An error occurred while doing the restore job. Type: {0},exception:{1}", contentDto.Type, e);
                            mError = e;
                        }
                    }

                    ProcessPackage(true);

                    if (multiReceiver != null && !multiReceiver.scheduler.IsEmpty)
                    {
                        multiReceiver.scheduler.Finish();
                        multiReceiver.Wait();
                        mLog.Info("End multiply upload.");
                    }
                    mAllJobStatus.Clear();
                }
                catch (JobStopException)
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
    }
}
