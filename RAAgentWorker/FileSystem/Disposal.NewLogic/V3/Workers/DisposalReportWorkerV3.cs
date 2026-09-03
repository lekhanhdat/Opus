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
using System;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.GCommon;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.FileSystem.DataIngestion;

namespace RAFileSystem.FileSystem.Disposal.NewLogic.V3.Workers
{
    /// <summary>
    /// Reads data ingestion execution results and logs disposal report details.
    /// This worker runs after data ingestion is completed.
    /// </summary>
    public class DisposalReportWorkerV3 : IFSDisposalWorker
    {
        private readonly AveLogger logger = AveLogger.GetInstance(typeof(DisposalReportWorkerV3));
        private readonly RMDataIngestionExecutionResultCollector resultCollector;
        private readonly CancellationToken token;

        public DisposalReportWorkerV3(RMDataIngestionExecutionResultCollector resultCollector, CancellationToken token)
        {
            this.resultCollector = resultCollector;
            this.token = token;
        }

        public async Task RunAsync()
        {
            try
            {
                logger.Info("Start disposal report worker.");
                await resultCollector.ReadItemExecutionResultsAsync(ProcessResult, token).ConfigureAwait(false);
                logger.Info("Disposal report worker completed.");
            }
            catch (OperationCanceledException)
            {
                logger.Warn("Disposal report worker canceled.");
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred in disposal report worker. Error: {0}", ex);
                throw;
            }
        }

        private void ProcessResult(RMDataIngestionAgentWorkItemExecutionResult item)
        {
            logger.Debug(
                "Receive ingestion result for item id: {0}, name: {1}, ruleName: {2}, ruleAction: {3}, isSucceed: {4}, {5}",
                item.Id,
                ExternalUtil.CombinePath(item.DirPath, item.LeafName),
                item.RuleName,
                GetActionString(item.RuleAction),
                item.Succeed,
                !item.Succeed ? $"ErrorMessage: {item.Message}" : string.Empty);
        }

        private static string GetActionString(int action)
        {
            switch (action)
            {
                case (int)RuleAction.ArchiveAndRemove:
                    return "RM_FS_DisposalAction_Remove";
                case (int)RuleAction.MoveAndDeclare:
                    return "RM_FS_DisposalAction_Move";
                default:
                    return string.Empty;
            }
        }
    }
}
