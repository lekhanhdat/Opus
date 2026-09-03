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
using AvePoint.Hybrid.Utility.OperationSystem;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.DataSync.V2;
using AvePoint.RA.FileSystem.Stubs;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.Utils;
using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using static AvePoint.RA.FileSystem.Stubs.Stub;

namespace RAFileSystem.FileSystem.DataSync.V2
{
    public class FSAnalysisWorker : IFSDataSyncWorker
    {
        private static int _workerCounter;
        private readonly AveLogger _logger;
        private FSDataSyncChannelProvider _channelProvider;
        private readonly CancellationToken _token;
        private IProgressService ProgressService { get; set; }
        private IReportService<JMJobDetails> JobDetailService { get; set; }
        private FSObjectAnalyzer FileAnalyzer;

        public FSAnalysisWorker(FSDataSyncChannelProvider channelProvider, CancellationToken token)
        {
            var workerId = Interlocked.Increment(ref _workerCounter);
            _logger = AveLogger.GetInstance(typeof(FSAnalysisWorker), $"FSAnalysisWorker-{workerId}");
            _channelProvider = channelProvider;
            _token = token;
            FileAnalyzer = new FSObjectAnalyzer();
            ProgressService = JobContext.Current.mProgressManager.Create();
            JobDetailService = JobContext.Current.JobDetailManager.Create();
            JobContext.Current.Count = 0;
        }

        public async Task RunAsync()
        {
            try
            {
                _logger.Info($"FSAnalysisWorker started.");
                using (new AgentPerformanceScope("AnalyserWorker.Run", addToStatistics: true))
                {
                    await RunBufferedAnalysisAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Analyzer thread occurs an unexpected Error. Exception:{0}", ex.ToString());
            }
            _logger.Info("FSAnalysisWorker completed successfully.");
        }

        private async Task RunBufferedAnalysisAsync()
        {
            using (new AgentPerformanceScope("AnalyserWorker.RunBufferedAnalysis", addToStatistics: true))
            {
                const int BufferSize = 100;
                const int MaxDegreeOfParallelism = 10;

                using (var bufferCollection = new BlockingCollection<Stub>(BufferSize))
                {
                    var producerTask = ProduceAnalysisRecordsAsync(bufferCollection);
                    var consumerTask = Enumerable.Range(0, MaxDegreeOfParallelism).Select(_ => ConsumeAnalysisRecordAsync(bufferCollection));

                    await Task.WhenAll(new[] { producerTask }.Concat(consumerTask));
                    _logger.Info("Producer and consumer tasks completed");
                }
            }
        }

        private async Task ProduceAnalysisRecordsAsync(BlockingCollection<Stub> bufferCollection)
        {
            try
            {
                _logger.Info("Producer started - draining AnalyzerChannel to buffer");
                await _channelProvider.AnalyzerChannel.Reader.DrainChannelAsync(
                    stub =>
                    {
                        bufferCollection.Add(stub, _token);
                        return Task.CompletedTask;
                    },
                    _token);
            }
            catch (OperationCanceledException)
            {
                _logger.Warn("Producer canceled");
            }
            catch (Exception ex)
            {
                _logger.Error($"Producer error: {ex}");
            }
            finally
            {
                bufferCollection.CompleteAdding();
                _logger.Info("Producer marked collection as complete");
            }
        }

        private async Task ConsumeAnalysisRecordAsync(BlockingCollection<Stub> bufferCollection)
        {
            _logger.Info("Consumer started - processing items from buffer");
            foreach (var item in bufferCollection.GetConsumingEnumerable(_token))
            {
                try
                {
                    await ExecuteAnalysisAsync(item);
                }
                catch (Exception ex)
                {
                    _logger.Error($"Error processing item {item.FullPath.LogBase64()}: {ex}");
                }
            }
            _logger.Info("Consumer completed - no more items to process");
        }

        private async Task ExecuteAnalysisAsync(Stub stub)
        {
            using (new AgentPerformanceScope("AnalyserWorker.ExecuteAnalysis", addToStatistics: true))
            {
                try
                {
                    JobContext.Current.Count++;
                    if (FSJobCache.Instance.JobController.JobType == FSJobType.RematchRuleFullJob && stub.Type == StubType.File)
                    {
                        if (NeedSkipForRematchRule(stub))
                        {
                            _logger.Debug($"This file [{stub.FullPath.LogBase64()}] will be skipped, term or file is not changed.");
                            ProgressService.Increase();
                            return;
                        }
                    }
                    FileSystemRecordDto record = FileAnalyzer.Analyze(stub, (int)FSDataCollectorV2.ClassificationLevel);
                    if ((stub.Type == Stub.StubType.File && record.FileSize != 0)
                        || (stub.Type == Stub.StubType.Folder && (stub.failedInPreJob || FilterdIn(new XDirectoryInfoEx(stub.MediaObj))))
                        || stub.Type == Stub.StubType.ConnectionGroup
                        || stub.Type == Stub.StubType.ConnectionGroups)
                    {
                        await _channelProvider.WriteToPersistAsync(record, _token);
                    }
                    else
                    {
                        if (FSJobCacheV2.Instance.LastJobFailedItemIdsContains(stub.SelfId))
                        {
                            FSJobCache.Instance.SuccessItemIdsInLastJobFailedItems.Add(stub.SelfId);
                        }
                        _logger.Debug("Skip record {0}", record.LeafName);
                        ProgressService.Increase();
                    }
                }
                catch (HttpRequestException ex)
                {
                    _logger.Error("An error occurred while sending the crop：{0}", ex.Message.ToString());
                    HandleFailureRecord(stub, ex);
                    JobContext.Current.Count--;
                }
                catch (Exception itemex)
                {
                    _logger.Error("Failed to process item. Object:{0}, Exception:{1}", stub.FullPath.LogBase64(), itemex.ToString());
                    HandleFailureRecord(stub, itemex);
                }
            }
        }

        private void HandleFailureRecord(Stub stub, Exception ex)
        {
            ProgressService.Increase();
            FSJobCache.Instance.FailedCount++;
            string comment = ex.Message.StartsWith("RM_FS_DisposalDetail_TermIsInvalid", StringComparison.OrdinalIgnoreCase) ? ex.Message : "RM_JM_FSFailedAddToExplorer";
            JobDetailService.Commit(
                 new FSDataSyncJobReportDetailV2()
                 {
                     AgentName = OSInformation.HostName,
                     ObjectName = Alphaleonis.Win32.Filesystem.Path.GetFileName(stub.FullPath),
                     FullPath = stub.FullPath,
                     Status = JobDetailsStatus.Failed,
                     Comment = comment,
                     Depth = stub.Depth,
                     DirPath = stub.Type == StubType.File ? Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(stub.FullPath) : stub.FullPath
                 }
               );
            AddFailureItemToAzure(stub, ex);
            if (JobContext.Current.Count == 2 + FSJobCache.Instance.FailedCount)
            {
                JobContext.Current.AllErrorNode = true;
            }
            else
            {
                JobContext.Current.AllErrorNode = false;
            }
        }

        private void AddFailureItemToAzure(Stub stub, Exception ex)
        {
            if (FSJobCache.Instance.FailedItems.Count <= FSJobCache.Instance.FailedItemThrottling && !FSJobCacheV2.Instance.LastJobFailedItemIdsContains(stub.SelfId))
            {
                RMAgentSyncFailureItem item = new RMAgentSyncFailureItem()
                {
                    SiteId = FSJobCache.Instance.RunJobScopePath.ToLowerInvariant().ToMd5().ToString(),
                    ItemId = Guid.NewGuid().ToString(),
                    URL = stub.Type == StubType.File ? stub.FullPath.Substring(FSJobCache.Instance.RootPath.Length + 1) : stub.FullPath,
                    SortTicks = Snowflake.Instance().GetTicks(),
                    JobId = JobContext.Current.JobId,
                    SourceFlag = (int)SourceFlag.FileSystem,
                    ObjectName = stub.MediaObj.Name,
                    Message = GetExceptionMessage(ex)
                };
                item.NodeId = stub.Type == StubType.Folder
                    ? stub.FullPath.ToLowerInvariant().ToMd5().ToString()
                    : ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, stub.MediaObj.HighName, stub.MediaObj.LowName).Substring(FSJobCache.Instance.RootPath.Length + 1).ToLowerInvariant().ToMd5().ToString();
                FSJobCache.Instance.FailedItems.Add(item);
            }
        }

        private string GetExceptionMessage(Exception e)
        {
            string comment = e.Message;
            if (e is System.Reflection.TargetInvocationException)
            {
                System.Reflection.TargetInvocationException te = e as System.Reflection.TargetInvocationException;
                if (te.InnerException != null)
                {
                    comment = te.InnerException.Message;
                }
            }
            return comment;
        }

        private bool NeedSkipForRematchRule(Stub stub)
        {
            var dbRecord = stub.DBRecord;
            XFileInfoEx xObj = new XFileInfoEx(stub.MediaObj);
            if (xObj.CreationTimeUtc > FSJobCache.Instance.JobController.IncrementalStartTime
                || xObj.LastWriteTimeUtc > FSJobCache.Instance.JobController.IncrementalStartTime)
            {
                return false;
            }

            if (dbRecord != null)
            {
                if (FSJobCache.Instance.ChangedTermIds != null && dbRecord.TermId != null && dbRecord.TermId != Guid.Empty && FSJobCache.Instance.ChangedTermIds.Contains(dbRecord.TermId))
                {
                    return false;
                }
            }

            return true;
        }

        private bool FilterdIn(XDirectoryInfoEx t)
        {
            if (t.Name.IndexOf(".stub.html", StringComparison.OrdinalIgnoreCase) >= 0) { return false; }
            if (t.IsHidden) { return false; }
            switch (FSJobCache.Instance.JobController.JobType)
            {
                case FSJobType.UserFullJob:
                case FSJobType.RematchRuleFullJob:
                    return true;
                case FSJobType.IncrementalJob:
                    return (t.LastWriteTimeUtc > FSJobCache.Instance.JobController.IncrementalStartTime
                        || t.CreationTimeUtc > FSJobCache.Instance.JobController.IncrementalStartTime);
                default:
                    _logger.Warn("The code shouldnt go this approach.");
                    return false;
            }
        }
    }
}
