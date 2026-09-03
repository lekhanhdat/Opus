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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.GoogleSyncNodeDao.Contract;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RADashboard.Collectors
{
    public class GControlDashboardCollector : DashboardCollector
    {
        public override SourceFlag Flag => SourceFlag.GGControl;
        public SourceFlag SourceFlagGoogle = SourceFlag.Google;

        private RALogger _logger = RALogger.GetInstance(typeof(GControlDashboardCollector));

        private IRMGoogleRemoteNodeDao _googleRemoteNodeDao => PlatformWindsorManager.GetService<IRMGoogleRemoteNodeDao>();

        private static readonly IDashboardUserWaitingApprovalCountDao _dashboardUserWaitingApprovalCountDao = PlatformWindsorManager.GetService<IDashboardUserWaitingApprovalCountDao>();

        private readonly long unixEpochTicks = 621355968000000000;

        protected override Dictionary<CollectorEventType, Func<Task>> SpecialActionForCollectorEventTypes => new()
        {
            {CollectorEventType.UserWaitingApprovalCount,  BasicCollectUserWaitingApprovalCountForGControlAsync}
        };

        private async Task BasicCollectUserWaitingApprovalCountForGControlAsync()
        {
            await _dashboardUserWaitingApprovalCountDao.RemoveAllAsync(SourceFlag.GGControl);
            Logger.Info($"Successful remove all user waiting approval count data by [{Flag}]");

            var sql = $@"
SELECT c.gControlCurrentApproverId AS reviewers, COUNT(1) as count FROM c WHERE 
c.manual_isManualSynced 
AND c.sourceFlag = {(int)SourceFlag.Google}
AND c.gControlCurrentStatus = 1
AND c.manual_extendTime < {DateTime.UtcNow.Ticks}
AND c.manual_archiveStatus != {(int)ActionStatus.Archiverd}
AND c.recordStatus != {(int)RMRecordStatus.Hidden}
AND c.recordStatus != {(int)RMRecordStatus.RMDeleted}
GROUP BY c.gControlCurrentApproverId";

            var sqlGControlReviwer = $@"
SELECT c.gControlManualReviewers AS reviewers, COUNT(1) as count FROM c WHERE 
c.manual_isManualSynced 
AND c.sourceFlag = {(int)SourceFlagGoogle}
AND c.gControlCurrentStatus = 1
AND c.manual_extendTime < {DateTime.UtcNow.Ticks}
AND c.manual_archiveStatus != {(int)ActionStatus.Archiverd}
AND c.recordStatus != {(int)RMRecordStatus.Hidden}
AND c.recordStatus != {(int)RMRecordStatus.RMDeleted}
GROUP BY c.gControlManualReviewers";


            var gControlApprovals = ExplorerDao.QueryReviewerWaitingApprovalItemCountForGControl(sql);

            var gcontrolReviwers = ExplorerDao.QueryReviewerWaitingApprovalItemCount(sqlGControlReviwer);

            var reviewersWaitingApprovalCountForGControl = new Dictionary<int, int>();

            foreach (var (reviewers, Count) in gcontrolReviwers)
            {
                if (reviewers.IsNullOrEmpty()) continue;

                foreach (var reviewer in reviewers)
                {
                    if (!reviewersWaitingApprovalCountForGControl.ContainsKey(reviewer))
                    {
                        reviewersWaitingApprovalCountForGControl.Add(reviewer, 0);
                    }
                    reviewersWaitingApprovalCountForGControl[reviewer] += Count;

                }
            }

            var allApprovalId = gControlApprovals.Select(item => item.GControlCurrentApproverId).ToList();

            var allApprovalAccounts = _dashboardUserWaitingApprovalCountDao.GetAccountInfosByUserIds(allApprovalId);

            foreach (var (approvalId, count) in gControlApprovals)
            {
                if (approvalId.IsNullOrEmpty() || approvalId == Guid.Empty.ToString()) continue;

                var account = allApprovalAccounts.FirstOrDefault(item => item.AADId == approvalId);

                if (account == null)
                {
                    _logger.Warn($"User {approvalId} removed in database");
                    continue;
                }

                if (!reviewersWaitingApprovalCountForGControl.ContainsKey(account.Id))
                {
                    reviewersWaitingApprovalCountForGControl.Add(account.Id, 0);
                }
                reviewersWaitingApprovalCountForGControl[account.Id] += count;
            }

            var top10OwnersWaitingApprovalsForGControl = reviewersWaitingApprovalCountForGControl.OrderByDescending(item => item.Value).ToDictionary(item => item.Key, item => item.Value);
            var top10OwnersWaitingApprovalIdsForGControl = top10OwnersWaitingApprovalsForGControl.Select(item => item.Key);
            var accountInfoForGControl = _dashboardUserWaitingApprovalCountDao.GetAccountInfosByOnwerIds(top10OwnersWaitingApprovalIdsForGControl);
            var resultGControl = accountInfoForGControl.ConvertAll(item => new RMDashboardUserWaitingApprovalCount
            {
                Id = Guid.NewGuid().ToString(),
                SourceFlag = (int)Flag,
                DisplayName = item.DisplayName,
                UserPrincipalName = item.UserPrincipalName,
                Count = top10OwnersWaitingApprovalsForGControl[item.Id]
            });
            resultGControl = resultGControl.OrderByDescending(item => item.Count).Take(10).ToList();

            _dashboardUserWaitingApprovalCountDao.BatchCreate(resultGControl);
        }


        protected override Dictionary<DataUsageStatus, string> CollectCosmosDBDataUsageOfDateSql(long startTicks)
        {
            var activeSql = $@"
            SELECT LEFT(TicksToDateTime(c.timeCreated - {unixEpochTicks}), 10) AS date , COUNT(1) AS count FROM c WHERE
            c.sourceFlag = {(int)SourceFlagGoogle}
            And c.nodeType = {(int)RMNodeLevel.GoogleFile}
            AND c.timeCreated >= {startTicks}
            GROUP BY LEFT(TicksToDateTime(c.timeCreated - {unixEpochTicks}), 10)
            ";

            var destroyedSql = $@"
            SELECT LEFT(TicksToDateTime(c.destroyedTime - 621355968000000000), 10) AS date , COUNT(1) AS count FROM c WHERE
            c.sourceFlag = {(int)SourceFlagGoogle}
            AND c.recordStatus = {(int)RMRecordStatus.Destroyed}
            AND c.destroyedTime >= {startTicks}
            AND ARRAY_CONTAINS([{(int)RMNodeLevel.GoogleFile}], c.nodeType)
            GROUP BY LEFT(TicksToDateTime(c.destroyedTime - 621355968000000000), 10)
            ";

            var waitingSql = $@"
            SELECT LEFT(TicksToDateTime(c.manual_collectionTime - 621355968000000000), 10) AS date , COUNT(1) AS count FROM c WHERE
            c.manual_isManualSynced
            AND c.sourceFlag = {(int)SourceFlag.Google}
            AND c.isGControlRecord = true
            AND c.manual_collectionTime >= 637865381843477781
            GROUP BY LEFT(TicksToDateTime(c.manual_collectionTime - 621355968000000000), 10)
            ";

            return new Dictionary<DataUsageStatus, string>
            {
                { DataUsageStatus.Active, activeSql },
                { DataUsageStatus.Destroyed, destroyedSql },
                { DataUsageStatus.WaitingForApproval, waitingSql }
            };
        }

        protected override async Task<List<RMDashboardDataUsage>> CollectDataUsageAsync()
        {
            try
            {
                var result = new List<RMDashboardDataUsage>();

                var drives = await _googleRemoteNodeDao.GetAllGoogleRemoteNodes();

                foreach (var drive in drives)
                {
                    var activeRecords = GetRecordsByContainerId(drive.Id, RMRecordStatus.Active);
                    var activeRecordsCount = activeRecords.Count;

                    var destroyedRecords = GetRecordsByContainerId(drive.Id, RMRecordStatus.Destroyed);
                    var destroyedRecordsCount = destroyedRecords.Count;

                    var archivedRecords = GetRecordsByContainerId(drive.Id, RMRecordStatus.Archived);
                    var archivedRecordsCount = archivedRecords.Count;

                    var dataUsage = new RMDashboardDataUsage
                    {
                        Id = Guid.NewGuid().ToString(),
                        SourceFlag = (int)Flag,
                        ContainerId = drive.ParentId,
                        ScopeId = drive.Id,
                        Title = drive.Name,
                        Path = drive.Name,
                        Active = activeRecordsCount,
                        Destroyed = destroyedRecordsCount,
                        Archived = archivedRecordsCount, // Google Drive does not have an archived state
                    };
                    result.Add(dataUsage);
                }

                return result;
            }
            catch (Exception e)
            {
                throw;
            }
        }

        protected override Dictionary<string, int> CollectTermUsage()
        {
            var sql = $@"SELECT c.termId, COUNT(1) AS termcount FROM items c
            where c.sourceFlag = {(int)SourceFlagGoogle} and c.recordStatus = {(int)RMRecordStatus.Active} 
            and c.termId != {Guid.Empty} and c.nodeType = {(int)RMNodeLevel.GoogleFile} GROUP BY c.termId";
            return ExplorerDao.QueryRelatedTermCount(sql);
        }
        private List<Record> GetRecordsByContainerId(string nodeId, RMRecordStatus status)
        {
            return ExplorerDao.QueryAll(record =>
             record.RecordStatus == (int)status
             && record.NodeType == (int)RMNodeLevel.GoogleFile
             && record.ScopeId == new Guid(nodeId)).ToList();
        }
    }
}
