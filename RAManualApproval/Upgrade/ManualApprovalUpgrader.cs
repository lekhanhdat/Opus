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
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApproval.Upgrade
{
    public class ManualApprovalUpgrader
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ManualApprovalUpgrader));

        private static readonly ITenantInfoDao TenantInfoDao = PlatformWindsorManager.GetService<ITenantInfoDao>();

        private static readonly IRMManualApproveDao ManualApproveDao = PlatformWindsorManager.GetService<IRMManualApproveDao>();

        private static readonly IWorkflowInstanceDao WorkflowInstanceDao = PlatformWindsorManager.GetService<IWorkflowInstanceDao>();

        private static readonly IAccountDao AccountDao = PlatformWindsorManager.GetService<IAccountDao>();

        private static readonly ConcurrentQueue<List<RMManualApprove>> DataQueue = new ConcurrentQueue<List<RMManualApprove>>();

        private static readonly int ConcurrentTaskCount = 5;

        private static bool ProduceComplete = false;

        private static readonly int BatchUpdateDataLimit = 50;

        private static readonly string TenantId = TenantLocalValue.LogonGroupId;

        public static async Task ExecuteAsync()
        {
            try
            {

                var needUpgrade = TenantInfoDao.NeedUpgradeManualData(TenantId);
                Logger.Info($"Current tenant need upgrade manual data: [{needUpgrade}].");
                if (!needUpgrade)
                {
                    return;
                }

                var consumeTask = StartConsumeAsync();

                var datas = ManualApproveDao.GetManualDatas();
                foreach (var data in datas)
                {
                    Logger.Info($"The data count: [{data.Count}]");
                    await BatchUpgradeManualDataAsync(data);
                }

                Logger.Info($"The data produce complete.");
                ProduceComplete = true;

                await consumeTask;
                TenantInfoDao.UpdateManualDataUpgradeStatusToSuccessful(TenantLocalValue.LogonGroupId);
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while execute manual approval data upgrade. Error: {e}");
            }
        }

        private static async Task BatchUpgradeManualDataAsync(List<RMManualApprove> datas)
        {
            var count = 0;
            var pageIndex = 0;
            do
            {
                var pageDatas = datas.OrderBy(item => item.Id).Skip(BatchUpdateDataLimit * pageIndex++).Take(BatchUpdateDataLimit).ToList();
                foreach(var data in pageDatas)
                {
                    try
                    {
                        using (new PerformanceScope($"Update manual data escalate to value.", "", true))
                        {
                            data.EscalateTo = await GetEscalateValueAsync(data);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"An error occurred while upgrade manual data: [{data.Id}]. Error: {e}");
                    }
                }

                count = pageDatas.Count();
                if(pageDatas.Any())
                {
                    DataQueue.Enqueue(pageDatas);
                }
            } while (BatchUpdateDataLimit == count);
        }

        private static async Task<string> GetEscalateValueAsync(RMManualApprove data)
        {
            if (data.WorkflowInstanceId == Guid.Empty)
            {
                return "|" + data.EscalateTo;
            }

            var userIds = WorkflowInstanceDao.GetReviewUserIdsByManualInfo(data);
            var wfReviewIds = (await AccountDao.GetUserByUserIdsAsync(userIds)).Select(item => item.Id);
            if (string.IsNullOrEmpty(data.EscalateTo))
            {
                data.EscalateTo = "";
            }
            var escalateToIds = data.EscalateTo.Split(new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries).Select(item => int.Parse(item));
            var ids = wfReviewIds.Union(escalateToIds).ToHashSet();
            if(!ids.Any())
            {
                return "";
            }
            return "|" + string.Join("|", ids) + "|";
        }

        private static Task StartConsumeAsync()
        {
            var tasks = new List<Task>();
            for(var i = 0; i < ConcurrentTaskCount; i++)
            {
                var task = Task.Run(ConsumeData);
                tasks.Add(task);
            }
            return Task.WhenAll(tasks);
        }

        private static void ConsumeData()
        {
            Logger.Info($"The task: [{System.Threading.Thread.CurrentThread.ManagedThreadId}] has been start.");
            TenantUtil.RunUnderTenant(TenantId, null, () =>
            {
                while (!ProduceComplete || DataQueue.Any())
                {
                    try
                    {
                        if (!DataQueue.TryDequeue(out var datas))
                        {
                            continue;
                        }

                        Logger.Info($"The task: [{System.Threading.Thread.CurrentThread.ManagedThreadId}] consume data.");
                        using(new PerformanceScope($"Batch update manual data escalate to column value."))
                        {
                            ManualApproveDao.BatchUpdate(datas, item => item.EscalateTo);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"An error occurred while batch update manual data. Error: {e}");
                    }
                    finally
                    {
                        Task.Delay(500).Wait();
                    }
                }
            });
            Logger.Info($"The task: [{System.Threading.Thread.CurrentThread.ManagedThreadId}] has been exist.");
        }
    }
}
