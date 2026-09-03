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
using AvePoint.GCommon.Utility.TransientFault;
using AvePoint.Hybrid.Contract;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.RMWeb.SingalR;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using CommonModel.DataModel;
using HybirdProxy.Implement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.SingalR
{
    public class HybridSharePointOnPremWorkerService : RMServiceBase, IHybridSharePointOnPremWorkerService
    {
        RALogger logger = new RALogger(MethodBase.GetCurrentMethod().DeclaringType);
        public ISignalRService signalRService => PlatformWindsorManager.GetService<ISignalRService>();
        private IRMSubJobDao RMSubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IJobInfoUpdater JobInfoUpdater => PlatformWindsorManager.GetService<IJobInfoUpdater>();
        private static AveRetryPolicy retryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(12, TimeSpan.FromSeconds(10)));
        private readonly object mStartJobLocker = new object();

        public void StartSPJob(RecordsJobArgs fSWorkerArgs)
        {
            AgentProxy proxy = retryPolicy.ExecuteAction(() => RASignalRAgentProxy.GetProxy());
            string tenantId = TenantLocalValue.LogonGroupId;
            var email = TenantLocalValue.LogonUserEmail;
            System.Threading.Tasks.Task t = System.Threading.Tasks.Task.Factory.StartNew(() => RealStartJobAsync(proxy, fSWorkerArgs, tenantId, email));
        }

        private async Task<FileSystemJobResult> RealStartJobAsync(AgentProxy proxy, RecordsJobArgs message, string tenantId, string email)
        {
            FileSystemJobResult result = new FileSystemJobResult() { Result = FileSystemResultEnum.Failed };
            try
            {
                TenantLocalValue.LogonGroupId = tenantId;
                TenantLocalValue.LogonUserEmail = email;
                lock (mStartJobLocker)
                {
                    result = StartJobWithRetryAsync(proxy, tenantId, message, GetRetryCount(), GetRetryInterval()).Result;
                }
            }
            catch (Exception e)
            {
                UpdateFailedJob(message, I18NEntity.GetString("RM_SS_FSFailedToStartJob"));
                logger.Error("An error occurred while starting job. JobId:{0} Error:{1}", message.JobId, e.ToString());
            }
            finally
            {
                if (result?.Result == FileSystemResultEnum.Failed)
                {
                    logger.Error(@$"Fail start job , job id:{message?.JobId}");
                }
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

        private async Task<FileSystemJobResult> StartJobWithRetryAsync(AgentProxy proxy, string tenantId, RecordsJobArgs message, int retryCount, int retryIntervalSecond)
        {
            FileSystemJobResult result = new FileSystemJobResult() { Result = FileSystemResultEnum.Failed };
            int mRetryCount = 0;
            string agentId = string.Empty;
            List<string> failedAgentIds = new List<string>();
            while (true)
            {
                try
                {
                    ICollection<AgentInformation> agents = await signalRService.GetAgentsByFarmIdAsync(tenantId, message.FarmId);
                    //signalRService.GetAgents(tenantId);
                    if (agents.Count == 0)
                    {
                        if (mRetryCount >= retryCount)
                        {
                            UpdateFailedJob(message, "RM_SS_SPLocalOneNoAgent");
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
                    var avaliableAgentIds = agents.Select(a => a.AgentId).ToList();
                    logger.Info("Available agent count : " + agents.Count);
                    var unusedAgentIds = avaliableAgentIds.Where(a => !failedAgentIds.Contains(a)).ToList();
                    if (unusedAgentIds.Count == 0)
                    {
                        failedAgentIds.Clear();
                    }
                    var agentJobCountGroup = RMSubJobDao.GetAgentJobCount(GetJobTypes(message.JobType));
                    agentId = GetAgentId(unusedAgentIds.Count > 0 ? unusedAgentIds : avaliableAgentIds, agentJobCountGroup);
                    AgentInformation agent = agents.Where(a => a.AgentId == agentId).First();
                    result = System.Threading.Tasks.Task.Run(() => proxy.InvokeOneAgentAysnc<SFileSystemJobExecute, RecordsJobArgs, FileSystemJobResult>(agent, new SFileSystemJobExecute() { MethodArgs = message })).Result;
                    logger.Debug("Send job to agent successfully. Agent id:{0} Job id:{1} Tenant id:{2}", agent.AgentId, message.JobId, agent.TenantId);
                }
                catch (Exception e)
                {
                    logger.Warn("An error occurred while starting job. JobId:{0} Error:{1}", message.JobId, e.ToString());
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
                    System.Threading.Thread.Sleep(retryIntervalSecond * 1000);
                    logger.Warn("Start job failed. Used agent:{0} Retry count:{1} Error:{2}", agentId, retryCount, result?.Message);
                }
                mRetryCount++;
            }

            if (result != null && result.Result == FileSystemResultEnum.Succeed)
            {
                await RMSubJobDao.UpdateAgentIdAsync(message.JobId, agentId);
                logger.Info("Finish to send message to agent.");
            }
            else
            {
                UpdateFailedJob(message, I18NEntity.GetString("RM_SS_FSFailedSendJobToAgent"));
                logger.Warn("Failed to send job to agent, job id:{0}", message.JobId);
            }
            return result;
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

        public async Task<int> GetAgentCountAsync(string farmId)
        {
            try
            {
                return (await signalRService.GetAgentsByFarmIdAsync(TenantLocalValue.LogonGroupId, farmId)).Count();
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while getting agent count. Error:{0}", e.ToString());
                return 0;
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
            else if (type == Hybrid.Contract.JobType.ImportFSSetting)
            {
                return new List<Contract.JobMonitor.JobType>() { Contract.JobMonitor.JobType.ImportFSSetting };
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
            else if (type == Hybrid.Contract.JobType.SharePointOnPremDataSync)
            {
                return new List<Contract.JobMonitor.JobType>() { Contract.JobMonitor.JobType.SPOnPremDataSync, Contract.JobMonitor.JobType.SPOnPremDataSyncSchedule };
            }
            else if (type == Hybrid.Contract.JobType.SPOnPremUniqueIDSetting)
            {
                return new List<Contract.JobMonitor.JobType>() { Contract.JobMonitor.JobType.SPOnPremUniqueIDSettingFullSchedule, Contract.JobMonitor.JobType.SPOnPremUniqueIDSettingIncrementalSchedule };
            }
            else if (type == Hybrid.Contract.JobType.SPOnPremGlobalSearch)
            {
                return new List<Contract.JobMonitor.JobType>() { Contract.JobMonitor.JobType.GlobalSearchAction };
            }
            else if (type == Hybrid.Contract.JobType.SPOnPremScanNode)
            {
                return new List<Contract.JobMonitor.JobType>() { Contract.JobMonitor.JobType.SPOnPremScanLocalNodes };
            }
            else
            {
                throw new Exception("invalid job type. Type:" + type.ToString());
            }
        }

        private int GetRetryCount()
        {
            int retryCount = RMGlobalConfiguration.AppConfig.GetNumberValue(Contract.Configurations.RMAppSettingKey.RETRY_COUNT_FOR_GET_AGENT, 3);
            return retryCount;
        }

        private int GetRetryInterval()
        {
            int retryInterval = RMGlobalConfiguration.AppConfig.GetNumberValue(Contract.Configurations.RMAppSettingKey.RETRY_INTERVAL_SECONDS_FOR_GET_AGENT, 30);
            return retryInterval;
        }
    }
}
