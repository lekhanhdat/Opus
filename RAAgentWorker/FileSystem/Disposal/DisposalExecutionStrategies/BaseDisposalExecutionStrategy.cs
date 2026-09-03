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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Media.Storage;
using AvePoint.RA.Common.Hybrid;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.Disposal;
using RAFileSystem.FileSystem.BaseProcessor;
using RAFileSystem.FileSystem.Disposal.Utils;
using System.Linq;

namespace RAFileSystem.FileSystem.Disposal.DisposalExecutionStrategies
{
    public abstract class BaseDisposalExecutionStrategy
    {
        private AveLogger _logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        protected void GetAllRecords(bool OnlyQueryFolder = false)
        {
            using (new AgentPerformanceScope("FSDisposal.GetAllRecords", addToStatistics: true))
            {
                SearchFilterParam searchFilterParam;
                using (new AgentPerformanceScope("FSDisposal.Init", addToStatistics: true))
                {
                    searchFilterParam = AssembleQueryDto(OnlyQueryFolder);
                }
                if (OnlyQueryFolder)
                {
                    searchFilterParam.DueDate = 0;
                }
                int index = 0;
                int totalCount = 0;
                do
                {
                    using (new AgentPerformanceScope("FSDisposal.QuerybyPage", addToStatistics: true))
                    {
                        var result = JobContext.Current.ApiClient.GetFSDueRecords(searchFilterParam);
                        if (result != null)
                        {
                            searchFilterParam.PageInfo.HasNextPage = !string.IsNullOrEmpty(result?.PageInfo?.PageIndex);
                            searchFilterParam.PageInfo.PageIndex = result?.PageInfo?.PageIndex;
                            int resultCount = result.Records != null ? result.Records.Count : 0;
                            totalCount += resultCount;
                            index++;
                            _logger.Info($"query for {index} times, result count:{resultCount}, has next page:{searchFilterParam.PageInfo.HasNextPage}");
                            SavePagingResult(result.Records);
                        }
                        else
                        {
                            _logger.Warn("Query result is null");
                            break;
                        }
                    }
                }
                while (searchFilterParam.PageInfo.HasNextPage);
                _logger.Info("finish searching, total result count {0}", totalCount);
            }
        }

        private void SavePagingResult(List<FileSystemRecordDto> result)
        {
            if (result == null)
            {
                return;
            }
            using (new AgentPerformanceScope("FSDisposal.SavePagingResult", addToStatistics: true))
            {
                FileSystemLiteDBWrapper fileSystemSqliteWrapper = FileSystemLiteDBWrapper.CreateInstance(
                    ReportUtil.GetDisposalDueRecordDBPath(JobContext.Current.JobId));
                fileSystemSqliteWrapper.Insert(result);
            }
        }
        
        private SearchFilterParam AssembleQueryDto(bool OnlyQueryFolder = false)
        {
            var searchFilterParam = new SearchFilterParam
            {
                DataSource = (int)SourceFlag.FileSystem,
                ScopeId = FSJobCache.Instance.RootPath.ToLowerInvariant().ToMd5().ToString(),
                DueDate = JobContext.Current.JobStartTime.Ticks,
                PageInfo = new SearchPageInfo
                {
                    PageIndex = "",
                    PageSize = 100
                }
            };

            searchFilterParam.Filter = new SearchFilterInfo
            {
                NodeTypes = new List<int> { (int)NodeLevel.FSFile }
            };
            if (!FSJobCache.Instance.RunJobScopePath.Equals(FSJobCache.Instance.RootPath, StringComparison.OrdinalIgnoreCase))
            {
                if (OnlyQueryFolder)
                {
                    _logger.Info($"AssembleQueryDto OnlyQueryFolder is ture, FSJobCache.Instance.RunJobParentScopePath is :{FSJobCache.Instance.RunJobParentScopePath}");
                    searchFilterParam.Filter.SearchScope = FSJobCache.Instance.RunJobParentScopePath;
                }
                else
                {
                    searchFilterParam.Filter.SearchScope = FSJobCache.Instance.RunJobScopePath;
                }
            }

            return searchFilterParam;
        }
        
        protected List<FSDisposalDiscoverFolder> GetDisposalDiscoverFolders()
        {
            FileSystemLiteDBWrapper fileSystemSqliteWrapper = FileSystemLiteDBWrapper.CreateInstance(
                ReportUtil.GetDisposalDueRecordDBPath(JobContext.Current.JobId));
            if (fileSystemSqliteWrapper == null)
            {
                return null;
            }

            using (new AgentPerformanceScope("FSDisposal.GetDisposalDiscoverFolders", addToStatistics: true))
            {
                var folders = fileSystemSqliteWrapper.GetDisposalDiscoverFolders();
                var system = ExternalUtil.OpenXSystem(FSJobCache.Instance.RunJobScopePath);
                var validFolders = new List<FSDisposalDiscoverFolder>();
                foreach (var folder in folders)
                {
                    if (!IsValidDisposalFolder(folder, system))
                    {
                        continue;
                    }
                    validFolders.Add(folder);
                }
                return validFolders;
            }
        }
        
        protected List<FSDisposalDiscoverFolder> GetDisposalDiscoverFoldersV2()
        {
            FileSystemLiteDBWrapper fileSystemSqliteWrapper = FileSystemLiteDBWrapper.CreateInstance(
                ReportUtil.GetDisposalDueRecordDBPath(JobContext.Current.JobId));
            if (fileSystemSqliteWrapper == null)
            {
                return null;
            }

            using (new AgentPerformanceScope("FSDisposal.GetDisposalDiscoverFolders", addToStatistics: true))
            {
                var folders = fileSystemSqliteWrapper.GetDisposalDiscoverFoldersV2();
                var enabledNodeIds = HybridApiClient.Instance.ValidateEnableRecordManagementNodes(folders.Select(f => f.FolderId).ToList());
                if(enabledNodeIds != null || enabledNodeIds.Count > 0)
                {
                    folders = folders.Where(f => enabledNodeIds.Contains(f.FolderId)).ToList();
                }
                var system = ExternalUtil.OpenXSystem(FSJobCache.Instance.RunJobScopePath);
                var validFolders = new List<FSDisposalDiscoverFolder>();
                foreach (var folder in folders)
                {
                    if (!IsValidDisposalFolder(folder, system))
                    {
                        continue;
                    }
                    validFolders.Add(folder);
                }
                return validFolders;
            }
        }

        private bool IsValidDisposalFolder(FSDisposalDiscoverFolder folder, IXSystem system)
        {
            if (!folder.FolderPath.StartsWith(FSJobCache.Instance.RunJobScopePath, StringComparison.OrdinalIgnoreCase))
            {
                _logger.Info($"Folder is not under run job scope. id:{folder?.FolderId} Run job scope:{FSJobCache.Instance.RunJobScopePath}");
                return false;
            }
            if (DisposalFilterHelper.IsBreakInheritNode(folder.FolderPath.ToLowerInvariant()))
            {
                _logger.Debug("The folder node {0} has unique setting.", folder?.FolderId);
                return false;
            }
            if (FSJobCache.Instance.ScopeSettingCache.ContainsKey(folder.FolderId)
                && !FSJobCache.Instance.ScopeSettingCache[folder.FolderId].IsActive)
            {
                _logger.Debug("The folder node {0} has been deactivated.", folder?.FolderId);
                return false;
            }
            if (DisposalFilterHelper.HasRunningJob(folder.FolderPath.ToLowerInvariant()))
            {
                _logger.Debug("There is already a job running on this node. id:{0}", folder?.FolderId);
                return false;
            }
            if (!folder.FolderPath.Equals(FSJobCache.Instance.RunJobScopePath, StringComparison.OrdinalIgnoreCase))
            {
                StorageInfo info = new StorageInfo
                {
                    HighName = folder.FolderPath.Substring(
                        FSJobCache.Instance.RunJobScopePath.Length,
                        folder.FolderPath.Length - FSJobCache.Instance.RunJobScopePath.Length)
                };
                if (!system.DirectoryExists(info))
                {
                    _logger.Info($"Folder no longer exist. id:{folder?.FolderId}");
                    return false;
                }
            }
   
            return true;
        }
        
        protected List<FileSystemRecordDto> GetAllFolders(FSJobProcessorContext _context)
        {
            using (new AgentPerformanceScope("FSDisposal.GetAllDifferentTermFolders", addToStatistics: true))
            {
                var searchFilterParam = new SearchFilterParam
                {
                    TermId = _context.Setting.DefaultTermId,
                    DataSource = (int)SourceFlag.FileSystem,
                    ScopeId = FSJobCache.Instance.RootPath.ToLowerInvariant().ToMd5().ToString(),
                    PageInfo = new SearchPageInfo
                    {
                        PageIndex = "",
                        PageSize = 100
                    }
                };

                searchFilterParam.Filter = new SearchFilterInfo
                {
                    NodeTypes = new List<int> { (int)NodeLevel.FSFolder }
                };
                if (!FSJobCache.Instance.RunJobScopePath.Equals(FSJobCache.Instance.RootPath, StringComparison.OrdinalIgnoreCase))
                {
                    searchFilterParam.Filter.SearchScope = FSJobCache.Instance.RunJobScopePath;
                    searchFilterParam.FolderId = FSJobCache.Instance.RunJobScopePath.ToLowerInvariant().ToMd5();
                }

                var ret = new List<FileSystemRecordDto>();
                int index = 0;
                int totalCount = 0;
                do
                {
                    using (new AgentPerformanceScope("FSDisposal.QuerybyPage", addToStatistics: true))
                    {
                        var result = JobContext.Current.ApiClient.GetFSDueRecords(searchFilterParam);
                        if (result != null)
                        {
                            searchFilterParam.PageInfo.HasNextPage = !string.IsNullOrEmpty(result?.PageInfo?.PageIndex);
                            searchFilterParam.PageInfo.PageIndex = result?.PageInfo?.PageIndex;
                            int resultCount = result.Records != null ? result.Records.Count : 0;
                            totalCount += resultCount;
                            index++;
                            _logger.Info($"query for {index} times, result count:{resultCount}, has next page:{searchFilterParam.PageInfo.HasNextPage}");
                            ret.AddRange(result.Records);
                        }
                        else
                        {
                            _logger.Warn("Query result is null");
                            break;
                        }
                    }
                }
                while (searchFilterParam.PageInfo.HasNextPage);
                _logger.Info("finish searching, total result count {0}", totalCount);
                return ret;
            }
        }

    }
}
