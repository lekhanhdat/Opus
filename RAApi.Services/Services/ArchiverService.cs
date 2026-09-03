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
using AutoMapper;
using AvePoint.Api.Contract;
using AvePoint.Common.RemoteNode.Impl;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.Common.Monitor;
using AvePoint.GCommon.Contract.Server.Common.RemoteNode;
using AvePoint.GCommon.Contract.Server.ControlPanel;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Server.EndUserRestoreSetting;
using AvePoint.GCommon.Contract.StorageOptimization.Archiver;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.AzureBlobStorage;
using AvePoint.Item.Restore;
using AvePoint.RA.Api.Services.Search;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.GraphApi.GroupSite;
using AvePoint.RA.Common.GraphApi.Mail;
using AvePoint.RA.Contract.Cache;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.RACommonUtility.Encryption;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.Service.Services;
using AvePoint.RA.Service.Services.ArchivedFullTextIndex.Query;
using AvePoint.RA.Service.Services.Discovery.Office365.License;
using AvePoint.RA.Service.Services.Discovery.Salesforce.License;
using AvePoint.RA.SharePoint.Common;
using AvePoint.StorageOptimization.Archiver.Service.Impl;
using AvePoint.Wrapper.Common.Graph;
using Cloud.Sdk.Data.AosModern;
using DocAveOnline.WebApi.Contracts;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Util;
using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;
using CommonFilter = AvePoint.GCommon.Contract.CommonFilter;
using Profile = AvePoint.GCommon.Contract.Server.Common.Profile;
using SOObject = AvePoint.GCommon.Contract.StorageOptimization.Object;
using TreeObject = AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.Api.Service.Implement
{
    public class ArchiverService : RMServiceBase, Interface.IArchiverService
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(ArchiverService));
        public IMArchiverService DaoArchiverService { get => new MArchiverService(); set { } }
        public IMStoragePolicyService StoragePolicyService { get; set; }
        public IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexService => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IRMJobQueueDao RMJobQueueDao => PlatformWindsorManager.GetService<IRMJobQueueDao>();
        private IRMJobSizeAndCountStatisticsDao mRMJobSizeAndCountStatisticsDao => PlatformWindsorManager.GetService<IRMJobSizeAndCountStatisticsDao>();
        public IRestoreSearchService RestoreSearchService => PlatformWindsorManager.GetService<IRestoreSearchService>();
        public IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        public IRemoteNodeService RemoteNodeService { get => new RemoteNodeService(); set { } }
        public IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private RMAesEncryptorWrapper AesEncryptorWrapper => new();
        public Profile.IMProfileService ProfileService { get; set; }
        public IJobMonitorOptionService JobMonitorOptionService { get; set; }
        public ILoginService LoginService => PlatformWindsorManager.GetService<ILoginService>();
        public IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IRMRestoreSiteMappingDao RMRestoreSiteMappingDao => PlatformWindsorManager.GetService<IRMRestoreSiteMappingDao>();

        public IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();

        private IKeyValueService KeyValueService = PlatformWindsorManager.GetService<IKeyValueService>();

        public Dictionary<string, string> StoragePolicyCache = new Dictionary<string, string>();


        public async Task<ExportedDataResult> GetExportedDataSASByJobInfoAsync(ExportJobInfo config)
        {
            ExportedDataResult result = null;
            if (CanGetSAS(config.IsStub))
            {
                config.ExportJobId = config.ExportJobId.ToUpper();
                if (config.ExportJobId.StartsWith("ORS") || config.ExportJobId.StartsWith("EASR"))
                {
                    result = await GetExportedDataSASByJobInfoFromOpusAsync(config);
                }
                else
                {
                    result = await GetExportedDataSASByJobInfoFromDaoAsync(config);
                }
            }
            return result;
        }
        private bool CanGetSAS(bool isStub)
        {
            if (isStub)
            {
                logger.Info($"this is stub sas,need to check permision");
                var setting = DaoArchiverService.GetEndUserRestoreSetting();
                if (setting != null && setting.PermissionSetting != null)
                {
                    return setting.PermissionSetting.IsExportStubLink;
                }
                else
                {
                    logger.Warn("Dao End User Restore Setting is null or PermissionSetting is null,return false");
                    return false;
                }
            }
            return true;
        }

        private async Task<ExportedDataResult> GetExportedDataSASByJobInfoFromDaoAsync(ExportJobInfo config)
        {
            logger.Info($"GetExportedDataSASByJobInfoFromDao {config.ExportJobId}");
            if (!await IsMigratedExportedDataExpired())
            {
                logger.Warn($"The time of migrated is exceeds the processing time");
                return null;
            }
            else
            {
                try
                {
                    DAOAPIClientV1 daoApiClient = new DAOAPIClientV1(true);
                    var (dataSasString, zipPassword) = await daoApiClient.GetExportDataSasByJobInfo(config.ExportJobId, config.Office365UserMail, config.IsDownload);
                    ExportedDataResult exDataResult = new ExportedDataResult();
                    exDataResult.DataSASString = dataSasString;
                    exDataResult.ZipPassword = zipPassword;
                    logger.Info("Finish download export content");
                    return exDataResult;
                }
                catch(Exception ex)
                {
                    logger.Error("An error occurd while get exported data sas info from dao", ex);
                    return null;
                }           
            }
        }

        private async Task<bool> IsMigratedExportedDataExpired()
        {
            var timeLimit = TimeSpan.FromDays(15);
            var upgradeOpusTime = await LicenseHelperService.GetUpgradeOpusTime();
            return (DateTime.UtcNow.Ticks - upgradeOpusTime) < timeLimit.Ticks;
        }

        private async Task<ExportedDataResult> GetExportedDataSASByJobInfoFromOpusAsync(ExportJobInfo config)
        {
            logger.Info($"GetExportedDataSASByJobInfoFromOpus {config.ExportJobId}");
            ExportedDataResult exDataResult = new ExportedDataResult();
            JMItemInfo job = await JobMonitorService.GetJobForRecenterAsync(config.ExportJobId);
            if (job == null || job.Status != AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus.Finished)
            {
                string state = job == null ? "-1" : job.Status.ToString();
                logger.Warn($"can not get SAS string by job id {config.ExportJobId},State is {state} ");
                return null;
            }

            if (!string.Equals(job.UserName, config.Office365UserMail, StringComparison.CurrentCultureIgnoreCase))
            {
                logger.Warn($"can not get SAS string by job id {config.ExportJobId}, run job user mismatch.");
                return null;
            }
            try
            {
                var dataInfo = DownloadDataInfoDao.GetDownloadDataInfosByJobId(config.ExportJobId);
                exDataResult.ZipPassword = AesEncryptorWrapper.Decrypt(dataInfo.Name);
            }
            catch (Exception e)
            {
                logger.Warn($"can not get SAS password by job id {config.ExportJobId}. error :{e.ToString()}");
            }

            if (config.IsDownload)
            {
                logger.Info("Start download export content");
                DateTime finishedTime = ParseStringToDateTime(job.EndTime);
                if (finishedTime.AddDays(7) < DateTime.UtcNow)
                {
                    logger.Warn($"can not get SAS string by job id {config.ExportJobId}, More than 7 days have passed since the job ended {finishedTime.ToString()}");
                    return exDataResult;
                }
                string azureConnectionString = CommonUtilityForSpecialTenant.GetStorageConnectionStringFromConfigFile(StorageStringType.SharedStorage);

                var containerName = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.SHARED_STORAGE_CONTAINER_NAME];
                AzureBlobStorage azureBlobStorage = new AzureBlobStorage(azureConnectionString, containerName);
                string blobName = "ArchivedExportContent" + "/" + TenantLocalValue.LogonGroupId + "/" + config.ExportJobId;
                string blobNameOld = config.ExportJobId + ".zip";
                
                var blob = await azureBlobStorage.GetBlob(blobName);
                if (blob != null)
                {
                    logger.Info($"blob not null,name:{blob.Name}");
                    //exDataResult.DataSASString = azureBlobStorage.CreateSASForBLOB(blob.Name);
                    exDataResult.DataSASString = Util.MSAzure.StorageUtil.GenerateSasUriForRead(azureConnectionString, containerName, blob.Name, TimeSpan.FromHours(6));
                }
                else
                {
                    var exit = await azureBlobStorage.CheckBlobExistAsync(blobNameOld);
                    if (exit)
                    {
                        exDataResult.DataSASString = azureBlobStorage.CreateSASForBLOB(blobNameOld);
                    }
                    else
                    {
                        exDataResult = null;
                        logger.Warn($"Cannot get blobs from device. blob oldname: {blobNameOld},new name:{blobName}");
                        return exDataResult;
                    }
                }
                
                logger.Info("Finish download export content");
            }
            return exDataResult;
            //return null;
        }

        public async Task<SearchResult> AdvanceSearchAsync(AdvanceSearchCondition searchCondition)
        {
            var result = new SearchResult();
            if (string.IsNullOrEmpty(searchCondition?.Scope))
            {
                logger.Warn("Advance search scope is empty!");
                return result;
            }
            logger.Info($"Start advance search.searchCondition:{searchCondition.ToString()}.");
            //经与Recenter确认:Run Restore Job首先check DAO Setting，如果不允许Restore & Export，则直接返回Error Code.
            //1.Allow end users to restore/export archived data,总开关直接关闭时，直接返回Error Code
            //2.各个Source的 restore/export Setting，单独判断其打开关闭，如果不允许Restore & Export，则直接返回Error Code.
            #region Check DAO End User Restore Setting
            var endUserRestoreSetting = DaoArchiverService.GetEndUserRestoreSetting();
                if (!endUserRestoreSetting.IsAllowRestore)
                {
                    result.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportTotalError;
                    logger.Error("AdvanceSearch:DAODoesNotAllowUserRestoreAndExportTotalError:IsAllowRestore[False].");
                    return result;
                }
                switch (searchCondition.ModuleType)
                {
                    case DocAveOnline.WebApi.Contracts.ModuleType.None:
                        if (!endUserRestoreSetting.PermissionSetting.IsRestoreStubLink && !endUserRestoreSetting.PermissionSetting.IsExportStubLink)
                        {
                            result.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                            logger.Error("AdvanceSearch:DAODoesNotAllowUserRestoreOrExportServiceError:IsRestoreStubLink[False].IsExportStubLink[False].");
                            return result;
                        }
                        break;
                    case DocAveOnline.WebApi.Contracts.ModuleType.SharePointOnline:
                        if (endUserRestoreSetting.PermissionSetting.IsSearchSiteCollection != null && !(bool)endUserRestoreSetting.PermissionSetting.IsSearchSiteCollection)
                        {
                            result.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                            logger.Error("AdvanceSearch:DAODoesNotAllowUserRestoreOrExportServiceError.IsRestoreSiteCollection[False].IsExportSiteCollection[False].");
                            return result;
                        }
                        break;
                    case DocAveOnline.WebApi.Contracts.ModuleType.Microsoft365Groups:
                    case DocAveOnline.WebApi.Contracts.ModuleType.MicrosoftTeams:
                    if (endUserRestoreSetting.PermissionSetting.IsSearchGroupTeamSite!=null && !(bool)endUserRestoreSetting.PermissionSetting.IsSearchGroupTeamSite)
                        {
                            result.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                            logger.Error("AdvanceSearch:DAODoesNotAllowUserRestoreOrExportServiceError.IsRestoreGroupTeamSite[False].IsExportGroupTeamSite[False].");
                            return result;
                        }
                        break;
                    case DocAveOnline.WebApi.Contracts.ModuleType.OneDriveForBusiness:
                    default:
                        result.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                        logger.Error($"AdvanceSearch:DAODoesNotAllowUserRestoreOrExportServiceError.ModuleType[Error].ModuleType:{searchCondition.ModuleType}.");
                        return result;
                }
            #endregion
            try
            {
                var sites = new List<RemoteSiteCollection>();
                string searchSiteUrl = string.Empty;
                logger.Info($"start get site mapping,scope:{searchCondition.Scope}");
                var siteMappingInfo = RMRestoreSiteMappingDao.GetMappingBySourceSiteUrl(searchCondition.Scope);
                if (siteMappingInfo != null)
                {
                    logger.Info($"siteMappingInfo not null,source:{siteMappingInfo.SourceSiteUrl},target:{siteMappingInfo.TargetSiteUrl}");
                    sites = RemoteNodeService.GetRemoteSiteCollectionByParam(new List<string> { siteMappingInfo.TargetSiteUrl});
                    searchSiteUrl = siteMappingInfo.SourceSiteUrl;
                }
                else
                {
                    if (searchCondition.Scope.Equals(searchCondition.User.Name, StringComparison.OrdinalIgnoreCase) || Regex.IsMatch(searchCondition.Scope, @"^[a-zA-Z0-9_.'-]+@[a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)+$"))
                    {
                        sites = RemoteNodeService.GetRemoteSiteCollectionByParam(new List<string> { searchCondition.Scope }, false);
                        if (sites == null || sites.Count == 0)
                        {
                            sites = new List<RemoteSiteCollection>() { await RemoteNodeService.GetRemoteNodeFromAosAsync(searchCondition.Office365TenantID, searchCondition.Scope, false) };
                        }
                        var tempSite = sites.First();
                        searchSiteUrl = tempSite?.url;
                    }
                    else
                    {
                        sites = RemoteNodeService.GetRemoteSiteCollectionByParam(new List<string> { searchCondition.Scope });
                        if (sites == null || sites.Count == 0)
                        {
                            sites = new List<RemoteSiteCollection>() { await RemoteNodeService.GetRemoteNodeFromAosAsync(searchCondition.Office365TenantID, searchCondition.Scope, true) };
                        }
                        var tempSite = sites.First();
                        searchSiteUrl = tempSite?.url;
                    }
                }
                if (sites == null || sites.Count == 0)
                {
                    logger.Warn($"Can not find site in the remote node. scope: {searchCondition.Scope}");
                    result.ErrorCode = ErrorCode.RemoveFromAos;
                    result.ErrorMessage = "Can not find site in the remote node.";
                    return result;
                }
                var site = sites.First();
                //if(site.ChannelType == TeamsChannelType.Private || site.ChannelType == TeamsChannelType.Shared || site.TemplateName== "PWA#0")
                //{
                //    logger.Warn($"This site type not surpport. scope: {searchCondition.Scope}");
                //    result.ErrorCode = ErrorCode.SiteTypeNotSupport;
                //    result.ErrorMessage = "This site type not surpport";
                //    return result;
                //}
                logger.Info($"Search object information: url: {site.url}, name: {site.Name}, type: {site.NodeType},searchSiteUrl:{searchSiteUrl}");
                if (siteMappingInfo == null && !string.IsNullOrEmpty(site.TenantId) && !string.IsNullOrEmpty(searchCondition.Office365TenantID))
                {
                    if (!site.TenantId.Equals(searchCondition.Office365TenantID, StringComparison.InvariantCultureIgnoreCase))
                    {
                        logger.Warn($"Tenant ID mismatch. site tenant ID: {site.TenantId}, search tenant ID: {searchCondition.Office365TenantID}");
                        result.ErrorCode = ErrorCode.TenantIDMismatchError;
                        return result;
                    }
                }
                var siteIndex = ArchiverSiteMasterIndexService.GetAllSiteCollectionNodsInfoByUrl(searchSiteUrl).FirstOrDefault();
                if (siteIndex == null)
                {
                    logger.Warn($"Advance search scope haven't archived history. scope: {searchCondition.Scope}");
                    result.ErrorCode = ErrorCode.NoArchiveHistory;
                    result.ErrorMessage = "Advance search scope haven't archived history";
                    return result;
                }
                result.SiteUrl = searchSiteUrl;
                //result.NodeType = ConvertApiUtil.ConvertToNodeType(site.NodeType);
                if (!searchCondition.IsAOSPSearch)
                {
                    result.ErrorCode = ValidatePermission(out string siteTitle, site, searchCondition.ModuleType, searchCondition.User?.Name, endUserRestoreSetting, searchCondition.Group?.Id);
                    if (result.ErrorCode != ErrorCode.none)
                    {
                        return result;
                    }
                    result.SiteTitle = siteTitle;
                }
                logger.Info("Init tree node information.");
                TreeObject.SPTreeNodeDto sPTreeNodeDto = new TreeObject.SPTreeNodeDto()
                {
                    ID = Guid.NewGuid().ToString(),
                    Name = siteIndex.SiteURL,
                    SPObjectId = siteIndex.SiteId,
                    SPVersion = siteIndex.SPVersion,
                    FullPath = siteIndex.SiteURL,
                    CanChildrenBeLoaded = true,
                    Level = TreeObject.NodeLevel.SiteCollection,
                    //Type = (TreeObject.NodeType)ConvertApiUtil.ConvertToNodeType(site.NodeType)
                };
                logger.Info("Get search result from media.");
                //var searchResult = ArchiverAdvancedSearchService.GetSearchNodesFromMedia(new List<SOObject.ArchiverSiteMasterIndexContract> { siteIndex }, new List<TreeObject.SPTreeNodeDto> { sPTreeNodeDto }, searchFilterPolicy);
                var searchResult = await RestoreSearchService.GetSearchTreeResultAsync(new ArchiverRestoreResult()
                {
                    PageSize = searchCondition.Size,
                    PageIndex = searchCondition.Page,
                    OrderBy = searchCondition.OrderBy,
                    IsDesc = searchCondition.Order == Order.Desc,
                    SerchContract = new BackupDataSearchContract()
                    {
                        SearchNode = new SiteCollectionNodesInfo() { SiteUrl = result.SiteUrl, SiteGroupId = siteIndex.SiteGroupId, SPObjectId = site.ObjectId },
                        FilterPolicy = new CommonFilter.ArchiverRestoreFilter()
                        {
                            FilterName = searchCondition.Keyword,
                            Level = AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion,
                            CreateStartTime = searchCondition.CreatedDateFrom == 0 ? string.Empty : searchCondition.CreatedDateFrom.ToString(),
                            CreateEndTime = searchCondition.CreatedDateTo == 0 ? string.Empty : searchCondition.CreatedDateTo.ToString(),
                            FolderName = searchCondition.FolderNameOrPath,
                            ModifiedBy = searchCondition.ModifiedBy,
                            CreatedBy = searchCondition.CreatedBy,
                            FilterDeleteType = CommonFilter.FilterDeletedType.Normal
                        }
                    }
                }, false);
                var isAllArchiveTier = endUserRestoreSetting != null && endUserRestoreSetting.IsRestoreArchivedTier;
                result.AdvanceSearchResults = GetRestoreSearchResults(searchResult.RestoreSerchNodes, isAllArchiveTier);
                result.HasNext = searchResult.HasNext;
                logger.Info($"Advance search finished, result: {result?.AdvanceSearchResults?.Count()}.");
                return result;
            }
            catch (Exception e)
            {
                logger.Error($"Get advance search result failed, error: {e}");
                result.ErrorCode = ErrorCode.AdvanceSearchError;
                result.ErrorMessage = e.ToString();
            }
            return result;
        }

        public async Task<ArchiverRestoreResult> AOSPAdvanceSearchAsync(AdvanceSearchCondition searchCondition)
        {
            var result = new ArchiverRestoreResult();
            if (string.IsNullOrEmpty(searchCondition?.Scope))
            {
                logger.Warn("Advance search scope is empty!");
                return result;
            }
            logger.Info($"Start advance search.searchCondition:{searchCondition.ToString()}.");

            try
            {
                var sites = new List<RemoteSiteCollection>();
                string searchSiteUrl = searchCondition.Scope;
               
                var siteIndex = ArchiverSiteMasterIndexService.GetAllSiteCollectionNodsInfoByUrl(searchSiteUrl).FirstOrDefault();
                if (siteIndex == null)
                {
                    logger.Warn($"Advance search scope haven't archived history. scope: {searchCondition.Scope}");
                    return result;
                }
                logger.Info("Init tree node information.");
                TreeObject.SPTreeNodeDto sPTreeNodeDto = new TreeObject.SPTreeNodeDto()
                {
                    ID = Guid.NewGuid().ToString(),
                    Name = siteIndex.SiteURL,
                    SPObjectId = siteIndex.SiteId,
                    SPVersion = siteIndex.SPVersion,
                    FullPath = siteIndex.SiteURL,
                    CanChildrenBeLoaded = true,
                    Level = TreeObject.NodeLevel.SiteCollection,
                    //Type = (TreeObject.NodeType)ConvertApiUtil.ConvertToNodeType(site.NodeType)
                };
                logger.Info("Get search result from media.");
                //var searchResult = ArchiverAdvancedSearchService.GetSearchNodesFromMedia(new List<SOObject.ArchiverSiteMasterIndexContract> { siteIndex }, new List<TreeObject.SPTreeNodeDto> { sPTreeNodeDto }, searchFilterPolicy);
                var searchResult = await RestoreSearchService.GetSearchTreeResultAsync(new ArchiverRestoreResult()
                {
                    PageSize = searchCondition.Size,
                    PageIndex = searchCondition.Page,
                    OrderBy = searchCondition.OrderBy,
                    IsDesc = searchCondition.Order == Order.Desc,
                    SerchContract = new BackupDataSearchContract()
                    {
                        SearchNode = new SiteCollectionNodesInfo() { SiteUrl = searchSiteUrl, SiteGroupId = siteIndex.SiteGroupId, SPObjectId = searchCondition.SiteId },
                        FilterPolicy = new CommonFilter.ArchiverRestoreFilter()
                        {
                            FilterName = searchCondition.Keyword,
                            ItemId = BuildSearchItemIds(searchCondition),
                            Level = (CommonFilter.PolicyLevel)searchCondition.PolicyLevel,
                            CreateStartTime = searchCondition.CreatedDateFrom == 0 ? string.Empty : searchCondition.CreatedDateFrom.ToString(),
                            CreateEndTime = searchCondition.CreatedDateTo == 0 ? string.Empty : searchCondition.CreatedDateTo.ToString(),
                            FolderName = searchCondition.FolderNameOrPath,
                            ModifiedBy = searchCondition.ModifiedBy,
                            CreatedBy = searchCondition.CreatedBy,
                            FilterDeleteType = CommonFilter.FilterDeletedType.Normal,
                            IsShowTotalCount = searchCondition.IsShowTotalCount
                        }
                    }
                }, false);
                result = searchResult;
                result.HasNext = searchResult.HasNext;
                logger.Info($"Advance search finished.");
                return result;
            }
            catch (Exception e)
            {
                logger.Error($"Get advance search result failed, error: {e}");
            }
            return result;
        }

        private static List<string> BuildSearchItemIds(AdvanceSearchCondition searchCondition)
        {
            var itemIds = searchCondition?.Ids?
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList() ?? new List<string>();

            if (itemIds.Count > 0)
            {
                return itemIds;
            }

            if (!string.IsNullOrWhiteSpace(searchCondition?.Id))
            {
                itemIds.Add(searchCondition.Id);
            }

            return itemIds;
        }


        public async Task<SearchResult> AdvanceFullTextAsync(AdvanceSearchCondition searchCondition)
        {
            var result = new SearchResult();
            if (string.IsNullOrEmpty(searchCondition?.Scope))
            {
                logger.Warn("Recenter advance search scope is empty!");
                return result;
            }
            logger.Info($"Start recenter advance search.searchCondition:{searchCondition.ToString()}.");

            var isEnableFullTextSearch = await RMCacheManager.Cache.TryGetAsync<bool>(
                IRMCache.Keys.EnableFullTextIndexSearch,
                () => Task.FromResult(RestoreSearchService.IsEnableFullTextIndexSearch()),
                TimeSpan.FromMinutes(15));
            if (!isEnableFullTextSearch)
            {
                logger.Error("User not license to use full index search");
                result.ErrorCode = ErrorCode.UserNotLicenseUseFullIndexSearch;
                return result;
            }
            //经与Recenter确认:Run Restore Job首先check DAO Setting，如果不允许Restore & Export，则直接返回Error Code.
            //1.Allow end users to restore/export archived data,总开关直接关闭时，直接返回Error Code
            //2.各个Source的 restore/export Setting，单独判断其打开关闭，如果不允许Restore & Export，则直接返回Error Code.
            #region Check DAO End User Restore Setting
            var endUserRestoreSetting = DaoArchiverService.GetEndUserRestoreSetting();
            if (!endUserRestoreSetting.IsAllowRestore)
            {
                result.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportTotalError;
                logger.Error("Recenter advanceSearch:DAODoesNotAllowUserRestoreAndExportTotalError:IsAllowRestore[False].");
                return result;
            }
            switch (searchCondition.ModuleType)
            {
                case DocAveOnline.WebApi.Contracts.ModuleType.None:
                    if (!endUserRestoreSetting.PermissionSetting.IsRestoreStubLink && !endUserRestoreSetting.PermissionSetting.IsExportStubLink)
                    {
                        result.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                        logger.Error("Recenter advanceSearch:DAODoesNotAllowUserRestoreOrExportServiceError:IsRestoreStubLink[False].IsExportStubLink[False].");
                        return result;
                    }
                    break;
                case DocAveOnline.WebApi.Contracts.ModuleType.SharePointOnline:
                    if (endUserRestoreSetting.PermissionSetting.IsSearchSiteCollection != null && !(bool)endUserRestoreSetting.PermissionSetting.IsSearchSiteCollection)
                    {
                        result.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                        logger.Error("Recenter advanceSearch:DAODoesNotAllowUserRestoreOrExportServiceError.IsRestoreSiteCollection[False].IsExportSiteCollection[False].");
                        return result;
                    }
                    break;
                case DocAveOnline.WebApi.Contracts.ModuleType.Microsoft365Groups:
                case DocAveOnline.WebApi.Contracts.ModuleType.MicrosoftTeams:
                    if (endUserRestoreSetting.PermissionSetting.IsSearchGroupTeamSite != null && !(bool)endUserRestoreSetting.PermissionSetting.IsSearchGroupTeamSite)
                    {
                        result.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                        logger.Error("Recenter advanceSearch:DAODoesNotAllowUserRestoreOrExportServiceError.IsRestoreGroupTeamSite[False].IsExportGroupTeamSite[False].");
                        return result;
                    }
                    break;
                case DocAveOnline.WebApi.Contracts.ModuleType.OneDriveForBusiness:
                default:
                    result.ErrorCode = ErrorCode.DAODoesNotAllowUserRestoreAndExportServiceError;
                    logger.Error($"Recenter advanceSearch:DAODoesNotAllowUserRestoreOrExportServiceError.ModuleType[Error].ModuleType:{searchCondition.ModuleType}.");
                    return result;
            }
            #endregion
            try
            {
                var sites = new List<RemoteSiteCollection>();
                if (searchCondition.Scope.Equals(searchCondition.User.Name, StringComparison.OrdinalIgnoreCase) || Regex.IsMatch(searchCondition.Scope, @"^[a-zA-Z0-9_.'-]+@[a-zA-Z0-9_-]+(\.[a-zA-Z0-9_-]+)+$"))
                {
                    sites = RemoteNodeService.GetRemoteSiteCollectionByParam(new List<string> { searchCondition.Scope }, false);
                }
                else
                {
                    sites = RemoteNodeService.GetRemoteSiteCollectionByParam(new List<string> { searchCondition.Scope });
                }
                if (sites == null || sites.Count == 0)
                {
                    logger.Warn($"(Recenter)Can not find site in the remote node. scope: {searchCondition.Scope}");
                    result.ErrorCode = ErrorCode.RemoveFromAos;
                    result.ErrorMessage = "Can not find site in the remote node.";
                    return result;
                }
                var site = sites.First();
                //if (site.ChannelType == TeamsChannelType.Private || site.ChannelType == TeamsChannelType.Shared || site.TemplateName== "PWA#0")
                //{
                //    logger.Warn($"(Recenter)This site type not surpport. scope: {searchCondition.Scope}");
                //    result.ErrorCode = ErrorCode.SiteTypeNotSupport;
                //    result.ErrorMessage = "This site type not surpport";
                //    return result;
                //}
                logger.Info($"Recenter search object information: url: {site.url}, name: {site.Name}, type: {site.NodeType}");
                if (!string.IsNullOrEmpty(site.TenantId) && !string.IsNullOrEmpty(searchCondition.Office365TenantID))
                {
                    if (!site.TenantId.Equals(searchCondition.Office365TenantID, StringComparison.InvariantCultureIgnoreCase))
                    {
                        result.ErrorCode = ErrorCode.TenantIDMismatchError;
                        return result;
                    }
                }

                if (!(await ArchiverSiteMasterIndexService.ExistsArchivedDataAsync(site.url)))
                {
                    logger.Warn($"Recenter dvance search scope haven't archived history. scope: {searchCondition.Scope}");
                    result.ErrorCode = ErrorCode.NoArchiveHistory;
                    result.ErrorMessage = "Advance search scope haven't archived history";
                    return result;
                }
                
                searchCondition.SiteId = site.ObjectId;
                searchCondition.SiteUrl = site.url;
                //result.NodeType = ConvertApiUtil.ConvertToNodeType(site.NodeType);
                result.ErrorCode = ValidatePermission(out string siteTitle, site, searchCondition.ModuleType, searchCondition.User?.Name, endUserRestoreSetting, searchCondition.Group?.Id);
                if (result.ErrorCode != ErrorCode.none)
                {
                    return result;
                }
                
                logger.Info("(Recenter)Get search result from media.");

                var isNewFullTextIndexkeyValue = KeyValueService.Get(KeyNameCollection.IsNewFullTextIndex);
                if (isNewFullTextIndexkeyValue != null && bool.TryParse(isNewFullTextIndexkeyValue.Value, out var isNew) && isNew)
                {
                    var querierV1 = new RMArchivedFullTextIndexReCenterQuerierV1(searchCondition);
                    result = await querierV1.QueryAsync(searchCondition.ContinuationToken, searchCondition.Size);
                }
                else
                {
                    RMArchivedFullTextIndexReCenterQuerier querier = new RMArchivedFullTextIndexReCenterQuerier(searchCondition);
                    result = await querier.QueryAsync(searchCondition.ContinuationToken, searchCondition.Size, searchCondition.CategoryId);
                }

                result.SiteTitle = siteTitle;
                result.SiteUrl = site.url;
                logger.Info($"Recenter advance search finished, result: {result?.AdvanceSearchResults?.Count()}.");
                return result;
            }
            catch (Exception e)
            {
                logger.Error($"Get recenter advance search result failed, error: {e}");
                result.ErrorCode = ErrorCode.AdvanceSearchError;
                result.ErrorMessage = e.ToString();
            }
            return result;
        }

        private ErrorCode ValidatePermission(out string siteTitle, RemoteSiteCollection site, DocAveOnline.WebApi.Contracts.ModuleType moduleType, string userName, EndUserRestoreSettingUIDto endUserRestoreSetting, string groupId = null)
        {
            SOObject.SOReturnMessage permission = null;
            siteTitle = string.Empty;
            logger.Info($"Validate permission site info: url = {site?.url}, nodetype = {site?.NodeType} moduleType: {moduleType} groupid: {groupId}");
            try
            {
                //Cloud Archiver不支持Search OneDriveForBusiness数据，所以此处没有判断。目前One Drive数据只支持Stub Restore
                if (moduleType == DocAveOnline.WebApi.Contracts.ModuleType.SharePointOnline)
                {
                    permission = DaoArchiverService.CheckPermissionForSharePointSite(site, userName);
                }
                else if (moduleType == DocAveOnline.WebApi.Contracts.ModuleType.MicrosoftTeams || moduleType == DocAveOnline.WebApi.Contracts.ModuleType.Microsoft365Groups)
                {
                    permission = DaoArchiverService.CheckPermissionForGroupOrTeamSite(site, groupId, userName);
                }
                logger.Info($"the EndUserRetore permissionSetting :IsRestoreSiteCollection:{endUserRestoreSetting.PermissionSetting.IsRestoreSiteCollection.ToString()},IsExportSiteCollection:{endUserRestoreSetting.PermissionSetting.IsExportSiteCollection.ToString()}");
                if (endUserRestoreSetting.PermissionSetting.IsRestoreSiteCollection && !endUserRestoreSetting.PermissionSetting.IsExportSiteCollection)
                {
                    if (permission != null && permission.IsReadOnlySite)
                    {
                        logger.Warn("this site is readonly site");
                        permission.MessageType = SOObject.SOMessageType.Failed;
                        permission.FailedType = SOObject.FailedType.SiteCollectionReadOnly;
                    }
                }
                if (permission != null && permission.MessageType == SOObject.SOMessageType.Failed)
                {
                    logger.Warn($"Validate permission failed type {permission.FailedType}");
                    if (permission.FailedType == SOObject.FailedType.InsufficientPrivilegesForSite)
                    {
                        return ErrorCode.InsufficientPrivileges4SiteOwner;
                    }
                    else if (permission.FailedType == SOObject.FailedType.SecurityTrimingException)
                    {
                        return ErrorCode.SCNotExistOrAccessDenied;
                    }
                    else if (permission.FailedType == SOObject.FailedType.SiteCollectionLocked)
                    {
                        return ErrorCode.SiteLockedError;
                    }
                    else if (permission.FailedType == SOObject.FailedType.UserNotGroupOwner)
                    {
                        return ErrorCode.UserNotInOwnerGroup;
                    }
                    else if (permission.FailedType == SOObject.FailedType.SiteNotRegistered)
                    {
                        return ErrorCode.NoArchiveHistory;
                    }
                    else if (permission.FailedType == SOObject.FailedType.RequestResourceNotFound)
                    {
                        return ErrorCode.GroupNotFound;
                    }
                    else if (permission.FailedType == SOObject.FailedType.UserNotGroupOwnerOrMember)
                    {
                        return ErrorCode.UserNotInOwnerOrMemberGroup;
                    }
                    else if (permission.FailedType == SOObject.FailedType.UserNotOwnerForSharePointSite)
                    {
                        return ErrorCode.UserNotInOwnerGroup;
                    }
                    else if (permission.FailedType == SOObject.FailedType.UserNotOwnerOrMemberForSharePointSite)
                    {
                        return ErrorCode.UserNotInOwnerOrMemberGroup;
                    }
                    else if (permission.FailedType == SOObject.FailedType.UserNotOwnerOrMemberOrVisitorForSharePointSite)
                    {
                        return ErrorCode.UserNotInOwnerOrMemberOrVisitorGroup;
                    }
                    else if (permission.FailedType == SOObject.FailedType.UserNotOwnerOrSpecifiedGroupForSharePointSite)
                    {
                        return ErrorCode.UserNotInOwnerOrSpecificGroup;
                    }
                    else if (permission.FailedType == SOObject.FailedType.SiteCollectionReadOnly)
                    {
                        return ErrorCode.SiteReadOnlyError;
                    }
                    else if (permission.FailedType == SOObject.FailedType.ActiveAppProfileNotFound)
                    {
                        return ErrorCode.ActiveAppProfileNotFound;
                    }
                    else
                    {
                        return ErrorCode.UnExpectedException;
                    }
                }
                siteTitle = permission?.SiteTitle;
                logger.Info($"Validate permission successful. sitetitle: {siteTitle}");
                return ErrorCode.none;
            }
            catch (Exception e)
            {
                logger.Error($"Validate permission exception {e}");
                throw e;
            }
        }
        private DateTime ParseStringToDateTime(string timeStr)
        {
            DateTime result = new DateTime(Convert.ToInt64(timeStr), DateTimeKind.Utc).ToLocalTime();
            return result;
        }
        private List<AdvanceSearchResult> GetRestoreSearchResults(List<ArchiverRestoreSerchResult> sPTreeNodeDtos, bool isAllArchiveTier)
        {
            var result = new List<AdvanceSearchResult>();
            if (sPTreeNodeDtos == null)
            {
                return result;
            }
            foreach (var spTreeNode in sPTreeNodeDtos)
            {
                result.Add(new AdvanceSearchResult
                {
                    Name = spTreeNode?.ObjectName,
                    FullPath = spTreeNode?.FullPath,
                    PathMD5 = spTreeNode?.PathMd5,
                    CreateTime = Convert.ToInt64(spTreeNode?.CreatedDateTicks),
                    ModifiedBy = spTreeNode?.ModifiedBy,
                    AbsolutePath = spTreeNode?.FullPath,
                    ContentLenth = spTreeNode.ContentLenth,
                    ModifiedTime = Convert.ToInt64(spTreeNode?.ModifiedTime),
                    ArchiveTime = Convert.ToInt64(spTreeNode?.ArchiveTime),
                    IsArchiveTier = spTreeNode.IsArchiveTier,
                });
            }
            return result;
        }


        public async Task<bool> OpusStorageOptimizationEnabled()
        {
            var tenantId = TenantLocalValue.LogonGroupId;
            try
            {
                logger.Info($"start check is new opus : {tenantId}");
                var isNewOpus = await LicenseHelperService.IsNewOpus(true, false);
                logger.Info($"finish check is new opus : {isNewOpus}");

                var tenantStatus = await TenantService.TryGetTenantStatusAsync(tenantId);
                if (tenantStatus == null || tenantStatus != TenantStatus.Normal)
                {
                    logger.Info($"Opus tenant not initialized. {tenantStatus}");
                    return false;
                }

                if (isNewOpus && await LicenseHelperService.ForceEnableSO()) 
                {
                    //both use old archiver and SO
                    isNewOpus = false;
                    logger.Info($"force enable SO from db flag.");
                }
                logger.Info($"finish check ForceEnableSO,is new opus:{isNewOpus}");
                return isNewOpus;
            }
            catch (Exception e)
            {
                logger.Error($"Something went wrong when RunDisposalInRecords value, tenant:{tenantId}, error:{e}");
                return false;
            }
        }

        public async Task<int> GetTenantJobQueueCount()
        {
            var tenantId = TenantLocalValue.LogonGroupId;
            try
            {
                logger.Info($"Start get tenant job queue count. tenant:{tenantId}");
                var count = await RMCacheManager.Cache.TryGetAsync<int>(
                    IRMCache.Keys.Tenant_JobQueueCount,
                    () => Task.FromResult(RMJobQueueDao.GetTenantJobQueueCount(tenantId)),
                    TimeSpan.FromMinutes(3));
                logger.Info($"Finish get tenant job queue count. tenant:{tenantId}, count:{count}");
                return count;
            }
            catch (Exception e)
            {
                logger.Error($"Something went wrong when getting tenant job queue count, tenant:{tenantId}, error:{e}");
                throw;
            }
        }

        public async Task<List<Microsoft365Group>> GetTeamsAsync(Microsoft365User microsoft365User)
        {
            try
            {
                logger.Info("start get teams");
                var appProfile = await PoolUserUtil.GetBPOSInfoAsync(microsoft365User.TenantId);
                if (appProfile == null)
                {
                    logger.Warn($"GetTeamsAsync can not find opus app,need to find aosp app:365tenant:{microsoft365User.TenantId}");
                    appProfile = await RMAosApiClient.GetAOSPAuthProfile(TenantLocalValue.LogonGroupId, microsoft365User.TenantId);
                    if (string.IsNullOrEmpty(appProfile?.TenantId))
                    {
                        logger.Warn($"GetTeamsAsync current aops app not have 365tenant id,need set it.365tenant id:{microsoft365User.TenantId}");
                        appProfile.TenantId = microsoft365User.TenantId;
                    }
                }
                var groupSite = new RMGraphGroupManager(appProfile);
                List<RMGroup> memberGroups = new List<RMGroup>();
                List<RMGroup> ownerGroups = new List<RMGroup>();
                var memberOfGroups = await groupSite.GetUserMemberOfGroups(microsoft365User.UserId);
                while (true)
                {
                    if (string.IsNullOrEmpty(memberOfGroups.OdataNextLink))
                    {
                        memberGroups.AddRange(memberOfGroups.Value.ToList());
                        break;
                    }
                    else
                    {
                        memberGroups.AddRange(memberOfGroups.Value.ToList());
                        memberOfGroups = await groupSite.GetUserMemberOfGroups(microsoft365User.UserId, memberOfGroups.OdataNextLink);
                    }
                }
                var ownerOfGroups = await groupSite.GetUserOwnedObject(microsoft365User.UserId);
                while (true)
                {
                    if (string.IsNullOrEmpty(ownerOfGroups.OdataNextLink))
                    {
                        ownerGroups.AddRange(ownerOfGroups.Value.ToList());
                        break;
                    }
                    else
                    {
                        ownerGroups.AddRange(ownerOfGroups.Value.ToList());
                        ownerOfGroups = await groupSite.GetUserOwnedObject(microsoft365User.UserId, ownerOfGroups.OdataNextLink);
                    }
                }
                var temp = memberGroups.UnionBy(ownerGroups, m => m.Id);
                List<Microsoft365Group> result = new List<Microsoft365Group>();
                foreach (var t in temp)
                {
                    if ((t.ResourceProvisioningOptions?.Contains("Team", StringComparer.OrdinalIgnoreCase) ?? false) || (t.CreationOptions?.Contains("Team", StringComparer.OrdinalIgnoreCase) ?? false))
                    {
                        if (t.GroupTypes != null && t.GroupTypes.Contains("Unified"))
                        {
                            result.Add(new Microsoft365Group()
                            {
                                Id = t.Id,
                                DisplayName = t.DisplayName,
                                GroupName = t.Mail
                            });
                        }
                    }
                }
                logger.Info("finsih get teams");
                return result;
            }
            catch (Exception e)
            {
                logger.Error($"something went wrong when get teams,error:{e.ToString()}");
                return null;
            }
        }

        public async Task<List<Microsoft365Group>> GetGroupsAsync(Microsoft365User microsoft365User)
        {
            try
            {
                logger.Info("start get groups");
                var appProfile = await PoolUserUtil.GetBPOSInfoAsync(microsoft365User.TenantId);
                if (appProfile == null)
                {
                    logger.Warn($"GetGroupsAsync can not find opus app,need to find aosp app:365tenant:{microsoft365User.TenantId}");
                    appProfile = await RMAosApiClient.GetAOSPAuthProfile(TenantLocalValue.LogonGroupId, microsoft365User.TenantId);
                    if (string.IsNullOrEmpty(appProfile?.TenantId))
                    {
                        logger.Warn($"GetTeamsAsync current aops app not have 365tenant id,need set it.365tenant id:{microsoft365User.TenantId}");
                        appProfile.TenantId = microsoft365User.TenantId;
                    }
                }
                var groupSite = new RMGraphGroupManager(appProfile);
                List<RMGroup> memberGroups = new List<RMGroup>();
                List<RMGroup> ownerGroups = new List<RMGroup>();
                var memberOfGroups = await groupSite.GetUserMemberOfGroups(microsoft365User.UserId);
                while (true)
                {
                    if (string.IsNullOrEmpty(memberOfGroups.OdataNextLink))
                    {
                        memberGroups.AddRange(memberOfGroups.Value.ToList());
                        break;
                    }
                    else
                    {
                        memberGroups.AddRange(memberOfGroups.Value.ToList());
                        memberOfGroups = await groupSite.GetUserMemberOfGroups(microsoft365User.UserId, memberOfGroups.OdataNextLink);
                    }
                }
                var ownerOfGroups = await groupSite.GetUserOwnedObject(microsoft365User.UserId);
                while (true)
                {
                    if (string.IsNullOrEmpty(ownerOfGroups.OdataNextLink))
                    {
                        ownerGroups.AddRange(ownerOfGroups.Value.ToList());
                        break;
                    }
                    else
                    {
                        ownerGroups.AddRange(ownerOfGroups.Value.ToList());
                        ownerOfGroups = await groupSite.GetUserOwnedObject(microsoft365User.UserId, ownerOfGroups.OdataNextLink);
                    }
                }
                var temp = memberGroups.UnionBy(ownerGroups, m => m.Id);
                List<Microsoft365Group> result = new List<Microsoft365Group>();
                foreach (var t in temp)
                {
                    if (!((t.ResourceProvisioningOptions?.Contains("Team", StringComparer.OrdinalIgnoreCase) ?? false) || (t.CreationOptions?.Contains("Team", StringComparer.OrdinalIgnoreCase) ?? false)))
                    {
                        if (t.GroupTypes != null && t.GroupTypes.Contains("Unified"))
                        {
                            result.Add(new Microsoft365Group()
                            {
                                Id = t.Id,
                                DisplayName = t.DisplayName,
                                GroupName = t.Mail
                            });
                        }
                    }
                }
                logger.Info("finish get groups");
                return result;
            }
            catch (Exception e)
            {
                logger.Error($"something went wrong when get groups,error:{e.ToString()}");
                return null;
            }
        }

        public async Task<byte[]> GetPhotoAsync(Microsoft365User microsoft365User)
        {
            try
            {
                logger.Info("start get user photo");
                AppProfileInfo appProfile = await PoolUserUtil.GetBPOSInfoAsync(microsoft365User.TenantId);
                if (appProfile == null)
                {
                    logger.Warn($"GetPhotoAsync can not find opus app,need to find aosp app:365tenant:{microsoft365User.TenantId}");
                    appProfile = await RMAosApiClient.GetAOSPAuthProfile(TenantLocalValue.LogonGroupId, microsoft365User.TenantId);
                    if (string.IsNullOrEmpty(appProfile?.TenantId))
                    {
                        logger.Warn($"GetPhotoAsync current aops app not have 365tenant id,need set it.365tenant id:{microsoft365User.TenantId}");
                        appProfile.TenantId = microsoft365User.TenantId;
                    }
                }
                var groupSite = new RMGraphGroupManager(appProfile);
                byte[] photoByte = await groupSite.GetUserPhotoValue(microsoft365User.UserId);
                logger.Info("finish get user photo");
                return photoByte;
            }
            catch (Exception e)
            {
                logger.Error($"some thing went wrong when get user photo,error:{e.ToString()}");
                return null;
            }
        }

        public async Task<bool> InitTenantForMigrationJob(string logonUserId)
        {
            var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
            var info = await client.CustomerService.GetAsync();
            TenantLocalValue.LogonUserEmail = info.Email;
            TenantLocalValue.LogonUserId = logonUserId;
            var isNewTenant = await TenantService.InitTenantAsync();
            logger.Info($"Verify and create default securtity profile.");
            await LoginService.InitSecurityProfileAsync();
            await GeneralSettingService.VerifyAndCreateDefaultSecurityProfileAsync();

            await LicenseHelperService.UpdateLicense(true, disableSO: true, true);

            return await Task.FromResult(true);
        }

        public async Task<MigrationJobReportSASResult> GetMigrationJobReportSASAsync(string jobId)
        {
            logger.Info($"GetMigrationJobReportSASAsync {jobId}");
            MigrationJobReportSASResult migrationJobSASResult = new MigrationJobReportSASResult();

            try
            {
                logger.Info("Start Create Migration Job Report SAS");
                string connectionString = CommonUtilityForSpecialTenant.GetStorageConnectionStringFromConfigFile(StorageStringType.SharedStorage);
                var containerName = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.SHARED_STORAGE_CONTAINER_NAME];
                AzureBlobStorage azureBlobStorage = new AzureBlobStorage(connectionString, containerName);

                string blobName = JobMonitorService.GetMigrationJobReportExcelBlobName(jobId);
                if (await azureBlobStorage.CheckBlobExistAsync(blobName))
                {
                    migrationJobSASResult.SasUri = Util.MSAzure.StorageUtil.GenerateSasUriForRead(connectionString, containerName, blobName, TimeSpan.FromHours(6));
                    migrationJobSASResult.Expired = DateTime.UtcNow.AddHours(6).AddMinutes(-10);
                    logger.Info("Finish Create Migration Job Report SAS");
                }
                else
                {
                    logger.Warn("Not Find job report file in blob");
                }

                return migrationJobSASResult;
            }
            catch (Exception ex)
            {
                logger.Error($"Error occurred while generating migration job report SAS Uri. {ex}");
                return new MigrationJobReportSASResult() { ErrorCode = ErrorCode.UnExpectedException, ErrorMessage = ex.Message };
            }
        }

        public async Task<bool> ClearLicenseUsageAsync()
        {
            try
            {
                logger.Info($"start Clear License Usage");
                await mRMJobSizeAndCountStatisticsDao.UpdateJobStatisticsStatusAsync();
                await RMDiscoveryOffice365LicenseHelper.RemoveAllExecutionAsync();
                await RMDiscoverySalesforceLicenseHelper.RemoveAllExecutionAsync();
                logger.Info($"finish Clear License Usage");
                return true;
            }
            catch (System.InvalidOperationException e)
            {
                logger.Error($"some thing went wrong when RemoveAllExecution,may be not have discover license or never run discover job,error:{e}");
                return true;
            }
            catch (Exception e)
            {
                logger.Error($"some thing went wrong when ClearLicenseUsageAsync,error:{e}");
                return false;
            }
        }
        public async Task<List<string>> GetAllStubSearchResultAsync(Microsoft365User microsoft365User)
        {
            try
            {
                StubSearch searchManager = new StubSearch(microsoft365User);
                var hasPermissionStubUrls = await searchManager.GetHasPermissionStubUrls();
                return hasPermissionStubUrls;
            }
            catch (Exception e)
            {
                logger.Error($"some thing went wrong when get all stub path,error:{e.ToString()}");
                return null;
            }
        }

        public async Task<Stream> GetStubPreviewStreamAsync(PreviewDataParam param)
        {
            AveItemRestoreMain archiverRestore = new AveItemRestoreMain();
            try
            {
                var result= await archiverRestore.GetStubStreamAsync(param);
                if (result.Length > ContractConstants.STUBPREVIEWSIZE)
                {
                    logger.Warn($"the file size more than 10m.so return null,size:{result.Length}");
                    return null;
                }
                result.Seek(0, SeekOrigin.Begin);
                return result;
            }
            catch (Exception ex) 
            {
                logger.Error($"some thing went wrong when stub preview,error :{ex}");
                return null;
            }
        }


        public Contract.Job.JMJobSummary GetJobSummary(string id)
        {
            try
            {
                logger.Info($"Get job summary, job id:{id}");
                var config = new MapperConfiguration(cfg =>
                    {
                        cfg.LicenseKey = ReadEmbeddedLicense();
                        cfg.CreateMap<JMJobSummary, Contract.Job.JMJobSummary>(MemberList.Destination);
                        cfg.CreateMap<RA.Contract.RMWeb.JobMonitor.JobStatus, Contract.Job.NewArchiverJobStatus>();
                        cfg.CreateMap<RMJobSummaryInfos, Contract.Job.RMJobSummaryInfos>();
                        cfg.CreateMap<RMJobSummaryItem, Contract.Job.RMJobSummaryItem>();
                        cfg.CreateMap<RMJobSummaryRow, Contract.Job.RMJobSummaryRow>();
                    }, NullLoggerFactory.Instance);
                var mapper = config.CreateMapper();
                JMJobSummary result = JobMonitorService.GetJobSummaryAsync(id).GetAwaiter().GetResult();
                var mapperResult = mapper.Map<Contract.Job.JMJobSummary>(result);
                return mapperResult;
            }
            catch (Exception e)
            {
                logger.Error($"GetJobSummary error: {e}");
                throw;
            }
        }

        public Contract.Job.JMDetailsResult GetJobDetails(Contract.Job.JMDetailsQuery queryModel)
        {
            try
            {
                logger.Info($"get job details, queryModel:{queryModel.JobID} page:{queryModel.CurrentPage} page size:{queryModel.PageSize}");
                var paramConfig = new MapperConfiguration(cfg =>
                    {
                        cfg.LicenseKey = ReadEmbeddedLicense();
                        cfg.CreateMap<Contract.Job.JMDetailsQuery, JMDetailsQuery>(MemberList.Source);
                        cfg.CreateMap<Contract.Job.JobDetailsStatus, JobDetailsStatus>();
                        cfg.CreateMap<Contract.Job.ActionTab, ActionTab>();
                    }, NullLoggerFactory.Instance);
                var paramMapper = paramConfig.CreateMapper();
                JMDetailsQuery convertQueryModel = paramMapper.Map<JMDetailsQuery>(queryModel);
                convertQueryModel.ActionTabFilters ??= [];
                convertQueryModel.EntityTypeFilters ??= [];
                convertQueryModel.StatusFilters ??= [];
                convertQueryModel.JobType = convertQueryModel.JobType != 0 ? convertQueryModel.JobType : (int)AvePoint.RA.Contract.JobMonitor.JobType.SpecifySitesArchiverBackup;
                convertQueryModel.PageSize = convertQueryModel.PageSize != 0 ? convertQueryModel.PageSize : 10;

                var result = JobMonitorService.GetJobDetailsAsync(convertQueryModel).GetAwaiter().GetResult();
                return JsonConvert.DeserializeObject<Contract.Job.JMDetailsResult>(result);
            }
            catch (Exception e)
            {
                logger.Error($"GetJobDetails error: {e}");
                throw;
            }
        }

        public Contract.Job.JMJobDetails GetJobSummaryStatistics(string id)
        {
            try
            {
                var config = new MapperConfiguration(cfg =>
                    {
                        cfg.LicenseKey = ReadEmbeddedLicense();
                        cfg.CreateMap<JMSOSummaryDetails, Contract.Job.JMSOSummaryDetails>(MemberList.Destination);
                        cfg.CreateMap<ActionStatistics, Contract.Job.ActionStatistics>();
                        cfg.CreateMap<ObjectStatistic, Contract.Job.ObjectStatistic>();
                        cfg.CreateMap<RA.Contract.RMWeb.JobMonitor.JobStatus, Contract.Job.NewArchiverJobStatus>();
                    }, NullLoggerFactory.Instance);
                var mapper = config.CreateMapper();
                var result = JobMonitorService.GetSOJobSummaryDetailsAsync(id).GetAwaiter().GetResult();
                Contract.Job.JMSOSummaryDetails mapperResult = mapper.Map<Contract.Job.JMSOSummaryDetails>(result);
                return mapperResult;
            }
            catch (Exception e)
            {
                logger.Error($"GetJobSummaryStatistics error: {e}");
                throw;
            }
        }

        public List<Contract.Job.JMJobInfo> GetOpusJobListByIds( List<string> ids)
        {
            List<JMItemInfo> jobInfoList = [];
            foreach (var jobId in ids)
            {
                JMItemInfo jobInfo = null;
                try
                {
                    jobInfo = JobMonitorService.GetJobAsync(jobId).GetAwaiter().GetResult();
                    if (jobInfo == null || string.IsNullOrEmpty(jobInfo.JobId))
                    {
                        jobInfo = new JMItemInfo();
                        var messages = JobQueueService.GetDBJobMessage().Where(m => m.TenantGroupId == TenantLocalValue.LogonGroupId);
                        var jqDto = messages.FirstOrDefault(m => m.Parameters?.Contains(jobId) ?? false);
                        if (jqDto != null)
                        {
                            jobInfo.JobTypeCode = (int)jqDto.JobType;
                            jobInfo.Progress = 0;
                            jobInfo.Status = RA.Contract.RMWeb.JobMonitor.JobStatus.Pending;
                            jobInfo.Comment = "";
                        }
                        else
                        {
                            var errorMessage = $"Can't find job : {jobId}";
                            logger.Error(errorMessage);
                            jobInfo.Comment = errorMessage;
                        }
                    }
                }
                catch (Exception e)
                {
                    var errorMessage = $"Get {jobId} job status failed, {e}";
                    logger.Error(errorMessage);
                    jobInfo.Comment = errorMessage;
                }
                jobInfoList.Add(jobInfo);
            }

            var config = new MapperConfiguration(cfg =>
            {
                cfg.LicenseKey = ReadEmbeddedLicense();
                cfg.CreateMap<JMItemInfo, Contract.Job.JMJobInfo>(MemberList.Destination);
                cfg.CreateMap<RA.Contract.RMWeb.JobMonitor.JobStatus, Contract.Job.NewArchiverJobStatus>();
            }, NullLoggerFactory.Instance);
            var mapper = config.CreateMapper();
            var mapperResult = mapper.Map<List<Contract.Job.JMJobInfo>>(jobInfoList);
            return mapperResult;
        }

        private string ReadEmbeddedLicense()
        {
            var assembly = typeof(ArchiverService).Assembly;
            using var stream = assembly.GetManifestResourceStream("AvePoint.RA.Api.Services.Services.automapper.lic");
            if (stream == null)
                throw new InvalidOperationException("Embedded resource 'AvePoint.RA.Api.Services.Services.automapper.lic' not found.");
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

    }
}
