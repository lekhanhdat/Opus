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
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.RMSharePointSettings.AuditHandler;
using AvePoint.RA.Service.Services.SharePointSetting.AuditHandler;
using AvePoint.RA.Contract.DocAve;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using AvePoint.RA.Service.Services.Settings.AuditHandler;
using AvePoint.GCommon.Contract.StorageOptimization.Connector;
using AvePoint.RA.Contract.Explorer;

namespace AvePoint.RA.Service.Services.RMPhysicalRecordSettings
{
    [Audit]
    public class RMPhysicalRecordSettingsService : RMServiceBase, IRMPhysicalRecordSettingsService
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMPhysicalRecordSettingsService));

        private IScheduleService _scheduleService => PlatformWindsorManager.GetService<IScheduleService>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        #region All Dao
        private IPhysicalRecordSettingDao _physicalRecordSettingDao => PlatformWindsorManager.GetService<IPhysicalRecordSettingDao>();
        private IRMLocationDao _locationDao => PlatformWindsorManager.GetService<IRMLocationDao>();

        private IRecordOwnerDao _recordOwnerDao => PlatformWindsorManager.GetService<IRecordOwnerDao>();
        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IRMJobService RunJobService => PlatformWindsorManager.GetService<IRMJobService>();
        private IJobQueueService mJobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private ITermDao termDao => PlatformWindsorManager.GetService<ITermDao>();
        private DB.Explorer.Dao.IExplorerDao explorerDao = new DB.Explorer.Dao.CosmosImp.ExplorerDao(true);
        #endregion
        public async Task<RMPRTreeNode> LoadPhysicalRecordSettingAsync(Guid locationUID)
        {
            var treeNode = new RMPRTreeNode
            {
                UniqueId = locationUID,
                IconStatus = IconStatus.NoSet
            };
            try
            {
                var dbLocation = _locationDao.GetLocationByUniqueId(locationUID);
                bool isTopLevelLocation;
                Guid topLevelLocationUniqueId;
                List<string> locationDirPathIds;
                CheckIsTopLevelSetting(dbLocation.DirPath, out isTopLevelLocation, out topLevelLocationUniqueId, out locationDirPathIds);
                treeNode.IsTopLevelSetting = isTopLevelLocation;
                var dbNode = _physicalRecordSettingDao.GetPhysicalRecordSetting(locationUID);
                if (dbNode != null)
                {
                    treeNode.IsCustomSetting = !isTopLevelLocation;
                    treeNode.IconStatus = IconStatus.Break;
                }
                else
                {
                    treeNode.IsCustomSetting = false;
                    if (!isTopLevelLocation && locationDirPathIds != null)
                    {
                        dbNode = _physicalRecordSettingDao.GetAncestryPhysicalRecordSetting(locationDirPathIds);
                    }
                    if (dbNode != null)
                    {
                        treeNode.IconStatus = isTopLevelLocation ? IconStatus.NoSet : IconStatus.Inhert;
                    }
                }

                if (string.IsNullOrEmpty(treeNode.ColumnName))
                {
                    //first config ColumnRequired default value is yes
                    treeNode.ColumnRequired = true;
                }
                if (dbNode != null)
                {
                    if (isTopLevelLocation)
                    {
                        treeNode.TopLevelSettingUniqueId = locationUID;
                        treeNode.ColumnName = dbNode.ColumnName;
                        treeNode.ColumnRequired = dbNode.ColumnRequired;
                        treeNode.EMailToRecordOwner = dbNode.EMailToRecordOwner;
                        treeNode.RecordOwner = await _recordOwnerDao.GetRecordOwnerAccountsAsync(dbNode.Id, RecordOwnerSettingType.PhysicalRecord);
                        treeNode.WorkflowReferenceId = dbNode.WorkflowReferenceId;
                        treeNode.ApprovalType = (int)dbNode.ApprovalType;
                    }
                    else
                    {
                        treeNode.TopLevelSettingUniqueId = topLevelLocationUniqueId;
                        var topLevelSetting = _physicalRecordSettingDao.GetPhysicalRecordSetting(topLevelLocationUniqueId);
                        treeNode.ColumnName = topLevelSetting.ColumnName;
                        treeNode.ColumnRequired = topLevelSetting.ColumnRequired;
                        treeNode.EMailToRecordOwner = topLevelSetting.EMailToRecordOwner;
                        treeNode.RecordOwner = await _recordOwnerDao.GetRecordOwnerAccountsAsync(topLevelSetting.Id, RecordOwnerSettingType.PhysicalRecord);
                        treeNode.WorkflowReferenceId = topLevelSetting.WorkflowReferenceId;
                        treeNode.ApprovalType = (int)topLevelSetting.ApprovalType;
                    }
                    var termDefaultValue = termDao.GetRMTermByGuId(dbNode.DefaultTermId);
                    RMTermSet termSet = null;
                    RMTerm termScope = null;
                    if (dbNode.TermId == Guid.Empty)
                    {
                        termSet = termDao.GetRMTermSetByGuid(dbNode.TermSetId);
                    }
                    else
                    {
                        termScope = termDao.GetRMTermByGuId(dbNode.TermId);
                    }
                    treeNode.IsTermRemoved = (termScope == null ? termSet?.IsRemoved : termScope?.IsRemoved) ?? false;
                    treeNode.IsDefaultTermRemoved = termDefaultValue == null ? false : termDefaultValue.IsRemoved;
                    treeNode.IsDefaultTermDeprecated = termDefaultValue == null ? false : termDefaultValue.IsDeprecated || termDao.IsExpiredTerm(termDefaultValue.Id);
                    treeNode.DefaultTermFullPath = dbNode.DefaultTermId != Guid.Empty ? termDao.GetTermNamesPathByTermId(dbNode.DefaultTermId) : "";
                    treeNode.DefaultTermName = dbNode.DefaultTermName;
                    treeNode.DefaultTermId = dbNode.DefaultTermId;
                    treeNode.TermName = dbNode.TermName;
                    treeNode.TermId = dbNode.TermId;
                    treeNode.TermSetId = dbNode.TermSetId;
                    treeNode.TermSetName = dbNode.TermSetName;
                    treeNode.TermScopeFullPath = dbNode.TermId != Guid.Empty ? termDao.GetTermNamesPathByTermId(dbNode.TermId) : termDao.GetTermSetNamesPathByTermSetId(dbNode.TermSetId);
                    treeNode.DeployTermMethod = (DeployTermMethod)dbNode.DeployTermMethod;
                }
                else
                {
                    treeNode.TopLevelSettingUniqueId = isTopLevelLocation ? locationUID : topLevelLocationUniqueId;
                }
                var profileId = this.GetProfileId(locationUID);
                var disposeSchedule = await _scheduleService.GetScheduleAsync(profileId, ScheduleType.PRDisposalSchedule);
                if (disposeSchedule != null)
                {
                    var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(disposeSchedule.TimeZoneId);
                    disposeSchedule.StartTime = string.Format($"{disposeSchedule.StartTime} {simplifyZoneInfo}");
                    disposeSchedule.EndTime = string.Format($"{disposeSchedule.EndTime} {simplifyZoneInfo}");
                    treeNode.DisposeScheduleInfo = disposeSchedule;
                    treeNode.IconStatus = IconStatus.Break;
                }
                else
                {
                    var ancestryDisposeSchedule = await _scheduleService.GetAncestryScheduleAsync(profileId, ScheduleType.PRDisposalSchedule);
                    if (ancestryDisposeSchedule != null)
                    {
                        var simplifyZoneInfo = DateTimeUtil.GetSimplifyZoneInfo(ancestryDisposeSchedule.TimeZoneId);
                        ancestryDisposeSchedule.StartTime = string.Format($"{ancestryDisposeSchedule.StartTime} {simplifyZoneInfo}");
                        ancestryDisposeSchedule.EndTime = string.Format($"{ancestryDisposeSchedule.EndTime} {simplifyZoneInfo}");
                        treeNode.DisposeScheduleInfo = ancestryDisposeSchedule;
                        treeNode.DisposeScheduleInfo.Id = "1";//回显先祖的schedule给假ID，防止删除schedule将先祖的删掉
                    }
                    else
                    {
                        treeNode.DisposeScheduleInfo = null;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("load PhysicalRecord setting error : {0}", e.ToString());
            }
            return treeNode;
        }

        public void CheckIsTopLevelSetting(string locationDirPath, out bool isTopLevelLocation, out Guid topLevelLocationUniqueId, out List<string> locationIds)
        {
            isTopLevelLocation = false;
            topLevelLocationUniqueId = default(Guid);
            locationIds = new List<string>();
            if (!string.IsNullOrEmpty(locationDirPath))
            {
                locationIds = locationDirPath.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (locationIds.Count > 0)
                {
                    if (locationIds.Count == 1)
                    {
                        isTopLevelLocation = true;
                    }
                    else
                    {
                        isTopLevelLocation = false;
                        //DirPath --> "1/2/3/"
                        //1 is root
                        //2 is topLevelSetting
                        var topLevelLocation = _locationDao.GetLocationById(Convert.ToInt32(locationIds[1]));
                        topLevelLocationUniqueId = topLevelLocation.UniqueId;
                    }
                }
            }
        }

        public string SaveColumn(Guid locationUID, string columnName, bool columnRequired = true)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                _physicalRecordSettingDao.SaveColumnName(locationUID, columnName, columnRequired);
            }
            catch (Exception e)
            {
                logger.Error("save column error: {0}", e.ToString());
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = e.Message;
            }
            return JsonConvert.SerializeObject(result);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings,
            Action = AuditAction.EditPRTermSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public RAReturnMessage SaveTerm(RMPRSaveTermDto termDto)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                _physicalRecordSettingDao.SaveTerm(termDto);
            }
            catch (Exception e)
            {
                logger.Error("save column error: {0}", e.ToString());
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = e.Message;
            }
            return result;
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings,
            Action = AuditAction.EditPRLocationOwnersSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public async Task<RAReturnMessage> SaveRecordOwnerAsync(RMPRSaveRecordOwnerDto recordOwnerDto)
        {
            RAReturnMessage result = new RAReturnMessage();
            try
            {
                await _physicalRecordSettingDao.SaveRecordOwnerAsync(recordOwnerDto);
            }
            catch (Exception e)
            {
                logger.Error("save column error: {0}", e.ToString());
                result.MessageType = RAMessageType.Failed;
                result.ErrorMessage = e.Message;
            }
            return result;
        }

        public string GetProfileId(Guid locationUid)
        {
            var location = _locationDao.GetLocationByUniqueId(locationUid);
            var locationIds = location.DirPath.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries).ToList();
            var profileIds = new List<Guid>();
            //需要优化，只访问一次DB
            foreach (var id in locationIds)
            {
                profileIds.Add(_locationDao.GetLocationById(Convert.ToInt32(id)).UniqueId);
            }
            profileIds.Add(locationUid);
            return string.Join("|", profileIds);
        }

        [Audit(Module = AuditModule.BusinessClassificationManagement, Category = AuditCategory.SharePointSettings,
            Action = AuditAction.EditPRInheritSetting, BeforeHandler = typeof(SharePointSettingBeforeAuditHandler), AfterHandler = typeof(SharePointSettingAfterAuditHandler))]
        public int InheritParentSetting(Guid locationUid)
        {
            return _physicalRecordSettingDao.InheritParentSetting(locationUid);
        }

        /// <summary>
        /// Physical job 已经挪到DAO， 此方法废弃。挪到DAO 后不走Record 的job queue
        /// </summary>
        /// <param name="profileId"></param>
        /// <param name="jobRunBy"></param>
        /// <returns></returns>
        [Obsolete]
        public string RunPhysicalDisposalScheduleJob(string profileId, JobRunBy jobRunBy)
        {
            string id = string.Empty;
            var jobType = AvePoint.RA.Contract.JobMonitor.JobType.PhysicalDisposal;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = profileId,
                };
                id = mJobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while run physical explorer timer job, ERROR : {ex.ToString()}.");
            }
            return id;
        }

        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.DisposalActivityManagement, Action = AuditAction.RunPRDisposalJob,
           AfterHandler = typeof(DisposalActivityManagementAfterAuditHandler))]
        public RAReturnMessage RealRunPhysicallDisposalScheduleJob(string param, JobRunBy JobRunType)
        {
            RAReturnMessage msg = new RAReturnMessage();
            string jobId = string.Empty;
            string jobRunByUser = JobRunType == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
            try
            {
                if (JobRunType == JobRunBy.Schedule)
                {
                    var guids = param.Split('|').ToList();
                    Guid locationUniqueId = new Guid(guids.Last());
                    var locationInt = _locationDao.GetLocationByUniqueId(locationUniqueId);
                    jobId = RMJobService.CreateJob(AvePoint.RA.Contract.JobMonitor.JobType.PhysicalDisposal, jobRunByUser);
                    mJobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = jobId,
                        RunBy = JobRunBy.Schedule,
                        JobType = AvePoint.RA.Contract.JobMonitor.JobType.PhysicalDisposal,
                        CommandLine = string.Format("{0} {1} {2}", AvePoint.RA.Contract.JobMonitor.JobType.PhysicalDisposal, jobId, locationInt.Id),
                    });
                }
                else
                {
                    jobId = RMJobService.CreateJob(AvePoint.RA.Contract.JobMonitor.JobType.PhysicalDisposal, jobRunByUser);
                    int locationIntId = Convert.ToInt32(param);
                    mJobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = jobId,
                        RunBy = JobRunBy.Schedule,
                        JobType = AvePoint.RA.Contract.JobMonitor.JobType.PhysicalDisposal,
                        CommandLine = string.Format("{0} {1} {2}", AvePoint.RA.Contract.JobMonitor.JobType.PhysicalDisposal, jobId, locationIntId),
                    });
                }
            }
            catch (Exception e)
            {
                logger.Error($"Run real physical disposal schedule Error:{e.ToString()}");
                msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }
            msg.Extension = jobId; //jobId;
            return msg;
        }

        /// <summary>
        /// 废弃方法
        /// </summary>
        /// <param name="profileId"></param>
        public void RunPhysicalDisposalScheduleJob(string profileId)
        {
            try
            {
                var guids = profileId.Split('|').ToList();
                Guid locationUniqueId = new Guid(guids.Last());
                var locationIntId = _locationDao.GetLocationByUniqueId(locationUniqueId);
                RunPhysicalDisposalJob(locationIntId.Id, JobRunBy.Schedule);
            }
            catch (Exception e)
            {
                logger.Error($"Run physical disposal schedule Error:{e.ToString()}");
            }
        }

        //[Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.DisposalActivityManagement, Action = AuditAction.RunPRDisposalJob,
            //AfterHandler = typeof(DisposalActivityManagementAfterAuditHandler))]
        public async Task<RAReturnMessage> RunPhysicalRecordsDisposalJobAsync(int locationId, JobRunBy jobRunBy, bool skipRemoveContentAndDestroyAction)
        {
            logger.Debug("start physical records disposal job");
            string id = string.Empty;
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                List<JobType> types = new List<JobType>() { JobType.PhysicalRecordsDisposal };
                var rules = await RunJobService.AssembleTermRuleMappingAsync(Contract.Explorer.SourceFlag.Physical);
                if (!rules.Any())
                {
                    msg.MessageType = RAMessageType.Failed;
                    msg.ErrorMessage = I18NEntity.GetString("RM_JS_DAM_RunJob_Failed_NoRules");
                    logger.Warn($"Physical rule has no associated term.");
                    return msg;
                }
                if (RMJobService.HasRunningArchiverJobOnScope(types, locationId.ToString()))
                {
                    msg.MessageType = RAMessageType.Failed;
                    //此处的提示信息与EXO使用同一个
                    msg.ErrorMessage = I18NEntity.GetString("RM_Job_ScheduledJobConflict");
                    logger.Warn($"Already has a job running on current node:{locationId}");
                    return msg;
                }
                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.PhysicalRecordsDisposal,
                    JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    Parameters = String.Format("{0} {1} {2}", locationId.ToString(), jobRunBy, skipRemoveContentAndDestroyAction)
                };

                id = mJobQueueService.AddToDBJobQueue(jqDto);
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while running physical records disposal job,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.DisposalActivityManagement, Action = AuditAction.RunPRDisposalJob,
           AfterHandler = typeof(DisposalActivityManagementAfterAuditHandler))]
        public string RealRunPhysicalRecordsDisposalJob(string jobRunByUser, JobRunBy jobRunBy, string param)
        {
            logger.Debug("start physical records disposal");
            string jobId = string.Empty;
            try
            {
                var args = param.Split(' ');
                string locationId = args[0];
                string topLocationId = null;
                if(int.TryParse(locationId, out var intLocationId))
                {
                    var uniqueLocationId = _locationDao.GetLocationUniqueIdById(intLocationId);
                    topLocationId = _locationDao.LoadTopLocationIdBySubLocation(uniqueLocationId).ToString();
                }
                bool skipRemoveContentAndDestroyAction = bool.Parse(args[2]);
                jobId = RMJobService.CreateJobWithScopeId(AvePoint.RA.Contract.JobMonitor.JobType.PhysicalRecordsDisposal, GetJobRunByUser(jobRunBy, null), locationId, topLocationId);
                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var loginName = TenantLocalValue.LogonUserEmail;
                List<JobType> indexJobTypes = JobTypeConstants.JobLevelConflictJobTypes;
                var mIndexJobs = RMJobService.GetRunningJobs(indexJobTypes);

                if (mIndexJobs.Count > 0)
                {
                    logger.Warn("Current has move index job running.");
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                    return jobId;
                }
                mJobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    RunBy = jobRunBy,
                    JobType = AvePoint.RA.Contract.JobMonitor.JobType.PhysicalRecordsDisposal,
                    CommandLine = string.Format("{0} {1} {2} {3}", JobType.PhysicalRecordsDisposal, jobId, locationId, skipRemoveContentAndDestroyAction)
                });
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while start physical records disposal,ERROR:{0}", ex.ToString());
            }

            return jobId;
        }
        [Audit(Module = AuditModule.ControlPanel, Category = AuditCategory.TimerJobSettings, Action = AuditAction.ApprovalProcessConfig, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public string RealRunPhysicalRecordsForApprovalDisposalJob(string jobRunByUser, JobRunBy jobRunBy)
        {
            logger.Debug("start physical records approval disposal");
            string jobId = string.Empty;
            try
            {
                List<JobType> indexJobTypes = JobTypeConstants.JobLevelConflictJobTypes;
                var mIndexJobs = RMJobService.GetRunningJobs(indexJobTypes);

                if (mIndexJobs.Count > 0)
                {
                    //has move index job, need skip.
                    logger.Warn("Current has move index job running.");
                    RMJobService.CreateJobWithScopeId(AvePoint.RA.Contract.JobMonitor.JobType.PhysicalRecordsDisposal, GetJobRunByUser(jobRunBy, null), null);
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                    return jobId;
                }
                var locationIdList = explorerDao.QueryAll(r=> r.ManualApprovedStatus == (int)Contract.SOApproveDBStatus.Approved && r.ManualArchiveStatus == (int)ActionStatus.None && r.LocationId!=Guid.Empty && r.RecordStatus != (int)RMRecordStatus.RMDeleted).ToList()?.Select(a=>a.LocationId).Distinct();
                foreach (var locationId in locationIdList)
                {
                    var dbLocation = _locationDao.GetLocationByUniqueId(locationId);
                    logger.Info($"start physical records approval disposal location id:{locationId.ToString()},int id:{dbLocation.Id}");
                    bool skipRemoveContentAndDestroyAction = false;
                    string tempJobId = InternalRunPhysicalRecordsForApprovalDisposalJob(jobRunBy, dbLocation.Id);
                    jobId = string.IsNullOrEmpty(jobId)? tempJobId:jobId + ";"+ tempJobId;
                    var groupId = TenantLocalValue.LogonGroupId;
                    //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                    var loginName = TenantLocalValue.LogonUserEmail;

                    mJobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = jobId,
                        RunBy = jobRunBy,
                        JobType = AvePoint.RA.Contract.JobMonitor.JobType.PhysicalRecordsDisposal,
                        CommandLine = string.Format("{0} {1} {2} {3} {4}", JobType.PhysicalRecordsDisposal, jobId, dbLocation.Id.ToString(), skipRemoveContentAndDestroyAction,true)
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while start physical records disposal,ERROR:{0}", ex.ToString());
            }

            return jobId;
        }
        public string InternalRunPhysicalRecordsForApprovalDisposalJob(JobRunBy jobRunBy,int locationId)
        {
            var result = RMJobService.CreateJobWithScopeId(AvePoint.RA.Contract.JobMonitor.JobType.PhysicalRecordsDisposal, GetJobRunByUser(jobRunBy, null), locationId.ToString());
            return result;
        }
        [Audit(Module = AuditModule.RetentionAndDisposalManagement, Category = AuditCategory.DisposalActivityManagement, Action = AuditAction.RunPRDisposalJob,
           AfterHandler = typeof(DisposalActivityManagementAfterAuditHandler))]
        public RAReturnMessage RunPhysicalDisposalJob(int locationId, JobRunBy jobRunBy)
        {
            logger.Debug("start physical disposal");
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                string jobId = string.Empty;
                jobId = RMJobService.CreateJob(AvePoint.RA.Contract.JobMonitor.JobType.PhysicalDisposal, GetJobRunByUser(jobRunBy, null));
                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                var loginName = TenantLocalValue.LogonUserEmail;
                mJobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = jobId,
                    RunBy = jobRunBy,
                    JobType = AvePoint.RA.Contract.JobMonitor.JobType.PhysicalDisposal,
                    CommandLine = string.Format("{0} {1} {2}", AvePoint.RA.Contract.JobMonitor.JobType.PhysicalDisposal, jobId, locationId),
                });
                if (string.IsNullOrEmpty(jobId))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
                msg.Extension = jobId;
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while start physical disposal,ERROR:{0}", ex.ToString());
            }

            return msg;
        }
        private string GetJobRunByUser(JobRunBy jobRunBy, string jobRunByUser)
        {
            if (jobRunBy == JobRunBy.Control)
            {
                jobRunByUser = string.IsNullOrEmpty(jobRunByUser) ? TenantLocalValue.LogonUserEmail : jobRunByUser;
            }
            else
            {
                jobRunByUser = "RM_TS_RunSchedule";
            }

            return jobRunByUser;
        }

        public void RunPhysicalTimerJob(JobRunBy jobRunBy)
        {
            throw new NotImplementedException();
        }

        public async Task<RAReturnMessage> SyncADUsersAsync(List<ToUserInfo> users)
        {
            var returnMessage = new RAReturnMessage();
            try
            {
                if (users != null && users.Count > 0)
                {
                    await UserService.SyncUsersAsync(TenantLocalValue.LogonGroupId, users);
                }
            }
            catch (Exception ex)
            {
                returnMessage.ErrorMessage = I18NEntity.GetString("RM_RegisterUser_Error_Message");
                returnMessage.MessageType = RAMessageType.Failed;
            }
            return returnMessage;
        }
    }
}
