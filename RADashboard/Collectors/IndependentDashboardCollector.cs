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
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RADashboard.Collectors
{
    public class IndependentDashboardCollector
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(IndependentDashboardCollector));

        private static readonly IDashboardTermApplyRuleUsageDao DashboardTermApplyRuleUsageDao = PlatformWindsorManager.GetService<IDashboardTermApplyRuleUsageDao>();



        private static readonly Dictionary<CollectorEventType, Action> CollectEvents = new Dictionary<CollectorEventType, Action>
        {
            { CollectorEventType.TermApplyRuleUsage, CollectTermApplyRuleUsage },
            { CollectorEventType.CheckHoldStatus, CheckHoldStatus }
        };

        public static void Collect(bool needCollectTermWithRule)
        {
            foreach (var collectEvent in CollectEvents)
            {
                if(collectEvent.Key == CollectorEventType.TermApplyRuleUsage && !needCollectTermWithRule)
                {
                    Logger.Info($"There is no change about term and rule, no need executing [CollectTermApplyRuleUsage]");
                    continue;
                }
                try
                {
                    Logger.Info($"Start execute {collectEvent.Key} dashboard collect.");

                    CollectDataPerformanceRecord(collectEvent.Value, $"{collectEvent.Key}");

                    Logger.Info($"Successfule execute {collectEvent.Key} dashboard collect.");
                }
                catch (Exception e)
                {
                    Logger.Error($"An error occurred while execute {collectEvent.Key} dashboard collect. Error: {e}");
                    DashboardCollectorJobManager.AddFailedJobDetail(SourceFlag.All, collectEvent.Key, e.Message);
                }
            }
        }

        private static void CollectTermApplyRuleUsage()
        {
            DashboardTermApplyRuleUsageDao.RemoveAll();
            Logger.Info($"Successful remove all term apply rule usage data.");

            var termUsageRuleCount = new Dictionary<string, RMDashboardTermApplyRuleUsage>();
            var termInfos = DashboardCollectorCache.DashboardTermInfos;
            var termApplyRuleIds = termInfos.Where(item => item.IsApplyRule).Select(item => item.TermId).ToHashSet();

            foreach (var termInfo in termInfos)
            {
                if (!termUsageRuleCount.ContainsKey(termInfo.TermSetId))
                {
                    termUsageRuleCount.Add(termInfo.TermSetId, new RMDashboardTermApplyRuleUsage
                    {
                        Id = Guid.NewGuid().ToString(),
                        TermSetId = termInfo.TermSetId,
                        TermGroupId = termInfo.TermGroupId,
                        TermApplyRuleCount = 0,
                        TermNonApplyRuleCount = 0
                    });
                }

                if (termInfo.IsApplyRule)
                {
                    termUsageRuleCount[termInfo.TermSetId].TermApplyRuleCount++;
                    continue;
                }

                if (termInfo.IsBreakInherit)
                {
                    termUsageRuleCount[termInfo.TermSetId].TermNonApplyRuleCount++;
                    continue;
                }

                var count = termInfo.TermPath.Substring(termInfo.TermPath.IndexOf("/") + 1).Split('/')
                    .Count(item => termApplyRuleIds.Contains(Convert.ToInt32(item)));

                if (count > 0)
                {
                    termUsageRuleCount[termInfo.TermSetId].TermApplyRuleCount++;
                    continue;
                }

                termUsageRuleCount[termInfo.TermSetId].TermNonApplyRuleCount++;
            }
            DashboardTermApplyRuleUsageDao.BatchCreate(termUsageRuleCount.Values.ToList());
        }

        private static void CheckHoldStatus()
        {
            var utcNow = DateTime.UtcNow.Ticks;
            Logger.Info("start to update record hold expired, utcNow:{0}.", utcNow);
            var ExplorerDao = new AvePoint.RA.DB.Explorer.Dao.CosmosImp.ExplorerDao(true);
            List<Guid> expiredIds = ExplorerDao.UpdateExpiredHeldRecords();
            Logger.Info("record hold expired success.");
        }

        private static void CollectDataPerformanceRecord(Action collectDataAction, string performanceName)
        {
            using (var scope = new PerformanceScope(performanceName))
            {
                collectDataAction();
            }
        }
    }
}
