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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using RACloudFS.Report;
using RMSynchronize.SyncNodeFromAOS.ChangeLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RMSynchronize.SyncStorage
{
    public class RMSyncStorageJobManager
    {
        private bool HasSucceed { get; set; }

        public bool HasFailed { get; set; }

        private bool s_hasUpgradeTeams;

        private readonly IRMReportManager _reportManager = ReportMangerFactory.Instance.ReportManager;

        private static readonly IRMKeyValueDao s_keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private static readonly IRMRemoteNodeDao s_remoteNodeDao = PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private static Dictionary<string, string> s_teamsContainerNameCache = new Dictionary<string, string>();

        public RMSyncStorageJobManager(string jobId)
        {
            ReportMangerFactory.Instance.Init(jobId, AvePoint.RA.Contract.JobMonitor.JobType.SyncSecurityContainer);
            _reportManager.StartUpdateJobProgress(60);
            Task.Delay(1000 * 3).GetAwaiter().GetResult();
            _reportManager.IncreaseBase(5000);
            _reportManager.Increase(1000);
            s_hasUpgradeTeams = s_keyValueDao.HasUpgradeTeams();

            _ = AutoUpdateProcess();
        }

        private async Task AutoUpdateProcess()
        {
            while (true)
            {
                _reportManager.Increase(1);
                await Task.Delay(1000 * 60);
            }
        }

        public void AddDetail(RMSyncNodeChangeInfo changeInfo, bool isSucceed)
        {
            if(isSucceed)
            {
                AddSucceedDetail(changeInfo);
            }
            else
            {
                AddFailedDetail(changeInfo);
            }
        }

        public void AddSucceedDetail(RMSyncNodeChangeInfo changeInfo)
        {
            if (s_hasUpgradeTeams)
            {
                ChangeContainerName4Channel(changeInfo);
            }

            _reportManager.SendJobDetail(new JMSyncSecurityContainerJobDetails
            {
                ObjectName = changeInfo.IsContainer ? string.Empty : changeInfo.Url,
                Container = changeInfo.IsContainer ? changeInfo.Url : changeInfo.ContainerName,
                Status = JobDetailsStatus.Successful
            });
            HasSucceed = true;
        }

        private static void ChangeContainerName4Channel(RMSyncNodeChangeInfo changeInfo)
        {
            if (changeInfo.IsContainer)
            {
                return;
            }
            if (changeInfo.NodeLevel == NodeLevel.O365GroupSites)
            {
                s_teamsContainerNameCache.TryAdd(changeInfo.RealId.ToString(), changeInfo.ContainerName);
            }


            if (new List<NodeLevel> { NodeLevel.PrivateChannel, NodeLevel.SharedChannel }.Contains(changeInfo.NodeLevel))
            {
                if (s_teamsContainerNameCache.TryGetValue(changeInfo.RealId.ToString(), out var containerName))
                {
                    changeInfo.ContainerName = containerName;
                    return;
                }
                var (teamsNode, _) = s_remoteNodeDao.GetTeamsGroupAndChannelsCollectionByTeamsId(changeInfo.RealId.ToString());
                if (teamsNode != null)
                {
                    var containerNode = s_remoteNodeDao.GetWebApplicationById(teamsNode.parentId);
                    changeInfo.ContainerName = containerNode?.url;
                }
            }
        }

        public void AddFailedDetail(RMSyncNodeChangeInfo changeInfo, string comment = "")
        {
            if (s_hasUpgradeTeams)
            {
                ChangeContainerName4Channel(changeInfo);
            }
            _reportManager.SendJobDetail(new JMSyncSecurityContainerJobDetails
            {
                ObjectName = changeInfo.IsContainer ? string.Empty : changeInfo.Url,
                Container = changeInfo.IsContainer ? changeInfo.Url : changeInfo.ContainerName,
                Status = JobDetailsStatus.Failed,
                Comment = comment,
            });
            HasFailed = true;
        }

        public void SetJobFinished()
        {
            var jobFinishStatus = HasSucceed && HasFailed ?
                JobStatus.FinishWithException :
                (
                    HasFailed ?
                    JobStatus.Failed :
                    JobStatus.Finished
                );
            _reportManager.SetJobFinished(jobFinishStatus);
        }

        public void SetJobFailed(string comment)
        {
            _reportManager.SetJobFinished(JobStatus.Failed, comment);
        }
    }
}
