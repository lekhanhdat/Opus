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
using AvePoint.Hybrid.Utility.Cryptography;
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
using RAFileSystem.FileSystem.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static AvePoint.RA.FileSystem.Stubs.Stub;

namespace RAFileSystem.FileSystem.DataSync.V2
{
    public class FSDiscoveryWorker : IFSDataSyncWorker
    {
        private static int _workerCounter;
        private readonly AveLogger _logger;
        private IXSystem _system;
        private IProgressService ProgressService { get; set; }
        private IReportService<JMJobDetails> JobDetailService { get; set; }
        private readonly FSDataSyncChannelProvider _channelProvider;
        private readonly CancellationToken _token;
        private int DEFAULT_BATCH_SIZE => ConfigUtils.WORKER_TRANSFER_DATA_COUNT;

        public FSDiscoveryWorker(FSDataSyncChannelProvider channelProvider, CancellationToken token)
        {
            var workerId = Interlocked.Increment(ref _workerCounter);
            _logger = AveLogger.GetInstance(typeof(FSDiscoveryWorker), $"FSDiscoveryWorker-{workerId}");
            _channelProvider = channelProvider;
            _token = token;
            ProgressService = JobContext.Current.mProgressManager.Create();
            JobDetailService = JobContext.Current.JobDetailManager.Create();
            _system = ExternalUtil.OpenXSystem(FSJobCache.Instance.RootPath);
        }

        public async Task RunAsync()
        {
            try
            {
                _logger.Info("Start data sync discovery worker...");
                using (new AgentPerformanceScope("DiscoveryWorker.Run", addToStatistics: true))
                {
                    var currentConnectionSettings = GetCurrentConnectionAllSettings();
                    var reader = _channelProvider.DiscoverChannel.Reader;
                    var overflowStack = _channelProvider.DiscoverOverflowStack;

                    while (true)
                    {
                        _token.ThrowIfCancellationRequested();

                        if (overflowStack.TryPop(out var overflowStub))
                        {
                            _logger.Debug("Processing item from overflow stack. Path:{0}, StackCount:{1}", overflowStub.FullPath.LogBase64(), overflowStack.Count);
                            await ExecuteDiscoveryAsync(overflowStub, currentConnectionSettings);
                        }
                        else if (reader.TryRead(out var channelStub))
                        {
                            try
                            {
                                await ExecuteDiscoveryAsync(channelStub, currentConnectionSettings);
                            }
                            finally
                            {
                                _channelProvider.DecreaseDiscoveryCount();
                            }
                        }
                        else
                        {
                            if (!overflowStack.IsEmpty) continue;
                            if (!await reader.WaitToReadAsync(_token))
                            {
                                var remainingCount = overflowStack.Count;
                                if (remainingCount > 0)
                                {
                                    _logger.Info("Channel completed. Draining {0} remaining items from overflow stack.", remainingCount);
                                }
                                while (overflowStack.TryPop(out var remainingStub))
                                {
                                    _token.ThrowIfCancellationRequested();
                                    await ExecuteDiscoveryAsync(remainingStub, currentConnectionSettings);
                                }
                                break;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to discover the files. Exception:{0}", ex.ToString());
            }
            _logger.Info("End discovery worker.");
        }

        private async Task ExecuteDiscoveryAsync(Stub stub, List<FSFolderCacheDto> connectionSettings)
        {
            using (new AgentPerformanceScope("DiscoveryWorker.ExecuteDiscovery", addToStatistics: true))
            {
                try
                {
                    if (!stub.failedInPreJob) stub.failedInPreJob = FailedInLastJob(stub.SelfId);
                    if (FSDataCollectorV2.ClassificationLevel == NodeLevel.FSFile)
                    {
                        _logger.Debug("Begin to query files in folder:{0}", stub.FullPath.LogBase64());
                        bool addedFolder = false;
                        foreach (var files in QueryFilesInBatch(stub))
                        {
                            using (new AgentPerformanceScope("DiscoveryWorker.ProcessQueryFiles", addToStatistics: true))
                            {
                                var fileIds = files.Select(f => f.SelfId).ToList();
                                if (!addedFolder)
                                {
                                    fileIds.Add(stub.SelfId);
                                }

                                var explorerRecords = QueryFileSystemRecords(fileIds).GroupBy(n => n.NodeId).ToDictionary(k => k.Key, v => v.ToList());

                                if (!addedFolder && explorerRecords.ContainsKey(stub.SelfId))
                                {
                                    stub.DBRecord = explorerRecords[stub.SelfId].FirstOrDefault();
                                    if (FSJobCache.Instance.EnableJPMC)
                                    {
                                        bool ruleHasModified = ClassCodeCommonStaticMethod.IsRuleModified(stub.DBRecord.TermId, stub.DBRecord.CollectionTime);
                                        var fsDto = new FileSystemRecordDto();
                                        if (ruleHasModified)
                                        {
                                            ClassCodeCommonStaticMethod.GenerateRetentionTimeCacheKeyAndSetEndTime(fsDto, new ClassCodeInfoDto()
                                            {
                                                CountryCode = stub.DBRecord.CountryCode,
                                                RetentionType = stub.DBRecord.RetentionType,
                                                TermId = stub.DBRecord.TermId,
                                                StartDate = stub.DBRecord.StartDate
                                            });
                                        }
                                        lock (FSJobCache.Instance.ContainerLevelClassCodeCacheLock)
                                        {
                                            if (!FSJobCache.Instance.ContainerLevelClassCodeCache.ContainsKey(stub.DBRecord.NodeId))
                                            {
                                                FSJobCache.Instance.ContainerLevelClassCodeCache.Add(stub.DBRecord.NodeId, new ClassCodeInfoDto()
                                                {
                                                    ClassCode = stub.DBRecord.ClassCode,
                                                    CountryCode = stub.DBRecord.CountryCode,
                                                    RetentionType = stub.DBRecord.RetentionType,
                                                    TermId = stub.DBRecord.TermId,
                                                    StartDate = stub.DBRecord.StartDate,
                                                    EndTime = stub.DBRecord.EndTime,
                                                    PolicyValueNumber = ruleHasModified ? fsDto.PolicyValueNumber : stub.DBRecord.PolicyValueNumber,
                                                    PolicyValueUnit = ruleHasModified ? fsDto.PolicyValueUnit : stub.DBRecord.PolicyValueUnit,
                                                    CollectionTime = stub.DBRecord.CollectionTime,
                                                });
                                            }
                                        }
                                    }
                                }
                                if (!addedFolder)
                                {
                                    await _channelProvider.WriteToAnalyzerAsync(stub, _token);
                                    addedFolder = true;
                                }

                                Parallel.ForEach(files, new ParallelOptions { MaxDegreeOfParallelism = 5 }, f =>
                                {
                                    if (explorerRecords.TryGetValue(f.SelfId, out var records))
                                    {
                                        f.DBRecord = records.FirstOrDefault();
                                    }
                                });
                                await _channelProvider.WriteBatchToAnalyzerAsync(files, _token);
                            }
                        }

                        if (!addedFolder) await DiscoverFolderStubWithoutFiles(stub);

                        await QuerySubFoldersFileLevelInBatch(stub);
                    }
                    else
                    {
                        await QuerySubFoldersInBatch(stub, connectionSettings);
                    }
                }
                catch (Exception ex)
                {
                    HandleDiscoveryError(stub, ex);
                }
            }
        }

        private async Task DiscoverFolderStubWithoutFiles(Stub folderStub)
        {
            var folderRecord = QueryFileSystemRecords(new List<Guid> { folderStub.SelfId }).FirstOrDefault();
            if (folderRecord != null)
            {
                folderStub.DBRecord = folderRecord;
            }
            await _channelProvider.WriteToAnalyzerAsync(folderStub, _token);
        }

        private void HandleDiscoveryError(Stub stub, Exception ex)
        {
            _logger.Error("Failed to process item. Object:{0}, Exception:{1}", stub.FullPath.LogBase64(), ex);

            ProgressService.Increase();
            JobContext.Current.HasErrorNode = true;

            JobDetailService.Commit(
                new FSDataSyncJobReportDetailV2
                {
                    AgentName = OSInformation.HostName,
                    ObjectName = Alphaleonis.Win32.Filesystem.Path.GetFileName(stub.FullPath),
                    FullPath = stub.FullPath,
                    Status = JobDetailsStatus.Failed,
                    Comment = "RM_JM_FSFailedToDiscoverFolder",
                    Depth = stub.Depth,
                    DirPath = stub.Type == StubType.File ? Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(stub.FullPath) : stub.FullPath,
                });

            AddFailureItemToAzure(stub);
        }

        private void AddFailureItemToAzure(Stub stub)
        {
            var fullPath = stub.FullPath;
            Guid nodeId = stub.SelfId;
            if (FSJobCache.Instance.FailedItems.Count <= FSJobCache.Instance.FailedItemThrottling && !FSJobCacheV2.Instance.LastJobFailedItemIdsContains(nodeId))
            {
                RMAgentSyncFailureItem item = new RMAgentSyncFailureItem()
                {
                    SiteId = FSJobCache.Instance.RunJobScopePath.ToLowerInvariant().ToMd5().ToString(),
                    ItemId = Guid.NewGuid().ToString(),
                    URL = fullPath,
                    SortTicks = RAFileSystem.Utils.Snowflake.Instance().GetTicks(),
                    JobId = JobContext.Current.JobId,
                    NodeId = nodeId.ToString(),
                    SourceFlag = (int)SourceFlag.FileSystem,
                    ObjectName = stub.MediaObj.Name,
                    Message = "RM_JM_FSFailedAddToExplorer"
                };
                FSJobCache.Instance.FailedItems.Add(item);
            }
        }

        private async Task QuerySubFoldersFileLevelInBatch(Stub stub)
        {
            using (new AgentPerformanceScope("DiscoveryWorker.QuerySubFoldersFileLevel", addToStatistics: true))
            {
                var dirsCollection = _system.GetDirectoriesInBatch(stub.MediaObj, DEFAULT_BATCH_SIZE);
                foreach (var dirs in dirsCollection)
                {
                    using (new AgentPerformanceScope("DiscoveryWorker.ProcessSubfoldersBatch.FileLevel", addToStatistics: true))
                    {
                        if (dirs == null || dirs.Count == 0) return;

                        var scopeSettingCache = FSJobCacheV2.Instance.ScopeSettingCache;
                        var rootPath = FSJobCache.Instance.RootPath;
                        var termSettingId = stub.ScopeSettingId;
                        var dirStubs = new ConcurrentBag<Stub>();

                        Parallel.ForEach(dirs, new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = _token }, dir =>
                        {
                            var fullPath = ExternalUtil.CombinePath(rootPath, dir.HighName, dir.LowName);
                            var normalizedPath = fullPath.ToLowerInvariant();
                            var selfId = normalizedPath.ToMd5();

                            if (scopeSettingCache.ContainsKey(selfId))
                            {
                                _logger.Debug("The folder node {0} has unique setting.", fullPath.LogBase64());
                                return;
                            }

                            if (HasRunningJob(normalizedPath))
                            {
                                _logger.Debug("There is already a job running on this node. Id:{0}", selfId);
                                return;
                            }

                            dirStubs.Add(new FSFolderStub
                            {
                                FullPath = fullPath,
                                MediaObj = dir,
                                ScopeSettingId = termSettingId,
                                SelfId = selfId,
                                ParentId = stub.SelfId,
                                failedInPreJob = stub.failedInPreJob,
                                Depth = stub.Depth + 1,
                            });
                        });

                        AssembleDBRecords(dirStubs.ToList());
                        DispatchToDiscoverOrOverflow(dirStubs.ToList());
                        ProgressService.IncreaseBase(dirStubs.Count);
                        _logger.Info("Found {0} new folders", dirStubs.Count);
                    }
                }
            }
            return;
        }

        private async Task QuerySubFoldersInBatch(Stub stub, List<FSFolderCacheDto> differentTermSubFolders)
        {
            using (new AgentPerformanceScope("DiscoveryWorker.QuerySubFolders", addToStatistics: true))
            {
                var dirsCollection = _system.GetDirectoriesInBatch(stub.MediaObj, DEFAULT_BATCH_SIZE);

                if (FilterdIn(new XDirectoryInfoEx(stub.MediaObj), stub.SelfId, stub.failedInPreJob))
                {
                    await _channelProvider.WriteToAnalyzerAsync(stub, _token);
                }

                foreach (var dirs in dirsCollection)
                {
                    using (new AgentPerformanceScope("DiscoveryWorker.ProcessSubfoldersBatch", addToStatistics: true))
                    {
                        var dirStubs = new ConcurrentBag<Stub>();
                        Parallel.ForEach(dirs, new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = _token }, dir =>
                        {
                            string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, dir.HighName, dir.LowName);
                            Guid id = fullPath.ToLowerInvariant().ToMd5();
                            if (FSJobCacheV2.Instance.ScopeSettingCache.ContainsKey(id))
                            {
                                _logger.Debug("The folder node {0}  has unique setting.", fullPath.LogBase64());
                                return;
                            }

                            if (HasRunningJob(fullPath.ToLowerInvariant()))
                            {
                                _logger.Debug("There is already a job running on this node.id:{0}", id);
                                return;
                            }

                            var newStub = new FSFolderStub
                            {
                                FullPath = fullPath,
                                MediaObj = dir,
                                ScopeSettingId = stub.ScopeSettingId,
                                SelfId = id,
                                ParentId = stub.SelfId,
                                failedInPreJob = stub.failedInPreJob,
                                TermId4Folder = stub.TermId4Folder,
                                TermName4Folder = stub.TermName4Folder,
                                Depth = stub.Depth + 1,
                            };
                            dirStubs.Add(newStub);
                        });

                        AssembleDBRecords(dirStubs.ToList());
                        DispatchToDiscoverOrOverflow(dirStubs.ToList());
                        _logger.Info("Found {0} new folders", dirs.Count);
                        ProgressService.IncreaseBase(dirStubs.Count);
                    }
                }
            }
        }

        private IEnumerable<List<Stub>> QueryFilesInBatch(Stub stub)
        {
            using (new AgentPerformanceScope("DiscoveryWorker.QueryFilesInBatch", addToStatistics: true))
            {
                var files = _system.GetFilesInBatch(stub.MediaObj, DEFAULT_BATCH_SIZE);
                var filesCount = 0;
                foreach (var batchFiles in files)
                {
                    var fileStubs = new ConcurrentBag<Stub>();
                    Parallel.ForEach(batchFiles, new ParallelOptions { MaxDegreeOfParallelism = 5, CancellationToken = _token }, t =>
                     {
                         if (FilterdIn(new XFileInfoEx(t), stub.failedInPreJob))
                         {
                             string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, t.HighName, t.LowName);
                             var fileId = fullPath.ToLowerInvariant().ToMd5();
                             fileStubs.Add(new FSFileStub
                             {
                                 FullPath = fullPath,
                                 MediaObj = t,
                                 SelfId = fileId,
                                 ParentId = stub.SelfId,
                                 ScopeSettingId = stub.ScopeSettingId,
                                 failedInPreJob = stub.failedInPreJob,
                             });
                         }
                     });
                    yield return fileStubs.ToList();
                    ProgressService.IncreaseBase(fileStubs.Count);
                    filesCount += fileStubs.Count;
                }
                _logger.Debug("Found {0} files filtered in {1}", filesCount, stub.MediaObj.HighName);
            }
        }

        private void AssembleDBRecords(List<Stub> stubs)
        {
            using (new AgentPerformanceScope("DiscoveryWorker.AssembleDBRecords", addToStatistics: true))
            {
                var folderRecords = QueryFileSystemRecords(stubs.Select(ds => ds.SelfId).ToList()).GroupBy(r => r.NodeId).ToDictionary(g => g.Key, g => g.First());
                foreach (var ds in stubs)
                {
                    bool ruleHasModified = false;
                    if (folderRecords.TryGetValue(ds.SelfId, out var record))
                    {
                        ds.DBRecord = record;
                        if (FSJobCache.Instance.EnableJPMC)
                        {
                            ruleHasModified = ClassCodeCommonStaticMethod.IsRuleModified(record.TermId, record.CollectionTime);
                            var fsDto = new FileSystemRecordDto();
                            if (ruleHasModified)
                            {
                                ClassCodeCommonStaticMethod.GenerateRetentionTimeCacheKeyAndSetEndTime(fsDto, new ClassCodeInfoDto()
                                {
                                    CountryCode = record.CountryCode,
                                    RetentionType = record.RetentionType,
                                    TermId = record.TermId,
                                    StartDate = record.StartDate
                                });
                            }
                            lock (FSJobCache.Instance.ContainerLevelClassCodeCacheLock)
                            {
                                if (!FSJobCache.Instance.ContainerLevelClassCodeCache.ContainsKey(record.NodeId))
                                {
                                    FSJobCache.Instance.ContainerLevelClassCodeCache.Add(record.NodeId, new ClassCodeInfoDto()
                                    {
                                        ClassCode = record.ClassCode,
                                        CountryCode = record.CountryCode,
                                        RetentionType = record.RetentionType,
                                        TermId = record.TermId,
                                        StartDate = record.StartDate,
                                        EndTime = record.EndTime,
                                        PolicyValueNumber = ruleHasModified ? fsDto.PolicyValueNumber : record.PolicyValueNumber,
                                        PolicyValueUnit = ruleHasModified ? fsDto.PolicyValueUnit : record.PolicyValueUnit,
                                        CollectionTime = record.CollectionTime,
                                    });
                                }
                            }
                        }
                    }
                    else
                    {
                        if (FSJobCache.Instance.ContainerLevelClassCodeCache.ContainsKey(ds.ParentId) && FSJobCache.Instance.EnableJPMC)
                        {
                            var classCodeDto = FSJobCache.Instance.ContainerLevelClassCodeCache[ds.ParentId];
                            lock (FSJobCache.Instance.ContainerLevelClassCodeCacheLock)
                            {
                                if (!FSJobCache.Instance.ContainerLevelClassCodeCache.ContainsKey(ds.SelfId))
                                {
                                    FSJobCache.Instance.ContainerLevelClassCodeCache.Add(ds.SelfId, new ClassCodeInfoDto()
                                    {
                                        ClassCode = classCodeDto.ClassCode,
                                        CountryCode = classCodeDto.CountryCode,
                                        RetentionType = classCodeDto.RetentionType,
                                        TermId = classCodeDto.TermId,
                                        StartDate = classCodeDto.StartDate,
                                        EndTime = classCodeDto.EndTime,
                                        PolicyValueNumber = classCodeDto.PolicyValueNumber,
                                        PolicyValueUnit = classCodeDto.PolicyValueUnit,
                                        CollectionTime = classCodeDto.CollectionTime,
                                    });
                                }
                            }
                        }
                    }
                }
            }
        }

        private void DispatchToDiscoverOrOverflow(List<Stub> stubs)
        {
            var writer = _channelProvider.DiscoverChannel.Writer;
            var overflowStack = _channelProvider.DiscoverOverflowStack;
            var overflowCount = 0;
            foreach (var stub in stubs)
            {
                if (writer.TryWrite(stub))
                {
                    _channelProvider.IncreaseDiscoveryCount();
                }
                else
                {
                    overflowStack.Push(stub);
                    overflowCount++;
                }
            }
            if (overflowCount > 0)
            {
                _logger.Warn("Channel full. Dispatched {0}/{1} items to overflow stack. Current stack size:{2}", overflowCount, stubs.Count, overflowStack.Count);
            }
        }

        private bool HasRunningJob(string url)
        {
            string sha1Url = RAEncodeUtil.EncryptBySHA1(url);
            if (FSJobCacheV2.Instance.RunningJobNodeUrlsContains(sha1Url))
                return true;
            return false;
        }

        private List<FileSystemRecordDto> QueryFileSystemRecords(List<Guid> ids)
        {
            List<FileSystemRecordDto> folderRecords = new List<FileSystemRecordDto>();
            try
            {
                List<FileSystemRecordDto> data = new List<FileSystemRecordDto>();
                for (int i = 0; i < ids.Count; i += 500)
                {
                    using (new AgentPerformanceScope("FSDicover.QueryFileSystemRecords", addToStatistics: true))
                    {
                        var tempIds = ids.Skip(i).Take(500).ToList();
                        data = JobContext.Current.ApiClient.QueryFileSystemRecords(FSJobCache.Instance.AveConnectionId.ToString(), tempIds);
                        _logger.Debug("Queried {0} file system records from db.", data?.Count ?? 0);
                        if (data != null && data.Count > 0)
                        {
                            folderRecords.AddRange(data);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Error("An error occurred while GetFSDBRecords. Error:{0}", e.ToString());
            }
            return folderRecords;
        }

        private List<FSFolderCacheDto> GetCurrentConnectionAllSettings()
        {
            using (new AgentPerformanceScope("FSDicover.GetCurrentConnectionAllSettings", addToStatistics: true))
            {
                var data = JobContext.Current.ApiClient.GetCurrentConnectionAllSettings(FSJobCache.Instance.RootPath);
                return data == null ? new List<FSFolderCacheDto>() : data;
            }
        }

        private bool FilterdIn(XFileInfoEx t, bool folderFailedInLastJob)
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
                        || t.CreationTimeUtc > FSJobCache.Instance.JobController.IncrementalStartTime
                        || t.LastAccessTimeUtc > FSJobCache.Instance.JobController.IncrementalStartTime)
                        || folderFailedInLastJob || FailedInLastJob(t);
                default:
                    _logger.Warn("The code shouldnt go this approach.");
                    return false;
            }
        }

        private bool FilterdIn(XDirectoryInfoEx t, Guid selfId, bool parentFailedInLastJob)
        {
            if (t.IsHidden) { return false; }
            switch (FSJobCache.Instance.JobController.JobType)
            {
                case FSJobType.UserFullJob:
                case FSJobType.RematchRuleFullJob:
                    return true;
                case FSJobType.IncrementalJob:
                    return (t.LastWriteTimeUtc > FSJobCache.Instance.JobController.IncrementalStartTime
                        || t.CreationTimeUtc > FSJobCache.Instance.JobController.IncrementalStartTime
                        || t.LastAccessTimeUtc > FSJobCache.Instance.JobController.IncrementalStartTime) || parentFailedInLastJob || FailedInLastJob(selfId);
                default:
                    _logger.Warn("The code shouldnt go this approach.");
                    return false;
            }
        }

        private bool FailedInLastJob(XFileInfoEx t)
        {
            var fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, t.HighName, t.LowName);
            var nodeId = fullPath.Substring(FSJobCache.Instance.RootPath.Length + 1).ToLowerInvariant().ToMd5();
            return FSJobCacheV2.Instance.LastJobFailedItemIdsContains(nodeId);

        }

        private bool FailedInLastJob(Guid selfId)
        {
            return FSJobCacheV2.Instance.LastJobFailedItemIdsContains(selfId);
        }
    }
}
