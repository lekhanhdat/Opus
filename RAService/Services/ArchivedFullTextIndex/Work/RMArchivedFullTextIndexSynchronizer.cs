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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.ArchivedFullTextIndex;
using AvePoint.RA.DB.Model.ArchivedFullTextIndex;
using AvePoint.RA.Service.Services.ArchivedFullTextIndex.Work.EDiscovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ArchivedFullTextIndex.Work
{
    public class RMArchivedFullTextIndexSynchronizer
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMArchivedFullTextIndexSynchronizer));

        private readonly RMArchivedFullTextIndexCategoryManagement _categoryManagementService = new();

        private readonly RMArchivedFullTextIndexJobManager _jobManager;

        private readonly RMArchivedFullTextIndexSiteManager _siteManager;

        private readonly ArchiverIndexSubInfoContract _subJobInfo;

        private readonly RMArchivedFullTextIndexDBManager _dbManager;

        private readonly LogicalDeviceDto _logicalDevice;

        private readonly Dictionary<string, RMArchivedFullTextIndexEDiscoveryDataItemDeletor> _dataItemDeletors = new();

        public RMArchivedFullTextIndexSynchronizer(
            RMArchivedFullTextIndexJobManager jobManager,
            RMArchivedFullTextIndexSiteManager siteManager,
            ArchiverIndexSubInfoContract subJobInfo,
            LogicalDeviceDto logicalDevice
            )
        {
            _jobManager = jobManager;
            _siteManager = siteManager;
            _subJobInfo = subJobInfo;
            _dbManager = new(siteManager);
            _logicalDevice = logicalDevice;
        }

        public async Task SyncAsync()
        {
            try
            {
                _logger.Info($"Start sync site [{_siteManager.SiteUrl}] data.");

                using (new PerformanceScope($"Open index db", $"[{_siteManager.SiteUrl}]", true))
                {
                    _dbManager.Open();
                }

                var syncJobManager = await CreateSyncJobManagerAsync();


                await SyncJobAsync(syncJobManager);

                await syncJobManager.SetToFinishedAsync();
                _jobManager.Add(_siteManager.SiteUrl, _subJobInfo.JobId, syncJobManager.Status);
                await _siteManager.IncreseLatestSyncTimeAsync(_subJobInfo.ArchiverTime);

                _logger.Info($"End sync site [{_siteManager.SiteUrl}] [{_subJobInfo.JobId}] data.");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while sync site [{_siteManager.SiteUrl}] [{_subJobInfo.JobId}] data. Error: {e}");
                _jobManager.Add(_siteManager.SiteUrl, _subJobInfo.JobId, Contract.RMWeb.JobMonitor.JobStatus.Failed);
            }
            finally
            {
                _dbManager.Dispose();
            }
        }

        private async Task<RMArchivedFullTextIndexSyncJobManager> CreateSyncJobManagerAsync()
        {
            var syncJobManager = new RMArchivedFullTextIndexSyncJobManager(
                _siteManager,
                _jobManager,
                _subJobInfo.JobId,
                _subJobInfo.SiteUniqueId,
                _subJobInfo.ArchiverTime);
            await syncJobManager.InitAsync(false);
            return syncJobManager;
        }

        private async Task SyncJobAsync(RMArchivedFullTextIndexSyncJobManager syncJobManager)
        {
            try
            {
                var succeed = true;

                _logger.Info($"Start sync job [{_subJobInfo.JobId}] data.");

                var dataAppender = new RMArchivedFullTextIndexEDiscoveryDataAppender(_siteManager, _jobManager, syncJobManager);

                var dataManager = new RMArchivedFullTextIndexDataManager(_dbManager, _siteManager, syncJobManager, _subJobInfo, _logicalDevice);

                using (new PerformanceScope($"Open storage blocks", $"[{_siteManager.SiteUrl}]", true))
                {
                    dataManager.Open();
                }

                succeed &= await ProcessItemsAsync(dataManager, dataAppender, syncJobManager);

                dataManager.ReadEnd();
                succeed &= await CompleteDeleteAsync(syncJobManager);
                succeed &= await CompleteAppendAsync(dataAppender, syncJobManager);

                _logger.Info($"End sync job [{_subJobInfo.JobId}] data. Succeed: [{succeed}].");
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while sync job [{_subJobInfo.JobId}] data. Error: {e}");
                UpdateProgress(syncJobManager, false);
            }
        }

        private async Task<bool> ProcessItemsAsync(
            RMArchivedFullTextIndexDataManager dataManager,
            RMArchivedFullTextIndexEDiscoveryDataAppender dataAppender,
            RMArchivedFullTextIndexSyncJobManager syncJobManager)
        {
            var succeed = true;

            await foreach (var item in dataManager.ReadAsync())
            {
                var relatedItems = dataManager.ReadRelateOldItems(item);
                if (relatedItems.Count > 0)
                {
                    foreach (var relatedItem in relatedItems)
                    {
                        var deletor = await GetOrCreateDeletorAsync(relatedItem.ArchiverJobId, syncJobManager);
                        if (deletor == null)
                        {
                            continue;
                        }

                        var deleteRes = await deletor.DeleteAsync(relatedItem.IndexDBUniqueId);
                        succeed &= deleteRes;
                        UpdateProgress(syncJobManager, deleteRes);
                    }
                }

                await _siteManager.RecordArchiverTimeAsync(item.ArchiverTime);
                var appendRes = await dataAppender.AppendAsync(item);
                succeed &= appendRes;
                UpdateProgress(syncJobManager, appendRes);
            }

            return succeed;
        }

        private async Task<RMArchivedFullTextIndexEDiscoveryDataItemDeletor> GetOrCreateDeletorAsync(
            string archiverJobId,
            RMArchivedFullTextIndexSyncJobManager syncJobManager)
        {
            if (_dataItemDeletors.TryGetValue(archiverJobId, out var deletor))
            {
                return deletor;
            }

            // var (has, relatedItemCategoryInfo) = await _categoryManagementService.TryGetCategoryInfoAsync(archiverJobId);
            // if (!has)
            // {
            //     _logger.Info($"The archiver job [{archiverJobId}] no category info found.");
            //     return null;
            // }

            // var sameCategoryDeletor = _dataItemDeletors.Values.FirstOrDefault(item => item.CategoryInfo.Name == relatedItemCategoryInfo.Name);
            deletor = new RMArchivedFullTextIndexEDiscoveryDataItemDeletor(_siteManager, _jobManager, syncJobManager);
            _dataItemDeletors[archiverJobId] = deletor;
            return deletor;
        }

        private async Task<bool> CompleteDeleteAsync(RMArchivedFullTextIndexSyncJobManager syncJobManager)
        {
            var succeed = true;
            foreach (var deletor in _dataItemDeletors.Values)
            {
                var deleteCompletedRes = await deletor.WaitAsync();
                succeed &= deleteCompletedRes;
                UpdateProgress(syncJobManager, deleteCompletedRes);
                await _categoryManagementService.SyncCategoryDataSizeAsync();
            }

            return succeed;
        }

        private async Task<bool> CompleteAppendAsync(
            RMArchivedFullTextIndexEDiscoveryDataAppender dataAppender,
            RMArchivedFullTextIndexSyncJobManager syncJobManager)
        {
            var appendCompletedRes = await dataAppender.WaitAsync();
            UpdateProgress(syncJobManager, appendCompletedRes);
            await _categoryManagementService.SyncCategoryDataSizeAsync();
            return appendCompletedRes;
        }

        private void UpdateProgress(RMArchivedFullTextIndexSyncJobManager syncJobManager, bool succeed)
        {
            syncJobManager.IncreseProgress(succeed);
            _siteManager.IncreseProgress(succeed);
        }
    }
}
