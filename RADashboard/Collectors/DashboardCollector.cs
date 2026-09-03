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
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RADashboard.Collectors
{

    public enum DataUsageStatus
    {
        Active = 0,
        Destroyed = 1,
        WaitingForApproval = 2,
    }

    public abstract class DashboardCollector
    {
        protected static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly IDashboardDataUsageDao DashboardDataUsageDao = PlatformWindsorManager.GetService<IDashboardDataUsageDao>();

        private static readonly IDashboardTermUsageDao DashboardTermUsageDao = PlatformWindsorManager.GetService<IDashboardTermUsageDao>();

        private static readonly IDashboardUserWaitingApprovalCountDao DashboardUserWaitingApprovalCountDao = PlatformWindsorManager.GetService<IDashboardUserWaitingApprovalCountDao>();

        private static readonly IDashboardDataUsageOfDateDao DashboardDataUsageOfDateDao = PlatformWindsorManager.GetService<IDashboardDataUsageOfDateDao>();

        protected static readonly IExplorerDao ExplorerDao = new ExplorerDao(true);

        public abstract SourceFlag Flag { get; }

        protected abstract Task<List<RMDashboardDataUsage>> CollectDataUsageAsync();

        protected abstract Dictionary<string, int> CollectTermUsage();

        protected abstract Dictionary<DataUsageStatus, string> CollectCosmosDBDataUsageOfDateSql(long startTicks);

        protected virtual Dictionary<CollectorEventType, Func<Task>> SpecialActionForCollectorEventTypes => [];

        private Dictionary<CollectorEventType, Func<Task>> EventActions => new ()
        {
            { CollectorEventType.DataUsage, BasicCollectDataUsageAsync },
            { CollectorEventType.TermUsage,  BasicCollectTermUsageAsync },
            { CollectorEventType.UserWaitingApprovalCount,  BasicCollectUserWaitingApprovalCountAsync },
            { CollectorEventType.DataUsageOfDate,  BasicCollectDataUsageOfDateAsync },
        };

        public async Task CollectAsync()
        {
            Logger.Info($"Start collect {Flag} dashboard data.");

            foreach (var eventAction in EventActions)
            {
                try
                {
                    Logger.Info($"Start collect [{Flag}-{eventAction.Key}].");
                    if (SpecialActionForCollectorEventTypes.TryGetValue(eventAction.Key, out var specialFunc))
                    {
                        await CollectDataPerformanceRecordAsync(specialFunc, $"{Flag}-{eventAction.Key}");
                        Logger.Info($"Successful collect [{Flag}-{eventAction.Key}].");
                        continue;
                    }
                    await CollectDataPerformanceRecordAsync(eventAction.Value, $"{Flag}-{eventAction.Key}");
                    Logger.Info($"Successful collect [{Flag}-{eventAction.Key}].");
                }
                catch (Exception e)
                {
                    Logger.Error($"An error occurred while collect [{Flag}-{eventAction.Key}]. Error: {e}");
                    DashboardCollectorJobManager.AddFailedJobDetail(Flag, eventAction.Key, e.Message);
                }
            }

            Logger.Info($"Successfule collect {Flag} dashboard data.");
        }

        private async Task BasicCollectDataUsageOfDateAsync()
        {

            var result = new Dictionary<string, RMDashboardDataUsageOfDate>();

            var yearAgo = DateTime.UtcNow.AddYears(-1);
            var startDateTicks = new DateTime(yearAgo.Year, yearAgo.Month, yearAgo.Day).Ticks;

            foreach (var item in CollectCosmosDBDataUsageOfDateSql(startDateTicks))
            {
                var dateUsageOfDateList = ExplorerDao.QueryDashboardDataUsageOfDate(item.Value);
                foreach(var (date, count) in dateUsageOfDateList)
                {
                    if (!result.ContainsKey(date))
                    {
                        result.Add(date, new RMDashboardDataUsageOfDate
                        {
                            Id = Guid.NewGuid().ToString(),
                            SourceFlag = (int)Flag,
                            Created = 0,
                            Destroyed = 0,
                            WaitingApproved = 0,
                            Date = Convert.ToDateTime(date).Ticks
                        });
                    }

                    switch(item.Key) 
                    {
                        case DataUsageStatus.Active:
                            result[date].Created += count;
                            break;
                        case DataUsageStatus.Destroyed:
                            result[date].Destroyed += count;
                            break;
                        case DataUsageStatus.WaitingForApproval:
                            result[date].WaitingApproved += count;
                            break;
                    }
                }
            }

            await DashboardDataUsageOfDateDao.RemoveAllAsync(Flag);
            Logger.Info($"Successful reomove all data usage of date by [{Flag}].");

            DashboardDataUsageOfDateDao.BatchCreate(result.Values.ToList());
        }

        private async Task BasicCollectUserWaitingApprovalCountAsync()
        {
            await DashboardUserWaitingApprovalCountDao.RemoveAllAsync(Flag);
            Logger.Info($"Successful remove all user waiting approval count data by [{Flag}]");
            var sql = $@"
SELECT c.manual_reviewer_Array AS reviewers, COUNT(1) as count FROM c WHERE 
c.manual_isManualSynced 
AND c.sourceFlag = {(int)Flag}
AND c.manual_approvedStatus = 1
AND c.manual_extendTime < {DateTime.UtcNow.Ticks}
AND c.manual_archiveStatus != {(int)ActionStatus.Archiverd}
AND c.recordStatus != {(int)RMRecordStatus.Hidden}
AND c.recordStatus != {(int)RMRecordStatus.RMDeleted}
GROUP BY c.manual_reviewer_Array";

            var res = ExplorerDao.QueryReviewerWaitingApprovalItemCount(sql);
            var reviewersWaitingApprovalCount = new Dictionary<int, int>();

            foreach (var (Reviewers, Count) in res)
            {
                foreach (var reviewId in Reviewers)
                {
                    if (!reviewersWaitingApprovalCount.ContainsKey(reviewId))
                    {
                        reviewersWaitingApprovalCount.Add(reviewId, 0);
                    }

                    reviewersWaitingApprovalCount[reviewId] += Count;
                }
            }

            var top10OwnersWaitingApprovals = reviewersWaitingApprovalCount.OrderByDescending(item => item.Value).ToDictionary(item => item.Key, item => item.Value);
            var top10OwnersWaitingApprovalIds = top10OwnersWaitingApprovals.Select(item => item.Key);

            var accountInfos = DashboardUserWaitingApprovalCountDao.GetAccountInfosByOnwerIds(top10OwnersWaitingApprovalIds);
            var result = accountInfos.ConvertAll(item => new RMDashboardUserWaitingApprovalCount
            {
                Id = Guid.NewGuid().ToString(),
                SourceFlag = (int)Flag,
                DisplayName = item.DisplayName,
                UserPrincipalName = item.UserPrincipalName,
                Count = top10OwnersWaitingApprovals[item.Id]
            });
            result = result.OrderByDescending(item => item.Count).Take(10).ToList();

            DashboardUserWaitingApprovalCountDao.BatchCreate(result);
        }


        private async Task BasicCollectDataUsageAsync()
        {
            await DashboardDataUsageDao.RemoveAllBySourceFlagAsync(Flag);
            Logger.Info($"Successful remove all data usage by [{Flag}]");

            var datas = CollectDataUsageAsync().Result;

            DashboardDataUsageDao.BatchCreate(datas);

            Logger.Info($"Successful collect data usage by [{Flag}].");
        }

        private async Task BasicCollectTermUsageAsync()
        {

            bool GetTermFullPath(RMDashboardTermInfo termInfo, Dictionary<int, RMDashboardTermInfo> termInfos, out string fullPath)
            {
                var termPath = termInfo.TermPath;
                var termPathIds = termPath.Substring(termPath.IndexOf('/') + 1).Split('/').ToList();
                var termPathNames = new List<string>();
                fullPath = "";

                foreach (var termPathId in termPathIds)
                {
                    if (!termInfos.TryGetValue(Convert.ToInt32(termPathId), out var term))
                    {
                        return false;
                    }
                    termPathNames.Add(term.TermName);
                }
                fullPath = $"{termInfo.TermGroupName}/{termInfo.TermSetName}/{string.Join("/", termPathNames)}";
                return true;
            }

            {
                await DashboardTermUsageDao.RemoveAllBySourceFlagAsync(Flag);
                Logger.Info($"Successful remove all term usage by source: [{Flag}]");
                
                var termUsageDatas = CollectTermUsage();
                var termIds = termUsageDatas.Keys.ToHashSet();
                
                var termInfos = DashboardCollectorCache.DashboardTermInfos.Where(item => termIds.Contains(item.TermUniqueId)).ToDictionary(item => item.TermId);
                var termCacheInfos = DashboardCollectorCache.DashboardTermInfos.ToDictionary(item => item.TermId);
                List<RMDashboardTermUsage> datas = [];
                
                foreach (var termInfo in termInfos.Values)
                {
                    if (!GetTermFullPath(termInfo, termCacheInfos, out var fullPath))
                    {
                        continue;
                    }

                    datas.Add(new RMDashboardTermUsage
                    {
                        Id = Guid.NewGuid().ToString(),
                        SourceFlag = (int)Flag,
                        TermId = termInfo.TermUniqueId,
                        TermSetId = termInfo.TermSetId,
                        TermGroupId = termInfo.TermGroupId,
                        TermName = termInfo.TermName,
                        TermFullPath = fullPath,
                        Active = termUsageDatas[termInfo.TermUniqueId]
                    });
                }

                DashboardTermUsageDao.BatchCreate(datas);
            }
        }

        private async Task CollectDataPerformanceRecordAsync(Func<Task> collectDataAction, string performanceName)
        {
            using (var scope = new PerformanceScope(performanceName))
            {
                await collectDataAction();
            }
        }
    }
}
