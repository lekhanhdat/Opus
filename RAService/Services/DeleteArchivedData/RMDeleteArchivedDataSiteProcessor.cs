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
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.DB.DBLocker;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.DeleteArchivedData.Archived;
using AvePoint.RA.Service.Services.DeleteArchivedData.Cache;
using AvePoint.RA.Service.Services.DeleteArchivedData.Models;
using AvePoint.RA.Service.Services.DeleteArchivedData.RestoredDataOperator;
using AvePoint.RA.Service.Services.DeleteArchivedData.SecurityChecker;
using Microsoft.SharePoint.Client;
using RAArchiverCommon.TeamsController;
using RAExportCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.DeleteArchivedData
{
    public class RMDeleteArchivedDataSiteProcessor
    {
        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDeleteArchivedDataSiteProcessor));

        private readonly RestoredSitesInfo _restoredSiteInfo;

        private readonly RMDeleteArchivedDataJobManager _jobManager;

        private readonly RMArchivedJobManager _archivedJobManager;

        private readonly RMDeleteArchivedDataSiteCacheManager _siteCacheManager;

        private readonly RMDeleteArchivedDataO365SecurityChecker _o365SecurityChecker;

        private readonly RMDeleteArchivedDataTelemetryDataManager _telemetryDataManager;

        private RMDeleteArchivedDataSettingManager _settingManager;

        private RMRestoredDataOperatorManager _restoredDataOperatorManager;

        private RMArchivedIndexDBOperator _archivedIndexDBOperator;

        private RMDeleteArchivedDataFullTextIndexManager _fullTextIndexManager;

        private RMDeleteArchivedDataWebCacheManager _webCacheManager;

        private RMDeleteArchivedDataStubManager _stubManager;

        private RMNeedDeleteArchivedDataTemporaryStorageManager _temporaryStorageManager;



        public RMDeleteArchivedDataSiteProcessor(RestoredSitesInfo restoredSiteInfo, RMDeleteArchivedDataJobManager jobManager, RMDeleteArchivedDataTelemetryDataManager telemetryDataManager)
        {
            _restoredSiteInfo = restoredSiteInfo;
            _jobManager = jobManager;
            _telemetryDataManager = telemetryDataManager;
            _archivedJobManager = new();
            _siteCacheManager = new();
            _o365SecurityChecker = new(_siteCacheManager);
        }

        public async Task<bool> ProcessAsync()
        {
            try
            {
                _logger.Info($"Start process site [{_restoredSiteInfo.SiteUrl}].");

                var hasRemaningItems = true;

                _settingManager = new RMDeleteArchivedDataSettingManager(_restoredSiteInfo);
                if (!_settingManager.IsEnableDeleteArchivedData())
                {
                    return false;
                }

                _restoredDataOperatorManager = new RMRestoredDataOperatorManager(_restoredSiteInfo, _settingManager);
                _archivedIndexDBOperator = new RMArchivedIndexDBOperator(_restoredSiteInfo);
                if (!RMGlobalConfiguration.EnvSetting.IsGCPEnvironment)
                {
                    _fullTextIndexManager = new RMDeleteArchivedDataFullTextIndexManager();
                }
                _webCacheManager = new RMDeleteArchivedDataWebCacheManager(_archivedIndexDBOperator);
                _stubManager = new RMDeleteArchivedDataStubManager(_restoredSiteInfo, _settingManager, _siteCacheManager, _webCacheManager);
                _temporaryStorageManager = new RMNeedDeleteArchivedDataTemporaryStorageManager();

                foreach (var restoredDataOperator in _restoredDataOperatorManager.GetRestoredDataOperators())
                {
                    hasRemaningItems &= await ProcessRestoredDataOperatorAsync(restoredDataOperator);
                    restoredDataOperator.Close();
                }

                if(!RMGlobalConfiguration.EnvSetting.IsGCPEnvironment)
                {
                    await _fullTextIndexManager.WaitAsync();
                }

                await MergeIndexAndDeleteDataBlockAsync();

                _logger.Info($"End process site [{_restoredSiteInfo.SiteUrl}].");
                
                _siteCacheManager.Dispose();
                _temporaryStorageManager.Close();
                RMArchivedDataBlockManager.CloseOperators();
                return !hasRemaningItems;
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while process site [{_restoredSiteInfo.SiteUrl}]. Error: {e}");
                return false;
            }
        }

        private async Task<bool> ProcessRestoredDataOperatorAsync(IRMRestoredDataOperator restoredDataOperator)
        {
            try
            {
                var allItemProcessSucceed = true;
                foreach(var restoredItem in restoredDataOperator.ReadItems())
                {
                    var processSucceed = await PreProcessRestoredItemAsync(restoredItem);
                    if(processSucceed)
                    {
                        restoredDataOperator.DeleteItem(restoredItem);
                    }
                    allItemProcessSucceed &= processSucceed;
                }

                var hasRemainingItems = !allItemProcessSucceed || restoredDataOperator.HasRemaingItems();
                _logger.Info($"The site [{_restoredSiteInfo.SiteUrl}] operator [{restoredDataOperator.Sign}] has remaining items [{hasRemainingItems}].");
                return hasRemainingItems;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while process site [{_restoredSiteInfo.SiteUrl}] operator [{restoredDataOperator.Sign}]. Error: {e}");
                return false;
            }
        }

        private async Task<bool> PreProcessRestoredItemAsync(RMRestoredItem restoredItem)
        {
            try
            {
                if(!await _archivedJobManager.HasJobInfoAsync(restoredItem.JobId))
                {
                    _logger.Warn($"[JobNotFound] Due to not found job [{restoredItem.JobId}] for site [{_restoredSiteInfo.SiteUrl}] item [{restoredItem.BasicIndexId}], skipped.");
                    return true;
                }

                var storageId = await _archivedJobManager.GetStorageIdAsync(restoredItem.JobId);

                if(!_archivedJobManager.IsFileLevelBackup(restoredItem.JobId) && !_archivedJobManager.IsSystemStorage(storageId))
                {
                    _logger.Warn($"[NotFileLevelBackup] Due to not file level backup job [{restoredItem.JobId}] for site [{_restoredSiteInfo.SiteUrl}] item [{restoredItem.BasicIndexId}], skipped.");
                    return true;
                }

                if(!_archivedIndexDBOperator.TryGetItemById(restoredItem.BasicIndexId, out var archivedItem))
                {
                    _logger.Warn($"[ItemNotFound] Due to not found item [{restoredItem.BasicIndexId}] for site [{_restoredSiteInfo.SiteUrl}] item [{restoredItem.BasicIndexId}], skipped.");
                    return true;
                }

                if(!_o365SecurityChecker.CheckIfItemExistsInRestorePath(archivedItem, restoredItem))
                {
                    _jobManager.AddFailedDetail(_settingManager, archivedItem.Url, restoredItem.RestoredUrl, false, "RM_JS_DRD_NotFoundData");
                    _logger.Warn($"[ItemNotExistsInRestorePath] Due to item [{restoredItem.BasicIndexId}] not exists in restore path for site [{_restoredSiteInfo.SiteUrl}] item [{restoredItem.BasicIndexId}], skipped.");
                    return true;
                }

                if(!await PreProcessRestoredRelatedItemAsync(archivedItem))
                {
                    _jobManager.AddFailedDetail(_settingManager, archivedItem.Url, restoredItem.RestoredUrl, false, "RM_JS_DRD_RelatedDeleteFailed");
                    _logger.Warn($"[DeleteRelateItemDataFailed] Due to delete site [{_restoredSiteInfo.SiteUrl}] item [{restoredItem.BasicIndexId}] relate items has failed. Skipped it.");
                    return false;
                }

                var succeed = await CommonPreProcessRestoredItemAsync(archivedItem);
                if(succeed)
                {
                    _temporaryStorageManager.Add(archivedItem.Id, false, restoredItem.RestoredUrl);
                }
                else
                {
                    _jobManager.AddFailedDetail(_settingManager, archivedItem.Url, restoredItem.RestoredUrl, false, "RM_HS_Criteria_View_Msg_ValidOtherError");
                }

                return succeed;
            }
            catch(Exception e)
            {
                _logger.Error($"An error occurred while process site [{_restoredSiteInfo.SiteUrl}] item [{restoredItem.BasicIndexId}]. Error: {e}");
                return false;
            }
        }

        private async Task<bool> PreProcessRestoredRelatedItemAsync(ArchiverBasicIndex archivedItem)
        {
            if(!_settingManager.IsEnableDeleteRelatedVersion() || archivedItem.Name.Contains(":"))
            {
                return true;
            }

            var processRelatedItemSucceed = true;
            var relatedItems = _archivedIndexDBOperator.GetRelateItems(archivedItem);
            foreach(var relatedItem in relatedItems)
            {
                var succeed = await CommonPreProcessRestoredItemAsync(relatedItem);
                if(succeed)
                {
                    _temporaryStorageManager.Add(relatedItem.Id, true, "");
                }
                else
                {
                    _jobManager.AddFailedDetail(_settingManager, archivedItem.Url, "", true, "RM_HS_Criteria_View_Msg_ValidOtherError");
                }
                processRelatedItemSucceed &= succeed;
            }

            return processRelatedItemSucceed;
        }

        private async Task<bool> CommonPreProcessRestoredItemAsync(ArchiverBasicIndex archivedItem)
        {
            var siteUniqueId = _archivedJobManager.GetSiteId(archivedItem.JobId);
            bool isSoJob = archivedItem.JobId.StartsWith("SO", StringComparison.OrdinalIgnoreCase)
                           || archivedItem.JobId.StartsWith("AR", StringComparison.OrdinalIgnoreCase)
                           || archivedItem.JobId.StartsWith("DSO", StringComparison.OrdinalIgnoreCase)
                           || archivedItem.JobId.StartsWith("DASO", StringComparison.OrdinalIgnoreCase);
            if (!isSoJob && !await RMDeleteArchivedDataCosmosDBManager.DeleteItemAsync(siteUniqueId, archivedItem))
            {
                _logger.Warn($"[ComsosDBItemDeleteFailed] Due to failed delete site [{archivedItem.SitePath}] item [{archivedItem.PathMD5}] data in cosmos db, skip.");
                return false;
            }

            if(!await _stubManager.DeleteStubsAsync(archivedItem))
            {
                _logger.Warn($"[ItemDeleteStubsFailed] Due to failed delete site [{archivedItem.SitePath}] item [{archivedItem.PathMD5}] stubs, skipped.");
                return false;
            }

            if(!RMGlobalConfiguration.EnvSetting.IsGCPEnvironment)
            {
                await _fullTextIndexManager.DeleteAsync(archivedItem);
            }

            return true;
        }

        private string GetArchivedJobId(ArchiverBasicIndex archivedItem)
        {
            if(!_archivedIndexDBOperator.IsDuplicateFile(archivedItem))
            {
                return archivedItem.JobId;
            }

            if (_archivedIndexDBOperator.IsLastDuplicatedFileWithSameCRC(archivedItem, [archivedItem.Id]))
            {
                _logger.Info($"This is a duplicated file, id: {archivedItem.Id}, backup jobId: {archivedItem.DedupSourceFileJobId}");
                return archivedItem.DedupSourceFileJobId;
            }

            return "";
        }

        private async Task MergeIndexAndDeleteDataBlockAsync()
        {
            try
            {
                _logger.Info($"Start process site [{_restoredSiteInfo.SiteUrl}] merge index and delete data block.");

                using var indexDbLocker = await SampleDBLocker.Get4IndexDBUpdater(
                    _restoredSiteInfo.SiteUrl, _restoredSiteInfo.SiteId, _jobManager.JobId, TimeSpan.FromHours(1)
                );

                _archivedIndexDBOperator.Reload();

                foreach(var needDeleteItem in _temporaryStorageManager.GetItems())
                {
                    try
                    {
                        if (!_archivedIndexDBOperator.TryGetItemById(needDeleteItem.ItemId, out var archivedItem))
                        {
                            _logger.Warn($"[MergeIndex][ItemNotFound] Due to not found site [{_restoredSiteInfo.SiteUrl}] item [{needDeleteItem.ItemId}] in backuped index db. skipped.");
                            continue;
                        }

                        var needDeleteItemArchivedJobId = GetArchivedJobId(archivedItem);
                        if (string.IsNullOrWhiteSpace(needDeleteItemArchivedJobId))
                        {
                            if (!_archivedIndexDBOperator.DeleteItem(archivedItem))
                            {
                                _jobManager.AddFailedDetail(_settingManager, archivedItem.Url, needDeleteItem.RestoredUrl, needDeleteItem.RelatedDelete == 1, "RM_HS_Criteria_View_Msg_ValidOtherError");
                                _logger.Error($"[DeleteMasterIndexItemFailed] Due to failed delete site [{_restoredSiteInfo.SiteUrl}] item [{needDeleteItem.ItemId}] data in master index. skipped.");
                            }
                            else
                            {
                                _jobManager.AddSucceedDetail(_settingManager, archivedItem.Url, needDeleteItem.RestoredUrl, needDeleteItem.RelatedDelete == 1);
                            }
                            _logger.Warn($"[ItemNotFoundArchivedJobId] Due to not found site [{_restoredSiteInfo.SiteUrl}] item [{needDeleteItem.ItemId}] archived job id. skipped.");

                            continue;
                        }

                        var storageId = await _archivedJobManager.GetStorageIdAsync(needDeleteItemArchivedJobId);
                        var dataBlockOperator = RMArchivedDataBlockManager.GetFileLevelDataOperator(storageId);

                        var deletedItemDataSize = 0L;
                        if (_archivedJobManager.IsFileLevelBackup(needDeleteItemArchivedJobId))
                        {
                            if (!dataBlockOperator.TryDeleteDataBlockIfExists(archivedItem, out deletedItemDataSize))
                            {
                                _jobManager.AddFailedDetail(_settingManager, archivedItem.Url, needDeleteItem.RestoredUrl, needDeleteItem.RelatedDelete == 1, "RM_HS_Criteria_View_Msg_ValidOtherError");
                                _logger.Warn($"[DataBlockDeleteFailed] Due to failed delete site [{_restoredSiteInfo.SiteUrl}] item [{needDeleteItem.ItemId}] data block. Skipped it.");
                                continue;
                            }
                        }
                        else
                        {
                            deletedItemDataSize = archivedItem.ContentLength;
                            _telemetryDataManager.Increase(archivedItem.ContentLength);
                        }

                        if (!_archivedIndexDBOperator.DeleteItem(archivedItem))
                        {
                            _jobManager.AddFailedDetail(_settingManager, archivedItem.Url, needDeleteItem.RestoredUrl, needDeleteItem.RelatedDelete == 1, "RM_HS_Criteria_View_Msg_ValidOtherError");
                            _logger.Error($"[DeleteMasterIndexItemFailed] Due to failed delete site [{_restoredSiteInfo.SiteUrl}] item [{needDeleteItem.ItemId}] data in master index. skipped.");
                            continue;
                        }

                        _archivedJobManager.DecreaseSize(archivedItem.JobId, deletedItemDataSize);
                        _jobManager.AddSucceedDetail(_settingManager, archivedItem.Url, needDeleteItem.RestoredUrl, needDeleteItem.RelatedDelete == 1);
                    }
                    catch(Exception e)
                    {
                        _logger.Error($"An error occurred while delete item [{needDeleteItem.ItemId}] data block. Error : {e}");
                    }
                }

                _archivedJobManager.SyncSubJobDataSize();
                var reportManager = new RMDeleteArchivedDataReportManager(_restoredSiteInfo, _archivedIndexDBOperator);
                reportManager.Calculate();
                var worker = new TeamsSODashboardWorker();
                await worker.UpdateTeamsGroupRelatedSiteArchivedInfo(_restoredSiteInfo.SiteUrl);
                _archivedIndexDBOperator.Commit();
                _logger.Info($"End process site [{_restoredSiteInfo.SiteUrl}] merge index and delete data block.");
            }
            catch (Exception e)
            {
                _logger.Info($"An error occurred while process site [{_restoredSiteInfo.SiteUrl}] merge index and delete data block. Error: {e}");
            }
        }
    }
}
