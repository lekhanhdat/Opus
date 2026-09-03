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
using AvePoint.Media.Service;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.Archiver.Media;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Media.WcfService;
using AvePoint.GCommon.Contract.MediaManagement.Object;
using AvePoint.GCommon.Contract.Retention;
using AvePoint.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.Media.Service.ArchiverBackup;
using DocumentFormat.OpenXml.Wordprocessing;
using AvePoint.Media.Service.DomainModel;
using AvePoint.Media.Service.ArchiverBackup.Backup;
using RAArchiverCommon;
using AvePoint.Media.Common;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using Merged18NResources.Archive;
using AvePoint.RA.Contract.RMWeb;
using RetentionRule = AvePoint.GCommon.Contract.Storage.Entity.RetentionRule;
using AvePoint.RA.Contract.Explorer;
using Newtonsoft.Json;
using AvePoint.RA.Contract.Tenant;
using System.Data.SQLite;
using Storage;
using AvePoint.GCommon.Utility;
using AvePoint.RA.RACommonUtility.Lcoker;
using AvePoint.RA.DB.DBLocker;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Contract.RMWeb.Telemetry;
using AvePoint.RA.Contract.Telemetry;
using RAArchiverCommon.TeamsController;
using RAArchiverCommon.DisposalProgress.Impl;

namespace RAArchiverMaintenance
{
    public class TeamsArchiverRetentionJobHandler
    {
        private static IRALogger mLog = new RALogger(typeof(ArchiverRetentionJobHandler));
        private string SubJobId = string.Empty;
        private string MainJobId = string.Empty;
        private string JobContextSetting = string.Empty;
        private JobType mJobType;
        private AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Finished;
        private bool HasCompleteNode { get; set; }
        private bool HasErrorNode { get; set; }
        private bool HasStop { get; set; }

        private IJobMonitorService _JobMonitorService;
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService(ref _JobMonitorService);
        private IRMArchiveSiteInfoDao ArchiveSiteInfoDao = PlatformWindsorManager.GetService<IRMArchiveSiteInfoDao>();
        private ICommonSiteMasterIndexDao ArchiverSiteMasterIndexDao => PlatformWindsorManager.GetService<ICommonSiteMasterIndexDao>();
        private IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao => PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
        private IRetentionIndexSubInfoDao RetentionIndexSubInfoDao => PlatformWindsorManager.GetService<IRetentionIndexSubInfoDao>();
        private IRATelemetryService RATelemetryService => PlatformWindsorManager.GetService<IRATelemetryService>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IArchiverJobDao ArchiverJobDao => PlatformWindsorManager.GetService<IArchiverJobDao>();
        private string AVEPOINT_DEFAULT_STORAGEID = "6A040C17-AF8A-4F1F-96C1-7CEB2E23B1F3";
        private List<string> successDeleteSubsubJobIds = new List<string>();
        private List<string> failedDeleteSubsubJobIds = new List<string>();
        private IRMReportManager mReportManger;

        private IRMReportManager ReportManager
        {
            get
            {
                if (mReportManger == null)
                {
                    mReportManger = ReportMangerFactory.Instance.ReportManager;
                }
                return mReportManger;
            }
        }

        private IRMKeyValueDao mKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public TeamsArchiverRetentionJobHandler(string jobId, JobType jobType)
        {
            SubJobId = jobId;
            mJobType = jobType;
            ReportMangerFactory.Instance.Init(jobId, jobType, true);
            MediaEnvironment.MediaServer = MediaServiceFactory.CreateMediaServer();
            MediaConfigInfo.CommonConfigInfo = MediaServiceFactory.CreateCommonConfigInfo();
            MediaConfigInfo.ArchiverConfigInfo = MediaServiceFactory.CreateArchiverConfigInfo();

            CompoundDisposalStatistics.Instance.Init(new RAArchiverCommon.DisposalProgress.DisposalStaticInitObject()
            {
                JobType = jobType,
                MainJobId = jobId.Split('_')[0],
                SubJobId = jobId
            });
        }
        public TeamsArchiverRetentionJobHandler()
        {
        }
        public void InitTeamsRetainForOrphanBlob(string jobId, JobType jobType)
        {
            SubJobId = jobId;
            mJobType = jobType;
        }
        /// <summary>
        /// 之前逻辑中，job运行时，会查询retention settings, 并保存在DB里，retention会按照这个设置来走.
        /// 例如，设置retention setting A 保留1个月的数据，运行Archive Job A，之后修改retention setting A为保留2个月，运行Archive Job B, 则Archive Job A的数据会按照保留1个月的数据的retention 来运行，
        /// 而Archive Job B的数据会按照保留2个月的数据的retention 来运行，但是并没有任何的地方可以让客户查看到之前的setting，所以优化为去除了TakeEffectToExistingData逻辑.
        /// 按照上面的例子，Archive Job A和Archive Job B都会按照保留2个月的setting来走。
        /// 升级时需要注意的问题：cloud archive是使用的LogicalDeviceId来记录识别，但是现在相当于只有physical device id, 如何进行转换
        /// </summary>
        public async Task RunAsync()
        {
            string commnet = string.Empty;
            CompoundDisposalStatistics.Instance.StartStatistic();
            try
            {
                ReportManager.StartUpdateJobProgress();
                IRMSubJobDao SubJobDao = new RMSubJobDao();
                IJobMonitorDao JobMonitorDao = new JobMonitorDao();
                //从子job的Context中获取当前需要处理的节点.
                RMSubJob subJobWithContext = SubJobDao.GetSubJob(SubJobId, true);
                MainJobId = subJobWithContext.ParentId;
                JobContextSetting = subJobWithContext.JobContext?.Settings;
                var archiverPruningJobs = SerializerHelper.DeserializeByDataContractSerializer<List<ArchiverPruningJob>>(JobContextSetting);
                //Process decrypt secret of connection string for google storage
                DecryptSecretForGoogleStorage(archiverPruningJobs);
                var dateTimeNow = DateTime.UtcNow;
                mLog.Info($"Current date time now is :{dateTimeNow},ticks:{dateTimeNow.Ticks},will user this time to check modified time");
                foreach (var archiverPruningJob in archiverPruningJobs)
                {
                    RARetentionJobTelemetry telemetry = null;
                    try
                    {
                        telemetry = await BuildRARetentionJobTelemetry(archiverPruningJob);
                        mLog.Info($"this archiver pruning job:{archiverPruningJob.JobId},retention data type:{archiverPruningJob.RetentionDataTimeType}");
                        if (archiverPruningJob.RetentionDataTimeType == KeepDateType.None)
                        {
                            mLog.Warn($"this retention info is old data info,need to reset it to archive time.");
                            archiverPruningJob.RetentionDataTimeType = KeepDateType.ArchiveTime;
                        }
                        archiverPruningJob.DateTimeNowTicks = ValidateModifiedTime(archiverPruningJob.KeepValue, archiverPruningJob.ArchiveDateUnit, dateTimeNow);
                        if (!CheckJobIsFileLevelBackup(archiverPruningJob.JobId) && archiverPruningJob.RetentionDataTimeType == KeepDateType.ModifiedTime)
                        {
                            mLog.Warn($"Skip run this info, because the job:{archiverPruningJob.JobId} is not file level backup.");
                            continue;
                        }
                        if (CheckArchiveTierLessThan90Days(archiverPruningJob))
                        {
                            mLog.Warn($"The archive time is less than 90 days, will not delete data.jobid:{archiverPruningJob.JobId}");
                            continue;
                        }

                        var lockResult = await SampleDBLocker.TryGet4IndexDBEmail(archiverPruningJob.SiteUrl, archiverPruningJob.SiteId, SubJobId, mJobType);
                        if (lockResult.Item1)
                        {
                            using var dbLocker = lockResult.Item2;
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
                                        archiverPruningJob.RetentionJob.Id = SubJobId;
                                    }
                                }
                                mLog.Info($"Start to run retention job for sub job id:{SubJobId}, retention action:{archiverPruningJob.RetentionAction}, is simulate job:{archiverPruningJob.IsSimulateJob}");
                                CheckIfEnableMoveToAnotherLocation(archiverPruningJob);
                                switch (archiverPruningJob.RetentionAction)
                                {
                                    case MediaArchiverRetentionAction.DeleteData:
                                        DeleteSubJobData(archiverPruningJob);
                                        break;
                                    case MediaArchiverRetentionAction.MoveData:
                                        await ExportDataToAnotherDeviceAsync(archiverPruningJob);
                                        break;
                                    case MediaArchiverRetentionAction.MarkTier:
                                        MarkSubJobDataTier(archiverPruningJob);
                                        break;
                                    default:
                                        throw new NotSupportedException("Current archiver retention action is not supported.");
                                }
                            }
                            catch (JobStopException e)
                            {
                                mLog.Error("Job will stop, Catch JobStopException.");
                                mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Stopped;
                                commnet = e.Message;
                            }
                            catch (Exception e)
                            {
                                mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.FinishWithException;
                                mLog.Error("Error :{0}", e.ToString());
                            }
                        }
                        else
                        {
                            ReportManager.SendJobDetail(GenerateRetentionSkipReport(archiverPruningJob.JobId, archiverPruningJob.RetentionAction, archiverPruningJob.IsSoftDelete));
                            mLog.Info($"Skip run this info, because cannot get site lock.");
                        }
                        ReportManager.Increase();
                    }
                    catch (Exception ex)
                    {
                        mLog.Error($"An error occured while process retention for {archiverPruningJob.SiteUrl}. Ex: {ex}");
                    }
                    finally
                    {
                        await EndAndSendRetentionJobTelelmetry(telemetry);
                    }
                }
            }
            finally
            {
                CompoundDisposalStatistics.Instance.PrepareEndStatistic();
                CompoundDisposalStatistics.Instance.WaitEndStatistic();
                if (mJobStatus == AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Stopped || mJobStatus == AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Failed)
                {
                    if (string.IsNullOrEmpty(commnet))
                    {
                        ReportManager.SetJobFinished(mJobStatus);
                    }
                    else
                    {
                        ReportManager.SetJobFinished(mJobStatus, commnet);
                    }
                }
                else
                {
                    ReportManager.SetJobFinished(GetJobStatus());
                }
            }
        }

        private async Task<RARetentionJobTelemetry> BuildRARetentionJobTelemetry(ArchiverPruningJob archiverPruningJob)
        {
            RARetentionJobTelemetry telemetry = new RARetentionJobTelemetry();
            try
            {
                telemetry.JobId = SubJobId;
                telemetry.MainJobId = MainJobId;
                telemetry.RetentionObject = archiverPruningJob.SiteUrl;
                telemetry.ArchivedSubJobId = archiverPruningJob.JobId;
                telemetry.JobType = ((int)JobType.TeamsArchiverRetention).ToString();
                telemetry.StorageName = archiverPruningJob?.DataLogicalDevice?.Name;
                telemetry.RetentionAction = (int)archiverPruningJob.RetentionAction;
                ArchiverIndexSubInfo indexSubInfo = await ArchiverIndexSubInfoDao.GetSubInfoBySubsubJobIdAsync(archiverPruningJob?.JobId);
                telemetry.MediaDataSize = indexSubInfo?.MediaDataSize ?? 0;
            }
            catch(Exception e)
            {
                mLog.Error("Fail build telemetry object :{0}", e.ToString());
            }
            return telemetry;
        }

        private async Task EndAndSendRetentionJobTelelmetry(RARetentionJobTelemetry telemetry)
        {
            try
            {
                ArchiverIndexSubInfo indexSubInfo = await ArchiverIndexSubInfoDao.GetSubInfoBySubsubJobIdAsync(telemetry.ArchivedSubJobId);
                telemetry.RemainingMediaDataSize = indexSubInfo?.MediaDataSize ?? 0;
                telemetry.RetentionDataSize = telemetry.MediaDataSize - telemetry.RemainingMediaDataSize;
                await RATelemetryService.AddTelemetryForRetentionJob(telemetry);
            }
            catch (Exception e) 
            {
                mLog.Error(@$"Fail end and send telemetry, ex:{e}");
            }
        }

        private void DecryptSecretForGoogleStorage(List<ArchiverPruningJob> archiverPruningJobs)
        {
            if(archiverPruningJobs != null && archiverPruningJobs.Count > 0)
            {
                foreach(var archiverPruningJob in archiverPruningJobs)
                {
                    if(archiverPruningJob.DataLogicalDevice != null)
                        DecryptGoogleStorageSecret(archiverPruningJob.DataLogicalDevice);
                    if (archiverPruningJob.IndexLogicalDevice != null)
                        DecryptGoogleStorageSecret(archiverPruningJob.IndexLogicalDevice);
                    if (archiverPruningJob.DestinationDevice != null)
                        DecryptGoogleStorageSecret(archiverPruningJob.DestinationDevice);
                }
            }
        }

        private void DecryptGoogleStorageSecret(LogicalDeviceDto dto)
        {
            if(dto.PhysicalDrives != null && dto.PhysicalDrives.Count > 0)
            {
                foreach(var physicalDrive in dto.PhysicalDrives)
                {
                    string begin = "-----BEGIN PRIVATE KEY-----";
                    string end = "-----END PRIVATE KEY-----";
                    if (physicalDrive.Type == (int)StorageDeviceType.Google)
                    {
                        if(physicalDrive.Password != null && physicalDrive.Password.Count > 0)
                        {
                            string[] keyValue = physicalDrive.Password[0].Split(new char[] { '=' });
                            if (!keyValue[0].EndsWith("tokensecret") && !(keyValue[1].StartsWith(begin) && keyValue[1].Contains(end)))
                            {
                                keyValue[1] = PhysicalDeviceDto.XRIUtil.ValueEncode(UnWrapKey(PhysicalDeviceDto.XRIUtil.ValueDecode(keyValue[1])));
                            }
                            physicalDrive.UpdatePassword(new List<string> { keyValue[0] + "=" + keyValue[1] });
                        }
                    }
                }
            }
        }

        private string UnWrapKey(string password)
        {
            var result = CspCrossPlatformExchangeWrapper.UnWrapKey(password);
            return Encoding.UTF8.GetString(result, 0, result.Length);
        }

        public async Task RunDeleteOrphanDatasAsync()
        {
            string commnet = string.Empty;
            try
            {
                ReportManager.StartUpdateJobProgress();
                IRMSubJobDao SubJobDao = new RMSubJobDao();
                IJobMonitorDao JobMonitorDao = new JobMonitorDao();
                RMSubJob subJobWithContext = SubJobDao.GetSubJob(SubJobId, true);
                MainJobId = subJobWithContext.ParentId;
                JobContextSetting = subJobWithContext.JobContext?.Settings;
                var archiverPruningJobs = SerializerHelper.DeserializeByDataContractSerializer<List<ArchiverPruningJob>>(JobContextSetting);
                foreach (var archiverPruningJob in archiverPruningJobs)
                {
                    try
                    {
                        mLog.Info($"this delete orphan datas job:{archiverPruningJob.JobId}");

                        var lockResult = await SampleDBLocker.TryGet4IndexDBUpdater(archiverPruningJob.SiteUrl, archiverPruningJob.SiteId, SubJobId);
                        if (lockResult.Item1)
                        {
                            using var dbLocker = lockResult.Item2;
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
                                        archiverPruningJob.RetentionJob.Id = SubJobId;
                                    }
                                }
                                DeleteSubJobData(archiverPruningJob);
                            }
                            catch (JobStopException)
                            {
                                mLog.Error("Job will stop, Catch JobStopException.");
                                mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Stopped;
                            }
                            catch (Exception e)
                            {
                                mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.FinishWithException;
                                mLog.Error("Error :{0}", e.ToString());
                            }
                        }
                        else
                        {
                            ReportManager.SendJobDetail(GenerateDeleteOrphanDatasReport(archiverPruningJob.JobId));
                            mLog.Info($"Skip run this info, because cannot get site lock.");
                        }
                        ReportManager.Increase();
                    }
                    finally
                    {

                    }
                }
                RemoveSuccessDeletedOrphanDatasSubjob();
            }
            finally
            {
                if (mJobStatus == AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Stopped || mJobStatus == AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Failed)
                {
                    if (string.IsNullOrEmpty(commnet))
                    {
                        ReportManager.SetJobFinished(mJobStatus);
                    }
                    else
                    {
                        ReportManager.SetJobFinished(mJobStatus, commnet);
                    }
                }
                else
                {
                    ReportManager.SetJobFinished(GetJobStatus());
                }
            }
        }
        public void RemoveSuccessDeletedOrphanDatasSubjob()
        {
            try
            {
                foreach (string subsubJobid in successDeleteSubsubJobIds)
                {
                    if (failedDeleteSubsubJobIds.Contains(subsubJobid))
                    {
                        mLog.Warn($"teams there exsit something wrong when delete orphan datas job,so donot delete this subjob record,id:{subsubJobid}");
                    }
                    else
                    {
                        mLog.Info($"teams delete orphan datas job success,delete subjob record,id:{subsubJobid}");
                        SubJobDao.DeleteSubJobById(subsubJobid.Substring(0, subsubJobid.LastIndexOf("_")));
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Error($"something wrong when remove success delete orphan datas subjob,error:{e}");
            }
        }
        private bool CheckArchiveTierLessThan90Days(ArchiverPruningJob archiverPruningJob)
        {
            try
            {
                if (archiverPruningJob.RetentionDataTimeType == KeepDateType.ModifiedTime)
                {
                    DateTime archiveTime = new DateTime(archiverPruningJob.ArchiverBackupTime);
                    DateTime now = DateTime.UtcNow;
                    if (archiverPruningJob.StoragePolicyId.Equals(AVEPOINT_DEFAULT_STORAGEID,StringComparison.OrdinalIgnoreCase))
                    {
                        if (archiveTime.AddDays(90).Ticks <= now.Ticks)
                        {
                            mLog.Info($"current job has arhived more than 90days,should process,archive time:{archiveTime},job id:{archiverPruningJob.JobId}");
                            return false;
                        }
                        else
                        {
                            mLog.Info($"current job has arhived less than 90days,should not process,archive time:{archiveTime},job id:{archiverPruningJob.JobId}");
                            return true;
                        }
                    }
                    else
                    {
                        mLog.Info($"current job has arhived less than 90days,but not avepoint storage,process it,archive time:{archiveTime},job id:{archiverPruningJob.JobId}");
                        return false;
                    }
                }
                else
                {
                    mLog.Info($"not retention by archive time,return false");
                    return false;
                }
            }
            catch (Exception e)
            {
                mLog.Error($"somthing went wrong when check archive job less than 90days,error:{e}");
                return true;
            }
        }
        private bool CheckJobIsFileLevelBackup(string subSubjobId)
        {
            var subjobId = GetSubJobId(subSubjobId);
            return false;//ArchiverSiteMasterIndexDao.IsFileLevelBlockBackup(subjobId);
        }
        private long ValidateModifiedTime(int keepValue, DateUnit dateUnit, DateTime dateTimeNow)
        {
            if (keepValue < 0)
            {
                mLog.Info($"keep value is zero,return false");
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
        private JMArchiverRententionJobDetails GenerateRetentionSkipReport(string subinfojobId,MediaArchiverRetentionAction action,bool isSoft)
        {
            try
            {
                var report = new JMArchiverRententionJobDetails();
                string subJobId = subinfojobId;// GetSubJobId(subinfojobId);
                if (string.IsNullOrEmpty(subJobId))
                {
                    mLog.Warn($"get sub job id failed,subIndexInfo job id is:{subinfojobId}");
                    return null;
                }
                List<ArchiverSiteMasterIndexContract> masterIndexs = ArchiverSiteMasterIndexDao.GetIndexByJobId(subJobId);
                ArchiverSiteMasterIndexContract siteInfo = masterIndexs[0];
                report.SiteUrl = siteInfo.SiteURL;
                report.Size = "0";
                report.Status = JobDetailsStatus.Skipped;
                report.JobId = subinfojobId;
                report.Comment = "RM_JM_Retention_IndexLock";
                report.Action = isSoft? "RM_AR_CP_GSS_Retention_SoftDelete" : action switch
                {
                    MediaArchiverRetentionAction.DeleteData => "RM_JS_Common_Delete",
                    MediaArchiverRetentionAction.MarkTier => "RM_AR_CP_GSS_Retention_MarkDataTier",
                    MediaArchiverRetentionAction.MoveData => GetJobDetailsActionForMoveData(),
                    _ => ""
                };
                return report;
            }
            catch (Exception e)
            {
                mLog.Error($"something wrong with Generate Retention Skip Report,error :{e.ToString()}");
                return null;
            }
        }
        private JMDeleteOrphanDatasJobDetails GenerateDeleteOrphanDatasReport(string subinfojobId)
        {
            try
            {
                var report = new JMDeleteOrphanDatasJobDetails();
                string subJobId = GetSubJobId(subinfojobId);
                if (string.IsNullOrEmpty(subJobId))
                {
                    mLog.Warn($"get sub job id failed,subIndexInfo job id is:{subinfojobId}");
                    return null;
                }
                List<ArchiverSiteMasterIndexContract> masterIndexs = ArchiverSiteMasterIndexDao.GetIndexByJobId(subJobId);
                ArchiverSiteMasterIndexContract siteInfo = masterIndexs[0];
                report.SiteUrl = siteInfo.SiteURL;
                report.Size = "0";
                report.Status = JobDetailsStatus.Skipped;
                report.JobId = subinfojobId;
                report.Comment = "RM_JM_Retention_IndexLock";
                return report;
            }
            catch (Exception e)
            {
                mLog.Error($"something wrong with Generate Retention Skip Report,error :{e.ToString()}");
                return null;
            }
        }

        public string GetTeamsMainJobId(string backupSubJobId)
        {
            try
            {
                var temp = backupSubJobId.Split("_");
                if (temp.Length >= 2)
                {
                    return string.Format("{0}_{1}", temp[0], temp[1]);
                }
                return backupSubJobId;
            }
            catch (Exception e)
            {
                mLog.Error($"Failed to get teams main job id[{backupSubJobId}], Error. {e.ToString()}");
                return backupSubJobId;
            }
        }
        public void DeleteSubJobData(ArchiverPruningJob archiverRetentionInfo)
        {
            mLog.Info($"DeleteSubJobData : SiteUrl:{archiverRetentionInfo.SiteUrl}");
            ArchiverRetentionResult result = null;
            string backupSubJobId = archiverRetentionInfo.JobId;// archiverRetentionInfo.JobId.Substring(0, archiverRetentionInfo.JobId.LastIndexOf("_", StringComparison.CurrentCulture));
            try
            {
                result = RealDeleteSubJobData(archiverRetentionInfo, backupSubJobId);
            }
            catch (Exception e)
            {
                mLog.Error($"DeleteSubJobData Error. {e.ToString()}");
            }
            //删除sub info表记录, 如果主表对应的子表记录全部删除, 则删除主表记录.

            var subIndex = ArchiverIndexSubInfoDao.Find(i => i.SubSubJobId == archiverRetentionInfo.JobId);
            if (archiverRetentionInfo.RetentionDataTimeType != KeepDateType.ModifiedTime && result?.HasIndexRelatedToBackupJob == false)
            {
                if (archiverRetentionInfo.IsSoftDelete && archiverRetentionInfo.IsFitSoftDelete)
                {
                    mLog.Info($"this index sub info is fit soft delete,will delete it,{subIndex?.SubSubJobId}");
                    ArchiverIndexSubInfoDao.Delete(subIndex);
                }
                else if (!archiverRetentionInfo.IsSoftDelete)
                {
                    mLog.Info($"delete sub job will delete it,{subIndex?.SubSubJobId}");
                    ArchiverIndexSubInfoDao.Delete(subIndex);
                }
                else
                {
                    mLog.Info($"do not delete this subinfo:{subIndex?.SubSubJobId},becaused it has set soft delete");
                }
            }
            if (!archiverRetentionInfo.IsFitSoftDelete && archiverRetentionInfo.IsSoftDelete && (subIndex.DeletedStatus == (int)DeletedStatus.Normal || subIndex.DeletedStatus == (int)DeletedStatus.Restored) && archiverRetentionInfo.RetentionDataTimeType != KeepDateType.ModifiedTime)
            {
                subIndex.DeletedStatus = (int)DeletedStatus.SoftDelete;
                subIndex.SoftDeleteTime = DateTime.UtcNow.Ticks;
                ArchiverIndexSubInfoDao.UpdateAsync(subIndex).GetAwaiter().GetResult();
            }

            if (result != null && result.State == 2)
            {
                try
                {

                    var subInfosCount = ArchiverIndexSubInfoDao.GetSubInfoCountAsync(GetTeamsMainJobId(backupSubJobId)).GetAwaiter().GetResult();
                    var job = ArchiverSiteMasterIndexDao.Find(i => i.JobId == backupSubJobId);
                    if(job == null)
                    {
                        var temp = backupSubJobId.Split("_");
                        if(temp.Length >= 1)
                        {
                            var mainJobId = string.Format("{0}_{1}", temp[0], temp[1]);
                            job = ArchiverSiteMasterIndexDao.Find(i => i.JobId == mainJobId);
                            mLog.Info($"Could not found index with JodId: {backupSubJobId}. So retry MainJobId: {job?.JobId}");
                        }
                    }
                    //var retentionSubInfo = GenaratRetentionSubInfo(subIndex, job);
                    //RetentionIndexSubInfoDao.InsertIntoRetentionIndexSubInfo(retentionSubInfo);
                    if (subInfosCount == 0)
                    {
                        if (job != null)
                        {
                            mLog.Info("Archiver Site Master Index with job id {0}, whose SubInfo is null or empty, has been deleted after retention job.", backupSubJobId);
                            ArchiverSiteMasterIndexDao.Delete(job);
                        }
                        else
                        {
                            mLog.Info("Archiver Site Master Index with job id {0}, but cannot find in table.", backupSubJobId);
                        }
                    }
                    else
                    {
                        mLog.Info("SubInfo count of sub job id {0} is {1}", backupSubJobId, subInfosCount);
                    }
                    if (archiverRetentionInfo.RetainType == RetainType.DeleteOrphanDatas)
                    {
                        mLog.Info("this delete action is delete orphan job.");
                        successDeleteSubsubJobIds.Add(archiverRetentionInfo.JobId);
                    }

                    //需要删除job
                    if (archiverRetentionInfo.IsDeleteJob)
                    {
                        mLog.Info("Delete data {0}, delete sub info.", archiverRetentionInfo.JobId);
                        var jobMonitorJobId = backupSubJobId.Substring(0, archiverRetentionInfo.JobId.IndexOf("_", StringComparison.CurrentCulture));
                        JobMonitorService.DeleteJobsAsync(new List<string> { jobMonitorJobId }).GetAwaiter().GetResult();
                        mLog.Info("Delete job record successful");

                        //TeamsCheck?
                        //try
                        //{
                        //    var archiverJob = ArchiverJobDao.GetJobByID(jobMonitorJobId);
                        //    if (archiverJob != null)
                        //    {
                        //        JobMonitorService.DeleteJobsAsync(new List<string> { archiverJob.RECOJobId }).GetAwaiter().GetResult();
                        //        ArchiverJobDao.Delete(archiverJob);
                        //        mLog.Info("Delete archiver job record successful");
                        //    }
                        //}
                        //catch (Exception e)
                        //{
                        //    mLog.Error($"Delete Archiver Job Error,{e}");
                        //}
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn(e.ToString());
                    result.State = 1;
                }
            }
            UpdateArchiverSize(archiverRetentionInfo.SiteUrl);
            if (result != null && result.State == 2)
            {
                if (archiverRetentionInfo.RetentionDataTimeType != KeepDateType.ModifiedTime)
                {
                    if (archiverRetentionInfo.RetainType == RetainType.DeleteOrphanDatas)
                    {
                        mLog.Info($"this retention job is delete orphan datas job");
                        var report = new JMDeleteOrphanDatasJobDetails();
                        report.SiteUrl = archiverRetentionInfo.SiteUrl;
                        report.Size = result.Size.ToString();
                        report.Status = JobDetailsStatus.Successful;
                        report.JobId = archiverRetentionInfo.JobId;
                        ReportManager.SendJobDetail(report);
                    }
                    else
                    {
                        if (archiverRetentionInfo.SoftDeleteTime > 0)
                        {
                            mLog.Info($"this job has soft deleted,no need to add detail,job id:{archiverRetentionInfo.JobId}");
                        }
                        else
                        {
                            var report = new JMArchiverRententionJobDetails();
                            report.SiteUrl = archiverRetentionInfo.SiteUrl;
                            report.Size = result.Size.ToString();
                            report.Status = JobDetailsStatus.Successful;
                            report.SrcStorageName = archiverRetentionInfo.DataLogicalDevice.Name;
                            report.JobId = archiverRetentionInfo.JobId;
                            report.Action = (!archiverRetentionInfo.IsFitSoftDelete && archiverRetentionInfo.IsSoftDelete) ? "RM_AR_CP_GSS_Retention_SoftDelete" :"RM_JS_Common_Delete";
                            ReportManager.SendJobDetail(report);
                        }
                    }
                }
                else
                {
                    mLog.Info($"this retention job is retention by modified time,so not job level detail");
                }
            }
            else
            {
                if (archiverRetentionInfo.RetentionDataTimeType != KeepDateType.ModifiedTime)
                {
                    if (archiverRetentionInfo.RetainType == RetainType.DeleteOrphanDatas)
                    {
                        mLog.Info($"this retention job is delete orphan datas job1");
                        var report = new JMDeleteOrphanDatasJobDetails();
                        report.SiteUrl = archiverRetentionInfo.SiteUrl;
                        report.Size = "0";
                        report.Status = JobDetailsStatus.Failed;
                        report.JobId = archiverRetentionInfo.JobId;
                        ReportManager.SendJobDetail(report);
                    }
                    else
                    {
                        var report = new JMArchiverRententionJobDetails();
                        report.SiteUrl = archiverRetentionInfo.SiteUrl;
                        report.Size = "0";
                        report.Status = JobDetailsStatus.Failed;
                        report.JobId = archiverRetentionInfo.JobId;
                        report.SrcStorageName = archiverRetentionInfo.DataLogicalDevice.Name;
                        report.Action = (!archiverRetentionInfo.IsFitSoftDelete && archiverRetentionInfo.IsSoftDelete) ? "RM_AR_CP_GSS_Retention_SoftDelete" : "RM_JS_Common_Delete";
                        ReportManager.SendJobDetail(report);
                    }
                }
                else
                {
                    mLog.Info($"this retention job is retention by modified time,so not job level detail and it has error");
                }
                failedDeleteSubsubJobIds.Add(archiverRetentionInfo.JobId);
            }
            //ThreadPool.QueueUserWorkItem(state =>
            //{
            //    RealDeleteSubJobData(state as ArchiverPruningJob);
            //}, archiverRetentionInfo);
        }

        public void MarkSubJobDataTier(ArchiverPruningJob archiverRetentionInfo)
        {
            mLog.Info($"MarkSubJobDataTier : Teams Address:{archiverRetentionInfo.SiteUrl}, AccessTierType:{archiverRetentionInfo.AccessTierType}");
            ArchiverRetentionResult result = null;
            string errorMessage = string.Empty;
            try
            {
                result = RealMarkDataTier(archiverRetentionInfo);
            }
            catch (Exception e)
            {
                mLog.Error($"MarkSubJobDataTier Error. {e.ToString()}");
                errorMessage = e.Message;
            }
            if (result != null && result.State == 2)
            {
                try
                {
                    //更新sub master index表中logical device id和retention time
                    var subIndex = ArchiverIndexSubInfoDao.Find(i => i.SubSubJobId == archiverRetentionInfo.JobId);
                    if (subIndex != null)
                    {
                        mLog.Info("Move data {0}, update retention time.", archiverRetentionInfo.JobId);
                        subIndex.RetentionTime = DateTime.UtcNow.Ticks;
                        subIndex.RetentionCount++;
                        ArchiverIndexSubInfoDao.UpdateAsync(subIndex).GetAwaiter().GetResult();
                        mLog.Info("Update sub master index successful");
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn(e.ToString());
                    result.State = 1;
                }
                var report = new JMArchiverRententionJobDetails();
                report.SiteUrl = archiverRetentionInfo.SiteUrl;
                report.Size = string.Empty;//result.Size.ToString();
                report.Status = JobDetailsStatus.Successful;
                report.SrcStorageName = archiverRetentionInfo.DataLogicalDevice.Name;
                report.JobId = archiverRetentionInfo.JobId;
                report.Action = "RM_AR_CP_GSS_Retention_MarkDataTier";
                report.Comment = result.IsArchiveTierToColdTier? "RM_AR_CP_GSS_Retention_ChangeToColdFromArchive":string.Empty;
                ReportManager.SendJobDetail(report);
            }
            else
            {
                var report = new JMArchiverRententionJobDetails();
                report.SiteUrl = archiverRetentionInfo.SiteUrl;
                report.Size = string.Empty;//"0";
                report.Status = JobDetailsStatus.Failed;
                report.JobId = archiverRetentionInfo.JobId;
                report.Comment = errorMessage;
                report.Action = "RM_AR_CP_GSS_Retention_MarkDataTier";
                ReportManager.SendJobDetail(report);
                mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Failed;

            }
            //ThreadPool.QueueUserWorkItem(state =>
            //{
            //    RealDeleteSubJobData(state as ArchiverPruningJob);
            //}, archiverRetentionInfo);
        }
        private void UpdateArchiverSize(string siteUrl)
        {
            try
            {
                var worker = new TeamsSODashboardWorker();
                worker.UpdateTeamsGroupArchivedInfo(siteUrl).GetAwaiter().GetResult();
            }
            catch (Exception e)
            {
                mLog.Warn($"SomeThing went wrong when update archiver size in archiverInfo,error :{e.ToString()}");
            }
        }
        private RetentionIndexSubInfo GenaratRetentionSubInfo(ArchiverIndexSubInfo subInfo, AvePoint.RA.DB.Model.ArchiverSiteMasterIndex siteMasterIndex)
        {
            RetentionIndexSubInfo result = new RetentionIndexSubInfo();
            result.Id = Guid.NewGuid().ToString();
            result.RetentionTime = subInfo.RetentionTime;
            result.JobId = this.SubJobId;
            result.ArchiverJobId = subInfo.SubSubJobId;
            result.SiteGroupId = siteMasterIndex.SiteGroupId;
            result.SiteURL = siteMasterIndex.SiteURL;
            result.SiteId = siteMasterIndex.SiteId;
            return result;
        }
        public async Task ExportDataToAnotherDeviceAsync(ArchiverPruningJob archiverRetentionInfo)
        {
            mLog.Info($"ExportDataToAnotherDevice : SiteUrl:{archiverRetentionInfo.SiteUrl}");

            ArchiverRetentionResult result = null;
            try
            {
                result = RealExportDataToAnotherDevice(archiverRetentionInfo);
            }
            catch (JobStopException e)
            {
                mLog.Warn("Job will stop, throw JobStopException.");
                throw;
            }
            catch (Exception e)
            {
                mLog.Error($"ExportDataToAnotherDevice Error. {e.ToString()}");
            }
            if (result != null && result.State == 2)
            {
                try
                {
                    //更新sub master index表中logical device id和retention time
                    var subIndex = ArchiverIndexSubInfoDao.Find(i => i.SubSubJobId == archiverRetentionInfo.JobId);
                    if (subIndex != null)
                    {
                        mLog.Info("Move data {0}, update retention time.", archiverRetentionInfo.JobId);
                        subIndex.CurrentStorageId = archiverRetentionInfo.DestinationPhysicalDeviceId;
                        subIndex.RetentionTime = DateTime.UtcNow.Ticks;
                        subIndex.RetentionCount++;
                        await ArchiverIndexSubInfoDao.UpdateAsync(subIndex);
                        mLog.Info("Update sub master index successful");
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn(e.ToString());
                    result.State = 1;
                }
            }
            if (result != null && result.State == 2)
            {
                var report = new JMArchiverRententionJobDetails();
                report.SiteUrl = archiverRetentionInfo.SiteUrl;
                report.Size = result.Size.ToString();
                report.Status = JobDetailsStatus.Successful;
                report.SrcStorageName = archiverRetentionInfo.DataLogicalDevice.Name;
                report.DesStorageName = archiverRetentionInfo.DestinationDevice?.Name;
                report.JobId = archiverRetentionInfo.JobId;
                report.Action = GetJobDetailsActionForMoveData();
                ReportManager.SendJobDetail(report);
            }
            else
            {
                var report = new JMArchiverRententionJobDetails();
                report.SiteUrl = archiverRetentionInfo.SiteUrl;
                report.Size = "0";
                report.Status = JobDetailsStatus.Failed;
                report.JobId = archiverRetentionInfo.JobId;
                report.Action = GetJobDetailsActionForMoveData();
                ReportManager.SendJobDetail(report);
                mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Failed;

            }
        }


        private ArchiverRetentionResult RealDeleteSubJobData(ArchiverPruningJob archiverRetentionInfo, string backupSubJobId)
        {
            ValidSoftDeleteIsFitRetainByModifiedTime(archiverRetentionInfo);
            var retentionInfo = new TeamsArchiverRetentionInfo(archiverRetentionInfo);
            if (archiverRetentionInfo.RetainType != RetainType.DeleteOrphanDatas)
            {
                retentionInfo.IsFileLevelBlockBackup = false;// ArchiverSiteMasterIndexDao.IsFileLevelBlockBackup(backupSubJobId);
            }
            retentionInfo.RetentionRule = AvePoint.Media.Service.DomainModel.RetentionRule.RetainArchiverJobData;
            var retentionService = (IRetentionService)PlatformWindsorManager.GetService("AvePoint.Media.Service.ArchiverBackup.TeamsArchiverBackupRetentionService", typeof(IRetentionService));
            var result = retentionService.Retain(retentionInfo, SendJobReport) as ArchiverRetentionResult;
            return result;
        }
        private void ValidSoftDeleteIsFitRetainByModifiedTime(ArchiverPruningJob archiverRetentionInfo)
        {
            if (archiverRetentionInfo.IsSoftDelete && archiverRetentionInfo.SoftDeleteTime > 0 && !archiverRetentionInfo.IsFitSoftDelete)
            {
                archiverRetentionInfo.IsFitSoftDelete = ValidateRetentionTime(archiverRetentionInfo.SoftDeleteTime, archiverRetentionInfo.SoftDeleteKeepValue, archiverRetentionInfo.SoftDeleteDateUnit);
            }

        }
        private bool ValidateRetentionTime(long retentionTimeTicks, int keepValue, DateUnit dateUnit)
        {
            if (keepValue < 0)
            {
                mLog.Info($"keep value is zero,return false");
                return false;
            }
            DateTime retentionTime = new DateTime(retentionTimeTicks);
            switch (dateUnit)
            {
                case DateUnit.Year:
                    retentionTime = retentionTime.AddYears(keepValue);
                    break;
                case DateUnit.Month:
                    retentionTime = retentionTime.AddMonths(keepValue);
                    break;
                case DateUnit.Week:
                    retentionTime = retentionTime.AddDays(keepValue * 7);
                    break;
                case DateUnit.Day:
                    retentionTime = retentionTime.AddDays(keepValue);
                    break;
            }
            mLog.Info($"ValidatesoftTime.RetentionTime {retentionTime.Ticks}");
            return retentionTime.Ticks <= DateTime.UtcNow.Ticks;
        }
        private ArchiverRetentionResult RealExportDataToAnotherDevice(ArchiverPruningJob archiverRetentionInfo)
        {
            var retentionInfo = new TeamsArchiverRetentionInfo(archiverRetentionInfo);
            retentionInfo.RetentionRule = AvePoint.Media.Service.DomainModel.RetentionRule.MoveArchiverJobData;
            var retentionService = (IRetentionService)PlatformWindsorManager.GetService("AvePoint.Media.Service.ArchiverBackup.TeamsArchiverBackupRetentionService", typeof(IRetentionService));
            var result = retentionService.Retain(retentionInfo, SendJobReport) as ArchiverRetentionResult;
            return result;
        }
        private ArchiverRetentionResult RealMarkDataTier(ArchiverPruningJob archiverRetentionInfo)
        {
            var retentionInfo = new TeamsArchiverRetentionInfo(archiverRetentionInfo);
            retentionInfo.RetentionRule = AvePoint.Media.Service.DomainModel.RetentionRule.MarkArchiverJobDataTier;
            var retentionService = (IRetentionService)PlatformWindsorManager.GetService("AvePoint.Media.Service.ArchiverBackup.TeamsArchiverBackupRetentionService", typeof(IRetentionService));
            var result = retentionService.Retain(retentionInfo, SendJobReport) as ArchiverRetentionResult;
            return result;
        }

        private string GetSubJobId(string subSubJobId)
        {
            if (!string.IsNullOrEmpty(subSubJobId))
            {
                return subSubJobId.Substring(0, subSubJobId.LastIndexOf("_", StringComparison.CurrentCulture));
            }
            return null;
        }

        private AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus GetJobStatus()
        {
            if (HasStop || CheckJobStatusUtility.isStopping)
            {
                mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Stopped;
            }
            else if (HasCompleteNode && !HasErrorNode)
            {
                mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Finished;
            }
            else if (HasCompleteNode && HasErrorNode)
            {
                mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.FinishWithException;
            }
            else if (!HasCompleteNode && !HasErrorNode)
            {
                mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Finished;
            }
            else if (!HasCompleteNode && HasErrorNode)
            {
                mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Failed;
            }
            return mJobStatus;
        }

        private void SendJobReport(JMArchiverRententionJobDetails rententionJobDetails)
        {
            AnalyzeStatus(rententionJobDetails.Status);
            ReportManager.SendJobDetail(rententionJobDetails);
        }

        private void AnalyzeStatus(JobDetailsStatus status)
        {
            if (status == JobDetailsStatus.Successful || status == JobDetailsStatus.Skipped)
            {
                HasCompleteNode = true;
            }
            else if (status == JobDetailsStatus.Failed)
            {
                HasErrorNode = true;
            }
        }

        private string GetJobDetailsActionForMoveData()
        {
            return mKeyValueDao.IsEnableCopyToAnotherLocation() ? "RM_JS_Common_Copy" : "RM_PRM_PRE_Move";
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
            if (!mKeyValueDao.IsEnableMoveToAnotherLocation())
            {
                string errorMessage = isMoveAction
                    ? "Move to another location is disabled. Both 'EnableMoveToAnotherLocation' and 'EnableCopyToAnotherLocation' settings are disabled."
                    : "There is \"Move to another location\" option in previous retention rules, but both 'EnableMoveToAnotherLocation' and 'EnableCopyToAnotherLocation' settings are disabled.";
                mLog.Error(errorMessage);
                var report = new JMArchiverRententionJobDetails
                {
                    SiteUrl = archiverRetentionInfo.SiteUrl,
                    Size = "0",
                    Status = JobDetailsStatus.Failed,
                    JobId = archiverRetentionInfo.JobId,
                    Action = this.GetJobDetailsActionForMoveData(),
                    Comment = "RM_Retention_MoveToAnotherLocationDisabled",
                    SrcStorageName = archiverRetentionInfo.DataLogicalDevice?.Name ?? string.Empty,
                    DesStorageName = archiverRetentionInfo.DestinationDevice?.Name ?? string.Empty,
                };
                this.SendJobReport(report);
                throw new Exception("RM_Retention_MoveToAnotherLocationDisabled");
            }
        }
    }
}
