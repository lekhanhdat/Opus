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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Hybrid.Utility.OperationSystem;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.DataSync.V2;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.FileSystem.DataIngestion;
using RAFileSystem.Utils;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.DataSync.V2
{
    public class FSReportWorker : IFSDataSyncWorker
    {
        private static int _workerCounter;
        private readonly AveLogger _logger;
        private RMDataIngestionExecutionResultCollector _ingestionExecutionResultCollector;
        private FSDataSyncChannelProvider _channelProvider;
        private IReportService<JMJobDetails> JobDetailService;
        private IProgressService ProgressService;
        private CancellationToken _token;
        private int DEFAULT_BATCH_SIZE => ConfigUtils.WORKER_TRANSFER_DATA_COUNT;
        private ConcurrentBag<Task> _inflightFlushes = new ConcurrentBag<Task>();

        // Windows System Error Code for Access Denied. WinError.h => #define ERROR_ACCESS_DENIED 5L
        // HRESULT: 0x80070005
        private const int ERROR_ACCESS_DENIED = unchecked((int)0x80070005);
        public FSReportWorker(FSDataSyncChannelProvider channelProvider, RMDataIngestionExecutionResultCollector ingestionExecutionResultCollector, CancellationToken token)
        {
            var workerId = Interlocked.Increment(ref _workerCounter);
            _logger = AveLogger.GetInstance(typeof(FSReportWorker), $"FSReportWorker-{workerId}");
            ProgressService = JobContext.Current.mProgressManager.Create();
            JobDetailService = JobContext.Current.JobDetailManager.Create();
            _ingestionExecutionResultCollector = ingestionExecutionResultCollector;
            _token = token;
            _channelProvider = channelProvider;
        }

        public async Task RunAsync()
        {
            try
            {
                _logger.Info("Start FS data report worker.");
                using (new AgentPerformanceScope("ReportWorker.Run", addToStatistics: true))
                {
                    var buffer = new List<RMDataIngestionAgentWorkItemExecutionResult>(DEFAULT_BATCH_SIZE);
                    await _ingestionExecutionResultCollector.ReadItemExecutionResultsAsync(record =>
                    {
                        try
                        {
                            buffer.Add(record);
                            ProgressService.Increase();
                            if (buffer.Count >= DEFAULT_BATCH_SIZE)
                            {
                                var batchToFlush = buffer;
                                StartFlushInBackground(batchToFlush);
                                buffer = new List<RMDataIngestionAgentWorkItemExecutionResult>(DEFAULT_BATCH_SIZE);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error("Failed to buffer the record. Exception:{0}", ex.ToString());
                            HandleRecordFailure(record);
                        }
                    }, _token);

                    if (buffer.Count > 0)
                    {
                        StartFlushInBackground(buffer);
                    }

                    await WaitForAllInflightFlushes();
                    _logger.Info("FSReportWorker has completed all tasks.");
                }
            }
            catch (OperationCanceledException ex)
            {
                _logger.Warn("FSReportWorker canceled.", ex);
            }
            catch (Exception ex)
            {
                _logger.Error("An error occurred while process FSReportWorker.", ex);
            }
        }

        private void StartFlushInBackground(List<RMDataIngestionAgentWorkItemExecutionResult> batch)
        {
            _logger.Debug("Scheduling background report flush. BatchSize:{0}, InflightCount:{1}", batch.Count, _inflightFlushes.Count);

            var task = Task.Run(() => ProcessSyncReports(batch), _token);
            _inflightFlushes.Add(task);

            // Periodically clean up completed tasks to avoid memory leak
            if (_inflightFlushes.Count > 50)
            {
                var allTasks = _inflightFlushes.ToArray();
                var activeTasks = allTasks.Where(t => !t.IsCompleted && !t.IsCanceled && !t.IsFaulted).ToArray();
                if (activeTasks.Length < allTasks.Length)
                {
                    // Rebuild bag with only active tasks
                    var newBag = new ConcurrentBag<Task>(activeTasks);
                    Interlocked.Exchange(ref _inflightFlushes, newBag);
                }
            }
        }

        private async Task WaitForAllInflightFlushes()
        {
            _logger.Info("FSReportWorker is stopping. Waiting for inflight flushes to complete.");
            Task[] tasks = _inflightFlushes.ToArray();
            try
            {
                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.Error("One or more flush tasks failed.", ex);
            }
        }

        private void ProcessSyncReports(List<RMDataIngestionAgentWorkItemExecutionResult> batch)
        {
            using (new AgentPerformanceScope("ReportWorker.ProcessSyncReports", addToStatistics: true))
            {
                int committedCount = 0;
                var reportsToCommit = new List<FSDataSyncJobReportDetailV2>(batch.Count);
                var failedIds = new HashSet<Guid>(batch.Where(r => !r.Succeed).Select(r => r.Id));
                failedIds.UnionWith(FSJobCache.Instance.CurrentJobFailedItemIds);

                foreach (var record in batch)
                {
                    ProcessSingleReport(record, failedIds, reportsToCommit);
                }

                if (reportsToCommit.Count > 0)
                {
                    try
                    {
                        JobDetailService.CommitBatch(reportsToCommit);
                        committedCount = reportsToCommit.Count;
                    }
                    catch (Exception ex)
                    {
                        _logger.Error("Batch commit failed. Count:{0}, Error:{1}", reportsToCommit.Count, ex);
                        int fallbackSuccess = 0;
                        foreach (var report in reportsToCommit)
                        {
                            try
                            {
                                JobDetailService.Commit(report);
                                fallbackSuccess++;
                            }
                            catch (Exception inner)
                            {
                                _logger.Error("Fallback commit failed for {0}: {1}", report.FullPath, inner.Message);
                            }
                        }
                        committedCount = fallbackSuccess;
                    }
                }
                _logger.Info("Batch complete. Processed:{0}, Committed:{1}", batch.Count, committedCount);
            }
        }
        private void ProcessSingleReport(RMDataIngestionAgentWorkItemExecutionResult result, HashSet<Guid> failedIdSetDics, List<FSDataSyncJobReportDetailV2> reportsToCommit)
        {
            if (result.NodeType == (int)NodeLevel.FSConnectionGroup || result.NodeType == (int)NodeLevel.FSConnectionGroups)
            {
                return;
            }
            FSDataSyncJobReportDetailV2 report = BuildReport(result);
            try
            {
                bool addedToCache = false;
                bool isFailed = failedIdSetDics.Contains(result.Id);

                if (isFailed)
                {
                    AddFailureItemToAzure(result);
                    report.Status = JobDetailsStatus.Failed;
                    report.Comment = "RM_JM_FSFailedAddToExplorer";
                    FSJobCache.Instance.FailedCount++;
                }
                else
                {
                    addedToCache = Add2SuccessItemCache(result);
                    report.Status = JobDetailsStatus.Successful;
                    FSJobCache.Instance.SuccessCount++;
                }

                if (HasChangedTermOrRule(result) || result.NodeType == (int)NodeLevel.FSFolder || addedToCache || isFailed)
                {
                    reportsToCommit.Add(report);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.Error($"Access Denied. Id:{result.Id}, FullPath:{report.FullPath}, Error:{ex}");
                report.Status = JobDetailsStatus.Failed;
                report.Comment = "RM_JS_JMD_FS_JPMC_PathCanNotAccess";
                reportsToCommit.Add(report);
                FSJobCache.Instance.FailedCount++;
            }
            catch (IOException ex) when (ex.HResult == ERROR_ACCESS_DENIED)
            {
                _logger.Error($"Access Denied (IO). Id:{result.Id}, FullPath:{report.FullPath}, Error:{ex}");
                report.Status = JobDetailsStatus.Failed;
                report.Comment = "RM_JS_JMD_FS_JPMC_PathCanNotAccess";
                reportsToCommit.Add(report);
                FSJobCache.Instance.FailedCount++;
            }
            catch (Exception ex)
            {
                _logger.Error($"Failed to process report. Id:{result.Id}, FullPath:{report.FullPath}, Error:{ex}");
                report.Status = JobDetailsStatus.Failed;
                report.Comment = "RM_JM_FSFailedAddToExplorer";
                reportsToCommit.Add(report);
                FSJobCache.Instance.FailedCount++;
            }
        }

        private FSDataSyncJobReportDetailV2 BuildReport(RMDataIngestionAgentWorkItemExecutionResult result)
        {
            return new FSDataSyncJobReportDetailV2
            {
                AgentName = OSInformation.HostName,
                ObjectName = result.LeafName,
                FullPath = ExternalUtil.CombinePath(result.DirPath, result.LeafName),
                Depth = result.Depth,
                DirPath = result.NodeType == (int)NodeLevel.FSFile ? result.DirPath : ExternalUtil.CombinePath(result.DirPath, result.LeafName),
            };
        }

        private bool HasChangedTermOrRule(RMDataIngestionAgentWorkItemExecutionResult res)
        {
            return res.HasRuleChanged || res.HasTermChanged;
        }

        private void AddFailureItemToAzure(RMDataIngestionAgentWorkItemExecutionResult dto)
        {
            var fullPath = ExternalUtil.CombinePath(dto.DirPath, dto.LeafName);
            if (!TryBuildRelativePath(fullPath, out string relativePath, out Guid nodeId, dto.Id))
            {
                _logger.Warn($"Skip adding failed item because the path is invalid. FullPath:{fullPath}, RootPath:{FSJobCache.Instance.RootPath}");
                return;
            }
            if (FSJobCache.Instance.FailedItems.Count <= FSJobCache.Instance.FailedItemThrottling
                && (dto.NodeType == (int)NodeLevel.FSFile || dto.NodeType == (int)NodeLevel.FSFolder) && !FSJobCacheV2.Instance.LastJobFailedItemIdsContains(nodeId))
            {
                RMAgentSyncFailureItem item = new RMAgentSyncFailureItem()
                {
                    SiteId = FSJobCache.Instance.RunJobScopePath.ToLowerInvariant().ToMd5().ToString(),
                    ItemId = Guid.NewGuid().ToString(),
                    URL = fullPath.Substring(FSJobCache.Instance.RootPath.Length + 1),
                    SortTicks = Snowflake.Instance().GetTicks(),
                    JobId = JobContext.Current.JobId,
                    NodeId = nodeId.ToString(),
                    SourceFlag = (int)SourceFlag.FileSystem,
                    ObjectName = dto.LeafName,
                    Message = "RM_JM_FSFailedAddToExplorer"
                };
                FSJobCache.Instance.FailedItems.Add(item);
                _logger.Info($"Add failed item to azure: {item.NodeId}");
            }
        }

        private void HandleRecordFailure(RMDataIngestionAgentWorkItemExecutionResult record)
        {
            JobDetailService.Commit(new FSDataSyncJobReportDetailV2()
            {
                AgentName = OSInformation.HostName,
                ObjectName = record.LeafName,
                FullPath = ExternalUtil.CombinePath(record.DirPath, record.LeafName),
                Status = JobDetailsStatus.Failed,
                Comment = "RM_JM_FSFailedAddToExplorer",
                Depth = record.Depth,
                DirPath = record.NodeType == (int)NodeLevel.FSFile ? record.DirPath : ExternalUtil.CombinePath(record.DirPath, record.LeafName),
            });
            FSJobCache.Instance.FailedCount++;
            AddFailureItemToAzure(record);
        }

        private bool Add2SuccessItemCache(RMDataIngestionAgentWorkItemExecutionResult dto)
        {
            bool success = false;
            var fullPath = ExternalUtil.CombinePath(dto.DirPath, dto.LeafName);
            if (!TryBuildRelativePath(fullPath, out string relativePath, out Guid nodeId, dto.Id))
            {
                _logger.Warn($"Skip adding success item cache because the path is invalid. FullPath:{fullPath}, RootPath:{FSJobCache.Instance.RootPath}");
                return false;
            }
            if (FSJobCacheV2.Instance.LastJobFailedItemIdsContains(nodeId))
            {
                FSJobCache.Instance.SuccessItemIdsInLastJobFailedItems.Add(nodeId);
                success = true;
            }
            return success;
        }
        private bool TryBuildRelativePath(string fullPath, out string relativePath, out Guid nodeId, Guid fallbackNodeId)
        {
            relativePath = string.Empty;
            nodeId = fallbackNodeId;

            var rootPath = FSJobCache.Instance.RootPath;
            if (string.IsNullOrEmpty(fullPath) || string.IsNullOrEmpty(rootPath))
            {
                return false;
            }

            var normalizedRootPath = rootPath.TrimEnd('\\');
            var normalizedFullPath = fullPath.TrimEnd('\\');

            if (normalizedFullPath.Equals(normalizedRootPath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (!normalizedFullPath.StartsWith(normalizedRootPath, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (normalizedFullPath.Length <= normalizedRootPath.Length
                || normalizedFullPath[normalizedRootPath.Length] != '\\')
            {
                return false;
            }

            relativePath = normalizedFullPath.Substring(normalizedRootPath.Length + 1);
            nodeId = relativePath.ToLowerInvariant().ToMd5();
            return true;
        }
    }
}
