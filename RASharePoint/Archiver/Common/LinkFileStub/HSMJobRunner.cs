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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service.DomainModel;
using AvePoint.PhysicalCore.SQL;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao.DisposalStubDao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.DisposalStub;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.SharePoint.Archiver.Common.ApprovalService;
using AvePoint.RA.SharePoint.Archiver.Common.Manual;
using AvePoint.RA.SharePoint.Archiver.Scan.DiscorverScan.AOSP;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.Common.JobExecutionProcess;
using AvePoint.RA.SharePoint.Extension;
using AvePoint.RA.SharePoint.Object;
using AvePoint.StorageOptimization.Schedule.Archiver.SPObjects.Discover.DBScan;
using AvePoint.StorageOptimization.Schedule.Common;
using AvePoint.Wrapper.Common;
using DataOrchestration.Tag.Sdk.Service.CloudRecords.Contract;
using HSMAzureCommon;
using HSMCommon;
using Microsoft.SharePoint.Client;
using Microsoft.SharePoint.Client.CompliancePolicy;
using Microsoft365.SharePoint;
using Newtonsoft.Json;
using Polly.Caching;
using RAArchiverCommon;
using RAArchiverCommon.DestructionCache;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;

namespace AvePoint.RA.SharePoint.Archiver
{
    public class HSMJobRunner
    {
        #region private fields
        private AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private ScheduleConfiguration mConfig;
        private CGDBReader dbReader;
        private string fileNameSUFFIX = string.Empty;
        private AveMultiReceiver multiReceiver = null;
        private Dictionary<Guid, ImportJobResources> mAllJobStatus = new Dictionary<Guid, ImportJobResources>();


        private readonly object lockObj = new object();

        private IExplorerDao _explorerDao;
        protected IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new ExplorerDao(true);
                }
                return _explorerDao;
            }
        }
        #endregion
        public HSMJobRunner(ScheduleConfiguration configuration)
        {
            mConfig = configuration;
            fileNameSUFFIX = LinkFileCommon.GetStubFileNameSuffix(mConfig);
            var archiverExtendSetting = mConfig.ArchiverExtendSetting;
            if (archiverExtendSetting != null && archiverExtendSetting.IsCGDiscovery)
            {
                dbReader = CGDBReader.GetInstance(mConfig.ArchiverExtendSetting, mConfig.SiteCollectionID.ToString(), mConfig.SiteCollectionUrl);
            }
            InitTaskManager();
        }

        public void ResetTaskManager()
        {
            fileNameSUFFIX = LinkFileCommon.GetStubFileNameSuffix(mConfig);
            var archiverExtendSetting = mConfig.ArchiverExtendSetting;
            if (archiverExtendSetting != null && archiverExtendSetting.IsCGDiscovery)
            {
                dbReader = CGDBReader.GetInstance(mConfig.ArchiverExtendSetting, mConfig.SiteCollectionID.ToString(), mConfig.SiteCollectionUrl);
            }
            InitTaskManager();
        }

        public void AddImportJobTask(IAveSite site, Guid webId, Guid listId, string dataContainerDir, string manifestContainerDir, bool isEncryption, string listUrl, List<ARRestoreFileInfo> currentPackageItemsList)
        {
            WinAzure temAzure = new WinAzure();

            MutliImportParameter importParameter = new MutliImportParameter() { AzureInfo = temAzure, Site = site, WebId = webId, ListId = listId, ManifestContainerDir = manifestContainerDir, DataContainerDir = dataContainerDir, MigrationModuleType = MigrationModuleType.SPMigration, IsEncryption = isEncryption, IsNeedCheckSourceFilesUploaded = false, RetryMigrationJobTime = 60, CurrentRestoreFileIdsList = currentPackageItemsList };

            FreeContainerManager fcManager = new FreeContainerManager();
            importParameter.FCParameters = fcManager.CreateFreeContainers(site);
            importParameter.IsFreeContainer = true;


            AzureMultipleImport import = new AzureMultipleImport(importParameter, 1);

            PostActionDelegate postAction = new PostActionDelegate(ImportPostAction);
            import.updateErrorReportsEvent += new UpdateErrorReportsDelegate(this.UpdateErrorReports);
            import.sendJobReportEvent += new SendJobReportDelegate(this.SendJobReports);
            import.sendErrorJobReportEvent += new SendErrorJobReportDelegate(this.SendErrorJobReport);
            import.PostActionEvent = postAction;
            mLog.Info("The package ListUrl is {0}. Name is {1}.", listUrl, importParameter.AzureInfo.AzureManifestContainerName);
            multiReceiver.scheduler.AddTask(import);
        }

        public void AddReportItemInfo(Guid listId, string url, string id, string md5)
        {
            CreateLinkFileReportDto report = new CreateLinkFileReportDto();
            report.FileUrl = url;
            report.Md5 = md5;
            if (!mAllJobStatus.ContainsKey(listId))
            {
                lock (lockObj)
                {
                    if (!mAllJobStatus.ContainsKey(listId))
                    {

                        ImportJobResources importJobResources = new ImportJobResources();
                        mAllJobStatus.Add(listId, importJobResources);
                    }
                }
            }
            mAllJobStatus[listId].AddReports(id, report);
        }

        public void WatingCompleted()
        {
            if (multiReceiver != null && !multiReceiver.scheduler.IsEmpty)
            {
                mLog.Info("Start Site Import.");
                multiReceiver.scheduler.Finish();
                multiReceiver.Wait();
                mLog.Info("End Site Import. ");
            }
        }

        #region Event
        private void ImportPostAction(MutliImportParameter multiImportParameter)
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
                        if (NeedStopCurrentJob())
                        {
                            return;
                        }
                        mLog.Error("Error File Url:{0}", m.Value.Url);
                        mLog.Error("Error Code:{0}", m.Value.ErrorCode);
                        mLog.Error("Message: {0}", m.Value.Message);
                        var reportMess = ArchiverCommonStaticMethod.GetMessageFromCallStack(m.Value.Message);
                        if (m.Value.ErrorCode == SPErrorCode.TP_E_DOCALREADYEXISTS.ToString()) // cannot get modified and editor from SPO error queueMessage
                        {
                            reportMess = "RM_PU_SkipItemMessage";
                        }
                        else if (m.Value.ErrorCode == SPErrorCode.TP_E_INVALIDFILENAME.ToString()) // 429 too many request also can meet this error
                        {
                            reportMess = string.Format(I18NEntity.GetString("RM_JM_JD_ConvertStub_Comment_StubPathInvalidOrBusy"), m.Value.Url);
                        }
                        if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(m.Key))
                        {
                            mAllJobStatus[multiImportParameter.ListId].SetReportStatusAndMessage(m.Key, JobDetailsStatus.Failed, reportMess);
                        }
                        else
                        {
                            //mAllJobStatus[multiImportParameter.ListId].AddErrorFileUrl(m.Value.Url);
                            string url = m.Value.Url;
                            string fileSuffix = LinkFileCommon.GetStubFileNameSuffix(mConfig);
                            if (url.EndsWith(fileSuffix))
                            {
                                url = url.Substring(0, url.Length - fileSuffix.Length - 1);
                            }
                            mAllJobStatus[multiImportParameter.ListId].SetReportStatusAndMessageByUrl(url, JobDetailsStatus.Failed, reportMess);
                            if (mConfig.IsConvertStubJob)
                            {
                                continue;
                            }
                            IAveFile stubfile = mWeb.GetFile(m.Value.Url);
                            //1.出异常的Stub未生成，则不需要删除源文件。
                            if (!stubfile.Exists)
                            {
                                mLog.Warn("Stub does not exist when UpdateErrorReports.StubUrl: {0}.", m.Value.Url);
                            }
                            //2.出异常的Stub文件生成了，需要先删除异常Stub再删除源文件。
                            else
                            {
                                bool isStubMatch = false;
                                try
                                {
                                    if (stubfile.Item != null)
                                    {
                                        var archiverLinkFileType = stubfile.Item.FieldValues["ArchiverLinkFileType"];
                                        if (archiverLinkFileType != null)
                                        {
                                            isStubMatch = !string.IsNullOrEmpty(archiverLinkFileType.ToString());
                                        }
                                        //Get stub file content if file does not have stub column
                                        if (!isStubMatch)
                                        {
                                            mLog.Info($"Current file:{stubfile.ServerRelativeUrl} does not contains ArchiverLinkFileType and need check by file content.");
                                            if (stubfile.Length < 1024 * 10 && IsStubFileType(stubfile.Name))
                                            {
                                                mLog.Info($"Current file:{stubfile.ServerRelativeUrl} may OPUS generate stub and should double check stub content.");
                                                using (Stream fileStream = stubfile.OpenBinaryStream())
                                                {
                                                    using (StreamReader reader = new StreamReader(fileStream))
                                                    {
                                                        string fileContent = reader.ReadToEnd();
                                                        if (fileContent.Contains(mConfig.ReCenterURL))
                                                        {
                                                            mLog.Info($"Current file:{stubfile.ServerRelativeUrl} is OPUS generate stub.");
                                                            isStubMatch = true;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    mLog.Warn("file not a stub,because it's fieldValues does not contain ArchiverLinkFileType,error:{0}", e.ToString());
                                }
                                if (isStubMatch)
                                {
                                    using (AvePerformanceScope pc2 = new AvePerformanceScope("FileLevelRetention.RemoveStubFromSharePoint.DeleteFile.StubMatch"))
                                    {
                                        try
                                        {
                                            DeleteStubFile(stubfile);
                                            mLog.Info("Delete exception stub file successfully. {0}", m.Value.Url);
                                        }
                                        catch (Exception exc)
                                        {
                                            mLog.Warn("Delete exception stub file has some error. Detail : {0}.", exc.ToString());
                                        }
                                    }
                                }
                                else
                                {
                                    mLog.Info($"The file is not a stub file. so don't delete it.");
                                }
                            }
                        }
                    }
                }
            }
        }

        private bool IsStubFileType(string fileName)
        {
            List<string> stubTypes = new List<string>() { ".aspx", ".txt", ".html", ".url" };
            foreach (string stubType in stubTypes)
            {
                if (fileName.EndsWith(stubType, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private void SendJobReports(MutliImportParameter multiImportParameter, bool isImportJobCanceled)
        {            //isImportJobCanceled = true;
            if (isImportJobCanceled)
            {
                mLog.Warn("---------Import Job Canceled, so will check stub file exists.---------");
                using var siteStateTransitionScope = DisposalActivityManagementProcessor.TryUnlockSiteCollection(mConfig);
                SendJobReportsForCanceledJob(multiImportParameter);
                mLog.Warn("---------Finished : Import Job Canceled, so will check stub file exists.---------");
            }
            else
            {
                mLog.Info("---------Import Job Finished, so will bulk delete files.---------");
                List<ARRestoreFileInfo> mRRestoreFileInfos = new List<ARRestoreFileInfo>();
                using (new AvePerformanceScope("Event:SendJobReports"))
                {
                    using (var siteStateTransitionScope = DisposalActivityManagementProcessor.TryUnlockSiteCollection(mConfig))
                    {
                        using IAveWeb mWeb = multiImportParameter.Site.OpenWeb(multiImportParameter.WebId);
                        IAveList mList = mWeb.GetList(multiImportParameter.ListId);
                        mLog.Info($"HSMJobRunner SendJobReports.SiteURL:{multiImportParameter.Site.Url}.WebId:{multiImportParameter.WebId}.WebURL:{mWeb.ServerRelativeUrl}.ListId:{multiImportParameter.ListId}.ListTitle:{mList.Title}.");
                        foreach (var info in multiImportParameter.CurrentRestoreFileIdsList)
                        {
                            if (NeedStopCurrentJob())
                            {
                                return;
                            }

                            if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(info.id) && mAllJobStatus[multiImportParameter.ListId].GetReport(info.id).Status == JobDetailsStatus.Failed)
                            {
                                if (mConfig.IsConvertStubJob && mConfig.StubCache.TryGetValue(info.id, out var failStub))
                                {
                                    failStub.Status = JobDetailsStatus.Failed;
                                    var relativeUrl = mConfig.GetConvertingStubFullUrl(info.serverRelativeUrl, true);
                                    mConfig.JobReportDto.AddRecordReport(relativeUrl, ConvertStubAction.Create, failStub.Status, mAllJobStatus[multiImportParameter.ListId].GetReport(info.id).Message);
                                }
                                mLog.Warn("[HSMJobRunner][SendJobReports]This file status is Failed.");
                                mConfig.JobReportDto.HasErrorNode = true;
                                //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, new Guid(info.id), 50000, multiImportParameter.JobId);
                                continue;
                            }

                            if (mConfig.IsConvertStubJob && mConfig.StubCache.TryGetValue(info.id, out var successStub))
                            {
                                successStub.Status = JobDetailsStatus.Successful;
                                var relativeUrl = info.serverRelativeUrl.Split('/').Last().StartsWith(mConfig.JobId+"_") ? info.serverRelativeUrl.Replace(mConfig.JobId + "_","") : info.serverRelativeUrl;
                                mConfig.JobReportDto.AddRecordReport($"{mConfig.siteUrlSchemeAndHost}/{relativeUrl.Trim('/')}{LinkFileCommon.GetStubFileNameSuffixWithDot(mConfig)}", ConvertStubAction.Create, successStub.Status);
                            }

                            mRRestoreFileInfos.Add(info);
                            if (mRRestoreFileInfos.Count >= 20)
                            {
                                BulkDeclareAndDelete(mWeb, mList, mRRestoreFileInfos, multiImportParameter);
                                mRRestoreFileInfos.Clear();
                            }
                        }
                        if (mRRestoreFileInfos.Count != 0)
                        {
                            BulkDeclareAndDelete(mWeb, mList, mRRestoreFileInfos, multiImportParameter);
                            mRRestoreFileInfos.Clear();
                        }
                    }

                    if (mConfig.IsConvertStubJob)
                    {
                        return;
                    }

                    #region send report
                    foreach (var info in multiImportParameter.CurrentRestoreFileIdsList)
                    {
                        if (NeedStopCurrentJob())
                        {
                            return;
                        }
                        if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(info.id))
                        {
                            try
                            {
                                var report = mAllJobStatus[multiImportParameter.ListId].GetReport(info.id);
                                SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(info.TotalSize, mConfig.GetNodeFullPath(report.FileUrl), GenerateDiscoveryOptimizationFileReport(info));
                                if (mConfig.IsDiscoverOptimization && mConfig.currentRule.PolicyLevel == AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.Document)
                                {
                                    try
                                    {
                                        if (isAOSPJob(mConfig.jobtype))
                                        {
                                            if (mConfig.UseAospArchiverProfile)
                                            {
                                                mLog.Info("This is AOSP archiver profile job, no need to tag the file in db.");
                                            }
                                            else
                                            {
                                                mLog.Info("This is AOSP job, use AOSP Scanner tag.");
                                                DiscoveryAOSPScanner.Instance.TagAsArchivedAsync(info.id).GetAwaiter().GetResult();
                                            }
                                        }
                                        else
                                        {
                                            DiscoverScanner.TagAsArchivedAsync(info.id).GetAwaiter().GetResult();
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        mLog.Error($"TagAsArchived file failed,file url:{report.FileUrl},will retry:10s,error:{e}");
                                        Thread.Sleep(1000 * 10);
                                        try
                                        {
                                            if (isAOSPJob(mConfig.jobtype))
                                            {
                                                mLog.Info("This is AOSP job, use AOSP Scanner retag.");
                                                DiscoveryAOSPScanner.Instance.TagAsArchivedAsync(info.id).GetAwaiter().GetResult();
                                            }
                                            else
                                            {
                                                DiscoverScanner.TagAsArchivedAsync(info.id).GetAwaiter().GetResult();
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            mLog.Error($"retry TagAsArchived file failed,file url:{report.FileUrl},error:{ex}");
                                        }
                                    }
                                }
                                JobExecutionProcessStatisticExecutor.Instance.CalculateSuccessDeleteAndStubSummary(report.Status, mConfig.currentRule.Id, "SO_Action_LevelStub", (int)CacheNodeType.Item);
                                mConfig.JobReportDto.AddDeletionReport(mConfig.GetNodeFullPath(report.FileUrl),
                                             info.TotalSize,
                                             report.Status,
                                             (int)CacheNodeType.Item,
                                             mConfig.JobId,
                                             mConfig.currentRule.Name,
                                             "",
                                             "SO_Action_LevelStub",
                                             report.Message,
                                             "");
                                var archiverExtendSetting = mConfig.ArchiverExtendSetting;
                                if (archiverExtendSetting != null && archiverExtendSetting.IsCGDiscovery && dbReader != null)
                                {
                                    dbReader.UpdateStatusAndArchiveSize(mConfig.SiteCollectionID.ToString(), new Guid(info.id), ConvertToBackupRestoreStatus(report.Status), info.size, mConfig.ArchiverUNCTime);
                                }
                            }
                            catch (Exception e)
                            {
                                mLog.Warn("An error occur when sending job report, error is :{0}", e.ToString());
                            }
                            mAllJobStatus[multiImportParameter.ListId].RemoveReports(info.id);
                            mConfig.ProgressDto.UpdateProgress(true);
                        }
                    }
                    #endregion
                }
            }
        }

        private bool isAOSPJob(JobType jobtype) => jobtype == JobType.AOSPRestore ||
                                                   jobtype == JobType.DiscoveryAOSPJob ||
                                                   jobtype == JobType.DiscoveryAOSPOptimization ||
                                                   jobtype == JobType.DiscoveryAOSPOptimizationCalculate;


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
        private RMDiscoveryOptimizationFileReport GenerateDiscoveryOptimizationFileReport(ARRestoreFileInfo fileInfo)
        {

            RMDiscoveryOptimizationFileReport report = new RMDiscoveryOptimizationFileReport();
            try
            {
                report.AuthorID = fileInfo.AuthorID;
                report.AuthorEmail = fileInfo.AuthorEmail;
                report.ModifiedID = fileInfo.ModifiedID;
                report.ModifiedEmail = fileInfo.ModifiedEmail;
                report.CreateTime = fileInfo.CreateTime;
                report.ModifiedTime = fileInfo.ModifiedTime;
                report.VersionCount = fileInfo.VersionCount;
            }
            catch (Exception e)
            {
                mLog.Error($"Some thing went wrong when genarat RMDiscoveryOptimizationFileReport,error:{e.ToString()}");
            }
            return report;
        }
        private BackupRestoreStatus ConvertToBackupRestoreStatus(JobDetailsStatus status)
        {
            switch(status)
            {
                case JobDetailsStatus.Successful:
                    return BackupRestoreStatus.Succeed;
                case JobDetailsStatus.Failed:
                    return BackupRestoreStatus.Failed;
                case JobDetailsStatus.Skipped:
                    return BackupRestoreStatus.Skipped;
                default:
                    return BackupRestoreStatus.UnKnown;
            }
        }
        private void SendJobReportsForCanceledJob(MutliImportParameter multiImportParameter)
        {
            using (new AvePerformanceScope("Event:SendJobReportsForCanceledJob"))
            {
                using (IAveWeb mWeb = multiImportParameter.Site.OpenWeb(multiImportParameter.WebId))
                {
                    bool declareStubFile = LinkFileCommon.IsDeclareLinkFile(mConfig);

                    foreach (var info in multiImportParameter.CurrentRestoreFileIdsList)
                    {
                        if (NeedStopCurrentJob())
                        {
                            return;
                        }
                        if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(info.id) && mAllJobStatus[multiImportParameter.ListId].GetReport(info.id).Status == JobDetailsStatus.Failed)
                        {
                            mLog.Warn("[HSMJobRunner][SendJobReportsForCanceledJob]This file status is Failed.");
                            mConfig.ProgressDto.HasErrorNode = true;
                            if (mConfig.IsConvertStubJob && mConfig.StubCache.TryGetValue(info.id, out var failStub))
                            {
                                failStub.Status = JobDetailsStatus.Failed;
                                var relativeUrl = mConfig.GetConvertingStubFullUrl(info.serverRelativeUrl, true);
                                mConfig.JobReportDto.AddRecordReport(relativeUrl, ConvertStubAction.Create, failStub.Status, mAllJobStatus[multiImportParameter.ListId].GetReport(info.id).Message);
                            }
                            continue;
                        }
                        if (mConfig.IsConvertStubJob)
                        {
                            if (!mConfig.StubCache.TryGetValue(info.id, out var stubFile)) continue;
                            var oldStubUrl = mConfig.GetConvertingStubFullUrl(info.serverRelativeUrl);
                            var newStubUrl = mConfig.GetConvertingStubFullUrl(info.serverRelativeUrl, true);
                            var isAddedCreateReport = false;
                            var isDeleteOldStub = false;
                            try
                            {
                                var newStub = mWeb.GetFile($"{info.serverRelativeUrl}.{fileNameSUFFIX}");
                                var oldStub = mWeb.GetList(multiImportParameter.ListId).GetItemById(info.rowid).File;

                                stubFile.Status = newStub.Exists && !oldStub.Exists ? JobDetailsStatus.Successful : JobDetailsStatus.Failed;
                                var jobStatus = mAllJobStatus[multiImportParameter.ListId];
                                var isJobFail = jobStatus.ContainsReport(info.id) && jobStatus.GetReport(info.id).Status == JobDetailsStatus.Failed;
                                var errorMess = string.Empty;

                                if (!newStub.Exists)
                                {
                                    errorMess = isJobFail ? jobStatus.GetReport(info.id).Message : "RM_PRM_PRE_Msg_NewItemError";
                                    mConfig.JobReportDto.AddRecordReport(newStubUrl, ConvertStubAction.Create, JobDetailsStatus.Failed, errorMess);
                                    isAddedCreateReport = true;
                                    continue;
                                }
                                else if (newStub.CheckedOutByUser != null)
                                {
                                    newStub.CheckIn("");
                                    mLog.Info($"SendJobReportsForCanceledJob.stubfile CheckIn success.URL:{newStub.ServerRelativeUrl}.");
                                }
                                // new stub created successfully, add tracking record
                                AddStubFileRecordMapping(multiImportParameter.WebId, multiImportParameter.ListId, info);

                                if (oldStub.Exists)
                                {
                                    mLog.Info("New stub exist and old stub exist->Delete old File.");
                                    RemoveRelatedRelationship(multiImportParameter.Site, oldStub.Item, oldStub.ServerRelativeUrl);
                                    DeleteStubFile(oldStub);
                                    // old stub deleted successfully, delete tracking record
                                    mConfig.DeleteStubFileRecordEntitiesInBatch(info.id);
                                    isDeleteOldStub = true;
                                }

                                if (mConfig.isConvertSameTypeStub)
                                {
                                    RenameStubFile(newStub, oldStubUrl, declareStubFile);
                                }
                                else if (declareStubFile)
                                {
                                    var listItem = newStub.Item.ParentList.GetItemById(newStub.Item.ID);
                                    mConfig.aveObjectModelFactory.CreateRecords().DeclareItemAsRecord(listItem);
                                }

                                mConfig.JobReportDto.AddRecordReport(newStubUrl, ConvertStubAction.Create, JobDetailsStatus.Successful);
                                isAddedCreateReport = true;

                                if (oldStub.Exists && !isDeleteOldStub)
                                {
                                    errorMess = isJobFail ? jobStatus.GetReport(info.id).Message : "StorageOptimization_SOARCOMArchiverReportDtoAddDeletionCommonsItem";
                                    mConfig.JobReportDto.AddRecordReport(oldStubUrl, ConvertStubAction.Delete, JobDetailsStatus.Failed, errorMess);
                                    continue;
                                }

                                mConfig.JobReportDto.AddRecordReport(oldStubUrl, ConvertStubAction.Delete, JobDetailsStatus.Successful);
                                stubFile.Status = JobDetailsStatus.Successful;
                                continue;
                            }
                            catch (Exception e)
                            {
                                mLog.Warn($"An error occur when SendJobReportsForCanceledJob for file: {oldStubUrl}, error is :{e}");
                                mConfig.ProgressDto.HasErrorNode = true;
                                if (!isAddedCreateReport)
                                {
                                    var status = isDeleteOldStub ? JobDetailsStatus.Failed : JobDetailsStatus.Successful;
                                    var message = isDeleteOldStub ? "RM_PRM_PRE_Msg_NewItemError" : null;

                                    mConfig.JobReportDto.AddRecordReport(newStubUrl, ConvertStubAction.Create, status, message);
                                }

                                if (isAddedCreateReport || !isDeleteOldStub)
                                {
                                    mConfig.JobReportDto.AddRecordReport(oldStubUrl, ConvertStubAction.Delete, JobDetailsStatus.Failed,
                                        "StorageOptimization_SOARCOMArchiverReportDtoAddDeletionCommonsItem");
                                }
                                continue;
                            }
                        }
                        try
                        {
                            IAveFile file = GetFile(mWeb, info.serverRelativeUrl, new Guid(info.id));

                            IAveFile stubfile = mWeb.GetFile(file.ServerRelativeUrl + "." + fileNameSUFFIX);
                            if (!stubfile.Exists)
                            {
                                mLog.Info($"SendJobReportsForCanceledJob.Stub does not exist.URL:{file.ServerRelativeUrl + "." + fileNameSUFFIX}.");
                                mConfig.ProgressDto.HasErrorNode = true;
                                if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(info.id))
                                {
                                    mAllJobStatus[multiImportParameter.ListId].SetReportStatus(info.id, JobDetailsStatus.Failed);
                                }
                                continue;
                            }
                            else if (stubfile.CheckedOutByUser != null)
                            {
                                stubfile.CheckIn("");
                                mLog.Info($"SendJobReportsForCanceledJob.stubfile CheckIn success.URL:{file.ServerRelativeUrl + "." + fileNameSUFFIX}.");
                            }
                            // stub created successfully, add tracking record
                            AddStubFileRecordMapping(multiImportParameter.WebId, multiImportParameter.ListId, info);

                            //Stub exist and source file exist->Delete Source File.
                            if (file.Exists)
                            {
                                mLog.Info($"Current stub exist and source file exist->Delete Source File.URL:{file.ServerRelativeUrl + "." + fileNameSUFFIX}.");
                                RemoveRelatedRelationship(multiImportParameter.Site, file.Item, file.ServerRelativeUrl);
                                DeleteFile(file);
                            }
                            IAveListItem newItem = stubfile.Item;
                            //REC-2432 Host Header Site Collection通过IAveFile GetFile(string serverRelativeUrl);方式获取不到IAveListItem对象.
                            if (newItem == null)
                            {
                                mLog.Debug("Current IAveListItem is null and will ReGet IAveListItem by List GetItemByUniqueId.");
                                newItem = file.ParentFolder.ParentList.GetItemByUniqueId(stubfile.UniqueId);
                                mLog.Info("ReGet IAveListItem successful by List GetItemByUniqueId. IAveListItem is null:{0}.", newItem == null);
                            }
                            ArgumentNullException.ThrowIfNull(newItem);
                            if (!newItem.FieldValues.ContainsKey(LinkFileCommon.LinkFileFieldName)
                                || newItem.FieldValues[LinkFileCommon.LinkFileFieldName] == null
                                || string.IsNullOrEmpty(newItem.FieldValues[LinkFileCommon.LinkFileFieldName].ToString()))
                            {
                                mLog.Info("Need set link field value again.");
                                LinkFileCommon.SetLinkFieldValue(stubfile.Item, mConfig);
                            }
                            if (DeclareLinkFile())
                            {
                                if (mConfig.IsOneDriverSite)
                                {
                                    mAllJobStatus?[multiImportParameter.ListId]?.SetReportMessage(info.id, "RM_SO_OneDriveDeclareItem_ErrorMessage");
                                }
                                else
                                {
                                    DeclareItem(multiImportParameter.Site, newItem);
                                }
                            }
                            if (mConfig.IsILMode)
                            {
                                string md5 = mAllJobStatus[multiImportParameter.ListId].ContainsReport(info.id) ? mAllJobStatus[multiImportParameter.ListId].GetReport(info.id).Md5 : string.Empty;
                                UpdateExploreDB(multiImportParameter.Site, new Guid(info.id), mConfig.actionType == ActionType.ArchchiveToStorage ? 8 : 2, pathMd5: md5);
                            }

                        }
                        catch (Exception e)
                        {
                            mLog.Warn("An error occur when post-processing item, error is :{0}", e.ToString());
                            mConfig.ProgressDto.HasErrorNode = true;
                        }
                    }
                }

                if (mConfig.IsConvertStubJob)
                {
                    return;
                }

                #region send report
                foreach (var info in multiImportParameter.CurrentRestoreFileIdsList)
                {
                    if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(info.id))
                    {
                        try
                        {
                            var report = mAllJobStatus[multiImportParameter.ListId].GetReport(info.id);
                            SOArchiverJobInfoStatistics.Instance.AccumulationItemsSize(info.TotalSize, mConfig.GetNodeFullPath(report.FileUrl), GenerateDiscoveryOptimizationFileReport(info));
                            JobExecutionProcessStatisticExecutor.Instance.CalculateSuccessDeleteAndStubSummary(report.Status, "", "SO_Action_LevelStub", (int)CacheNodeType.Item);
                            mConfig.JobReportDto.AddDeletionReport(mConfig.GetNodeFullPath(report.FileUrl),
                                         info.TotalSize,
                                         report.Status,
                                         (int)CacheNodeType.Item,
                                         mConfig.JobId,
                                         mConfig.currentRule?.Name ?? "",
                                         "",
                                         "SO_Action_LevelStub",
                                         report.Message,
                                         "");
                            var archiverExtendSetting = mConfig.ArchiverExtendSetting;
                            if (archiverExtendSetting != null && archiverExtendSetting.IsCGDiscovery && dbReader != null)
                            {
                                dbReader.UpdateStatusAndArchiveSize(mConfig.SiteCollectionID.ToString(), new Guid(info.id), ConvertToBackupRestoreStatus(report.Status), info.size, mConfig.ArchiverUNCTime);
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Warn("An error occur when sending job report, error is :{0}", e.ToString());
                        }
                        mAllJobStatus[multiImportParameter.ListId].RemoveReports(info.id);
                    }
                }
                #endregion
            }
        }

        private void SendErrorJobReport(string errorMessage, MutliImportParameter multiImportParameter)
        {
            //RemoveReports
            mLog.Error("an error occurred when running job. error is {0}", errorMessage);
            if (NeedStopCurrentJob())
            {
                mLog.Info("this job has stopped");
                return;
            }
            mLog.Warn("[HSMJobRunner][SendErrorJobReport]This job has error. Set HasErrorNode to true.");
            mConfig.JobReportDto.HasErrorNode = true;
            mConfig.JobReportDto.summaryComments = errorMessage;

            var defaultErrorMessage = string.Empty;
            if (errorMessage.Contains("Only a site collection administrator can add a work item") || mConfig.IsSiteReadOnly) // cannot create migration job for a locked site
            {
                defaultErrorMessage = "RM_AR_Restore_SiteLocked_ErrorMessage";
            }
            foreach (var info in multiImportParameter.CurrentRestoreFileIdsList)
            {
                if (NeedStopCurrentJob())
                {
                    return;
                }
                if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(info.id))
                {
                    try
                    {
                        var report = mAllJobStatus[multiImportParameter.ListId].GetReport(info.id);
                        JobExecutionProcessStatisticExecutor.Instance.CalculateSuccessDeleteAndStubSummary(JobDetailsStatus.Failed, "", "SO_Action_LevelStub", (int)CacheNodeType.Item);
                        mConfig.JobReportDto.AddDeletionReport(mConfig.GetNodeFullPath(report.FileUrl),
                                     0,
                                     JobDetailsStatus.Failed,
                                     (int)CacheNodeType.Item,
                                     mConfig.JobId,
                                     "",
                                     "",
                                     "SO_Action_LevelStub",
                                     defaultErrorMessage,
                                     "");
                        var archiverExtendSetting = mConfig.ArchiverExtendSetting;
                        if (archiverExtendSetting != null && archiverExtendSetting.IsCGDiscovery && dbReader != null)
                        {
                            dbReader.UpdateStatus(mConfig.SiteCollectionID.ToString(), new Guid(info.id), BackupRestoreStatus.Failed);
                        }
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("An error occur when sending job report, error is :{0}", e.ToString());
                    }
                    mAllJobStatus[multiImportParameter.ListId].RemoveReports(info.id);
                }
            }
        }


        #endregion
        private void InitTaskManager()
        {
            multiReceiver = new AveMultiReceiver(mConfig.BackgroundSettings.TotalMultiDeleteThreadNumber);
            multiReceiver.scheduler.AddTask(new AveMutiEmpty(0, true));
        }

        private IAveFile GetFile(IAveWeb web, string serverRelativeUrl, Guid id)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiverDeletion.GetCreateLinkFile"))
            {
                IAveFile linkFile = web.GetFile(serverRelativeUrl);
                if (!linkFile.Exists)
                {
                    mLog.Info("Load file by id.");
                    try
                    {
                        //Office 365 Root Site Collection need send serverRelativeUrl.RECO-1278
                        linkFile = web.GetFile(id);
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Can't reGet file UniqueId.Message:{0}.", ex.ToString());
                    }
                }
                if (!linkFile.Exists)
                {
                    mLog.Warn($"Load file failed,ServerRelativeUrl: {serverRelativeUrl} id:{id}");
                }
                return linkFile;
            }
        }

        private void DeclareItem(IAveSite site, IAveListItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("SP2013ArchiveBackUp.KeepData.DeclareItem"))
            {
                try
                {
                    if (!ScheduleConfiguration.CheckisRecord(item))
                    {
                        mConfig.aveObjectModelFactory.CreateRecords().DeclareItemAsRecord(item);
                    }
                }
                catch (Exception exc)
                {
                    mLog.Warn("Declare Item has some error, detail : {0}", exc.ToString());
                    throw;
                }
            }
        }

        /// <summary>
        /// DeclareLinkFile is RA job option.
        /// DeclareStubOption is DAO job option.
        /// DeclareStubType.None for old rule,
        /// DeclareStubType.Declare for new rule.
        /// </summary>
        /// <returns></returns>
        private bool DeclareLinkFile()
        {
            //mConfig.currentRule.DeclareLinkFile = false;
            if (mConfig.IsILMode)
            {
                if (mConfig.currentRule != null && mConfig.currentRule.DeclareLinkFile)
                {
                    return true;
                }
                return false;
            }
            else if (mConfig.currentRule != null && (mConfig.currentRule.DeclareLinkFile || mConfig.currentRule.DeclareStubOption == DeclareStubType.Declare || mConfig.currentRule.DeclareStubOption == DeclareStubType.None))
            {
                return true;
            }
            return false;
        }

        private void RemoveRelatedRelationship(IAveSite site, IAveListItem aveListItem, string itemUrl)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.RemoveRelatedRelationship"))
            {
                if (mConfig.IsILMode)
                {
                    try
                    {
                        //移除related 关系
                        var utility = new RelatedRecordsUtility();
                        var relatedInfos = utility.GetRelatedProperties(aveListItem);
                        if (relatedInfos.IsNullOrEmpty())
                        {
                            return;
                        }
                        foreach (var relatedInfo in relatedInfos)
                        {
                            utility.RemoveRelateColumnValue(relatedInfo, site, itemUrl, aveListItem.UniqueId, "");
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Info("An error occur when remove related relationship.ItemUrl:{0}.Message:{1}.", itemUrl, ex.ToString());
                    }
                }
            }
        }


        private void BulkDeclareAndDelete(IAveWeb mWeb, IAveList mList, List<ARRestoreFileInfo> infos, MutliImportParameter multiImportParameter)
        {
            try
            {
                bool declareStubFile = LinkFileCommon.IsDeclareLinkFile(mConfig);
                //1.先remove掉没有成功生成的Stub数据，这种数据不需要删除源文件.同时也不需要对Declare文件执行Declare操作.
                List<BulkDeclareAndDeleteFileInfo> mBulkDeclareAndDeleteFileInfos = new List<BulkDeclareAndDeleteFileInfo>();
                foreach (ARRestoreFileInfo mRRestoreFileInfo in infos)
                {
                    AddStubFileRecordMapping(multiImportParameter.WebId, multiImportParameter.ListId, mRRestoreFileInfo);

                    BulkDeclareAndDeleteFileInfo bulkDeclareAndDeleteFileInfo = new BulkDeclareAndDeleteFileInfo();
                    bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo = mRRestoreFileInfo;
                    if (declareStubFile)
                    {
                        using (new AvePerformanceScope("SP2013ArchiveBackUp.BulkDeclareAndDelete.GetStubItem"))
                        {
                            IAveFile stubfile = mWeb.GetFile(mRRestoreFileInfo.serverRelativeUrl + "." + fileNameSUFFIX);
                            if (!stubfile.Exists)
                            {
                                mLog.Warn("Stub does not exist when BulkDeclareAndDelete.StubUrl: {0}.", mRRestoreFileInfo.serverRelativeUrl + "." + fileNameSUFFIX);
                                mConfig.JobReportDto.HasErrorNode = true;
                                if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(mRRestoreFileInfo.id))
                                {
                                    mAllJobStatus[multiImportParameter.ListId].SetReportStatus(mRRestoreFileInfo.id, JobDetailsStatus.Failed);
                                }
                                //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, new Guid(mRRestoreFileInfo.id), 50000, mRRestoreFileInfo.subjobid);
                                continue;
                            }
                            else if (stubfile.CheckedOutByUser != null)
                            {
                                stubfile.CheckIn("");
                            }
                            if (!stubfile.Item.FieldValues.ContainsKey(LinkFileCommon.LinkFileFieldName)
                            || stubfile.Item.FieldValues[LinkFileCommon.LinkFileFieldName] == null
                            || string.IsNullOrEmpty(stubfile.Item.FieldValues[LinkFileCommon.LinkFileFieldName].ToString()))
                            {
                                mLog.Info("Need set link field value again.");
                                LinkFileCommon.SetLinkFieldValue(stubfile.Item, mConfig);
                            }
                            bulkDeclareAndDeleteFileInfo.stubListItem = stubfile.Item;
                            bulkDeclareAndDeleteFileInfo.stubItemRowId = stubfile.Item.ID;
                        }
                    }
                    mBulkDeclareAndDeleteFileInfos.Add(bulkDeclareAndDeleteFileInfo);
                }
                //2.如果勾选Declare，则对Stub文件执行Declare操作(先批量，出异常再one by one).
                //测试结果：批量Declare，Declare数据和非Declare数据同时存在，也可以批量Declare成功。
                // convert stub job + same type will declare one by one later after rename
                if (declareStubFile && (!mConfig.IsConvertStubJob || !mConfig.isConvertSameTypeStub))
                {
                    if(mConfig.currentRule.DeclareStubOption == DeclareStubType.AddRecordLabel)
                    {
                        mLog.Info("Start Add record label for stub file");
                        if(mConfig.SharePointRetentionLabel == null)
                        {
                            mConfig.InitRetentionLabelCollections(mWeb.Site);
                        }
                        string retentionLabel = mConfig.GeneralRetentionLabel;
                        if (string.IsNullOrEmpty(retentionLabel))
                        {
                            mLog.Warn($"Record label in general setting is empty");
                            foreach (BulkDeclareAndDeleteFileInfo bulkDeclareAndDeleteFileInfo in mBulkDeclareAndDeleteFileInfos)
                            {
                                mAllJobStatus[multiImportParameter.ListId].SetReportStatus(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id, JobDetailsStatus.Failed, "StorageOptimization_SOARRecordLabelDoesNotSetValue");
                            }
                        }
                        else if (mConfig.SharePointRetentionLabel.TryGetValue(retentionLabel, out var info))
                        {
                            if(info.BlockDelete && info.BlockEdit)
                            {
                                using (new AvePerformanceScope("SP2013ArchiveBackUp.BulkDeclareAndDelete.SetComplianceTagOnBulkItems"))
                                {
                                    var record = mConfig.aveObjectModelFactory.CreateRecords();
                                    foreach (BulkDeclareAndDeleteFileInfo bulkDeclareAndDeleteFileInfo in mBulkDeclareAndDeleteFileInfos)
                                    {
                                        try
                                        {
                                            var listItem = bulkDeclareAndDeleteFileInfo.stubListItem;
                                            var isNeedDeclareRecord = false;
                                            if (listItem.IsRecord())
                                            {
                                                mLog.Info($"Start undeclare item {bulkDeclareAndDeleteFileInfo.stubItemRowId}");
                                                record.UndeclareItemAsRecord(listItem);
                                                isNeedDeclareRecord = true;
                                            }
                                            listItem.SetComplianceTag(info.TagName, true, true, false, false);
                                            if (isNeedDeclareRecord)
                                            {
                                                mLog.Info($"Declare item {bulkDeclareAndDeleteFileInfo.stubItemRowId}");
                                                record.DeclareItemAsRecord(listItem);
                                            }
                                        }
                                        catch(Exception e)
                                        {
                                            mLog.Warn("Failed record label and Add record label item one by one.Message:{0}", e.ToString());
                                            mAllJobStatus[multiImportParameter.ListId].SetReportStatus(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id, JobDetailsStatus.Failed, e.Message);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                mLog.Error($"Current label : {retentionLabel} is not record label");
                                foreach (BulkDeclareAndDeleteFileInfo bulkDeclareAndDeleteFileInfo in mBulkDeclareAndDeleteFileInfos)
                                {
                                    mAllJobStatus[multiImportParameter.ListId].SetReportStatus(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id, JobDetailsStatus.Failed, "StorageOptimization_SOARCurrentLabelIsNotRecordLabel");
                                }
                            }
                        }
                        else
                        {
                            mLog.Error($"Cannot get label : {retentionLabel} in current site collection.");
                            foreach (BulkDeclareAndDeleteFileInfo bulkDeclareAndDeleteFileInfo in mBulkDeclareAndDeleteFileInfos)
                            {
                                mAllJobStatus[multiImportParameter.ListId].SetReportStatus(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id, JobDetailsStatus.Failed, "StorageOptimization_SOARTagCannotGetLabelByName");
                            }
                        }
                    }
                    else if (mConfig.IsOneDriverSite)
                    {
                        try
                        {

                            mLog.Warn("OneDrive site unable declare item as record, will update report message and status");
                            foreach (BulkDeclareAndDeleteFileInfo bulkDeclareAndDeleteFileInfo in mBulkDeclareAndDeleteFileInfos)
                            {
                                mAllJobStatus[multiImportParameter.ListId].SetReportMessage(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id, "RM_SO_OneDriveDeclareItem_ErrorMessage");
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
                            using (new AvePerformanceScope("SP2013ArchiveBackUp.BulkDeclareAndDelete.BulkDeclareItem"))
                            {
                                mLog.Info("Begin BulkDeclareItem. ItemsCount : {0}.", mBulkDeclareAndDeleteFileInfos.Count);
                                mList.DeclareItemsByRowIds(mBulkDeclareAndDeleteFileInfos.Select(x => x.stubItemRowId).ToList<int>());
                                mLog.Info("End BulkDeclareItem. ItemsCount : {0}.", mBulkDeclareAndDeleteFileInfos.Count);
                            }
                        }
                        catch (Exception ex)
                        {
                            mLog.Warn("Failed DeclareItemsByRowIds and declare item one by one.Message:{0}", ex.ToString());
                            //one by one declare
                            foreach (BulkDeclareAndDeleteFileInfo bulkDeclareAndDeleteFileInfo in mBulkDeclareAndDeleteFileInfos)
                            {
                                using (new AvePerformanceScope("SP2013ArchiveBackUp.BulkDeclareAndDelete.DeclareItemOneByOne"))
                                {
                                    try
                                    {
                                        if (!ArchiverCommonStaticMethod.CheckisRecord(bulkDeclareAndDeleteFileInfo.stubListItem))
                                        {
                                            mConfig.aveObjectModelFactory.CreateRecords().DeclareItemAsRecord(bulkDeclareAndDeleteFileInfo.stubListItem);
                                            mLog.Info("Success declare file one by one.Items Url: {0}.", bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.serverRelativeUrl);
                                        }
                                    }
                                    catch (Exception exc)
                                    {
                                        mLog.Warn("Declare Item has some error when one by one declare. Detail : {0}.", exc.ToString());
                                        mConfig.JobReportDto.HasErrorNode = true;
                                        bulkDeclareAndDeleteFileInfo.hasErrorNode = true;
                                        if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id))
                                        {
                                            if (mConfig.IsOneDriverSite)
                                            {
                                                mAllJobStatus[multiImportParameter.ListId].SetReportStatus(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id, JobDetailsStatus.Failed, "RM_SO_OneDriveDeclare_ErrorMessage");
                                            }
                                            else
                                            {
                                                mAllJobStatus[multiImportParameter.ListId].SetReportStatus(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id, JobDetailsStatus.Failed, exc.Message);
                                            }
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
                try
                {
                    using (new AvePerformanceScope("SP2013ArchiveBackUp.BulkDeclareAndDelete.BulkDeleteItem"))
                    {
                        mLog.Info("Begin BulkDeleteItem. ItemsCount : {0}.", mBulkDeclareAndDeleteFileInfos.Count);
                        if (WrapperConfiguration.EnableRemoveRetentionLabel ||
                            mConfig.currentRule.IncludeDeleteRecordLabel ||
                            (mConfig.currentRule.KeepDataOption & (int)KeepDataOption.IsEnableRemoveRetentionLabel) == (int)KeepDataOption.IsEnableRemoveRetentionLabel)
                        {
                            mList.SetComplianceTagOnBulkItems(mBulkDeclareAndDeleteFileInfos.Select(x => x.mARRestoreFileInfo.rowid).ToList<int>(), "");
                        }
                        try
                        {
                            StringBuilder builder = new StringBuilder();
                            foreach (var f in mBulkDeclareAndDeleteFileInfos)
                            {
                                builder.AppendFormat("{0},", f.mARRestoreFileInfo.rowid);
                            }
                            mLog.Info($"Begin BulkDeleteItem.Ids {builder.ToString()}");
                        }
                        catch (Exception e)
                        {
                            mLog.Error($"error occured when BulkDeclareAndDelete1,error:{e}");
                        }
                        var deletableItems = mBulkDeclareAndDeleteFileInfos
                            .Where(x => !x.mARRestoreFileInfo.IsManifestStub)
                            .ToList();

                        if (deletableItems.Count > 0)
                        {
                            mList.DeleteItemsByRowIds(
                                deletableItems.ToDictionary(x => x.mARRestoreFileInfo.rowid, y => y.mARRestoreFileInfo.ModifiedTimeTicks),
                                deletableItems.ToDictionary(x => x.mARRestoreFileInfo.rowid, y => y.mARRestoreFileInfo.TimeLastModifiedTicks));
                        }
                        else
                        {
                            mLog.Info("No non-manifest stub items to delete in this batch.");
                        }

                        if (!mConfig.IsConvertStubJob)
                        {
                            foreach (var bulkDeclareAndDeleteFileInfo in mBulkDeclareAndDeleteFileInfos)
                            {
                                SendDestructionReport(multiImportParameter.Site, bulkDeclareAndDeleteFileInfo);
                            }
                        }
                        mLog.Info("End BulkDeleteItem. ItemsCount : {0}.", mBulkDeclareAndDeleteFileInfos.Count);
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn("Failed DeleteItemsByRowIds and delete item one by one.Message:{0}", ex.ToString());
                    //one by one delete
                    foreach (BulkDeclareAndDeleteFileInfo bulkDeclareAndDeleteFileInfo in mBulkDeclareAndDeleteFileInfos)
                    {
                        var oldStubUrl = string.Empty;
                        using (new AvePerformanceScope("SP2013ArchiveBackUp.BulkDeclareAndDelete.DeleteItemOneByOne"))
                        {
                            try
                            {
                                if (mConfig.IsConvertStubJob)
                                {
                                    var stubfile = mList.GetItemById(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.rowid).File;
                                    if (stubfile.Exists)
                                    {
                                        DeleteStubFile(stubfile);
                                        if (mConfig.isConvertSameTypeStub)
                                        {
                                            oldStubUrl = mConfig.siteUrlSchemeAndHost + stubfile.ServerRelativeUrl;
                                            stubfile = mWeb.GetFile(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.serverRelativeUrl + "." + fileNameSUFFIX);
                                            if (stubfile.Exists) RenameStubFile(stubfile, oldStubUrl, declareStubFile);
                                        }
                                        else
                                        {
                                            oldStubUrl = mConfig.GetConvertingStubFullUrl(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.serverRelativeUrl);
                                        }
                                        bulkDeclareAndDeleteFileInfo.isSendedReport = true;
                                        mConfig.DeleteStubFileRecordEntitiesInBatch(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id);
                                        mConfig.JobReportDto.AddRecordReport(oldStubUrl, ConvertStubAction.Delete, JobDetailsStatus.Successful);
                                    }
                                    continue;
                                }
                                IAveFile file = mWeb.GetFile(new Guid(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id), bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.serverRelativeUrl);
                                if (file != null && !file.Exists)
                                {
                                    mLog.Info("Current file already deleted in batch.Items Url: {0}.", bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.serverRelativeUrl);
                                    continue;
                                }
                                else
                                {
                                    if (CheckItemHasModifiedAfterBackup(file.Item, bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.ModifiedTimeTicks, bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.TimeLastModifiedTicks))
                                    {
                                        mAllJobStatus[multiImportParameter.ListId].SetReportMessage(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id, "StorageOptimization_DeleteItemSkip_Modified");
                                        continue;
                                    }
                                    var docId = new Guid(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id);
                                    var record = GetRecordInExplorerDao(multiImportParameter.Site, docId);
                                    DeleteFile(file);
                                    SendDestructionReport(multiImportParameter.Site, bulkDeclareAndDeleteFileInfo);
                                }
                            }
                            catch (Exception exc)
                            {
                                if (mConfig.IsConvertStubJob)
                                {
                                    var baseException = exc is ServerException se ? se :
                                                        exc.InnerException is ServerException innerSe ? innerSe :
                                                        null;

                                    if (exc.Message.Contains("Item does not exist. It may have been deleted by another user") ||
                                        (baseException != null &&
                                         (baseException.ServerErrorCode == AveSPErrorCode.FILE_NOT_FOUND ||
                                          baseException.ServerErrorCode == AveSPErrorCode.ERROR_COLUMN_DOES_NOT_EXIST)))
                                    {
                                        mLog.Info($"Old stub {bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.serverRelativeUrl} is already deleted.");
                                        // add DeleteStubFileRecordEntitiesInBatch logic in the foreach below, not here
                                        continue;
                                    }

                                    oldStubUrl = mConfig.GetConvertingStubFullUrl(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.serverRelativeUrl);
                                    mLog.Warn($"Delete old stub {oldStubUrl} has some error when one by one delete. Detail : {exc}.");
                                    if (mConfig.StubCache.TryGetValue(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id, out var successStub))
                                    {
                                        successStub.Status = JobDetailsStatus.Failed;
                                    }
                                    bulkDeclareAndDeleteFileInfo.isSendedReport = true;
                                    mConfig.JobReportDto.AddRecordReport(oldStubUrl, ConvertStubAction.Delete, JobDetailsStatus.Failed, exc.Message);
                                    continue;
                                }

                                IAveFile stubfile = mWeb.GetFile(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.serverRelativeUrl + "." + fileNameSUFFIX);
                                if (stubfile.Exists)
                                {
                                    DeleteStubFile(stubfile);
                                    mLog.Warn($"Delete archived file has error, need to delete stub file. File: {bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.serverRelativeUrl.LogBase64()}");
                                    mConfig.DeleteStubFileRecordEntitiesInBatch(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id);
                                }
                                mLog.Warn("Delete Item has some error when one by one delete. Detail : {0}.", exc.ToString());
                                mConfig.JobReportDto.HasErrorNode = true;
                                bulkDeclareAndDeleteFileInfo.hasErrorNode = true;
                                if (mAllJobStatus[multiImportParameter.ListId].ContainsReport(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id))
                                {
                                    JobDetailsStatus status = JobDetailsStatus.Failed;
                                    if (exc.Message == "StorageOptimization_Skip_Unlock_Status_Item")
                                    {
                                        status = JobDetailsStatus.Skipped;
                                    }
                                    mAllJobStatus[multiImportParameter.ListId].SetReportStatus(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id, status, exc.Message);
                                }
                                continue;
                            }
                        }
                    }
                }
                foreach (BulkDeclareAndDeleteFileInfo bulkDeclareAndDeleteFileInfo in mBulkDeclareAndDeleteFileInfos)
                {
                    if (mConfig.IsConvertStubJob)
                    {
                        var oldStubUrl = mConfig.GetConvertingStubFullUrl(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.serverRelativeUrl);
                        if (!bulkDeclareAndDeleteFileInfo.isSendedReport)
                        {
                            if (mConfig.isConvertSameTypeStub)
                            {
                                //var stubfile = mList.GetItemById(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.rowid).File;
                                var stubfile = mWeb.GetFile(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.serverRelativeUrl + "." + fileNameSUFFIX);
                                if (stubfile.Exists) RenameStubFile(stubfile, oldStubUrl, declareStubFile);
                            }
                            mConfig.DeleteStubFileRecordEntitiesInBatch(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id);
                            mConfig.JobReportDto.AddRecordReport(oldStubUrl, ConvertStubAction.Delete, JobDetailsStatus.Successful);
                        }
                        continue;
                    }
                    if (mConfig.IsILMode)
                    {
                        string md5 = mAllJobStatus[multiImportParameter.ListId].ContainsReport(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id) ? mAllJobStatus[multiImportParameter.ListId].GetReport(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id).Md5 : string.Empty;
                        UpdateExploreDB(multiImportParameter.Site, new Guid(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id), mConfig.actionType == ActionType.ArchchiveToStorage ? 8 : 2, pathMd5: md5);
                    }
                    if (bulkDeclareAndDeleteFileInfo.hasErrorNode)
                    {
                        //mConfig.soArchiverQueryWorkerForDel.UpdateStatus(SOApproveDBStatus.Failed, new Guid(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id), 50000, infos.FirstOrDefault().subjobid);
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Warn("An error occur when post-processing item, error is :{0}", e.ToString());
                mConfig.JobReportDto.HasErrorNode = true;
            }
        }

        private void AddStubFileRecordMapping(Guid webId, Guid listId, ARRestoreFileInfo fileInfo)
        {
            try
            {
                var stubFileRecord = new StubFileRecordDto()
                {
                    //SiteCollectionID = mConfig.SiteCollectionID,
                    ArchivedItemId = new Guid(fileInfo.id),
                    RefDateTime = DateTime.UtcNow,
                    //StubTemplateId = mConfig.currentRule.StubTemplateId,
                    StubId = fileInfo.StubId,
                    //StubType = mConfig.currentRule.LeaveStubType,
                    ArchivedFileFullPath = fileInfo.serverRelativeUrl,
                    ListId = listId,
                    WebId = webId,
                };
                mConfig.AddStubFileRecord(stubFileRecord);
            }
            catch (Exception e)
            {
                mLog.Warn("AddStubFileRecordMapping error {0}", e);
            }
        }

        private bool CheckItemHasModifiedAfterBackup(IAveListItem item, long archiverModifiedTime, long archiverTimeLastModified)
        {
            try
            {
                DateTime modifiedTime = (DateTime)item.FieldValues["Modified"];
                long modifiedTimeToleranceTicks = TimeSpan.FromSeconds(5).Ticks;

                if (archiverTimeLastModified > 0 && archiverTimeLastModified+ modifiedTimeToleranceTicks < modifiedTime.Ticks)
                {
                    mLog.Error($"Success repeat time statistic error result to unable leave stub, modifiedTime:{modifiedTime.Ticks}, archiverModifiedTime:{archiverModifiedTime}, timeLastModified:{archiverTimeLastModified}");
                }

                if(archiverModifiedTime <= 0)
                {
                    mLog.Error($"archiverModifiedTime is 0, modifiedTime:{modifiedTime.Ticks}, archiverModifiedTime:{archiverModifiedTime}, timeLastModified:{archiverTimeLastModified}");
                    return true;
                }
                else if (archiverModifiedTime > 0 && archiverModifiedTime+ modifiedTimeToleranceTicks < modifiedTime.Ticks)
                {
                    mLog.Warn($"one by one stub current doc has modifed,can not deleted it,archiver modified time:{archiverModifiedTime},modfied time:{modifiedTime.Ticks}, timeLastModified:{archiverTimeLastModified}");
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                mLog.Error($"one by one stub CheckItemHasModifiedAfterBackup failed, error:{ex}");
                return true;
            }
        }


        private void SendDestructionReport(IAveSite site, BulkDeclareAndDeleteFileInfo bulkDeclareAndDeleteFileInfo)
        {
            try
            {
                //for performance, SO disposal job skip SendDestructionReport.
                if (mConfig != null && mConfig.IsILMode)
                {
                    mLog.Info($"Begin Send destruction report, doc id:{bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id}");
                    var docId = new Guid(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id);
                    var record = GetRecordInExplorerDao(site, docId);
                    var versionInfos = ScanDBOperationFactory.GetScanDBOperation(mConfig).SelectItemVersionsWithJsonMeta(mConfig.currentRule.Id, new Guid(bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id));
                    foreach (var info in versionInfos)
                    {
                        DestructionReport destructionReport = new()
                        {
                            NodeId = info.NodeId,
                            ListId = info.ListID,
                            FullPath = info.FullPath,
                            RuleID = new Guid(mConfig.currentRule.Id),
                            ArchivedTime = DateTime.UtcNow.Ticks,
                            SortTicks = Snowflake.Instance().GetTicks().ToString(),
                            JsonMeta = info.JsonMeta,
                            ActionType = (int)mConfig.actionType
                        };
                        if (mConfig.IsOneDriverSite && record != null)
                        {
                            var jsonMeta = JsonConvert.DeserializeObject<ArchiverSharePointDto>(destructionReport.JsonMeta);
                            jsonMeta.OnedriveTermName = record.TermName;
                            destructionReport.JsonMeta = JsonConvert.SerializeObject(jsonMeta);
                        }
                        AddToDestructionCache(destructionReport);
                    }
                    mLog.Info($"Finish Send destruction report, doc id:{bulkDeclareAndDeleteFileInfo.mARRestoreFileInfo.id}");
                }
            }
            catch (Exception e)
            {
                mLog.Warn($"SendDestructionReport error {e}");
            }
        }

        private void AddToDestructionCache(DestructionReport destructionReport)
        {
            if (destructionReport == null)
            {
                return;
            }
            DestructionFactory.GetInstance(mConfig.SiteCollectionID.ToString(), mConfig.JobId).InsertValueToDB(new List<DestructionReport>() { destructionReport });
        }
        private void DeleteComplianceTagIfEnableRemove(IAveListItem listItem, ListItemComplianceInfo complianceInfo, out bool deletedComplianceTag)
        {
            deletedComplianceTag = false;
            try
            {
                if (!string.IsNullOrWhiteSpace(complianceInfo?.ComplianceTag))
                {
                    if (mConfig.currentRule.IncludeDeleteRecordLabel)
                    {
                        var isRecordTypeLabel = IsRecordTypeComplianceTag(listItem?.Web?.Site, complianceInfo.ComplianceTag);
                        if (isRecordTypeLabel)
                        {
                            mLog.Info("Current label is record label and the rule is include the record label");
                            if (complianceInfo.TagPolicyHold && !complianceInfo.TagPolicyRecord && isRecordTypeLabel)
                            {
                                mLog.Info("Current status of label is unlock status. So start lock label before remove the current label");
                                listItem?.LockRecordItem();
                            }
                            listItem?.SetComplianceTagOnBulkItems(""); 
                            deletedComplianceTag = true;
                        }
                    }
                    if (WrapperConfiguration.EnableRemoveRetentionLabel ||
                        ((mConfig.currentRule.KeepDataOption & (int)KeepDataOption.IsEnableRemoveRetentionLabel) == (int)KeepDataOption.IsEnableRemoveRetentionLabel))
                    {
                        if (!IsRecordTypeComplianceTag(listItem?.Web?.Site, complianceInfo.ComplianceTag))
                        {
                            mLog.Info("Current label is not record label and the rule is enable remove retention label");
                            listItem?.SetComplianceTagOnBulkItems("");
                            deletedComplianceTag = true;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Error($"Fail delete retention label,error message:{e.Message},error:{e}");
            }
        }

        private bool SetComplianceTagIfEnableRemove(IAveListItem listItem, ListItemComplianceInfo complianceInfo)
        {
            if (!string.IsNullOrWhiteSpace(complianceInfo?.ComplianceTag) &&
                (WrapperConfiguration.EnableRemoveRetentionLabel ||
                (mConfig.currentRule.KeepDataOption & (int)KeepDataOption.IsEnableRemoveRetentionLabel) == (int)KeepDataOption.IsEnableRemoveRetentionLabel))
            {
                try
                {
                    listItem.SetComplianceTagOnBulkItems(complianceInfo.ComplianceTag);
                    if (mConfig.SharePointRetentionLabel == null)
                    {
                        mConfig.InitRetentionLabelCollections(listItem.Web.Site);
                    }
                    if (mConfig.SharePointRetentionLabel.TryGetValue(complianceInfo.ComplianceTag, out AveComplianceTagInfo aveComplianceTagInfo))
                    {
                        if (aveComplianceTagInfo.UnlockedAsDefault && complianceInfo.TagPolicyHold && complianceInfo.TagPolicyRecord && IsRecordTypeComplianceTag(listItem.Web.Site, complianceInfo.ComplianceTag))
                        {
                            listItem.LockRecordItem();
                        }
                    }
                    else
                    {
                        mLog.Warn($"can not get compliance init lock status, compliane name :{complianceInfo.ComplianceTag}");
                    }
                    return true;
                }
                catch (Exception ex)
                {
                    mLog.Error($"Fail set retention label,label:{complianceInfo.ComplianceTag},error message:{ex.Message},error:{ex}");
                    throw;
                }
            }
            return false;
        }

        private bool GetComplianceTagIfEnableRemove(IAveListItem listItem, out ListItemComplianceInfo complianceInfo)
        {
            try
            {
                complianceInfo = null;
                string retentionLabel = listItem.GetComplianceTagName();
                if (!string.IsNullOrWhiteSpace(retentionLabel) &&
                    (WrapperConfiguration.EnableRemoveRetentionLabel ||
                    mConfig.currentRule.IncludeDeleteRecordLabel ||
                    (mConfig.currentRule.KeepDataOption & (int)KeepDataOption.IsEnableRemoveRetentionLabel) == (int)KeepDataOption.IsEnableRemoveRetentionLabel))
                {
                    var nowComplianceInfo = listItem.GetComplianceInfo(false);
                    if (string.IsNullOrWhiteSpace(nowComplianceInfo?.ComplianceTag))
                    {
                        return false;
                    }
                    complianceInfo = new ListItemComplianceInfo()
                    {
                        ComplianceTag = nowComplianceInfo.ComplianceTag,
                        TagPolicyHold = nowComplianceInfo.TagPolicyHold,
                        TagPolicyEventBased = nowComplianceInfo.TagPolicyEventBased,
                        TagPolicyRecord = nowComplianceInfo.TagPolicyRecord
                    };
                    return true;
                }
                return false;
            }
            catch (Exception e)
            {
                mLog.Error($"fail get complianceTag, item:{listItem.Url},Exception:{e}");
                throw;
            }
        }

        protected bool IsRecordTypeComplianceTag(IAveSite site, string complianceTagName)
        {
            try
            {
                if (mConfig.SharePointRetentionLabel == null)
                {
                    mConfig.InitRetentionLabelCollections(site);
                }
                if (mConfig.SharePointRetentionLabel.TryGetValue(complianceTagName, out AveComplianceTagInfo info))
                {
                    if (info.BlockDelete && info.BlockEdit)
                    {
                        return true;
                    }
                }
                else
                {
                    mLog.Warn($"Unable get complianceTag info from site avaliable compliance tags by tag name:{complianceTagName}, site url:{site.Url}");
                }
                return false;
            }
            catch (Exception ex)
            {
                mLog.Error($"Fail get complianceTag info from site avaliable compliance tags by tag name:{complianceTagName}, site url:{site.Url}, ex:{ex}");
                throw;
            }
        }
        private void DeleteFile(IAveFile file)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("SP2013ArchiveBackUp.DeleteFile"))
            {
                IAveListItem listItem = null;
                ListItemComplianceInfo complianceInfo = null;
                bool needRestoreComplianceTag = false;
                try
                {
                    listItem = file.Item;
                    GetComplianceTagIfEnableRemove(listItem, out complianceInfo);
                    if (complianceInfo != null && complianceInfo.TagPolicyHold && !complianceInfo.TagPolicyRecord && IsRecordTypeComplianceTag(listItem?.Web?.Site, complianceInfo.ComplianceTag))
                    {
                        throw new Exception("StorageOptimization_Skip_Unlock_Status_Item");
                    }
                    #region delete label file
                    if (mConfig.IsILMode
                        && file.Item.Fields.ContainsField("Retention label")
                        && RecordsDBOperation.RMEXOLabels.Where(
                            x => x.LabelName == file.Item["Retention label"].ToString()
                            && x.Status == 1 && x.Type == 1).FirstOrDefault() != null)
                    {
                        mLog.Info("Current file is label file and Records remove label and delete.FileName:{0}.", file.UniqueId);
                        //file.Item.SetComplianceTag(string.Empty, false, false, false, false, false);
                        file.Item.SetComplianceTagOnBulkItems(string.Empty);
                    }
                    #endregion

                    #region delete declare document for office 365
                    if ((mConfig.BackgroundSettings.IsDeleteRecord || mConfig.currentRule.DeleteRecords || (mConfig.IsILMode && ArchiverCommonStaticMethod.IsBlockDeleteOnlyRecord(listItem)))
                        && ArchiverCommonStaticMethod.CheckisRecord(listItem))
                    {
                        mLog.Info("Office365 Begin CheckFileIsRecord");
                        if (ArchiverCommonStaticMethod.CheckIsRecordOnly(listItem))
                        {
                            mLog.Info("This File is Declare File.FileName:{0}", file.UniqueId);
                            mConfig.aveObjectModelFactory.CreateRecords().UndeclareItemAsRecord(listItem);
                        }
                        else
                        {
                            mLog.Warn("This File is Declare And Hold File.FileName:{0}", file.UniqueId);
                            mConfig.JobReportDto.HasErrorNode = true;
                            throw new Exception("This File is Declare And Hold File.");
                        }
                    }
                    #endregion

                    DeleteComplianceTagIfEnableRemove(listItem, complianceInfo, out needRestoreComplianceTag);
                    try
                    {
                        file.Delete();
                        needRestoreComplianceTag = false;
                        mLog.Info("Delete the file successfully. {0}", file.ServerRelativeUrl);
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
                                file = mConfig.aveObjectModelFactory.CreateSite(file.ParentFolder.ParentList.ParentWeb.Site.Url).OpenWeb(file.ParentFolder.ParentList.ParentWeb.ID).GetFile(file.UniqueId, file.ServerRelativeUrl);
                                if (file.Exists)
                                {
                                    file.Delete();
                                    needRestoreComplianceTag = false;
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
                            || file.CheckOutType != AveCheckOutType.None
                            ))
                        {
                            mLog.Info($"Current file is check out file and Records check in and delete.FileName:{file.Name}.checkOutType:{file.CheckOutType}");
                            file.CheckIn("");
                            file.Delete();
                            needRestoreComplianceTag = false;
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
                            needRestoreComplianceTag = false;
                            mLog.Info("Delete hold file success.File name:{0}", file.Name);
                            return;
                        }
                        #endregion
                        else
                        {
                            file = file.ParentFolder.ParentList.ParentWeb.GetFile(file.UniqueId, file.ServerRelativeUrl);
                            file.Delete();
                            needRestoreComplianceTag = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    mLog.Debug("File Delete Error: {0} error message: {1}", file.Name, ex.ToString());
                    if (needRestoreComplianceTag)
                    {
                        SetComplianceTagIfEnableRemove(listItem, complianceInfo);
                    }
                    throw;
                }
            }
        }

        private void DeleteStubFile(IAveFile file)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("SP2013ArchiveBackUp.DeleteStubFile"))
            {
                try
                {
                    IAveListItem listItem = file.Item;
                    if (ArchiverCommonStaticMethod.CheckisRecord(listItem))
                    {
                        mLog.Info("This Stub File is Declare File.FileName:{0}", file.UniqueId);
                        mConfig.aveObjectModelFactory.CreateRecords().UndeclareItemAsRecord(listItem);
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
                                file = mConfig.aveObjectModelFactory.CreateSite(file.ParentFolder.ParentList.ParentWeb.Site.Url).OpenWeb(file.ParentFolder.ParentList.ParentWeb.ID).GetFile(file.UniqueId, file.ServerRelativeUrl);
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
                            || file.CheckOutType != AveCheckOutType.None
                            ))
                        {
                            mLog.Info($"Current file is check out file and Records check in and delete.FileName:{file.Name}.checkOutType:{file.CheckOutType}");
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

        private void RenameStubFile(IAveFile file, string newUrl, bool isDeclareStubFile)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("SP2013ArchiveBackUp.DeleteStubFile"))
            {
                try
                {
                    IAveListItem listItem = file.Item;
                    var editor = listItem.FieldValues["Editor"].ToString();
                    var modified = (DateTime)listItem.FieldValues["Modified"];
                    file.MoveToKeepEditor(newUrl, editor, modified, AveMoveOperations.None);
                    if (isDeclareStubFile)
                    {
                        listItem = listItem.ParentList.GetItemById(listItem.ID);
                        mConfig.aveObjectModelFactory.CreateRecords().DeclareItemAsRecord(listItem);
                    }
                }
                catch (Exception ex)
                {
                    mLog.Debug("Stub File Update Error: {0} error message: {1}", file.Name, ex.ToString());
                    if (ex.Message.Contains("read-only") || ex.Message.Contains("locked"))
                    {
                        try
                        {
                            IAveListItem listItem = file.Item;
                            var editor = listItem.FieldValues["Editor"].ToString();
                            var modified = (DateTime)listItem.FieldValues["Modified"];

                            mConfig.aveObjectModelFactory.CreateRecords().UndeclareItemAsRecord(listItem);
                            file.MoveToKeepEditor(newUrl, editor, modified, AveMoveOperations.None);
                            if (isDeclareStubFile)
                            {
                                listItem = listItem.ParentList.GetItemById(listItem.ID);
                                mConfig.aveObjectModelFactory.CreateRecords().DeclareItemAsRecord(listItem);
                            }
                        }
                        catch (Exception e)
                        {
                            mLog.Debug("Stub File Retry Update Error: {0} error message: {1}", file.Name, ex.ToString());
                        }
                    }
                }
            }
        }

        private void UpdateExploreDB(IAveSite mSite, Guid nodeID, int updateStatus, Record addRecord = null, string pathMd5 = null)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.UpdateExploreDB"))
            {
                Guid recordID = ArchiverCommonStaticMethod.GetRecordId(mSite.ID, nodeID);

                if (mConfig.IsILMode && mConfig.ExplorerDao != null)
                {
                    try
                    {
                        Record record = null;
                        if (mConfig.exploreDBSPRecords.Count > 0)
                        {
                            //CosmosDB数据先从缓存中读取，默认缓存 1w条，如果超过再从DB中读取
                            if (mConfig.exploreDBSPRecords.Where(x => x.ScopeId == mSite.ID && x.Id == recordID).FirstOrDefault() != null)
                            {
                                record = mConfig.exploreDBSPRecords.Where(x => x.ScopeId == mSite.ID && x.Id == recordID).FirstOrDefault();
                            }
                            else
                            {
                                if (mConfig.exploreDBSPRecords.Count >= 10000)
                                {
                                    record = mConfig.ExplorerDao.ReadById(mSite.ID, recordID);
                                }
                                else
                                {
                                    mLog.Info("Current object:{0} doesn't exist in explore by Cache.", recordID);
                                }
                            }
                        }
                        else
                        {
                            record = mConfig.ExplorerDao.ReadById(mSite.ID, recordID);
                        }
                        if (record != null)
                        {
                            if (mConfig.currentRule.IsManualApproval)
                            {
                                AddManualHistory(record);
                                if (updateStatus == (int)RMRecordStatus.Archived && record.RecordStatus == (int)RMRecordStatus.ManualPreSync)
                                {  
                                    //unsync data,keep status when archiver
                                    mConfig.ExplorerDao.UpdateRecordStatusAndDestroyedTime4Manual(mSite.ID, recordID, (int)RMRecordStatus.ManualPreSync);
                                }
                                else
                                {
                                    mConfig.ExplorerDao.UpdateRecordStatusAndDestroyedTime4Manual(mSite.ID, recordID, updateStatus);
                                }
                                //mConfig.ExplorerDao.UpdateRecordStatusAndDestroyedTime4Manual(mSite.ID, recordID, updateStatus);
                            }
                            else
                            {
                                mConfig.ExplorerDao.UpdateRecordStatusAndDestroyedTime(mSite.ID, recordID, updateStatus);
                            }
                            mLog.Info("Update Record Status successful");
                            if (updateStatus == 8)
                            {
                                ArchiverIndexDto indexDto = null; ;
                                try
                                {
                                    indexDto = Convert2ArchiverIndexDto(mConfig.GetArchiverIndex(pathMd5));
                                }
                                catch (Exception e)
                                {
                                    mLog.Error($"Error occurred while getting archiver index. Node id:{nodeID} Error:{e.ToString()}");
                                }
                                //ecord.AppendMetaInfoForArchiverIndex(SerializerHelper.SerializeByDataContractJsonSerializer(indexDto));
                                mConfig.ExplorerDao.AddArchivedRelatedColumn(mSite.ID, recordID, pathMd5, mConfig.CurrentIndexJobID, indexDto != null ? SerializerHelper.SerializeByDataContractJsonSerializer(indexDto) : null);
                                mLog.Info("Update archived custom column successful");
                            }
                        }
                        else
                        {
                            mLog.Info("Current object:{0} doesn't exist in explore.", recordID);
                            if (addRecord != null)
                            {
                                //RECO-9707 if site collection doesn't exist in explorer, add it
                                addRecord.DestroyedTime = DateTime.UtcNow.Ticks;
                                mConfig.ExplorerDao.Add(addRecord);
                                mLog.Info("Add object:{0} in explorer.", addRecord.NodeId);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Update Record Status Failed.Message:{0}.", ex.ToString());
                    }
                }
            }
        }

        private Record GetRecordInExplorerDao(IAveSite mSite, Guid nodeID)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("ArchiveBackUp.UpdateExploreDB"))
            {
                Guid recordID = ScheduleConfiguration.GetRecordId(mSite.ID, nodeID);
                if (mConfig.IsILMode && mConfig.ExplorerDao != null)
                {
                    try
                    {
                        Record record = null;
                        if (mConfig.IsRelativeDataJob)
                        {
                            record = mConfig.ExplorerDao.ReadById(mSite.ID, recordID);
                        }
                        else
                        {
                            record = mConfig.exploreDBSPRecords.Where(x => x.ScopeId == mSite.ID && x.Id == recordID).FirstOrDefault();
                            if (record == null && mConfig.exploreDBSPRecords.Count >= 10000)
                            {
                                record = mConfig.ExplorerDao.ReadById(mSite.ID, recordID);
                            }
                        }
                        return record;
                    }
                    catch (Exception ex)
                    {
                        mLog.Warn("Update Record Status Failed.Message:{0}.", ex.ToString());
                        return null;
                    }
                }
                else
                { return null; }
            }
        }
        private void AddManualHistory(Record record)
        {
            ManualUtil manualUtil = new ManualUtil(mConfig);
            manualUtil.AddManualHistory(record);
        }

        private ArchiverIndexDto Convert2ArchiverIndexDto(ArchiverBasicIndex index)
        {
            ArchiverIndexDto dto = new ArchiverIndexDto()
            {
                ContentDataFileNumber = index.ContentDataFileNumber,
                ContentDataHeaderOffset = index.ContentDataHeaderOffset,
                ContentDataOffset = index.ContentDataOffset,
                ContentLength = index.ContentLength,
                ContentOffset = index.ContentOffset,
                ContentPageSize = index.ContentPageSize,
                DataFileLength = index.DataFileLength,
                DataFileNumber = index.DataFileNumber,
                DataFileOffset = index.DataFileOffset,
                Flag = index.Flag,
                JobId = index.JobId,
                MetaDataHeaderOffset = index.MetaDataHeaderOffset,
                Version = index.Version
            };
            return dto;
        }
    }
}
