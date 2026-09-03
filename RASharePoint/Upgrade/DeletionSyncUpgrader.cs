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
using AvePoint.RA.Common.Report;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Explorer.Bulk;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Upgrade
{
    public class DeletionSyncUpgrader
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(DeletionSyncUpgrader));

        private static readonly IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;

        private static readonly IExplorerDao ExplorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao(true);

        private static readonly ICosmosBulkOperator CosmosOperator;

        private static int SucceedCount { get; set; }

        private static int FailedCount { get; set; }

        static DeletionSyncUpgrader()
        {
            CosmosOperator = CosmosBulkOperator.Instance;
            CosmosOperator.Start(5, SucceedProcessRecord, FailedProcessRecord);
        }

        public static void Process(string jobId)
        {
            try
            {
                ReportMangerFactory.Instance.Init(jobId, AvePoint.RA.Contract.JobMonitor.JobType.SharePointOnlineDeletionSyncUpgrade);
                ReportManager.StartUpdateJobProgress();
                ReportManager.IncreaseBase(10000);
                var deletedFolders = ExplorerDao.QueryAll(item => item.RecordStatus == (int)RMRecordStatus.RMDeleted && item.NodeType == (int)NodeLevel.Folder);
                deletedFolders = deletedFolders.OrderByDescending(item => item.DirPath.Length);
                ProcessFolders(deletedFolders);

                CosmosOperator.Complete();
                var status = SucceedCount > 0 && FailedCount > 0 ? JobStatus.FinishWithException :
                    FailedCount > 0 ? JobStatus.Failed : JobStatus.Finished;
                ReportManager.SetJobFinished(status);
                Logger.Debug($"Deletion item upgrade complete. Succeed: [{SucceedCount}]. Failed: [{FailedCount}].");
            }
            catch (Exception e)
            {
                ReportManager.SetJobFinished(JobStatus.Failed);
                Logger.Error($"An error occurred while process delete sync failed items. Error: {e}");
            }
        }

        private static void ProcessFolders(IEnumerable<Record> deletedFolders)
        {
            var processedFolders = new List<string>();

            foreach (var deletedFolder in deletedFolders)
            {
                var dirPth = deletedFolder.DirPath + "/";
                if (processedFolders.Any(item => dirPth.StartsWith(item)))
                {
                    continue;
                }

                ProcessFolder(deletedFolder);

                processedFolders.Add(dirPth);
            }
        }

        private static void ProcessFolder(Record deletedFolder)
        {
            var continuationToken = string.Empty;
            var dirPath = deletedFolder.DirPath + "/";
            var processedItemCount = 0;
            try
            {
                do
                {
                    var res = ExplorerDao.QueryByPage(item =>
                        item.DirPath.StartsWith(dirPath)
                        && item.RecordStatus != (int)RMRecordStatus.RMDeleted
                        && (item.NodeType == (int)NodeLevel.Item || item.NodeType == (int)NodeLevel.Folder), 1000, continuationToken);

                    continuationToken = res.Item2;
                    var willDeleteItems = res.Item1.ToList();

                    willDeleteItems.ForEach(item =>
                    {
                        item.RecordStatus = (int)RMRecordStatus.RMDeleted;
                        CosmosOperator.Add(item);
                    });

                    processedItemCount += willDeleteItems.Count;

                } while (!string.IsNullOrEmpty(continuationToken));
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while process folder [{deletedFolder.Id}]. Error: {e}");
            }

            Logger.Debug($"Folder [{deletedFolder.Id}] processed sub item count: [{processedItemCount}].");
        }

        private static async System.Threading.Tasks.Task SucceedProcessRecord(Record record)
        {
            SucceedCount++;
            ReportManager.Increase();
            Logger.Info($"Succeed process record: [{record.DirPath}]-[{record.Id}].");
        }

        private static void FailedProcessRecord(Record record, Exception e)
        {
            FailedCount++;
            ReportManager.Increase();
            Logger.Error($"An error occurred while process record: [{record.DirPath}]-[{record.Id}]. Error: {e}");
        }
    }
}

