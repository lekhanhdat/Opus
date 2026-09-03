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
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.SyncNode;
using AvePoint.RA.Contract.SyncNode.GoogleSyncNode;
using AvePoint.RA.DB.Core;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMSynchronize.SyncNodeFromAOS
{
    public class RMSyncNodeJobManager
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMSyncNodeJobManager));

        private static readonly IRMReportManager s_reportManager = ReportMangerFactory.Instance.ReportManager;

        private static readonly Dictionary<RMSyncNodeAction, string> s_actionI18Ns = new()
        {
            { RMSyncNodeAction.Add, "RM_JS_SRN_Action_Add" },
            { RMSyncNodeAction.Update, "RM_JS_SRN_Action_Update" },
            { RMSyncNodeAction.Delete, "RM_JS_SRN_Action_Delete" },
            { RMSyncNodeAction.Upgrade, "RM_JS_SRN_Action_Upgrade" },
            { RMSyncNodeAction.None, "" }
        };
        
        private static string s_commentI18Ns(Exception ex) => ex switch 
        {
            RMRetryException retryEx => "RM_JS_SRN_Exception_RetrieveAosNode",
            _ => ex.Message
        };

        private static readonly Dictionary<SourceFlag, string> s_contentSourceI18Ns = new()
        {
            { SourceFlag.SharePoint, "RM_JS_Common_ReportType_SharePoint" },
            { SourceFlag.Exchange, "RM_JS_Common_ReportType_Exchange" },
            { SourceFlag.OneDrive, "RM_JS_Common_ReportType_OneDrive" },
            { SourceFlag.Google, "RM_JS_Common_ReportType_GoogleDrive" },
            { SourceFlag.Teams, "RM_JS_Common_ReportType_Teams" },
        };

        private static int SucceedCount = 0;

        private static int FailedCount = 0;

        public static HashSet<RMSiteNodeAdaption> CacheTeamsNodes = new HashSet<RMSiteNodeAdaption>();
        public static void Init(string jobId)
        {
            ReportMangerFactory.Instance.Init(jobId, AvePoint.RA.Contract.JobMonitor.JobType.SyncNodesFromAOS);
            s_reportManager.StartUpdateJobProgress(60);
            s_reportManager.IncreaseBase(1000000);
            s_reportManager.Increase(1000);

            _ = AutoUpdateProcess();
        }

        private static async Task AutoUpdateProcess()
        {
            while(true)
            {
                s_reportManager.Increase(1);
                await Task.Delay(1000 * 60);
            }
        }

        private static async Task<bool> IsTeamsAvailableAsync()
        {
            const string enableTeamsFeatureKey = "EnableTeamsFeature";
            const string hasUpgradeTeamsKey = "HasUpgradeTeams";
            using var context = RMDBContextManager.GetNewDBContext();
            var keys = new[] { enableTeamsFeatureKey, hasUpgradeTeamsKey };
            var kvs = await context.RMKeyValue.Where(k => keys.Contains(k.Key)).ToListAsync();
            var enableKv = kvs.FirstOrDefault(k => k.Key == enableTeamsFeatureKey);
            if (enableKv != null)
            {
                if (!bool.TryParse(enableKv.Value, out var enableParsed) || !enableParsed)
                {
                    return false;
                }
            }

            var upgradeKv = kvs.FirstOrDefault(k => k.Key == hasUpgradeTeamsKey);
            if (upgradeKv != null && bool.TryParse(upgradeKv.Value, out var upgraded) && upgraded)
            {
                return true;
            }
            return false;
        }

        public static async Task AddSucceedJobDetail(SourceFlag contentSource, RMSyncNodeAction action, IEnumerable<RMContainerInfoAdaption> containerInfoes)
        {
            bool isEnableTeams = await IsTeamsAvailableAsync();

            if (isEnableTeams && contentSource == SourceFlag.Teams)
            {
                containerInfoes = containerInfoes.Where(item => item.Name != "Default Private Channel Sites Container");
                if (!containerInfoes.Any())
                {
                    return;
                }
            }

            var contentSourceI18n = s_contentSourceI18Ns[contentSource];
            var actionI18n = s_actionI18Ns[action];
            var jobDetails = containerInfoes.ConvertAll(item => new JMSyncRemoteNodesJobDetails
            {
                Container = RMSyncNodeConverter.ContainerNameConvertToJobDetail(item.Name),
                ItemType = contentSourceI18n,
                Action = actionI18n,
                Status = JobDetailsStatus.Successful,
            });

            s_reportManager.BatchSendJobDetail(jobDetails);
            s_reportManager.Increase(jobDetails.Count());
            Interlocked.Add(ref SucceedCount, jobDetails.Count());
        }

        private static string GetContainerName(SourceFlag contentSource, RMContainerInfoAdaption containerInfo, RMSiteNodeAdaption node)
        {
            if (contentSource == SourceFlag.Teams && node.SiteCollectionType == SiteCollectionType.PrivateChannel)
            {
                var cNode = CacheTeamsNodes.FirstOrDefault(cNode => cNode.TeamId == node.TeamId && cNode.NodeLevel == NodeLevel.O365GroupSites);
                if (cNode != null)
                {
                    return cNode.ContainerName;
                }
            }
            return RMSyncNodeConverter.ContainerNameConvertToJobDetail(containerInfo.Name);
        }
        public static void AddSucceedJobDetail(SourceFlag contentSource, RMSyncNodeAction action, RMContainerInfoAdaption containerInfo, IEnumerable<RMSiteNodeAdaption> nodes)
        {
            var contentSourceI18n = s_contentSourceI18Ns[contentSource];
            var actionI18n = s_actionI18Ns[action];
            var jobDetails = nodes.ConvertAll(item => new JMSyncRemoteNodesJobDetails
            {
                Container = GetContainerName(contentSource, containerInfo, item),
                ObjectName = item.Url,
                ItemType = contentSourceI18n,
                Action = actionI18n,
                Status = JobDetailsStatus.Successful,
            });

            s_reportManager.BatchSendJobDetail(jobDetails);
            s_reportManager.Increase(jobDetails.Count());
            Interlocked.Add(ref SucceedCount, jobDetails.Count());
        }
        
        public static void AddSucceedJobDetail(SourceFlag contentSource, RMSyncNodeAction action, RMContainerInfoAdaption containerInfo, IEnumerable<RMGoogleNodeAdaption> nodes)
        {
            var contentSourceI18n = s_contentSourceI18Ns[contentSource];
            var actionI18n = s_actionI18Ns[action];
            var jobDetails = nodes.ConvertAll(item => new JMSyncRemoteNodesJobDetails
            {
                Container = RMSyncNodeConverter.ContainerNameConvertToJobDetail(containerInfo.Name),
                ObjectName = item.Name,
                ItemType = contentSourceI18n,
                Action = actionI18n,
                Status = JobDetailsStatus.Successful,
            });

            s_reportManager.BatchSendJobDetail(jobDetails);
            s_reportManager.Increase(jobDetails.Count());
            Interlocked.Add(ref SucceedCount, jobDetails.Count());
        }

        public static void AddSucceedJobDetail(SourceFlag contentSource, RMSyncNodeAction action, RMContainerInfoAdaption containerInfo, IEnumerable<RMSiteNodeAdaption> nodes, string comment)
        {
            var contentSourceI18n = s_contentSourceI18Ns[contentSource];
            var actionI18n = s_actionI18Ns[action];
            var jobDetails = nodes.ConvertAll(item => new JMSyncRemoteNodesJobDetails
            {
                Container = RMSyncNodeConverter.ContainerNameConvertToJobDetail(containerInfo.Name),
                ObjectName = item.Url,
                ItemType = contentSourceI18n,
                Action = actionI18n,
                Status = JobDetailsStatus.Successful,
                Comment = comment,
            });

            s_reportManager.BatchSendJobDetail(jobDetails);
            s_reportManager.Increase(jobDetails.Count());
            Interlocked.Add(ref SucceedCount, jobDetails.Count());
        }

        public static void AddSucceedJobDetail(SourceFlag contentSource, RMSyncNodeAction action, RMContainerInfoAdaption containerInfo, IEnumerable<RMExchangeNodeAdaption> nodes)
        {
            var contentSourceI18n = s_contentSourceI18Ns[contentSource];
            var actionI18n = s_actionI18Ns[action];
            var jobDetails = nodes.ConvertAll(item => new JMSyncRemoteNodesJobDetails
            {
                Container = RMSyncNodeConverter.ContainerNameConvertToJobDetail(containerInfo.Name),
                ObjectName = item.EmailAddress,
                ItemType = contentSourceI18n,
                Action = actionI18n,
                Status = JobDetailsStatus.Successful,
            });

            s_reportManager.BatchSendJobDetail(jobDetails);
            s_reportManager.Increase(jobDetails.Count());
            Interlocked.Add(ref SucceedCount, jobDetails.Count());
        }

        public static void AddSkippedJobDetail(SourceFlag contentSource, RMSyncNodeAction action, RMContainerInfoAdaption containerInfo, IEnumerable<RMSiteNodeAdaption> nodes, string comment)
        {
            var contentSourceI18n = s_contentSourceI18Ns[contentSource];
            var actionI18n = s_actionI18Ns[action];
            var jobDetails = nodes.ConvertAll(item => new JMSyncRemoteNodesJobDetails
            {
                Container = RMSyncNodeConverter.ContainerNameConvertToJobDetail(containerInfo.Name),
                ObjectName = item.Url,
                ItemType = contentSourceI18n,
                Action = actionI18n,
                Status = JobDetailsStatus.Skipped,
                Comment = comment,
            });

            s_reportManager.BatchSendJobDetail(jobDetails);
            s_reportManager.Increase(jobDetails.Count());
            Interlocked.Add(ref SucceedCount, jobDetails.Count());
        }


        public static void AddFailedJobDetail(SourceFlag contentSource, RMSyncNodeAction action, IEnumerable<RMContainerInfoAdaption> containerInfoes, Exception ex)
        {
            var contentSourceI18n = s_contentSourceI18Ns[contentSource];
            var actionI18n = s_actionI18Ns[action];
            var jobDetails = containerInfoes.ConvertAll(item => new JMSyncRemoteNodesJobDetails
            {
                Container = RMSyncNodeConverter.ContainerNameConvertToJobDetail(item.Name),
                ItemType = contentSourceI18n,
                Action = actionI18n,
                Status = JobDetailsStatus.Failed,
                Comment = s_commentI18Ns(ex),
            });

            s_reportManager.BatchSendJobDetail(jobDetails);
            s_reportManager.Increase(jobDetails.Count());
            Interlocked.Add(ref FailedCount, jobDetails.Count());
        }

        public static void AddFailedJobDetail(SourceFlag contentSource, RMSyncNodeAction action, RMContainerInfoAdaption containerInfo, IEnumerable<RMSiteNodeAdaption> nodes)
        {
            var contentSourceI18n = s_contentSourceI18Ns[contentSource];
            var actionI18n = s_actionI18Ns[action];
            var jobDetails = nodes.ConvertAll(item => new JMSyncRemoteNodesJobDetails
            {
                Container = RMSyncNodeConverter.ContainerNameConvertToJobDetail(containerInfo.Name),
                ObjectName = item.Url,
                ItemType = contentSourceI18n,
                Action = actionI18n,
                Status = JobDetailsStatus.Failed,
            });

            s_reportManager.BatchSendJobDetail(jobDetails);
            s_reportManager.Increase(jobDetails.Count());
            Interlocked.Add(ref FailedCount, jobDetails.Count());
        }

        public static void AddFailedJobDetail(SourceFlag contentSource, RMSyncNodeAction action, RMContainerInfoAdaption containerInfo, IEnumerable<RMExchangeNodeAdaption> nodes)
        {
            var contentSourceI18n = s_contentSourceI18Ns[contentSource];
            var actionI18n = s_actionI18Ns[action];
            var jobDetails = nodes.ConvertAll(item => new JMSyncRemoteNodesJobDetails
            {
                Container = RMSyncNodeConverter.ContainerNameConvertToJobDetail(containerInfo.Name),
                ObjectName = item.EmailAddress,
                ItemType = contentSourceI18n,
                Action = actionI18n,
                Status = JobDetailsStatus.Failed,
            });

            s_reportManager.BatchSendJobDetail(jobDetails);
            s_reportManager.Increase(jobDetails.Count());
            Interlocked.Add(ref FailedCount, jobDetails.Count());
        }

        public static void SetJobFinished()
        {
            var jobFinishStatus = SucceedCount > 0 && FailedCount > 0 ?
                JobStatus.FinishWithException :
                (
                    FailedCount > 0 ?
                    JobStatus.Failed :
                    JobStatus.Finished
                );
            s_reportManager.SetJobFinished(jobFinishStatus);

            s_logger.Debug($"Succeed item count: [{SucceedCount}]. Failed item count: [{FailedCount}].");
        }

        public static void SetJobFailed(string comment)
        {
            s_reportManager.SetJobFinished(JobStatus.Failed, comment);
        }
    }

    public enum RMSyncNodeAction
    {
        Add = 0,
        Update = 1,
        Delete = 2,
        Upgrade = 3,
        None = 4
    }
}
