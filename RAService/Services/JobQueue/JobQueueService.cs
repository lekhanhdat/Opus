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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.AzureService;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Discovery;
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core.Discovery.DBManager;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Discovery.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Dao.Discovery.Impl.Google;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.Service.Services.Discovery;
using AvePoint.RA.Service.Services.Discovery.AOSP;
using AvePoint.RA.Service.Services.Discovery.Office365;
using AvePoint.RA.Service.Services.JobMonitor.AuditHandler;
using AvePoint.RA.SharePoint.Archiver.Scan.Base;
using Microsoft.Azure.Amqp.Framing;
using Newtonsoft.Json;
using NVelocity.Tool;
using RecordsHotfixMaintenanceService;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.JobQueue
{
    [Audit]
    public class JobQueueService : RMServiceBase, IJobQueueService
    {
        private RALogger logger = RALogger.GetInstance(typeof(JobQueueService));
        private IGeneralSettingService mGeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IRMJobQueueDao mRMJobQueueDao => PlatformWindsorManager.GetService<IRMJobQueueDao>();
        private IRMCache mRMCache => PlatformWindsorManager.GetService<IRMCache>();

        private readonly IRMDiscoveryOffice365ProgressService _RMDiscoveryProgressService = new RMDiscoveryOffice365ProgressService();

        private readonly IRMDiscoveryAOSPProgressService _RMDiscoveryAOSPProgressService = new RMDiscoveryAOSPProgressService();

        private readonly IRMDiscoveryOffice365ProfileDao _profileDao = new RMDiscoveryOffice365ProfileDao();
        
        private readonly IRMDiscoveryGoogleProfileDao _ggProfileDao = new RMDiscoveryGoogleProfileDao();
        #region public function

        public bool CheckEndUserArchvierJobInJobQueue(string jobId)
        {
            try
            {
                logger.Info($"check end user archive job in job queue, job id:{jobId}");
                var cacheKey = $"CheckEndUserArchvierJobInJobQueue_{jobId}";
                return mRMCache.TryGetAsync(cacheKey, () =>
                {
                    Expression<Func<RMJobQueue, bool>> whereLambda =
                    message =>
                    message.JobType == (int)JobType.RMEndUserArchiverBackup
                    && message.Parameters.Contains(jobId);
                    RMJobQueue queueMessage = mRMJobQueueDao.GetQueues(1, 1, out int totalRecord, "CreateTime", true, whereLambda).FirstOrDefault();
                    if (totalRecord == 0)
                    {
                        return Task.FromResult(false);
                    }
                    var endUserTreeNodeInfo = SerializerHelper.DeserializeByDataContractSerializer<EndUserArchiveContainerConfig>(queueMessage.Parameters);
                    if (endUserTreeNodeInfo != null && endUserTreeNodeInfo.JobId.Equals(jobId, StringComparison.OrdinalIgnoreCase))
                    {
                        return Task.FromResult(true);
                    }
                    else
                    {
                        return Task.FromResult(false);
                    }
                }, TimeSpan.FromSeconds(20)).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while check end user archiver job in jobqueue, jobId:{0}, ERROR:{1}", jobId, ex.ToString());
                return false;
            }
        }

        public string AddToDBJobQueue(JobQueueDto jobInfo)
        {
            try
            {
                var job = this.ConvertToJobQueueModel(jobInfo);

                return mRMJobQueueDao.AddToJobQueue(job);

            }
            catch (DbEntityValidationException dbex)
            {
                logger.Error("error occurred while add to db jobqueue,commandLine:{0}, ERROR:{1}", jobInfo.Parameters, dbex);
                foreach (var entityValidationError in dbex.EntityValidationErrors)
                {
                    logger.Error($"Entity of type '{entityValidationError.Entry.Entity.GetType().Name}' has the following validation errors:");
                    foreach (var validationError in entityValidationError.ValidationErrors)
                    {
                        logger.Error($"- Property: '{validationError.PropertyName}', Error: '{validationError.ErrorMessage}'");
                    }
                }
                throw;
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while add to db jobqueue,commandLine:{0}, ERROR:{1}", jobInfo.Parameters, ex.ToString());
                return string.Empty;
            }
        }

        public void DeleteDBJobQueueMessage(string messageId, string tenantId)
        {
            try
            {
                mRMJobQueueDao.DeleteQueueMessage(messageId, tenantId);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while delete db jobqueue,id:{0}, ERROR:{1}", messageId, ex.ToString());
            }
        }

        public List<JobQueueDto> GetDBJobQueueMessage(string tenantId, string useEmail, JobType jobType)
        {
            List<JobQueueDto> jobResult = new List<JobQueueDto>();
            try
            {
                var jobs = mRMJobQueueDao.GetDBJobQueueMessage(tenantId, useEmail, jobType);
                if (jobs != null && jobs.Count() > 0)
                {
                    jobResult = jobs.ConvertAll<JobQueueDto>(o => ConvertToJobQueueDto(o));
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while get all db jobqueue, ERROR:{0}", ex.ToString());
                throw;
            }
            return jobResult;
        }


        public List<JobQueueDto> GetDBJobMessage()
        {
            List<JobQueueDto> jobResult = new List<JobQueueDto>();
            try
            {
                var versionnumber = RMGlobalConfiguration.EnvSetting.ProductVersion;
                var jobs = mRMJobQueueDao.GetQueueMessage(versionnumber);
                if (jobs != null && jobs.Count() > 0)
                {
                    jobResult = jobs.ConvertAll<JobQueueDto>(o => ConvertToJobQueueDto(o));
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while get all db jobqueue, ERROR:{0}", ex.ToString());
                throw;
            }
            return jobResult;
        }
        public List<JobQueueDto> GetDBJobMessageGroupByTenant(int top)
        {
            List<JobQueueDto> jobResult = new List<JobQueueDto>();
            try
            {
                var jobs = mRMJobQueueDao.GetDBJobMessageGroupByTenant(top);
                if (jobs != null && jobs.Count() > 0)
                {
                    foreach (var item in jobs)
                    {
                        var value = item.Value.ConvertAll<JobQueueDto>(o => ConvertToJobQueueDto(o));
                        jobResult.AddRange(value);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while get top db job queue, ERROR:{0}", ex.ToString());
                throw;
            }
            return jobResult.OrderByDescending(j => j.JobPriority).ThenBy(j => j.CreatedTime).ToList();
        }

        public int GetMessagesCount(string tenantId, JobType jobType)
        {
            return mRMJobQueueDao.GetMessagesCount(tenantId, jobType);
        }

        public async Task<string> GetDBJobQueueDataAsync(JMPager pager)
        {
            int totalCount;
            var dbResult = mRMJobQueueDao.GetQueues(pager.JumpPage, pager.PageSize, out totalCount, pager.SortBy, (pager.IsSort && !pager.IsDesc));
            List<JMJobQueueInfo> resultList = new List<JMJobQueueInfo>();
            if (!dbResult.IsNullOrEmpty())
            {
                GeneralSettingModel gls = await mGeneralSettingService.GetGeneralSettingAsync();
                foreach (var r in dbResult)
                {
                    resultList.Add(new JMJobQueueInfo()
                    {
                        MessageId = r.MessageId,
                        JobType = I18NEntity.GetString("RM_JS_JM_JobType_" + ((JobType)r.JobType).ToString()),
                        CreatedBy = r.JobRunType == (int)JobRunBy.Schedule || r.JobRunType == (int)JobRunBy.ChangeTab || r.JobRunBy == "RM_TS_RunSchedule" ? I18NEntity.GetString("RM_TS_RunSchedule") : r.JobRunBy,
                        CreatedTime = r.CreateTime == 0 ? "" : mGeneralSettingService.ConvertTiksToDateTime(gls, r.CreateTime, true).SimplifyFormatTime,
                        JobPriority = r.JobPriority
                    });
                }
            }
            JQPageResult responseResult = new JQPageResult();
            responseResult.TotalNumber = totalCount;
            responseResult.Result = resultList;
            return JsonConvert.SerializeObject(responseResult);
        }

        public async Task<string> GetRCCDBJobQueueByLoginNameAsync(string loginName, List<string> scopeIds)
        {
            if (string.IsNullOrEmpty(loginName)) return null;

            var dbResult = await Task.Run(() => mRMJobQueueDao.GetRCCDBJobQueueByLoginName(loginName, scopeIds));

            return JsonConvert.SerializeObject(dbResult);
        }

        public async Task<string> GetDisposalHistoryDBJobQueueByLoginNameAsync(string loginName, string scopeId)
        {
            if (string.IsNullOrEmpty(loginName)) return null;

            var dbResult = await Task.Run(() => mRMJobQueueDao.GetDisposalHistoryDBJobQueueByLoginName(loginName, scopeId));

            return JsonConvert.SerializeObject(dbResult);
        }

        public async Task<string> GetAllDBJobQueueByLoginNameAsync(string loginName, int jobType)
        {
            if (string.IsNullOrEmpty(loginName)) return null;
            var dbResult = await Task.Run(() => mRMJobQueueDao.GetAllDBJobQueueByLoginName(loginName, jobType));
            return JsonConvert.SerializeObject(dbResult);
        }

        [Audit(Module = AuditModule.JobMonitor, Category = AuditCategory.JobMonitor, Action = AuditAction.DeleteQueues, BeforeHandler = typeof(JobMonitorServiceBeforeAuditHandler),AfterHandler = typeof(JobMonitorServiceAuditHandler))]
        public async Task DeleteDBJobQueue(string messageId, string tenantId)
        {
            try
            {
                var tempJob = mRMJobQueueDao.GetQueue(messageId, tenantId);
                mRMJobQueueDao.DeleteQueueMessage(messageId, tenantId);
                if (tempJob != null)
                {
                    switch (tempJob.JobType)
                    {
                        case (int)JobType.DiscoverOptimization:
                            var jobParaInfo = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoverOptimizationJobInfo>(tempJob.Parameters);
                            _ = _RMDiscoveryProgressService.GetCancelJobAsync(jobParaInfo.o365Info.UniqueId, jobParaInfo.settingInfo.SettingId);
                            break;
                        case (int)JobType.DiscoveryPlanProOptimization:
                            var planProJobParaInfo = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoverOptimizationJobInfo>(tempJob.Parameters);
                            _ = _RMDiscoveryProgressService.GetCancelJobAsync(planProJobParaInfo.o365Info.UniqueId, planProJobParaInfo.settingInfo.SettingId);
                            break;
                        case (int)JobType.DiscoveryAOSPOptimization:
                            var jobAOSPParaInfo = SerializerHelper.DeserializeByDataContractSerializer<RMDiscoverAOSPOptimizationJobInfo>(tempJob.Parameters);
                            _ = _RMDiscoveryAOSPProgressService.GetCancelJobAsync(jobAOSPParaInfo.o365Info.UniqueId, jobAOSPParaInfo.settingInfo.SettingId);
                            break;
                        case (int)JobType.DiscoveryProfileJob:
                            var jobProFileInfo = JsonConvert.DeserializeObject<RMDiscoveryProfileJobDefinition>(tempJob.Parameters);
                             await _profileDao.DeleteProfileFailedInfoesAsync(jobProFileInfo.O365TenantId, jobProFileInfo.SpecifyProfileId);
                             await _profileDao.DeleteProfileInfoAsync(jobProFileInfo.O365TenantId, jobProFileInfo.SpecifyProfileId);
                             await RMDiscoveryDBManager.DropOffice365InactiveProfileTablsAsync(jobProFileInfo.O365TenantId, jobProFileInfo.SpecifyProfileId);
                            break;
                        case (int)JobType.DiscoveryGoogleProfileJob:
                            var jobGGProfileInfo = JsonConvert.DeserializeObject<RMDiscoveryGoogleProfileJobDefinition>(tempJob.Parameters);
                            await _ggProfileDao.DeleteProfileFailedInfoesAsync(jobGGProfileInfo.GoogleOrganizationId, jobGGProfileInfo.SpecifyProfileId);
                            await _ggProfileDao.DeleteProfileInfoAsync(jobGGProfileInfo.GoogleOrganizationId, jobGGProfileInfo.SpecifyProfileId);
                            await RMDiscoveryDBManager.DropGoogleInactiveProfileTablesAsync(jobGGProfileInfo.GoogleOrganizationId, jobGGProfileInfo.SpecifyProfileId);
                            break;
                    }

                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while delete db jobqueue,id:{0}, ERROR:{1}", messageId, ex.ToString());
                throw;
            }
        }

        public async Task<int> DeleteQueueMessageBatchAsync(List<string> idList)
        {
            if (idList.Count == 0)
            {
                return 0;
            }
            return await mRMJobQueueDao.DeleteQueueMessageBatchAsync(idList);
        }

        public void ResetDBJobQueue(string messageId, string tenantId)
        {
            try
            {
                mRMJobQueueDao.ReEnterQueueMessage(messageId, tenantId);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while reset the db jobqueue,id:{0}, ERROR:{1}", messageId, ex.ToString());
                throw;
            }
        }

        public async Task<int> ReEnterQueueMessageBatchAsync(List<string> idList)
        {
            if(idList.Count == 0)
            {
                return 0;
            }
            return await mRMJobQueueDao.ReEnterQueueMessageBatchAsync(idList);
        }

        public bool IsDBExsitJobQueue(string messageId, string tenantId)
        {
            RMJobQueue jq = mRMJobQueueDao.GetQueue(messageId, tenantId);
            return jq != null;
        }

        public JobQueueMessage GetCloudJobMessage()
        {
            string jobQueueName = CommonUtilityForSpecialTenant.GetJobQueueNameFromConfigFile();
            return QueueMessageUtilFactory.GetUtil(QueueMessageType.Job, jobQueueName).ReceiveMessageWithRetry<JobQueueMessage>();
        }

        public bool SendMessageToCloud(JobQueueMessage queueMessage, QueueMessageType messageType)
        {
            try
            {
                QueueMessageUtilFactory.GetUtil(messageType).SendMessage(queueMessage);
                return true;
            }
            catch (Exception ex)
            {
                logger.Error("Send message failed. {0}", ex);
                return false;
            }
        }

        public void HandleMessage(JobQueueMessage msg)
        {
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                ThrowUtil.ThrowIfNull(groupId, string.Format("LogonGroupId is null, args:{0}", msg.CommandLine));

                msg.JobTenantInfo = new JobTenantInfo()
                {
                    TenantId = groupId,
                    RegisterEmail = loginName
                };

                if (RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
                {
                    logger.Debug("dev mode start job, jobId:{0}, type:{1}.", msg.JobId, msg.JobType);
                    WriteJobContext2Local(msg);
                    ProcessStart(msg);
                }
                else
                {
                    logger.Debug("prod mode start job, jobId:{0}, type:{1}.", msg.JobId, msg.JobType);
                    WriteJobContext2Blob(msg);
                    string jobQueueName = CommonUtilityForSpecialTenant.GetJobQueueNameFromConfigFile();
                    SendContainerMessage(msg, QueueMessageType.Job, jobQueueName);
                    logger.Debug("end of sending message to service bus, jobId:{0}, type:{1}.", msg.JobId, msg.JobType);

                }
            }
            catch (Exception ex)
            {
                logger.Error("handle message error:{0}", ex.ToString());
                throw;
            }
        }

        public void HandleO365Message(JobQueueMessage msg)
        {
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                ThrowUtil.ThrowIfNull(groupId, string.Format("LogonGroupId is null, args:{0}", msg.CommandLine));

                msg.JobTenantInfo = new JobTenantInfo()
                {
                    TenantId = groupId,
                    RegisterEmail = loginName
                };

                if (RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
                {
                    logger.Debug("dev mode start job, jobId:{0}, type:{1}.", msg.JobId, msg.JobType);
                    WriteJobContext2Local(msg);
                    ProcessStart(msg);
                }
                else
                {
                    logger.Debug("o365 prod mode start job, jobId:{0}, type:{1}.", msg.JobId, msg.JobType);
                    WriteJobContext2Blob(msg);
                    SendContainerMessage(msg, QueueMessageType.O365Job);
                    logger.Debug("o365 end of sending message to service bus, jobId:{0}, type:{1}.", msg.JobId, msg.JobType);
                }
            }
            catch (Exception ex)
            {
                logger.Error("o365 handle message error:{0}", ex.ToString());
                throw;
            }
        }

        public void HandleCustomerMessage(JobQueueMessage msg, string jobQueueName)
        {
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                ThrowUtil.ThrowIfNull(groupId, string.Format("LogonGroupId is null, args:{0}", msg.CommandLine));

                msg.JobTenantInfo = new JobTenantInfo()
                {
                    TenantId = groupId,
                    RegisterEmail = loginName
                };

                if (RMGlobalConfiguration.EnvSetting.IsDevEnvironment)
                {
                    logger.Debug("dev mode start job, jobId:{0}, type:{1}.", msg.JobId, msg.JobType);
                    WriteJobContext2Local(msg);
                    ProcessStart(msg);
                }
                else
                {
                    logger.Debug("o365 prod mode start job, jobId:{0}, type:{1}.", msg.JobId, msg.JobType);
                    WriteJobContext2Blob(msg);
                    SendContainerMessage(msg, QueueMessageType.CustomerJob, jobQueueName);
                    logger.Debug("o365 end of sending message to service bus, jobId:{0}, type:{1}.", msg.JobId, msg.JobType);
                }
            }
            catch (Exception ex)
            {
                logger.Error("o365 handle message error:{0}", ex.ToString());
                throw;
            }
        }

        [Audit(Module = AuditModule.JobMonitor, Category = AuditCategory.JobMonitor, Action = AuditAction.UpdateJobQueuePriority, BeforeHandler = typeof(JobMonitorServiceBeforeAuditHandler), AfterHandler = typeof(JobMonitorServiceAuditHandler))]
        public bool UpdateJobPriority(string messageId, JobPriority newPriority, string tenantId)
        {
            try
            {
                return mRMJobQueueDao.UpdateJobPriority(messageId, newPriority, tenantId);
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while update job priority, tenantId:{0}, ERROR:{1}", tenantId, ex.ToString());
                return false;
            }
        }

        public List<JobQueueDto> GetTimeoutProcessingMessages(long timeoutPeriod, string anchorMessageId, int top)
        {
            var domainList = mRMJobQueueDao.GetTimeoutProcessingMessages(timeoutPeriod, anchorMessageId, top);
            return domainList.Select(ConvertToJobQueueDto).ToList();
        }

        #endregion

        #region private function

        private void SendContainerMessage(JobQueueMessage msg, QueueMessageType messageType, string jobQueueName = "")
        {
            var containerMsg = new  
            {
                TenantId = msg.JobTenantInfo.TenantId,
                JobId = msg.JobId,
                JobType = msg.JobType.ToString() 
            };
            QueueMessageUtilFactory.GetUtil(messageType, jobQueueName).SendMessage(containerMsg);
        }

        private void WriteJobContext2Local(JobQueueMessage msg)
        {
            var blobName = $"{msg.JobId}.json";
            var context = JsonConvert.SerializeObject(msg);
            var location = Path.Combine(RecordsEnv.LogFolder, msg.JobTenantInfo.TenantId, "JobContext");
            FileSystemUtil.CreateFolder(location);
            FileSystemUtil.CreateFile(AvePoint.GCommon.Utility.SecurityUtils.SafeCombinePath(location, blobName), context, false);
        }

        private void WriteJobContext2Blob(JobQueueMessage msg)
        {
            var blobName = $"{msg.JobTenantInfo.TenantId}/{msg.JobId}.json";
            RAStorageUtil.UploadBlob(RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.RECO_STORAGE_CONNECTION_STRING], RMGlobalConfiguration.StorageConfig[Contract.Configurations.RMStorageSettingKey.JOB_CONTEXT_CONTAINER_NAME], blobName, msg);
        }

        private void ProcessStart(JobQueueMessage msg)
        {
            string installPath = WebUtil.GetInstallPath();
            string filePath = Path.Combine(installPath, "RevIMScheduleJob.exe");
            logger.Info("scheduleJobFilePath:{0}", filePath);
            var index = Math.Max(installPath.IndexOf("RATimerWorkerRole"), installPath.IndexOf("RAWeb"));
            index = Math.Max(index, installPath.IndexOf("RAApi"));
            string devScheduleJobPath = index > 0 ? installPath.Substring(0, index) : string.Empty;
            string path = string.IsNullOrEmpty(devScheduleJobPath) ? filePath : Path.Combine(devScheduleJobPath, "RAScheduleJob/bin/net10.0/RevIMScheduleJob.exe");
            string args = string.Format("{0} {1} {2}",
                    msg.CommandLine,
                    msg.JobTenantInfo?.TenantId,
                    string.IsNullOrEmpty(msg.JobTenantInfo?.RegisterEmail) ? "RM_TS_RunSchedule" : msg.JobTenantInfo?.RegisterEmail);
            if (SecurityUtils.ValidateCommandArgs(args))
            {
                var startInfo = new ProcessStartInfo(
                    path, args
                    )
                {
                    //var debugStartInfo = new ProcessStartInfo(path);
                    CreateNoWindow = true,
                    UseShellExecute = false
                };
                var process = Process.Start(startInfo);
                logger.Info("process has started,tenantId:{1}, commandLine:{0} ", msg.CommandLine, msg.JobTenantInfo?.TenantId);
            }
            else
            {
                logger.Info("process has not started due to invalid command args,tenantId:{1}, args:{0} ", args, msg.JobTenantInfo?.TenantId);
            }
        }

        private JobQueueDto ConvertToJobQueueDto(RMJobQueue rmJob)
        {
            var jobResult = new JobQueueDto()
            {
                MessageId = rmJob.MessageId,
                JobType = (JobType)rmJob.JobType,
                JobRunType = (JobRunBy)rmJob.JobRunType,
                Parameters = rmJob.Parameters,
                TenantGroupId = rmJob.TenantId,
                JobRunByUser = rmJob.JobRunBy,
                PartnerUser = rmJob.PartnerUser,
                CreatedTime = rmJob.CreateTime,
                ClientIP = rmJob.ClientIP,
                ProductType = rmJob.ProductType,
                JobPriority = rmJob.JobPriority,
                UpdateTime = rmJob.UpdateTime,
            };
            return jobResult;
        }

        private RMJobQueue ConvertToJobQueueModel(JobQueueDto rmJob)
        {
            var jobResult = new RMJobQueue()
            {
                MessageId = rmJob.MessageId,
                JobType = (int)rmJob.JobType,
                JobRunType = (int)rmJob.JobRunType,
                Parameters = rmJob.Parameters,
                TenantId = rmJob.TenantGroupId,
                JobRunBy = rmJob.JobRunByUser,
                PartnerUser = TenantLocalValue.PartnerUser,
                ProductVersion = RMGlobalConfiguration.EnvSetting.ProductVersion,
                ProductType = rmJob.ProductType,
                JobPriority = rmJob.JobPriority,
            };
            return jobResult;
        }




        #endregion
    }
}
