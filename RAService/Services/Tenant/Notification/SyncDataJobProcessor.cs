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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.RA.Cache.Services;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Cache;
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.Tenant.Notification.Excutor;
using Cloud.Sdk.Data.Aos.Tenant;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AvePoint.RA.Service.Services.Tenant.Notification
{
    public interface ISyncDataJobProcessor
    {
        void Run(JobQueueMessage msg);
    }

    public class SyncDataJobProcessor : ISyncDataJobProcessor
    {

        private static RALogger logger = RALogger.GetInstance(typeof(SyncDataJobProcessor));
        private Dictionary<RMAosQueueMessageType, IAosQueueMessageExecutor> executors;
        private SyncNodesExecutor syncNodesExecutor;
        private SyncDataJobContext jobContext;
        private IRMReportManager reportManager = ReportMangerFactory.Instance.ReportManager;

        private IRMAOSNotificationService AOSNotificationService => PlatformWindsorManager.GetService<IRMAOSNotificationService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IRMRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private IRMMailboxDao MailboxDao => PlatformWindsorManager.GetService<IRMMailboxDao>();

        private static int ProcessedItemCount { get; set; }

        private void InitProcessor(JobQueueMessage msg)
        {
            ReportMangerFactory.Instance.ReportManager.DetailBufferCount = 1000;
            ReportMangerFactory.Instance.ReportManager.StartUpdateJobProgress();
            jobContext = BuildContext();

            syncNodesExecutor = new SyncNodesExecutor(jobContext);
            executors = new Dictionary<RMAosQueueMessageType, IAosQueueMessageExecutor>()
            {
                { RMAosQueueMessageType.InitNodes, syncNodesExecutor },
                { RMAosQueueMessageType.LastSyncMessage, syncNodesExecutor },
                { RMAosQueueMessageType.SyncNodes, syncNodesExecutor },
                { RMAosQueueMessageType.DeleteNodes, new DeleteNodesExecutor(jobContext) }
            };
        }

        private SyncDataJobContext BuildContext()
        {
            RMDependTypeForInitNode dependTypeForInitNode;
            var tenantInitNodeState = TenantService.GetTenantInitNodeState(TenantLocalValue.LogonGroupId, out dependTypeForInitNode);
            return new SyncDataJobContext()
            {
                TenantInitNodeState = tenantInitNodeState,
                DependTypeForInitNode = dependTypeForInitNode,
                TenantGroupId = TenantLocalValue.LogonGroupId
            };
        }

        public void Run(JobQueueMessage msg)
        {
            var tenantId = TenantLocalValue.LogonGroupId;
            bool hasSuccessMsg = false;
            bool hasFailedMsg = false;
            bool quitJob = false;
            try
            {
                InitProcessor(msg);
                List<RMAosQueueMessage> queueMessages = null;
                bool isInitNodesJob = string.Equals("true", msg.Extension, StringComparison.OrdinalIgnoreCase);
                logger.Warn($"Is init job: [{isInitNodesJob}]");
                if (isInitNodesJob)
                {
                    queueMessages = AOSNotificationService.GetInitNodeMessage(tenantId);
                    if (jobContext.TenantInitNodeState == RMInitNodeState.Synced)
                    {
                        logger.Warn("The tenant has been initialized.");
                        foreach (var message in queueMessages)
                        {
                            AOSNotificationService.Delete(message.QueueMessageId);
                        }
                        reportManager.SetJobFinished(JobStatus.Finished);
                        return;
                    }
                    ClearCacheAndDBNodes(tenantId);
                    syncNodesExecutor.InitDataForFirstJob();
                }
                else
                {
                    queueMessages = AOSNotificationService.GetSyncNodeMessages(tenantId);
                }

                UpgradeExecutor.Upgrade();

                while (queueMessages.Count > 0)
                {
                    queueMessages = SortQueueMessages(queueMessages);
                    ReportMangerFactory.Instance.ReportManager.IncreaseBase(10 * queueMessages.Count);
                    foreach (var queueMsg in queueMessages)
                    {
                        if(ExecuteSync(queueMsg))
                        {
                            hasSuccessMsg = true;
                        }
                        else
                        {
                            logger.Error($"The message: [{queueMsg.QueueMessageId}] processed failed.");
                            hasFailedMsg = true;
                            //如果有Incremental Sync Message 处理失败会被忽略，并继续处理其他Message
                            //因为AOS里Scan或者Import后5分钟，会再发送LastSyncJobMessage，SRN Job会进行Full Sync
                            if(isInitNodesJob || queueMsg.IsLastSyncJob || queueMsg.MessageType == RMAosQueueMessageType.DeleteNodes)
                            {
                                quitJob = true;
                                break;
                            }
                            else
                            {
                                AOSNotificationService.Delete(queueMsg.QueueMessageId);
                            }
                        }
                    }

                    if(quitJob || isInitNodesJob)
                    {
                        break;
                    }
                    else
                    {
                        queueMessages = AOSNotificationService.GetSyncNodeMessages(tenantId);
                    }
                }
                
                SetJobCompletedStatus(hasFailedMsg, hasSuccessMsg);
            }
            catch (Exception ex)
            {
                SetJobCompletedStatus(true, hasSuccessMsg);
                logger.Error($"Error occurred while syncing data: {ex}");
            }
            finally
            {
                //AOSNotificationService.DecrementRunningSRNJobCount(tenantId);
            }
        }

        private bool ExecuteSync(RMAosQueueMessage queueMsg)
        {
            var messageContent = CspCommunicationWrapper.WrapKeyToBase64StringByDefault(
                CryptoUtil.ConvertStringToBytes(JsonConvert.SerializeObject(queueMsg)));
            logger.Info($"Execute queue msg: {messageContent}");
            IAosQueueMessageExecutor executor = null;
            var msgType = queueMsg.MessageType;
            if (executors.TryGetValue(msgType, out executor))
            {
                if (executor.Execute(queueMsg))
                {
                    AOSNotificationService.Delete(queueMsg.QueueMessageId);
                }
                else
                {
                    return false;
                }
            }
            else
            {
                logger.Warn($"No available executor of message type: {queueMsg.MessageType}");
            }
            return true;
        }

        private List<RMAosQueueMessage> SortQueueMessages(List<RMAosQueueMessage> messages)
        {
            return messages.OrderBy(m => m.ReceiveMessageTime).ToList();
        }

        private void SetJobCompletedStatus(bool hasFailedMsg, bool hasSuccessMsg)
        {
            if (jobContext.TenantInitNodeState != RMInitNodeState.Synced)
            {
                TenantService.UpdateSyncNodeState(TenantLocalValue.LogonGroupId, hasFailedMsg ? RMInitNodeState.SyncFailed : RMInitNodeState.Synced);
            }
            var jobComment = "";
            if(!hasFailedMsg && ProcessedItemCount == 0)
            {
                jobComment = "RM_JS_SRN_Comment_NoChange";
            }
            reportManager.SetJobFinished(hasFailedMsg ? (hasSuccessMsg ? JobStatus.FinishWithException : JobStatus.Failed) : JobStatus.Finished, jobComment);
        }

        private void ClearCacheAndDBNodes(string tenantGroupId)
        {
            logger.Info("Begain to clear cache and DBNodes.");

            var mbKey = RedisKeyUtil.GenerateSyncNodesTenantLevelKey(tenantGroupId, AOSSync_TenantLevelFuncType.Mailbox);
            var pcKey = RedisKeyUtil.GenerateSyncNodesTenantLevelKey(tenantGroupId, AOSSync_TenantLevelFuncType.PrivateChannel);
            var rnKey = RedisKeyUtil.GenerateSyncNodesTenantLevelKey(tenantGroupId, AOSSync_TenantLevelFuncType.RemoteNode);

            if (RedisCacheService.CacheProvider.KeyExists(mbKey))
            {
                RedisCacheService.CacheProvider.KeyDel(mbKey);
            }
            if (RedisCacheService.CacheProvider.KeyExists(pcKey))
            {
                RedisCacheService.CacheProvider.KeyDel(pcKey);
            }
            if (RedisCacheService.CacheProvider.KeyExists(rnKey))
            {
                RedisCacheService.CacheProvider.KeyDel(rnKey);
            }

            RemoteNodeDao.ClearAll();
            MailboxDao.ClearAll();

            logger.Info("Finish to clear cache and DBNodes.");
        }

        #region Sync nodes Job Details
        internal static void AddJobDetails4ContainerAdded(RMRemoteNodeSourceType sourceType, IEnumerable<string> containers)
        {
            foreach (var container in containers)
            {
                AddJobDetails4Added(sourceType, container, string.Empty);
            }
        }

        internal static void AddJobDetails4ContainerUpdate(RMRemoteNodeSourceType sourceType, IEnumerable<string> containers)
        {
            foreach (var container in containers)
            {
                AddJobDetails4Updated(sourceType, container, string.Empty);
            }
        }

        internal static void AddJobDetails4ObjectAdded(RMRemoteNodeSourceType sourceType, string container, IEnumerable<string> objects)
        {
            foreach (var objectName in objects)
            {
                AddJobDetails4Added(sourceType, container, objectName);
            }
        }
        internal static void AddJobDetails4Added(RMRemoteNodeSourceType sourceType, string container, string objectName = "")
        {
            AddJobDetail(RMSyncRemoteNodeAction.Add, sourceType, container, objectName);
        }

        internal static void AddJobDetails4Removed(RMRemoteNodeSourceType sourceType, string container, string objectName = "")
        {
            AddJobDetail(RMSyncRemoteNodeAction.Delete, sourceType, container, objectName);
        }

        internal static void AddJobDetails4Removed(RMRemoteNodeSourceType sourceType, string container, IEnumerable<string> objects)
        {
            foreach (var objectName in objects)
            {
                AddJobDetails4Removed(sourceType, container, objectName);
            }
        }

        internal static void AddJobDetails4Updated(RMRemoteNodeSourceType sourceType, string container, string objectName = "")
        {
            AddJobDetail(RMSyncRemoteNodeAction.Update, sourceType, container, objectName);
        }
        internal static void AddJobDetails4Updated(RMRemoteNodeSourceType sourceType, string container, IEnumerable<string> objects)
        {
            foreach (var objectName in objects)
            {
                AddJobDetail(RMSyncRemoteNodeAction.Update, sourceType, container, objectName);
            }
        }

        internal static void AddJobDetail(
            RMSyncRemoteNodeAction actionType,
            RMRemoteNodeSourceType sourceType,
            string container,
            string objectName = "",
            JobDetailsStatus status = JobDetailsStatus.Successful,
            string exceptionMsg = null)
        {
            try
            {
                ReportMangerFactory.Instance.ReportManager.SendJobDetail(new JMSyncRemoteNodesJobDetails()
                {
                    Container = GetDefaultGroupName(sourceType, container),
                    ObjectName = GetDefaultGroupName(sourceType, objectName),
                    ItemType = GetNodeTypeString(sourceType),
                    Action = GetActionTypeString(actionType),
                    Status = status,
                    Comment = exceptionMsg,
                });

                ProcessedItemCount++;
            }
            catch(Exception e)
            {
                logger.Error($"An error occurred while add detail. Error: {e}");
            }
            
        }

      /*  private static void PushAllJobDetails()
        {
            foreach (var item in JobDetailsCache.OrderBy(d => $"{(d.ItemType == "RM_JS_Common_ReportType_SharePoint" ? "1" : "2")}{d.Container}{d.ObjectName}"))
            {
                ReportMangerFactory.Instance.ReportManager.SendJobDetail(item);
            }
            JobDetailsCache.Clear();
        }*/

        internal static string GetActionTypeString(RMSyncRemoteNodeAction actionType)
        {
            switch (actionType)
            {
                case RMSyncRemoteNodeAction.Add:
                    return "RM_JS_SRN_Action_Add";
                case RMSyncRemoteNodeAction.Delete:
                    return "RM_JS_SRN_Action_Delete";
                case RMSyncRemoteNodeAction.Update:
                    return "RM_JS_SRN_Action_Update";
                default:
                    return string.Empty;
            }
        }

        internal static string GetNodeTypeString(RMRemoteNodeSourceType sourceType)
        {
            switch (sourceType)
            {
                case RMRemoteNodeSourceType.SharePointOnline:
                    return "RM_JS_Common_ReportType_SharePoint";
                case RMRemoteNodeSourceType.ExchangeOnline:
                    return "RM_JS_Common_ReportType_Exchange";
                case RMRemoteNodeSourceType.OneDrive:
                    return "RM_JS_Common_ReportType_OneDrive";
                default:
                    return string.Empty;
            }
        }

        private static string GetDefaultGroupName(RMRemoteNodeSourceType sourceType, string containerName)
        {
            if(sourceType == RMRemoteNodeSourceType.SharePointOnline)
            {
                if (containerName == RMConstants.DEFAULT_O365_SITES_GROUP)
                {
                    return "RM_SPS_DefaultGroupTeamSiteContainer";
                }
                if (containerName == RMConstants.DEFAULT_SPSITES_GROUP)
                {
                    return "RM_SPS_DefaultSharePointSitesGroup";
                }
                if (containerName == RMConstants.DEFAULT_SKYDRIVEPROS_GROUP)
                {
                    return "RM_SPS_DefaultOneDriveforBusinessGroup";
                }
                if (containerName == RMConstants.DefaultPrivateChannelSitesGroup)
                {
                    return "RM_SPS_DefaultPrivateChannelSitesContainer";
                }
            }
            else if(sourceType == RMRemoteNodeSourceType.OneDrive)
            {
                if(string.Equals(containerName, RMConstants.DEFAULT_SKYDRIVEPROS_GROUP))
                {
                    return "RM_SPS_DefaultOneDriveforBusinessGroup";
                }
            }
            else
            {
                if (string.Equals(containerName, RMConstants.DEFAULT_MAILBOX_GROUP))
                {
                    return "RM_EXO_Default_Container";
                }
                else if (string.Equals(containerName, RMConstants.DEFAULT_O365_GROUPS_GROUP))
                {
                    return "Default Microsoft 365 Group Mailbox Container";
                }
            }
            return containerName;
        }
        #endregion
    }
}
