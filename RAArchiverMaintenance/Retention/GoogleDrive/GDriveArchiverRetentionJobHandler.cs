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

using AvePoint.Archiver.Media;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Common;
using AvePoint.Media.Service;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Telemetry;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Telemetry;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.DBLocker;
using AvePoint.RA.DB.Model;
using CsvHelper.Expressions;
using RAArchiverCommon;
using JobMonitorStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;

namespace RAArchiverMaintenance.Retention.GoogleDrive
{
    public class GDriveArchiverRetentionJobHandler
    {
        private static IRALogger _logger = new RALogger(typeof(GDriveArchiverRetentionJobHandler));
        private readonly string _subJobId;
        private string _mainJobId;
        private string _jobContextSetting;
        private JobMonitorStatus _mJobStatus = JobMonitorStatus.Finished;
        private bool HasCompleteNode { get; set; }
        private bool HasErrorNode { get; set; }
        private bool HasStop { get; set; }

        private readonly IJobMonitorService _jobMonitorService = PlatformWindsorManager.GetService<IJobMonitorService>();

        private readonly IRMArchiveGDriveInfoDao _archiveGDriveInfoDao = PlatformWindsorManager.GetService<IRMArchiveGDriveInfoDao>();

        private readonly IArchiverSiteMasterIndexDao _archiverSiteMasterIndexDao = PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        
        private readonly IArchiverIndexSubInfoDao _archiverIndexSubInfoDao = PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
        
        private readonly IRetentionIndexSubInfoDao _retentionIndexSubInfoDao = PlatformWindsorManager.GetService<IRetentionIndexSubInfoDao>();
        
        private readonly IRATelemetryService _raTelemetryService = PlatformWindsorManager.GetService<IRATelemetryService>();
        
        private readonly IRMSubJobDao _subJobDao = PlatformWindsorManager.GetService<IRMSubJobDao>();

        private readonly IArchiverJobDao _archiverJobDao = PlatformWindsorManager.GetService<IArchiverJobDao>();

        private const string AVEPOINT_DEFAULT_STORAGEID = "6A040C17-AF8A-4F1F-96C1-7CEB2E23B1F3";

        private readonly List<string> _successDeleteSubSubJobIds = [];
        
        private readonly List<string> _failedDeleteSubSubJobIds = [];

        private IRMKeyValueDao mKeyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private readonly IRMReportManager _reportManager = ReportMangerFactory.Instance.ReportManager;

        public GDriveArchiverRetentionJobHandler(string jobId, JobType jobType)
        {
            _subJobId = jobId;
            ReportMangerFactory.Instance.Init(jobId, jobType, true);
            MediaEnvironment.MediaServer = MediaServiceFactory.CreateMediaServer();
            MediaConfigInfo.CommonConfigInfo = MediaServiceFactory.CreateCommonConfigInfo();
            MediaConfigInfo.ArchiverConfigInfo = MediaServiceFactory.CreateArchiverConfigInfo();
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
            string comment = string.Empty;
            try
            {
                _reportManager.StartUpdateJobProgress();
                //从子job的Context中获取当前需要处理的节点.
                var subJobWithContext = _subJobDao.GetSubJob(_subJobId, true);
                _mainJobId = subJobWithContext.ParentId;
                _jobContextSetting = subJobWithContext.JobContext?.Settings;
                var archiverPruningJobs = SerializerHelper.DeserializeByDataContractSerializer<List<ArchiverPruningJob>>(_jobContextSetting);
                //Process decrypt secret of connection string for google storage
                GDriveArchiverUtil.DecryptSecretForGoogleStorage(archiverPruningJobs);
                var dateTimeNow = DateTime.UtcNow;
                _logger.Info($"Current date time now is :{dateTimeNow},ticks:{dateTimeNow.Ticks},will user this time to check modified time");
                foreach (var archiverPruningJob in archiverPruningJobs)
                {
                    RARetentionJobTelemetry telemetry = null;
                    try
                    {
                        telemetry = await BuildRARetentionJobTelemetry(archiverPruningJob);
                        _logger.Info($"this archiver pruning job:{archiverPruningJob.JobId},retention data type:{archiverPruningJob.RetentionDataTimeType}");
                        if (archiverPruningJob.RetentionDataTimeType == KeepDateType.None)
                        {
                            _logger.Warn($"this retention info is old data info,need to reset it to archive time.");
                            archiverPruningJob.RetentionDataTimeType = KeepDateType.ArchiveTime;
                        }
                        archiverPruningJob.DateTimeNowTicks = GDriveArchiverUtil.ValidateModifiedTime(archiverPruningJob.KeepValue, archiverPruningJob.ArchiveDateUnit, dateTimeNow);
                        
                        if (!CheckJobIsFileLevelBackup(archiverPruningJob.JobId) && archiverPruningJob.RetentionDataTimeType == KeepDateType.ModifiedTime)
                        {
                            _logger.Warn($"Skip run this info, because the job:{archiverPruningJob.JobId} is not file level backup.");
                            continue;
                        }
                        if (CheckArchiveTierLessThan90Days(archiverPruningJob))
                        {
                            _logger.Warn($"The archive time is less than 90 days, will not delete data.job id:{archiverPruningJob.JobId}");
                            continue;
                        }

                        var lockResult = await SampleDBLocker.TryGet4IndexDBUpdaterForGoogle(archiverPruningJob.SiteUrl, archiverPruningJob.SiteId, _subJobId);
                        if (lockResult.Item1)
                        {
                            using var dbLocker = lockResult.Item2;
                            try
                            {
                                if (archiverPruningJob.CacheSettings == null)
                                {
                                    var archiveTemp = BackgroundSettings.GetInstance().ArchiveTemp;
                                    if (!System.IO.Directory.Exists(archiveTemp))
                                    {
                                        System.IO.Directory.CreateDirectory(archiveTemp);
                                    }
                                    archiverPruningJob.CacheSettings = InitCacheSetting(archiveTemp);
                                    if (archiverPruningJob.RetentionJob != null)
                                    {
                                        archiverPruningJob.RetentionJob.Id = _subJobId;
                                    }
                                }
                                _logger.Info($"Start to run retention job for sub job id:{_subJobId}, retention action:{archiverPruningJob.RetentionAction}, is simulate job:{archiverPruningJob.IsSimulateJob}");
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
                                _logger.Error("Job will stop, Catch JobStopException.");
                                _mJobStatus = JobMonitorStatus.Stopped;
                                comment = e.Message;
                            }
                            catch (Exception e)
                            {
                                _mJobStatus = JobMonitorStatus.FinishWithException;
                                _logger.Error("Error :{0}", e.ToString());
                            }
                        }
                        else
                        {
                            _reportManager.SendJobDetail(GenerateRetentionSkipReport(archiverPruningJob.JobId, archiverPruningJob.RetentionAction, archiverPruningJob.IsSoftDelete));
                            _logger.Info($"Skip run this info, because cannot get site lock.");
                        }
                        _reportManager.Increase();
                    }
                    finally
                    {
                        await EndAndSendRetentionJobTelelmetry(telemetry);
                    }
                }
            }
            finally
            {
                if (_mJobStatus is JobMonitorStatus.Stopped or JobMonitorStatus.Failed)
                {
                    if (string.IsNullOrEmpty(comment))
                    {
                        _reportManager.SetJobFinished(_mJobStatus);
                    }
                    else
                    {
                        _reportManager.SetJobFinished(_mJobStatus, comment);
                    }
                }
                else
                {
                    _reportManager.SetJobFinished(GetJobStatus());
                }
            }
        }

        private async Task<RARetentionJobTelemetry> BuildRARetentionJobTelemetry(ArchiverPruningJob archiverPruningJob)
        {
            try
            {
                RARetentionJobTelemetry telemetry = new()
                {
                    JobId = _subJobId,
                    MainJobId = _mainJobId,
                    RetentionObject = archiverPruningJob.SiteUrl,
                    ArchivedSubJobId = archiverPruningJob.JobId,
                    JobType = ((int)JobType.ArchiverRetention).ToString(),
                    StorageName = archiverPruningJob?.DataLogicalDevice?.Name,
                    RetentionAction = (int)archiverPruningJob.RetentionAction
                };
                ArchiverIndexSubInfo indexSubInfo = await _archiverIndexSubInfoDao.GetSubInfoBySubsubJobIdAsync(archiverPruningJob?.JobId);
                telemetry.MediaDataSize = indexSubInfo?.MediaDataSize ?? 0;
                return telemetry;
            }
            catch(Exception e)
            {
                _logger.Error("Fail build telemetry object :{0}", e.ToString());
            }
            return new();
        }

        private async Task EndAndSendRetentionJobTelelmetry(RARetentionJobTelemetry telemetry)
        {
            try
            {
                ArchiverIndexSubInfo indexSubInfo = await _archiverIndexSubInfoDao.GetSubInfoBySubsubJobIdAsync(telemetry.ArchivedSubJobId);
                telemetry.RemainingMediaDataSize = indexSubInfo?.MediaDataSize ?? 0;
                telemetry.RetentionDataSize = telemetry.MediaDataSize - telemetry.RemainingMediaDataSize;
                await _raTelemetryService.AddTelemetryForRetentionJob(telemetry);
            }
            catch (Exception e) 
            {
                _logger.Error(@$"Fail end and send telemetry, ex:{e}");
            }
        }
        
        public async Task RunDeleteOrphanDatasAsync()
        {
            string commnet = string.Empty;
            try
            {
                _reportManager.StartUpdateJobProgress();
                IRMSubJobDao SubJobDao = new RMSubJobDao();
                IJobMonitorDao JobMonitorDao = new JobMonitorDao();
                RMSubJob subJobWithContext = SubJobDao.GetSubJob(_subJobId, true);
                _mainJobId = subJobWithContext.ParentId;
                _jobContextSetting = subJobWithContext.JobContext?.Settings;
                var archiverPruningJobs = SerializerHelper.DeserializeByDataContractSerializer<List<ArchiverPruningJob>>(_jobContextSetting);
                foreach (var archiverPruningJob in archiverPruningJobs)
                {
                    try
                    {
                        _logger.Info($"this delete orphan datas job:{archiverPruningJob.JobId}");

                        var lockResult = await SampleDBLocker.TryGet4IndexDBUpdater(archiverPruningJob.SiteUrl, archiverPruningJob.SiteId, _subJobId);
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
                                        archiverPruningJob.RetentionJob.Id = _subJobId;
                                    }
                                }
                                DeleteSubJobData(archiverPruningJob);
                            }
                            catch (JobStopException)
                            {
                                _logger.Error("Job will stop, Catch JobStopException.");
                                _mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Stopped;
                            }
                            catch (Exception e)
                            {
                                _mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.FinishWithException;
                                _logger.Error("Error :{0}", e.ToString());
                            }
                        }
                        else
                        {
                            _reportManager.SendJobDetail(GenerateDeleteOrphanDatasReport(archiverPruningJob.JobId));
                            _logger.Info($"Skip run this info, because cannot get site lock.");
                        }
                        _reportManager.Increase();
                    }
                    finally
                    {

                    }
                }
                RemoveSuccessDeletedOrphanDatasSubjob();
            }
            finally
            {
                if (_mJobStatus == AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Stopped || _mJobStatus == AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Failed)
                {
                    if (string.IsNullOrEmpty(commnet))
                    {
                        _reportManager.SetJobFinished(_mJobStatus);
                    }
                    else
                    {
                        _reportManager.SetJobFinished(_mJobStatus, commnet);
                    }
                }
                else
                {
                    _reportManager.SetJobFinished(GetJobStatus());
                }
            }
        }
        private void RemoveSuccessDeletedOrphanDatasSubjob()
        {
            try
            {
                foreach (string subsubJobid in _successDeleteSubSubJobIds)
                {
                    if (_failedDeleteSubSubJobIds.Contains(subsubJobid))
                    {
                        _logger.Warn($"there exsit something wrong when delete orphan datas job,so donot delete this subjob record,id:{subsubJobid}");
                    }
                    else
                    {
                        _logger.Info($"delete orphan datas job success,delete subjob record,id:{subsubJobid}");
                        _subJobDao.DeleteSubJobById(subsubJobid.Substring(0, subsubJobid.LastIndexOf("_")));
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error($"something wrong when remove success delete orphan datas subjob,error:{e}");
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
                            _logger.Info($"current job has archived more than 90days,should process,archive time:{archiveTime},job id:{archiverPruningJob.JobId}");
                            return false;
                        }
                        else
                        {
                            _logger.Info($"current job has archived less than 90days,should not process,archive time:{archiveTime},job id:{archiverPruningJob.JobId}");
                            return true;
                        }
                    }
                    else
                    {
                        _logger.Info($"current job has archived less than 90days,but not avepoint storage,process it,archive time:{archiveTime},job id:{archiverPruningJob.JobId}");
                        return false;
                    }
                }
                else
                {
                    _logger.Info($"not retention by archive time,return false");
                    return false;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"something went wrong when check archive job less than 90days,error:{e}");
                return true;
            }
        }
        private bool CheckJobIsFileLevelBackup(string subSubjobId)
        {
            var subJobId = GetSubJobId(subSubjobId);
            return _archiverSiteMasterIndexDao.IsFileLevelBlockBackup(subJobId);
        }

        private CacheSettingDto InitCacheSetting(string path)
        {
            string cachePath = path;
            CacheSettingDto cache = new CacheSettingDto
            {
                Extension = new CacheSettingExtension
                {
                    Path =
                    [
                        new PathMap()
                        {
                            DiskInfo = new DiskInfoDto()
                        }
                    ]
                }
            };
            cache.Extension.Path[0].DiskInfo.Path = cachePath;
            return cache;
        }
        private JMArchiverRententionJobDetails GenerateRetentionSkipReport(string subinfojobId,MediaArchiverRetentionAction action,bool isSoft)
        {
            try
            {
                var report = new JMArchiverRententionJobDetails();
                string subJobId = GetSubJobId(subinfojobId);
                if (string.IsNullOrEmpty(subJobId))
                {
                    _logger.Warn($"get sub job id failed,subIndexInfo job id is:{subinfojobId}");
                    return null;
                }
                List<ArchiverSiteMasterIndexContract> masterIndexs = _archiverSiteMasterIndexDao.GetIndexByJobId(subJobId);
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
                _logger.Error($"something wrong with Generate Retention Skip Report,error :{e.ToString()}");
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
                    _logger.Warn($"get sub job id failed,subIndexInfo job id is:{subinfojobId}");
                    return null;
                }
                List<ArchiverSiteMasterIndexContract> masterIndexs = _archiverSiteMasterIndexDao.GetIndexByJobId(subJobId);
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
                _logger.Error($"something wrong with Generate Retention Skip Report,error :{e.ToString()}");
                return null;
            }
        }
        public void DeleteSubJobData(ArchiverPruningJob archiverRetentionInfo)
        {
            _logger.Info($"DeleteSubJobData : Google Drive Id:{archiverRetentionInfo.SiteUrl}");
            ArchiverRetentionResult result = null;
            string backupSubJobId = archiverRetentionInfo.JobId.Substring(0, archiverRetentionInfo.JobId.LastIndexOf("_", StringComparison.CurrentCulture));
            try
            {
                result = RealDeleteSubJobData(archiverRetentionInfo, backupSubJobId);
            }
            catch (Exception e)
            {
                _logger.Error($"DeleteSubJobData Error. {e.ToString()}");
            }
            //删除sub info表记录, 如果主表对应的子表记录全部删除, 则删除主表记录.

            var subIndex = _archiverIndexSubInfoDao.Find(i => i.SubSubJobId == archiverRetentionInfo.JobId);
            if (archiverRetentionInfo.RetentionDataTimeType != KeepDateType.ModifiedTime && result?.HasIndexRelatedToBackupJob == false)
            {
                if (archiverRetentionInfo.IsSoftDelete && archiverRetentionInfo.IsFitSoftDelete && !archiverRetentionInfo.IsSystemStorage)
                {
                    _logger.Info($"this index sub info is fit soft delete,will delete it,{subIndex?.SubSubJobId}");
                    _archiverIndexSubInfoDao.Delete(subIndex);
                }
                else if (!archiverRetentionInfo.IsSoftDelete || archiverRetentionInfo.IsSystemStorage)
                {
                    _logger.Info($"delete sub job will delete it,{subIndex?.SubSubJobId}");
                    _archiverIndexSubInfoDao.Delete(subIndex);
                }
                else
                {
                    _logger.Info($"do not delete this subinfo:{subIndex?.SubSubJobId},becaused it has set soft delete");
                }
            }
            if (!archiverRetentionInfo.IsFitSoftDelete && archiverRetentionInfo.IsSoftDelete && (subIndex.DeletedStatus == (int)DeletedStatus.Normal || subIndex.DeletedStatus == (int)DeletedStatus.Restored) && archiverRetentionInfo.RetentionDataTimeType != KeepDateType.ModifiedTime && !archiverRetentionInfo.IsSystemStorage)
            {
                subIndex.DeletedStatus = (int)DeletedStatus.SoftDelete;
                subIndex.SoftDeleteTime = DateTime.UtcNow.Ticks;
                _archiverIndexSubInfoDao.UpdateAsync(subIndex).GetAwaiter().GetResult();
            }

            if (result != null && result.State == 2)
            {
                try
                {
                    var subInfosCount = _archiverIndexSubInfoDao.GetSubInfoCountAsync(backupSubJobId).GetAwaiter().GetResult();
                    var job = _archiverSiteMasterIndexDao.Find(i => i.JobId == backupSubJobId);
                    var retentionSubInfo = GenaratRetentionSubInfo(subIndex, job);
                    _retentionIndexSubInfoDao.InsertIntoRetentionIndexSubInfo(retentionSubInfo);
                    if (subInfosCount == 0)
                    {
                        if (job != null)
                        {
                            _logger.Info("Archiver Site Master Index with job id {0}, whose SubInfo is null or empty, has been deleted after retention job.", backupSubJobId);
                            _archiverSiteMasterIndexDao.Delete(job);
                        }
                        else
                        {
                            _logger.Info("Archiver Site Master Index with job id {0}, but cannot find in table.", backupSubJobId);
                        }
                    }
                    else
                    {
                        _logger.Info("SubInfo count of sub job id {0} is {1}", backupSubJobId, subInfosCount);
                    }
                    if (archiverRetentionInfo.RetainType == RetainType.DeleteOrphanDatas)
                    {
                        _logger.Info("this delete action is delete orphan job.");
                        _successDeleteSubSubJobIds.Add(archiverRetentionInfo.JobId);
                    }

                    //需要删除job
                    if (archiverRetentionInfo.IsDeleteJob)
                    {
                        _logger.Info("Delete data {0}, delete sub info.", archiverRetentionInfo.JobId);
                        var jobMonitorJobId = backupSubJobId.Substring(0, archiverRetentionInfo.JobId.IndexOf("_", StringComparison.CurrentCulture));
                        _jobMonitorService.DeleteJobsAsync(new List<string> { jobMonitorJobId }).GetAwaiter().GetResult();
                        _logger.Info("Delete job record successful");

                        try
                        {
                            var archiverJob = _archiverJobDao.GetJobByID(jobMonitorJobId);
                            if (archiverJob != null)
                            {
                                _jobMonitorService.DeleteJobsAsync(new List<string> { archiverJob.RECOJobId }).GetAwaiter().GetResult();
                                _archiverJobDao.Delete(archiverJob);
                                _logger.Info("Delete archiver job record successful");
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.Error($"Delete Archiver Job Error,{e}");
                        }
                    }
                }
                catch (Exception e)
                {
                    _logger.Warn(e.ToString());
                    result.State = 1;
                }
            }
            UpdateArchiverSize(archiverRetentionInfo.SiteId);
            if (result != null && result.State == 2)
            {
                if (archiverRetentionInfo.RetentionDataTimeType != KeepDateType.ModifiedTime)
                {
                    if (archiverRetentionInfo.RetainType == RetainType.DeleteOrphanDatas)
                    {
                        _logger.Info($"this retention job is delete orphan datas job");
                        var report = new JMDeleteOrphanDatasJobDetails();
                        report.SiteUrl = archiverRetentionInfo.SiteUrl;
                        report.Size = result.Size.ToString();
                        report.Status = JobDetailsStatus.Successful;
                        report.JobId = archiverRetentionInfo.JobId;
                        _reportManager.SendJobDetail(report);
                    }
                    else
                    {
                        if (archiverRetentionInfo.SoftDeleteTime > 0)
                        {
                            _logger.Info($"this job has soft deleted,no need to add detail,job id:{archiverRetentionInfo.JobId}");
                        }
                        else
                        {
                            var report = new JMArchiverRententionJobDetails
                            {
                                SiteUrl = archiverRetentionInfo.FarmName,
                                Size = result.Size.ToString(),
                                Status = JobDetailsStatus.Successful,
                                SrcStorageName = archiverRetentionInfo.DataLogicalDevice.Name,
                                JobId = archiverRetentionInfo.JobId,
                                Action = (!archiverRetentionInfo.IsFitSoftDelete && archiverRetentionInfo.IsSoftDelete) ? "RM_AR_CP_GSS_Retention_SoftDelete" :"RM_JS_Common_Delete"
                            };
                            _reportManager.SendJobDetail(report);
                        }
                    }
                }
                else
                {
                    _logger.Info($"this retention job is retention by modified time,so not job level detail");
                }
            }
            else
            {
                if (archiverRetentionInfo.RetentionDataTimeType != KeepDateType.ModifiedTime)
                {
                    if (archiverRetentionInfo.RetainType == RetainType.DeleteOrphanDatas)
                    {
                        _logger.Info($"this retention job is delete orphan datas job1");
                        var report = new JMDeleteOrphanDatasJobDetails();
                        report.SiteUrl = archiverRetentionInfo.SiteUrl;
                        report.Size = "0";
                        report.Status = JobDetailsStatus.Failed;
                        report.JobId = archiverRetentionInfo.JobId;
                        _reportManager.SendJobDetail(report);
                    }
                    else
                    {
                        var report = new JMArchiverRententionJobDetails
                        {
                            SiteUrl = archiverRetentionInfo.FarmName,
                            Size = "0",
                            Status = JobDetailsStatus.Failed,
                            JobId = archiverRetentionInfo.JobId,
                            SrcStorageName = archiverRetentionInfo.DataLogicalDevice.Name,
                            Action = (!archiverRetentionInfo.IsFitSoftDelete && archiverRetentionInfo.IsSoftDelete) ? "RM_AR_CP_GSS_Retention_SoftDelete" : "RM_JS_Common_Delete"
                        };
                        _reportManager.SendJobDetail(report);
                    }
                }
                else
                {
                    _logger.Info($"this retention job is retention by modified time,so not job level detail and it has error");
                }
                _failedDeleteSubSubJobIds.Add(archiverRetentionInfo.JobId);
            }
        }

        public void MarkSubJobDataTier(ArchiverPruningJob archiverRetentionInfo)
        {
            _logger.Info($"MarkSubJobDataTier : Drive Id:{archiverRetentionInfo.SiteUrl}");
            ArchiverRetentionResult result = null;
            string errorMessage = string.Empty;
            try
            {
                result = RealMarkDataTier(archiverRetentionInfo);
            }
            catch (Exception e)
            {
                _logger.Error($"MarkSubJobDataTier Error. {e}");
                errorMessage = e.Message;
            }
            if (result is { State: 2 })
            {
                try
                {
                    //更新sub master index表中logical device id和retention time
                    var subIndex = _archiverIndexSubInfoDao.Find(i => i.SubSubJobId == archiverRetentionInfo.JobId);
                    if (subIndex != null)
                    {
                        _logger.Info("Move data {0}, update retention time.", archiverRetentionInfo.JobId);
                        subIndex.RetentionTime = DateTime.UtcNow.Ticks;
                        subIndex.RetentionCount++;
                        _archiverIndexSubInfoDao.UpdateAsync(subIndex).GetAwaiter().GetResult();
                        _logger.Info("Update sub master index successful");
                    }
                }
                catch (Exception e)
                {
                    _logger.Warn(e.ToString());
                    result.State = 1;
                }
                var report = new JMArchiverRententionJobDetails
                {
                    SiteUrl = archiverRetentionInfo.FarmName,
                    Size = string.Empty, //result.Size.ToString();
                    Status = JobDetailsStatus.Successful,
                    SrcStorageName = archiverRetentionInfo.DataLogicalDevice.Name,
                    JobId = archiverRetentionInfo.JobId,
                    Action = "RM_AR_CP_GSS_Retention_MarkDataTier",
                    Comment = result.IsArchiveTierToColdTier? "RM_AR_CP_GSS_Retention_ChangeToColdFromArchive":string.Empty
                };
                _reportManager.SendJobDetail(report);
            }
            else
            {
                var report = new JMArchiverRententionJobDetails
                {
                    SiteUrl = archiverRetentionInfo.FarmName,
                    Size = string.Empty, //"0";
                    Status = JobDetailsStatus.Failed,
                    JobId = archiverRetentionInfo.JobId,
                    Comment = errorMessage,
                    Action = "RM_AR_CP_GSS_Retention_MarkDataTier"
                };
                _reportManager.SendJobDetail(report);
                _mJobStatus = JobMonitorStatus.Failed;

            }
            //ThreadPool.QueueUserWorkItem(state =>
            //{
            //    RealDeleteSubJobData(state as ArchiverPruningJob);
            //}, archiverRetentionInfo);
        }
        private void UpdateArchiverSize(string driveId)
        {
            try
            {
                _logger.Info("start update archiver size for archiver info");
                var driveIdAndJobIdMapping = _archiverSiteMasterIndexDao.GetAllBackupGDriveDistinctJobIdMappings(new List<string>() { driveId });
                var driveIdAndSizeMapping = _archiverIndexSubInfoDao.GetAllGoogleArchiverIndexSubInfoByDriveIds(driveIdAndJobIdMapping);
                _archiveGDriveInfoDao.UpdateGoogleArchiveInfo(driveId, driveIdAndSizeMapping[driveId]);
            }
            catch (Exception e)
            {
                _logger.Warn($"SomeThing went wrong when update archiver size in archiverInfo,error :{e.ToString()}");
            }
        }
        private RetentionIndexSubInfo GenaratRetentionSubInfo(ArchiverIndexSubInfo subInfo, AvePoint.RA.DB.Model.ArchiverSiteMasterIndex siteMasterIndex)
        {
            RetentionIndexSubInfo result = new RetentionIndexSubInfo();
            result.Id = Guid.NewGuid().ToString();
            result.RetentionTime = subInfo.RetentionTime;
            result.JobId = this._subJobId;
            result.ArchiverJobId = subInfo.SubSubJobId;
            result.SiteGroupId = siteMasterIndex.SiteGroupId;
            result.SiteURL = siteMasterIndex.SiteURL;
            result.SiteId = siteMasterIndex.SiteId;
            return result;
        }
        public async Task ExportDataToAnotherDeviceAsync(ArchiverPruningJob archiverRetentionInfo)
        {
            _logger.Info($"ExportDataToAnotherDevice : Drive Id:{archiverRetentionInfo.SiteUrl}");

            ArchiverRetentionResult result = null;
            try
            {
                result = RealExportDataToAnotherDevice(archiverRetentionInfo);
            }
            catch (JobStopException e)
            {
                _logger.Warn("Job will stop, throw JobStopException.");
                throw;
            }
            catch (Exception e)
            {
                _logger.Error($"ExportDataToAnotherDevice Error. {e}");
            }
            if (result is { State: 2 })
            {
                try
                {
                    //更新sub master index表中logical device id和retention time
                    var subIndex = _archiverIndexSubInfoDao.Find(i => i.SubSubJobId == archiverRetentionInfo.JobId);
                    if (subIndex != null)
                    {
                        _logger.Info("Move data {0}, update retention time.", archiverRetentionInfo.JobId);
                        subIndex.CurrentStorageId = archiverRetentionInfo.DestinationPhysicalDeviceId;
                        subIndex.RetentionTime = DateTime.UtcNow.Ticks;
                        subIndex.RetentionCount++;
                        await _archiverIndexSubInfoDao.UpdateAsync(subIndex);
                        _logger.Info("Update sub master index successful");
                    }
                }
                catch (Exception e)
                {
                    _logger.Warn(e.ToString());
                    result.State = 1;
                }
            }
            if (result is { State: 2 })
            {
                var report = new JMArchiverRententionJobDetails
                {
                    SiteUrl = archiverRetentionInfo.FarmName,
                    Size = result.Size.ToString(),
                    Status = JobDetailsStatus.Successful,
                    SrcStorageName = archiverRetentionInfo.DataLogicalDevice.Name,
                    DesStorageName = archiverRetentionInfo.DestinationDevice?.Name,
                    JobId = archiverRetentionInfo.JobId,
                    Action = GetJobDetailsActionForMoveData()
                };
                _reportManager.SendJobDetail(report);
            }
            else
            {
                var report = new JMArchiverRententionJobDetails
                {
                    SiteUrl = archiverRetentionInfo.FarmName,
                    Size = "0",
                    Status = JobDetailsStatus.Failed,
                    JobId = archiverRetentionInfo.JobId,
                    Action = GetJobDetailsActionForMoveData()
                };
                _reportManager.SendJobDetail(report);
                _mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Failed;

            }
        }


        private ArchiverRetentionResult RealDeleteSubJobData(ArchiverPruningJob archiverRetentionInfo, string backupSubJobId)
        {
            ValidSoftDeleteIsFitRetainByModifiedTime(archiverRetentionInfo);
            var retentionInfo = new GoogleArchiverRetentionInfo(archiverRetentionInfo);
            if (archiverRetentionInfo.RetainType != RetainType.DeleteOrphanDatas)
            {
                retentionInfo.IsFileLevelBlockBackup = _archiverSiteMasterIndexDao.IsFileLevelBlockBackup(backupSubJobId);
            }
            retentionInfo.RetentionRule = AvePoint.Media.Service.DomainModel.RetentionRule.RetainArchiverJobData;
            var retentionService = (IRetentionService)PlatformWindsorManager.GetService("AvePoint.Media.Service.ArchiverBackup.GDriveArchiverBackupRetentionService", typeof(IRetentionService));
            var result = retentionService.Retain(retentionInfo, SendJobReport) as ArchiverRetentionResult;
            return result;
        }
        private void ValidSoftDeleteIsFitRetainByModifiedTime(ArchiverPruningJob archiverRetentionInfo)
        {
            if (archiverRetentionInfo is { IsSoftDelete: true, SoftDeleteTime: > 0, IsFitSoftDelete: false })
            {
                archiverRetentionInfo.IsFitSoftDelete = ValidateRetentionTime(archiverRetentionInfo.SoftDeleteTime, archiverRetentionInfo.SoftDeleteKeepValue, archiverRetentionInfo.SoftDeleteDateUnit);
            }

        }
        private bool ValidateRetentionTime(long retentionTimeTicks, int keepValue, DateUnit dateUnit)
        {
            if (keepValue < 0)
            {
                _logger.Info($"keep value is zero,return false");
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
            _logger.Info($"ValidatesoftTime.RetentionTime {retentionTime.Ticks}");
            return retentionTime.Ticks <= DateTime.UtcNow.Ticks;
        }
        private ArchiverRetentionResult RealExportDataToAnotherDevice(ArchiverPruningJob archiverRetentionInfo)
        {
            var retentionInfo = new GoogleArchiverRetentionInfo(archiverRetentionInfo)
            {
                RetentionRule = AvePoint.Media.Service.DomainModel.RetentionRule.MoveArchiverJobData
            };
            var retentionService = (IRetentionService)PlatformWindsorManager.GetService("AvePoint.Media.Service.ArchiverBackup.GDriveArchiverBackupRetentionService", typeof(IRetentionService));
            var result = retentionService.Retain(retentionInfo, SendJobReport) as ArchiverRetentionResult;
            return result;
        }
        private ArchiverRetentionResult RealMarkDataTier(ArchiverPruningJob archiverRetentionInfo)
        {
            var retentionInfo = new GoogleArchiverRetentionInfo(archiverRetentionInfo)
            {
                RetentionRule = AvePoint.Media.Service.DomainModel.RetentionRule.MarkArchiverJobDataTier
            };
            var retentionService = (IRetentionService)PlatformWindsorManager.GetService("AvePoint.Media.Service.ArchiverBackup.GDriveArchiverBackupRetentionService", typeof(IRetentionService));
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

        private JobMonitorStatus GetJobStatus()
        {
            if (HasStop || CheckJobStatusUtility.isStopping)
            {
                _mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Stopped;
            }
            else if (HasCompleteNode && !HasErrorNode)
            {
                _mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Finished;
            }
            else if (HasCompleteNode && HasErrorNode)
            {
                _mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.FinishWithException;
            }
            else if (!HasCompleteNode && !HasErrorNode)
            {
                _mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Finished;
            }
            else if (!HasCompleteNode && HasErrorNode)
            {
                _mJobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Failed;
            }
            return _mJobStatus;
        }

        private void SendJobReport(JMArchiverRententionJobDetails rententionJobDetails)
        {
            AnalyzeStatus(rententionJobDetails.Status);
            _reportManager.SendJobDetail(rententionJobDetails);
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
                _logger.Error(errorMessage);
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
