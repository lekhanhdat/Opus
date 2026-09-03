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
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Explorer.Bulk;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApproval
{
    public class ManualApprovalDataSyncManager
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ManualApprovalDataSyncManager));

        private static readonly IExplorerDao ExplorerDao = new ExplorerDao(true);

        private static readonly ICosmosBulkOperator CosmosOperator;

        private static bool HasSucceed { get; set; }

        private static bool HasFailed { get; set; }
        
        private static int BulkSize { get; set; }

        private static Func<Record, Task> ProcessItemSucceedCallback { get; set; }

        private static Action<Record, string> ProcessItemFailedCallback { get; set; }

        static ManualApprovalDataSyncManager()
        {
            var keyValueDao = PlatformWindsorManager.GetService<IRMKeyValueDao>();
            BulkSize = keyValueDao.GetCosmosBulkInsertOperationBufferSize();
            if (BulkSize <= 0)
            {
                BulkSize = CosmosBulkOperator.DefualtBufferSize;
            }

            CosmosOperator = CosmosBulkOperator.Instance;
            CosmosOperator.Start(BulkSize, SucceedProcessRecordAsync, FailedProcessRecord);
            Logger.Info($"Succeed start cosmos db bulk operator. Bulk size: [{BulkSize}].");
        }

        public static void Commit()
        {
            CosmosOperator.Complete();
            CosmosOperator.Reset();
            CosmosOperator.Start(BulkSize, SucceedProcessRecordAsync, FailedProcessRecord);
            Logger.Info($"Succeed start cosmos db bulk operator. Bulk size: [{BulkSize}].");
        }

        public static void RegisteProcessItemCallback(Func<Record,Task> processItemSucceed, Action<Record, string> processItemFailed)
        {
            ProcessItemSucceedCallback = processItemSucceed;
            ProcessItemFailedCallback = processItemFailed;
        }

        public static void Add(Record record)
        {
            CosmosOperator.Add(record);
        }

        public static IEnumerable<List<Record>> GetAllApproveOrRejectedRecord(SourceFlag source)
        {
            if(source == SourceFlag.Connector)
            {
                yield return new();
            }

            var archiveStatus = (int)ActionStatus.Archiverd;
            Expression<Func<Record, bool>> filterFunc;
            if(source == SourceFlag.LifecycleRetention)
            {
                filterFunc = item => item.ManualRetentionStatus != 0
                && item.ManualArchiveStatus != archiveStatus && (item.ManualApprovedStatus == (int)SOApproveDBStatus.Approved || item.ManualApprovedStatus == (int)SOApproveDBStatus.Rejected);
            }
            else
            {
                filterFunc = item => item.SourceFlag == (int)source && item.ManualRetentionStatus == 0
                && item.ManualArchiveStatus != archiveStatus && (item.ManualApprovedStatus == (int)SOApproveDBStatus.Approved || item.ManualApprovedStatus == (int)SOApproveDBStatus.Rejected);
            }

            var continuationToken = string.Empty;
            do
            {
                var res = ExplorerDao.QueryByPage(filterFunc, 5000, continuationToken);
                continuationToken = res.Item2;
                yield return res.Item1.ToList();
            } while (!string.IsNullOrEmpty(continuationToken));
        }

        public static bool TryGet(Expression<Func<Record, bool>> predicate, out Record record)
        {
            record = ExplorerDao.GetFirstOrDefault(predicate);
            return record != null;
        }

        public static bool TryGet(Expression<Func<Record, bool>> predicate, Expression<Func<Record, dynamic>> orderLambda, out Record record)
        {
            record = ExplorerDao.GetFirstOrDefault(predicate, orderLambda);
            return record != null;
        }

        public static IEnumerable<IEnumerable<Record>> QueryItems(Expression<Func<Record, bool>> predicate)
        {
            var continuation = string.Empty;
            var pageSize = 1000;
            do
            {
                var result = ExplorerDao.QueryByPage(predicate, pageSize, continuation, false);
                continuation = result.Item2;
                yield return result.Item1;

            } while (!string.IsNullOrEmpty(continuation));
        }

        public static ManualApprovalOperationCosmosItemStatus WaitComplete()
        {
            Logger.Info($"Waiting cosmos db bulk operator job complete.");
            CosmosOperator.Complete();
            Logger.Info($"The cosmos db bulk operator job complete.");

            if (HasFailed && HasSucceed)
            {
                return ManualApprovalOperationCosmosItemStatus.HasException;
            }

            if (HasFailed)
            {
                return ManualApprovalOperationCosmosItemStatus.Failed;
            }

            return ManualApprovalOperationCosmosItemStatus.Succeed;
        }

        private static Task SucceedProcessRecordAsync(Record record)
        {
            Logger.Info($"Succeed process record. Source: [{(SourceFlag)record.SourceFlag}], Id: [{record.Id}], Container id: [{record.ContainerId}], Node id: [{record.NodeId}].");
            HasSucceed = true;
            if(ProcessItemSucceedCallback == null)
            {
                return Task.CompletedTask;
            }
            return ProcessItemSucceedCallback.Invoke(record);
        }

        private static void FailedProcessRecord(Record record, Exception e)
        {
            Logger.Error($"An error occurred while process record. Source: [{(SourceFlag)record.SourceFlag}], Id: [{record.Id}], Container id: [{record.ContainerId}], Node id: [{record.NodeId}]. Error: {e}");
            HasFailed = true;
            ProcessItemFailedCallback?.Invoke(record, e.Message);
        }
    }

    public enum ManualApprovalOperationCosmosItemStatus
    {
        Succeed,
        Failed,
        HasException,
    }
}
