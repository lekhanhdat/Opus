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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using RAFileSystem.Disposal;
using RAFileSystem.FileSystem.Collector;
using RAFileSystem.FileSystem.Disposal.DisposalExecutionStrategies;
using RAFileSystem.FileSystem.Disposal.NewLogic.V3.Services;
using RAFileSystem.FileSystem.FileSystem.Collector.FilterImplement;

namespace RAFileSystem.FileSystem.Disposal.NewLogic.V3.Workers
{
    /// <summary>
    /// Orchestrates the disposal discovery pipeline.
    /// Discovers files/folders and delegates analysis to specialized services.
    /// </summary>
    public class DisposalDiscoveryWorkerV3 : IFSDisposalWorker
    {
        private readonly AveLogger _logger = AveLogger.GetInstance(typeof(DisposalDiscoveryWorkerV3));
        private readonly DisposalFileAnalyzer _fileAnalyzer;
        private readonly DisposalReportService _reportService;

        public DisposalDiscoveryWorkerV3(DisposalReportService reportService, DisposalFileAnalyzer fileAnalyzer)
        {
            _reportService = reportService;
            _fileAnalyzer = fileAnalyzer;
        }

        public async Task RunAsync()
        {
            try
            {
                _logger.Info("Start disposal discovery worker.");

                if (DisposalExecutionStrategyV3.ClassificationLevel != NodeLevel.FSFolder)
                {
                    await RunFromFileLevelClassifyAsync();
                }
                else
                {
                    await RunFromFolderLevelClassifyAsync();
                }

                _logger.Info("Disposal discovery worker completed.");
            }
            catch (Exception ex)
            {
                _logger.Error("An error occurred in disposal discovery worker. Error: {0}", ex);
                throw;
            }
            finally
            {
                try
                {
                    _fileAnalyzer.FinalAddRejectFileToAzureTable();
                    _fileAnalyzer.FinalRemoveManualData();
                    _fileAnalyzer.Finish();
                }
                catch (Exception e)
                {
                    _logger.Error("An error occurred while final update. Error:{0}", e.ToString());
                }
            }
        }

        private async Task RunFromFolderLevelClassifyAsync()
        {
            using (new AgentPerformanceScope( "FSDiscover.Process.Folders"))
            {
                _logger.Info("Run from folder level");

                var filter = new FileSystemFolderDisposalFilter(FSJobCache.Instance.BreakNodeUrls,
                    FSJobCache.Instance.RunningJobNodeUrls, FSJobCache.Instance.ScopeSettingCache);

                var collector = new FileSystemCollector(FSJobCache.Instance.RootPath, filter);
                
                collector.UseNewCollector();

                _ = collector.StartAsync(FSJobCache.Instance.RunJobScopePath).ConfigureAwait(false);

                foreach (var folder in collector.CollectAsync())
                {
                    var fullPath = folder.FullPath;
                    var folderId = fullPath.ToLowerInvariant().ToMd5();
                    try
                    {
                        foreach (var files in folder.ChildrenCollector.CollectInBatch())
                        {
                            FSFolderStub rootStub = new FSFolderStub
                            {
                                MediaObj = folder.CurrentItem,
                                FullPath = fullPath,
                                SelfId = fullPath.ToLowerInvariant().ToMd5(),
                                //ParentId = parentId, 
                                ScopeSettingId = FSJobCache.Instance.DispoalSettingScopeId
                            };
                            var azureRecords = await FetchRecordsAsync(folderId, files.Select(f => f.SelfId).ToList());
                            await _fileAnalyzer.AnalyzeFilesFromFolderAsync(files, rootStub, azureRecords.ToDictionary(r => r.NodeId));
                        }
                    }
                    catch (Exception itemex)
                    {
                        _logger.Error("Failed to process item. Object:{0}, Exception:{1}", folder.FullPath,
                            itemex.ToString());
                        _reportService.ReportFailedFolder(folder);
                    }
                }

                collector.Dispose();
            }
        }


        private async Task RunFromFileLevelClassifyAsync()
        {
            var filter = BuildDisposalFilter();

            var collectorForCount = new FileSystemCollector(FSJobCache.Instance.RootPath, filter);
            collectorForCount.UseNewCollector();
            _ = collectorForCount.StartAsync(FSJobCache.Instance.RunJobScopePath).ConfigureAwait(false);
            foreach (var folder in collectorForCount.CollectAsync())
            {
                try
                {
                    var totalFileCount = folder.ChildrenCollector.GetFilesCount();
                    if (totalFileCount > 0)
                    {
                        _reportService.IncreaseProgressBase(totalFileCount);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to process item1. Object:{0}, Exception:{1}", folder.FullPath, ex.ToString());
                }
            }
            collectorForCount.Dispose();
            var collector = new FileSystemCollector(FSJobCache.Instance.RootPath, filter);
            collector.UseNewCollector();
            _ = collector.StartAsync(FSJobCache.Instance.RunJobScopePath).ConfigureAwait(false);
            foreach (var folder in collector.CollectAsync())
            {
                try
                {
                    string fullPath = folder.FullPath;
                    var folderId = fullPath.ToLowerInvariant().ToMd5();
                    foreach (var files in folder.ChildrenCollector.CollectInBatch())
                    {
                        _logger.Info("Processing folder:{0}, files count in batch:{1}", fullPath, files.Count);
                        var azureRecords = await FetchRecordsAsync(folderId, files.Select(f => f.SelfId).ToList());
                        await _fileAnalyzer.AnalyzeFiles(folder, files, azureRecords.ToDictionary(r => r.NodeId));
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("Failed to process item. Object:{0}, Exception:{1}", folder.FullPath, ex.ToString());
                    _reportService.ReportFailedFolder(folder);
                }
            }
            collector.Dispose();

        }

        private IFileSystemFilter BuildDisposalFilter()
        {
            if (IsClassCodeDisposalJob())
            {
                _logger.Info("Class code disposal detected. Using folder-level filter to include all subfolders.");
                return new FileSystemFolderDisposalFilter(
                    FSJobCache.Instance.BreakNodeUrls,
                    FSJobCache.Instance.RunningJobNodeUrls,
                    FSJobCache.Instance.ScopeSettingCache);
            }

            var validFolderPaths = FSJobCache.Instance.DisposalFolderCache.TakeAll().Select(i => i.FolderPath);
            return new FileSystemFileDisposalFilter(validFolderPaths);
        }

        private static bool IsClassCodeDisposalJob()
        {
            var classCodeIds = FSJobCache.Instance.ClassCodeIds;
            return classCodeIds != null && classCodeIds.Count > 0;
        }

        private Task<List<FileSystemRecordDto>> FetchRecordsAsync(Guid folderId, List<Guid> nodeIds)
        {
            var classCodeIds = FSJobCache.Instance.ClassCodeIds;
            if (classCodeIds != null && classCodeIds.Count > 0)
            {
                _logger.Info("ClassCode disposal: fetching records for folder:{0}, nodeIds count:{1}, classCodeIds count:{2}",
                    folderId, nodeIds.Count, classCodeIds.Count);
                return GetDBRecordsByClassCodeAndNodeIdsAsync(folderId, nodeIds, classCodeIds);
            }

            return GetDBRecordsByNodeIdsAsync(folderId, nodeIds);
        }

        public async Task<List<FileSystemRecordDto>> GetDueRecordsInFolderAsync(Guid folderId, IEnumerable<Guid> fileSelfIds)
        {
            using (new AgentPerformanceScope("FSDisposal.GetDueRecordsInFolder", addToStatistics: true))
            {
                var wrapper = FileSystemLiteDBWrapper.CreateInstance(
                    ReportUtil.GetDisposalDueRecordDBPath(JobContext.Current.JobId));
                var records =await Task.Run(() =>wrapper.QueryBySelfIds(fileSelfIds));
                _logger.Info("Get due records in folder:{0} finished. Count:{1}", folderId, records.Count);
                return records;
            }
        }
        
        public async Task<List<FileSystemRecordDto>> GetDBRecordsByNodeIdsAsync(Guid folderId, List<Guid> nodeIds)
        {
            using (new AgentPerformanceScope("FSDiscover.GetAzureDataByFolder", addToStatistics: true))
            {
                var sortTicks = DateTime.MinValue.Ticks;
                var connectionId = FSJobCache.Instance.RootPath.ToLowerInvariant().ToMd5().ToString();
                var records= await Task.Run(() => JobContext.Current.ApiClient.GetDBRecordsByNodeIds(nodeIds, connectionId, sortTicks));
                _logger.Info("Get azure records in folder:{0} finished. Count:{1}", folderId, records.Count);
                return records;
            }
        }
        public async Task<List<FileSystemRecordDto>> GetDBRecordsByClassCodeAndNodeIdsAsync(Guid folderId, List<Guid> nodeIds, List<Guid> classCodeIds)
        {
            using (new AgentPerformanceScope("FSDiscover.GetAzureDataByClassCode", addToStatistics: true))
            {
                var sortTicks = DateTime.MinValue.Ticks;
                var connectionId = FSJobCache.Instance.RootPath.ToLowerInvariant().ToMd5().ToString();
                var records = await JobContext.Current.ApiClient.GetDBRecordsByClassCodeAndFilterByEndTimeAsync(
                    nodeIds, classCodeIds, connectionId, sortTicks);
                _logger.Info("Get azure records by class code in folder:{0} finished. Queried:{1}, Returned:{2}",
                    folderId, nodeIds.Count, records?.Count ?? 0);

                if ((records == null || records.Count == 0) && nodeIds.Count > 0)
                {
                    _logger.Warn("No class code records returned for folder:{0}. " +
                                 "Verify files have class code TermId assigned in server DB. " +
                                 "ScopeId used:{1}, ClassCodeIds:{2}",
                        folderId,
                        connectionId,
                        string.Join(",", classCodeIds));
                }

                return records ?? new List<FileSystemRecordDto>();
            }
        }
    }
}
