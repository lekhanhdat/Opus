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
using Aspose.Pdf.Operators;
using AvePoint.GCommon.Utility.TransientFault;
using AvePoint.Hybrid.Contract;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.Contract.Multi_Geo;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.RACommonUtility.JobControl.JPMC;
using AvePoint.RA.Service.JobMonitor;
using CommonModel.DataModel;
using HybirdProxy.Implement;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JobType = AvePoint.RA.Contract.JobMonitor.JobType;

namespace AvePoint.RA.Service.Services.SignalR
{
    public class HybridFileSystemWorkerService : RMServiceBase, IHybridFileSystemWorkerService
    {
        RALogger logger = new RALogger(MethodBase.GetCurrentMethod().DeclaringType);
        private ISignalRService signalRService => PlatformWindsorManager.GetService<ISignalRService>();
        private IRMSubJobDao RMSubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IJobInfoUpdater JobInfoUpdater => PlatformWindsorManager.GetService<IJobInfoUpdater>();
        private IMultiGeoSettingService MultiGeoSettingService => PlatformWindsorManager.GetService<IMultiGeoSettingService>();

        private IFSConnectionGroupDao FSConnectionGroupDao => PlatformWindsorManager.GetService<IFSConnectionGroupDao>();

        private IFSConnectionGroupWithAgentMemebershipDao FSConnectionGroupWithAgentMemebershipDao => PlatformWindsorManager.GetService<IFSConnectionGroupWithAgentMemebershipDao>();
        //private IDynamicJobController DynamicJobController => PlatformWindsorManager.GetService<IDynamicJobController>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private static AveRetryPolicy retryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(12, TimeSpan.FromSeconds(10)));
        private readonly object mStartJobLocker = new object();
        
        public void StartJob(RecordsJobArgs fSWorkerArgs)
        {
            string tenantId = TenantLocalValue.LogonGroupId;
            var email = TenantLocalValue.LogonUserEmail;
            AgentProxy proxy = retryPolicy.ExecuteAction(() => RASignalRAgentProxy.GetProxy());
            System.Threading.Tasks.Task t = System.Threading.Tasks.Task.Factory.StartNew(() => RealStartJobAsync(proxy, fSWorkerArgs, tenantId, email, Guid.Empty));
        }

        public void StartJobWithConnectionGroupId(RecordsJobArgs fSWorkerArgs, Guid connectionGroupId)
        {
            var connType = FSConnectionGroupDao.GetGroupById(connectionGroupId)?.AccessConnectionType;
            logger.Info($"The group: [{connectionGroupId}] specify agent access connection type: [{connType}].");
            if(connType == Contract.FileSystemRegister.AccessConnectionType.All)
            {
                StartJob(fSWorkerArgs);
                return;
            }
            string tenantId = TenantLocalValue.LogonGroupId;
            var email = TenantLocalValue.LogonUserEmail;
            
            AgentProxy proxy = retryPolicy.ExecuteAction(() => RASignalRAgentProxy.GetProxy());
            System.Threading.Tasks.Task t = System.Threading.Tasks.Task.Factory.StartNew(() => RealStartJobAsync(proxy, fSWorkerArgs, tenantId, email, connectionGroupId));
        }

        public async System.Threading.Tasks.Task StartJobWithConnectionGroupIdDirectlyAsync(RecordsJobArgs fSWorkerArgs, Guid connectionGroupId)
        {
            var connType = FSConnectionGroupDao.GetGroupById(connectionGroupId)?.AccessConnectionType;
            logger.Info($"The group: [{connectionGroupId}] specify agent access connection type: [{connType}].");
            if (connType == Contract.FileSystemRegister.AccessConnectionType.All)
            {
                StartJob(fSWorkerArgs);
                return;
            }
            string tenantId = TenantLocalValue.LogonGroupId;
            var email = TenantLocalValue.LogonUserEmail;
            
            AgentProxy proxy = retryPolicy.ExecuteAction(() => RASignalRAgentProxy.GetProxy());
            await RealStartJobAsync(proxy, fSWorkerArgs, tenantId, email, connectionGroupId);
        }
        

        private async Task<FileSystemJobResult> RealStartJobAsync(AgentProxy proxy, RecordsJobArgs message, string tenantId, string email, Guid connectionGroupId)
        {
            FileSystemJobResult result = new FileSystemJobResult();
            try
            {
                TenantLocalValue.LogonGroupId = tenantId;
                TenantLocalValue.LogonUserEmail = email;
                lock (mStartJobLocker)
                {
                    result = StartJobWithRetryAsync(proxy, tenantId, message, GetRetryCount(), GetRetryInterval(), connectionGroupId).Result;
                }
            }
            catch (Exception e)
            {
                UpdateFailedJob(message, I18NEntity.GetString("RM_SS_FSFailedToStartJob"));
                logger.Error("An error occurred while starting job. JobId:{0} Error:{1}", message.JobId, e.ToString());
            }
            return result;
        }

        private void UpdateFailedJob(RecordsJobArgs message, string comment)
        {
            try
            {
                JobInfoUpdater.UpdateJobState(message.JobId, (int)JobStatus.Failed, comment);
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while updating job to failed. Job id:{0} Error:{1}", message?.JobId, e.ToString());
            }
        }

        private async Task<FileSystemJobResult> StartJobWithRetryAsync(AgentProxy proxy, string tenantId, RecordsJobArgs message, int retryCount, int retryIntervalSecond, Guid connectionGroupId)
        {
            FileSystemJobResult result = new FileSystemJobResult() { Result = FileSystemResultEnum.Failed };
            int mRetryCount = 0;
            string agentId = string.Empty;
            List<string> failedAgentIds = new List<string>();
            if (string.IsNullOrEmpty(message.JobId))
            {
                logger.Error("job id is null is failed.");
                return result;
            }
            ConcurrencyBudgetUtil concurrencyBudgetUtil = new ConcurrencyBudgetUtil();
            var checkRunableAgentJob = await concurrencyBudgetUtil.CheckRunableAgentJob(TenantLocalValue.LogonGroupId);
            if(checkRunableAgentJob == false)
            {
                logger.Warn("start updating job state to waiting because not enough resource.");
                RMSubJobDao.UpdateRunable(message.JobId);
                return result;
            }
            while (true)
            {
                try
                {
                    ICollection<AgentInformation> agents;
                    if (connectionGroupId == Guid.Empty)
                    {
                        agents = await signalRService.GetAgentsByTypeAsync(tenantId, AvePoint.Hybrid.Contract.Object.SourceType.FileSystem);
                        if (await MultiGeoSettingService.IsEnableMultiGeoFeature())
                        {
                            agents = MultiGeoSettingService.GetAvailableAgentForMultiGeoRedirect(agents);
                        }
                    }
                    else
                    {
                        agents = await signalRService.GetAgentsByTypeAndConnectionGroupIdAsync(tenantId, AvePoint.Hybrid.Contract.Object.SourceType.FileSystem, connectionGroupId);
                    }             
                    if (agents.Count == 0)
                    {
                        if (mRetryCount >= retryCount)
                        {
                            UpdateFailedJob(message, I18NEntity.GetString("RM_SS_FSNoAvailableAgent"));
                            logger.Error("Cannot find available agent, job is failed. Job Id:{0}", message?.JobId);
                            return result;
                        }
                        else
                        {
                            logger.Warn("Cannot get available agent. Retry count:{0}", mRetryCount);
                            await System.Threading.Tasks.Task.Delay(retryIntervalSecond * 1000);
                        }
                        mRetryCount++;
                        continue;
                    }
                    if (string.IsNullOrEmpty(message.AgentId) || connectionGroupId != Guid.Empty)
                    {
                        var avaliableAgentIds = agents.Select(a => a.AgentId).ToList();
                        logger.Info("Available agent count : " + agents.Count);
                        var unusedAgentIds = avaliableAgentIds.Where(a => !failedAgentIds.Contains(a)).ToList();
                        if (unusedAgentIds.Count == 0)
                        {
                            failedAgentIds.Clear();
                        }
                        var agentJobCountGroup = RMSubJobDao.GetAgentJobCount(GetJobTypes(message.JobType));
                        List<string> agentIds = unusedAgentIds.Count > 0 ? unusedAgentIds : avaliableAgentIds;
                        agentJobCountGroup = FilterAgentsForGroup(agentJobCountGroup, agentIds);
                        agentId = GetAgentId(agentIds, agentJobCountGroup);
                    }
                    else
                    {
                        logger.Info("Use agent id from message. Agent id:{0}", message.AgentId);
                        agentId = message.AgentId;
                    }
                    AgentInformation agent = agents.Where(a => a.AgentId == agentId).FirstOrDefault();
                    result = await proxy.InvokeOneAgentAysnc<SFileSystemJobExecute, RecordsJobArgs, FileSystemJobResult>(agent, new SFileSystemJobExecute() { MethodArgs = new RecordsJobArgs() { TenantId = message.TenantId, JobId = message.JobId, JobType = message.JobType, AgentId = agentId, TenantRegisterEmail = TenantLocalValue.LogonUserEmail, Extensions = message.Extensions } });
                    logger.Debug("Send job to agent successfully. Agent id:{0} Job id:{1} Job Type:{2} Tenant id:{3}", agent?.AgentId, message?.JobId, message?.JobType, agent?.TenantId);
                }
                catch (Exception e)
                {
                    logger.Warn("An error occurred while starting job. JobId:{0} Error:{1}", message?.JobId, e.ToString());
                }
                if ((result != null && result.Result == FileSystemResultEnum.Succeed) || mRetryCount >= retryCount)
                {
                    break;
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(agentId) && !failedAgentIds.Contains(agentId))
                    {
                        failedAgentIds.Add(agentId);
                    }
                    await System.Threading.Tasks.Task.Delay(retryIntervalSecond * 1000);
                    logger.Warn("Start job failed. Used agent:{0} Retry count:{1} Error:{2}", agentId, retryCount, result?.Message);
                }
                mRetryCount++;
            }

            if (result != null && result.Result == FileSystemResultEnum.Succeed)
            {
                await RMSubJobDao.UpdateAgentIdAsync(message?.JobId, agentId);
                logger.Info("Finish to send message to agent.");
            }
            else
            {
                UpdateFailedJob(message, I18NEntity.GetString("RM_SS_FSFailedSendJobToAgent"));
                logger.Warn("Failed to send job to agent, job id:{0}", message?.JobId);
            }
            return result;
        }

        private Dictionary<string, int> FilterAgentsForGroup(Dictionary<string, int> allGroup, List<string> agentIds)
        {
            Dictionary<string, int> keyValuePairs = new Dictionary<string, int>();
            foreach (var g in allGroup)
            {
                if (agentIds.Contains(g.Key))
                {
                    keyValuePairs.Add(g.Key, g.Value);
                }
            }
            return keyValuePairs;
        }

        private string GetAgentId(List<string> avaliableAgentIds, Dictionary<string, int> agentJobCountGroup)
        {
            logger.Debug("Available agent ids:{0}", string.Join(",", avaliableAgentIds));
            foreach (var a in agentJobCountGroup)
            {
                logger.Debug("Agent id:{0} job count:{1}", a.Key, a.Value);
            }
            string id = string.Empty;
            if (agentJobCountGroup.Count > 0)
            {
                var runningAgentIds = agentJobCountGroup.Keys;
                id = avaliableAgentIds.Where(s => !runningAgentIds.Contains(s))?.FirstOrDefault()?.ToString();
                if (string.IsNullOrWhiteSpace(id))
                {
                    var orderedAgentJobGroup = agentJobCountGroup.OrderBy(s => s.Value);
                    foreach (var tempAgent in orderedAgentJobGroup)
                    {
                        if (avaliableAgentIds.Contains(tempAgent.Key))
                        {
                            id = tempAgent.Key;
                            break;
                        }
                    }
                }
            }
            else
            {
                if (avaliableAgentIds.Count > 0)
                {
                    Random r = new Random();
                    /* Fortify Issue Type: Insecure Randomness 
                    * Sink Details: this class StartJobWithRetryAsync 
                    * Ignore Reason: random用于 从传入列表中随机选一个值，传入列表值不是固定的，所以是安全的 
                    */
                    var number = r.Next(avaliableAgentIds.Count);
                    id = avaliableAgentIds[number];
                }
            }
            return id;
        }

        public async Task<int> GetAgentCountAsync()
        {
            try
            {
                return (await signalRService.GetAgentsByTypeAsync(TenantLocalValue.LogonGroupId, Hybrid.Contract.Object.SourceType.FileSystem)).Count();
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting agent count. Error:{0}", e.ToString());
                return 0;
            }
        }

        public async Task<int> GetAgentCountByGroupsAsync(IEnumerable<Guid> groupIds)
        {
            var agentInfos = await signalRService.GetAgentsByTypeAsync(TenantLocalValue.LogonGroupId, Hybrid.Contract.Object.SourceType.FileSystem);
            var agentIds = agentInfos.Select(item => new Guid(item.AgentId)).ToHashSet();
            var hasAccessTypeAll = groupIds.Any(item =>
            {
                return FSConnectionGroupDao.GetGroupById(item).AccessConnectionType == Contract.FileSystemRegister.AccessConnectionType.All;
            });
            if(hasAccessTypeAll)
            {
                if (await MultiGeoSettingService.IsEnableMultiGeoFeature())
                {
                    var multigeoAgents = MultiGeoSettingService.GetAvailableAgentForMultiGeoRedirect(agentInfos);
                    agentIds = multigeoAgents.Select(item => new Guid(item.AgentId)).ToHashSet();
                }
                return agentIds.Count;
            }

            var underGroupAgentIds = (await FSConnectionGroupWithAgentMemebershipDao.FindListAsync(item => groupIds.Contains(item.ConnectionGroupId))
                ).Select(item => item.AgentId).ToHashSet();

            var availableAgentIds = underGroupAgentIds.Intersect(agentIds);

            return availableAgentIds.Count();
        }

        public async Task<List<string>> GetAvailableAgentIdsByGroupsAsync(IEnumerable<Guid> groupIds)
        {
            var agentInfos = await signalRService.GetAgentsByTypeAsync(TenantLocalValue.LogonGroupId, Hybrid.Contract.Object.SourceType.FileSystem);
            var agentIds = agentInfos.Select(a => new Guid(a.AgentId)).ToHashSet();

            bool hasAccessTypeAll = groupIds.Any(item => FSConnectionGroupDao.GetGroupById(item).AccessConnectionType == Contract.FileSystemRegister.AccessConnectionType.All);

            if (hasAccessTypeAll)
            {
                if (await MultiGeoSettingService.IsEnableMultiGeoFeature())
                    agentIds = MultiGeoSettingService.GetAvailableAgentForMultiGeoRedirect(agentInfos).Select(a => new Guid(a.AgentId)).ToHashSet();
                return agentIds.Select(g => g.ToString()).ToList();
            }

            var underGroupAgentIds = (await FSConnectionGroupWithAgentMemebershipDao
                .FindListAsync(m => groupIds.Contains(m.ConnectionGroupId)))
                .Select(m => m.AgentId).ToHashSet();

            return underGroupAgentIds.Intersect(agentIds).Select(g => g.ToString()).ToList();
        }

        public async System.Threading.Tasks.Task StopAgentJobAsync(string jobId, string tenantId, string agentId)
        {
            try
            {
                AgentProxy proxy = retryPolicy.ExecuteAction(() => RASignalRAgentProxy.GetProxy());
                await proxy.SendToAgentAsync(tenantId, agentId, new SRecordsJobStop
                {
                    MethodArgs = new RecordsJobStopArgs
                    {
                        JobId = jobId,
                        TenantId = tenantId
                    }
                });
                logger.Info("Sent stop signal to agent. AgentId:{0} JobId:{1} TenantId:{2}", agentId, jobId, tenantId);
            }
            catch (Exception e)
            {
                logger.Warn("Failed to send stop signal to agent. AgentId:{0} JobId:{1} Error:{2}", agentId, jobId, e.ToString());
            }
        }

        private List<AvePoint.RA.Contract.JobMonitor.JobType> GetJobTypes(AvePoint.Hybrid.Contract.JobType type)
        {
            if (type == Hybrid.Contract.JobType.FSDataSync)
            {
                return new List<Contract.JobMonitor.JobType>() { Contract.JobMonitor.JobType.FSDataSynchronization, Contract.JobMonitor.JobType.FSDataSynchronizationSchedule };
            }
            else if (type == Hybrid.Contract.JobType.FSDisposal)
            {
                return new List<Contract.JobMonitor.JobType>() { Contract.JobMonitor.JobType.FSDisposal, Contract.JobMonitor.JobType.FSDisposalSchedule };
            }
            else if (type == Hybrid.Contract.JobType.FSDisposalByClassCode)
            {
                return new List<Contract.JobMonitor.JobType>() { Contract.JobMonitor.JobType.FSDisposalByClassCode };
            }
            else if (type == Hybrid.Contract.JobType.FSContentDueReport)
            {
                return new List<Contract.JobMonitor.JobType>() { Contract.JobMonitor.JobType.FSItemsFilesDueDisposal };
            }
            else if (type == Hybrid.Contract.JobType.FSCreationAndDestructionReport)
            {
                return new List<Contract.JobMonitor.JobType>() { Contract.JobMonitor.JobType.FSCreateAndDestroyedFileReport };
            }
            else if (type == Hybrid.Contract.JobType.SharePointOnPremApplySetting)
            {
                return new List<Contract.JobMonitor.JobType>() { Contract.JobMonitor.JobType.SPOnPremApplySetting, Contract.JobMonitor.JobType.SPOnPremApplySettingSchedule };
            }
            else if (type == Hybrid.Contract.JobType.SPOnPremTermSynchronization)
            {
                return new List<Contract.JobMonitor.JobType>() { Contract.JobMonitor.JobType.SPOnPremTermSynchronization, Contract.JobMonitor.JobType.SPOnPremTermSynchronizationSchedule };
            }
            else if (type == Hybrid.Contract.JobType.SharePointOnPremEnforceRuleAction)
            {
                return new List<Contract.JobMonitor.JobType>() { Contract.JobMonitor.JobType.SPOnPremEnforceRuleAction, Contract.JobMonitor.JobType.SPOnPremEnforceRuleActionSchedule };
            }
            else if (type == Hybrid.Contract.JobType.ImportFSSetting)
            {
                return new List<Contract.JobMonitor.JobType>() { Contract.JobMonitor.JobType.ImportFSSetting };
            }
            else if (type == Hybrid.Contract.JobType.FSArchiverRestore)
            {
                return new List<Contract.JobMonitor.JobType>() { Contract.JobMonitor.JobType.FSArchiverRestore };
            }
            else if (type == Hybrid.Contract.JobType.FSRetain)
            {
                return new List<Contract.JobMonitor.JobType>() { Contract.JobMonitor.JobType.FSRetain };
            }
            else if (type == Hybrid.Contract.JobType.FSRetainSimulate)
            {
                return new List<Contract.JobMonitor.JobType>() { Contract.JobMonitor.JobType.FSRetainSimulate };
            }
            else if (type == Hybrid.Contract.JobType.FSDiscovery)
            {
                return new List<Contract.JobMonitor.JobType> { Contract.JobMonitor.JobType.DiscoveryFileSystemV1 };
            }
            else
            {
                throw new Exception("invalid job type. Type:" + type.ToString());
            }
        }

        private int GetRetryCount()
        {
            int retryCount = 3;
            try 
            {
                retryCount =int.Parse( RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.RETRY_COUNT_FOR_GET_AGENT]);
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while getting fs retry count. Error:{0}", e.ToString());
            }
            return retryCount;
        }

        private int GetRetryInterval()
        {
            int retryCount = 30;
            try
            {
                retryCount = int.Parse(RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.RETRY_INTERVAL_SECONDS_FOR_GET_AGENT]);
            }
            catch (Exception e)
            {
                logger.Warn("An error occurred while getting fs retry interval. Error:{0}", e.ToString());
            }
            return retryCount;
        }
    }
}
