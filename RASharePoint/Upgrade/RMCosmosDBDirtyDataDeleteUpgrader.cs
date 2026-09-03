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
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Bulk;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Upgrade
{
    public class RMCosmosDBDirtyDataDeleteUpgrader
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMCosmosDBDirtyDataDeleteUpgrader));

        private const string S_DIRTY_DATA_KEY = "COSMOS_DIRTY_DATA_WILL_DELETE_DEFINITION";

        private static readonly IRMReportManager s_reportManager = ReportMangerFactory.Instance.ReportManager;

        private static readonly IExplorerDao s_explorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao(true);

        private static readonly IRMKeyValueDao s_keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private static readonly ICosmosBulkOperator s_cosmosOperator;

        private static readonly RMRetryer s_retryer;

        private static int SucceedCount { get; set; }

        private static int FailedCount { get; set; }

        static RMCosmosDBDirtyDataDeleteUpgrader()
        {
            s_cosmosOperator = CosmosBulkOperator.Instance;
            s_cosmosOperator.Start(5, SucceedProcessRecord, FailedProcessRecord);
            s_retryer = RMRetryerBuilder.CreateBuilder().Build();
        }

        public static void Process(string jobId)
        {
            try
            {
                ReportMangerFactory.Instance.Init(jobId, Contract.JobMonitor.JobType.CosmosDBDirtyDataDeleteUpgrade);
                s_reportManager.StartUpdateJobProgress();
                s_reportManager.IncreaseBase(10000);

                var setting = s_keyValueDao.GetValueByKey(S_DIRTY_DATA_KEY);
                if (setting != null)
                {
                    var definition = JsonConvert.DeserializeObject<RMCosmosDBDirtyDataNeedProcessedDefinition>(setting.Value);
                    if (definition.NeedProcess)
                    {
                        if(definition.SpecifySite)
                        {
                            foreach(var siteId in definition.SiteIds)
                            {
                                Process(siteId, definition.BeforeTicks);
                            }
                        }
                        else
                        {
                            Process(definition.BeforeTicks);
                        }

                        s_cosmosOperator.Complete();

                        definition.NeedProcess = false;
                        setting.Value = JsonConvert.SerializeObject(definition);
                        s_keyValueDao.SaveOrUpdateAsync(setting).GetAwaiter().GetResult();
                    }
                }

                var status = SucceedCount > 0 && FailedCount > 0 ? JobStatus.FinishWithException :
                    FailedCount > 0 ? JobStatus.Failed : JobStatus.Finished;
                s_reportManager.SetJobFinished(status);
            }
            catch (Exception e)
            {
                s_reportManager.SetJobFinished(JobStatus.Failed);
                s_logger.Error($"An error occurred while process job. Error: {e}");
            }

            s_logger.Debug($"Succeed process item count: [{SucceedCount}], failed item count: [{FailedCount}].");
        }

        private static void Process(long beforeTicks)
        {
            s_logger.Debug($"Use full mode process.");

            var sources = new List<int>
            {
                (int)SourceFlag.SharePoint,
                (int)SourceFlag.OneDrive
            };
            var queriedItemCount = 0;
            try
            {
                var continuationToken = string.Empty;
                do
                {
                    var res = s_retryer.Retry(() =>
                    {
                        return s_explorerDao.QueryByPage(item =>
                        sources.Contains(item.SourceFlag) &&
                        item.CollectTime < beforeTicks &&
                        item.RecordStatus == (int)RMRecordStatus.Active, 1000, continuationToken);
                    });

                    s_logger.Debug($"Batch query item count: [{res.Item1.Count()}].");

                    res.Item1.ToList().ForEach(item =>
                    {
                        item.RecordStatus = (int)RMRecordStatus.RMDeleted;
                        s_cosmosOperator.Add(item);
                    });

                    queriedItemCount += res.Item1.Count();
                    continuationToken = res.Item2;

                } while (!string.IsNullOrEmpty(continuationToken));
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while process. Error: {e}");
            }

            s_logger.Debug($"Queried item count: [{queriedItemCount}].");
        }

        private static void Process(Guid siteId, long beforeTicks)
        {
            s_logger.Debug($"Use specify mode process.");

            var sources = new List<int>
            {
                (int)SourceFlag.SharePoint,
                (int)SourceFlag.OneDrive
            };
            s_logger.Info($"Start process site [{siteId}].");
            var queriedItemCount = 0;
            try
            {
                var continuationToken = string.Empty;
                do
                {
                    var res = s_retryer.Retry(() =>
                    {
                        return s_explorerDao.QueryByPage(item =>
                        item.ScopeId == siteId &&
                        sources.Contains(item.SourceFlag) &&
                        item.CollectTime < beforeTicks &&
                        item.RecordStatus == (int)RMRecordStatus.Active, 1000, continuationToken);
                    });

                    s_logger.Debug($"Batch query item count: [{res.Item1.Count()}].");

                    res.Item1.ToList().ForEach(item =>
                    {
                        item.RecordStatus = (int)RMRecordStatus.RMDeleted;
                        s_cosmosOperator.Add(item);
                    });

                    queriedItemCount += res.Item1.Count();
                    continuationToken = res.Item2;

                } while (!string.IsNullOrEmpty(continuationToken));
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while process [{siteId}]. Error: {e}");
            }

            s_logger.Debug($"Site [{siteId}] queried item count: [{queriedItemCount}].");
        }

        private static async Task SucceedProcessRecord(Record record)
        {
            SucceedCount++;
            s_reportManager.Increase();
            s_logger.Info($"Succeed process record: [{record.Id}].");
        }

        private static void FailedProcessRecord(Record record, Exception e)
        {
            FailedCount++;
            s_reportManager.Increase();
            s_logger.Error($"An error occurred while process record: [{record.Id}]. Error: {e}");
        }
    }

    public class RMCosmosDBDirtyDataNeedProcessedDefinition
    {
        public bool NeedProcess { get; set; }

        public bool SpecifySite { get; set; }

        public List<Guid> SiteIds { get; set; } = new List<Guid>();

        public long BeforeTicks { get; set; } = 0;
    }
}
