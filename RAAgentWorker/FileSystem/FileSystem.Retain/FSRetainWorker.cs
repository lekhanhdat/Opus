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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.Job;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.Media.Service;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Storage;
using AvePoint.Media.Storage.Util;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Global.Exceptions;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Telemetry;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using DocAveOnline.WebApi.Contracts;
using log4net;
using Newtonsoft.Json;
using NVelocity.Util.Introspection;
using RAFileSystem.FileSystem.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.FileSystem.Retain
{
    public class FSRetainWorker
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public IIndexService<ArchiverIndexServiceOpenParameter> IndexService = new ArchiverIndexService();
        private IProgressService ProgressService { get; set; }
        public IArchiverBackupIndexService BackupIndexService = new ArchiverBackupIndexService();
        public CacheSettingDto CacheSetting { get; set; }
        public CacheSettingDto RestoredPathSetting { get; set; }
        private IReportService<JMJobDetails> JobDetailService { get; set; }
        public LogicalDeviceDto IndexLogicalDevice { get; set; }
        public List<LogicalDeviceDto> DataLogicalDeviceList { get; set; }
        public LogicalDeviceDto LogicalDeviceDto { get; set; }
        public IXSystem indexLogicalDevice;
        public IXSystem dataLogicalDevice;
        //public String ConnectionName { get; set; }
        //public String ConnectionId { get; set; }
        public String IndexVolume { get; set; }
        public String DataVolume { get; set; }
        private string rehydrationTemp;
        public List<ArchiverPruningJob> archiverPruningJobs { get; set; }
        //public List<RestoreSecurityInfoWrapper> restoreSecurityInfos { get; set; }
        public IStorageDeviceManager StorageDeviceManager = new StorageDeviceManager();
        public ICacheService CacheManager = new CacheService();
        public ICacheService RestoreLocationManager = new CacheService();
        public IDataReader<ArchiverRestoreJob> DataReader = new ArchiverRestoreDataReader();
        public IEncryptionInfoManager EncryptionInfoManager = new EncryptionInfoManager();
        public ArchiverIndexService _ArchiverIndexService = new ArchiverIndexService();
        public FSRetainWorker(List<ArchiverPruningJob> info)
        {
            archiverPruningJobs = info;
            ProgressService = JobContext.Current.mProgressManager.Create();
            JobDetailService = JobContext.Current.JobDetailManager.Create();
            DataLogicalDeviceList = new List<LogicalDeviceDto>();
            LogicalDeviceDto = new LogicalDeviceDto() { PhysicalDrives = new List<PhysicalDeviceDto>() };
        }
        public void RunRetainJob()
        {
            try
            {
                var dateTimeNow = DateTime.UtcNow;
                foreach (var archiverPruningJob in archiverPruningJobs)
                {
                    if (archiverPruningJob.IsSimulateJob)
                    {
                        dateTimeNow = new DateTime(archiverPruningJob.SimulateJobRunTime, DateTimeKind.Utc);
                        if (archiverPruningJob.RetentionAction != MediaArchiverRetentionAction.DeleteData)
                        {
                            logger.Info($"this is a simulate job, skip other RetentionAction: {archiverPruningJob.RetentionAction}.");
                            continue;
                        }
                    }

                    RARetentionJobTelemetry telemetry = BuildRARetentionJobTelemetry(archiverPruningJob);
                    using (var pc1 = new AgentPerformanceScope("FSDocumentDisposal.OneArchiverPruningJob", addToStatistics: true))
                    {
                        logger.Info($"this archiver pruning job:{archiverPruningJob.JobId},retention data type:{archiverPruningJob.RetentionDataTimeType}");
                        if (archiverPruningJob.RetentionDataTimeType == KeepDateType.None)
                        {
                            logger.Warn($"this retention info is old data info,need to reset it to archive time.");
                            archiverPruningJob.RetentionDataTimeType = KeepDateType.ArchiveTime;
                        }
                        archiverPruningJob.DateTimeNowTicks = ValidateModifiedTime(archiverPruningJob.KeepValue, archiverPruningJob.ArchiveDateUnit, dateTimeNow);
                        try
                        {
                            if (archiverPruningJob.CacheSettings == null)
                            {
                                var ArchiveTemp = BackgroundSettings.GetInstance().ArchiveTemp;
                                if (!System.IO.Directory.Exists(ArchiveTemp))
                                {
                                    System.IO.Directory.CreateDirectory(ArchiveTemp);
                                }
                                archiverPruningJob.CacheSettings = InitCacheSetting(ArchiveTemp);
                                if (archiverPruningJob.RetentionJob != null)
                                {
                                    archiverPruningJob.RetentionJob.Id = JobContext.Current.JobId;
                                }
                            }
                            logger.Info($"Start to run retention job for sub job id:{JobContext.Current.JobId}, retention action:{archiverPruningJob.RetentionAction}, is simulate job:{archiverPruningJob.IsSimulateJob}");
                            CheckIfEnableMoveToAnotherLocation(archiverPruningJob);
                            switch (archiverPruningJob.RetentionAction)
                            {
                                case MediaArchiverRetentionAction.DeleteData:
                                    DeleteSubJobData(archiverPruningJob);
                                    break;
                                case MediaArchiverRetentionAction.MoveData:
                                    ExportDataToAnotherDeviceAsync(archiverPruningJob).GetAwaiter().GetResult();
                                    break;
                                case MediaArchiverRetentionAction.MarkTier:
                                    MarkSubJobDataTier(archiverPruningJob);
                                    break;
                                default:
                                    throw new NotSupportedException("Current archiver retention action is not supported.");
                            }
                        }
                        catch (JobStopException)
                        {
                            logger.Error("Job will stop, Catch JobStopException.");
                            FSJobCache.RestoreInstance.FailedCount++;
                            //mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Stopped;
                        }
                        catch (Exception e)
                        {
                            //mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.FinishWithException;
                            FSJobCache.RestoreInstance.FailedCount++;
                            logger.Error("Error :{0}", e.ToString());
                        }
                    }
                    EndAndSendRetentionJobTelelmetry(telemetry);
                }
            }
            finally
            {
            }
        }

        private RARetentionJobTelemetry BuildRARetentionJobTelemetry(ArchiverPruningJob archiverPruningJob)
        {
            RARetentionJobTelemetry telemetry = new RARetentionJobTelemetry();
            try
            {
                telemetry.JobId = JobContext.Current.JobId;
                telemetry.MainJobId = JobContext.Current?.JobId?.Split('_')?.FirstOrDefault();
                telemetry.RetentionObject = archiverPruningJob.SiteUrl + ";" + archiverPruningJob.UNCPath;
                telemetry.ArchivedSubJobId = archiverPruningJob.JobId;
                telemetry.JobType = (8060).ToString();//JobType.FSRetain
                telemetry.StorageName = archiverPruningJob?.DataLogicalDevice?.Name;
                telemetry.RetentionAction = (int)archiverPruningJob.RetentionAction;
                ArchiverIndexSubInfoContract indexSubInfo = HybridApiClient.Instance.GetFSIndexSubinfoBySubsubJobId(archiverPruningJob?.JobId);
                telemetry.MediaDataSize = indexSubInfo?.MediaDataSize ?? 0;
            }
            catch (Exception e)
            {
                logger.Error("Fail build telemetry object :{0}", e.ToString());
            }
            return telemetry;
        }

        private void EndAndSendRetentionJobTelelmetry(RARetentionJobTelemetry telemetry)
        {
            try
            {
                ArchiverIndexSubInfoContract indexSubInfo = HybridApiClient.Instance.GetFSIndexSubinfoBySubsubJobId(telemetry.ArchivedSubJobId);
                telemetry.RemainingMediaDataSize = indexSubInfo?.MediaDataSize ?? 0;
                telemetry.RetentionDataSize = telemetry.MediaDataSize - telemetry.RemainingMediaDataSize;
                HybridApiClient.Instance.AddTelemetryForRetentionJob(telemetry);
            }
            catch (Exception e)
            {
                logger.Error($@"Fail end and send telemetry, ex:{e}");
            }
        }


        public void MarkSubJobDataTier(ArchiverPruningJob archiverRetentionInfo)
        {
            using (var pc1 = new AgentPerformanceScope("FSRetain.MarkSubJobDataTier", addToStatistics: true))
            {
                logger.Info($"MarkSubJobDataTier : SiteUrl:{archiverRetentionInfo.SiteUrl.LogBase64()}");
                ArchiverRetentionResult result = null;
                string errorMessage = string.Empty;
                try
                {
                    var retentionInfo = new ArchiverRetentionInfo(archiverRetentionInfo);
                    retentionInfo.RetentionRule = AvePoint.Media.Service.DomainModel.RetentionRule.MarkArchiverJobDataTier;
                    FSRetainActionWorker worker = new FSRetainActionWorker(JobDetailService);
                    result = worker.InternalRetain(retentionInfo);
                }
                catch (Exception e)
                {
                    logger.Error($"MarkSubJobDataTier Error. {e.ToString()}");
                    errorMessage = e.Message;
                }
                if (result != null && result.State == 2)
                {
                    try
                    {
                        //更新sub master index表中logical device id和retention time
                        var subIndex = HybridApiClient.Instance.GetFSIndexSubinfoBySubsubJobId(archiverRetentionInfo.JobId);
                        if (subIndex != null)
                        {
                            logger.Info("Move data {0}, update retention time.", archiverRetentionInfo.JobId);
                            subIndex.RetentionTime = DateTime.UtcNow.Ticks;
                            subIndex.RetentionCount++;
                            HybridApiClient.Instance.UpdateFSIndexSubInfo(subIndex);
                            logger.Info("Update sub master index successful");
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn(e.ToString());
                        result.State = 1;
                    }
                    var report = new JMFSRetainJobDetails();
                    report.SiteUrl = archiverRetentionInfo.UNCPath;
                    report.Size = string.Empty;//result.Size.ToString();
                    report.Status = JobDetailsStatus.Successful;
                    report.SrcStorageName = archiverRetentionInfo.DataLogicalDevice.Name;
                    report.JobId = archiverRetentionInfo.JobId;
                    report.Action = "RM_AR_CP_GSS_Retention_MarkDataTier";
                    report.Comment = result.IsArchiveTierToColdTier? "RM_AR_CP_GSS_Retention_ChangeToColdFromArchive":string.Empty;
                    JobDetailService.Commit(report);
                }
                else
                {
                    var report = new JMFSRetainJobDetails();
                    report.SiteUrl = archiverRetentionInfo.UNCPath;
                    report.Size = string.Empty;//"0";
                    report.Status = JobDetailsStatus.Failed;
                    report.JobId = archiverRetentionInfo.JobId;
                    report.Comment = errorMessage;
                    report.Action = "RM_AR_CP_GSS_Retention_MarkDataTier";
                    JobDetailService.Commit(report);

                }
            }
        }
        public async Task ExportDataToAnotherDeviceAsync(ArchiverPruningJob archiverRetentionInfo)
        {
            logger.Info($"ExportDataToAnotherDevice : SiteUrl:{archiverRetentionInfo.SiteUrl.LogBase64()}");
            using (var pc1 = new AgentPerformanceScope("FSRetain.ExportDataToAnotherDeviceAsync", addToStatistics: true))
            {
                ArchiverRetentionResult result = null;
                try
                {
                    var retentionInfo = new ArchiverRetentionInfo(archiverRetentionInfo);
                    retentionInfo.RetentionRule = AvePoint.Media.Service.DomainModel.RetentionRule.MoveArchiverJobData;
                    FSRetainActionWorker worker = new FSRetainActionWorker(JobDetailService);
                    result = worker.InternalRetain(retentionInfo);
                }
                catch (JobStopException e)
                {
                    logger.Warn("Job will stop, throw JobStopException.");
                    throw;
                }
                catch (Exception e)
                {
                    FSJobCache.RestoreInstance.FailedCount++;
                    logger.Error($"ExportDataToAnotherDevice Error. {e.ToString()}");
                }
                if (result != null && result.State == 2)
                {
                    try
                    {
                        //更新sub master index表中logical device id和retention time
                        var subIndex = HybridApiClient.Instance.GetFSIndexSubinfoBySubsubJobId(archiverRetentionInfo.JobId);
                        if (subIndex != null)
                        {
                            logger.Info("Move data {0}, update retention time.", archiverRetentionInfo.JobId);
                            subIndex.CurrentStorageId = archiverRetentionInfo.DestinationPhysicalDeviceId;
                            subIndex.RetentionTime = DateTime.UtcNow.Ticks;
                            subIndex.RetentionCount++;
                            HybridApiClient.Instance.UpdateFSIndexSubInfo(subIndex);
                            logger.Info("Update sub master index successful");
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn(e.ToString());
                        result.State = 1;
                    }
                }
                if (result != null && result.State == 2)
                {
                    var report = new JMFSRetainJobDetails();
                    report.SiteUrl = archiverRetentionInfo.UNCPath;
                    report.Size = result.Size.ToString();
                    report.Status = JobDetailsStatus.Successful;
                    report.SrcStorageName = archiverRetentionInfo.DataLogicalDevice.Name;
                    report.DesStorageName = archiverRetentionInfo.DestinationDevice?.Name;
                    report.JobId = archiverRetentionInfo.JobId;
                    report.Action = this.GetJobDetailsActionForMoveData(archiverRetentionInfo);
                    JobDetailService.Commit(report);
                }
                else if (result != null && result.State == 7)
                {
                    try
                    {
                        var subIndex = HybridApiClient.Instance.GetFSIndexSubinfoBySubsubJobId(archiverRetentionInfo.JobId);
                        if (subIndex != null)
                        {
                            logger.Info("skip Move data {0}, just update retention time.", archiverRetentionInfo.JobId);
                            subIndex.RetentionTime = DateTime.UtcNow.Ticks;
                            HybridApiClient.Instance.UpdateFSIndexSubInfo(subIndex);
                            logger.Info("Update skip move sub master index successful");
                            var report = new JMFSRetainJobDetails();
                            report.SiteUrl = archiverRetentionInfo.UNCPath;
                            report.Size = "0";
                            report.Status = JobDetailsStatus.Skipped;
                            report.JobId = archiverRetentionInfo.JobId;
                            report.Action = this.GetJobDetailsActionForMoveData(archiverRetentionInfo);
                            report.Comment = "RM_FS_Retain_MoveAction_NotSurpportAvepointStorage";
                            JobDetailService.Commit(report);
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn(e.ToString());
                        result.State = 1;
                    }
                }
                else
                {
                    var report = new JMFSRetainJobDetails();
                    report.SiteUrl = archiverRetentionInfo.UNCPath;
                    report.Size = "0";
                    report.Status = JobDetailsStatus.Failed;
                    report.JobId = archiverRetentionInfo.JobId;
                    report.Action = this.GetJobDetailsActionForMoveData(archiverRetentionInfo);
                    JobDetailService.Commit(report);
                }
            }
        }

        public void SimulateDeleteSubJobData(ArchiverPruningJob archiverRetentionInfo)
        {
            using (var pc1 = new AgentPerformanceScope("FSRetain.SimulateDeleteSubJobData", addToStatistics: true))
            {
                logger.Info($"DeleteSubJobData : SiteUrl:{archiverRetentionInfo.SiteUrl.LogBase64()}");
                ArchiverRetentionResult result = null;
                string backupSubJobId = archiverRetentionInfo.JobId.Substring(0, archiverRetentionInfo.JobId.LastIndexOf("_", StringComparison.CurrentCulture));
                try
                {
                    var retentionInfo = new ArchiverRetentionInfo(archiverRetentionInfo);
                    retentionInfo.RetentionRule = AvePoint.Media.Service.DomainModel.RetentionRule.RetainArchiverJobData;
                    FSRetainActionWorker worker = new FSRetainActionWorker(JobDetailService);
                    result = worker.InternalRetain(retentionInfo);
                }
                catch (Exception e)
                {
                    FSJobCache.RestoreInstance.FailedCount++;
                    logger.Error($"DeleteSubJobData Error. {e.ToString()}");
                }
 
                //UpdateArchiverSize(archiverRetentionInfo.SiteUrl);
                if (result != null && result.State == 2)
                {
                    //if (archiverRetentionInfo.RetentionDataTimeType != KeepDateType.ModifiedTime)
                    //{
                    //    var report = new JMFSRetainJobDetails();
                    //    report.SiteUrl = archiverRetentionInfo.UNCPath;
                    //    report.Size = result.Size.ToString();
                    //    report.Status = JobDetailsStatus.Successful;
                    //    report.SrcStorageName = archiverRetentionInfo.DataLogicalDevice.Name;
                    //    report.JobId = archiverRetentionInfo.JobId;
                    //    report.Action = "RM_JS_Common_Delete";
                    //    CommitReport(report, archiverRetentionInfo);
                    //}
                    //else
                    //{
                    //    logger.Info($"this retention job is retention by modified time,so not job level detail");
                    //}
                }
                else
                {
                    if (archiverRetentionInfo.RetentionDataTimeType != KeepDateType.ModifiedTime)
                    {

                        var report = new JMFSRetainJobDetails();
                        report.SiteUrl = archiverRetentionInfo.UNCPath;
                        report.Size = "0";
                        report.Status = JobDetailsStatus.Failed;
                        report.JobId = archiverRetentionInfo.JobId;
                        report.Action = "RM_JS_Common_Delete";
                        CommitReport(report, archiverRetentionInfo);
                    }
                    else
                    {
                        logger.Info($"this retention job is retention by modified time,so not job level detail and it has error");
                    }
                }
            }
        }

        public void DeleteSubJobData(ArchiverPruningJob archiverRetentionInfo)
        {
            if (archiverRetentionInfo.IsSimulateJob)
            {
                SimulateDeleteSubJobData(archiverRetentionInfo);
                return;
            }
            using (var pc1 = new AgentPerformanceScope("FSRetain.DeleteSubJobData", addToStatistics: true))
            {
                logger.Info($"DeleteSubJobData : SiteUrl:{archiverRetentionInfo.SiteUrl.LogBase64()}");
                ArchiverRetentionResult result = null;
                string backupSubJobId = archiverRetentionInfo.JobId.Substring(0, archiverRetentionInfo.JobId.LastIndexOf("_", StringComparison.CurrentCulture));
                try
                {
                    var retentionInfo = new ArchiverRetentionInfo(archiverRetentionInfo);
                    retentionInfo.RetentionRule = AvePoint.Media.Service.DomainModel.RetentionRule.RetainArchiverJobData;
                    FSRetainActionWorker worker = new FSRetainActionWorker(JobDetailService);
                    result = worker.InternalRetain(retentionInfo);
                }
                catch (Exception e)
                {
                    FSJobCache.RestoreInstance.FailedCount++;
                    logger.Error($"DeleteSubJobData Error. {e.ToString()}");
                }
                //删除sub info表记录, 如果主表对应的子表记录全部删除, 则删除主表记录.

                var subIndex = HybridApiClient.Instance.GetFSIndexSubinfoBySubsubJobId(archiverRetentionInfo.JobId);
                if (archiverRetentionInfo.RetentionDataTimeType != KeepDateType.ModifiedTime)
                {
                    logger.Info($"delete sub job will delete it,{subIndex?.JobId}");
                    HybridApiClient.Instance.DeleteFSIndexSubInfo(subIndex);
                }
                if (result != null && result.State == 2)
                {
                    try
                    {
                        var existSubInfo = HybridApiClient.Instance.ExistFSIndexSubInfoBySubJobId(backupSubJobId);
                        var job = HybridApiClient.Instance.GetMasterIndexBySubjobId(backupSubJobId);
                        if (!existSubInfo)
                        {
                            if (job != null)
                            {
                                logger.Info("Archiver Site Master Index with job id {0}, whose SubInfo is null or empty, has been deleted after retention job.", backupSubJobId);
                                HybridApiClient.Instance.DeleteFSMasterIndex(job);
                            }
                            else
                            {
                                logger.Info("Archiver Site Master Index with job id {0}, but cannot find in table.", backupSubJobId);
                            }
                        }
                        else
                        {
                            logger.Info("SubInfo count of sub job id {0} is {1}", backupSubJobId, existSubInfo);
                        }
                        //需要删除job
                        if (archiverRetentionInfo.IsDeleteJob)
                        {
                            logger.Info("Delete data {0}, delete sub info.", archiverRetentionInfo.JobId);
                            var jobMonitorJobId = backupSubJobId.Substring(0, archiverRetentionInfo.JobId.IndexOf("_", StringComparison.CurrentCulture));
                            HybridApiClient.Instance.DeleteJobById(jobMonitorJobId);
                            logger.Info("Delete job record successful");
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Warn(e.ToString());
                        result.State = 1;
                    }
                }
                //UpdateArchiverSize(archiverRetentionInfo.SiteUrl);
                if (result != null && result.State == 2)
                {
                    if (archiverRetentionInfo.RetentionDataTimeType != KeepDateType.ModifiedTime)
                    {
                        var report = new JMFSRetainJobDetails();
                        report.SiteUrl = archiverRetentionInfo.UNCPath;
                        report.Size = result.Size.ToString();
                        report.Status = JobDetailsStatus.Successful;
                        report.SrcStorageName = archiverRetentionInfo.DataLogicalDevice.Name;
                        report.JobId = archiverRetentionInfo.JobId;
                        report.Action = "RM_JS_Common_Delete";
                        CommitReport(report, archiverRetentionInfo);
                    }
                    else
                    {
                        logger.Info($"this retention job is retention by modified time,so not job level detail");
                    }
                }
                else
                {
                    if (archiverRetentionInfo.RetentionDataTimeType != KeepDateType.ModifiedTime)
                    {

                        var report = new JMFSRetainJobDetails();
                        report.SiteUrl = archiverRetentionInfo.UNCPath;
                        report.Size = "0";
                        report.Status = JobDetailsStatus.Failed;
                        report.JobId = archiverRetentionInfo.JobId;
                        report.Action = "RM_JS_Common_Delete";
                        CommitReport(report, archiverRetentionInfo);
                    }
                    else
                    {
                        logger.Info($"this retention job is retention by modified time,so not job level detail and it has error");
                    }
                }
            }
        }

        private void CommitReport(JMFSRetainJobDetails report, ArchiverPruningJob archiverRetentionInfo)
        {
            if (archiverRetentionInfo.IsSimulateJob)
            {
                JMFSRetainDashboardDetails dashboardReport = new JMFSRetainDashboardDetails(report)
                {
                    RetentionKeepDate = archiverRetentionInfo.KeepValue,
                    RetentionKeepDateUnit = (int)archiverRetentionInfo.ArchiveDateUnit,
                    RetentionSource = archiverRetentionInfo.RetentionSourceName,
                    SourceFlag = archiverRetentionInfo.SourceFlag,
                };
                JobDetailService.Commit(dashboardReport);
            }
            else
            {
                JobDetailService.Commit(report);
            }
        }


        private CacheSettingDto InitCacheSetting(string path)
        {
            string cachePath = path;
            CacheSettingDto cache = new CacheSettingDto();
            cache.Extension = new CacheSettingExtension();
            cache.Extension.Path = new List<PathMap>();
            cache.Extension.Path.Add(new PathMap() { DiskInfo = new DiskInfoDto() });
            cache.Extension.Path[0].DiskInfo.Path = cachePath;
            return cache;
        }
        private long ValidateModifiedTime(int keepValue, DateUnit dateUnit, DateTime dateTimeNow)
        {
            if (keepValue < 0)
            {
                logger.Info($"keep value is zero,return false");
                return 0;
            }
            DateTime DateTimeNow = dateTimeNow;
            switch (dateUnit)
            {
                case DateUnit.Year:
                    DateTimeNow = DateTimeNow.AddYears(-keepValue);
                    break;
                case DateUnit.Month:
                    DateTimeNow = DateTimeNow.AddMonths(-keepValue);
                    break;
                case DateUnit.Week:
                    DateTimeNow = DateTimeNow.AddDays(-keepValue * 7);
                    break;
                case DateUnit.Day:
                    DateTimeNow = DateTimeNow.AddDays(-keepValue);
                    break;
            }
            return DateTimeNow.Ticks;
        }

        /// <summary>
        /// Check if move to another location is enabled. If not, throw JobStopException to stop the job and report failure.
        /// </summary>
        /// <param name="archiverRetentionInfo"></param>
        /// <exception cref="JobStopException"></exception>
        private void CheckIfEnableMoveToAnotherLocation(ArchiverPruningJob archiverRetentionInfo)
        {
            bool isMoveAction = archiverRetentionInfo.RetentionAction == MediaArchiverRetentionAction.MoveData;
            if (!archiverRetentionInfo.HasMoveActionInPreviousRules && !isMoveAction)
            {
                return;
            }
            if (!archiverRetentionInfo.IsEnableMoveToAnotherLocation)
            {
                string errorMessage = isMoveAction
                    ? "Move to another location is disabled. Both 'EnableMoveToAnotherLocation' and 'EnableCopyToAnotherLocation' settings are disabled."
                    : "There is \"Move to another location\" option in previous retention rules, but both 'EnableMoveToAnotherLocation' and 'EnableCopyToAnotherLocation' settings are disabled.";
                logger.Error(errorMessage);
                var report = new JMFSRetainJobDetails
                {
                    SiteUrl = archiverRetentionInfo.UNCPath,
                    Size = "0",
                    Status = JobDetailsStatus.Failed,
                    JobId = archiverRetentionInfo.JobId,
                    Action = this.GetJobDetailsActionForMoveData(archiverRetentionInfo),
                    Comment = "RM_Retention_MoveToAnotherLocationDisabled",
                    SrcStorageName = archiverRetentionInfo.DataLogicalDevice?.Name ?? string.Empty,
                    DesStorageName = archiverRetentionInfo.DestinationDevice?.Name ?? string.Empty,
                };
                JobDetailService.Commit(report);
                throw new Exception("RM_Retention_MoveToAnotherLocationDisabled");
            }
        }

        private string GetJobDetailsActionForMoveData(ArchiverPruningJob archiverRetentionInfo)
        {
            return archiverRetentionInfo.IsEnableCopyToAnotherLocation ? "RM_JS_Common_Copy" : "RM_PRM_PRE_Move";
        }
    }
}
