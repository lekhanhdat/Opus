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
using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Hybrid.Contract;
using AvePoint.Media.Storage;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using Newtonsoft.Json;
using RAFileSystem.FileSystem.BulkProcessing;
using RAFileSystem.FileSystem.Disposal.Utils;
using RAFileSystem.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Collector
{

    #region Test
    //public class TestCollector
    //{
    //    private readonly AveLogger logger = AveLogger.GetInstance(typeof(TestCollector));

    //    private readonly BatchController<FileSystemRecordDto> _batchController;
    //    private readonly BatchReportPoller _reportPoller;

    //    private readonly string _jobId = "test-job-id";
    //    private readonly JobType _jobType = JobType.FSDataSync;
    //    private IReportService<JMJobDetails> reportServcie;
    //    public TestCollector(string jobId, JobType jobType)
    //    {
    //        _jobId = jobId;
    //        _jobType = jobType;

    //        _batchController = new BatchController<FileSystemRecordDto>(jobId, jobType, FSBatchOperationType.DataCollection);
    //        _reportPoller = new BatchReportPoller();
    //        Initialize();
    //    }

    //    public void Initialize()
    //    {
    //        try
    //        {
    //            JobContext.Current.mProgressManager.Create().IncreaseBase(3);
    //            reportServcie = JobContext.Current.JobDetailManager.Create();

    //            HybridApiClient.Instance.StartQueueListenerAsync(_jobId, _jobType);
    //            _batchController.Start();
    //            _reportPoller.InitiatePolling(_batchController.UploadedBatchIdReader, _jobId);
    //        }
    //        catch (Exception e)
    //        {
    //            logger.Error("Initialization failed. {0}", e);
    //        }
    //    }

    //    private Task reportTask;

    //    public async Task TestCollection(string rootPath, string collectFolderPath)
    //    {
    //        Stopwatch stopwatch = Stopwatch.StartNew();
    //        List<FileSystemCollectionFolder> items = new List<FileSystemCollectionFolder>();
    //        rootPath = @"\\172.29.20.27\C$\Test FS";
    //        var collector = new FileSystemCollector(rootPath); // root path
    //        try
    //        {
    //            reportTask = Task.Run(() => HandleBatchResults());
    //            _ = collector.StartAsync(collectFolderPath).ConfigureAwait(false);
    //            foreach (var item in collector.CollectAsync())
    //            {
    //                var fullName = item.FullPath;
    //                var parentPath = item.ParentPath;
    //                if (item.Level == FileSystemLevel.Folder)
    //                {
    //                    var folderInfo = AssembleFolderBasicInfo(item);
    //                    folderInfo.CreateDate = Convert.ToInt32(folderInfo.TimeCreated1.ToString("yyyyMMdd"));
    //                    _batchController.AddItem(folderInfo);
    //                    var test = false;
    //                    if (test)
    //                    {
    //                        break;
    //                    }
    //                }

    //                if (item.Level == FileSystemLevel.File)
    //                {
    //                    // assume process something and result is RecordDto of that file here
    //                    var fileInfo = AssembleFileBasicInfo(item);
    //                    fileInfo.CreateDate = Convert.ToInt32(fileInfo.TimeCreated1.ToString("yyyyMMdd"));
    //                    _batchController.AddItem(fileInfo);
    //                    var test = false;
    //                    if (test)
    //                    {
    //                        break;
    //                    }
    //                }
    //            }
    //        }
    //        catch (Exception e)
    //        {
    //            logger.Error("TestCollection failed. {0}", e);
    //        }
    //        finally
    //        {
    //            collector.Dispose();
    //            _batchController.Dispose();
    //        }
    //        stopwatch.Stop();

    //        if (reportTask != null)
    //        {
    //            await reportTask;
    //        }

    //        _reportPoller.Dispose();

    //        HybridApiClient.Instance.DisposeQueueListener(_jobId);

    //        logger.Info($"Finish cancel queue listener. Processing batch report...");

    //        reportServcie.CommitBatch(reports);

    //        if (items.Count > 0)
    //        {
    //            logger.Info($"Total collected items: {items.Count}, Time taken: {stopwatch.Elapsed.TotalSeconds} seconds.");
    //            //while (true)
    //            //{
    //            //    Thread.Sleep(10000);
    //            //}
    //        }
    //    }

    //    //private List<FSItemReportDto> testReportList = new List<FSItemReportDto>();
    //    private List<FSDataSyncJobReportDetail> reports = new List<FSDataSyncJobReportDetail>();
    //    public async Task HandleBatchResults()
    //    {
    //        try
    //        {
    //            foreach (var batchResult in _reportPoller.CollectBatchReports())
    //            {
    //                if (batchResult.BatchStatus == JobDetailsStatus.Failed)
    //                {
    //                    logger.Error($"Batch job failed for MessageId: {batchResult.MessageId}, Error: {batchResult.ErrorMessage}");
    //                }
    //                // just use the result record list if dont care about the batch state
    //                foreach (var record in batchResult.Records)
    //                {
    //                    lock (reports)
    //                    {
    //                        var report = new FSDataSyncJobReportDetail()
    //                        {
    //                            AgentName = OSInformation.HostName,
    //                            ObjectName = record.ObjectName,
    //                            FullPath = record.OriginalFullPath,
    //                            Status = record.Status,
    //                            Comment = record.ErrorMessage,
    //                        };

    //                        reports.Add(report);
    //                    }
    //                    // Todo: Process add report for each record here
    //                }
    //            }
    //        }
    //        catch (Exception)
    //        {
    //            //throw;
    //        }
    //    }

    //    private FileSystemRecordDto AssembleFileBasicInfo(FileSystemCollectionFolder stub)
    //    {
    //        FileSystemRecordDto record = new FileSystemRecordDto();
    //        try
    //        {
    //            XFileInfoEx xObj = new XFileInfoEx(stub.CurrentItem);
    //            //record.ScopeId = FSJobCache.Instance.RootPath.ToLowerInvariant().ToMd5();
    //            //record.AveSiteId = FSJobCache.Instance.AveConnectionId.ToString();
    //            // record.CreateDate = DateTime.UtcNow.Ticks;
    //            if (xObj.Owner.Contains('\\'))
    //            {
    //                var splitCreateBy = xObj.Owner.Split('\\');
    //                var createByName = splitCreateBy[1];
    //                if (createByName.Any(char.IsUpper) && createByName.Any(char.IsLower))
    //                {
    //                    record.CreatedBy = xObj.Owner;
    //                }
    //                else
    //                {
    //                    record.CreatedBy = string.Join("\\", splitCreateBy[0], createByName.ToLower());
    //                }
    //            }
    //            else
    //            {
    //                record.CreatedBy = xObj.Owner;
    //            }
    //            record.DirPath = Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(stub.FullPath);
    //            record.ExtensionForFile = Alphaleonis.Win32.Filesystem.Path.GetExtension(xObj.FileFullPath).TrimStart(new char[] { '.' });
    //            //record.FolderId = stub.ParentId;
    //            record.FullPath = stub.FullPath;
    //            var itemId = stub.FullPath.ToMd5();
    //            record.ItemId = itemId; //stub.SelfId;
    //            record.LeafName = xObj.Name;
    //            record.NodeId = itemId; //stub.SelfId;
    //            record.NodeType = (int)NodeLevel.FSFile;
    //            record.SourceFlag = (int)SourceFlag.FileSystem;
    //            record.TimeCreated1 = xObj.CreationTimeUtc;
    //            record.TimeLastModified = xObj.LastWriteTimeUtc.Ticks;
    //            record.SortTicks = Snowflake.Instance().GetTicks();
    //            RecordMetaInfo metaInfo = new RecordMetaInfo
    //            {
    //                FileSize = xObj.FileSize,
    //                LastAccessTime = xObj.LastAccessTimeUtc.Ticks,
    //                Owner = xObj.Owner,
    //                LocalFullPath = xObj.FileFullPath
    //            };
    //            record.FileSize = xObj.FileSize;
    //            //record.ParentId = stub.ParentId;
    //            record.MetaInfo = JsonConvert.SerializeObject(metaInfo);

    //            record.BulkImportEnabled = true;
    //            //record.FSJobType = FSJobType.UserFullJob;
    //        }
    //        catch (Exception ex)
    //        {
    //            logger.Error("Failed to assemble the basic info of the file/folder. Exception:{0}", ex.ToString());
    //            throw;
    //        }

    //        return record;
    //    }

    //    private FileSystemRecordDto AssembleFolderBasicInfo(FileSystemCollectionFolder stub)
    //    {
    //        FileSystemRecordDto record = new FileSystemRecordDto();
    //        try
    //        {

    //            XDirectoryInfoEx xObj = new XDirectoryInfoEx(stub.CurrentItem);
    //            //record.AveSiteId = FSJobCache.Instance.AveConnectionId.ToString();
    //            //record.CollectionTime = DateTime.UtcNow.Ticks;
    //            if (xObj.Owner.Contains('\\'))
    //            {
    //                var splitCreateBy = xObj.Owner.Split('\\');
    //                var createByName = splitCreateBy[1];
    //                if (createByName.Any(char.IsUpper) && createByName.Any(char.IsLower))
    //                {
    //                    record.CreatedBy = xObj.Owner;
    //                }
    //                else
    //                {
    //                    record.CreatedBy = string.Join("\\", splitCreateBy[0], createByName.ToLower());
    //                }
    //            }
    //            else
    //            {
    //                record.CreatedBy = xObj.Owner;
    //            }
    //            record.DirPath = Alphaleonis.Win32.Filesystem.Path.GetDirectoryName(stub.FullPath);
    //            //record.FolderId = stub.ParentId;
    //            record.FullPath = stub.FullPath;
    //            var itemId = stub.FullPath.ToMd5();
    //            record.ItemId = itemId;
    //            record.ItemRowId = -1;
    //            record.LeafName = xObj.Name;
    //            record.ListId = Guid.Empty;
    //            record.NodeId = itemId;
    //            record.NodeType = (int)NodeLevel.FSFolder;
    //            //record.ScopeId = FSJobCache.Instance.RootPath.ToLowerInvariant().ToMd5();
    //            record.SourceFlag = (int)SourceFlag.FileSystem;
    //            record.TimeCreated1 = xObj.CreationTimeUtc;
    //            record.TimeLastModified = xObj.LastWriteTimeUtc.Ticks;
    //            //record.ParentId = stub.ParentId;
    //            record.SortTicks = Snowflake.Instance().GetTicks();
    //            RecordMetaInfo metaInfo = new RecordMetaInfo
    //            {
    //                FileSize = xObj.Length,
    //                LocalFullPath = xObj.LocalFullPath
    //            };
    //            record.FileSize = xObj.Length;
    //            record.MetaInfo = JsonConvert.SerializeObject(metaInfo);
    //        }
    //        catch (Exception ex)
    //        {
    //            logger.Error("Failed to assemble the basic info of the folder. Exception:{0}", ex.ToString());
    //            throw;
    //        }

    //        return record;
    //    }
    //}
    #endregion

    public class FileSystemCollector : IDisposable
    {
        private readonly AveLogger logger = AveLogger.GetInstance(typeof(FileSystemCollector));

        private readonly string rootPath;
        private readonly int minWorkers;
        private readonly int maxWorkers;
        private readonly IFileSystemFilter filter;

        private readonly Channel<FileSystemCollectionFolder> collectionChannel;
        private readonly Channel<FileSystemCollectionFolder> resultChannel;
        private readonly CancellationTokenSource cts = new CancellationTokenSource();

        private IXSystem xSystem;
        private volatile int pendingDirectoryCount;
        private volatile bool started;

        private readonly FileSystemWorkerPoolController workerController;
        private bool disposed;
        public const int defaultMinWorkers = 4;
        private const int defaultMaxWorkers = 16;

        public FileSystemCollector(string rootPath) : this(rootPath, new FileSystemDefaultFilter()) { }

        public FileSystemCollector(string rootPath, IFileSystemFilter filter) : this(rootPath, filter, defaultMinWorkers, defaultMaxWorkers) { }

        public FileSystemCollector(string rootPath, IFileSystemFilter filter, int minWorkers, int maxWorkers)
        {
            if (string.IsNullOrWhiteSpace(rootPath))
                throw new ArgumentNullException(nameof(rootPath));

            this.rootPath = rootPath;
            this.minWorkers = minWorkers <= 0 ? 1 : minWorkers;
            this.maxWorkers = Math.Max(this.minWorkers, maxWorkers);
            this.filter = filter ?? new FileSystemDefaultFilter();

            collectionChannel = Channel.CreateUnbounded<FileSystemCollectionFolder>();
            resultChannel = Channel.CreateUnbounded<FileSystemCollectionFolder>(); // single reader true ? 
            workerController = new FileSystemWorkerPoolController(
                ProcessLoopAsync,
                this.minWorkers,
                this.maxWorkers,
                () => pendingDirectoryCount);
        }

        public void UseNewCollector()
        {
            workerController.UseNewWorkerLoop(ProcessLoopV2Async);
        }

        /// <summary>
        /// Opens IXSystem for root and starts collection from the specified folder path or root if not specified.
        /// </summary>
        /// <param name="folderPath">specified folder path in the root</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task StartAsync(string folderPath = "", CancellationToken cancellationToken = default)
        {
            if (started) return;
            started = true;

            try
            {
                xSystem = ExternalUtil.OpenXSystem(rootPath);
                logger.Info("XSystem opened. Location={0}", xSystem.SystemLocation.LogBase64());

                var startFolder = folderPath.StartsWith(rootPath) ? folderPath.Substring(rootPath.Length) : folderPath;

                StorageInfo dirInfo = new StorageInfo() { HighName = startFolder };

                if (!xSystem.DirectoryExists(dirInfo))
                {
                    logger.Warn($"The specified folder path does not exist. RootPath:{rootPath.LogBase64()}, folderPath: {folderPath.LogBase64()}.");
                    CompleteChannels();
                    return;
                }

                var dir = xSystem.OpenDirectory(dirInfo, FileMode.Open);

                var workItem = new FileSystemCollectionFolder
                {
                    CurrentItem = dir,
                    Level = FileSystemLevel.Folder,
                    FullPath = dir.OriginalDirFullPath,
                    ParentPath = dir.ParentFullName,
                    Depth = 0
                };

                // add first directory.
                Interlocked.Increment(ref pendingDirectoryCount);
                //if (!filter.ShouldDiscoverDirectory(workItem.CurrentItem))
                //{
                //    logger.Info("The root folder is skipped by filter policy. RootPath:{0}, folderPath: {1}.", rootPath, folderPath);
                //    Interlocked.Decrement(ref pendingDirectoryCount);
                //    CompleteChannels();
                //    return;
                //}
                await collectionChannel.Writer.WriteAsync(workItem, cancellationToken).ConfigureAwait(false);
                workerController.Start();
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to start FileSystemCollector. RootPath:{rootPath.LogBase64()}, folderPath: {folderPath.LogBase64()}. Error={ex}");
                CompleteChannels();
                throw;
            }
        }

        public IEnumerable<FileSystemCollectionFolder> CollectAsync(CancellationToken cancellationToken = default)
        {
            var spin = new SpinWait();

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (resultChannel.Reader.TryRead(out var item))
                {
                    spin.Reset();
                    yield return item;
                    continue;
                }

                if (resultChannel.Reader.Completion.IsCompleted)
                {
                    while (resultChannel.Reader.TryRead(out item))  // ensure all items are read
                    {
                        yield return item;
                    }
                    yield break;
                }

                spin.SpinOnce();
            }
        }


        /// <summary>
        /// Internal per-worker loop consuming directory work items.
        /// </summary>
        private async Task ProcessLoopAsync(int workerId, CancellationToken cancellationToken)
        {
            while (await collectionChannel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                while (collectionChannel.Reader.TryRead(out var workItem))
                {
                    workerController.MarkBusy(workerId, true);
                    try
                    {
                        await ProcessDirectoryAsync(workItem, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error processing directory {0}. Error={1}", workItem?.FullPath.LogBase64(), ex.Message);
                        // Continue;
                    }
                    finally
                    {
                        if (Interlocked.Decrement(ref pendingDirectoryCount) == 0)
                        {
                            CompleteChannels();
                        }
                        workerController.MarkBusy(workerId, false);
                    }
                }
            }
        }
        
        private async Task ProcessLoopV2Async(int workerId, CancellationToken cancellationToken)
        {
            while (await collectionChannel.Reader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                while (collectionChannel.Reader.TryRead(out var workItem))
                {
                    workerController.MarkBusy(workerId, true);
                    try
                    {
                        await ProcessDirectoryV2Async(workItem, cancellationToken).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        logger.Error("Error processing directory {0}. Error={1}", workItem?.FullPath.LogBase64(), ex.Message);
                        // Continue;
                    }
                    finally
                    {
                        if (Interlocked.Decrement(ref pendingDirectoryCount) == 0)
                        {
                            CompleteChannels();
                        }
                        workerController.MarkBusy(workerId, false);
                    }
                }
            }
        }

        private async Task ProcessDirectoryAsync(FileSystemCollectionFolder workItem, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool isAddResult = false;
            try
            {
                var fileCollection = new FileSystemFileCollector(xSystem, workItem.CurrentItem, workItem.FullPath).Collect();
                var folderCollection = new FileSystemFolderCollector(xSystem, workItem.CurrentItem, workItem.FullPath).Collect();

                if (filter.ShouldIncludeDirectory(workItem.CurrentItem))
                {
                    await ProcessFilesAsync(fileCollection, workItem).ConfigureAwait(false);

                    workItem.FinishTime = DateTime.UtcNow.Ticks;
                    await resultChannel.Writer.WriteAsync(workItem);
                    isAddResult = true;
                }

                var subLevel = workItem.Depth + 1;
                await ProcessFoldersAsync(folderCollection, subLevel).ConfigureAwait(false);

                return;
            }
            catch (FileSystemCollectorException e)
            {
                logger.Warn($"FileSystemCollectorException for: {workItem?.FullPath.LogBase64()}. Exception: {e}");
                workItem.Status = JobDetailsStatus.Failed;
                workItem.ErrorMessage = e.I18nMessageKey;
            }
            catch (Exception e)
            {
                logger.Error("ProcessDirectory failed for {0}. {1}", workItem?.FullPath.LogBase64(), e);
                workItem.Status = JobDetailsStatus.Failed;
                workItem.ErrorMessage = e.Message;
            }

            if (!isAddResult && filter.ShouldIncludeDirectory(workItem.CurrentItem))
            {
                workItem.FinishTime = DateTime.UtcNow.Ticks;
                await resultChannel.Writer.WriteAsync(workItem);
            }
        }
        private bool CheckCurrentFolderIsBreakInherit(string fullPath)
        {
            return DisposalFilterHelper.IsBreakInheritNode(fullPath.ToLowerInvariant());
        }
        private async Task ProcessDirectoryV2Async(FileSystemCollectionFolder workItem, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool isAddResult = false;
            try
            {
                var folderCollection = new FileSystemFolderCollector(xSystem, workItem.CurrentItem, workItem.FullPath);

                if (filter.ShouldIncludeDirectory(workItem.CurrentItem) && !CheckCurrentFolderIsBreakInherit(workItem.FullPath))
                {
                    workItem.ChildrenCollector =
                        new FileSystemFileCollector(xSystem, workItem.CurrentItem, workItem.FullPath, workItem.Depth + 1, filter);

                    workItem.FinishTime = DateTime.UtcNow.Ticks;
                    await resultChannel.Writer.WriteAsync(workItem);
                    isAddResult = true;
                }
                else
                {
                    logger.Info($"current node has break inherit,will skip discover it,fullpath:{workItem.FullPath}");
                }
                if (isAddResult)
                {
                    var subLevel = workItem.Depth + 1;
                    await ProcessFoldersAsync(folderCollection, subLevel).ConfigureAwait(false);
                }
                return;
            }
            catch (FileSystemCollectorException e)
            {
                logger.Warn($"FileSystemCollectorException for: {workItem?.FullPath.LogBase64()}. Exception: {e}");
                workItem.Status = JobDetailsStatus.Failed;
                workItem.ErrorMessage = e.I18nMessageKey;
            }
            catch (Exception e)
            {
                logger.Error("ProcessDirectory failed for {0}. {1}", workItem?.FullPath.LogBase64(), e);
                workItem.Status = JobDetailsStatus.Failed;
                workItem.ErrorMessage = e.Message;
            }

            if (!isAddResult && filter.ShouldIncludeDirectory(workItem.CurrentItem))
            {
                workItem.FinishTime = DateTime.UtcNow.Ticks;
                await resultChannel.Writer.WriteAsync(workItem);
            }
        }

        private async Task ProcessFoldersAsync(List<XDirectoryInfo> folderCollection, long deepLevel)
        {
            foreach (var subDir in folderCollection)
            {
                try
                {
                    if (!filter.ShouldDiscoverDirectory(subDir))
                    {
                        logger.Info($"The folder is skipped by filter policy. FolderPath:{subDir.FullName}, DeepLevel: {deepLevel}");
                        continue;
                    }
                    var subWorkItem = new FileSystemCollectionFolder
                    {
                        CurrentItem = subDir,
                        Level = FileSystemLevel.Folder,
                        FullPath = Path.Combine(subDir.OriginalDirFullPath, subDir.Name),
                        ParentPath = subDir.ParentFullName,
                        Depth = deepLevel
                    };
                    await collectionChannel.Writer.WriteAsync(subWorkItem).ConfigureAwait(false);
                    Interlocked.Increment(ref pendingDirectoryCount);
                }
                catch (Exception e)
                {
                    logger.Error("Enqueue sub-directory failed for {0}. {1}", subDir?.FullName.LogBase64(), e);
                }
            }
        }
        
        private async Task ProcessFoldersAsync(FileSystemFolderCollector folderCollection, long deepLevel)
        {
            foreach (var subDirs in folderCollection.CollectInBatch())
            {
                foreach (var subDir in subDirs)
                {
                    try
                    {
                        if (!filter.ShouldDiscoverDirectory(subDir))
                        {
                            logger.Info($"The folder is skipped by filter policy. FolderPath:{subDir.FullName}, DeepLevel: {deepLevel}");
                            continue;
                        }
                        var subWorkItem = new FileSystemCollectionFolder
                        {
                            CurrentItem = subDir,
                            Level = FileSystemLevel.Folder,
                            FullPath = Path.Combine(subDir.OriginalDirFullPath, subDir.Name),
                            ParentPath = subDir.ParentFullName,
                            Depth = deepLevel
                        };
                        await collectionChannel.Writer.WriteAsync(subWorkItem).ConfigureAwait(false);
                        Interlocked.Increment(ref pendingDirectoryCount);
                    }
                    catch (Exception e)
                    {
                        logger.Error("Enqueue sub-directory failed for {0}. {1}", subDir?.FullName.LogBase64(), e);
                    }
                }
            }
        }

        private async Task ProcessFilesAsync(List<XFileInfo> fileCollection, FileSystemCollectionFolder workItem)
        {
            foreach (var file in fileCollection)
            {
                try
                {
                    if (!filter.ShouldIncludeFile(file)) continue;

                    //var subWorkItem = new FileSystemCollectionFolder
                    //{
                    //    CurrentItem = file,
                    //    Level = FileSystemLevel.File,
                    //    FullPath = file.OriginalFileFullPath,
                    //    ParentPath = file.ParentFullName,
                    //    FinishTime = DateTime.UtcNow.Ticks,
                    //};
                    //await resultChannel.Writer.WriteAsync(subWorkItem).ConfigureAwait(false);

                    workItem.ChildrenFiles.Add(file);
                }
                catch (Exception e)
                {
                    logger.Error("Write result failed for file {0}. {1}", file?.FullName.LogBase64(), e);
                }
            }

            workItem.ChildrenFilesCount = workItem.ChildrenFiles.Count;
        }

        private void CompleteChannels()
        {
            collectionChannel.Writer.TryComplete();
            resultChannel.Writer.TryComplete();
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true;

            cts.Cancel();
            workerController.Dispose();

            try
            {
                if (xSystem != null)
                {
                    xSystem.Close();
                    xSystem.Dispose();
                }
            }
            catch (Exception e)
            {
                logger.Error("Error disposing XSystem: {0}", e.Message);
            }

            CompleteChannels();
            cts.Dispose();
        }
    }
}
