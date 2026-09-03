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
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.FileSystemRegister;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.FSMasterIndex;
using AvePoint.RA.Contract.RMWeb.SignalR;
using AvePoint.RA.Contract.RMWeb.SingalR;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.RACommonUtility.JobControl.O365Tenant;
using AvePoint.RA.Service.Common;
using AvePoint.RA.Service.Services.Tenant;
using AvePoint.RA.Service.SharePointSetting;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Util;
using static AvePoint.GCommon.Utility.I18N.EventIds.Configuration;

namespace AvePoint.RA.Service.RMTasks
{
    public class SubJobTaskExecutor : ITaskExecutor
    {
        #region Properties
        private RALogger mLogger = RALogger.GetInstance(typeof(SubJobTaskExecutor));
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public IJobMonitorService mJobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();

        public ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IRMFileSystemRegisterService FSRegisterService => PlatformWindsorManager.GetService<IRMFileSystemRegisterService>();
        private IFSMasterIndexService FSMasterIndexService => PlatformWindsorManager.GetService<IFSMasterIndexService>();

        public IHybridFileSystemWorkerService HybridFileSystemWorkerService => PlatformWindsorManager.GetService<IHybridFileSystemWorkerService>();
        public IHybridSharePointOnPremWorkerService HybridSharePointWorkerService => PlatformWindsorManager.GetService<IHybridSharePointOnPremWorkerService>();
        #endregion

        /// <summary>
        /// 获取Runnable的sub job, send to azure queue.  once/30s
        /// </summary>
        /// <param name="context"></param>
        public System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {
                       
                var tInfos = TenantService.GetAllAvailableTenantInfo();
                foreach (var tInfo in tInfos)
                {
                    TenantUtil.RunUnderTenant(tInfo.TenantId, tInfo.RegisterEmail, SubJobChecker);
                }
            }
            catch (Exception e)
            {
                mLogger.Error("Sub Job Runnable Checker Task error {0}", e.ToString());
            }
            return System.Threading.Tasks.Task.CompletedTask;
        }

        public void SubJobChecker()
        {
            try
            {
                //获取Runnable的sub job, send to azure queue. 
                List<RMSubJobDto> subJobs = mJobMonitorService.GetRunnableSubJob();
                var enableSuperPriorityJobQueue = bool.TryParse(KeyValueDao.GetValueByKey(RMKeyValuesConstants.ENABLE_SUPER_PRIORITY_JOB_QUEUE)?.Value, out var enableSuperQueue) && enableSuperQueue;
                string superJobQueueName = null;
                if (enableSuperPriorityJobQueue)
                {
                    superJobQueueName = KeyValueDao.GetValueByKey(RMKeyValuesConstants.SUPER_PRIORITY_JOB_QUEUE_NAME)?.Value;
                    if (string.IsNullOrEmpty(superJobQueueName))
                    {
                        superJobQueueName = RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.HIGHEST_PRIORITY_JOB_QUEUE_NAME];
                        if (string.IsNullOrEmpty(superJobQueueName))
                        {
                            enableSuperPriorityJobQueue = false;
                            mLogger.Error("Enable highest job queue, but not config for it");
                        }
                        else
                        {
                            mLogger.Info($"Use default highest job queue name: {superJobQueueName}");
                        }
                    }
                    else
                    {
                        mLogger.Info($"Custom highest job queue name: {superJobQueueName}");
                    }
                }

                var funcPriorityPushToHighQueue = (Contract.CloudService.JobQueueMessage message, bool isM365UserSeatControlledJob) =>
                {
                    if (enableSuperPriorityJobQueue)
                    {
                        mLogger.Info($"Start to send {message.JobId} to highest job queue {superJobQueueName}");
                        JobQueueService.HandleCustomerMessage(message, superJobQueueName);
                    }
                    else if(isM365UserSeatControlledJob)
                    {
                        mLogger.Info("Start to send o365 {0} to high level queue", message.JobId);
                        JobQueueService.HandleO365Message(message);
                    }
                    else
                    {
                        JobQueueService.HandleMessage(message);
                    }
                };

                if (subJobs != null && subJobs.Count > 0)
                {
                    mLogger.Info("Get runnable sub job count{0}", subJobs.Count);
                    foreach (RMSubJobDto dto in subJobs)
                    {
                        var subJobId = dto.Id;
                        if (RMO365TenantSubJobControlConstants.CONTROLLED_JOBS.Contains((JobType)dto.JobType) && !string.IsNullOrWhiteSpace(dto.O365TenantId))
                        {
                            SendSubJob(subJobId,
                                mJobMonitorService.UpdateRunable(subJobId, RecordsConstants.SubJob_Runnable_Runing),
                                (subJobId) =>
                                {
                                    funcPriorityPushToHighQueue(new Contract.CloudService.JobQueueMessage()
                                    {
                                        JobId = subJobId,
                                        RunBy = JobRunBy.Control,
                                        JobType = (JobType)dto.JobType,
                                        CommandLine = GetCmdLine(dto)
                                    }, true);
                                });
                        }
                        else if ((JobType)dto.JobType == JobType.ArchiverRetention || (JobType)dto.JobType == JobType.EXOArchiverRetention || (JobType)dto.JobType == JobType.TeamsArchiverRetention)
                        {
                            SendSubJob(subJobId,
                                mJobMonitorService.UpdateRunable(subJobId, RecordsConstants.SubJob_Runnable_Runing),
                                (subJobId) =>
                                {
                                    funcPriorityPushToHighQueue(new Contract.CloudService.JobQueueMessage()
                                    {
                                        JobId = subJobId,
                                        RunBy = JobRunBy.Control,
                                        JobType = (JobType)dto.JobType,
                                        CommandLine = GetCmdLine(dto)
                                    }, false);
                                });
                        }
                        else if (dto.JobType == (int)JobType.RecordsExplorerMove)
                        {
                            SendSubJob(subJobId,
                                mJobMonitorService.UpdateRunable(subJobId, RecordsConstants.SubJob_Runnable_Runing, true),
                                (subJobId) =>
                                {
                                    //处理Move被互斥的job
                                    List<string> runningId = mJobMonitorService.GetRunningMoveJobByDestUrl(dto.String1);
                                    if (runningId.IsNullOrEmpty())
                                    {
                                        mLogger.Info(string.Format("Start explorer move job : {0}", subJobId));
                                        JobQueueService.HandleMessage(new Contract.CloudService.JobQueueMessage()
                                        {
                                            JobId = subJobId,
                                            RunBy = JobRunBy.Control,
                                            JobType = JobType.RecordsExplorerMove,
                                            CommandLine = string.Format("{0} {1}", JobType.RecordsExplorerMove.ToString(), subJobId),
                                        });
                                        mLogger.Info(string.Format("Finished add job to job queue, job id is : {0}", subJobId));
                                    }
                                    else
                                    {
                                        mLogger.Info("Move records job {0} can not run, for there is {1} using the same destination {2}", subJobId, string.Join(",", runningId.ToArray()), dto.String1);
                                    }
                                });
                        }
                        else if (dto.JobType == (int)JobType.PhysicalSetPermission)
                        {
                            //有多个Physical Set Permission Job的情况下，后起的Job需要等前面的Job都finish才可以处理,否则是Wait状态
                            List<string> runningJobIds = mJobMonitorService.GetRunningSetPermissionJob(subJobId);
                            if (runningJobIds.IsNullOrEmpty())
                            {
                                SendSubJob(subJobId,
                                    mJobMonitorService.UpdateRunable(subJobId, RecordsConstants.SubJob_Runnable_Runing),
                                    (subJobId) =>
                                    {
                                        JobQueueService.HandleMessage(new Contract.CloudService.JobQueueMessage()
                                        {
                                            JobId = subJobId,
                                            RunBy = JobRunBy.Control,
                                            JobType = JobType.PhysicalSetPermission,
                                            CommandLine = string.Format("{0} {1}", JobType.PhysicalSetPermission, subJobId),
                                        });
                                        mLogger.Info($"run physical set permission job success, JobId : {subJobId}.");
                                    });
                            }
                            else
                            {
                                mLogger.Info($"Physical set permission job [{subJobId}] can not run, There are jobs of the same type [{string.Join(",", runningJobIds.ToArray())}] is running.");
                            }
                        }
                        else if (dto.JobType == (int)JobType.FSDataSynchronization || dto.JobType == (int)JobType.FSDataSynchronizationSchedule)
                        {
                            var enabledJPMCFSFeature = KeyValueDao.TryGetBoolValue(KeyNameCollection.EnableJPMCFileSystemFeature, out var enabled) && enabled;
                            SendSubJob(subJobId,
                                mJobMonitorService.UpdateRunable(subJobId, RecordsConstants.SubJob_Runnable_Runing),
                                (subJobId) =>
                                {
                                    mLogger.Info("Start to run fs data sync job. Job Id {0}", subJobId);
                                    var settings = mJobMonitorService.GetJobContextSettingByJobId(subJobId);
                                    var nodes = SerializerHelper.DeserializeByDataContractSerializer<List<RMFSTreeNode>>(settings);
                                    var node = nodes.First();
                                    //HybridFileSystemWorkerService.StartJob(new Hybrid.Contract.RecordsJobArgs()
                                    //{
                                    //    JobId = dto.Id,
                                    //    JobType = AvePoint.Hybrid.Contract.JobType.FSDataSync,
                                    //    TenantId = TenantLocalValue.LogonGroupId
                                    //});
                                    HybridFileSystemWorkerService.StartJobWithConnectionGroupId(new Hybrid.Contract.RecordsJobArgs()
                                    {
                                        JobId = subJobId,
                                        JobType = AvePoint.Hybrid.Contract.JobType.FSDataSync,
                                        TenantId = TenantLocalValue.LogonGroupId,
                                        Extensions = enabledJPMCFSFeature ? KeyNameCollection.EnableJPMCFileSystemFeature : string.Empty
                                    }, node.ConnGroupId);
                                });
                        }
                        else if (dto.JobType == (int)JobType.DiscoveryFileSystemV1)
                        {
                            SendSubJob(subJobId,
                               mJobMonitorService.UpdateRunable(subJobId, RecordsConstants.SubJob_Runnable_Runing),
                               (subJobId) =>
                               {
                                   mLogger.Info("Start to run fs data sync job. Job Id {0}", subJobId);
                                   var settings = mJobMonitorService.GetJobContextSettingByJobId(subJobId);
                                   var connection = SerializerHelper.DeserializeByDataContractSerializer<RMFSDiscoveryJobSettingDto>(settings);
                                   HybridFileSystemWorkerService.StartJobWithConnectionGroupId(new Hybrid.Contract.RecordsJobArgs()
                                   {
                                       JobId = subJobId,
                                       JobType = AvePoint.Hybrid.Contract.JobType.FSDiscovery,
                                       TenantId = TenantLocalValue.LogonGroupId
                                   }, connection.ConnectionGroupId);
                               });
                        }
                        else if (dto.JobType == (int)JobType.FSItemsFilesDueDisposal)
                        {
                            SendSubJob(subJobId,
                                mJobMonitorService.UpdateRunable(subJobId, RecordsConstants.SubJob_Runnable_Runing),
                                (subJobId) =>
                                {
                                    mLogger.Info("Start to run FSItemsFilesDueDisposal job from timer. Job Id {0}", subJobId);
                                    var settings = mJobMonitorService.GetJobContextSettingByJobId(subJobId);
                                    var nodes = SerializerHelper.DeserializeByDataContractSerializer<List<FSTreeNodeDto>>(settings);
                                    var node = nodes.FirstOrDefault();
                                    HybridFileSystemWorkerService.StartJobWithConnectionGroupId(new Hybrid.Contract.RecordsJobArgs()
                                    {
                                        JobId = subJobId,
                                        JobType = AvePoint.Hybrid.Contract.JobType.FSContentDueReport,
                                        TenantId = TenantLocalValue.LogonGroupId
                                    }, new Guid(node?.ParentId));
                                });
                        }
                        else if (dto.JobType == (int)JobType.FSCreateAndDestroyedFileReport)
                        {
                            SendSubJob(subJobId,
                                mJobMonitorService.UpdateRunable(subJobId, RecordsConstants.SubJob_Runnable_Runing),
                                (subJobId) =>
                                {
                                    mLogger.Info("Start to run FSCreateAndDestroyedFileReport job from timer. Job Id {0}", subJobId);
                                    var settings = mJobMonitorService.GetJobContextSettingByJobId(subJobId);
                                    var nodes = SerializerHelper.DeserializeByDataContractSerializer<List<FSTreeNodeDto>>(settings);
                                    var node = nodes.FirstOrDefault();
                                    HybridFileSystemWorkerService.StartJobWithConnectionGroupId(new Hybrid.Contract.RecordsJobArgs()
                                    {
                                        JobId = subJobId,
                                        JobType = AvePoint.Hybrid.Contract.JobType.FSCreationAndDestructionReport,
                                        TenantId = TenantLocalValue.LogonGroupId
                                    }, new Guid(node?.ParentId));
                                });
                        }
                        else if (dto.JobType == (int)JobType.FSDisposal || dto.JobType == (int)JobType.FSDisposalSchedule)
                        {
                            var enabledJPMCFSFeature = KeyValueDao.TryGetBoolValue(KeyNameCollection.EnableJPMCFileSystemFeature, out var enabled) && enabled;
                            SendSubJob(subJobId,
                                mJobMonitorService.UpdateRunable(subJobId, RecordsConstants.SubJob_Runnable_Runing),
                                (subJobId) =>
                                {
                                    mLogger.Info("Start to run fs data sync job. Job Id {0}", subJobId);
                                    var settings = mJobMonitorService.GetJobContextSettingByJobId(subJobId);
                                    var nodes = SerializerHelper.DeserializeByDataContractSerializer<List<RMFSTreeNode>>(settings);
                                    var node = nodes.FirstOrDefault();
                                    if (node == null)
                                    {
                                        throw new NullReferenceException("The node is null");
                                    }
                                    //HybridFileSystemWorkerService.StartJob(new Hybrid.Contract.RecordsJobArgs()
                                    //{
                                    //    JobId = dto.Id,
                                    //    JobType = AvePoint.Hybrid.Contract.JobType.FSDisposal,
                                    //    TenantId = TenantLocalValue.LogonGroupId
                                    //});
                                    HybridFileSystemWorkerService.StartJobWithConnectionGroupId(new Hybrid.Contract.RecordsJobArgs()
                                    {
                                        JobId = subJobId,
                                        JobType = AvePoint.Hybrid.Contract.JobType.FSDisposal,
                                        TenantId = TenantLocalValue.LogonGroupId,
                                        Extensions = enabledJPMCFSFeature ? KeyNameCollection.EnableJPMCFileSystemFeature : string.Empty
                                    }, node.ConnGroupId);
                                });
                        }
                        else if (dto.JobType == (int)JobType.FSDisposalByClassCode)
                        {
                            var enabledJPMCFSFeature = KeyValueDao.TryGetBoolValue(KeyNameCollection.EnableJPMCFileSystemFeature, out var enabled) && enabled;
                            SendSubJob(subJobId,
                                mJobMonitorService.UpdateRunable(subJobId, RecordsConstants.SubJob_Runnable_Runing),
                                (subJobId) =>
                                {
                                    mLogger.Info("Start to run fs disposal by class code sub job. Job Id {0}", subJobId);
                                    var settings = mJobMonitorService.GetJobContextSettingByJobId(subJobId);
                                    var nodes = SerializerHelper.DeserializeByDataContractSerializer<List<RMFSTreeNode>>(settings);
                                    var node = nodes.FirstOrDefault();
                                    if (node == null)
                                    {
                                        throw new NullReferenceException("The node is null for FSDisposalByClassCode sub job.");
                                    }
                                    HybridFileSystemWorkerService.StartJobWithConnectionGroupId(new Hybrid.Contract.RecordsJobArgs()
                                    {
                                        JobId = subJobId,
                                        JobType = AvePoint.Hybrid.Contract.JobType.FSDisposalByClassCode,
                                        TenantId = TenantLocalValue.LogonGroupId,
                                        Extensions = enabledJPMCFSFeature ? KeyNameCollection.EnableJPMCFileSystemFeature : string.Empty
                                    }, node.ConnGroupId);
                                });
                        }
                        else if (dto.JobType == (int)JobType.FSRetain || dto.JobType == (int)JobType.FSRetainSimulate)
                        {
                            SendSubJob(subJobId,
                                mJobMonitorService.UpdateRunable(subJobId, RecordsConstants.SubJob_Runnable_Runing),
                                (subJobId) =>
                                {
                                    mLogger.Info("Start to run fs retain job. Job Id {0}", subJobId);
                                    var settings = mJobMonitorService.GetJobContextSettingByJobId(subJobId);
                                    List<ArchiverPruningJob> retainInfos = SerializerHelper.DeserializeByJsonSerializer<List<ArchiverPruningJob>>(settings);
                                    var tempRetainInfo = retainInfos?.FirstOrDefault();
                                    string connId = tempRetainInfo?.SiteId;
                                    string agentId = tempRetainInfo?.AgentId ?? "";
                                    ConnectionDto connection = new ConnectionDto();
                                    try
                                    {
                                        connection = FSRegisterService.GetConnectionByIdAsync(new Guid(connId)).GetAwaiter().GetResult();
                                    }
                                    catch (Exception e)
                                    {
                                        mLogger.Error($"Get connection by id failed, connection id is {connId},{e}");
                                    }

                                    var jobType = dto.JobType == (int)JobType.FSRetain
                                        ? AvePoint.Hybrid.Contract.JobType.FSRetain
                                        : AvePoint.Hybrid.Contract.JobType.FSRetainSimulate;
                                    HybridFileSystemWorkerService.StartJobWithConnectionGroupId(new Hybrid.Contract.RecordsJobArgs()
                                    {
                                        JobId = subJobId,
                                        JobType = jobType,
                                        TenantId = TenantLocalValue.LogonGroupId,
                                        AgentId = agentId
                                    }, connection == null ? new Guid() : connection.GroupId);
                                });
                        }
                        else if (dto.JobType == (int)JobType.SPOnPremApplySetting || dto.JobType == (int)JobType.SPOnPremApplySettingSchedule)
                        {
                            SendSubJob(subJobId,
                                mJobMonitorService.UpdateRunable(subJobId, RecordsConstants.SubJob_Runnable_Runing),
                                (subJobId) => {
                                    mLogger.Info("Start to run on premise SharePoint setting job. Job Id {0}", subJobId);
                                    HybridSharePointWorkerService.StartSPJob(new Hybrid.Contract.RecordsJobArgs()
                                    {
                                        JobId = subJobId,
                                        JobType = AvePoint.Hybrid.Contract.JobType.SharePointOnPremApplySetting,
                                        TenantId = TenantLocalValue.LogonGroupId,
                                        FarmId = dto.FarmId
                                    });
                                });
                        }
                        else if (dto.JobType == (int)JobType.SPOnPremDataSync || dto.JobType == (int)JobType.SPOnPremDataSyncSchedule)
                        {
                            SendSubJob(subJobId,
                                mJobMonitorService.UpdateRunable(subJobId, RecordsConstants.SubJob_Runnable_Runing),
                                (subJobId) => {
                                    mLogger.Info("Start to run on premise SharePoint data sync job. Job Id {0}", subJobId);
                                    HybridSharePointWorkerService.StartSPJob(new Hybrid.Contract.RecordsJobArgs()
                                    {
                                        JobId = subJobId,
                                        JobType = AvePoint.Hybrid.Contract.JobType.SharePointOnPremDataSync,
                                        TenantId = TenantLocalValue.LogonGroupId,
                                        FarmId = dto.FarmId
                                    });
                                });
                        }
                        else if (dto.JobType == (int)JobType.SPOnPremEnforceRuleAction || dto.JobType == (int)JobType.SPOnPremEnforceRuleActionSchedule)
                        {
                            SendSubJob(subJobId,
                                mJobMonitorService.UpdateRunable(subJobId, RecordsConstants.SubJob_Runnable_Runing),
                                (subJobId) =>
                                {
                                    mLogger.Info("Start to run onpremise enforce rule action job. Job Id {0}.", subJobId);
                                    HybridSharePointWorkerService.StartSPJob(new Hybrid.Contract.RecordsJobArgs()
                                    {
                                        JobId = subJobId,
                                        JobType = AvePoint.Hybrid.Contract.JobType.SharePointOnPremEnforceRuleAction,
                                        TenantId = TenantLocalValue.LogonGroupId,
                                        FarmId = dto.FarmId
                                    });
                                });

                        }
                        else if (dto.JobType == (int)JobType.SPOnPremUniqueIDSettingFullSchedule || dto.JobType == (int)JobType.SPOnPremUniqueIDSettingIncrementalSchedule)
                        {
                            SendSubJob(subJobId,
                                mJobMonitorService.UpdateRunable(subJobId, RecordsConstants.SubJob_Runnable_Runing),
                                (subJobId) =>
                                {
                                    mLogger.Info("Start to run on premise SharePoint unique id job. Job Id {0}", subJobId);
                                    HybridSharePointWorkerService.StartSPJob(new Hybrid.Contract.RecordsJobArgs()
                                    {
                                        JobId = subJobId,
                                        JobType = AvePoint.Hybrid.Contract.JobType.SPOnPremUniqueIDSetting,
                                        TenantId = TenantLocalValue.LogonGroupId,
                                        FarmId = dto.FarmId
                                    });
                                });
                        }
                        else if (dto.JobType == (int)JobType.EXORecordsDisposal)
                        {
                            SendSubJob(subJobId,
                                mJobMonitorService.UpdateRunable(subJobId, RecordsConstants.SubJob_Runnable_Runing),
                                (subJobId) =>
                                {
                                    mLogger.Info("Start to send o365 {0} to high level queue", subJobId);
                                    JobQueueService.HandleO365Message(new Contract.CloudService.JobQueueMessage()
                                    {
                                        JobId = subJobId,
                                        RunBy = JobRunBy.Control,
                                        JobType = (JobType)dto.JobType,
                                        CommandLine = GetCmdLine(dto)
                                    });
                                });
                        }
                        else
                        {
                            SendSubJob(subJobId,
                                mJobMonitorService.UpdateRunable(subJobId, RecordsConstants.SubJob_Runnable_Runing),
                                (subJobId) =>
                                {
                                    //处理正常的子job
                                    mLogger.Info("Start to send {0} to queue", subJobId);
                                    JobQueueService.HandleMessage(new Contract.CloudService.JobQueueMessage()
                                    {
                                        JobId = subJobId,
                                        RunBy = JobRunBy.Control,
                                        JobType = (JobType)dto.JobType,
                                        CommandLine = GetCmdLine(dto)
                                    });
                                });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("error occurred while check runable sub job:{0}", ex.ToString());
            }
            
        }

        private void SendSubJob(string subJobId, bool updateSubJobToRunningResult, Action<string> realSendJobAction)
        {
            if (updateSubJobToRunningResult)
            {
                realSendJobAction?.Invoke(subJobId);
                return;
            }
            mLogger.Warn($"Skipped to send {subJobId} to queue.");
        }

        private string GetCmdLine(RMSubJobDto subJob)
        {
            int jobType = subJob.JobType;
            if (jobType == (int)JobType.ItemsFilesDueDisposal
                || jobType == (int)JobType.EXOItemsFilesDueDisposalReport
                || jobType == (int)JobType.BCSTermUsageReport
                || jobType == (int)JobType.EXOTermUsageReport
                || jobType == (int)JobType.RetiredTermReport
                || jobType == (int)JobType.EXORetiredTermUsageReport
                || jobType == (int)JobType.OrphanedTermReport
                || jobType == (int)JobType.EXOOrphanedTermUsageReport
                || jobType == (int)JobType.CreateAndDestroyedFileReport
                || jobType == (int)JobType.EXOCreateAndDestroyedFileReport
                || jobType == (int)JobType.OneDriveItemsFilesDueDisposalReport
                || jobType == (int)JobType.OneDriveTermUsageReport
                || jobType == (int)JobType.OneDriveCreateAndDestroyedFileReport
                || jobType == (int)JobType.SPOnPremItemsFilesDueDisposal
                || jobType == (int)JobType.TeamsCreateAndDestroyedFileReport
                || jobType == (int)JobType.TeamsItemsFilesDueDisposalReport
                || jobType == (int)JobType.TeamsBCSTermUsageReport
                || jobType == (int)JobType.TeamsRetiredTermUsageReport
                || jobType == (int)JobType.TeamsOrphanedTermUsageReport
                || jobType == (int)JobType.GoogleCreateAndDestroyedFileReport
                || jobType == (int)JobType.GoogleBCSTermUsageReport
                || jobType == (int)JobType.GoogleOrphanedTermUsageReport
                || jobType == (int)JobType.GoogleRetiredTermUsageReport
                || jobType == (int)JobType.GoogleItemsFilesDueDisposalReport)
            {
                string string1 = subJob.String1;
                return string.Format(string1, subJob.JobType, subJob.Id);
            }
            else
            {
                return string.Format("{0} {1}", subJob.JobType, subJob.Id);
            }
        }
    }
}
