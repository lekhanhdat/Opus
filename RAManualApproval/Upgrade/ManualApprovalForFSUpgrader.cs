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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.AzureCosmosDB;
using AvePoint.RA.DB.CosmosDBControl;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Model;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using RACloudFS.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApproval.Upgrade
{
    public class ManualApprovalForFSUpgrader
    {
        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(ManualApprovalForFSUpgrader));

        private static readonly IRMReportManager s_reportManager = ReportMangerFactory.Instance.ReportManager;

        private static readonly DateTime s_start_datetime = DateTime.UtcNow.AddDays(-14);
        private static IRMKeyValueDao s_keyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private static readonly Random s_random = new();

        private const int S_LIMIT_PAGE = 1000;

        private const int S_RANDOM_DAY_RANGE = 14;

        private const string S_CREATE_DATE_SETTING = "NeedRenewCreateDateSetting";
        public static async Task Run(string jobId)
        {
            try
            {
                ReportMangerFactory.Instance.Init(jobId, AvePoint.RA.Contract.JobMonitor.JobType.ManualFileSystemUpgrade);
                s_reportManager.StartUpdateJobProgress();
                s_reportManager.IncreaseBase(10000);

                var container = await RMAzureCosmosDBContext.GetContainerAsync();
                string continuationToken = null;

                var createDates = GetNeedRenewCreateDates();

                do
                {
                    var result = await container.UseLinqQuery().Where(item =>
                        item.SourceFlag == (int)SourceFlag.FileSystem &&
                        (item.ManualApprovedStatus == (int)SOApproveDBStatus.Approved ||
                         item.ManualApprovedStatus == (int)SOApproveDBStatus.Rejected ||
                         item.ManualApprovedStatus == (int)SOApproveDBStatus.WaitingApprove) &&
                        item.ManualArchiveStatus == (int)ActionStatus.None &&
                        createDates.Contains(item.CreateDate)).AsResultSet().PaginateAsync(continuationToken, S_LIMIT_PAGE);

                    var items = result.Values.ToList();
                    continuationToken = result.ContinuationToken;

                    await ProcessItems(container, items);

                    s_reportManager.Increase(S_LIMIT_PAGE);

                    s_logger.Debug($"Batch queried item count: [{items.Count}]");

                } while (!string.IsNullOrEmpty(continuationToken));

                s_reportManager.SetJobFinished(JobStatus.Finished);
            }
            catch(Exception e)
            {
                s_reportManager.SetJobFinished(JobStatus.Failed);
                s_logger.Error($"An error occurred while process delete sync failed items. Error: {e}");
            }
        }

        public static async Task ProcessItems(RMAzureCosmosDBContainer container, List<Record> items)
        {
            foreach (var item in items)
            {
                try
                {
                    item.AppendMetaInfoForOldLogic();
                    /* Fortify Issue Type: Insecure Randomness 
                    * Sink Details: this class  Run method 
                    * Ignore Reason: random用于生成日期，不涉及安全问题
                    */
                    var addedDays = s_random.Next(1, S_RANDOM_DAY_RANGE);

                    var originalCreateDate = item.CreateDate;

                    item.CreateDate = int.Parse(s_start_datetime.AddDays(addedDays).ToString("yyyyMMdd"));

                    item.AppendCustomColumns();

                    await container.AddAsync(item);

                    await container.DeleteAsync(item.Id, item.BuildPartitionKey());

                    s_logger.Info($"Succeed process item: [{item.Id}].");
                }
                catch (Exception e)
                {
                    s_logger.Error($"An error occurred while process item: [{item.Id}]. Error: {e}");
                }
            }
        }

        /// <summary>
        /// Read the CreateDate setting that needs to be renewed from the keyvalue table.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        private static List<int> GetNeedRenewCreateDates()
        {
            var dateSetting = s_keyValueDao.GetValueByKey(S_CREATE_DATE_SETTING);

            if (string.IsNullOrEmpty(dateSetting?.Value))
            {
                throw new ArgumentNullException("dateSetting", $"Please check the value of {S_CREATE_DATE_SETTING} in the keyvalue table.");
    		}

            s_logger.Info(dateSetting.Value);

            var result = dateSetting.Value.Split(',').Select(int.Parse).ToList();

            return result;
        }
    }
}
