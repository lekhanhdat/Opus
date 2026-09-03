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
using Aspose.Pdf.Operators;
using AvePoint.Common.RemoteNode.Impl;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.ContentManager.Object;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Media.WcfService;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.ControlPanel;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Exceptions;
using AvePoint.Item.Restore;
using AvePoint.Media.Service;
using AvePoint.Media.Service.ArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Stub;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.Audit.Async;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RestoreCenter;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.FSMasterIndex;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RMWeb.StorageDevice;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Core;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.GoogleSyncNodeDao.Contract;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Common;
using AvePoint.RA.Service.Services.AccountManager;
using AvePoint.RA.Service.Services.ArchivedFullTextIndex;
using AvePoint.RA.Service.Services.Common;
using AvePoint.RA.Service.Services.JobQueue;
using AvePoint.RA.Service.Services.RMGeneralSetting;
using AvePoint.RA.Service.Services.RMSharePointSettings;
using AvePoint.RA.Service.Services.Settings;
using AvePoint.RA.Service.Services.Settings.AuditHandler;
using AvePoint.RA.Service.Services.Tenant;
using AvePoint.RA.SharePoint.RestoreJob.Restore.Content;
using AvePoint.Wrapper.Common;
using DocumentFormat.OpenXml.Spreadsheet;
using Merged18NResources.MediaServiceArchiverBackup;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.SharePoint.Client;
using Newtonsoft.Json;
using PnP.Framework.Modernization.Cache;
using RAExportCommon;
using RAGoogle.Restore.Content;
using Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using TimeZoneConverter;
using static AvePoint.RA.RACommonUtility.Common.CommonUtilityForSpecialTenant;
using ArgumentCheck = AvePoint.GCommon.Utility.ArgumentCheck;
using JobStatus = AvePoint.RA.Contract.RMWeb.JobMonitor.JobStatus;

namespace AvePoint.RA.Service.Services.Archiver.Restore
{
    [Audit]
    public class RestoreSearchService : RMServiceBase, IRestoreSearchService
    {
        private RALogger logger = RALogger.GetInstance(typeof(RestoreSearchService));

        //public IArchiverSiteMasterIndexService ArchiverIndexService { get; set; }
        private  IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private  IDownloadDataInfoDao DownloadDataInfoDao => PlatformWindsorManager.GetService<IDownloadDataInfoDao>();
        private IArchiverSiteMasterIndexService ArchiverIndexService => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexService>();
        private ICommonSiteMasterIndexService CommonSiteMasterIndexService => PlatformWindsorManager.GetService<ICommonSiteMasterIndexService>();
        private IRMSubJobDao SubJobDao => PlatformWindsorManager.GetService<IRMSubJobDao>();
        private IUserService UserService => PlatformWindsorManager.GetService<IUserService>();
        private IRMScopeRoleAssignmentDao RMScopeRoleAssignmentDao => PlatformWindsorManager.GetService<IRMScopeRoleAssignmentDao>();
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
        public IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao => PlatformWindsorManager.GetService<IArchiverIndexSubInfoDao>();
        private IRMRemoteNodeService RemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();
        public IAdvancedConditionsHandler _AdvancedConditionsHandler { get; set; }
        public IAdvancedConditionsHandler AdvancedConditionsHandler
        {
            get
            {
                if (_AdvancedConditionsHandler == null)
                {
                    _AdvancedConditionsHandler = new AdvancedConditionsHandler();
                    return _AdvancedConditionsHandler;
                }
                else
                {
                    return _AdvancedConditionsHandler;
                }
            }
            set { }
        }
        private IMCacheSettingService _CacheSettingService { get; set; }
        public IMCacheSettingService CacheSettingService
        {
            get
            {
                if (_CacheSettingService == null)
                {
                    _CacheSettingService = new CacheSettingService();
                    return _CacheSettingService;
                }
                else
                {
                    return _CacheSettingService;
                }
            }
            set { }
        }

        //public IMOffice365Service Office365Service { get; set; }
        //public IIndexService<ArchiverIndexServiceOpenParameter> IndexService { get; set; }
        public IEndUserRestoreSettingService EndUserSetting => PlatformWindsorManager.GetService<IEndUserRestoreSettingService>();
        private IArchiverSiteMasterIndexDao ArchiverSiteMasterIndexDao => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexDao>();
        private IArchiverSiteMasterIndexService ArchiverSiteMasterIndexService => PlatformWindsorManager.GetService<IArchiverSiteMasterIndexService>();
        private ICommonSiteMasterIndexDao CommonSiteMasterIndexDao => PlatformWindsorManager.GetService<ICommonSiteMasterIndexDao>();
        private ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();
        private IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();
        public IJobMonitorDao JMDao => PlatformWindsorManager.GetService<IJobMonitorDao>();
        private IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private IKeyValueService KeyValueService => PlatformWindsorManager.GetService<IKeyValueService>();
        private IRMRestoreSiteMappingDao RMRestoreSiteMappingDao => PlatformWindsorManager.GetService<IRMRestoreSiteMappingDao>();
        private IRMRemoteNodeDao RMRemoteNode => PlatformWindsorManager.GetService<IRMRemoteNodeDao>();
        private IRMGoogleRemoteNodeDao RMDriveRemoteNodeDao => PlatformWindsorManager.GetService<IRMGoogleRemoteNodeDao>();
        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private IRMCacheManager RMCacheManager => PlatformWindsorManager.GetService<IRMCacheManager>();
        private IStorageDeviceService StorageDeviceService => PlatformWindsorManager.GetService<IStorageDeviceService>();
        private IRMJobSizeAndCountStatisticsDao mRMJobSizeAndCountStatisticsDao = PlatformWindsorManager.GetService<IRMJobSizeAndCountStatisticsDao>();
        private IRMFileSystemRegisterService FSRegisterService => PlatformWindsorManager.GetService<IRMFileSystemRegisterService>();
        public ITreeNodeConverter TreeNodeConverter { get { return new TreeNodeConverter(); } set { } }
        private IFSMasterIndexService FSMasterIndexService => PlatformWindsorManager.GetService<IFSMasterIndexService>();
        private IRMArchiveSiteInfoDao ArchiveSiteInfoDao = PlatformWindsorManager.GetService<IRMArchiveSiteInfoDao>();

        private string BackUpJobId {  get; set; }
        
        private const string MultiSiteCollectionRestoreRunningKey = "RunMultiSiteCollectionRestoreTicket";
        private static readonly TimeSpan MultiSiteCollectionRestoreLockHeartbeatInterval = TimeSpan.FromHours(1);
        private static readonly TimeSpan MultiSiteCollectionRestoreLockExpiration = TimeSpan.FromHours(6);


        public bool IsEnableFullTextIndexSearch()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                var setting = RMKeyValueDao.GetValueByKey("ENABLE_ARCHIVE_FULL_TEXT_INDEX");
                if (setting != null && !string.IsNullOrWhiteSpace(setting.Value))
                {
                    if (bool.TryParse(setting.Value, out var enable) && enable)
                    {
                        return enable;
                    }
                }
                var licenseInfo = RMAosApiClient.GetLicenseInfo(TenantLocalValue.LogonGroupId).GetAwaiter().GetResult();
                if (licenseInfo.AdditionalProduct.HasFlag(PaidForProduct.OpusSO) && licenseInfo.StorageLicenseInfo != null)
                {
                    return licenseInfo.StorageLicenseInfo.EnableContentSearch;
                }
                    return false;
                }
            catch (Exception e)
            {
                logger.Error($"An error occurred while enable full text index search. Error: {e}");
                return false;
            }
            finally
            {
                stopwatch.Stop();
                logger.Info(@$"Check enable full text index search custom {stopwatch.ElapsedMilliseconds} milliseconds, Tenant id:{TenantLocalValue.LogonGroupId}, user id:{TenantLocalValue.LogonUserId}");
            }
        }

        public bool ForceEnableFullTextIndexInBackend()
        {
            try
            {
                var setting = RMKeyValueDao.GetValueByKey("ENABLE_ARCHIVE_FULL_TEXT_INDEX");
                if (setting != null && !string.IsNullOrWhiteSpace(setting.Value))
                {
                    if (bool.TryParse(setting.Value, out var enable))
                    {
                        return enable;
                    }
                }
                return false;
            }
            catch(Exception e)
            {
                logger.Error($"An error occurred while enable full text index search. Error: {e}");
                return false;
            }
        }

        public bool CanSendFullTextIndexJobMessage()
        {
            try
            {
                var enableFeature = false;
                var setting = RMKeyValueDao.GetValueByKey("ENABLE_ARCHIVE_FULL_TEXT_INDEX");

                if (setting != null && !string.IsNullOrWhiteSpace(setting.Value) && bool.TryParse(setting.Value, out var enable) && enable)
                {
                    //Enable in the background
                    enableFeature = true;
                }
                else
                {
                    var licenseInfo = RMAosApiClient.GetLicenseInfo(TenantLocalValue.LogonGroupId).GetAwaiter().GetResult();
                    if (licenseInfo.AdditionalProduct.HasFlag(PaidForProduct.OpusSO) && licenseInfo.StorageLicenseInfo != null)
                    {
                        if(licenseInfo.StorageLicenseInfo.EnableContentSearch)
                        {
                            //Enable in COP
                            enableFeature = true;
                        }
                    }
                }

                if(enableFeature)
                {
                    if (KeyValueService.IsSCBlackListForEdiscovery())
                    {
                        return true;
                    }
                    return RMRestoreSiteMappingDao.GetWhiteListCount() > 0;
                }

                return false;
            }
            catch(Exception e)
            {
                logger.Error($"An error occurred while check can send full text index job message. Error: {e}");
                return false;
            }
        }

        public bool HasReachedIndexSizeLimitation()
        {
            try
            {
                var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
                var info = client.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME).GetAwaiter().GetResult();
                if (info.Extension is Cloud.Sdk.Data.AosModern.CloudRecordsExtension)
                {
                    //These index field sizes are supported for all sale types.
                    Cloud.Sdk.Data.AosModern.CloudRecordsExtension extension = (Cloud.Sdk.Data.AosModern.CloudRecordsExtension)info.Extension;
                    int consumedIndexSize = extension.ConsumedIndexSize;
                    int purchasedIndexSize = extension.PurchasedIndexSize;
                    if (!extension.EnableContentSearch)
                    {
                        logger.Info($"Content search is not enabled in COP, no index size limitation. Consumed size: {consumedIndexSize}GB, purchased size: {purchasedIndexSize}GB.");
                        return false;
                    }
                    if (purchasedIndexSize == 0)
                    {
                        logger.Warn($"Content search is enabled, and purchased index size is 0, which means the current account is in preview mode. Consumed size: {consumedIndexSize}GB.");
                        return false;
                    }
                    if (consumedIndexSize <= purchasedIndexSize * 1.5)
                    {
                        logger.Info($"Not reached 150% index size limit yet, consumed size: {consumedIndexSize}GB, purchased size: {purchasedIndexSize}GB.");
                        return false;
                    }
                    logger.Warn($"Index size exceeded 150% limit! Consumed: {consumedIndexSize}GB, Purchased: {purchasedIndexSize}GB.");
                }
            }
            catch (Exception e)
            {
                logger.Error($"An error while checking index size limitation. Ex: {e}");
                return false;
            }
            return true;
        }

        public void SyncCategoryDataSize()
        {
            RMArchivedFullTextIndexCategoryManagement _categoryManagementService = new();
            _categoryManagementService.SyncCategoryDataSizeAsync().ExecuteAsyncTask();
        }

        public bool DisableCheckDestinationSiteInfo()
        {
            try
            {
                var setting = RMKeyValueDao.GetValueByKey("DisableCheckDestinationSiteInfo");
                if (setting != null && !string.IsNullOrWhiteSpace(setting.Value))
                {
                    if (bool.TryParse(setting.Value, out var enable))
                    {
                        return enable;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while DisableCheckDestinationSiteInfo. Error: {e}");
                return false;
            }
        }

        public List<TreeNode> GetSearchNodesFromMedia(List<ArchiverSiteMasterIndexContract> indexes, List<SiteCollectionNodesInfo> searchNodes, ArchiverRestoreFilter filterPolicy, int openIndexTimeoutInMs, ArchiverRestoreOrderBy orderBy)
        {
            var sitesMap = AssembleSearchParamInfo(indexes, searchNodes);
            var advancedSearchInfo = ConverToArchiverAdvancedInfo(sitesMap, filterPolicy);
            advancedSearchInfo.OpenIndexDbTimeoutInMs = openIndexTimeoutInMs;
            var advancedSearchService = new ArchiverAdvancedSearchService();
            var searchResult = advancedSearchService.Search(advancedSearchInfo, orderBy);
            return searchResult;
        }
        public List<ArchiverBasicIndex> GetFSSearchNodesFromMedia(List<FSMasterIndexContract> indexes, List<SiteCollectionNodesInfo> searchNodes, ArchiverRestoreFilter filterPolicy, int openIndexTimeoutInMs, ArchiverRestoreOrderBy orderBy)
        {
            var sitesMap = AssembleFSSearchParamInfo(indexes, searchNodes);
            var advancedSearchInfo = ConverToFSArchiverAdvancedInfo(sitesMap, filterPolicy);
            advancedSearchInfo.OpenIndexDbTimeoutInMs = openIndexTimeoutInMs;
            var advancedSearchService = new ArchiverAdvancedSearchService();
            var searchResult = advancedSearchService.SearchForFS(advancedSearchInfo, orderBy);
            return searchResult;
        }
        public List<TreeNode> GetGDriveSearchNodesFromMedia(List<ArchiverSiteMasterIndexContract> indexes, List<SiteCollectionNodesInfo> searchNodes, ArchiverRestoreFilter filterPolicy, int openIndexTimeoutInMs, ArchiverRestoreOrderBy orderBy)
        {
            var sitesMap = AssembleGDriveSearchParamInfo(indexes, searchNodes);
            var advancedSearchInfo = ConvertToGDriveArchiverAdvancedInfo(sitesMap, filterPolicy);
            advancedSearchInfo.OpenIndexDbTimeoutInMs = openIndexTimeoutInMs;
            var advancedSearchService = new ArchiverAdvancedSearchService();
            var searchResult = advancedSearchService.SearchForGoogle(advancedSearchInfo, orderBy);
            return searchResult;
        }
        public List<RMArchiveSiteInfo> GetSearchNodesFromMediaForJob(List<ArchiverSiteMasterIndexContract> indexes, List<SiteCollectionNodesInfo> searchNodes, ArchiverRestoreFilter filterPolicy)
        {
            var sitesMap = AssembleSearchParamInfo(indexes, searchNodes);
            var advancedSearchInfo = ConverToArchiverAdvancedInfo(sitesMap, filterPolicy);
            var advancedSearchService = new ArchiverAdvancedSearchService();
            var searchResult = advancedSearchService.SearchForJob(advancedSearchInfo);
            return searchResult;
        }
        public List<RMArchiveGDriveInfo> GetGDriveSearchNodesFromMediaForJob(List<ArchiverSiteMasterIndexContract> indexes, List<SiteCollectionNodesInfo> searchNodes, ArchiverRestoreFilter filterPolicy)
        {
            var sitesMap = AssembleGDriveSearchParamInfo(indexes, searchNodes);
            var advancedSearchInfo = ConvertToGDriveArchiverAdvancedInfo(sitesMap, filterPolicy);
            var advancedSearchService = new ArchiverAdvancedSearchService();
            var searchResult = advancedSearchService.SearchForGoogleJob(advancedSearchInfo);
            return searchResult;
        }

        private async Task<bool> CheckPermissionForSearchTree()
        {
            bool isOpusILAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOAdmin);
            if (isOpusILAdmin)
            {
                return true;
            }
            bool isOpusSOAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.SPOAdmin);
            if (isOpusSOAdmin)
            {
                return true;
            }
            bool isTeamsILEndUser = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsEndUser);
            bool isTeamsSOEndUser = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.TeamsEndUser);
            if(isTeamsILEndUser || isTeamsSOEndUser)
            {
                return true;
            }
            bool isGDRiveILEndUser = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleEndUser);
            bool isGControlLicense = await TenantService.HasInitGControlPlatForm();
            if (isGDRiveILEndUser || isGControlLicense)
            {
                return true;
            }
            var tempPermission = await SecurityTrimmingHelper.GetUserRestoreCenterFunctionPermissionAsync();
            if(tempPermission != FunctionSubPermission.None)
            {
                return true;
            }
            var permissionContainerIds = await GetPermissionContainerIdsAsync(RMBrowseTreeNodeSourceType.All);
            if (permissionContainerIds!=null && permissionContainerIds.Count>0)
            {
                return true;
            }
            return false;
        }

        private async Task<bool> CheckPermissionForTeamsSearchTree()
        {
            bool isOpusILAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsAdmin) || await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOAdmin);
            bool isOpusSOAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.TeamsAdmin) || await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.SPOAdmin);
            var tempPermission = await SecurityTrimmingHelper.GetUserRestoreCenterFunctionPermissionAsync();
            if (isOpusILAdmin || isOpusSOAdmin || tempPermission != FunctionSubPermission.None) return true;
            var permissionContainerIds = await GetPermissionContainerIdsAsync(RMBrowseTreeNodeSourceType.Teams);
            if (permissionContainerIds != null && permissionContainerIds.Count > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<ArchiverRestoreResult> GetSearchTreeResultAsync(ArchiverRestoreResult searchContract, bool needCheckPermission = true)
        {
            this.BackUpJobId = searchContract.SerchContract.BackupJobId;
            ArchiverRestoreResult re = new ArchiverRestoreResult();
            SiteCollectionNodesInfo node = searchContract.SerchContract.SearchNode;
            try
            {
                if (!needCheckPermission || ValidSiteCollectionsPermission([node.SiteUrl]).All(res => res.permission != FunctionSubPermission.None))
                {
                    logger.Info($"Do a rchiverRestore search, search node:{node.SiteUrl}.SiteGroupId:{node.SiteGroupId}.SPObjectId:{node.SPObjectId}.BackUpJobId:{this.BackUpJobId}.");
                    re = await HandleSearchCommonNodeAsync(searchContract, node,
                        new ArchiverRestoreOrderBy
                        {
                            ColName = searchContract.OrderBy,
                            Order = searchContract.IsDesc ? DocAveOnline.WebApi.Contracts.Order.Desc : DocAveOnline.WebApi.Contracts.Order.Asc
                        });
                    if (re.SerchContract?.FilterPolicy?.DataSource != null)
                    {
                        re.SerchContract.FilterPolicy.DataSource = (int)RestoreDataSource.M365;
                    }
                }
                else
                {
                    logger.Warn($"User:{TenantLocalValue.LogonUserId} has no permission to do Archiver restore search.");
                }
            }
            catch (AveException ex)
            {
                logger.Error("Archiver Restore searching failed:", ex.ToString());
                throw;
            }
            catch (OpenIndexDbTimeoutException ex)
            {
                re.Failed = true;
                re.Message = "WaitDownloadIndexDb";
                logger.Error(ex.Message);
            }
            catch (Exception ex)
            {
                re.Failed = true;
                logger.Error("Error occured while Archiver Restore searching:", ex.ToString());
            }
            if (re?.RestoreSerchNodes != null)
            {
                if (re.RestoreSerchNodes.Count() >= searchContract.PageSize)
                {
                    re.TotalNumber = int.MaxValue;
                }
                else
                {
                    re.TotalNumber = 0;
                }  
            }
            return re;
        }
        public async Task<ArchiverRestoreResult> GetDriveSearchTreeResultAsync(ArchiverRestoreResult searchContract, bool needCheckPermission = true, bool isControlPlus = false)
        {
            this.BackUpJobId = searchContract.SerchContract.BackupJobId;
            ArchiverRestoreResult re = new ArchiverRestoreResult();
            SiteCollectionNodesInfo node = searchContract.SerchContract.SearchNode;
            try
            {
                if (!needCheckPermission || await CheckPermissionForSearchTree())
                {
                    logger.Info($"Do google drive archiver restore search, search node:{node.SiteUrl}.SiteGroupId:{node.SiteGroupId}.SPObjectId:{node.SPObjectId}.BackUpJobId:{this.BackUpJobId}.");
                    re = await HandleGDriveSearchCommonNodeAsync(searchContract, node,
                        new ArchiverRestoreOrderBy
                        {
                            ColName = searchContract.OrderBy,
                            Order = searchContract.IsDesc ? DocAveOnline.WebApi.Contracts.Order.Desc : DocAveOnline.WebApi.Contracts.Order.Asc
                        }, isControlPlus);
                    if (re.SerchContract?.FilterPolicy?.DataSource != null)
                    {
                        re.SerchContract.FilterPolicy.DataSource = (int)RestoreDataSource.GoogleDrive;
                    }
                }
                else
                {
                    logger.Warn($"User:{TenantLocalValue.LogonUserId} has no permission to do google drive archiver restore search.");
                }
            }
            catch (AveException ex)
            {
                logger.Error("Archiver Restore searching failed:", ex.ToString());
                throw;
            }
            catch (OpenIndexDbTimeoutException ex)
            {
                re.Failed = true;
                re.Message = "WaitDownloadIndexDb";
                logger.Error(ex.Message);
            }
            catch (Exception ex)
            {
                logger.Error("Error occured while Archiver Restore searching:", ex.ToString());
            }
            return re;
        }
        public async Task<ArchiverRestoreResult> GetFSSearchResultAsync(ArchiverRestoreResult searchContract)
        {
            ArchiverRestoreResult re = new ArchiverRestoreResult();
            SiteCollectionNodesInfo node = searchContract.SerchContract.SearchNode;
            bool isOpusILAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOAdmin);
            bool isOpusSOAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.SPOAdmin);
            var tempPermission = await SecurityTrimmingHelper.GetUserRestoreCenterFunctionPermissionAsync();
            bool isFSAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.FSEnduser);
            try
            {
                if (isOpusILAdmin || isOpusSOAdmin || isFSAdmin || tempPermission != FunctionSubPermission.None)
                {
                    logger.Info($"Do a FS rchiverRestore search, search node:{node.SiteUrl}.ConnectionGroupId:{node.SiteGroupId}.ConncetionId:{node.SPObjectId}.BackUpJobId:{this.BackUpJobId}.");
                    re = await HandleFSSearchCommonNodeAsync(searchContract, node,
                        new ArchiverRestoreOrderBy
                        {
                            ColName = searchContract.OrderBy,
                            Order = searchContract.IsDesc ? DocAveOnline.WebApi.Contracts.Order.Desc : DocAveOnline.WebApi.Contracts.Order.Asc
                        });
                    //re.SerchContract?.FilterPolicy?.DataSource = (int)RestoreDataSource.FS;
                }
                else
                {
                    logger.Warn($"User:{TenantLocalValue.LogonUserId} has no permission to do FS restore search.");
                }
            }
            catch (AveException ex)
            {
                logger.Error("FS Archiver Restore searching failed:", ex.ToString());
                throw;
            }
            catch (OpenIndexDbTimeoutException ex)
            {
                re.Failed = true;
                re.Message = "WaitDownloadIndexDb";
                logger.Error(ex.Message);
            }
            catch (Exception ex)
            {
                logger.Error("Error occured while FS Archiver Restore searching:", ex.ToString());
            }
            if(re?.RestoreSerchNodes != null && re.RestoreSerchNodes.Count() >= searchContract.PageSize)
            {
                re.TotalNumber = int.MaxValue;
            }
            return re;
        }

        public async Task<ArchiverRestoreResult> GetSearchTeamsTreeResultAsync(ArchiverRestoreResult searchContract, bool needCheckPermission = true)
        {
            BackUpJobId = searchContract.SerchContract.BackupJobId;
            ArchiverRestoreResult result = new ArchiverRestoreResult();
            SiteCollectionNodesInfo node = searchContract.SerchContract.SearchNode;
            try
            {
                if (!needCheckPermission || await CheckPermissionForTeamsSearchTree())
                {
                    logger.Info($"Do archiverRestore search, search node:{node.SiteUrl}.SiteGroupId:{node.SiteGroupId}.SPObjectId:{node.SPObjectId}.BackUpJobId:{this.BackUpJobId}.");
                    var order = new ArchiverRestoreOrderBy
                    {
                        ColName = searchContract.OrderBy,
                        Order = searchContract.IsDesc ? DocAveOnline.WebApi.Contracts.Order.Desc : DocAveOnline.WebApi.Contracts.Order.Asc
                    };
                    result = await HandleTeamsSearchCommonNodeAsync(searchContract, node, order);
                    if (result.SerchContract?.FilterPolicy?.DataSource != null)
                    {
                        result.SerchContract.FilterPolicy.DataSource = (int)RestoreDataSource.Teams;
                    }
                }
                else
                {
                    logger.Warn($"User:{TenantLocalValue.LogonUserId} has no permission to do Archiver restore search.");
                }
            }
            catch (AveException ex)
            {
                logger.Error("Archiver Restore searching failed:", ex.ToString());
                throw;
            }
            catch (Exception ex)
            {
                logger.Error("Error occured while Archiver Restore searching:", ex.ToString());
            }
            return result;
        }

        public async Task<string> GetSearchTreeResultForJobAsync(List<ArchiverSiteMasterIndexContract> indexes, ArchiverRestoreResult filterPolicy, List<SiteCollectionNodesInfo> searchNodes)
        {
            var resultStr = string.Empty;
            try
            {
                var result = await HandleSearchCommonNodeForJobAsync(indexes, filterPolicy, searchNodes);
                resultStr = SerializerHelper.SerializeByDataContractSerializer(result);
            }
            catch (AveException ex)
            {
                logger.Error("Archiver Restore searching failed:", ex.ToString());
                throw;
            }
            catch (Exception ex)
            {
                logger.Error("Error occured while Archiver Restore searching:", ex.ToString());
            }
            return resultStr;
        }
        public RAReturnMessage SaveAndRunRestoreJob(RestoreInfo selectedTree, GCommon.Contract.StorageOptimization.Object.RestoreType restoreType, bool? runInWebRole = null)
        {
            logger.Info("RestoreSearchService start SaveAndRunRestoreJob.");
            RAReturnMessage msg = new RAReturnMessage();
            if (selectedTree.RestoreVersionOption == RestoreDocumentVersionsOption.SpecifyVersions && selectedTree.KeepVersionsNumber<=0)
            {
                msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                return msg;
            }

            bool isEndUserJob = runInWebRole != true && (selectedTree.IsEndUserJob || restoreType == GCommon.Contract.StorageOptimization.Object.RestoreType.AOPSOop);
            if (!isEndUserJob && selectedTree.NodeObjects is not null && selectedTree.NodeObjects.Count > 0)
            {
                var scUrls = selectedTree.NodeObjects.Select(node => !string.IsNullOrEmpty(node.SiteUrl) ? node.SiteUrl : node.SitePath);
                if (ValidSiteCollectionsPermission(scUrls).Any(res => res.permission != FunctionSubPermission.RestoreCenterFullControl))
                {
                    logger.Warn($"Part sc don't have permission, all sc url: {string.Join("; ", scUrls)}");
                    return new RAReturnMessage
                    {
                        MessageType = RAMessageType.Failed,
                        FaildType = RAFailedType.None,
                        ErrorMessage = I18NEntity.GetString("RM_AR_RestoreApp_ServiceAccountNotExsit_ErrorMessage"),
                    };
                }
            }

            RestoreSettingAndTree restoreTreeAndSetting = BuildRestoreSettingAndTree(selectedTree);

            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                //var loginName = jobRunBy == JobRunBy.Control ? TenantLocalValue.LogonUserEmail : "RM_TS_RunSchedule";
                string loginName = string.Empty;
                var jobRunby = JobRunBy.Schedule;
                if (selectedTree.IsEndUserJob)
                {
                    loginName = "RM_TS_RunSchedule";// + TenantLocalValue.LogonUserEmail;
                }
                else
                {
                    loginName = TenantLocalValue.LogonUserEmail;
                    jobRunby = JobRunBy.Control;
                }
                JobType tempType;
                if (restoreType == GCommon.Contract.StorageOptimization.Object.RestoreType.InPlace)
                {
                    tempType = JobType.ArchiverRestore;
                }
                else if (restoreType == GCommon.Contract.StorageOptimization.Object.RestoreType.StubOop)
                {
                    tempType = JobType.StubOopRestore;
                }
                else if (restoreType == GCommon.Contract.StorageOptimization.Object.RestoreType.AOPSOop)
                {
                    tempType = JobType.AOSPRestore;
                    TenantLocalValue.LogonUserEmail = "RM_TS_RunSchedule";
                }
                else if (restoreType == GCommon.Contract.StorageOptimization.Object.RestoreType.ToSPOLocation)
                {
                    tempType = JobType.ArchiverToSpoRestore;
                }
                else if (restoreType == GCommon.Contract.StorageOptimization.Object.RestoreType.ArchivedStubs)
                {
                    tempType = JobType.StubArchiverRestore;
                }
                else if (restoreType == GCommon.Contract.StorageOptimization.Object.RestoreType.M365InPlaceArchivedFiles)
                {
                    tempType = JobType.M365InPlaceArchiverRestore;
                }
                else
                {
                    tempType = JobType.ArchiverOutPlaceRestore;
                }
                if (runInWebRole != true && (selectedTree.IsEndUserJob || tempType == JobType.AOSPRestore))
                {
                    logger.Info("end user job,will create subjob and not insert into job queue");
                    msg.MessageType = RAMessageType.Successful;
                    msg.FaildType = RAFailedType.None;
                    msg.Extension = PlatformWindsorManager.GetService<IRestoreSearchService>().RealRunArchiverRestoreJob(jobRunby, loginName, SerializerHelper.SerializeByDataContractSerializer(restoreTreeAndSetting), tempType, selectedTree.JobPriority);
                    return msg;
                }
                else
                {
                    JobQueueDto jqDto = new JobQueueDto()
                    {
                        JobType = tempType,
                        //JobRunType = jobRunBy,
                        TenantGroupId = groupId,
                        JobRunByUser = loginName,
                        JobRunType = jobRunby,
                        Parameters = SerializerHelper.SerializeByDataContractSerializer(restoreTreeAndSetting),
                        JobPriority = selectedTree.JobPriority,
                    };
                    id = JobQueueService.AddToDBJobQueue(jqDto);
                }
                logger.Info($"RestoreSearchService finished SaveAndRunRestoreJob.JobType:{tempType}.LogonGroupId:{TenantLocalValue.LogonGroupId}.RealRunJobUser:{TenantLocalValue.LogonUserId}.JobQueueMessageId:{id}.");
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while ApplySettings,ERROR:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.FaildType = RAFailedType.None;
                msg.ErrorMessage = ex.Message;
            }

            return msg;
        }
        public RAReturnMessage SaveAndRunFSRestoreJob(RestoreInfo selectedItems, GCommon.Contract.StorageOptimization.Object.RestoreType restoreType, bool? runInWebRole = null)
        {
            logger.Info("FSRestoreSearchService start SaveAndRunRestoreJob.");
            RAReturnMessage msg = new RAReturnMessage();

            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;

                var loginName = TenantLocalValue.LogonUserEmail;
                var jobRunby = JobRunBy.Control;

                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.FSArchiverRestore,
                    //JobRunType = jobRunBy,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    JobRunType = jobRunby,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(selectedItems),
                    JobPriority = selectedItems.JobPriority
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);

                logger.Info($"FSRestoreSearchService finished SaveAndRunRestoreJob.JobType:{JobType.FSArchiverRestore}.LogonGroupId:{TenantLocalValue.LogonGroupId}.RealRunJobUser:{TenantLocalValue.LogonUserId}.JobQueueMessageId:{id}.");
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while FS ApplySettings,ERROR:{0}", ex.ToString());
            }

            return msg;
        }

        public RAReturnMessage SaveAndRunTeamsRestoreJob(RestoreInfo selectedTree, GCommon.Contract.StorageOptimization.Object.RestoreType restoreType, bool? runInWebRole = null)
        {
            logger.Info("TeamsRestoreJob start SaveAndRunTeamsRestoreJob.");
            RAReturnMessage msg = new RAReturnMessage();
            if (selectedTree.RestoreVersionOption == RestoreDocumentVersionsOption.SpecifyVersions && selectedTree.KeepVersionsNumber <= 0)
            {
                msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                return msg;
            }

            RestoreSettingAndTree restoreTreeAndSetting = BuildTeamsRestoreSettingAndTree(selectedTree);

            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                string loginName = string.Empty;
                var jobRunBy = JobRunBy.Schedule;
                if (selectedTree.IsEndUserJob)
                {
                    loginName = "RM_TS_RunSchedule";// + TenantLocalValue.LogonUserEmail;
                }
                else
                {
                    loginName = TenantLocalValue.LogonUserEmail;
                    jobRunBy = JobRunBy.Control;
                }
                if (runInWebRole != true && selectedTree.IsEndUserJob)
                {
                    logger.Info("end user job,will create subjob and not insert into job queue");
                    id = PlatformWindsorManager.GetService<IRestoreSearchService>().RealRunTeamsArchiverRestoreJob(jobRunBy, loginName, SerializerHelper.SerializeByDataContractSerializer(restoreTreeAndSetting), JobType.TeamsArchiverRestore);
                    msg.Extension = id;
                }
                else
                {
                    JobType jobType = JobType.TeamsArchiverRestore;
                    if (restoreTreeAndSetting.Setting.RestoreTypeSelect == GCommon.Contract.Server.Common.BackupDataSearch.RestoreType.OutOfPlace && selectedTree.RestoreObjectLevel != RestoreObjectLevel.Teams)
                    {
                        jobType = JobType.MailBoxArchiverRestore;
                    } 
                    else if(restoreTreeAndSetting.Setting.RestoreTypeSelect == GCommon.Contract.Server.Common.BackupDataSearch.RestoreType.OutOfPlace)
                    {
                        jobType = JobType.TeamsOutPlaceRestore;
                    }
                    JobQueueDto jqDto = new JobQueueDto()
                    {
                        JobType = jobType,
                        TenantGroupId = groupId,
                        JobRunByUser = loginName,
                        JobRunType = jobRunBy,
                        Parameters = SerializerHelper.SerializeByDataContractSerializer(restoreTreeAndSetting),
                        JobPriority = selectedTree.JobPriority
                    };
                    id = JobQueueService.AddToDBJobQueue(jqDto);
                }
                logger.Info($"TeamsRestoreSearchService finished SaveAndRunRestoreJob.JobType:{JobType.TeamsArchiverRestore}.LogonGroupId:{TenantLocalValue.LogonGroupId}.RealRunJobUser:{TenantLocalValue.LogonUserId}.JobQueueMessageId:{id}.");
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while ApplySettings,ERROR:{0}", ex.ToString());
            }

            return msg;
        }
        public RAReturnMessage SaveAndRunDriveRestoreJob(RestoreInfo selectedTree, GCommon.Contract.StorageOptimization.Object.RestoreType restoreType, bool? runInWebRole = null)
        {
            logger.Info("TeamsRestoreJob start SaveAndRunTeamsRestoreJob.");
            RAReturnMessage msg = new RAReturnMessage();
            if (selectedTree.RestoreVersionOption == RestoreDocumentVersionsOption.SpecifyVersions && selectedTree.KeepVersionsNumber <= 0)
            {
                msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                return msg;
            }

            GDriveRestoreSettingAndTree restoreTreeAndSetting = BuildGDriveRestoreSettingAndTree(selectedTree);

            string id = string.Empty;
            try
            {
                var groupId = TenantLocalValue.LogonGroupId;
                string loginName = string.Empty;
                var jobRunBy = JobRunBy.Schedule;
                if (selectedTree.IsEndUserJob)
                {
                    loginName = "RM_TS_RunSchedule";
                }
                else
                {
                    loginName = TenantLocalValue.LogonUserEmail;
                    jobRunBy = JobRunBy.Control;
                }
                
                JobType jobType = JobType.GoogleArchiverRestore;

                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName,
                    JobRunType = jobRunBy,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(restoreTreeAndSetting),
                    JobPriority = selectedTree.JobPriority
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
                
                logger.Info($"DriveRestoreSearchService finished SaveAndRunRestoreJob.JobType:{JobType.GoogleArchiverRestore}.LogonGroupId: {TenantLocalValue.LogonGroupId}.RealRunJobUser:{TenantLocalValue.LogonUserId}.JobQueueMessageId:{id}.");
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while ApplySettings,ERROR:{0}", ex.ToString());
            }

            return msg;
        }
        private RestoreSettingAndTree BuildTeamsRestoreSettingAndTree(RestoreInfo selectedTree)
        {
            List<TreeNode> temp = new List<TreeNode>();
            foreach (var tr in selectedTree.NodeObjects)
            {
                TreeNode tree = SerializerHelper.DeserializeByDataContractSerializer<TreeNode>(tr.TreeNode);
                var treeClone = Clone(tree);
                tree.Depth = 0; //CaculateDepth(treeClone) // only Teams node
                //SetIsSelectTreeNode(tree);
                tree.IsSelectNode = true;
                temp.Add(tree);
            }
            TreeNode teamsTreeNode = SerializerHelper.DeserializeByDataContractSerializer<TreeNode>(selectedTree.NodeObjects.FirstOrDefault()?.TreeNode);
            //var treeLevelClone = Clone(teamsTreeNode);
            //while (true)
            //{
            //    if (treeLevelClone.Children != null && treeLevelClone.Children.Count > 0)
            //    {
            //        treeLevelClone = treeLevelClone.Children[0];
            //    }
            //    else
            //    {
            //        break;
            //    }
            //}
            //SPTreeNodeDto
            //List<TreeNode> resultChildren = AdvancedConditionsHandler.AssembleTreeByAdvancedConditions(BubbleSort(temp, teamsTreeNode.TreeNodeLevel), "(1)");
            var tempResult = TreeNodeConverter.ConvertTreeNodeListToTeamsTreeNodeList(temp, ConvertTeamsNodeLevel(teamsTreeNode.TreeNodeLevel));
            //var realResult = ExtractResult(tempResult);

            List<DB.Model.CommonSiteMasterIndex> indexes = CommonSiteMasterIndexDao.GetAllSiteCollectionNodsInfoByUrl(temp.FirstOrDefault()?.FullPath);
            var teamsIndex = indexes.FirstOrDefault();
            string teamsGroupId = string.Empty;
            if (teamsIndex != null)
            {
                teamsGroupId = teamsIndex.SiteGroupId;
                var realSite = RMRemoteNode.GetTeamsGroupAndChannelsCollectionByTeamsId(teamsIndex.TeamId).Item1;
                if (realSite != null)
                {
                    teamsGroupId = realSite.parentId;
                    tempResult?.ForEach(node => node.SPObjectId = realSite.TeamId);
                }
            }
            else
            {
                logger.Warn($"Get TeamsGroupId failed, CommonSiteMasterIndex is null or count <0,site url:{tempResult.FirstOrDefault()?.SitePath}");
            }
            return new RestoreSettingAndTree()
            {
                Tree = tempResult,
                Setting = selectedTree,
                JobId = selectedTree.JobId,
                SiteGroupId = teamsGroupId,
                IsEndUserJob = selectedTree.IsEndUserJob,
                ConnectionString = selectedTree.ConnectionString,
                NodeType = selectedTree.NodeType,
                IsOpusArchivedDownloadJob = selectedTree.IsOpusArchivedDownloadJob,
                RealRunJobUser = TenantLocalValue.LogonUserEmail,
                IsRecenterExport = selectedTree.IsRecenterExport,
                oopStubUrl = selectedTree.OopStubUrl,
                BackUpJobId = selectedTree.BackUpJobId,
            };
        }

        private RestoreSettingAndTree BuildRestoreSettingAndTree(RestoreInfo selectedTree)
        {
            List<TreeNode> temp = new List<TreeNode>();
            List<SPTreeNodeDto> realResult = new List<SPTreeNodeDto>();
            string siteGroupId = string.Empty;
            if (string.IsNullOrEmpty(selectedTree.FailedJobId))
            {
                if ((selectedTree?.NodeObjects == null || selectedTree.NodeObjects.Count == 0) && selectedTree?.RestoreExecutionRequest != null)
                {
                    return BuildDeferredPublicApiRestoreSettingAndTree(selectedTree);
                }

                PreprocessingSelectedNodes(selectedTree.NodeObjects);
                foreach (var tr in selectedTree.NodeObjects)
                {
                    TreeNode tree = SerializerHelper.DeserializeByDataContractSerializer<TreeNode>(tr.TreeNode);
                    var treeClone = Clone(tree);
                    tree.Depth = CaculateDepth(treeClone);
                    SetIsSelectTreeNode(tree);
                    temp.Add(tree);
                }
                TreeNode treeLevel = SerializerHelper.DeserializeByDataContractSerializer<TreeNode>(selectedTree.NodeObjects.FirstOrDefault()?.TreeNode);
                var treeLevelClone = Clone(treeLevel);
                while (true)
                {
                    if (treeLevelClone.Children != null && treeLevelClone.Children.Count > 0)
                    {
                        treeLevelClone = treeLevelClone.Children[0];
                    }
                    else
                    {
                        break;
                    }
                }
                //SPTreeNodeDto
                List<TreeNode> resultChildren = AdvancedConditionsHandler.AssembleTreeByAdvancedConditions(BubbleSort(temp, treeLevelClone.TreeNodeLevel), "(1)");
                var tempResult = TreeNodeConverter.ConvertTreeNodeListToSPTreeNodeList(resultChildren, ConverNodeLevel(treeLevelClone.TreeNodeLevel));
                realResult = ExtractResult(tempResult);
                Stopwatch sw = new Stopwatch();
                sw.Start();
                List<DB.Model.ArchiverSiteMasterIndex> index = ArchiverSiteMasterIndexDao.GetAllSiteCollectionNodsInfoByUrl(realResult.FirstOrDefault()?.SitePath);
                if (index != null && index.Count > 0)
                {
                    siteGroupId = index.FirstOrDefault()?.SiteGroupId;
                    var realSite = RMRemoteNode.GetRemoteSiteCollectionByUrl(index.FirstOrDefault()?.SiteURL);
                    if (realSite != null)
                    {
                        siteGroupId = realSite.parentId;
                        if (RMKeyValueDao.HasUpgradeTeams() && siteGroupId.Equals(RMConstants.DefaultPrivateChannelSitesGroupId, StringComparison.OrdinalIgnoreCase))
                        {
                            logger.Info($"The account has upgrade teams and the current site under the default channel container.");
                            var (teamsNode, listSiteNode) = RMRemoteNode.GetTeamsGroupAndChannelsCollectionByTeamsId(realSite.TeamId);
                            logger.Info($"teams is null: {teamsNode == null}");
                            if (teamsNode != null)
                            {
                                siteGroupId = teamsNode.parentId;
                            }
                        }
                        if (tempResult != null)
                        {
                            tempResult.ForEach(node => node.SPObjectId = realSite.ObjectId);
                        }
                    }
                    else
                    {
                        siteGroupId = string.Empty;
                    }
                }
                else
                {
                    logger.Warn($"Get sitegroupId failed,siteMasterIndex is null or count <0,site url:{realResult.FirstOrDefault()?.SitePath}");
                }
                sw.Stop();
                logger.Info($"linkRestoreReport BuildRestoreSettingAndTree query db cost time:{sw.ElapsedMilliseconds}");
                return new RestoreSettingAndTree()
                {
                    Tree = realResult,
                    Setting = selectedTree,
                    JobId = selectedTree.JobId,
                    SiteGroupId = siteGroupId,
                    IsEndUserJob = selectedTree.IsEndUserJob,
                    ConnectionString = selectedTree.ConnectionString,
                    NodeType = selectedTree.NodeType,
                    IsOpusArchivedDownloadJob = selectedTree.IsOpusArchivedDownloadJob,
                    RealRunJobUser = TenantLocalValue.LogonUserEmail,
                    IsRecenterExport = selectedTree.IsRecenterExport,
                    oopStubUrl = selectedTree.OopStubUrl,
                    BackUpJobId = selectedTree.BackUpJobId,
                    IsSearchAllRestore = selectedTree.SerchContract != null,
                };
            }
            else
            {
                var failedRestoreJob = JMDao.GetJobById(selectedTree.FailedJobId);
                RestoreSettingAndTree result = new RestoreSettingAndTree();
                logger.Info($"this is failed restore rerun job ,failed job id:{selectedTree.FailedJobId},site url:{selectedTree.SiteUrl}");
                if (!string.IsNullOrEmpty(failedRestoreJob.Extension))
                {
                    result = SerializerHelper.DeserializeByJsonSerializer<RestoreSettingAndTree>(failedRestoreJob.Extension);
                    result.Setting.FailedJobId = selectedTree.FailedJobId;
                }
                else
                {
                    logger.Warn($"the failed restore job not exist restore setting,failedid:{selectedTree.FailedJobId}");
                    result.Setting = new RestoreInfo();
                    result.Setting.FailedJobId = selectedTree.FailedJobId;
                }
                return result;
            }
        }

        private RestoreSettingAndTree BuildDeferredPublicApiRestoreSettingAndTree(RestoreInfo selectedTree)
        {
            string siteUrl = selectedTree.RestoreExecutionRequest?.Scope ?? string.Empty;
            selectedTree.SiteUrl = siteUrl;

            string siteGroupId = string.Empty;
            if (!string.IsNullOrWhiteSpace(siteUrl))
            {
                siteGroupId = ArchiverSiteMasterIndexDao.GetRestoringSiteCollectionInfoByUrl(siteUrl)?.SiteGroupId ?? string.Empty;
            }

            return new RestoreSettingAndTree()
            {
                Tree = new List<SPTreeNodeDto>(),
                Setting = selectedTree,
                JobId = selectedTree.JobId,
                SiteGroupId = siteGroupId,
                IsEndUserJob = selectedTree.IsEndUserJob,
                ConnectionString = selectedTree.ConnectionString,
                NodeType = selectedTree.NodeType,
                IsOpusArchivedDownloadJob = selectedTree.IsOpusArchivedDownloadJob,
                RealRunJobUser = TenantLocalValue.LogonUserEmail,
                IsRecenterExport = selectedTree.IsRecenterExport,
                oopStubUrl = selectedTree.OopStubUrl,
                BackUpJobId = selectedTree.BackUpJobId,
                IsSearchAllRestore = selectedTree.SerchContract != null,
            };
        }
        private GDriveRestoreSettingAndTree BuildGDriveRestoreSettingAndTree(RestoreInfo selectedTree)
        {
            List<TreeNode> temp = new List<TreeNode>();
            foreach (var tr in selectedTree.NodeObjects)
            {
                TreeNode tree = SerializerHelper.DeserializeByDataContractSerializer<TreeNode>(tr.TreeNode);
                var treeClone = Clone(tree);
                tree.Depth = CaculateDepth(treeClone);
                SetIsSelectTreeNode(tree);
                temp.Add(tree);
            }
            TreeNode treeLevel = SerializerHelper.DeserializeByDataContractSerializer<TreeNode>(selectedTree.NodeObjects.FirstOrDefault()?.TreeNode);
            var treeLevelClone = Clone(treeLevel);
            while (true)
            {
                if (treeLevelClone.Children != null && treeLevelClone.Children.Count > 0)
                {
                    treeLevelClone = treeLevelClone.Children[0];
                }
                else
                {
                    break;
                }
            }
            //SPTreeNodeDto
            var advancedConditionsHandler = new AdvancedConditionsHandler(isGoogle: true);
            List<TreeNode> resultChildren = advancedConditionsHandler.AssembleTreeByAdvancedConditions(BubbleSort(temp, treeLevelClone.TreeNodeLevel), "(1)");
            var tempResult = TreeNodeConverter.ConvertTreeNodeListToGDriveTreeNodeList(resultChildren, ConvertGDriveNodeLevel(treeLevelClone.TreeNodeLevel));
            var realResult = tempResult;//ExtractResult(tempResult); google has no virtual node

            List<DB.Model.ArchiverSiteMasterIndex> index = ArchiverSiteMasterIndexDao.GetAllSiteCollectionNodsInfoByUrl(realResult.FirstOrDefault()?.FullPath);
            string siteGroupId = string.Empty;
            if (index != null && index.Count > 0)
            {
                siteGroupId = index.FirstOrDefault()?.SiteGroupId;
                var realSite = RMDriveRemoteNodeDao.GetGoogleDriveById(index.FirstOrDefault()?.SiteId);
                if (realSite != null)
                {
                    siteGroupId = realSite.ContainerId;
                    if (tempResult != null)
                    {
                        tempResult.ForEach(node => node.ObjectId = realSite.ObjectId);
                    }
                }
            }
            else
            {
                logger.Warn($"Get sitegroupId failed,siteMasterIndex is null or count <0,site url:{realResult.FirstOrDefault()?.FullPath}");
            }
            return new GDriveRestoreSettingAndTree()
            {
                Tree = realResult,
                Setting = selectedTree,
                JobId = selectedTree.JobId,
                SiteGroupId = siteGroupId,
                IsEndUserJob = selectedTree.IsEndUserJob,
                ConnectionString = selectedTree.ConnectionString,
                NodeType = selectedTree.NodeType,
                IsOpusArchivedDownloadJob = selectedTree.IsOpusArchivedDownloadJob,
                RealRunJobUser = TenantLocalValue.LogonUserEmail,
                IsRecenterExport = selectedTree.IsRecenterExport,
                oopStubUrl = selectedTree.OopStubUrl,
                BackUpJobId = selectedTree.BackUpJobId,
            };
        }
        private void PreprocessingSelectedNodes(List<ArchiverRestoreSerchResult> selectedNodes)
        {
            if (selectedNodes == null || selectedNodes.Count == 0)
            {
                return;
            }

            selectedNodes.RemoveAll(node => node == null);
            if (selectedNodes.Count == 0)
            {
                return;
            }

            var groups = selectedNodes.GroupBy(x => x.PathMd5);

            foreach (var group in groups)
            {
                if (group.Count() > 1)
                {
                    var maxTimeItem = group.OrderByDescending(x => x.ArchiveTime).First();
                    int maxIndex = selectedNodes.IndexOf(maxTimeItem);
                    int firstIndex = selectedNodes.IndexOf(group.First());
                    if (maxIndex != firstIndex)
                    {
                        var temp = selectedNodes[firstIndex];
                        selectedNodes[firstIndex] = selectedNodes[maxIndex];
                        selectedNodes[maxIndex] = temp;
                    }
                }
            }
        }
        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.SimulateRunArchiverRestoreJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public RAReturnMessage SaveAndRunSimulateRestoreJob(RestoreInfo selectedTree)
        {
            logger.Info("RestoreSearchService start SaveAndRunsimulateRestoreJob.");
            RAReturnMessage msg = new RAReturnMessage();

            StopAllRunningOrWaitSimulateRestoreJob();

            RestoreSettingAndTree restoreSettingAndTree = BuildRestoreSettingAndTree(selectedTree);
            restoreSettingAndTree.JobId = RMJobService.GenerateJobId(JobType.SimulateRestore)+"_000";
            msg.Extension = restoreSettingAndTree.JobId;

            JobQueueDto jqDto = new JobQueueDto()
            {
                JobType = JobType.SimulateRestore,
                TenantGroupId = TenantLocalValue.LogonGroupId,
                JobRunByUser = TenantLocalValue.LogonUserEmail,
                JobRunType = JobRunBy.Control,
                Parameters = SerializerHelper.SerializeByJsonConvert(restoreSettingAndTree),
            };
            string id = JobQueueService.AddToDBJobQueue(jqDto);

            logger.Info($"RestoreSearchService finished SaveAndRunsimulateRestoreJob.JobType:{JobType.SimulateRestore}.LogonGroupId:{TenantLocalValue.LogonGroupId}.RealRunJobUser:{TenantLocalValue.LogonUserId}.JobQueueMessageId:{id}.");
            if (string.IsNullOrEmpty(id))
            {
                msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }
            return msg;
        }

        private void StopAllRunningOrWaitSimulateRestoreJob()
        {
            List<JobQueueDto> jobQueueMessage = JobQueueService.GetDBJobQueueMessage(TenantLocalValue.LogonGroupId, TenantLocalValue.LogonUserEmail, JobType.SimulateRestore);
            if (jobQueueMessage.Count() > 0)
            {
                foreach (JobQueueDto message in jobQueueMessage)
                {
                    try
                    {
                        string subJobId = SerializerHelper.DeserializeByJsonConvert<RestoreSettingAndTree>(message.Parameters).JobId;
                        CreateSubJob(subJobId, null, JobType.SimulateRestore, 1, message.Parameters, true, TenantLocalValue.LogonUserEmail, null, JobStatus.Stopped);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"Fail create stop status subjob,ex:{ex}");
                    }
                    JobQueueService.DeleteDBJobQueueMessage(message.MessageId, message.TenantGroupId);
                }
            }

            List<RMSubJob> runningJobs = SubJobDao.GetRunningAndRunnableSubJobListAsync(JobType.SimulateRestore).GetAwaiter().GetResult();
            foreach (var runningJob in runningJobs)
            {
                if (runningJob.String1 == TenantLocalValue.LogonGroupEmail)
                {
                    if(runningJob.Status == (int)JobStatus.Wait)
                    {
                        SubJobDao.UpdateStatus(runningJob.Id, (int)JobStatus.Stopped, DateTime.UtcNow.Ticks);
                    }
                    else
                    {
                        SubJobDao.UpdateStatus(runningJob.Id, (int)JobStatus.Stopping, DateTime.UtcNow.Ticks);
                    }
                }
            }
        }

        /// <summary>
        /// Restricts each tenant to a limited number of preview restore requests within a rolling time window,
        /// using a Redis-backed counter that is tenant-scoped (via IRMCache's BuildTenantKey) and re-armed on every call.
        /// </summary>
        public async Task<RAReturnMessage> CheckPreviewRestoreRateLimitAsync()
        {
            int count = await RMCacheManager.Cache.GetAsync<int?>(IRMCache.Keys.PreviewRestorePerMinuteCount) ?? 0;
            if (count >= RMConstants.PreviewRestorePerMinuteLimit)
            {
                logger.Warn($"Tenant:{TenantLocalValue.LogonGroupId} exceeded preview restore rate limit:{RMConstants.PreviewRestorePerMinuteLimit} per minute.");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, ErrorMessage = I18NEntity.GetString("RM_AR_PreviewRestore_RateLimitExceeded_ErrorMessage", RMConstants.PreviewRestorePerMinuteLimit) };
            }
            await RMCacheManager.Cache.SetAsync(IRMCache.Keys.PreviewRestorePerMinuteCount, count + 1, TimeSpan.FromMinutes(1));
            return null;
        }

        public RAReturnMessage PreviewRestore(List<RestoreInfo> selectedTrees)
        {
            logger.Info("RestoreSearchService start PreviewRestore.");
            RAReturnMessage msg = new RAReturnMessage();
            if (selectedTrees == null || selectedTrees.Count == 0)
            {
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }
            foreach (var selectedTree in selectedTrees)
            {
                if (selectedTree.RestoreVersionOption == RestoreDocumentVersionsOption.SpecifyVersions && selectedTree.KeepVersionsNumber <= 0)
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
                if (selectedTree.NodeObjects is not null && selectedTree.NodeObjects.Count > 0)
                {
                    var scUrls = selectedTree.NodeObjects.Select(node => !string.IsNullOrEmpty(node.SiteUrl) ? node.SiteUrl : node.SitePath);
                    if (ValidSiteCollectionsPermission(scUrls).Any(res => res.permission != FunctionSubPermission.RestoreCenterFullControl))
                    {
                        logger.Warn($"Part sc don't have permission, all sc url: {string.Join("; ", scUrls)}");
                        return new RAReturnMessage
                        {
                            MessageType = RAMessageType.Failed,
                            FaildType = RAFailedType.None,
                            ErrorMessage = I18NEntity.GetString("RM_AR_RestoreApp_ServiceAccountNotExsit_ErrorMessage"),
                        };
                    }
                }
            }
            List<RestoreSettingAndTree> restoreTreeAndSettings = selectedTrees.Select(BuildRestoreSettingAndTree).ToList();
            return QueuePreviewRestoreJob(restoreTreeAndSettings, selectedTrees.First().JobPriority);
        }

        private RAReturnMessage QueuePreviewRestoreJob(List<RestoreSettingAndTree> restoreTreeAndSettings, JobPriority jobPriority)
        {
            RAReturnMessage msg = new RAReturnMessage();
            string id = string.Empty;
            try
            {
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.PreviewRestore,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail,
                    JobRunType = JobRunBy.Control,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(restoreTreeAndSettings),
                    JobPriority = jobPriority,
                };
                id = JobQueueService.AddToDBJobQueue(jqDto);
                logger.Info($"RestoreSearchService finished PreviewRestore.JobType:{JobType.PreviewRestore}.LogonGroupId:{TenantLocalValue.LogonGroupId}.RealRunJobUser:{TenantLocalValue.LogonUserId}.JobQueueMessageId:{id}.");
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
                else
                {
                    msg.MessageType = RAMessageType.Successful;
                    msg.Extension = id;
                }
            }
            catch (Exception ex)
            {
                logger.Error("error occurred while PreviewRestore,ERROR:{0}", ex.ToString());
                msg.MessageType = RAMessageType.Failed;
                msg.FaildType = RAFailedType.None;
                msg.ErrorMessage = ex.Message;
            }
            return msg;
        }

        public string RealRunPreviewRestoreJob(JobRunBy jobRunBy, string jobRunByUser, string param, string messageId)
        {
            JobType jobType = JobType.PreviewRestore;
            string previewJobId = RMJobService.GenerateJobId(jobType);
            // The message id travels together with the restore tree list inside Extension, so the job runner
            // does not need to also parse it out of CommandLine.
            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = previewJobId,
                RunBy = jobRunBy,
                JobType = jobType,
                CommandLine = string.Format("{0} {1}", jobType, previewJobId),
                Extension = SerializerHelper.SerializeByDataContractSerializer(new KeyValuePair<string, string>(messageId, param)),
            });
            logger.Info($"Create virtual sub job {previewJobId} sucessfull, type PreviewRestore.PreviewRestoreJobID:{previewJobId}.JobQueueMessageId:{messageId}.");
            return previewJobId;
        }

        public async Task<RAReturnMessage> GetPreviewRestoreResult(string messageId)
        {
            RAReturnMessage msg = new RAReturnMessage();
            if (string.IsNullOrEmpty(messageId))
            {
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, ErrorMessage = "messageId is required" };
            }
            // The preview restore data size job has no RMSubJob database record, so the result is read back
            // from Redis cache (written by AveItemPreviewRestoreMain once the job finishes) instead of DB.
            SimulateResotreResult result = await RMCacheManager.Cache.GetAsync<SimulateResotreResult>(IRMCache.Keys.PreviewRestoreResult + messageId);
            msg.MessageType = RAMessageType.Successful;
            msg.Extsion1 = result;
            return msg;
        }

        public RAReturnMessage PreviewMultiSiteCollectionRestoreAsync(RestoreInfo info)
        {
            logger.Info("RestoreSearchService start PreviewMultiSiteCollectionRestoreAsync.");

            if (info.DataSource != (int)RestoreDataSource.M365)
            {
                logger.Error($"preview restore data size only supports M365 data source,current data source:{info.DataSource}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, ErrorMessage = I18NEntity.GetString("RM_AR_PreviewRestore_UnsupportedDataSourceType_ErrorMessage") };
            }

            List<ArchiverRestoreSerchResult> siteCollections = ExtractSiteCollections(info);
            if (siteCollections.Count > RMConstants.PreviewRestoreMaxSelectedObjectCount)
            {
                logger.Warn($"selected site collections count:{siteCollections.Count} exceeds the max limit:{RMConstants.PreviewRestoreMaxSelectedObjectCount},can not run preview restore data size job.");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.ParameterIsIncorrect, ErrorMessage = I18NEntity.GetString("RM_AR_PreviewRestore_MaxSelectedObjectsExceeded_ErrorMessage", RMConstants.PreviewRestoreMaxSelectedObjectCount) };
            }

            if (siteCollections.Count == 0)
            {
                logger.Warn("No site collection selected, can not run preview restore data size job.");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, ErrorMessage = I18NEntity.GetString("RM_AR_PreviewRestore_NoSiteCollectionSelected_ErrorMessage") };
            }

            // Defer the per-site index search/tree-build to the worker (same JobType.PreviewRestore job,
            // executed by AveItemPreviewRestoreMain) instead of doing it synchronously in RAWeb, so a large
            // selection can't block or time out the web request. This applies even for a single site collection.
            List<RestoreSettingAndTree> pendingTreeAndSettings = siteCollections
                .Select(siteCollection => BuildPendingPreviewRestoreSettingAndTree(info, siteCollection))
                .ToList();
            logger.Info($"deferring preview restore data size search/tree-build to the worker for {pendingTreeAndSettings.Count} site collection(s).");
            return QueuePreviewRestoreJob(pendingTreeAndSettings, info.JobPriority);
        }

        public RAReturnMessage HaveRunningSimulateRestoreJob()
        {
            RAReturnMessage msg = new RAReturnMessage();
            List<JobQueueDto> jobQueues = JobQueueService.GetDBJobQueueMessage(TenantLocalValue.LogonGroupId, TenantLocalValue.LogonUserEmail, JobType.SimulateRestore);
            if (jobQueues.Count() > 0)
            {
                msg.Extsion1 = true;
            }
            else
            {
                List<RMSubJob> runningJobs = SubJobDao.GetRunningAndRunnableSubJobListAsync(JobType.SimulateRestore).GetAwaiter().GetResult();
                if (runningJobs.Exists(job => job.String1 == TenantLocalValue.LogonUserEmail))
                {
                    msg.Extsion1 = true;
                }
                else
                {
                    msg.Extsion1 = false;
                }
            }

            return msg;
        }

        public RAReturnMessage GetSimulareRestoreJobResult(string jobId)
        {
            RAReturnMessage msg = new RAReturnMessage();
            List<JobQueueDto> jobQueueMessages = JobQueueService.GetDBJobQueueMessage(TenantLocalValue.LogonGroupId, TenantLocalValue.LogonUserEmail, JobType.SimulateRestore);
            if (jobQueueMessages.Exists(message => SerializerHelper.DeserializeByJsonConvert<RestoreSettingAndTree>(message.Parameters).JobId == jobId))
            {
                msg.Extension = ((int)JobStatus.Wait).ToString();
            }
            else
            {
                RMSubJob job = SubJobDao.GetSubJob(jobId);
                if (job == null)
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty, ErrorMessage = I18NEntity.GetString("RM_RS_UnablefoundMappingSimulateResotre") };
                }
                else if (job.String1 != TenantLocalValue.LogonUserEmail)
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty, ErrorMessage = I18NEntity.GetString("RM_RS_OnlyCanGetYourSelfSimulateRestore") };
                }
                else
                {
                    switch (job.Status)
                    {
                        case (int)JobStatus.Finished:
                            msg.Extsion1 = SerializerHelper.DeserializeByJsonConvert<SimulateResotreResult>(job.Comment);
                            break;
                        case (int)JobStatus.Wait:
                        case (int)JobStatus.InProgress:
                        case (int)JobStatus.Stopping:
                        case (int)JobStatus.Stopped:
                            break;
                        case (int)JobStatus.Failed:
                        default:
                            msg.MessageType = RAMessageType.Failed;
                            msg.ErrorMessage = I18NEntity.GetString("RM_RS_SimulateRestoreFail");
                            break;
                    }
                    msg.Extension = job.Status.ToString();
                }
            }
            return msg;
        }
        private NodeLevel ConverNodeLevel(TreeNodeLevel tLevel)
        {
            NodeLevel nodeLevel = NodeLevel.Item;
            if (tLevel == TreeNodeLevel.Folder)
                nodeLevel = NodeLevel.Folder;
            if (tLevel == TreeNodeLevel.Site)
                nodeLevel = NodeLevel.Site;
            if (tLevel == TreeNodeLevel.List)
                nodeLevel = NodeLevel.List;
            return nodeLevel;
        }
        private NodeLevel ConvertGDriveNodeLevel(TreeNodeLevel tLevel)
        {
            return tLevel switch
            {
                TreeNodeLevel.GoogleMyDrive => NodeLevel.GoogleMyDrive,
                TreeNodeLevel.GoogleSharedDrive => NodeLevel.GoogleSharedDrive,
                TreeNodeLevel.GoogleDriveFolder => NodeLevel.GoogleFolder,
                TreeNodeLevel.GoogleDriveFile => NodeLevel.GoogleFile,
                _ => NodeLevel.GoogleFile,
            };
        }
        private void SetIsSelectTreeNode(TreeNode treeNode)
        {
            TreeNode temp = treeNode;
            while (temp.Children.Count > 0)
            { 
               temp = temp.Children[0];
            }
            temp.IsSelectNode = true;
            return;
        }
        private List<TreeNode> SortItems(List<TreeNode> items)
        {
            items.Sort((x, y) =>
            {
                TreeNode tempX = Clone(x);
                TreeNode tempY = Clone(y);
                while (true)
                {
                    if (tempX.Children != null && tempX.Children.Count > 0)
                    {
                        tempX = tempX.Children[0];
                    }
                    else
                    {
                        break;
                    }
                }
                while (true)
                {
                    if (tempY.Children != null && tempY.Children.Count > 0)
                    {
                        tempY = tempY.Children[0];
                    }
                    else
                    {
                        break;
                    }
                }
                string tempNameX = tempX.Name;
                string tempNameY = tempY.Name;
                if (tempX.Name.Contains(":"))
                {
                    tempNameX = tempNameX.Substring(0, tempNameX.IndexOf(":"));
                }
                if (tempY.Name.Contains(":"))
                {
                    tempNameY = tempNameY.Substring(0, tempNameY.IndexOf(":"));
                }
                int result = string.Compare(tempNameX, tempNameY, StringComparison.OrdinalIgnoreCase);
                if (result == 0)
                {
                    if (ItemMajorVersion(tempX) < ItemMajorVersion(tempY))
                        result = -1;
                    else if (ItemMajorVersion(tempX) > ItemMajorVersion(tempY))
                        result = 1;
                    else if (Math.Abs(ItemMajorVersion(tempX) - ItemMajorVersion(tempY)) < 1E-06)
                    {
                        if (ItemMinorVersion(tempX) < ItemMinorVersion(tempY))
                            result = -1;
                        else if (ItemMinorVersion(tempX) > ItemMinorVersion(tempY))
                            result = 1;
                        else
                        {
                            if (string.Compare(tempX.TreeNodeLevel.ToString(), tempY.TreeNodeLevel.ToString(), StringComparison.OrdinalIgnoreCase) > 0)
                                result = -1;
                            else
                                result = 0;
                        }
                    }
                }
                return result;
            });
            return items;
        }
        private float ItemMajorVersion(TreeNode node)
        {
            float majorVersion = float.MaxValue;
            if (node.TreeNodeLevel == TreeNodeLevel.Item || node.TreeNodeLevel == TreeNodeLevel.Document)
            {
                int flag = node.Name.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
                if (flag >= 0)
                {
                    string versionStr = node.Name.Substring(flag + 1);
                    String[] version = versionStr.Split('.');
                    if (!float.TryParse(version[0], out majorVersion))
                    {
                        majorVersion = float.MaxValue;
                    }
                }
            }
            return majorVersion;
        }
        private float ItemMinorVersion(TreeNode node)
        {
            float minorVersion = float.MaxValue;
            if (node.TreeNodeLevel == TreeNodeLevel.Item || node.TreeNodeLevel == TreeNodeLevel.Document)
            {
                int flag = node.Name.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
                if (flag >= 0)
                {
                    string versionStr = node.Name.Substring(flag + 1);
                    String[] version = versionStr.Split('.');
                    if (version.Length >= 2)
                    {
                        if (!float.TryParse(version[1], out minorVersion))
                        {
                            minorVersion = float.MaxValue;
                        }
                    }
                    else
                    {
                        logger.Warn($"check minor version failed,versionStr:{node.Name}");
                    }
                }
            }
            return minorVersion;
        }
        private List<SPTreeNodeDto> ExtractResult(List<SPTreeNodeDto> works)
        {
            List<SPTreeNodeDto> results = new List<SPTreeNodeDto>();
            List<SPTreeNodeDto> searchNodes = works;
            if (!searchNodes.IsNullOrEmpty())
            {
                foreach (var node in searchNodes)
                {
                    AddVirtualNode(node);
                    results.Add(node);
                }
            }
            return results;
        }

        private void AddVirtualNode(SPTreeNodeDto node)
        {
            if (null == node)
            {
                return;
            }
            foreach (SPTreeNodeDto child in node.Children)
            {
                child.FarmID = node.FarmID;
                child.FarmName = node.FarmName;
                child.SPType = node.SPType;
                AddVirtualNode(child);
            }

            if (node.Level == NodeLevel.Folder || node.Level == NodeLevel.List)
            {
                if (node.Children.Count == 0)
                {
                    return;
                }
                SPTreeNodeDto itemsNode = new SPTreeNodeDto { ID = Guid.NewGuid().ToString(), SPVersion = node.SPVersion, SPType = node.SPType, Name = GConstants.SPNodeName.Items, Level = NodeLevel.Items, CanChildrenBeLoaded = false, ChildrenLoaded = true, NodeExtension = new NodeExtensionDto() { IsAdvancedSearchResult = true }, Expanded = true, };
                SPTreeNodeDto foldersNode = new SPTreeNodeDto { ID = Guid.NewGuid().ToString(), SPVersion = node.SPVersion, SPType = node.SPType, Name = GConstants.SPNodeName.Folders, Level = NodeLevel.Folders, CanChildrenBeLoaded = false, ChildrenLoaded = true, NodeExtension = new NodeExtensionDto() { IsAdvancedSearchResult = true }, Expanded = true, };
                foreach (var child in node.Children)
                {
                    ///将folder或list下的节点分散到两个虚节点下
                    if (child.Level == NodeLevel.Item)
                    {
                        itemsNode.Children.Add(child);
                        itemsNode.ChildrenLoaded = true;
                        itemsNode.Expanded = true;
                    }
                    else if (child.Level == NodeLevel.Folder)
                    {
                        foldersNode.Children.Add(child);
                        foldersNode.ChildrenLoaded = true;
                        foldersNode.Expanded = true;
                    }
                }
                node.Children.Clear();
                ///设置虚节点下children的个数。
                itemsNode.ChildrenCount = itemsNode.Children.Count;
                foldersNode.ChildrenCount = foldersNode.Children.Count;
                if (node.Level == NodeLevel.List) //list节点
                {
                    SPTreeNodeDto rootFolderNode = new SPTreeNodeDto { ID = Guid.NewGuid().ToString(), SPVersion = node.SPVersion, SPType = node.SPType, Name = GConstants.SPNodeName.RootFolder, Level = NodeLevel.RootFolder, CanChildrenBeLoaded = true, ChildrenLoaded = true, Expanded = true };
                    rootFolderNode.Children.Add(foldersNode);
                    if (itemsNode.Children.Count > 0)
                    {
                        rootFolderNode.Children.Add(itemsNode);
                    }
                    rootFolderNode.ChildrenCount = rootFolderNode.Children.Count;
                    itemsNode.Parent = rootFolderNode;
                    foldersNode.Parent = rootFolderNode;
                    node.Children.Add(rootFolderNode);
                    rootFolderNode.Parent = node;
                }
                else
                {
                    node.Children.Add(foldersNode);
                    foldersNode.Parent = node;
                    if (itemsNode.Children.Count > 0)
                    {
                        node.Children.Add(itemsNode);
                        itemsNode.Parent = node;
                    }
                }
            }
            else if (node.Level == NodeLevel.Site)
            {
                if (node.Children.Count == 0)
                {
                    return;
                }
                ///构造虚节点
                SPTreeNodeDto listsNode = new SPTreeNodeDto() { ID = Guid.NewGuid().ToString(), SPVersion = node.SPVersion, SPType = node.SPType, Name = GConstants.SPNodeName.Lists, Level = NodeLevel.Lists, CanChildrenBeLoaded = true };
                SPTreeNodeDto sitesNode = new SPTreeNodeDto() { ID = Guid.NewGuid().ToString(), SPVersion = node.SPVersion, SPType = node.SPType, Name = GConstants.SPNodeName.Sites, Level = NodeLevel.Sites, CanChildrenBeLoaded = true };
                SPTreeNodeDto appsNode = new SPTreeNodeDto() { ID = Guid.NewGuid().ToString(), SPVersion = node.SPVersion, SPType = node.SPType, Name = GConstants.SPNodeName.Apps, Level = NodeLevel.Apps, CanChildrenBeLoaded = true };
                ///将site下的子节点分散到两个虚节点下
                foreach (var child in node.Children)
                {
                    if (child.Level == NodeLevel.List)
                    {
                        listsNode.Children.Add(child);
                        listsNode.ChildrenLoaded = true;
                        listsNode.Expanded = true;
                    }
                    else if (child.Level == NodeLevel.Site)
                    {
                        sitesNode.Children.Add(child);
                        sitesNode.ChildrenLoaded = true;
                        sitesNode.Expanded = true;
                    }
                    else if (child.Level == NodeLevel.App)
                    {
                        appsNode.Children.Add(child);
                        appsNode.ChildrenLoaded = true;
                        appsNode.Expanded = true;
                    }
                }
                node.Children.Clear();//清空子节点
                listsNode.ChildrenCount = listsNode.Children.Count;
                sitesNode.ChildrenCount = sitesNode.Children.Count;
                appsNode.ChildrenCount = appsNode.Children.Count;
                ///将虚拟节点添加到该节点下
                node.Children.Add(appsNode);
                appsNode.Parent = node;
                node.Children.Add(listsNode);
                listsNode.Parent = node;
                node.Children.Add(sitesNode);
                sitesNode.Parent = node;

            }
        }
        private TreeNode Clone(TreeNode source)
        {
            var settings = new JsonSerializerSettings
            {
                MaxDepth = 512, 
                TypeNameHandling = TypeNameHandling.Auto, 
                Formatting = Formatting.None,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };
            var serialized = JsonConvert.SerializeObject(source, settings);
            return JsonConvert.DeserializeObject<TreeNode>(serialized, settings);
        }
        private List<TreeNode> QuickSort(List<TreeNode> unsorted)
        {
            if (unsorted.Count <= 1)
            {
                return unsorted;
            }

            int pivotIndex = unsorted.Count / 2;
            TreeNode pivot = unsorted[pivotIndex];
            List<TreeNode> left = new List<TreeNode>();
            List<TreeNode> right = new List<TreeNode>();

            for (int i = 0; i < unsorted.Count; i++)
            {
                if (i == pivotIndex) continue;
                if (unsorted[i].Depth <= pivot.Depth)
                {
                    left.Add(unsorted[i]);
                }
                else
                {
                    right.Add(unsorted[i]);
                }
            }

            List<TreeNode> sorted = QuickSort(left);
            sorted.Add(pivot);
            sorted.AddRange(QuickSort(right));
            return sorted;
        }
        private List<TreeNode> BubbleSort(List<TreeNode> unsorted, TreeNodeLevel nodeLevel)
        {
            int n = unsorted.Count;
            for (int i = 0; i < n - 1; i++)
            {
                for (int j = 0; j < n - i - 1; j++)
                {
                    if (unsorted[j].Depth > unsorted[j + 1].Depth)
                    {
                        // swap arr[j] and arr[j+1]
                        TreeNode temp = unsorted[j];
                        unsorted[j] = unsorted[j + 1];
                        unsorted[j + 1] = temp;
                    }
                }
            }
            if (nodeLevel == TreeNodeLevel.Folder || nodeLevel == TreeNodeLevel.List || nodeLevel == TreeNodeLevel.Site)
            {
                logger.Info($"restore level is {nodeLevel}");
                return unsorted;
            }
            return SortItems(unsorted);
        }
        private int CaculateDepth(TreeNode tree)
        {
            int depth = 0;
            while (tree.Children.Count > 0)
            {
                tree = tree.Children[0];
                depth++;
            }
            return depth;
        }
        private ArchiverAdvancedSearchInfo ConverToArchiverAdvancedInfo(List<ArchiverRestoreSearchContractDto> searchContract, ArchiverRestoreFilter filterPolicy)
        {
            ArchiverAdvancedSearchInfo searchInfo = new ArchiverAdvancedSearchInfo()
            {
                NodeInfos = new List<ArchiverSearchNodeInfo>(),
                FilterInfors = new ArchiverRestoreFilter(),
            };
            searchContract.ForEach(node =>
            {
                searchInfo.NodeInfos.Add(new ArchiverSearchNodeInfo()
                {
                    BrowseInfo = new ArchiverBrowseInfo(node.SearchParam),
                    SiteId = node.SearchNode.SPObjectId,
                });
            });
            searchInfo.FilterInfors = filterPolicy;
            logger.Info($"ConverToArchiverAdvancedInfo.searchInfo.NodeInfos count:{searchInfo.NodeInfos.Count}." +
                $"searchInfo.FilterInfors.PolicyLevel:{filterPolicy.Level}." +
                $"FilterName:{filterPolicy.FilterName}." +
                $"CreateStartTime:{filterPolicy.CreateStartTime}." +
                $"CreateEndTime:{filterPolicy.CreateEndTime}." +
                $"ModifiedStartTime:{filterPolicy.ModifiedStartTime}." +
                $"ModifiedEndTime:{filterPolicy.ModifiedEndTime}." +
                $"ArchiveStartTime:{filterPolicy.ArchivedStartTime}" +
                $"ArchiveEndTime:{filterPolicy.ArchivedEndTime}" +
                $"CreateBy:{filterPolicy.CreatedBy}" +
                $"ModifyBy:{filterPolicy.ModifiedBy}" +
                $"MainJobId:{filterPolicy.MainJobId}");
            return searchInfo;
        }
        private ArchiverAdvancedSearchInfo ConverToFSArchiverAdvancedInfo(List<ArchiverRestoreSearchContractDto> searchContract, ArchiverRestoreFilter filterPolicy)
        {
            ArchiverAdvancedSearchInfo searchInfo = new ArchiverAdvancedSearchInfo()
            {
                NodeInfos = new List<ArchiverSearchNodeInfo>(),
                FilterInfors = new ArchiverRestoreFilter(),
            };
            searchContract.ForEach(node =>
            {
                searchInfo.NodeInfos.Add(new ArchiverSearchNodeInfo()
                {
                    BrowseInfo = new ArchiverBrowseInfo(node.SearchParam, ProductModule.FSArchiverBackup),
                    SiteId = node.SearchNode.SPObjectId,
                });
            });
            searchInfo.FilterInfors = filterPolicy;
            logger.Info($"ConverToFSArchiverAdvancedInfo.searchInfo.NodeInfos count:{searchInfo.NodeInfos.Count}." +
                $"searchInfo.FilterInfors.PolicyLevel:{filterPolicy.Level}." +
                $"FilterName:{filterPolicy.FilterName}." +
                $"CreateStartTime:{filterPolicy.CreateStartTime}." +
                $"CreateEndTime:{filterPolicy.CreateEndTime}." +
                $"ModifiedStartTime:{filterPolicy.ModifiedStartTime}." +
                $"ModifiedEndTime:{filterPolicy.ModifiedEndTime}.");
            return searchInfo;
        }
        private GDriveArchiverAdvancedSearchInfo ConvertToGDriveArchiverAdvancedInfo(List<ArchiverRestoreSearchContractDto> searchContract, ArchiverRestoreFilter filterPolicy)
        {
            GDriveArchiverAdvancedSearchInfo searchInfo = new GDriveArchiverAdvancedSearchInfo()
            {
                NodeInfos = new List<GDriveArchiverSearchNodeInfo>(),
                FilterInfors = new ArchiverRestoreFilter(),
            };
            searchContract.ForEach(node =>
            {
                searchInfo.NodeInfos.Add(new GDriveArchiverSearchNodeInfo()
                {
                    BrowseInfo = new GDriveBrowseInfo(node.SearchParam as GDriveRestoreParamDto, ProductModule.GDriveArchiverBackup),
                    SiteId = node.SearchNode.SPObjectId,
                }); 
            });
            searchInfo.FilterInfors = filterPolicy;
            logger.Info($"ConverToArchiverAdvancedInfo.searchInfo.NodeInfos count:{searchInfo.NodeInfos.Count}." +
                $"searchInfo.FilterInfors.PolicyLevel:{filterPolicy.Level}." +
                $"FilterName:{filterPolicy.FilterName}." +
                $"CreateStartTime:{filterPolicy.CreateStartTime}." +
                $"CreateEndTime:{filterPolicy.CreateEndTime}." +
                $"ModifiedStartTime:{filterPolicy.ModifiedStartTime}." +
                $"ModifiedEndTime:{filterPolicy.ModifiedEndTime}." +
                $"MainJobId:{filterPolicy.MainJobId}");
            return searchInfo;
        }

        private List<ArchiverRestoreSearchContractDto> AssembleSearchParamInfo(List<ArchiverSiteMasterIndexContract> indexes, List<SiteCollectionNodesInfo> searchNodes)
        {
            logger.Info($"AssembleSearchParamInfo.indexes count:{indexes.Count}.searchNodes count:{searchNodes.Count}.");
            List<ArchiverRestoreSearchContractDto> sitesMap = new List<ArchiverRestoreSearchContractDto>();
            ArchiverSiteMasterIndexContract currentIndex = null;
            foreach (var node in searchNodes)
            {
                string siteURL = node.SiteUrl;
                currentIndex = indexes.Where<ArchiverSiteMasterIndexContract>(s => s.SiteURL.Equals(siteURL, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (currentIndex == null)
                {
                    logger.Warn($"AssembleSearchParamInfo.currentIndex is null.SiteUrl:{siteURL}.");
                    continue;
                }
                else
                {
                    logger.Warn($"AssembleSearchParamInfo.Succsss add ArchiverRestoreSearchContractDto.SiteUrl:{siteURL}.");
                }
                ArchiverRestoreSearchContractDto paramDto = new ArchiverRestoreSearchContractDto();
                paramDto.SearchNode = node;
                paramDto.SearchParam = AssembleRestoreParamDto(currentIndex, node);
                paramDto.SearchParam.CacheLocation = CacheSettingService.GetBrowserCacheInfo();
                sitesMap.Add(paramDto);
            }
            logger.Info($"Finished AssembleSearchParamInfo.sitesMap count:{sitesMap.Count}.");
            return sitesMap;
        }
        private List<ArchiverRestoreSearchContractDto> AssembleFSSearchParamInfo(List<FSMasterIndexContract> indexes, List<SiteCollectionNodesInfo> searchNodes)
        {
            logger.Info($"AssembleFSSearchParamInfo.indexes count:{indexes.Count}.searchNodes count:{searchNodes.Count}.");
            List<ArchiverRestoreSearchContractDto> sitesMap = new List<ArchiverRestoreSearchContractDto>();
            FSMasterIndexContract currentIndex = null;
            foreach (var node in searchNodes)
            {
                string connectionId = node.SPObjectId;
                string siteURL = node.SiteUrl;
                currentIndex = indexes.Where<FSMasterIndexContract>(s => s.ConnectionId.Equals(connectionId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (currentIndex == null)
                {
                    logger.Warn($"AssembleFSSearchParamInfo.currentIndex is null.SiteUrl:{siteURL}.");
                    continue;
                }
                else
                {
                    logger.Warn($"AssembleFSSearchParamInfo.Succsss add ArchiverRestoreSearchContractDto.SiteUrl:{siteURL}.");
                }
                ArchiverRestoreSearchContractDto paramDto = new ArchiverRestoreSearchContractDto();
                paramDto.SearchNode = node;
                paramDto.SearchParam = AssembleFSRestoreParamDto(currentIndex, node);
                paramDto.SearchParam.CacheLocation = CacheSettingService.GetBrowserCacheInfo();
                sitesMap.Add(paramDto);
            }
            logger.Info($"Finished AssembleFSSearchParamInfo.sitesMap count:{sitesMap.Count}.");
            return sitesMap;
        }
        private List<ArchiverRestoreSearchContractDto> AssembleGDriveSearchParamInfo(List<ArchiverSiteMasterIndexContract> indexes, List<SiteCollectionNodesInfo> searchNodes)
        {
            logger.Info($"AssembleGoogleSearchParamInfo.indexes count:{indexes.Count}.searchNodes count:{searchNodes.Count}.");
            List<ArchiverRestoreSearchContractDto> sitesMap = new List<ArchiverRestoreSearchContractDto>();
            ArchiverSiteMasterIndexContract currentIndex = null;
            foreach (var node in searchNodes)
            {
                string siteURL = node.SiteUrl;
                currentIndex = indexes.Where<ArchiverSiteMasterIndexContract>(s => s.SiteURL.Equals(siteURL, StringComparison.OrdinalIgnoreCase) && s.SiteId.Equals(node.SPObjectId, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (currentIndex == null)
                {
                    logger.Warn($"AssembleGoogleSearchParamInfo.currentIndex is null.SiteUrl:{siteURL}.");
                    continue;
                }
                else
                {
                    logger.Warn($"AssembleGoogleSearchParamInfo.Successs add ArchiverRestoreSearchContractDto.SiteUrl:{siteURL}.");
                }
                ArchiverRestoreSearchContractDto paramDto = new ArchiverRestoreSearchContractDto();
                paramDto.SearchNode = node;
                paramDto.SearchParam = AssembleGDriveRestoreParamDto(currentIndex, node);
                paramDto.SearchParam.CacheLocation = CacheSettingService.GetBrowserCacheInfo();
                sitesMap.Add(paramDto);
            }
            logger.Info($"Finished AssembleGoogleSearchParamInfo.sitesMap count:{sitesMap.Count}.");
            return sitesMap;
        }
        private ArchiverRestoreParamDto AssembleRestoreParamDto(ArchiverSiteMasterIndexContract index, SiteCollectionNodesInfo searchNode)
        {
            StorageDeviceDto Indexdevice = null;
            SettingProfileDto indexDto = new SettingProfileDto()
            {
                Type = (int)SettingProfilesType.IndexDevice,
                Name = "UsingIndexDevice"
            };
            var indexDBInfo = SettingProfileDao.Load(indexDto);
            if (indexDBInfo != null)
            {
                Indexdevice = StorageDeviceService.GetStorageDeviceById(indexDBInfo.Settings, needDecryptSecert: true);
            }
            if (this.BackUpJobId!=null && !this.BackUpJobId.Contains('_'))
            {
                logger.Warn($"this stub may rebuild stub,will use main index,job id is:{this.BackUpJobId},index job id:{index.JobId}");
                this.BackUpJobId = string.Empty;
            }
            ArchiverRestoreParamDto param = new ArchiverRestoreParamDto
            {
                Path = searchNode.SiteUrl,
                //Level = searchNode.Level,
                BackupJobId =string.IsNullOrEmpty(this.BackUpJobId)?index.JobId: this.BackUpJobId,
                FarmName = string.Empty,
                BackupPlanId = index.PlanId,
                EndTime = DateTime.MaxValue.Ticks,
                //LogicalDevice = SOUtilityService.GetLogicalDeviceInfo(index.LogicalDeviceId),
                IndexLogicalDevice = Indexdevice,
                LoadTreeOption = string.IsNullOrEmpty(this.BackUpJobId)?ArchiverLoadTreeOption.SiteCollectionMode: ArchiverLoadTreeOption.JobMode,
                StorageInfo = index.StorageInfo,
                SiteUrl = searchNode.SiteUrl,
            };
            return param;
        }
        private ArchiverRestoreParamDto AssembleGDriveRestoreParamDto(ArchiverSiteMasterIndexContract index, SiteCollectionNodesInfo searchNode)
        {
            StorageDeviceDto Indexdevice = null;
            SettingProfileDto indexDto = new SettingProfileDto()
            {
                Type = (int)SettingProfilesType.IndexDevice,
                Name = "UsingIndexDevice"
            };
            var indexDBInfo = SettingProfileDao.Load(indexDto);
            if (indexDBInfo != null)
            {
                Indexdevice = StorageDeviceService.GetStorageDeviceById(indexDBInfo.Settings, needDecryptSecert: true);
            }
            if (this.BackUpJobId != null && !this.BackUpJobId.Contains('_'))
            {
                logger.Warn($"this stub may rebuild stub,will use main index,job id is:{this.BackUpJobId},index job id:{index.JobId}");
                this.BackUpJobId = string.Empty;
            }
            var param = new GDriveRestoreParamDto
            {
                Path = searchNode.SiteUrl,
                //Level = searchNode.Level,
                BackupJobId = string.IsNullOrEmpty(this.BackUpJobId) ? index.JobId : this.BackUpJobId,
                FarmName = string.Empty,
                BackupPlanId = index.PlanId,
                EndTime = DateTime.MaxValue.Ticks,
                //LogicalDevice = SOUtilityService.GetLogicalDeviceInfo(index.LogicalDeviceId),
                IndexLogicalDevice = Indexdevice,
                LoadTreeOption = string.IsNullOrEmpty(this.BackUpJobId) ? ArchiverLoadTreeOption.SiteCollectionMode : ArchiverLoadTreeOption.JobMode,
                StorageInfo = index.StorageInfo,
                SiteUrl = searchNode.SiteUrl,
                DriveId = searchNode.SPObjectId,
                TenantId = searchNode.SiteGroupId,
            };
            return param;
        }
        private ArchiverRestoreParamDto AssembleFSRestoreParamDto(FSMasterIndexContract index, SiteCollectionNodesInfo searchNode)
        {
            StorageDeviceDto Indexdevice = null;
            SettingProfileDto indexDto = new SettingProfileDto()
            {
                Type = (int)SettingProfilesType.IndexDevice,
                Name = "UsingIndexDevice"
            };
            var indexDBInfo = SettingProfileDao.Load(indexDto);
            if (indexDBInfo != null)
            {
                Indexdevice = StorageDeviceService.GetStorageDeviceById(indexDBInfo.Settings, needDecryptSecert: true);
            }
            ArchiverRestoreParamDto param = new ArchiverRestoreParamDto
            {
                Path = searchNode.SPObjectId,
                //Level = searchNode.Level,
                BackupJobId = string.IsNullOrEmpty(this.BackUpJobId) ? index.JobId : this.BackUpJobId,
                FarmName = string.Empty,
                BackupPlanId = index.PlanId,
                EndTime = DateTime.MaxValue.Ticks,
                //LogicalDevice = SOUtilityService.GetLogicalDeviceInfo(index.LogicalDeviceId),
                IndexLogicalDevice = Indexdevice,
                LoadTreeOption = ArchiverLoadTreeOption.SiteCollectionMode,
                StorageInfo = index.StorageInfo,
                SiteUrl = searchNode.SPObjectId
            };
            return param;
        }
        private async Task<ArchiverRestoreResult> HandleSearchCommonNodeAsync(ArchiverRestoreResult filterPolicy, SiteCollectionNodesInfo searchNode, ArchiverRestoreOrderBy orderBy)
        {
            ArchiverRestoreResult res = new ArchiverRestoreResult();
            List<ArchiverRestoreSerchResult> result = new List<ArchiverRestoreSerchResult>();
            ArchiverSiteMasterIndexContract siteIndex = GetSiteCollctionIndex(searchNode);
            if (null == siteIndex)
            {
                logger.Warn("the siteIndex is null");
                return null;
            }
            List<ArchiverSiteMasterIndexContract> indexes = new List<ArchiverSiteMasterIndexContract> { siteIndex };
            filterPolicy.SerchContract.FilterPolicy.PageIndex = filterPolicy.PageIndex;
            filterPolicy.SerchContract.FilterPolicy.PageSize = filterPolicy.PageSize;
            filterPolicy.SerchContract.FilterPolicy.ExtraQuerySize = filterPolicy.PageSize > 0 ? 1 : 0;
            logger.Info($"HandleSearchCommonNode.FilterPolicy PageIndex:{filterPolicy.PageIndex}.FilterPolicy PageSize:{filterPolicy.PageSize}.OpenIndexDbTimeoutInMs:{filterPolicy.OpenIndexDbTimeoutInMs}.");
            List <TreeNode> trees = GetSearchNodesFromMedia(indexes, new List<SiteCollectionNodesInfo> { searchNode }, filterPolicy.SerchContract.FilterPolicy, filterPolicy.OpenIndexDbTimeoutInMs, orderBy);
            if (trees == null || trees.Count == 0)
            {
                logger.Warn("HandleSearchCommonNode.List <TreeNode> trees is null");
            }
            var tempTree = trees.FirstOrDefault();
            res.TotalNumber = tempTree==null ? 0: tempTree.Count;
            res.TotalCount = tempTree==null ? 0: tempTree.TotalCount;
            //if (filterPolicy.PageSize >= 0)
            //{
            //    trees = trees.Skip((filterPolicy.PageIndex - 1) * filterPolicy.PageSize).Take(filterPolicy.PageSize).ToList<TreeNode>();
            //}
            var gls = await GeneralSettingService.GetGeneralSettingAsync();
            //List<ArchiverBasicIndex> sites = GetSearchNodesFromMedia(indexes, new List<SiteCollectionNodesInfo> { searchNode }, filterPolicy);
            if (null != trees && trees.Count > 0)
            {
                
                if(filterPolicy.SerchContract.FilterPolicy.ExtraQuerySize > 0)
                {
                    res.HasNext = trees.Count > filterPolicy.PageSize;
                    trees = trees.Take(filterPolicy.PageSize).ToList();
                }

                foreach (var re in trees)
                {
                    var temp = ConvertToSerchResult(re, filterPolicy.SerchContract.FilterPolicy, gls);
                    if (!result.Contains(temp))
                    {
                        result.Add(temp);
                    }
                }
            }
            else
            {
                if (filterPolicy.SerchContract.FilterPolicy.Level > PolicyLevel.SiteCollection)
                {
                    logger.Warn("serch tree is null");
                    return res;
                    //throw new AveException("No match data got from media.");
                }
            }
            res.RestoreSerchNodes = result;
            return res;
        }
        private async Task<ArchiverRestoreResult> HandleGDriveSearchCommonNodeAsync(ArchiverRestoreResult filterPolicy, SiteCollectionNodesInfo searchNode, ArchiverRestoreOrderBy orderBy, bool isControlPlus = false)
        {
            ArchiverRestoreResult res = new ArchiverRestoreResult();
            List<ArchiverRestoreSerchResult> result = new List<ArchiverRestoreSerchResult>();
            ArchiverSiteMasterIndexContract siteIndex = GetGoogleDriveIndex(searchNode);
            if (null == siteIndex)
            {
                logger.Warn("the siteIndex is null");
                return null;
            }
            List<ArchiverSiteMasterIndexContract> indexes = new List<ArchiverSiteMasterIndexContract> { siteIndex };
            filterPolicy.SerchContract.FilterPolicy.PageIndex = filterPolicy.PageIndex;
            filterPolicy.SerchContract.FilterPolicy.PageSize = filterPolicy.PageSize;
            filterPolicy.SerchContract.FilterPolicy.ExtraQuerySize = 1;
            logger.Info($"HandleSearchCommonNode.FilterPolicy PageIndex:{filterPolicy.PageIndex}.FilterPolicy PageSize:{filterPolicy.PageSize}.OpenIndexDbTimeoutInMs:{filterPolicy.OpenIndexDbTimeoutInMs}.");
            List<TreeNode> trees = GetGDriveSearchNodesFromMedia(indexes, new List<SiteCollectionNodesInfo> { searchNode }, filterPolicy.SerchContract.FilterPolicy, filterPolicy.OpenIndexDbTimeoutInMs, orderBy);
            if (trees == null || trees.Count == 0)
            {
                logger.Warn("HandleSearchCommonNode.List <TreeNode> trees is null");
            }
            var tempTree = trees.FirstOrDefault();
            res.TotalNumber = tempTree == null ? 0 : tempTree.Count;
            //if (filterPolicy.PageSize >= 0)
            //{
            //    trees = trees.Skip((filterPolicy.PageIndex - 1) * filterPolicy.PageSize).Take(filterPolicy.PageSize).ToList<TreeNode>();
            //}
            var gls = await GeneralSettingService.GetGeneralSettingAsync();
            //List<ArchiverBasicIndex> sites = GetSearchNodesFromMedia(indexes, new List<SiteCollectionNodesInfo> { searchNode }, filterPolicy);
            if (null != trees && trees.Count > 0)
            {
                res.HasNext = trees.Count > filterPolicy.PageSize;

                foreach (var re in trees.Take(filterPolicy.PageSize))
                {
                    var temp = ConvertToSerchResult(re, filterPolicy.SerchContract.FilterPolicy, gls, isControlPlus);
                    if (!result.Contains(temp))
                    {
                        result.Add(temp);
                    }
                }
            }
            else
            {
                if (filterPolicy.SerchContract.FilterPolicy.Level > PolicyLevel.SiteCollection)
                {
                    logger.Warn("serch tree is null");
                    return res;
                    //throw new AveException("No match data got from media.");
                }
            }
            res.RestoreSerchNodes = result;
            return res;
        }
        private ArchiverSiteMasterIndexContract GetGoogleDriveIndex(SiteCollectionNodesInfo node)
        {
            ArchiverSiteMasterIndexContract siteIndex = new ArchiverSiteMasterIndexContract { SiteId = node.SPObjectId, SiteURL = node.SiteUrl };
            return ArchiverIndexService.GetGoogleDriveInfo(siteIndex);
        }
        private async Task<ArchiverRestoreResult> HandleFSSearchCommonNodeAsync(ArchiverRestoreResult filterPolicy, SiteCollectionNodesInfo searchNode, ArchiverRestoreOrderBy orderBy)
        {
            ArchiverRestoreResult res = new ArchiverRestoreResult();
            List<ArchiverRestoreSerchResult> result = new List<ArchiverRestoreSerchResult>();
            FSMasterIndexContract fsIndex = GetConnectionIndex(searchNode);
            if (null == fsIndex)
            {
                logger.Warn("the siteIndex is null");
                return null;
            }
            List<FSMasterIndexContract> indexes = new List<FSMasterIndexContract> { fsIndex };
            filterPolicy.SerchContract.FilterPolicy.PageIndex = filterPolicy.PageIndex;
            filterPolicy.SerchContract.FilterPolicy.PageSize = filterPolicy.PageSize;
            filterPolicy.SerchContract.FilterPolicy.ExtraQuerySize = 1;
            logger.Info($"HandleFSSearchCommonNode.FilterPolicy PageIndex:{filterPolicy.PageIndex}.FilterPolicy PageSize:{filterPolicy.PageSize}.OpenIndexDbTimeoutInMs:{filterPolicy.OpenIndexDbTimeoutInMs}.");
            List<ArchiverBasicIndex> fsIndexdb = GetFSSearchNodesFromMedia(indexes, new List<SiteCollectionNodesInfo> { searchNode }, filterPolicy.SerchContract.FilterPolicy, filterPolicy.OpenIndexDbTimeoutInMs, orderBy);
            if (fsIndexdb == null || fsIndexdb.Count == 0)
            {
                logger.Warn("HandleFSSearchCommonNode.List <TreeNode> trees is null");
            }
            res.TotalNumber = fsIndexdb == null || fsIndexdb.Count==0 ? 0 : fsIndexdb[0].PlatformType;
            var gls = await GeneralSettingService.GetGeneralSettingAsync();
            if (null != fsIndexdb && fsIndexdb.Count > 0)
            {
                res.HasNext = fsIndexdb.Count > filterPolicy.PageSize;

                foreach (var re in fsIndexdb.Take(filterPolicy.PageSize))
                {
                    var temp = ConvertToFSSerchResult(re, gls);
                    if (!result.Contains(temp))
                    {
                        result.Add(temp);
                    }
                }
            }
            res.RestoreSerchNodes = result;
            return res;
        }
        private async Task<ArchiverRestoreResult> HandleTeamsSearchCommonNodeAsync(ArchiverRestoreResult filterPolicy, SiteCollectionNodesInfo searchNode, ArchiverRestoreOrderBy orderBy)
        {
            ArchiverRestoreResult res = new ArchiverRestoreResult();
            List<ArchiverRestoreSerchResult> result = new List<ArchiverRestoreSerchResult>();
            ArchiverSiteMasterIndexContract siteIndex = new ArchiverSiteMasterIndexContract()
            {
                WebId = searchNode.SiteGroupId,
                SiteURL = searchNode.SiteUrl,
                SourceFlag = (int)SourceFlag.Teams,
                TeamsId = searchNode.TeamsId
            };
            var contract = CommonSiteMasterIndexService.GetTeamsInfo(siteIndex);
            if (contract == null)
            {
                logger.Warn($"Cound not found index, TeamsId [{searchNode.TeamsId}], URL [{searchNode.SiteUrl}]");
                return null;
            }

            if ((filterPolicy.SerchContract.FilterPolicy.FilterDeleteType == FilterDeletedType.Soft && contract.IsSoftDeleted == false) || (filterPolicy.SerchContract.FilterPolicy.FilterDeleteType == FilterDeletedType.Normal && contract.IsSoftDeleted))
            {
                res.TotalNumber = result.Count;
                res.RestoreSerchNodes = result;
                return res;
            }
            var ext = (ArchiverGroupSiteMasterIndexExtension)contract.Extension;
            var tempTree = new TreeNode()
            {
                JobId = contract.JobId,
                Name = contract.SiteURL,
                SPObjectId = contract.TeamsId,
                SitePath = contract.SiteURL,
                FullPathForUI = contract.SiteURL,
                FullPath = contract.SiteURL,
                Location = contract.SiteURL,
                TreeNodeLevel = TreeNodeLevel.ExchangeOnlineMailbox,
                Type = ext.IsMicrosoftTeam ? TreeNodeType.EOMailBox : TreeNodeType.EOO365Group,
                ArchivedTime = contract.ArchiverTime,
                CreatedTime = ext.GroupCreated,
                ModifiedTime = 0L, //todo
                IsSoftDeleted= contract.IsSoftDeleted,
            };

            var gls = await GeneralSettingService.GetGeneralSettingAsync();
            var temp = ConvertToSerchResult(tempTree, filterPolicy.SerchContract.FilterPolicy, gls);
            result.Add(temp);
            res.TotalNumber = result.Count;
            res.RestoreSerchNodes = result;
            return res;
        }


        private async Task<List<RMArchiveSiteInfo>> HandleSearchCommonNodeForJobAsync(List<ArchiverSiteMasterIndexContract> indexes, ArchiverRestoreResult filterPolicy, List<SiteCollectionNodesInfo> searchNodes)
        {
            var result = GetSearchNodesFromMediaForJob(indexes, searchNodes, filterPolicy.SerchContract.FilterPolicy);   
            return result;
        }
        private async Task<List<RMArchiveGDriveInfo>> HandleGDriveSearchCommonNodeForJobAsync(List<ArchiverSiteMasterIndexContract> indexes, ArchiverRestoreResult filterPolicy, List<SiteCollectionNodesInfo> searchNodes)
        {
            var result = GetGDriveSearchNodesFromMediaForJob(indexes, searchNodes, filterPolicy.SerchContract.FilterPolicy);
            return result;
        }
        private ArchiverSiteMasterIndexContract GetSiteCollctionIndex(SiteCollectionNodesInfo node)
        {
            ArchiverSiteMasterIndexContract siteIndex = new ArchiverSiteMasterIndexContract { WebId = node.SiteGroupId, SiteId = node.SPObjectId, SiteURL = node.SiteUrl, SPVersion = 4 };
            return ArchiverIndexService.GetSiteCollectionInfo(siteIndex);
        }
        private FSMasterIndexContract GetConnectionIndex(SiteCollectionNodesInfo node)
        {
            FSMasterIndexContract fsIndex = new FSMasterIndexContract { ConnectionId = node.SPObjectId, ConnectionName = node.SiteUrl, SPVersion = 4 };
            return FSMasterIndexService.GetConnectionMasterInfo(fsIndex);
        }
        private SPTreeNodeDto AssembleSearchTree(SPTreeNodeDto node)
        {
            node.NodeExtension.SelectorHidden = true;
            if (node.Level != NodeLevel.Farm)
            {
                SPTreeNodeDto parentNode = node.Parent;
                parentNode.NodeExtension.BackupTime = node.NodeExtension.BackupTime;
                parentNode.Children = new List<SPTreeNodeDto>();
                parentNode.Children.Add(node);
                parentNode.ChildrenCount = parentNode.Children.Count;
                return AssembleSearchTree(parentNode);
            }
            return node;
        }

        List<SPTreeNodeDto> IRestoreSearchService.GetSearchNodesFromMedia(List<ArchiverSiteMasterIndexContract> indexes, List<SPTreeNodeDto> searchNodes, RestoreSearchFilterPolicy filterPolicy)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> IsOnlySupportExactSearchSiteAsync()
        {
            try
            {
                var configContent = RMKeyValueDao.GetValueByKey(RMKeyValuesConstants.RestoreExactSearchSiteConfig);
                if (string.IsNullOrEmpty(configContent?.Value))
                {
                    return false;
                }

                var restoreExactSearchSiteCfg = JsonConvert.DeserializeObject<RestoreExactSearchSiteConfig>(configContent.Value);

                var totalArchivedSites = await ArchiveSiteInfoDao.GetAllArchivedSitesCountAsync();
                return totalArchivedSites > restoreExactSearchSiteCfg.Limit;
            }
            catch (Exception ex)
            {
                logger.Error($"Check is only support exact search site failed. {ex}");
            }

            return false;
        }

        public async Task<bool> EDiscoveryIsOnlySupportExactSearchSiteAsync()
        {
            try
            {
                var configContent = RMKeyValueDao.GetValueByKey(RMKeyValuesConstants.RestoreExactSearchSiteConfig);
                if (string.IsNullOrEmpty(configContent?.Value))
                {
                    return false;
                }
                var restoreExactSearchSiteCfgLimit = JsonConvert.DeserializeObject<RestoreExactSearchSiteConfig>(configContent.Value).Limit;
                
                return await ArchiveSiteInfoDao.GetAllArchivedSitesCountAsync() > restoreExactSearchSiteCfgLimit;
            }
            catch (Exception ex)
            {
                logger.Error($"Check ediscovery is only support exact search site failed. {ex}");
            }

            return false;
        }

        public async Task<List<SiteCollectionNodesInfo>> GetEdiscoveryAllSiteCollectionNodesAsync(string siteUrl = null)
        {
            if (!TenantService.IsNewOpusTenant())
            {
                logger.Warn($"old logic account cann't use GetEdiscoveryAllSiteCollectionNodesAsync");
                return new List<SiteCollectionNodesInfo>();
            }
            try
            {
                var isBlacklistMode = RMKeyValueDao.TryGetBoolValue(KeyNameCollection.IsSCBlackListForEdiscovery, out var currentValue) && currentValue;
                var normalizedSiteUrl = NormalizeSiteUrl(siteUrl);
                var siteCandidates = string.IsNullOrWhiteSpace(siteUrl)
                    ? Array.Empty<string>()
                    : new[] { siteUrl, normalizedSiteUrl }.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct(StringComparer.OrdinalIgnoreCase).ToArray();

                if (!string.IsNullOrWhiteSpace(siteUrl))
                {
                    if (isBlacklistMode)
                    {
                        if (RMRestoreSiteMappingDao.ExistBlacklistInSiteUrls(siteCandidates))
                        {
                            logger.Info($"black list contains target site url:{siteUrl}");
                            return new List<SiteCollectionNodesInfo>();
                        }
                    }
                    else if (!RMRestoreSiteMappingDao.ExistWhitelistInSiteUrls(siteCandidates))
                    {
                        logger.Info($"white list is not contains target site url:{siteUrl}");
                        return new List<SiteCollectionNodesInfo>();
                    }
                }

                var nodes = await GetAllSiteCollectionNodesAsync(siteUrl);

                if (nodes == null || nodes.Count == 0)
                {
                    return nodes ?? new List<SiteCollectionNodesInfo>();
                }

                if (isBlacklistMode)
                {
                    var blacklist = RMRestoreSiteMappingDao.GetAllBlacklist()
                        .Select(b => NormalizeSiteUrl(b.SourceSiteUrl))
                        .Where(url => !string.IsNullOrWhiteSpace(url))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    return nodes
                        .Where(node => !blacklist.Contains(NormalizeSiteUrl(node.SiteUrl)))
                        .ToList();
                }

                if (string.IsNullOrWhiteSpace(siteUrl))
                {
                    var whitelist = RMRestoreSiteMappingDao.GetAllWhitelist()
                        .Select(w => NormalizeSiteUrl(w.SourceSiteUrl))
                        .Where(url => !string.IsNullOrWhiteSpace(url))
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    return nodes
                        .Where(node => whitelist.Contains(NormalizeSiteUrl(node.SiteUrl)))
                        .ToList();
                }

                return nodes;
            }
            catch(Exception e)
            {
                logger.Error($"Accure exception when execute GetEdiscoveryAllSiteCollectionNodesAsync, site url:{siteUrl}, ex:{e}");
                throw;
            }
        }

        private static string NormalizeSiteUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return string.Empty;
            }

            return url.Trim().TrimEnd('/');
        }


        public async Task<List<SiteCollectionNodesInfo>> GetSiteCollectionNodesByUrlAsync(string siteUrl)
        {
            if (string.IsNullOrWhiteSpace(siteUrl))
                throw new ArgumentException("URL cannot be null or empty", nameof(siteUrl));

            try
            {
                logger.Info($"Start GetSiteCollectionNodesByUrlAsync, url: {siteUrl}");
                var sw = Stopwatch.StartNew();

                var siteIndex = ArchiverSiteMasterIndexDao.GetRestoringSiteCollectionInfoByUrl(siteUrl);
                var indexList = siteIndex == null
                    ? new List<DB.Model.ArchiverSiteMasterIndex>()
                    : new List<DB.Model.ArchiverSiteMasterIndex> { siteIndex };

                var result = GetSiteCollectionNodes(new HashSet<string>(), indexList).ToList();

                sw.Stop();
                logger.Info($"GetSiteCollectionNodesByUrlAsync finished, count: {result.Count}, elapsed: {sw.ElapsedMilliseconds}ms");

                return result;
            }
            catch (Exception e)
            {
                logger.Error($"GetSiteCollectionNodesByUrlAsync failed, url: {siteUrl}, error: {e}");
                throw;
            }
        }
        public async Task<List<SiteCollectionNodesInfo>> GetAllSiteCollectionNodesAsync(string siteUrl = null)
        {
            try
            {
                logger.Info($"start GetAllSiteCollectionNodsInfo.");
                Stopwatch sw = new Stopwatch();
                sw.Start();
                List<DB.Model.ArchiverSiteMasterIndex> index = null;
                if (string.IsNullOrWhiteSpace(siteUrl))
                {
                    index = ArchiverSiteMasterIndexDao.GetAllSiteCollectionNodsInfo(new List<int>() { (int)SourceFlag.Google });
                }
                else
                {
                    index = new List<DB.Model.ArchiverSiteMasterIndex>();
                    var siteIdx = ArchiverSiteMasterIndexDao.GetRestoringSiteCollectionInfoByUrl(siteUrl);
                    if (siteIdx != null)
                    {
                        index.Add(siteIdx);
                    }
                }
                sw.Stop();
                logger.Info($"GetAllSiteCollectionNodsInfo count:{index?.Count},use time:{sw.ElapsedMilliseconds}.");

                Stopwatch sw1 = new Stopwatch();
                sw1.Start();
                logger.Info($"start GetAllSiteCollectionNodsInfo has permission fullpaths.");
                List<SiteCollectionNodesInfo> result = new List<SiteCollectionNodesInfo>();
                async Task<bool> isOpusILAdmin() => await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOAdmin);
                async Task<bool> isOpusSOAdmin() => await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.SPOAdmin);
                if (await isOpusILAdmin() || await isOpusSOAdmin())
                {
                    result.AddRange(GetSiteCollectionNodes(new HashSet<string>(), index));
                }
                else
                {
                    result.AddRange(await GetSharepointAndODSiteCollectionNodesAsync(index));
                    result.AddRange(await GetTeamsSiteCollectionNodesAsync(index));

                    var tempPermission = await SecurityTrimmingHelper.GetUserRestoreCenterFunctionPermissionAsync();
                    if (tempPermission != FunctionSubPermission.None) //user has function permission
                    {
                        var tempAllResult = GetSiteCollectionNodes(new HashSet<string>(), index);
                        foreach (var temp in tempAllResult)
                        {
                            if (!result.Exists(t => t.SiteUrl == temp.SiteUrl))
                            {
                                temp.PermissionLevel = (int)tempPermission;
                            }
                        }
                        result = tempAllResult;
                    }
                }
                sw1.Stop();
                logger.Info($"GetAllSiteCollectionNodsInfo has permission fullpaths count:{result?.Count},use time:{sw1.ElapsedMilliseconds}.");
                return result.OrderByDescending(r => r.ArchiveTime).ToList();
            }
            catch (Exception e)
            {
                logger.Error($"GetAllSiteCollectionNodesAsync failed,errror:{e}.");
                throw;
            }
        }

        private async Task<List<SiteCollectionNodesInfo>> GetSharepointAndODSiteCollectionNodesAsync(List<DB.Model.ArchiverSiteMasterIndex> index)
        {
            var permissionContainerIds = new List<Guid>();
            if (RMKeyValueDao.HasUpgradeTeams())
            {
                permissionContainerIds = await GetPermissionContainerIdsAsync(RMBrowseTreeNodeSourceType.SPAndOD);
            }
            else
            {
                permissionContainerIds = await GetPermissionContainerIdsAsync(RMBrowseTreeNodeSourceType.All);
            }
            return GetSiteNodesThatHasPermission(index, permissionContainerIds);
        }

        private async Task<List<SiteCollectionNodesInfo>> GetTeamsSiteCollectionNodesAsync(List<DB.Model.ArchiverSiteMasterIndex> index)
        {
            async Task<bool> isTeamsILEndUser() => await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsEndUser);
            async Task<bool> isTeamsSOEndUser() => await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.TeamsEndUser);
            if (!RMKeyValueDao.HasUpgradeTeams() || (!await isTeamsILEndUser() && !await isTeamsSOEndUser()))
            {
                return new();
            }
            var permissionContainerIds = await GetPermissionContainerIdsAsync(RMBrowseTreeNodeSourceType.Teams);
            return await CollectSiteNodeInfoesForTeams(index, permissionContainerIds);
        }

        private IEnumerable<(string url, FunctionSubPermission permission)> ValidSiteCollectionsPermission(IEnumerable<string> scUrl)
        {
            Dictionary<string, FunctionSubPermission> scUrlToPermission = new Dictionary<string, FunctionSubPermission>(StringComparer.OrdinalIgnoreCase);
            if (scUrl == null || !scUrl.Any())
            {
                yield break;
            }

            bool isOpusILAdmin() => SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOAdmin).GetAwaiter().GetResult();
            bool isOpusSOAdmin() => SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.SPOAdmin).GetAwaiter().GetResult();
            FunctionSubPermission restoreCenterPermission = default;
            FunctionSubPermission getRestoreFunction() => restoreCenterPermission = SecurityTrimmingHelper.GetUserRestoreCenterFunctionPermissionAsync().GetAwaiter().GetResult();
            if (isOpusILAdmin() || isOpusSOAdmin() || getRestoreFunction() == FunctionSubPermission.RestoreCenterFullControl)
            {
                foreach (var url in scUrl)
                {
                    yield return (url, FunctionSubPermission.RestoreCenterFullControl);
                }
                yield break;
            }
            
            var containerIds = GetPermissionContainerIdsAsync(RMBrowseTreeNodeSourceType.All).GetAwaiter().GetResult().Select(id => id.ToString());
            if (containerIds.Count() == 0)
            {
                foreach (var url in scUrl)
                {
                    yield return (url, restoreCenterPermission);
                }
                yield break;
            }

            var batches = scUrl.Select(NormalizeSiteUrl).Batch(500);
            bool hasUpgradeTeams = RMKeyValueDao.HasUpgradeTeams();
            HashSet<string> teamsContainerId = null;
            if (hasUpgradeTeams)
            {
                teamsContainerId = GetPermissionContainerIdsAsync(RMBrowseTreeNodeSourceType.Teams).GetAwaiter().GetResult().Select(id => id.ToString()).ToHashSet();
                containerIds = containerIds.Except(teamsContainerId);
            }

            foreach (var batch in batches)
            {
                IEnumerable<string> permissionRemoteNodes = GetFullPermissionUrl(batch, containerIds, hasUpgradeTeams, teamsContainerId);
                foreach (var url in batch)
                {
                    if(permissionRemoteNodes.Select(NormalizeSiteUrl).ToHashSet().Contains(url))
                    {
                        yield return (url, FunctionSubPermission.RestoreCenterFullControl);
                    }
                    else
                    {
                        yield return (url, restoreCenterPermission);
                    }
                }
            }
        }

        public IEnumerable<string> GetFullPermissionUrl(IEnumerable<string> urls, IEnumerable<string> spAndODContainerId, bool hasUpgradeTeams, HashSet<string> teamsContainerId)
        {
            List<string> remoteNodes = new();
            List<RemoteSiteCollection> spAndODSites = RMRemoteNode.GetRemoteSiteCollectionBySiteUrls(urls, spAndODContainerId);
            remoteNodes.AddRange(spAndODSites.Select(node => node.url).Select(NormalizeSiteUrl));

            if (hasUpgradeTeams)
            {
                IEnumerable<string> mayTeamsSc = urls.Except(remoteNodes);
                Dictionary<string, string> scUrlAndTeamIdMap = RMRemoteNode.GetTeamsIdsOfSites(mayTeamsSc);
                HashSet<string> permissionTeams = RMRemoteNode.GetHavePermissionTeams(scUrlAndTeamIdMap.Values, teamsContainerId);
                foreach (var scUrlAndTeamId in scUrlAndTeamIdMap)
                {
                    if (permissionTeams.Contains(scUrlAndTeamId.Value))
                    {
                        remoteNodes.Add(NormalizeSiteUrl(scUrlAndTeamId.Key));
                    }
                }
            }
            return remoteNodes.ToHashSet();
        }

        public async Task<ArchiverRestoreResult> GetAllSiteCollectionSerchResultAsync(ArchiverRestoreResult searchContract)
        {
            ArchiverRestoreResult response = InitializeSiteCollectionSearchResponse(searchContract);
            ArchiverRestoreFilter filterPolicy = response.SerchContract.FilterPolicy;

            try
            {
                if (!await CheckPermissionForSearchTree())
                {
                    logger.Warn($"User:{TenantLocalValue.LogonUserId} has no permission to search site collections.");
                    return response;
                }

                SiteCollectionPermissionContext permissionContext = await BuildSiteCollectionPermissionContextAsync();
                int totalCount = 0;

                (List<DB.Model.ArchiverSiteMasterIndex> records, totalCount) = await ArchiverSiteMasterIndexDao.GetSiteCollectionNodesByFilterAsync(
                        permissionContext.PermissionContainerIds,
                        filterPolicy.FilterName,
                        response.PageIndex,
                        response.PageSize,
                        permissionContext.FilterByContainers);

                response.RestoreSerchNodes = await ConvertToSiteCollectionSearchResultsAsync(records);

                UpdatePaginationInfo(response, totalCount, filterPolicy.FilterName);
            }
            catch (Exception ex)
            {
                response.Failed = true;
                response.Message = I18NEntity.GetString("RM_RS_UnkonwExceptionPleaseRetry");
                logger.Error("GetAllSiteCollectionSerchResultAsync failed:", ex);
            }

            return response;
        }

        private async Task<(List<ArchiverRestoreSerchResult> records, int totalCount)> GetTeamsSiteCollectionNodesByFilter(IEnumerable<Guid> containerIds,
            string filterKeyword,
            int pageIndex,
            int pageSize)
        {
            List<DB.Model.ArchiverSiteMasterIndex> index = ArchiverSiteMasterIndexDao.GetAllSiteCollectionNodsInfo(new List<int>() { (int)SourceFlag.Google });
            List<SiteCollectionNodesInfo> siteNodeInfoes = await CollectSiteNodeInfoesForTeams(index, containerIds.ToList());
            if (!string.IsNullOrEmpty(filterKeyword))
            {
                string pattern = filterKeyword;
                if (filterKeyword.Length >= 2 && filterKeyword.StartsWith("\"") && filterKeyword.EndsWith("\""))
                {
                    pattern = "^" + Regex.Escape(filterKeyword.Substring(1, filterKeyword.Length - 2)) + "$";
                }
                else
                {
                    pattern = Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".");
                    pattern = $"{pattern}";
                }
                var regex = new Regex(pattern, RegexOptions.IgnoreCase);
                siteNodeInfoes = siteNodeInfoes.Where(n => n.SiteUrl != null && regex.IsMatch(n.SiteUrl)).ToList();
            }
            List<ArchiverRestoreSerchResult> records = await ConvertToSiteCollectionSearchResultsAsync(siteNodeInfoes.AsEnumerable());

            return (records.OrderBy(record => record.SiteUrl).Skip(pageIndex - 1 * pageSize).Take(pageSize).ToList(), records.Count);
        }


        private ArchiverRestoreResult InitializeSiteCollectionSearchResponse(ArchiverRestoreResult searchContract)
        {
            searchContract ??= new ArchiverRestoreResult();

            ArchiverRestoreResult response = new ArchiverRestoreResult
            {
                PageIndex = searchContract.PageIndex,
                PageSize = searchContract.PageSize,
                IsDesc = searchContract?.IsDesc ?? false,
                OrderBy = searchContract?.OrderBy,
                RestoreSerchNodes = new List<ArchiverRestoreSerchResult>()
            };

            BackupDataSearchContract requestContract = searchContract?.SerchContract ?? new BackupDataSearchContract();
            ArchiverRestoreFilter filterPolicy = requestContract.FilterPolicy ?? new ArchiverRestoreFilter();
            response.SerchContract = requestContract;
            response.SerchContract.FilterPolicy = filterPolicy;
            response.SerchContract.FilterPolicy.DataSource = (int)RestoreDataSource.M365;

            return response;
        }

        private async Task<List<ArchiverRestoreSerchResult>> ConvertToSiteCollectionSearchResultsAsync(IEnumerable<DB.Model.ArchiverSiteMasterIndex> records)
        {
            GeneralSettingModel generalSetting = await GeneralSettingService.GetGeneralSettingAsync();

            return (records ?? Enumerable.Empty<AvePoint.RA.DB.Model.ArchiverSiteMasterIndex>())
                .Where(record => !string.IsNullOrWhiteSpace(record?.SiteURL))
                .Select(record => ConvertToSiteSearchResult(record, generalSetting))
                .ToList();
        }

        private async Task<List<ArchiverRestoreSerchResult>> ConvertToSiteCollectionSearchResultsAsync(IEnumerable<SiteCollectionNodesInfo> records)
        {
            GeneralSettingModel generalSetting = await GeneralSettingService.GetGeneralSettingAsync();

            return (records ?? Enumerable.Empty<SiteCollectionNodesInfo>())
                .Where(record => !string.IsNullOrWhiteSpace(record?.SiteUrl))
                .Select(record => ConvertToSiteSearchResult(record, generalSetting))
                .ToList();
        }

        private void ApplyRestoreCenterPermission(List<ArchiverRestoreSerchResult> nodes, List<Guid> permissionContainerIds, FunctionSubPermission restoreCenterPermission)
        {
            if (nodes == null || nodes.Count == 0 || permissionContainerIds == null)
            {
                return;
            }

            List<DB.Model.ArchiverSiteMasterIndex> index = ArchiverSiteMasterIndexDao.GetAllSiteCollectionNodsInfo(new List<int> { (int)SourceFlag.Google });
            List<SiteCollectionNodesInfo> permittedNodes = GetSiteNodesThatHasPermission(index, permissionContainerIds);

            foreach (ArchiverRestoreSerchResult node in nodes)
            {
                if (!permittedNodes.Exists(permittedNode => permittedNode.SiteUrl == node.SiteUrl))
                {
                    node.PermissionLevel = (int)restoreCenterPermission;
                }
            }
        }

        private void UpdatePaginationInfo(ArchiverRestoreResult response, int totalCount, string searchValue)
        {
            response.TotalNumber = totalCount;
            response.HasNext = response.PageIndex * response.PageSize < totalCount;
            response.SearchValue = searchValue;
        }

        private async Task<SiteCollectionPermissionContext> BuildSiteCollectionPermissionContextAsync()
        {
            SiteCollectionPermissionContext context = new SiteCollectionPermissionContext();
            Stopwatch sw1 = Stopwatch.StartNew();
            logger.Info($"start GetAllSiteCollectionNodsInfo has permission fullpaths.");
            bool isOpusILAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOAdmin);
            bool isOpusSOAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.SPOAdmin);
            if (isOpusILAdmin || isOpusSOAdmin)
            {
                context.FilterByContainers = false;
            }
            else
            {
                context.RestoreCenterPermission = await SecurityTrimmingHelper.GetUserRestoreCenterFunctionPermissionAsync();
                if (context.RestoreCenterPermission == FunctionSubPermission.RestoreCenterFullControl)
                {
                    context.FilterByContainers = false;
                    context.NeedCheckPermissionLevelForEachNode = true;
                }
                else
                {
                    context.PermissionContainerIds = await GetPermissionContainerIdsAsync(RMBrowseTreeNodeSourceType.All);
                }
            }
            sw1.Stop();
            logger.Info($"GetAllSiteCollectionNodsInfo has permission use time:{sw1.ElapsedMilliseconds}.");

            return context;
        }

        private sealed class SiteCollectionPermissionContext
        {
            public bool IsTeamsSiteCollectonSearch { get; set; }
            public bool FilterByContainers { get; set; } = true;
            public bool NeedCheckPermissionLevelForEachNode { get; set; }
            public FunctionSubPermission RestoreCenterPermission { get; set; } = FunctionSubPermission.RestoreCenterFullControl;
            public List<Guid> PermissionContainerIds { get; set; }
        }

        public async Task<RAReturnMessage> SaveMultiSiteCollectionRestoreSettingAndRunAsync(RestoreInfo info, bool needCleanCache = false)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }
            logger.Info($"Start multi-site collection restore request.sc list:{string.Join(';', info.NodeObjects.Select(node => node.SiteUrl))}");
            info.IsEndUserJob = false;
            GCommon.Contract.StorageOptimization.Object.RestoreType restoreType = DetermineRestoreType(info);

            RAReturnMessage validationResult = ValidateMultiSiteRestoreRequest(info);
            if (validationResult != null)
            {
                return validationResult;
            }

            List<ArchiverRestoreSerchResult> siteCollections = siteCollections = ExtractSiteCollections(info);
            List<ArchiverRestoreSerchResult> successProcessedSites = new List<ArchiverRestoreSerchResult>();

            foreach (ArchiverRestoreSerchResult siteCollection in siteCollections)
            {
                bool succeeded = await TryRunSiteCollectionRestoreAsync(info, siteCollection, restoreType);
                if (succeeded)
                {
                    successProcessedSites.Add(siteCollection);
                }
                if (needCleanCache)
                {
                    CleanAllBrowerCacheInfo();
                }
            }

            logger.Info("Finish multi-site collection restore request.");
            return BuildMultiSiteRestoreResponse(siteCollections, successProcessedSites);
        }

        public RAReturnMessage SaveMultiSiteCollectionRestoreSettingAndRunInVirtualJob(SelectMultiScRestoreInfo info)
        {
            logger.Info("RestoreSearchService start MultiSiteCollectionRestore.");
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            RAReturnMessage msg = new RAReturnMessage();
            if (!TryGetMultiSiteCollectionRestoreRunLock())
            {
                logger.Warn($"Skip MultiSiteCollectionRestore because another job is running. User:{TenantLocalValue.LogonUserId}.");
                return new RAReturnMessage()
                {
                    MessageType = RAMessageType.Failed,
                    FaildType = RAFailedType.RunningJobExist,
                    Extension = string.Empty,
                    ErrorMessage = I18NEntity.GetString("RM_RS_HasRunningMultiSiteCollectionRestoreJob"),
                };
            }

            JobQueueDto jqDto = new JobQueueDto()
            {
                JobType = JobType.MultiSiteCollectionRestore,
                TenantGroupId = TenantLocalValue.LogonGroupId,
                JobRunByUser = TenantLocalValue.LogonUserEmail,
                JobRunType = JobRunBy.Control,
                Parameters = SerializerHelper.SerializeByJsonConvert(info),
            };
            string id = string.Empty;
            try
            {
                id = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                ReleaseMultiSiteCollectionRestoreRunLock();
                logger.Error($"Failed to queue MultiSiteCollectionRestore job. User:{TenantLocalValue.LogonUserId}. Error:{ex}");
                throw;
            }

            logger.Info($"RestoreSearchService finished MultiSiteCollectionRestore.JobType:{JobType.MultiSiteCollectionRestore}.LogonGroupId: {TenantLocalValue.LogonGroupId}.RealRunJobUser:{TenantLocalValue.LogonUserId}.JobQueueMessageId:{id}.");
            if (string.IsNullOrEmpty(id))
            {
                ReleaseMultiSiteCollectionRestoreRunLock();
                msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }
            return msg;
        }

        public bool UpdateMultiSiteCollectionRestoreRunLock()
        {
            try
            {
                long currentTime = DateTime.UtcNow.Ticks;
                var currentKeyValue = RMKeyValueDao.GetValueByKey(MultiSiteCollectionRestoreRunningKey);
                if (currentKeyValue == null || string.IsNullOrWhiteSpace(currentKeyValue.Value))
                {
                    logger.Warn("Skip updating MultiSiteCollectionRestore ticket because no current record exists.");
                    return false;
                }

                string newValue = currentTime.ToString();
                currentKeyValue.Value = newValue;
                if (RMKeyValueDao.Update(currentKeyValue, item => item.Value))
                {
                    logger.Info($"Updated MultiSiteCollectionRestore ticket. LastRunningJobTime:{currentTime}. HeartbeatInterval:{MultiSiteCollectionRestoreLockHeartbeatInterval}.");
                    return true;
                }

                logger.Warn("Failed to update MultiSiteCollectionRestore ticket.");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to update MultiSiteCollectionRestore ticket. Error:{ex}");
            }

            return false;
        }

        public void ReleaseMultiSiteCollectionRestoreRunLock()
        {
            try
            {
                if (RMKeyValueDao.DeleteByKey(MultiSiteCollectionRestoreRunningKey))
                {
                    logger.Info("Released MultiSiteCollectionRestore ticket.");
                    return;
                }

                logger.Warn("Failed to release MultiSiteCollectionRestore ticket.");
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to release MultiSiteCollectionRestore ticket. Error:{ex}");
            }
        }

        private bool TryGetMultiSiteCollectionRestoreRunLock()
        {
            long currentTime = DateTime.UtcNow.Ticks;
            string newValue = currentTime.ToString();

            var currentKeyValue = RMKeyValueDao.GetValueByKey(MultiSiteCollectionRestoreRunningKey);
            if (currentKeyValue == null)
            {
                bool saved = RMKeyValueDao.Save(new RMKeyValue
                {
                    Key = MultiSiteCollectionRestoreRunningKey,
                    Value = newValue,
                });

                if (saved)
                {
                    logger.Info($"Created MultiSiteCollectionRestore ticket. LastRunningJobTime:{currentTime}.");
                    return true;
                }

                logger.Warn("Failed to create MultiSiteCollectionRestore ticket record.");
                return false;
            }

            return TryGetAndUpdateExistingMultiSiteCollectionRestoreTicket(currentKeyValue, currentTime, newValue);
        }

        private bool TryGetAndUpdateExistingMultiSiteCollectionRestoreTicket(RMKeyValue currentKeyValue, long currentTime, string newValue)
        {
            string currentValue = currentKeyValue.Value;
            bool isTicketStillValid = long.TryParse(currentValue, out long lastRunningJobTime) &&
                currentTime - lastRunningJobTime <= MultiSiteCollectionRestoreLockExpiration.Ticks;

            if (!isTicketStillValid)
            {
                currentKeyValue.Value = newValue;
                if (RMKeyValueDao.Update(currentKeyValue, item => item.Value))
                {
                    if (string.IsNullOrWhiteSpace(currentValue))
                    {
                        logger.Info($"Acquired MultiSiteCollectionRestore ticket from empty existing record. LastRunningJobTime:{currentTime}.");
                    }
                    else
                    {
                        logger.Info($"Reacquired expired MultiSiteCollectionRestore ticket. PreviousValue:{currentValue}. LastRunningJobTime:{currentTime}. Expiration:{MultiSiteCollectionRestoreLockExpiration}.");
                    }

                    return true;
                }

                if (string.IsNullOrWhiteSpace(currentValue))
                {
                    logger.Warn($"Failed to acquire MultiSiteCollectionRestore ticket from empty existing record. Current:{currentTime}.");
                }
                else
                {
                    logger.Warn($"Failed to reacquire expired MultiSiteCollectionRestore ticket. PreviousValue:{currentValue}. Current:{currentTime}.");
                }
            }
            else
            {
                logger.Warn($"MultiSiteCollectionRestore ticket already exists and is still valid. LastRunningJobTime:{lastRunningJobTime}. Current:{currentTime}. Expiration:{MultiSiteCollectionRestoreLockExpiration}.");
            }

            return false;
        }



        private GCommon.Contract.StorageOptimization.Object.RestoreType DetermineRestoreType(RestoreInfo info)
        {
            if (info.RestoreTypeSelect == AvePoint.GCommon.Contract.Server.Common.BackupDataSearch.RestoreType.InPlace)
            {
                return GCommon.Contract.StorageOptimization.Object.RestoreType.InPlace;
            }

            return GCommon.Contract.StorageOptimization.Object.RestoreType.OutPlace;
        }

        private RAReturnMessage ValidateMultiSiteRestoreRequest(RestoreInfo info)
        {
            if (info.DataSource != (int)RestoreDataSource.M365)
            {
                logger.Error($"SaveMultiSiteCollectionRestoreSettingAndRun only supports M365 site collections. Data source:{info.DataSource}");
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    FaildType = RAFailedType.None,
                    ErrorMessage = "Save operation only supports M365 site collections."
                };
            }

            var scUrls = info.NodeObjects.Select(node => node.SiteUrl);
            if (ValidSiteCollectionsPermission(scUrls).Any(res => res.permission != FunctionSubPermission.RestoreCenterFullControl))
            {
                logger.Warn($"Part sc don't have permission,all sc url:{string.Join(';', scUrls)}");
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    FaildType = RAFailedType.None,
                    ErrorMessage = "Part sc don't have permission"
                };
            }

            return null;
        }

        private List<ArchiverRestoreSerchResult> ExtractSiteCollections(RestoreInfo info)
        {
            if (info.NodeObjects == null)
            {
                return new List<ArchiverRestoreSerchResult>();
            }

            List<ArchiverRestoreSerchResult> siteCollections = info.NodeObjects
                .Where(node => node != null)
                .Select(node => 
                {
                    node.ObjectName = node.SiteUrl;
                    node.Location = node.SiteUrl;
                    node.SitePath = node.SiteUrl;
                    node.FullPath = node.SiteUrl;
                    return node; 
                })
                .ToList();

            return siteCollections;
        }

        private ArchiverRestoreResult BuildSiteCollectionSearchContract(ArchiverRestoreSerchResult siteCollection)
        {
            if (siteCollection == null)
            {
                return null;
            }

            BackupDataSearchContract searchContract = new BackupDataSearchContract
            {
                SearchNode = siteCollection,
                FilterPolicy = new ArchiverRestoreFilter
                {
                    FilterDeleteType = FilterDeletedType.All,
                    DataSource = (int)RestoreDataSource.M365,
                    Level = PolicyLevel.SiteCollection,
                    FilterName = string.Empty
                }
            };

            return new ArchiverRestoreResult
            {
                PageIndex = 1,
                PageSize = 1,
                SerchContract = searchContract
            };
        }

        private RestoreInfo CloneRestoreInfo(RestoreInfo source)
        {
            if (source == null)
            {
                return null;
            }

            string serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<RestoreInfo>(serialized);
        }

        private RAReturnMessage BuildMultiSiteRestoreResponse(List<ArchiverRestoreSerchResult> siteCollections, List<ArchiverRestoreSerchResult> successProcessedSites)
        {
            if (successProcessedSites.Count == siteCollections.Count)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Successful,
                    FaildType = RAFailedType.None,
                    ErrorMessage = string.Empty
                };
            }
            else
            {
                try
                {
                    var successUrls = successProcessedSites.Select(site => site.SiteUrl).ToHashSet(StringComparer.OrdinalIgnoreCase);
                    var failedUrls = siteCollections.Where(site => !successUrls.Contains(site.SiteUrl)).Select(site => site.SiteUrl);
                    logger.Warn($"Part sc fail try restore,all sc:{string.Join(',', siteCollections.Select(site => site.SiteUrl))} ." +
                        $"success sc:{string.Join(',', successUrls)}. " +
                        $"fail sc:{string.Join(',', failedUrls)}");
                }
                catch(Exception e)
                {
                    logger.Error($"Fail log exception sc of BuildMultiSiteRestoreResponse,e:{e}");
                }                
            }

            if (successProcessedSites.Count == 0)
            {
                return new RAReturnMessage
                {
                    MessageType = RAMessageType.Failed,
                    FaildType = RAFailedType.None,
                    ErrorMessage = "Failed to process restore for all selected site collections."
                };
            }

            return new RAReturnMessage
            {
                MessageType = RAMessageType.Failed,
                FaildType = RAFailedType.None,
                ErrorMessage = "Exception to process restore for all selected site collections."
            };
        }

        public async Task<List<SiteCollectionNodesInfo>> GetAllConnectionNodesAsync()
        {
            List<SiteCollectionNodesInfo> result = new List<SiteCollectionNodesInfo>();
            bool isOpusILAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOAdmin);
            bool isOpusSOAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.SPOAdmin);
            var permissionContainerIds = await GetPermissionContainerIdsAsync(RMBrowseTreeNodeSourceType.All);
            List<FSMasterIndexContract> index = FSMasterIndexService.GetAllConnectionNodsInfo();
            var connectionIds = index.Select(i => new Guid(i.ConnectionId)).Distinct().ToList();
            var connections = await FSRegisterService.GetConnectionByIdsAsync(connectionIds);
            var existConnections = connections?.Select(a=>a.Id).ToList();
            List<string> needAddConIds = new List<string>();
            List<FSMasterIndexContract> tempIndex = new List<FSMasterIndexContract>();
            foreach (var con in connectionIds)
            {
                if (!existConnections.Contains(con))
                {
                    logger.Info($"this connection not exist,use index info.id:{con}");
                    needAddConIds.Add(con.ToString());
                }
            }
            if (needAddConIds.Count > 0)
            {
                tempIndex = index.Where(a => needAddConIds.Contains(a.ConnectionId)).DistinctBy(a=>a.ConnectionId).ToList();
            }
            logger.Info($"this restore search load connections exist count:{result.Count}");
            if (isOpusILAdmin || isOpusSOAdmin)
            {
                foreach (var con in connections)
                {
                    logger.Info($"this restore search connections name:{con.Name},id:{con.Id.ToString()}");
                    result.Add(new SiteCollectionNodesInfo()
                    {
                        MasterIndexId = con.Id.ToString(),
                        SiteUrl = con.Name,
                        SiteGroupId = con.GroupId.ToString(),
                        SPObjectId = con.Id.ToString(),
                        PermissionLevel = (int)FunctionSubPermission.RestoreCenterFullControl
                    });
                }
                foreach (var ind in tempIndex)
                {
                    logger.Info($"this1 restore search connections name:{ind.ConnectionName},id:{ind.ConnectionId.ToString()}");
                    result.Add(new SiteCollectionNodesInfo()
                    {
                        MasterIndexId = ind.ConnectionId,
                        SiteUrl = ind.ConnectionName,
                        //SiteGroupId = con.GroupId.ToString(),
                        SPObjectId = ind.ConnectionId,
                        PermissionLevel = (int)FunctionSubPermission.RestoreCenterFullControl
                    });
                }
            }
            else
            {
                var tempPermission = await SecurityTrimmingHelper.GetUserRestoreCenterFunctionPermissionAsync();
                bool isFSAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.FSEnduser);
                if(isFSAdmin)
                {
                    foreach (var con in connections)
                    {
                        logger.Info($"this restore search connections name:{con.Name},id:{con.Id.ToString()}");
                        result.Add(new SiteCollectionNodesInfo()
                        {
                            MasterIndexId = con.Id.ToString(),
                            SiteUrl = con.Name,
                            SiteGroupId = con.GroupId.ToString(),
                            SPObjectId = con.Id.ToString(),
                            PermissionLevel = (int)FunctionSubPermission.RestoreCenterFullControl
                        });
                    }
                    foreach (var ind in tempIndex)
                    {
                        logger.Info($"this1 restore search connections name:{ind.ConnectionName},id:{ind.ConnectionId.ToString()}");
                        result.Add(new SiteCollectionNodesInfo()
                        {
                            MasterIndexId = ind.ConnectionId,
                            SiteUrl = ind.ConnectionName,
                            //SiteGroupId = con.GroupId.ToString(),
                            SPObjectId = ind.ConnectionId,
                            PermissionLevel = (int)FunctionSubPermission.RestoreCenterFullControl
                        });
                    }

                }
                else if (tempPermission != FunctionSubPermission.None) //user has function permission
                {
                    foreach (var con in connections)
                    {
                        logger.Info($"this restore search connections name:{con.Name},id:{con.Id.ToString()}");
                        result.Add(new SiteCollectionNodesInfo()
                        {
                            MasterIndexId = con.Id.ToString(),
                            SiteUrl = con.Name,
                            SiteGroupId = con.GroupId.ToString(),
                            SPObjectId = con.Id.ToString(),
                            PermissionLevel = (int)tempPermission
                        });
                    }
                    foreach (var ind in tempIndex)
                    {
                        logger.Info($"this1 restore search connections name:{ind.ConnectionName},id:{ind.ConnectionId.ToString()}");
                        result.Add(new SiteCollectionNodesInfo()
                        {
                            MasterIndexId = ind.ConnectionId,
                            SiteUrl = ind.ConnectionName,
                            //SiteGroupId = con.GroupId.ToString(),
                            SPObjectId = ind.ConnectionId,
                            PermissionLevel = (int)tempPermission
                        });
                    }

                }
            }
            return result.OrderBy(r => r.SiteUrl, StringComparer.Ordinal).ToList();
        }

        public async Task<List<SiteCollectionNodesInfo>> GetAllTeamsNodesAsync()
        {
            List<SiteCollectionNodesInfo> result = new List<SiteCollectionNodesInfo>();
            var restoreCenterPremission = await SecurityTrimmingHelper.GetUserRestoreCenterFunctionPermissionAsync();
            bool isOpusILAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.TeamsAdmin);
            bool isOpusSOAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.TeamsAdmin);
            List<string> fullPaths = new List<string>();
            List<DB.Model.CommonSiteMasterIndex> index = CommonSiteMasterIndexDao.GetAllTeamIndexInfoes();
            var permissionContainerIds = await GetPermissionContainerIdsAsync(RMBrowseTreeNodeSourceType.Teams);
            if (isOpusILAdmin || isOpusSOAdmin)
            {
                result.AddRange(GetSiteCollectionNodes(fullPaths, index));
            }
            else
            {
                var tempPermission = await SecurityTrimmingHelper.GetUserRestoreCenterFunctionPermissionAsync();
                if (tempPermission != FunctionSubPermission.None)
                {
                    var tempAllResult = GetSiteCollectionNodes(fullPaths, index);
                    var tempPermissionResult = GetSiteNodesThatHasPermission(index, permissionContainerIds);
                    foreach (var temp in tempAllResult)
                    {
                        if (!tempPermissionResult.Exists(t => t.SiteUrl == temp.SiteUrl))
                        {
                            temp.PermissionLevel = (int)tempPermission;
                        }
                        result.Add(temp);
                    }
                }
                else
                {
                    result.AddRange(GetSiteNodesThatHasPermission(index, permissionContainerIds));
                }

            }
            return result.OrderBy(r => r.SiteUrl, StringComparer.Ordinal).ToList();
        }

        private List<SiteCollectionNodesInfo> GetSiteNodesThatHasPermission(List<DB.Model.ArchiverSiteMasterIndex> index,List<Guid> permissionContainerIds, HashSet<string> whitelist = null)
        {
            if(permissionContainerIds == null || !permissionContainerIds.Any())
            {
                return new();
            }
            List<SiteCollectionNodesInfo> result = new List<SiteCollectionNodesInfo>();
            var availableSitesUrl = new List<string>();
            HashSet<string> fullPaths = new HashSet<string>();
            var sites = RMRemoteNode.GetSiteCollectionByParentIds(permissionContainerIds.ConvertAll(id => id.ToString())).Values;
            foreach (var siteList in sites)
            {
                availableSitesUrl.AddRange(siteList.Select(site => site.Scope));
            }
            var temps = index.Where(temp => availableSitesUrl.Contains(temp.SiteURL)).ToList();
            result.AddRange(GetSiteCollectionNodes(fullPaths, temps, whitelist));
            return result;
        }

        private List<SiteCollectionNodesInfo> GetSiteNodesThatHasPermission(List<DB.Model.CommonSiteMasterIndex> index, List<Guid> permissionContainerIds, HashSet<string> whitelist = null)
        {
            List<SiteCollectionNodesInfo> result = new List<SiteCollectionNodesInfo>();
            var availableSitesUrl = new List<string>();
            var stringPermissionContainerIds = permissionContainerIds.Select(i => i.ToString());
            var temps = index.Where(i => stringPermissionContainerIds.Contains(i.SiteGroupId)).DistinctBy(i => i.SiteURL).ToList();
            result.AddRange(GetSiteCollectionNodes(new(), temps, whitelist));
            return result;
        }

        private List<SiteCollectionNodesInfo> GetSiteCollectionNodes(HashSet<string> fullPaths, List<DB.Model.ArchiverSiteMasterIndex> index, HashSet<string> whitelist = null)
        {
            List<SiteCollectionNodesInfo> result = new List<SiteCollectionNodesInfo>();
            if (!index.Any())
            {
                return result;
            }
            GeneralSettingModel generalSetting = GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult();
            foreach (var group in index.GroupBy(i => i.SiteURL.ToLower()))
            {
                var temp = group.MaxBy(i => i.ArchiverTime);
                if ((whitelist == null || whitelist.Contains(temp.SiteURL)) && fullPaths.Add(temp.SiteURL))
                {
                    result.Add(new SiteCollectionNodesInfo()
                    {
                        MasterIndexId = temp.Id,
                        SiteUrl = temp.SiteURL,
                        SiteGroupId = temp.SiteGroupId,
                        SPObjectId = temp.SiteId,
                        PermissionLevel = (int)FunctionSubPermission.RestoreCenterFullControl,
                        ArchiveTime = temp.ArchiverTime,
                        ArchivedTime = GeneralSettingService.ConvertTiksToDateTime(generalSetting, temp.ArchiverTime, true).SimplifyFormatTime,
                        ObjectName = temp.SiteURL?.Split('/').LastOrDefault()
                    });
                }
            }
            return result;
        }
        #region google
        private List<SiteCollectionNodesInfo> GetGDriveNodes(HashSet<string> fullPaths, List<DB.Model.ArchiverSiteMasterIndex> index, HashSet<string> whitelist = null)
        {
            List<SiteCollectionNodesInfo> result = new List<SiteCollectionNodesInfo>();
            foreach (var temp in index)
            {
                if ((whitelist == null || whitelist.Contains(temp.SiteURL)) && fullPaths.Add(temp.SiteId))
                {
                    result.Add(new SiteCollectionNodesInfo()
                    {
                        MasterIndexId = temp.Id,
                        SiteUrl = temp.SiteURL,
                        SiteGroupId = temp.SiteGroupId,
                        SPObjectId = temp.SiteId,
                        PermissionLevel = (int)FunctionSubPermission.RestoreCenterFullControl
                    });
                }
            }
            return result;
        }
        private List<SiteCollectionNodesInfo> GetGDriveNodesThatHasPermission(List<DB.Model.ArchiverSiteMasterIndex> index, List<Guid> permissionContainerIds, HashSet<string> whitelist = null)
        {
            List<SiteCollectionNodesInfo> result = new List<SiteCollectionNodesInfo>();
            var availableSitesUrl = new List<string>();
            HashSet<string> fullPaths = new HashSet<string>();
            var sites = RMRemoteNode.GetSiteCollectionByParentIds(permissionContainerIds.ConvertAll(id => id.ToString())).Values;
            foreach (var siteList in sites)
            {
                availableSitesUrl.AddRange(siteList.Select(site => site.Scope));
            }
            var temps = index.Where(temp => availableSitesUrl.Contains(temp.SiteURL)).ToList();
            result.AddRange(GetGDriveNodes(fullPaths, temps, whitelist));
            return result;
        }
        #endregion

        private List<SiteCollectionNodesInfo> GetSiteCollectionNodes(List<string> fullPaths, List<DB.Model.CommonSiteMasterIndex> index, HashSet<string> whitelist = null)
        {
            List<SiteCollectionNodesInfo> result = new List<SiteCollectionNodesInfo>();
            foreach (var temp in index)
            {
                if (!fullPaths.Contains(temp.SiteURL) &&
                    (whitelist == null || whitelist.Contains(temp.SiteURL)))
                {
                    fullPaths.Add(temp.SiteURL);
                    result.Add(new SiteCollectionNodesInfo()
                    {
                        MasterIndexId = temp.Id,
                        SiteUrl = temp.SiteURL,
                        SiteGroupId = temp.SiteGroupId,
                        SPObjectId = temp.SiteId,
                        TeamsId = temp.TeamId,
                        PermissionLevel = (int)FunctionSubPermission.RestoreCenterFullControl
                    });
                }
            }
            return result;
        }

        private async Task<List<SiteCollectionNodesInfo>> CollectSiteNodeInfoesForTeams(List<DB.Model.ArchiverSiteMasterIndex> indexes, List<Guid> permissionContainerIds)
        {
            if(permissionContainerIds == null || !permissionContainerIds.Any())
            {
                return new();
            }
            List<SiteCollectionNodesInfo> result = new List<SiteCollectionNodesInfo>();
            var teamsIds = RMRemoteNode.GetTeamsIdByContainerId(permissionContainerIds.ConvertAll(id => id.ToString()));
            var relatedSiteUrls = await CommonSiteMasterIndexDao.GetAllRelatedSPSiteUrls(teamsIds);
            HashSet<string> availableSitesUrl = new HashSet<string>(relatedSiteUrls);
            foreach (var teamsId in teamsIds)
            {
                var (groupMailbox, channels) = RMRemoteNode.GetTeamsGroupAndChannelsCollectionByTeamsId(teamsId, true);
                availableSitesUrl.Add(groupMailbox.url);
                if (channels != null && channels.Count > 0)
                {
                    availableSitesUrl.UnionWith(channels.Select(c => c.url));
                }
            }
            var temps = indexes.Where(temp => availableSitesUrl.Contains(temp.SiteURL)).ToList();
            return GetSiteCollectionNodes(new HashSet<string>(), temps);
        }

        private async Task<List<Guid>> GetPermissionContainerIdsAsync(RMBrowseTreeNodeSourceType type)
        {
            var containerIds = new List<Guid>();
            try
            {
                var userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
                var allContainers = (await RMScopeRoleAssignmentDao.GetAllContainersByUsersAsync(userAndGroupUserIds)).Where(x => GetPermissionDataSrouceType(type).Contains(x.Key));
                foreach (KeyValuePair<int, List<Guid>> item in allContainers)
                {
                    item.Value.ForEach(o =>
                    {
                        if (!containerIds.Contains(o))
                        {
                            containerIds.Add(o);
                        }
                    });
                }
            }
            catch (Exception ex)
            {
                logger.Error($"Failed to get container ids, error:{ex}");
            }
            return containerIds;
        }
        private List<int> GetPermissionDataSrouceType(RMBrowseTreeNodeSourceType type)
        {
            var types = new List<int>();
            if (type == RMBrowseTreeNodeSourceType.All)
            {
                types.Add((int)SourceFlag.SharePoint);
                types.Add((int)SourceFlag.OneDrive);
                types.Add((int)SourceFlag.Teams);
            }
            if (type == RMBrowseTreeNodeSourceType.SPAndOD)
            {
                types.Add((int)SourceFlag.OneDrive);
                types.Add((int)SourceFlag.SharePoint);
            }
            if (RMBrowseTreeNodeSourceType.SharepointOnline == type)
            {
                types.Add((int)SourceFlag.SharePoint);
            }
            if (RMBrowseTreeNodeSourceType.SPAndOD == type)
            {
                types.Add((int)SourceFlag.SharePoint);
                types.Add((int)SourceFlag.OneDrive);
            }
            if (RMBrowseTreeNodeSourceType.SkyDrivePro == type)
            {
                types.Add((int)SourceFlag.OneDrive);
            }
            if (RMBrowseTreeNodeSourceType.Teams == type)
            {
                types.Add((int)SourceFlag.Teams);
            }
            if (RMBrowseTreeNodeSourceType.Google == type)
            {
                types.Add((int)SourceFlag.Google);
            }
            return types;
        }
     

        private ArchiverRestoreSerchResult ConvertToSiteSearchResult(SiteCollectionNodesInfo node, GeneralSettingModel gls)
        {
            return new ArchiverRestoreSerchResult
            {
                MasterIndexId = node.MasterIndexId,
                SiteUrl = node.SiteUrl,
                SiteGroupId = node.SiteGroupId,
                SPObjectId = node.SPObjectId,
                PermissionLevel = node.PermissionLevel,
                ObjectName = node.SiteUrl,
                Location = node.SiteUrl,
                SitePath = node.SiteUrl,
                FullPath = node.SiteUrl,
                ArchiveTime = node.ArchiveTime,
                ArchivedTime = GeneralSettingService.ConvertTiksToDateTime(gls, node.ArchiveTime, true).SimplifyFormatTime
            };
        }

        private ArchiverRestoreSerchResult ConvertToSerchResult(TreeNode index, ArchiverRestoreFilter filterPolicy, Contract.RMWeb.CP.GeneralSettingModel gls, bool isControlPlus = false)
        {
            ArchiverRestoreSerchResult result = new ArchiverRestoreSerchResult();
            result.TreeNode = SerializerHelper.SerializeByDataContractSerializer(index);
            while (true)
            {
                if (index.Children.Count > 0)
                {
                    if (filterPolicy.Level != PolicyLevel.Document && filterPolicy.Level != PolicyLevel.Attachment && filterPolicy.Level.ToString() == index.TreeNodeLevel.ToString())
                    {
                        if (filterPolicy.Level == PolicyLevel.Folder || filterPolicy.Level == PolicyLevel.Site)
                        {
                            index = index.Children[0];
                        }
                    }
                    else
                    {
                        index = index.Children[0];
                    }
                }
                else
                {
                    break;
                }
            }
            if (index.TreeNodeLevel != TreeNodeLevel.Item)
            {
                if (index.TreeNodeLevel == TreeNodeLevel.Site)
                {
                    result.ObjectName = index.Title;
                }
                else
                {
                    result.ObjectName = index.Name;
                }
            }
            else
            {
                if (index.TypeInIndex == "I")
                {
                    string temp = index.Description.Substring("Title:".Length, index.Description.IndexOf(Environment.NewLine) - "Title:".Length);
                    result.ObjectName = index.Name.IndexOf(":")<0? index.Name+ $"({temp})":index.Name.Insert(index.Name.IndexOf(":"),$"({temp})");
                }
                else if (index.TypeInIndex == "A")
                {
                    result.ObjectName = index.Name;
                }
                else
                {
                    result.ObjectName = index.Name;
                }
            }
            result.JobId = index.JobId;
            result.Id = index.Id;
            result.MainJobId = string.IsNullOrWhiteSpace(index.JobId) ? string.Empty : index.JobId.Split('_')[0];
            result.Author = index.Author;
            result.Location = index.FullPathForUI;
            result.FullPath = index.FullPath;
            result.ParentPathMd5 = index.ParentPathMD5;
            result.PathMd5 = index.PathMD5;
            result.ModifiedBy = index.ModifiedBy;
            result.SitePath = index.SitePath;
            result.IsArchiveTier = index.IsArchiveTier;
            result.ModifiedTime = index.ModifiedTime;
            result.ArchiveTime = index.ArchivedTime;
            result.ContentLenth = index.ContentLenth;
            result.IsSoftDeleted = index.IsSoftDeleted;
            result.IsVersion = !string.IsNullOrEmpty(index.Name) && index.Name.Contains(":");
            if (isControlPlus) gls.TimeZoneId = GeneralSettingService.ConvertBrowserTimeZoneToWindows(TenantLocalValue.TimezoneId);

            if (index.ModifiedTime > 0)
            {
                result.LastModifiedTime = GeneralSettingService.ConvertTiksToDateTime(gls, index.ModifiedTime, true).SimplifyFormatTime;
            }
            else
            {
                result.LastModifiedTime = string.Empty;
            }
            if (index.ArchivedTime > 0)
            {
                result.ArchivedTime = GeneralSettingService.ConvertTiksToDateTime(gls, index.ArchivedTime, true).SimplifyFormatTime;
            }
            else
            {
                result.ArchivedTime = string.Empty;
            }
            if (index.CreatedTime > 0)
            {
                result.CreatedDate = GeneralSettingService.ConvertTiksToDateTime(gls, index.CreatedTime, true).SimplifyFormatTime;
                result.CreatedDateTicks = index.CreatedTime.ToString();
            }
            else
            {
                result.CreatedDate = string.Empty;
            }
            return result;
        }
        private ArchiverRestoreSerchResult ConvertToFSSerchResult(ArchiverBasicIndex index,  Contract.RMWeb.CP.GeneralSettingModel gls)
        {
            ArchiverRestoreSerchResult result = new ArchiverRestoreSerchResult();
            result.ObjectName = index.Name;
            string tempExtraInfo = string.IsNullOrEmpty(index.ExtraInfo)?"": index.ExtraInfo + "\\";
            result.Location = index.Attributes+"\\"+ tempExtraInfo + index.Name;
            result.FullPath = index.Url;
            //result.ParentPathMd5 = index.ParentPathMD5;
            result.PathMd5 = index.PathMD5;
            //result.ModifiedBy = index.ModifiedBy;
            result.SitePath = index.SitePath;
            //result.IsArchiveTier = index.IsArchiveTier;
            result.ModifiedTime = index.ModifyTime;
            result.ArchiveTime = index.ArchiveTime;
            result.ContentLenth = index.ContentLength;
            result.TreeNode = index.Attributes;
            //result.IsSoftDeleted = index.IsSoftDeleted;
            if (index.ModifyTime > 0)
            {
                result.LastModifiedTime = GeneralSettingService.ConvertTiksToDateTime(gls, index.ModifyTime, true).SimplifyFormatTime;
            }
            else
            {
                result.LastModifiedTime = string.Empty;
            }
            if (index.ArchiveTime > 0)
            {
                result.ArchivedTime = GeneralSettingService.ConvertTiksToDateTime(gls, index.ArchiveTime, true).SimplifyFormatTime;
            }
            else
            {
                result.ArchivedTime = string.Empty;
            }
            if (index.CreateTime > 0)
            {
                result.CreatedDate = GeneralSettingService.ConvertTiksToDateTime(gls, index.CreateTime, true).SimplifyFormatTime;
                result.CreatedDateTicks = index.CreateTime.ToString();
            }
            else
            {
                result.CreatedDate = string.Empty;
            }
            return result;
        }

        public bool ShouldQueryInJobForEndUserRestore(string param)
        {
            try
            {
                EndUserRestoreJobConfig endUserRestoreJobConfig = SerializerHelper.DeserializeByDataContractSerializer<EndUserRestoreJobConfig>(param);
                return endUserRestoreJobConfig is EndUserRestoreJobConfig;
            }
            catch
            {
                return false;
            }
        }

        private string AssembleExportStorageXriString()
        {
            string containerName = RMGlobalConfiguration.StorageConfig[RMStorageSettingKey.SHARED_STORAGE_CONTAINER_NAME];
            // string accountName = string.Empty;
            // string accountKey = string.Empty;
            var tempConn = CommonUtilityForSpecialTenant.GetStorageConnectionStringFromConfigFile(StorageStringType.SharedStorage);
            // var setting = ParseStringIntoSettings(tempConn);
            // return $"docave-xam://azure_vim?accessPoint={setting["DefaultEndpointsProtocol"]}://blob.{setting["EndpointSuffix"]}&containerName={containerName}&name={setting["AccountName"]}&secret={setting["AccountKey"].Replace("=","%3D")}";
            return RA.Common.Util.AzureUtil.GetConnectionBuilderString(tempConn, containerName);
        }

        

        public async Task<RestoreSettingAndTree> BuildRestoreSettingAndTreeForEndUserJobAsync(EndUserRestoreJobConfig jobConfig)
        {
            if (jobConfig == null)
            {
                throw new ArgumentNullException(nameof(jobConfig));
            }

            TenantLocalValue.LogonUserEmail = jobConfig.RunJobUser;
            ArchiverRestoreResult searchResult = await SearchEndUserRestoreAsync(jobConfig);
            if (searchResult?.RestoreSerchNodes == null || !searchResult.RestoreSerchNodes.Any())
            {
                throw new Exception("Can not find the restore node,it has retained.");
            }

            RestoreInfo restoreInfo = new RestoreInfo() { NodeObjects = new List<ArchiverRestoreSerchResult>() };
            foreach (var temp in searchResult.RestoreSerchNodes)
            {
                restoreInfo.NodeObjects.Add(new ArchiverRestoreSerchResult() { TreeNode = temp.TreeNode, SitePath = temp.SitePath });
            }
            if (jobConfig.PermissionCheckType == CheckPermissionType.None)
            {
                restoreInfo.IsOpusArchivedDownloadJob = true;
            }

            if (jobConfig.RestoreType == GCommon.Contract.StorageOptimization.Object.RestoreType.InPlace || jobConfig.RestoreType == GCommon.Contract.StorageOptimization.Object.RestoreType.StubOop)
            {
                restoreInfo.RestoreOption = RestoreOption.Append;
                restoreInfo.RestoreTypeSelect = jobConfig.RestoreType == GCommon.Contract.StorageOptimization.Object.RestoreType.InPlace ? GCommon.Contract.Server.Common.BackupDataSearch.RestoreType.InPlace : GCommon.Contract.Server.Common.BackupDataSearch.RestoreType.StubOop;
                restoreInfo.IncludeSharingLink = EndUserSetting.GetEndUserRestoreSetting()?.IsIncludeSharedLinks ?? false;
            }
            else if (jobConfig.RestoreType == GCommon.Contract.StorageOptimization.Object.RestoreType.OutPlace)
            {
                string azureConnectionString = AssembleExportStorageXriString();
                if (string.IsNullOrEmpty(jobConfig.RestoreStorage))
                {
                    restoreInfo.ConnectionString = azureConnectionString;
                }
                else
                {
                    restoreInfo.StorageDeviceDto = StorageDeviceConvert.ConvertStorageDeviceDtoToUIDto(StorageDeviceService.GetStorageDeviceById(jobConfig.RestoreStorage));
                }
                restoreInfo.NotificationUsers = new List<ToExportUserInfo>();
                restoreInfo.RestoreTypeSelect = GCommon.Contract.Server.Common.BackupDataSearch.RestoreType.OutOfPlace;
                restoreInfo.IsRecenterExport = true;
            }

            restoreInfo.IsEndUserJob = true;
            restoreInfo.StubType = jobConfig.StubType;
            restoreInfo.OopStubUrl = string.IsNullOrEmpty(jobConfig.OopStubUrl) ? string.Empty : Uri.UnescapeDataString(jobConfig.OopStubUrl);
            restoreInfo.BackUpJobId = jobConfig.BackUpJobId;
            restoreInfo.NodeType = (int)DocAveOnline.WebApi.Contracts.RemoveNodeType.SiteCollection;
            restoreInfo.AppProfileId = jobConfig.AppProfileId;
            restoreInfo.SiteAdminUrl = jobConfig.SiteAdminUrl;

            RestoreSettingAndTree restoreSettingAndTree = BuildRestoreSettingAndTree(restoreInfo);
            restoreSettingAndTree.RealRunJobUser = jobConfig.RunJobUser;
            restoreSettingAndTree.BackUpJobId = jobConfig.BackUpJobId;
            restoreSettingAndTree.IsEndUserJob = true;
            restoreSettingAndTree.SiteGroupId = restoreSettingAndTree.SiteGroupId ?? searchResult?.SerchContract?.SearchNode?.SiteGroupId;
            restoreSettingAndTree.JobId = restoreInfo.JobId;
            restoreSettingAndTree.oopStubUrl = restoreInfo.OopStubUrl;
            restoreSettingAndTree.IsOpusArchivedDownloadJob = restoreInfo.IsOpusArchivedDownloadJob;
            return restoreSettingAndTree;
        }

        private async Task<ArchiverRestoreResult> SearchEndUserRestoreAsync(EndUserRestoreJobConfig jobConfig)
        {
            ArchiverRestoreResult searchResult = await SearchOnceAsync(jobConfig);
            if (searchResult?.RestoreSerchNodes != null && searchResult.RestoreSerchNodes.Any())
            {
                return searchResult;
            }

            StubRebuildMd5Configs stubRebuildMd5Configs = null;
            logger.Info($"unable found ArchiverRestore record, sc:{jobConfig.SiteUrl}");
            foreach (var url in RMRestoreSiteMappingDao.GetSourceSCUrlsByTargetSCUrl(jobConfig.SiteUrl))
            {
                jobConfig.SiteUrl = url;
                logger.Info($"try found ArchiverRestore record, sc:{jobConfig.SiteUrl}");
                searchResult = await SearchOnceAsync(jobConfig);
                if (searchResult?.RestoreSerchNodes != null && searchResult.RestoreSerchNodes.Any())
                {
                    logger.Info($"find ArchiverRestore record by site mapping url:{url}");
                    break;
                }
                else if (jobConfig.PermissionCheckType == CheckPermissionType.StubRestoreLink)
                {
                    try
                    {
                        if (stubRebuildMd5Configs == null)
                        {
                            stubRebuildMd5Configs = GetStubRebuildMd5Configs();
                        }

                        StubRebuildMd5Config config = stubRebuildMd5Configs?.Configs?.FirstOrDefault(c => c != null && url.Equals(c?.SiteCollectionUrl));
                        if (config == null)
                        {
                            continue;
                        }

                        string oldPathMd5 = jobConfig.Items[0].PathMD5;
                        jobConfig.Items[0].PathMD5 = HashCodeHelper.ToMD5HashCode(BuildDestinationPathFromFullUrl(url, jobConfig.Items[0].FullPath, config.LibPathMapping));
                        searchResult = await SearchOnceAsync(jobConfig);
                        if (searchResult?.RestoreSerchNodes != null && searchResult.RestoreSerchNodes.Any())
                        {
                            logger.Info($"find ArchiverRestore record by old url and site mapping url:{url}");
                            break;
                        }
                        jobConfig.Items[0].PathMD5 = oldPathMd5;
                    }
                    catch (Exception ex)
                    {
                        logger.Warn($"exception found site index by sc,sc:{jobConfig.SiteUrl},ex:{ex}");
                        continue;
                    }
                }
            }
            return searchResult;
        }

        private async Task<ArchiverRestoreResult> SearchOnceAsync(EndUserRestoreJobConfig jobConfig)
        {
            try
            {
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                DB.Model.ArchiverSiteMasterIndex siteIndex = await ArchiverSiteMasterIndexDao.GetLatestSiteCollectionNodeInfoByUrlAsync(jobConfig.SiteUrl);

                if (siteIndex == null)
                {
                    logger.Warn($"unable found site index by sc,sc:{jobConfig.SiteUrl}");
                    return null;
                }

                ArchiverRestoreResult searchResult = await GetSearchTreeResultAsync(new ArchiverRestoreResult()
                {
                    PageSize = -1,
                    SerchContract = new BackupDataSearchContract()
                    {
                        SearchNode = new SiteCollectionNodesInfo() { SiteUrl = jobConfig.SiteUrl, SiteGroupId = siteIndex.SiteGroupId },
                        FilterPolicy = new GCommon.Contract.CommonFilter.ArchiverRestoreFilter() { FilterName = "", Level = AvePoint.GCommon.Contract.CommonFilter.PolicyLevel.DocumentVersion, PathMD5List = jobConfig.Items.Select(i => i.PathMD5).ToList(), FilterDeleteType = GCommon.Contract.CommonFilter.FilterDeletedType.Normal },
                        BackupJobId = jobConfig.BackUpJobId
                    }
                }, false);
                stopwatch.Stop();
                logger.Info($"linkRestoreReport stub restore search cost time:{stopwatch.ElapsedMilliseconds}");
                return searchResult;
            }
            catch (Exception e)
            {
                logger.Warn($"exception found site index by sc,sc:{jobConfig.SiteUrl},ex:{e}");
                return null;
            }
        }

        private StubRebuildMd5Configs GetStubRebuildMd5Configs()
        {
            StubRebuildMd5Configs result = new StubRebuildMd5Configs();
            try
            {
                var key = RMKeyValueDao.GetValueByKey("StubRebuildMd5Configs");
                if (string.IsNullOrWhiteSpace(key?.Value))
                {
                    result = new StubRebuildMd5Configs();
                }
                else
                {
                    result = SerializerHelper.DeserializeByJsonSerializer<StubRebuildMd5Configs>(key?.Value);
                }
            }
            catch (Exception ex)
            {
                logger.Warn($"Error Deserialize StubRebuildMd5Configs,Exception:{ex.ToString()}");
            }
            return result;
        }

        private string BuildDestinationPathFromFullUrl(string siteCollectionUrl, string fullFileUrl, Dictionary<string, string> libPathMapping)
        {
            try
            {
                var uri = new Uri(fullFileUrl);
                return BuildDestinationPath(siteCollectionUrl, uri.LocalPath, libPathMapping);
            }
            catch (Exception ex)
            {
                logger.Error($"Error in BuildDestinationPathFromFullUrl: {ex.Message}");
                return null;
            }
        }

        private string BuildDestinationPath(string siteCollectionUrl, string fileRelativeUrl, Dictionary<string, string> libPathMapping)
        {
            try
            {
                string fileSubPath = fileRelativeUrl.Trim('/');

                foreach (var item in libPathMapping)
                {
                    var libPath = item.Key.Trim('/');
                    var index = fileSubPath.IndexOf(libPath, StringComparison.OrdinalIgnoreCase);
                    if (index >= 0)
                    {
                        fileSubPath = $"{item.Value}{fileSubPath.Substring(index + libPath.Length).Replace('/', '\\')}";
                        break;
                    }
                }

                return $"{siteCollectionUrl.TrimEnd('/')}\\{fileSubPath}";
            }
            catch (Exception ex)
            {
                logger.Error($"Error in BuildDestinationPath: {ex.Message}");
                return string.Empty;
            }
        }

        private string GetRestoreScopeForEndUserJob(JobType jobType, EndUserRestoreJobConfig jobConfig)
        {
            if (jobConfig == null)
            {
                return string.Empty;
            }

            string restoreScope = string.Empty;
            if (jobType == JobType.StubOopRestore && !string.IsNullOrWhiteSpace(jobConfig.OopStubUrl))
            {
                restoreScope = GenerateSiteCollecitonUrl(jobConfig.OopStubUrl);
            }
            else if (jobType == JobType.ArchiverOutPlaceRestore)
            {
                restoreScope = "RM_RS_RecenterDefaultLocation";
            }
            else if ((jobType == JobType.ArchiverRestore || jobType == JobType.AOSPRestore) && !string.IsNullOrWhiteSpace(jobConfig.SiteUrl))
            {
                restoreScope = GenerateSiteCollecitonUrl(jobConfig.SiteUrl);
                if (!string.IsNullOrWhiteSpace(restoreScope))
                {
                    RMRestoreSiteMapping siteMappingInfo = RMRestoreSiteMappingDao.GetMappingBySourceSiteUrl(restoreScope);
                    if (siteMappingInfo != null)
                    {
                        restoreScope = siteMappingInfo.TargetSiteUrl;
                    }
                }
            }
            else if (jobType == JobType.ArchiverToSpoRestore && !string.IsNullOrWhiteSpace(jobConfig.SiteUrl))
            {
                restoreScope = GenerateSiteCollecitonUrl(jobConfig.SiteUrl);
            }

            if (string.IsNullOrWhiteSpace(restoreScope))
            {
                restoreScope = jobConfig.SiteUrl;
            }

            return restoreScope;
        }

        private string CreateAndQueueSubJob(JobRunBy runBy, string jobId, JobType jobType, string param, string restoreScope, string tenantId)
        {
            var enableSuperPriorityJobQueue = bool.TryParse(RMKeyValueDao.GetValueByKey(RMKeyValuesConstants.ENABLE_SUPER_PRIORITY_JOB_QUEUE)?.Value, out var enableSuperQueue) && enableSuperQueue;
            string superJobQueueName = null;
            if (enableSuperPriorityJobQueue)
            {
                superJobQueueName = RMKeyValueDao.GetValueByKey(RMKeyValuesConstants.SUPER_PRIORITY_JOB_QUEUE_NAME)?.Value;
                if (string.IsNullOrEmpty(superJobQueueName))
                {
                    superJobQueueName = RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.HIGHEST_PRIORITY_JOB_QUEUE_NAME];
                    if (string.IsNullOrEmpty(superJobQueueName))
                    {
                        logger.Error("Enable highest job queue, but not config for it");
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_JS_DAM_RunJob_Failed");
                        return string.Empty;
                    }
                }
                else
                {
                    logger.Info($"Custom highest job queue name: {superJobQueueName}");
                }
            }

            string subJobId = CreateSubJobForDisposal(jobId, 0, jobType, 1, param, true, restoreScope, tenantId);

            if (RMKeyValueDao.GetSubJobCountFromDB((int)jobType) > 0)
            {
                if (enableSuperPriorityJobQueue)
                {
                    logger.Info($"Start to send {subJobId} to highest job queue {superJobQueueName}");
                    JobQueueService.HandleCustomerMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = runBy,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType, subJobId),
                    }, superJobQueueName);
                }
                else
                {
                    JobQueueService.HandleO365Message(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = runBy,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType, subJobId),
                    });
                }
            }

            return subJobId;
        }


        public string RealRunEndUserArchiverRestoreJob(JobRunBy jobRunBy, string jobRunByUser, string param, JobType tempJobType, JobPriority jobPriority = JobPriority.Normal)
        {
            logger.Info("Start real run end user archiver restore job");
            EndUserRestoreJobConfig jobConfig = SerializerHelper.DeserializeByDataContractSerializer<EndUserRestoreJobConfig>(param);
            if (jobConfig == null)
            {
                logger.Warn("end user restore job config is null");
                return string.Empty;
            }

            string restoreScope = GetRestoreScopeForEndUserJob(tempJobType, jobConfig);

            List<JobType> types = new List<JobType>() { tempJobType };
            string jobId = RMJobService.GenerateJobId(tempJobType);
            var account = AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail).GetAwaiter().GetResult();

            if (tempJobType == JobType.ArchiverOutPlaceRestore && account!=null)
            {
                if (!string.IsNullOrWhiteSpace(jobId))
                {
                    DownloadDataInfoDao.CreateDownloadDataInfo(new RMDownloadDataInfo()
                    {
                        FileDownloadTime = DateTime.UtcNow.Ticks,
                        JobId = jobId,
                        RecordsId = (Guid)(jobConfig.Items.FirstOrDefault()?.ItemId),
                        JobStatus = (int)DownloadContentJobStatus.Wait,
                        UserId = account.UserId,
                        Name = jobConfig.Items.FirstOrDefault()?.Name,
                    });
                }
            }
            
            logger.Info($"Restore archived content job start success. JobId:{jobId}");
            //TenantLocalValue.LogonUserEmail = jobRunByUser;
            RMJobService.CreateJobWithScopeIdForRecenter(tempJobType, jobRunByUser, restoreScope, jobId, (int)DocAveOnline.WebApi.Contracts.RemoveNodeType.SiteCollection, jobRunByUser, jobConfig?.SiteUrl);
            JMDao.UpdateJobPriorityAsync(new List<string> { jobId }, jobPriority).GetAwaiter().GetResult();

            bool canRunRestore = CheckRestoreLimit();
            if (!canRunRestore)
            {
                logger.Warn("Your month restore size is running out.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_Job_RestoreSizeLimit");
                return jobId;
            }

            List<JobType> indexJobTypes = new List<JobType>() { JobType.ArchiverMoveIndex };
            var mIndexJobs = RMJobService.GetRunningJobs(indexJobTypes);
            if (mIndexJobs.Count > 0)
            {
                logger.Warn("Current has move index job running.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }

            string subJobId = CreateAndQueueSubJob(jobRunBy, jobId, tempJobType, param, restoreScope, jobConfig?.O365TenantId);
            if (string.IsNullOrEmpty(subJobId))
            {
                return jobId;
            }

            logger.Info("Job has start running");
            return jobId;
        }

        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.RunArchiverRestoreJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public string RealRunArchiverRestoreJob(JobRunBy jobRunBy, string jobRunByUser, string param, JobType tempJobType, JobPriority jobPriority = JobPriority.Normal)
        {
            logger.Info("Start real run archiver restore job");
            Stopwatch sw = new Stopwatch();
            sw.Start();
            List<JobType> types = new List<JobType>() { tempJobType };
            RestoreSettingAndTree restoreSettingAndTree = new RestoreSettingAndTree();
            restoreSettingAndTree = SerializerHelper.DeserializeByDataContractSerializer<RestoreSettingAndTree>(param);
            JobType jobType = tempJobType;
            string jobId = restoreSettingAndTree.JobId;
            bool skipCheckRunningRestoreJob = RMKeyValueDao.HasSkipCheckRunningRestoreJob();
            string restoreScope = GetRestoreScope(jobType, restoreSettingAndTree);

            if (!restoreSettingAndTree.IsEndUserJob && tempJobType != JobType.ArchiverOutPlaceRestore 
                && RMJobService.HasRunningArchiverJobOnScope(types, restoreScope) && !skipCheckRunningRestoreJob)
            {
                if (string.IsNullOrEmpty(jobId))
                {
                    jobId = RMJobService.CreateJobWithScopeId(tempJobType, jobRunByUser, restoreScope, restoreSettingAndTree.SiteGroupId);
                    GenerateAndUpdateRestoreJobExtension(restoreSettingAndTree, jobId);
                }
                else
                {
                    TenantLocalValue.LogonUserEmail = jobRunByUser;
                    RMJobService.CreateJobWithScopeIdForRecenter(tempJobType, jobRunByUser, restoreScope, jobId, restoreSettingAndTree.NodeType, restoreSettingAndTree.RealRunJobUser, restoreSettingAndTree.SiteGroupId);
                    GenerateAndUpdateRestoreJobExtension(restoreSettingAndTree, jobId);
                }
                logger.Warn($"Current has job running on same scope.{restoreScope}，jobid:{jobId}");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }
            else
            {
                if (string.IsNullOrEmpty(jobId))
                {
                    jobId = RMJobService.CreateJobWithScopeId(tempJobType, jobRunByUser, restoreScope, restoreSettingAndTree.SiteGroupId);
                    GenerateAndUpdateRestoreJobExtension(restoreSettingAndTree, jobId);
                }
                else
                {
                    logger.Info("this is enduser restore job");
                    TenantLocalValue.LogonUserEmail = jobRunByUser;
                    if (!string.IsNullOrEmpty(restoreSettingAndTree.Setting.FailedJobId))
                    {
                        jobId = RMJobService.GenerateJobId(jobType);
                    }
                    RMJobService.CreateJobWithScopeIdForRecenter(tempJobType, jobRunByUser, restoreScope, jobId, restoreSettingAndTree.NodeType, restoreSettingAndTree.RealRunJobUser, restoreSettingAndTree.SiteGroupId);
                    GenerateAndUpdateRestoreJobExtension(restoreSettingAndTree, jobId);
                }
            }
            string O365TenantId = string.Empty;
            if (!restoreSettingAndTree.IsEndUserJob)
            {
                string tempSitePath;
                if (restoreSettingAndTree?.Setting?.NodeObjects == null || restoreSettingAndTree?.Setting?.NodeObjects.Count == 0)
                {
                    tempSitePath = restoreSettingAndTree.Setting.SiteUrl;
                }
                else
                {
                    tempSitePath = restoreSettingAndTree?.Setting?.NodeObjects[0]?.SitePath;
                }
                if (string.IsNullOrEmpty(tempSitePath))
                {
                    tempSitePath = restoreSettingAndTree.SiteGroupId;
                    logger.Info($"this tempSitePath is empty,will use SiteGroupId : {tempSitePath}");
                }
                O365TenantId = GetO365TenantId(tempSitePath);
            }
            logger.Info($"Create job success,job id:{jobId}");
            JMDao.UpdateJobPriorityAsync(new List<string> { jobId }, jobPriority).GetAwaiter().GetResult();

            bool canRunRestore = CheckRestoreLimit();
            if (!canRunRestore)
            {
                logger.Warn("Your month restore size is running out.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_Job_RestoreSizeLimit");
                return jobId;
            }
            List<JobType> indexJobTypes = new List<JobType>() { JobType.ArchiverMoveIndex };
            var mIndexJobs = RMJobService.GetRunningJobs(indexJobTypes);

            if (mIndexJobs.Count > 0)
            {
                logger.Warn("Current has move index job running.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }

            string subJobId = CreateAndQueueSubJob(JobRunBy.Control, jobId, jobType, param, restoreScope, O365TenantId);
            sw.Stop();
            logger.Info($"linkRestoreReport RealRunArchiverRestoreJob query db cost time:{sw.ElapsedMilliseconds}");

            if (string.IsNullOrEmpty(subJobId))
            {
                return jobId;
            }

            logger.Info("Job has start running");
            return jobId;
        }
        private void GenerateAndUpdateRestoreJobExtension(RestoreSettingAndTree tempRestoreSetting,string jobId)
        {
            try
            {
                var tempSettingString = SerializerHelper.SerializeByJsonSerializer(tempRestoreSetting);
                RestoreSettingAndTree mRestore = SerializerHelper.DeserializeByJsonSerializer<RestoreSettingAndTree>(tempSettingString);
                var siteUrl = mRestore.Tree != null && mRestore.Tree.Count > 0
                    ? mRestore.Tree[0].SitePath
                    : mRestore.Setting?.RestoreExecutionRequest?.Scope ?? mRestore.Setting?.SiteUrl ?? "";
                mRestore.Tree = null;
                if (mRestore.Setting != null)
                {
                    mRestore.Setting.NodeObjects = null;
                    mRestore.Setting.SerchContract = null;
                    mRestore.Setting.SiteUrl = siteUrl;
                    mRestore.IsSearchAllRestore = false;
                    mRestore.BackUpJobId = "";
                    logger.Info($"this restore setting conflict action is {mRestore.Setting.RestoreOption.ToString()}");
                    if (mRestore.Setting.RestoreOption == RestoreOption.NotOverWrite)
                    {
                        mRestore.Setting.RestoreOption = RestoreOption.Append;
                    }
                    mRestore.Setting.RestoreVersionOption = RestoreDocumentVersionsOption.None;
                }
                JobMonitorService.UpdateJobExtensionById(jobId, SerializerHelper.SerializeByJsonSerializer(mRestore));
            }
            catch (Exception e)
            {
                logger.Error($"error occured when GenerateAndUpdateRestoreJobExtension,error:{e},jobid:{jobId},skip add extension");
            }
        }
        public string RealRunMultiSiteCollectionRestoreJob(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            JobType jobType = JobType.MultiSiteCollectionRestore;
            string rebuildJobId = RMJobService.GenerateJobId(JobType.MultiSiteCollectionRestore);
            SubJobDao.AddJobContext(rebuildJobId, param);
            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = rebuildJobId,
                RunBy = JobRunBy.Control,
                JobType = jobType,
                CommandLine = string.Format("{0} {1} {2}", jobType, rebuildJobId, param),
            });
            logger.Info($"Create virtual sub job {rebuildJobId} sucessfull, type MultiSiteCollectionRestore. param:{param}.");
            return rebuildJobId;
        }

        public async Task SaveBaseArchiveJobIdMultiRestoreSettingAndRunAsync(BackendBatchRestoreInfo backendBatchRestoreInfo)
        {

            RestoreInfo info = new()
            {
                DataSource = (int)RestoreDataSource.M365,
                IncludeSharingLink = backendBatchRestoreInfo.IncludeSharingLink,
                //KeepVersionsNumber = 1,
                RestoreVersionOption = RestoreDocumentVersionsOption.AllVersions,
                RestoreAPPOption = RestoreOption.NotOverWrite,
                RestoreOption = RestoreOption.NotOverWrite,
                RestoreTypeSelect = GCommon.Contract.Server.Common.BackupDataSearch.RestoreType.InPlace,
                SpecifyUserList = new(),
                SerchContract = new BackupDataSearchContract()
                {
                    FilterPolicy = new ArchiverRestoreFilter
                    {
                        FilterDeleteType = FilterDeletedType.All,
                        DataSource = (int)RestoreDataSource.M365,
                        Level = PolicyLevel.Document,
                        FilterName = ""
                    }
                }
            };
            if (backendBatchRestoreInfo.SubJobIDs != null && backendBatchRestoreInfo.SubJobIDs.Count > 0)
            {
            List<DB.Model.ArchiverSiteMasterIndex> masterIndexes = ArchiverSiteMasterIndexDao.GetSiteMastersInfoByJobIds(backendBatchRestoreInfo.SubJobIDs);
                logger.Info($"SubJobIDs.masterIndex count is:{masterIndexes.Count}");
            var siteGroupToSiteMasters = masterIndexes.GroupBy(index => index.SiteURL);

            foreach(var group in siteGroupToSiteMasters)
            {
                logger.Info($"Start process site collection:{group.Key}");
                foreach(var siteMaster in group)
                {
                    logger.Info($"Start process sc:{group.Key}, job :{siteMaster?.JobId}");
                    try
                    {
                        ArchiverRestoreSerchResult siteCollection = ConvertToSiteSearchResult(siteMaster, null);
                        info.NodeObjects = new List<ArchiverRestoreSerchResult>() { siteCollection };
                        info.SerchContract.SearchNode = siteCollection;
                        info.SerchContract.FilterPolicy.MainJobId = siteMaster.JobId.Split('_').First();
                        await TryRunSiteCollectionRestoreAsync(info, siteCollection, GCommon.Contract.StorageOptimization.Object.RestoreType.InPlace);
                    }
                    catch(Exception e)
                    {
                        logger.Error($"fail process sc:{group.Key}, job :{siteMaster?.JobId},ex:{e}");
                    }
                }
                CleanAllBrowerCacheInfo();
            }
            }
            else if (backendBatchRestoreInfo.JobIDs != null && backendBatchRestoreInfo.JobIDs.Count > 0)
            {
                foreach (var mainJobId in backendBatchRestoreInfo.JobIDs)
                {
                    List<DB.Model.ArchiverSiteMasterIndex> masterIndexes = ArchiverSiteMasterIndexDao.GetSiteMastersInfoByMainJobId(mainJobId);
                    logger.Info($"JobIDs.masterIndex count is:{masterIndexes.Count}.JobId:{mainJobId}.");
                    var siteGroupToSiteMasters = masterIndexes.GroupBy(index => index.SiteURL);

                    foreach (var group in siteGroupToSiteMasters)
                    {
                        logger.Info($"Start process site collection:{group.Key}");
                        foreach (var siteMaster in group)
                        {
                            logger.Info($"Start process sc:{group.Key}, job :{siteMaster?.JobId}");
                            try
                            {
                                ArchiverRestoreSerchResult siteCollection = ConvertToSiteSearchResult(siteMaster, null);
                                info.NodeObjects = new List<ArchiverRestoreSerchResult>() { siteCollection };
                                info.SerchContract.SearchNode = siteCollection;
                                info.SerchContract.FilterPolicy.MainJobId = siteMaster.JobId.Split('_').First();
                                await TryRunSiteCollectionRestoreAsync(info, siteCollection, GCommon.Contract.StorageOptimization.Object.RestoreType.InPlace);
                            }
                            catch (Exception e)
                            {
                                logger.Error($"fail process sc:{group.Key}, job :{siteMaster?.JobId},ex:{e}");
                            }
                        }
                        CleanAllBrowerCacheInfo();
                    }
                }
            }
            logger.Info("Finish multi-site collection restore request.");
        }

        private ArchiverRestoreSerchResult ConvertToSiteSearchResult(AvePoint.RA.DB.Model.ArchiverSiteMasterIndex node, GeneralSettingModel gls)
        {
            var res = new ArchiverRestoreSerchResult
            {
                MasterIndexId = node.Id,
                SiteUrl = node.SiteURL,
                SiteGroupId = node.SiteGroupId,
                SPObjectId = node.SiteId,
                PermissionLevel = (int)FunctionSubPermission.RestoreCenterFullControl,
                ObjectName = node.SiteURL,
                Location = node.SiteURL,
                SitePath = node.SiteURL,
                FullPath = node.SiteURL,
                ArchiveTime = node.ArchiverTime,
            };
            if(gls != null)
            {
                res.ArchivedTime = GeneralSettingService.ConvertTiksToDateTime(gls, node.ArchiverTime, true).SimplifyFormatTime;
            }
            return res;
        }

        private async Task<bool> TryRunSiteCollectionRestoreAsync(RestoreInfo baseInfo, ArchiverRestoreSerchResult siteCollection, GCommon.Contract.StorageOptimization.Object.RestoreType restoreType)
        {
            try
            {
                ArchiverRestoreResult searchRequest = BuildSiteCollectionSearchContract(siteCollection);
                if (searchRequest == null)
                {
                    return false;
                }

                logger.Info($"Start search before restore for site collection: {searchRequest.SerchContract?.SearchNode?.SiteUrl}");

                ArchiverRestoreResult searchResult = await GetSearchTreeResultAsync(searchRequest);

                List<ArchiverRestoreSerchResult> restoreNodes = GetRestoreNodesForProcessing(searchResult);

                if (restoreNodes.Count == 0)
                {
                    logger.Warn($"No archived items matched the provided criteria for site: {searchRequest.SerchContract?.SearchNode?.SiteUrl}");
                    return false;
                }

                List<ArchiverRestoreSerchResult> selectedNodes = restoreNodes.Take(1).ToList();

                RestoreInfo restoreInfo = CloneRestoreInfo(baseInfo);
                if (restoreInfo == null)
                {
                    return false;
                }

                restoreInfo.NodeObjects = selectedNodes;

                RAReturnMessage tempResult = SaveAndRunRestoreJob(restoreInfo, restoreType);
                return tempResult != null && tempResult.MessageType == RAMessageType.Successful;
            }
            catch (Exception exception)
            {
                logger.Error($"TryRunSiteCollectionRestoreAsync failed for site: {siteCollection?.Location}", exception);
                return false;
            }
        }

        private List<ArchiverRestoreSerchResult> GetRestoreNodesForProcessing(ArchiverRestoreResult searchResult)
        {
            if (searchResult?.RestoreSerchNodes == null)
            {
                return new List<ArchiverRestoreSerchResult>();
            }

            List<ArchiverRestoreSerchResult> restoreNodes = searchResult.RestoreSerchNodes
                .Where(node => node != null && !string.IsNullOrWhiteSpace(node.TreeNode))
                .ToList();

            return restoreNodes;
        }

        // Preview-only counterpart of TryRunSiteCollectionRestoreAsync: resolves the restorable root node for a
        // site collection the same way, but returns the expanded RestoreInfo instead of running a restore job,
        // so callers can batch multiple site collections into a single PreviewRestore job.
        private async Task<RestoreInfo> BuildPreviewRestoreInfoForSiteCollectionAsync(RestoreInfo baseInfo, ArchiverRestoreSerchResult siteCollection)
        {
            try
            {
                ArchiverRestoreResult searchRequest = BuildSiteCollectionSearchContract(siteCollection);
                if (searchRequest == null)
                {
                    return null;
                }

                logger.Info($"Start search before preview restore for site collection: {searchRequest.SerchContract?.SearchNode?.SiteUrl}");

                ArchiverRestoreResult searchResult = await GetSearchTreeResultAsync(searchRequest);

                List<ArchiverRestoreSerchResult> restoreNodes = GetRestoreNodesForProcessing(searchResult);

                if (restoreNodes.Count == 0)
                {
                    logger.Warn($"No archived items matched the provided criteria for site: {searchRequest.SerchContract?.SearchNode?.SiteUrl}");
                    return null;
                }

                List<ArchiverRestoreSerchResult> selectedNodes = restoreNodes.Take(1).ToList();

                RestoreInfo restoreInfo = CloneRestoreInfo(baseInfo);
                if (restoreInfo == null)
                {
                    return null;
                }

                restoreInfo.NodeObjects = selectedNodes;
                return restoreInfo;
            }
            catch (Exception exception)
            {
                logger.Error($"BuildPreviewRestoreInfoForSiteCollectionAsync failed for site: {siteCollection?.Location}", exception);
                return null;
            }
        }

        private RestoreSettingAndTree BuildPendingPreviewRestoreSettingAndTree(RestoreInfo baseInfo, ArchiverRestoreSerchResult siteCollection)
        {
            RestoreInfo perSiteInfo = CloneRestoreInfo(baseInfo);
            perSiteInfo.NodeObjects = new List<ArchiverRestoreSerchResult> { siteCollection };
            return new RestoreSettingAndTree
            {
                Setting = perSiteInfo,
            };
        }

        public async Task<RestoreSettingAndTree> ResolvePendingPreviewRestoreTreeAsync(RestoreInfo perSiteCollectionInfo)
        {
            ArchiverRestoreSerchResult siteCollection = perSiteCollectionInfo?.NodeObjects?.FirstOrDefault();
            if (siteCollection == null)
            {
                logger.Warn("ResolvePendingPreviewRestoreTreeAsync received no site collection to resolve.");
                return null;
            }

            try
            {
                RestoreInfo resolvedInfo = await BuildPreviewRestoreInfoForSiteCollectionAsync(perSiteCollectionInfo, siteCollection);
                if (resolvedInfo == null)
                {
                    return null;
                }

                return BuildRestoreSettingAndTree(resolvedInfo);
            }
            finally
            {
                // Each pending site collection downloads its own index db while resolving, same as
                // SaveMultiSiteCollectionRestoreSettingAndRunAsync does per site collection. Clean it up
                // right after use so disk space doesn't accumulate across a large multi-site-collection job.
                CleanAllBrowerCacheInfo();
            }
        }

        private void CleanAllBrowerCacheInfo()
        {
            try
            {
                CacheSettingService.CleanAllBrowerCacheInfo();
            }
            catch (Exception e)
            {
                logger.Error($"Fail CleanAllBrowerCacheInfo,e :{e}");
            }
        }

        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.RunTeamsArchiverRestoreJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public string RealRunTeamsArchiverRestoreJob(JobRunBy jobRunBy, string jobRunByUser, string param, JobType tempJobType, JobPriority jobPriority = JobPriority.Normal)
        {
            logger.Info("Start real run archiver restore job");
            List<JobType> types = new List<JobType>() { tempJobType };
            RestoreSettingAndTree restoreSettingAndTree = new RestoreSettingAndTree();
            restoreSettingAndTree = SerializerHelper.DeserializeByDataContractSerializer<RestoreSettingAndTree>(param);
            JobType jobType = tempJobType;
            string jobId = restoreSettingAndTree.JobId;
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            bool skipCheckRunningRestoreJob = RMKeyValueDao.HasSkipCheckRunningRestoreJob();
            List<SPTreeNodeDto> selectedNodes = restoreSettingAndTree.Tree;
            SPTreeNodeDto selectedNode = selectedNodes.First();
            //string restoreScope = GetRestoreScope(jobType, restoreSettingAndTree);
            string restoreScope = selectedNode.FullPath;
            if(tempJobType == JobType.TeamsOutPlaceRestore)
            {
                restoreScope = restoreSettingAndTree.Setting?.StorageDeviceDto?.Name ?? string.Empty;
            }

            //if (!restoreSettingAndTree.IsEndUserJob && RMJobService.HasRunningArchiverJobOnScope(types, restoreScope) && !skipCheckRunningRestoreJob)
            //{
            //    logger.Warn($"Current has job running on same scope.{restoreScope}");
            //    if (string.IsNullOrEmpty(jobId))
            //    {
            //        jobId = RMJobService.CreateJobWithScopeId(tempJobType, jobRunByUser, restoreScope, restoreSettingAndTree.SiteGroupId);
            //    }
            //    else
            //    {
            //        TenantLocalValue.LogonUserEmail = jobRunByUser;
            //        RMJobService.CreateJobWithScopeIdForRecenter(tempJobType, jobRunByUser, restoreScope, jobId, restoreSettingAndTree.NodeType, restoreSettingAndTree.RealRunJobUser, restoreSettingAndTree.SiteGroupId);
            //    }
            //    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
            //    return jobId;
            //}
            //else
            //{
                if (string.IsNullOrEmpty(jobId))
                {
                    jobId = RMJobService.CreateJobWithScopeId(tempJobType, jobRunByUser, restoreScope, restoreSettingAndTree.SiteGroupId);
                }
                else
                {
                    logger.Info("this is enduser restore job");
                    TenantLocalValue.LogonUserEmail = jobRunByUser;
                    RMJobService.CreateJobWithScopeIdForRecenter(tempJobType, jobRunByUser, restoreScope, jobId, restoreSettingAndTree.NodeType, restoreSettingAndTree.RealRunJobUser, restoreSettingAndTree.SiteGroupId);
                }
            //}
            string O365TenantId = string.Empty;
            string tempSitePath = restoreSettingAndTree?.Setting?.NodeObjects?.FirstOrDefault()?.SitePath;
            if (!string.IsNullOrWhiteSpace(tempSitePath))
            {
                O365TenantId = GetO365TeamsTenantId(tempSitePath);
            }
            logger.Info($"Create job success,job id:{jobId}");
            JMDao.UpdateJobPriorityAsync(new List<string> { jobId }, jobPriority).GetAwaiter().GetResult();
            //List<RMSPTreeNode> availableNode = AssembleDisposalRunnableNode(selectedNode);

            //if (availableNode.IsNullOrEmpty())
            //{
            //    logger.Warn("No available sc to run");
            //    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_SS_NoSCUnderGroup");
            //    return jobId;
            //}
            bool canRunRestore = CheckRestoreLimit();
            if (!canRunRestore)
            {
                logger.Warn("Your month restore size is running out.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_Job_RestoreSizeLimit");
                return jobId;
            }
            List<JobType> indexJobTypes = new List<JobType>() { JobType.ArchiverMoveIndex };
            var mIndexJobs = RMJobService.GetRunningJobs(indexJobTypes);

            if (mIndexJobs.Count > 0)
            {
                //has move index job, need skip.
                logger.Warn("Current has move index job running.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }

            var scopes = RMJobService.GetRunningArchiverJobsScopes(types);
            if (!restoreSettingAndTree.IsEndUserJob && scopes.Contains(selectedNode.Name))
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SS_JobSkip");
                return jobId;
            }

            var enableSuperPriorityJobQueue = bool.TryParse(RMKeyValueDao.GetValueByKey(RMKeyValuesConstants.ENABLE_SUPER_PRIORITY_JOB_QUEUE)?.Value, out var enableSuperQueue) && enableSuperQueue;
            string superJobQueueName = null;
            if (enableSuperPriorityJobQueue)
            {
                superJobQueueName = RMKeyValueDao.GetValueByKey(RMKeyValuesConstants.SUPER_PRIORITY_JOB_QUEUE_NAME)?.Value;
                if (string.IsNullOrEmpty(superJobQueueName))
                {
                    superJobQueueName = RMGlobalConfiguration.AppConfig[Contract.Configurations.RMAppSettingKey.HIGHEST_PRIORITY_JOB_QUEUE_NAME];
                    if (string.IsNullOrEmpty(superJobQueueName))
                    {
                        logger.Error($"Enable highest job queue, but not config for it");
                        RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_JS_DAM_RunJob_Failed");
                        return jobId;
                    }
                }
                else
                {
                    logger.Info($"Custom highest job queue name: {superJobQueueName}");
                }
            }

            int currentSubjobIndex = 0;
            int subJobCount = 1;
            string subJobId = CreateSubJobForDisposal(jobId, currentSubjobIndex, jobType, subJobCount, param, true, restoreScope, O365TenantId);
            if (currentSubjobIndex < subJobCountInConfigFile)  //一次只发两个子job, 后续在JobInfoUpdater中触发
            {
                if (enableSuperPriorityJobQueue)
                {
                    logger.Info($"Start to send {subJobId} to highest job queue {superJobQueueName}");
                    JobQueueService.HandleCustomerMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Control,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType, subJobId),
                    }, superJobQueueName);
                }
                else
                {
                    JobQueueService.HandleO365Message(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Control,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType, subJobId),
                    });
                }
            }
            logger.Info("Job has start running");
            return jobId;
        }

        private NodeLevel ConvertTeamsNodeLevel(TreeNodeLevel tLevel)
        {
            NodeLevel nodeLevel = NodeLevel.Office365GroupEntire;
            switch (tLevel)
            {
                case TreeNodeLevel.ExchangeOnlineMailbox:
                    nodeLevel = NodeLevel.Office365GroupEntire;
                    break;
                    // channel, plan,...
                default:
                    break;
            }
            return nodeLevel;
        }

        public string GetO365TeamsTenantId(string teamsAddress)
        {
            string tenantId = string.Empty;
            try
            {
                var teamsNode = RemoteNodeService.GetTeamsNodeByTeamsAddress(teamsAddress);
                if (!string.IsNullOrEmpty(teamsAddress) && teamsNode != null)
                {
                    logger.Info($"Get tenant id from teams success,teamsAddress:{teamsAddress},tenant id:{teamsNode.TenantId}");
                    tenantId = teamsNode.TenantId;
                }
                else
                {
                    logger.Warn($"Get tenant id from teams failed,teamsAddress:{teamsAddress}");
                    var domainName = teamsAddress.Split("@")?.LastOrDefault();
                    tenantId = RMAosApiClient.GetO365TenantIdByFullDomain(domainName);
                }
            }
            catch (Exception e)
            {
                logger.Error($"Failed to get tenant id.e:{e}");
            }
            return tenantId;
        }

        private string GetRestoreScope(JobType jobType, RestoreSettingAndTree restoreSettingAndTree)
        {
            try
            {
                string fullPath;
                List<SPTreeNodeDto> selectedNodes = restoreSettingAndTree?.Tree;
                if (selectedNodes == null || selectedNodes.Count == 0)
                {
                    fullPath = restoreSettingAndTree.Setting.SiteUrl;
                }
                else
                {
                    SPTreeNodeDto selectedNode = selectedNodes?.FirstOrDefault();
                    fullPath = selectedNode.FullPath;
                }
                
                string restoreScope = string.Empty;
                if (jobType == JobType.StubOopRestore && !String.IsNullOrWhiteSpace(restoreSettingAndTree?.oopStubUrl))
                {
                    restoreScope = GenerateSiteCollecitonUrl(restoreSettingAndTree.oopStubUrl);
                }
                else if(jobType == JobType.ArchiverOutPlaceRestore && restoreSettingAndTree.IsEndUserJob)
                {
                    restoreScope = "RM_RS_RecenterDefaultLocation";
                }
                else if (jobType == JobType.ArchiverOutPlaceRestore && !String.IsNullOrWhiteSpace(restoreSettingAndTree?.Setting?.StorageDeviceDto?.Name))
                {
                    restoreScope = restoreSettingAndTree?.Setting?.StorageDeviceDto?.Name;
                }
                else if ((jobType == JobType.ArchiverRestore || jobType == JobType.AOSPRestore) && !String.IsNullOrWhiteSpace(fullPath))
                {
                    restoreScope = GenerateSiteCollecitonUrl(fullPath);
                    if (!string.IsNullOrWhiteSpace(restoreScope))
                    {
                        RMRestoreSiteMapping siteMappingInfo = RMRestoreSiteMappingDao.GetMappingBySourceSiteUrl(restoreScope);
                        if (siteMappingInfo != null)
                        {
                            restoreScope = siteMappingInfo.TargetSiteUrl;
                        }
                    }
                }
                else if (jobType == JobType.ArchiverToSpoRestore)
                {
                    //restoreScope = GenerateSiteCollecitonUrl(fullPath);
                    if (restoreSettingAndTree?.Setting?.DestDto == null)
                    {
                        logger.Warn($"DestDto is null for ArchiverToSpoRestore job, nodePath: {fullPath}. Fallback to use fullPath from selected node");
                        restoreScope = GenerateSiteCollecitonUrl(fullPath);
                    }
                    else
                    {
                        var tempPath = string.IsNullOrWhiteSpace(restoreSettingAndTree.Setting.DestDto.FolderPath)
                        ? restoreSettingAndTree.Setting.DestDto.ListPath
                        : restoreSettingAndTree.Setting.DestDto.FolderPath;
                        restoreScope = WebUtil.MakeFullUrl(restoreSettingAndTree.Setting.DestDto.SiteCollectionUrl, tempPath);
                    }
                }
                else if (jobType == JobType.StubArchiverRestore || jobType == JobType.M365InPlaceArchiverRestore)
                {
                    if (restoreSettingAndTree?.Setting?.DestDto == null)
                    {
                        logger.Warn($"DestDto is null for StubArchiverRestore or M365InPlaceArchiverRestore job, nodePath: {fullPath}. Fallback to use fullPath from selected node");
                        restoreScope = GenerateSiteCollecitonUrl(fullPath);
                    }
                    else
                    {
                        var tempPath = string.IsNullOrWhiteSpace(restoreSettingAndTree.Setting.DestDto.FolderPath)
                        ? restoreSettingAndTree.Setting.DestDto.ListPath
                        : restoreSettingAndTree.Setting.DestDto.FolderPath;
                        if (string.IsNullOrEmpty(tempPath))
                        {
                            restoreScope = restoreSettingAndTree.Setting.DestDto.FullUrl;
                        }
                        else
                        {
                            restoreScope = WebUtil.MakeFullUrl(restoreSettingAndTree.Setting.DestDto.SiteCollectionUrl, tempPath);
                        }
                    }
                }

                if (string.IsNullOrWhiteSpace(restoreScope))
                {
                    if (restoreSettingAndTree == null)
                    {
                        logger.Warn($@"Dev test log,Unable parse resotre Scope,tree is null");
                    }
                    else
                    {
                        logger.Warn($@"Dev test log,Unable parse resotre Scope,tree: {SerializerHelper.SerializeByJsonConvert(restoreSettingAndTree)}");
                    }
                }
                return restoreScope;
            }
            catch(Exception e)
            {
                logger.Error(@$"Fail get restore scope,ex: {e}");
            }
            return string.Empty;
        }

        private string GenerateSiteCollecitonUrl(string OopStubUrl)
        {
            string result = string.Empty;
            if (string.IsNullOrWhiteSpace(OopStubUrl))
            {
                return result;
            }
            var path = OopStubUrl.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (path.Length > 3 && path[0].StartsWith("https"))
            {
                if (path[2].Equals("sites", StringComparison.OrdinalIgnoreCase) || path[2].Equals("personal", StringComparison.OrdinalIgnoreCase) || path[2].Equals("teams", StringComparison.OrdinalIgnoreCase))
                {
                    result = path[0] + "//" + path[1] + "/" + path[2] + "/" + path[3];
                }
                else
                {
                    result = path[0] + "//" + path[1];
                }
            }
            else if(path.Length == 2 && path[0].StartsWith("https"))
            {
                result = OopStubUrl;
            }
            return result;
        }

        public string GetO365TenantId(string siteUrl)
        {
            string tenantId = string.Empty;
            try
            {
                var siteCollection = RemoteNodeService.GetRemoteSiteCollectionByUrl(siteUrl);
                if (!string.IsNullOrEmpty(siteUrl) && siteCollection!=null)
                {
                    logger.Info($"Get tenant id from site collection success,site url:{siteUrl},tenant id:{siteCollection.TenantId}");
                    tenantId = siteCollection.TenantId;
                }
                else
                {
                    logger.Warn($"Get tenant id from site collection failed,site url:{siteUrl}");
                    var profiles = RMAosApiClient.GetHasADPermissionProfiles(TenantLocalValue.LogonGroupId);
                    foreach (var temp in profiles)
                    {
                        if (siteUrl.Substring("https://".Length, temp.DomainName.Length).StartsWith(temp.DomainName, StringComparison.OrdinalIgnoreCase))
                        {
                            tenantId = temp.TenantId;
                            break;
                        }
                    }
                }
            }
            catch(Exception e)
            {
                logger.Error($"Failed to get tenant id.e:{e}");
            }
            return tenantId;
        }
        public string RealRunSimulateArchiverRestoreJob(JobRunBy jobRunBy, string jobRunByUser, string param ,string tenantGroupId)
        {
            logger.Info("Start real run simulate archiver restore job");
            
            List<RMSubJob> runningJobs = SubJobDao.GetRunningAndRunnableSubJobListAsync(JobType.SimulateRestore).GetAwaiter().GetResult();
            foreach (var runningJob in runningJobs)
            {
                if (runningJob.String1 == jobRunByUser)
                {
                    if(runningJob.Status == (int)JobStatus.Wait)
                    {
                        SubJobDao.UpdateStatus(runningJob.Id, (int)JobStatus.Stopped, DateTime.UtcNow.Ticks);
                    }
                    else
                    {
                        SubJobDao.UpdateStatus(runningJob.Id, (int)JobStatus.Stopping, DateTime.UtcNow.Ticks);
                    }
                }
            }

            RestoreSettingAndTree restoreSettingAndTree = SerializerHelper.DeserializeByJsonConvert<RestoreSettingAndTree>(param);
            string subJobId = restoreSettingAndTree.JobId;
            CreateSubJob(subJobId, null, JobType.SimulateRestore, 1, param, true, jobRunByUser, null);

            JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = subJobId,
                        RunBy = JobRunBy.Control,
                        JobType = JobType.SimulateRestore,
                        JobTenantInfo = new JobTenantInfo 
                        { 
                            TenantId = tenantGroupId, 
                            RegisterEmail = jobRunByUser
                        },
                        CommandLine = string.Format("{0} {1}", JobType.SimulateRestore, subJobId),
                    });
            logger.Info("Job has start running");
            return subJobId;
        }

        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.ImportRestoreSiteMapping, BeforeHandler = typeof(ArchiverSettingsBeforeAuditHandler), AfterHandler = typeof(ArchiverSettingsAfterAuditHandler))]
        public string RealRunImportSCMappingJob(string jobRunByUser, string filePath)
        {
            logger.Info("Start real run import sc mapping job");
            List<string> importJobs = JobMonitorService.GetRunningJobs(JobType.ImportSCMapping);
            string id = JobMonitorService.CreateJob(JobType.ImportSCMapping, jobRunByUser);
            if (importJobs.Count > 0)
            {
                JobMonitorService.UpdateJobStatus(id, JobStatus.Skipped, "RM_ImportTerm_JobSkip");
            }
            else
            {
                JobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = id,
                    RunBy = JobRunBy.Control,
                    JobType = JobType.ImportSCMapping,
                    CommandLine = string.Format("{0} {1} {2}", JobType.ImportSCMapping, id, filePath),
                });
            }
            return id;
        }
        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.ExportRestoreSiteMapping, BeforeHandler = typeof(ArchiverSettingsBeforeAuditHandler), AfterHandler = typeof(ArchiverSettingsAfterAuditHandler))]

        public string RealRunExportSCMappingJob(string jobRunByUser)
        {
            logger.Info("Start real run export sc mapping job");
            string id = JobMonitorService.CreateJob(JobType.ExportSCMapping, jobRunByUser);
            var account = AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail).GetAwaiter().GetResult();
            DownloadDataInfoDao.Create(new RMDownloadDataInfo()
            {
                FileDownloadTime = DateTime.UtcNow.Ticks,
                JobId = id,
                RecordsId = Guid.NewGuid(),
                JobStatus = (int)DownloadContentJobStatus.Wait,
                UserId = account.UserId,
                Name = id + ".zip",
                DownloadType = DownloadContentType.ExportSCMapping,
            });

            JobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = id,
                    RunBy = JobRunBy.Control,
                    JobType = JobType.ExportSCMapping,
                    CommandLine = string.Format("{0} {1}", JobType.ExportSCMapping, id)
                });
            
            return id;
        }


        public string RealRunExportSearchResultJob(JobRunBy jobRunBy, string jobRunByUser, string param, JobType tempJobType)
        {
            logger.Info("Start real run export search result job");
            DocAveOnline.WebApi.Contracts.EndUserRestoreConfig ExportSetting = SerializerHelper.DeserializeByDataContractSerializer<DocAveOnline.WebApi.Contracts.EndUserRestoreConfig>(param);
            JobType jobType = tempJobType;
            string jobId = ExportSetting.JobId;

            logger.Info("this is enduser export search result job");
            RMJobService.CreateJobWithScopeIdForRecenter(tempJobType, jobRunByUser, "", jobId, 0, ExportSetting.RunJobUser);
            logger.Info($"Create export search result job success,job id:{jobId}");
            List<JobType> indexJobTypes = new List<JobType>() { JobType.ArchiverMoveIndex };
            var mIndexJobs = RMJobService.GetRunningJobs(indexJobTypes);

            if (mIndexJobs.Count > 0)
            {
                //has move index job, need skip.
                logger.Warn("Current has move index job running.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }
            int currentSubjobIndex = 0;
            int subJobCount = 1;
            string subJobId = CreateSubJobForDisposal(jobId, currentSubjobIndex, jobType, subJobCount, param, true, "");

            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = subJobId,
                RunBy = JobRunBy.Control,
                JobType = jobType,
                CommandLine = string.Format("{0} {1}", jobType, subJobId),
            });
            logger.Info("Job has start running");
            return jobId;
        }
        public string RealRunRestoreCenterExportSearchResultJob(JobRunBy jobRunBy, string jobRunByUser, string param, JobType tempJobType)
        {
            logger.Info("Start real run export search result job");
            JobType jobType = tempJobType;
            logger.Info("this is admin export search result job");
            var account = AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail).GetAwaiter().GetResult();
            var jobId = RMJobService.CreateJobWithScopeId(tempJobType, jobRunByUser, "", account?.UserId);
            DownloadDataInfoDao.CreateDownloadDataInfo(new RMDownloadDataInfo()
            {
                FileDownloadTime = DateTime.UtcNow.Ticks,
                JobId = jobId,
                RecordsId = Guid.NewGuid(),
                JobStatus = (int)DownloadContentJobStatus.Wait,
                UserId = account.UserId,
                Name = jobId + ".zip",
                DownloadType = DownloadContentType.ExportRestoreCenterSeachResult,
            });
            List<JobType> indexJobTypes = new List<JobType>() { JobType.ArchiverMoveIndex };
            var mIndexJobs = RMJobService.GetRunningJobs(indexJobTypes);

            if (mIndexJobs.Count > 0)
            {
                //has move index job, need skip.
                logger.Warn("Current has move index job running.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }
            int currentSubjobIndex = 0;
            int subJobCount = 1;
            string subJobId = CreateSubJobForDisposal(jobId, currentSubjobIndex, jobType, subJobCount, param, true, "");

            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = subJobId,
                RunBy = JobRunBy.Control,
                JobType = jobType,
                CommandLine = string.Format("{0} {1}", jobType, subJobId),
            });
            logger.Info("Job has start running");
            return jobId;
        }
        public RAReturnMessage ExportSearchResult(ArchiverRestoreResult searchContract)
        {
            logger.Info($"start run export search result job,mode:{searchContract.SearchMode}");

            var searchNode = searchContract.SerchContract?.SearchNode;
            var siteUrl = searchNode?.SiteUrl;

            if (string.IsNullOrWhiteSpace(siteUrl))
            {
                logger.Warn("ExportSearchResult failed because site url is empty.");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed };
            }

            if (ValidSiteCollectionsPermission(new[] { siteUrl }).Any(res => res.permission != FunctionSubPermission.RestoreCenterFullControl))
            {
                logger.Warn($"ExportSearchResult permission denied, site url:{siteUrl}, user:{TenantLocalValue.LogonUserId}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed };
            }

            JobQueueDto jqDto = new JobQueueDto()
            {
                JobType = JobType.ExportRestoreCenterSeachResult,
                TenantGroupId = TenantLocalValue.LogonGroupId,
                JobRunByUser = TenantLocalValue.LogonUserEmail,
                JobRunType = JobRunBy.Control,
                Parameters = SerializerHelper.SerializeByJsonSerializer(searchContract),
            };
            JobQueueService.AddToDBJobQueue(jqDto);
            return new RAReturnMessage();
        }

        private bool CheckRestoreLimit()
        {
            bool canRunRestoreJob = true;
            try
            {
                var client = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId);
                var info = client.LicenseService.GetLicenseAsync(RecordsConstants.RECORDS_APPLICATION_NAME).GetAwaiter().GetResult();
                if (info.Extension is Cloud.Sdk.Data.AosModern.CloudRecordsExtension)
                {
                    Cloud.Sdk.Data.AosModern.CloudRecordsExtension extension = info.Extension as Cloud.Sdk.Data.AosModern.CloudRecordsExtension;
                    if (extension.SaleType == Cloud.Sdk.Data.AosModern.SaleType.PrePaidConsumption)
                    {
                        int restoreSize = extension.ConsumedRestoreCapacity;
                        int purchasedAvepointStorageCapacity = extension.PurchasedAvepointStorageCapacity;
                        bool isReachedRestoreSizeLimit = restoreSize > purchasedAvepointStorageCapacity * 0.2;
                        if (IsCancelRestoreSizeLimit() || !isReachedRestoreSizeLimit)
                        {
                            logger.Info($"purchasedAvepointStorageCapacity size is {purchasedAvepointStorageCapacity}gb,current month has restored size:{restoreSize}gb");
                        }
                        else
                        {
                            logger.Info($"reached restore size limit, size is {purchasedAvepointStorageCapacity}gb,current month has restored size:{restoreSize}gb");
                            canRunRestoreJob = false;
                        }
                    }
                    else
                    {
                        logger.Info($"not prepaid license so permit restore,license type is:{extension.SaleType}");
                    }
                }
            }
            catch(Exception e)
            {
                logger.Error($"check is run restore job failed!,error:{e}");
            }
            return canRunRestoreJob;
        }
        private bool IsCancelRestoreSizeLimit()
        {
            var key = RMKeyValueDao.GetValueByKey("IsCancelRestoreSizeLimit");
            bool.TryParse(key?.Value, out bool result);
            return result;
        }
        private int CaculateGBSize(List<RMJobSizeAndCountStatistics> sizeList)
        {
            long realSize = 0;
            int sizeOfGB = 0;
            foreach (var temp in sizeList)
            {
                realSize += temp.Size;
            }
            sizeOfGB = (int)(realSize / (1024 * 1024 * 1024));
            return sizeOfGB;
        }
        private string CreateSubJobForDisposal(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, string param, bool sendNow, string scope,string o365TenantId = "")
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount,O365TenantId = o365TenantId };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = param };
            subJob.String1 = scope;
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} Path {3}", subJob.Id, subJob.JobType, subJob.Weight, scope);
            return subJobId;
        }
        
        private string CreateSubJobForDisposal(string jobId, int currentSubjobIndex, JobType jobType, int subJobCount, string param, bool sendNow, string scope, string nodeId,string o365TenantId = "")
        {
            string subJobId = string.Format(jobId + "_{0:D3}", currentSubjobIndex);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount,O365TenantId = o365TenantId };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = param };
            subJob.String1 = scope;
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2}, NodeId {3}", subJob.Id, subJob.JobType, subJob.Weight, nodeId);
            return subJobId;
        }

        private string CreateSubJob(string subJobId, string mainJobId, JobType jobType, int subJobCount, string param, bool sendNow, string scope, string comment, JobStatus status = JobStatus.Wait)
        {
            var subJob = new RMSubJob() { Id = subJobId, ParentId = mainJobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d / subJobCount };
            subJob.Runable = sendNow ? RecordsConstants.SubJob_Runnable_Runing : RecordsConstants.SubJob_Runnable_Waiting;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = param };
            subJob.String1 = scope;
            subJob.Comment = comment;
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} Path {3}", subJob.Id, subJob.JobType, subJob.Weight, scope);
            return subJobId;
        }
        private string GetSPContainerId(SPTreeNodeDto selectedNode)
        {
            if (selectedNode.Level == NodeLevel.SiteCollection)
            {
                return selectedNode.SPObjectId;
            }
            else
            {
                return GetSPContainerId(selectedNode.Parent);
            }
        }
        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.SaveRestoreSiteMapping, BeforeHandler = typeof(ArchiverSettingsBeforeAuditHandler),AfterHandler = typeof(ArchiverSettingsAfterAuditHandler))]
        public RAReturnMessage AddSCMappings(List<SiteMappingInfo> siteMappings)
        {
            try
            {
                if (!TenantService.IsNewOpusTenant())
                {
                    logger.Error($"old logic account cann't use add sc mappings");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed };
                }
                logger.Info($"save restore site mapping,site mapping count is {siteMappings.Count}");

                #region check data
                if (siteMappings.Count > 10)
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_SiteMappings_OutOfSaveLimitCount") };
                }

                foreach(SiteMappingInfo siteMapping in siteMappings)
                {
                    siteMapping.SourceSiteUrl = siteMapping.SourceSiteUrl?.TrimEnd('/')?.TrimEnd('\\');
                    siteMapping.TargetSiteUrl = siteMapping.TargetSiteUrl?.TrimEnd('/')?.TrimEnd('\\');
                }

                if (RMRestoreSiteMappingDao.ExistMappingInSourcesSiteUrls(siteMappings.Select(mapping => mapping.SourceSiteUrl)))
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_SiteMappings_PartSourceAlreadyMapping") };
                }
                CheckSCMappings(siteMappings, out List<SiteMappingInfo> targetNotExistData, out List<SiteMappingInfo> notSameSourceData, out List<SiteMappingInfo> unKnowExceptionData, out List<SiteMappingInfo> validData, out Dictionary<string, List<SiteMappingInfo>> dedupData);
                if (dedupData.Count > 0)
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_SiteMappings_HaveDedupSourceUrl") };
                }
                if (targetNotExistData.Count > 0)
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_SiteMappings_ErrorMessage") };
                }
                if (notSameSourceData.Count > 0)
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_SiteMappings_NotSameSource") };
                }
                if (unKnowExceptionData.Count > 0)
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_SiteMappings_ErrorMessage") };
                }
                #endregion

                #region save
                List<RMRestoreSiteMapping> insertInfo = new List<RMRestoreSiteMapping>();
                int maxId = RMRestoreSiteMappingDao.GetLastMappingIntId();
                foreach (var temp in siteMappings)
                {
                    temp.Id = Guid.NewGuid().ToString();
                    insertInfo.Add(new RMRestoreSiteMapping()
                    {
                        Id = temp.Id,
                        SourceSiteUrl = temp.SourceSiteUrl,
                        TargetSiteUrl = temp.TargetSiteUrl,
                        intId = ++maxId,
                        SettingFlag = RestoreSettingFlag.SiteMapping,
                    });
                }
                RMRestoreSiteMappingDao.CreateByBulkCopyAsync(insertInfo).GetAwaiter().GetResult();
                logger.Info("save restore site mapping success");
                #endregion

                return new RAReturnMessage() { MessageType = RAMessageType.Successful, Extsion1 = siteMappings };

            }
            catch (Exception e)
            {
                logger.Error($"save restore site mapping failed,error:{e}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_SiteMappings_ErrorMessage") };
            }
        }

        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.RunArchiverRestoreGoogleDriveJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public string RealRunDriveArchiverRestoreJob(JobRunBy jobRunBy, string jobRunByUser, string param, JobType tempJobType)
        {
            logger.Info("Start real run drive archiver restore job");
            List<JobType> types = new List<JobType>() { tempJobType };
            GDriveRestoreSettingAndTree restoreDriveSettingAndTree = SerializerHelper.DeserializeByDataContractSerializer<GDriveRestoreSettingAndTree>(param);
            JobType jobType = tempJobType;
            string jobId = restoreDriveSettingAndTree.JobId;
            int subJobCountInConfigFile = RMKeyValueDao.GetSubJobCountFromDB((int)jobType);
            bool skipCheckRunningRestoreJob = RMKeyValueDao.HasSkipCheckRunningRestoreJob();
            List<GoogleDriveTreeNodeDto> selectedDriveNodes = restoreDriveSettingAndTree.Tree;
            GoogleDriveTreeNodeDto selectedNode = selectedDriveNodes.First();
            string restoreScope = GetDriveRestoreScope(jobType, restoreDriveSettingAndTree);

            if (!restoreDriveSettingAndTree.IsEndUserJob && RMJobService.HasRunningArchiverJobOnScope(types, restoreScope) && !skipCheckRunningRestoreJob)
            {
                logger.Warn($"Current has job running on same scope.{selectedNode.ID}");
                if (string.IsNullOrEmpty(jobId))
                {
                    jobId = RMJobService.CreateJobWithScopeId(tempJobType, jobRunByUser, restoreScope, restoreDriveSettingAndTree.SiteGroupId, restoreScope);
                }
                else
                {
                    TenantLocalValue.LogonUserEmail = jobRunByUser;
                    RMJobService.CreateJobWithScopeIdForRecenter(tempJobType, jobRunByUser, restoreScope, jobId, restoreDriveSettingAndTree.NodeType, restoreDriveSettingAndTree.RealRunJobUser, restoreDriveSettingAndTree.SiteGroupId);
                }
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }
            else
            {
                if (string.IsNullOrEmpty(jobId))
                {
                    jobId = RMJobService.CreateJobWithScopeId(tempJobType, jobRunByUser, restoreScope, restoreDriveSettingAndTree.SiteGroupId, restoreScope);
                }
                else
                {
                    logger.Info("this is enduser restore job");
                    TenantLocalValue.LogonUserEmail = jobRunByUser;
                    RMJobService.CreateJobWithScopeIdForRecenter(tempJobType, jobRunByUser, restoreScope, jobId, restoreDriveSettingAndTree.NodeType, restoreDriveSettingAndTree.RealRunJobUser, restoreDriveSettingAndTree.SiteGroupId);
                }
            }
            logger.Info($"Create job success,job id:{jobId}");
            //gogole restore don't support to check limit
            //bool canRunRestore = CheckRestoreLimit();
            //if (!canRunRestore)
            //{
            //    logger.Warn("Your month restore size is running out.");
            //    RMJobService.UpdateJobStatus(jobId, JobStatus.Failed, "RM_Job_RestoreSizeLimit");
            //    return jobId;
            //}
            List<JobType> indexJobTypes = new List<JobType>() { JobType.ArchiverMoveIndex };
            var mIndexJobs = RMJobService.GetRunningJobs(indexJobTypes);

            if (mIndexJobs.Count > 0)
            {
                logger.Warn("Current has move index job running.");
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_Job_ScheduledJobConflict");
                return jobId;
            }

            var scopes = RMJobService.GetRunningArchiverJobsScopes(types);
            if (!restoreDriveSettingAndTree.IsEndUserJob && scopes.Contains(selectedNode.Name))
            {
                RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_SS_JobSkip");
                return jobId;
            }
           
            int currentSubjobIndex = 0;
            int subJobCount = 1;
            string subJobId = CreateSubJobForDisposal(jobId, currentSubjobIndex, jobType, subJobCount, param, true, restoreScope, selectedNode.ID, string.Empty);
            if (currentSubjobIndex < subJobCountInConfigFile)
            {            
                JobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = subJobId,
                    RunBy = JobRunBy.Control,
                    JobType = jobType,
                    CommandLine = string.Format("{0} {1}", jobType, subJobId),
                });               
            }
            logger.Info("Job has start running");
            return jobId;
        }

        private string GetDriveRestoreScope(JobType jobType, GDriveRestoreSettingAndTree restoreDriveSettingAndTree)
        {
            try
            {
                List<GoogleDriveTreeNodeDto> selectedDriveNodes = restoreDriveSettingAndTree.Tree;
                GoogleDriveTreeNodeDto selectedNode = selectedDriveNodes.First();
                string restoreScope = string.Empty;          
                restoreScope = selectedNode?.FullPath;                                
                if (string.IsNullOrWhiteSpace(restoreScope))
                {
                    if (restoreDriveSettingAndTree == null)
                    {
                        logger.Warn($@"Dev test log,Unable parse restore Scope,tree is null");
                    }
                    else
                    {
                        logger.Warn($@"Dev test log,Unable parse restore Scope,tree: {SerializerHelper.SerializeByJsonConvert(restoreDriveSettingAndTree)}");
                    }
                }
                return restoreScope;
            }
            catch (Exception e)
            {
                logger.Error(@$"Fail get restore scope,ex: {e}");
            }
            return string.Empty;
        }



        public bool CheckSCMappings(List<SiteMappingInfo> sources, out List<SiteMappingInfo> targetNotExistData, out List<SiteMappingInfo> notSameSourceData, out List<SiteMappingInfo> unKnowExceptionData, out List<SiteMappingInfo> validData, out Dictionary<string, List<SiteMappingInfo>> dedupData)
        {
            #region check dudup data
            dedupData = sources.GroupBy(x => x.SourceSiteUrl).Where(group => group.Count() > 1).ToDictionary(group => group.Key, group => group.ToList());
            #endregion

            #region check data
            targetNotExistData = new List<SiteMappingInfo>();
            notSameSourceData = new List<SiteMappingInfo>();
            validData = new List<SiteMappingInfo>();
            unKnowExceptionData = new List<SiteMappingInfo>();

            HashSet<string> needCheckedUrls = sources.Select(mapping => mapping.TargetSiteUrl).ToHashSet();
            var desRemoteNodes = RMRemoteNode.GetRemoteSiteCollectionBySiteUrls(needCheckedUrls);
            bool disableCheckDestinationSiteInfo = DisableCheckDestinationSiteInfo();
            foreach (var siteMappingInfo in sources)
            {
                try
                {
                    if (!disableCheckDestinationSiteInfo && !desRemoteNodes.Exists(node => node.url == siteMappingInfo.TargetSiteUrl))
                    {
                        targetNotExistData.Add(siteMappingInfo);
                    }
                    else if (!CheckSourceFlasIsSame(desRemoteNodes, siteMappingInfo))
                    {
                        notSameSourceData.Add(siteMappingInfo);
                    }
                    else if (!dedupData.Keys.Contains(siteMappingInfo.SourceSiteUrl))
                    {
                        validData.Add(siteMappingInfo);
                    }
                }
                catch(Exception e)
                {
                    logger.Warn($"fail check site mapping info, ex:{e}");
                    unKnowExceptionData.Add(siteMappingInfo);
                }
                
            }
            #endregion

            return dedupData.Count == 0 && targetNotExistData.Count == 0 && notSameSourceData.Count == 0 && unKnowExceptionData.Count == 0;
        }


        private bool CheckSourceFlasIsSame(List<RemoteSiteCollection> remoteSiteCollections, SiteMappingInfo map)
        {
            return IsOnedrive(map.TargetSiteUrl) == IsOnedrive(map.SourceSiteUrl);
        }

        private bool IsOnedrive(string siteUrl)
        {
            var reg = new Regex(@"https://([^/]+?)-my\.(sharepoint[^/]*)(/.*)?");
            var matches = reg.Match(siteUrl);
            if (matches.Success)
            {
                logger.Info($"Current site is onedrive site. Url:[{siteUrl}]");
            }
            return matches.Success;
        }

        public RestoreSiteMappingInfo GetSCMappings(int page, int size)
        {
            RestoreSiteMappingInfo resultInfo = new RestoreSiteMappingInfo();
            try
            {
                if (!TenantService.IsNewOpusTenant())
                {
                    logger.Error($"old logic account cann't use get sc mappings");
                    return new RestoreSiteMappingInfo() { SiteMappings = new List<SiteMappingInfo>() };
                }

                var temp = RMRestoreSiteMappingDao.GetMappings(++page, size, out int total);//前台从0开始算
                if (total > 0)
                {
                    logger.Info($"get restore site mapping count is {temp.Count}");
                    resultInfo.SiteMappings = temp.Select(t => new SiteMappingInfo() { SourceSiteUrl = t.SourceSiteUrl, TargetSiteUrl = t.TargetSiteUrl , Id = t.Id}).ToList();
                    resultInfo.TotalCount = total;
                }
                else
                {
                    resultInfo.SiteMappings = new List<SiteMappingInfo>();
                }
                return resultInfo;
            }
            catch (Exception e)
            {
                logger.Error($"get restore site mapping failed,error:{e}");
                return new RestoreSiteMappingInfo() { SiteMappings = new List<SiteMappingInfo>() };
            }
        }


        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.DeleteRestoreSiteMapping, BeforeHandler = typeof(ArchiverSettingsBeforeAuditHandler), AfterHandler = typeof(ArchiverSettingsAfterAuditHandler))]
        public RAReturnMessage DeleteSCMappings(List<string> ids)
        {
            try
            {
                if (!TenantService.IsNewOpusTenant())
                {
                    logger.Error($"old logic account cann't use Delete sc mappings");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed };
                }

                RMRestoreSiteMappingDao.BatchDeleteMapping(ids.ToArray());
                return new RAReturnMessage() { MessageType = RAMessageType.Successful };
            }
            catch (Exception e)
            {
                logger.Error($"Fail delete restore site mappings, ids:{ids}, ex:{e}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed };
            }
        }

        public RAReturnMessage ImportSiteCollectionMapping(Stream xlsxFileStream)
        {
            try
            {
                if (!TenantService.IsNewOpusTenant())
                {
                    logger.Error($"old logic account cann't use import sc mappings");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed };
                }

                string fileName = JobReportUtility.ImportSCMappingFile + DateTime.Now.Ticks.ToString() + ".xlsx";
                var blobName = Path.Combine(JobReportUtility.GetTenantIdentity(), JobReportUtility.ImportSCMappingFolder, fileName);
                RAStorageUtil.UploadReportBlob(blobName, xlsxFileStream);
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ImportSCMapping,
                    Parameters = blobName,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail
                };
                JobQueueService.AddToDBJobQueue(jqDto);
                return new RAReturnMessage() { MessageType = RAMessageType.Successful };
            }
            catch(Exception e)
            {
                logger.Error($"Fail import site collection mapping ,ex:{e}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed };
            }
        }

        public RAReturnMessage ExportSiteCollectionMapping()
        {
            try
            {
                if (!TenantService.IsNewOpusTenant())
                {
                    logger.Error($"old logic account cann't use export sc mappings");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed };
                }
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = JobType.ExportSCMapping,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail
                };
                JobQueueService.AddToDBJobQueue(jqDto);
                return new RAReturnMessage() { MessageType = RAMessageType.Successful };
            }
            catch (Exception e)
            {
                logger.Error($"Fail export site collection mapping ,ex:{e}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed };
            }
        }

        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.SwitchFullTextIndexType, BeforeHandler = typeof(ArchiverSettingsBeforeAuditHandler), AfterHandler = typeof(ArchiverSettingsAfterAuditHandler))]
        public RAReturnMessage SwitchFullTextIndexType(SwitchFullTextIndexParam param)
        {
            if (param == null)
            {
                logger.Error("SwitchFullTextIndexType parameter is null");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = "Switch full text index type parameter is null." };
            }

            try
            {
                if (!TenantService.IsNewOpusTenant())
                {
                    logger.Error("old logic account can't use switch full text index type");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = "Current tenant is not supported for switching full text index type." };
                }

                if (!IsEnableFullTextIndexSearch())
                {
                    logger.Error("not enable full text index search");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = "Full text index search is not enabled." };
                }

                bool targetIsBlackList = param.Type == FullTextIndexType.BackList;

                if (KeyValueService.IsSCBlackListForEdiscovery() == targetIsBlackList)
                {
                    logger.Error($"unable operate Swtich SiteCollectionList,type:{param.Type}");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_FullTextIndex_UICacheTypeErrorMessage") };
                }                

                RMKeyValueDao.UpsertAsync(KeyNameCollection.IsSCBlackListForEdiscovery, targetIsBlackList.ToString()).GetAwaiter().GetResult();

                
                if (!param.CleanSCList)
                {
                    var sourceFlag = targetIsBlackList ? RestoreSettingFlag.SearchWhitelist : RestoreSettingFlag.SearchBlacklist;
                    var targetFlag = targetIsBlackList ? RestoreSettingFlag.SearchBlacklist : RestoreSettingFlag.SearchWhitelist;
                    RMRestoreSiteMappingDao.DeleteMappingsByFlag(targetFlag);
                    RMRestoreSiteMappingDao.ConvertFullTextIndexListType(sourceFlag, targetFlag);
                }
                else
                {
                    RMRestoreSiteMappingDao.DeleteMappingsByFlag(RestoreSettingFlag.SearchWhitelist);
                    RMRestoreSiteMappingDao.DeleteMappingsByFlag(RestoreSettingFlag.SearchBlacklist);
                }

                logger.Info($"Switch full text index type to {(targetIsBlackList ? "blacklist" : "whitelist")}, delete current data:{param.CleanSCList}");

                return new RAReturnMessage() { MessageType = RAMessageType.Successful };
            }
            catch (Exception ex)
            {
                logger.Error($"Fail switch full text index type, ex:{ex}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_OperateFullTextIndexListError") };
            }
        }

        #region blacklist
        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.SaveRestoreSiteBlacklist, BeforeHandler = typeof(ArchiverSettingsBeforeAuditHandler), AfterHandler = typeof(ArchiverSettingsAfterAuditHandler))]
        public RAReturnMessage AddSCBlacklist(List<WhitelistInfo> blacklist)
        {
            try
            {
                if (!TenantService.IsNewOpusTenant())
                {
                    logger.Error("old logic account can't use add sc blacklist");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed };
                }

                if (!IsEnableFullTextIndexSearch())
                {
                    logger.Error("not enable full text index search");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed };
                }

                if (!KeyValueService.IsSCBlackListForEdiscovery())
                {
                    logger.Error("unable operate AddSCBlacklist");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed , ErrorMessage = I18NEntity.GetString("RM_RS_FullTextIndex_UICacheTypeErrorMessage") };
                }

                logger.Info($"save restore blacklist, blacklist count is {blacklist?.Count ?? 0}");

                if (blacklist == null || blacklist.Count == 0)
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Successful, Extsion1 = new List<WhitelistInfo>() };
                }

                if (blacklist.Count > 10)
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_AR_RC_AddBlacklist_ErrorMsg") };
                }

                foreach(WhitelistInfo info in blacklist)
                {
                    info.SiteCollectionUrl = info.SiteCollectionUrl.Trim(' ', '/', '\\');
                }

                if (RMRestoreSiteMappingDao.ExistBlacklistInSiteUrls(blacklist.Select(mapping => mapping.SiteCollectionUrl)))
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_Blacklist_PartSiteAlreadyExist") };
                }

                if (!CheckSiteCollectionList(blacklist, true, out List<WhitelistInfo> notExistSites, out List<WhitelistInfo> validSites, out List<(WhitelistInfo, Exception)> unKnowExceptionSites, out List<string> dupSites))
                {
                    var errorMess = dupSites.Count > 0 ? "RM_RS_SiteWhitelist_HaveDupSiteUrl" : "RM_RS_Blacklist_ErrorMessage";
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString(errorMess) };
                }

                List<RMRestoreSiteMapping> insertInfo = new List<RMRestoreSiteMapping>();
                int maxId = RMRestoreSiteMappingDao.GetLastBlacklistIntId();
                foreach (var temp in validSites)
                {
                    temp.Id = Guid.NewGuid().ToString();
                    insertInfo.Add(new RMRestoreSiteMapping()
                    {
                        Id = temp.Id,
                        SourceSiteUrl = temp.SiteCollectionUrl,
                        intId = ++maxId,
                        SettingFlag = RestoreSettingFlag.SearchBlacklist,
                    });
                }
                if (insertInfo.Count > 0)
                {
                    RMRestoreSiteMappingDao.CreateByBulkCopyAsync(insertInfo).GetAwaiter().GetResult();
                }

                logger.Info("save restore site blacklist success");

                return new RAReturnMessage() { MessageType = RAMessageType.Successful, Extsion1 = validSites };
            }
            catch (Exception e)
            {
                logger.Error($"save restore site blacklist failed,error:{e}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_Blacklist_ErrorMessage") };
            }
        }

        public RestoreSearchWhitelistInfo GetSCBlacklist(int page, int size)
        {
            RestoreSearchWhitelistInfo resultInfo = new();
            try
            {
                if (!KeyValueService.IsSCBlackListForEdiscovery())
                {
                    logger.Error("unable operate GetSCBlacklist");
                    return new () { ErrorMessage = I18NEntity.GetString("RM_RS_FullTextIndex_UICacheTypeErrorMessage") };
                }

                var temp = RMRestoreSiteMappingDao.GetBlacklistByPage(++page, size, out int total);
                if (total > 0)
                {
                    logger.Info($"get restore site blacklist count is {temp.Count}");
                    resultInfo.SiteCollections = temp.Select(t => new WhitelistInfo() { SiteCollectionUrl = t.SourceSiteUrl, Id = t.Id }).ToList();
                    resultInfo.TotalCount = total;
                }
                else
                {
                    resultInfo.SiteCollections = new List<WhitelistInfo>();
                }
                return resultInfo;
            }
            catch (Exception e)
            {
                logger.Error($"get restore site blacklist failed,error:{e}");
                return new RestoreSearchWhitelistInfo() { SiteCollections = new List<WhitelistInfo>() };
            }
        }
        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.DeleteRestoreSiteBlacklist, BeforeHandler = typeof(ArchiverSettingsBeforeAuditHandler), AfterHandler = typeof(ArchiverSettingsAfterAuditHandler))]
        public RAReturnMessage DeleteSCBlacklist(List<string> ids)
        {
            try
            {
                if (!TenantService.IsNewOpusTenant())
                {
                    logger.Error("old logic account can't use delete sc blacklist");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed };
                }

                if (!IsEnableFullTextIndexSearch())
                {
                    logger.Error("not enable full text index search");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed };
                }

                if (!KeyValueService.IsSCBlackListForEdiscovery())
                {
                    logger.Error("unable operate delete SCBlacklist");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_FullTextIndex_UICacheTypeErrorMessage") };
                }

                RMRestoreSiteMappingDao.BatchDeleteBlacklist(ids?.ToArray() ?? Array.Empty<string>());
                return new RAReturnMessage() { MessageType = RAMessageType.Successful };
            }
            catch (Exception e)
            {
                logger.Error($"Fail delete restore site blacklist, ids: [{string.Join(';', ids ?? new List<string>())}], ex:{e}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_OperateFullTextIndexListError") };
            }
        }

        public RAReturnMessage ImportSCBlacklist(Stream xlsxFileStream)
        {
            return ImportSiteCollectionList(xlsxFileStream, JobType.ImportSCBlacklist);
        }

        #endregion

        #region whitelist

        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.SaveRestoreSiteWhitelist, BeforeHandler = typeof(ArchiverSettingsBeforeAuditHandler), AfterHandler = typeof(ArchiverSettingsAfterAuditHandler))]
        public RAReturnMessage AddSCWhitelist(List<WhitelistInfo> whitelist)
        {
            try
            {
                if (!TenantService.IsNewOpusTenant())
                {
                    logger.Error($"old logic account cann't use add sc whitelist");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed };
                }

                if (!IsEnableFullTextIndexSearch())
                {
                    logger.Error($"not enable full text index search");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed };
                }

                if (KeyValueService.IsSCBlackListForEdiscovery())
                {
                    logger.Error("unable operate AddSCwhitelist");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_FullTextIndex_UICacheTypeErrorMessage") };
                }

                logger.Info($"save restore whitelist, whitelist count is {whitelist.Count}");

                #region check data
                if (whitelist.Count > 10)
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_Whitelist_OutOfSaveLimitCount") };
                }

                foreach (WhitelistInfo info in whitelist)
                {
                    info.SiteCollectionUrl = info.SiteCollectionUrl.Trim(' ', '/', '\\');
                }

                if (RMRestoreSiteMappingDao.ExistWhitelistInSiteUrls(whitelist.Select(mapping => mapping.SiteCollectionUrl)))
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_Whitelist_PartSiteAlreadyExist") };
                }

                if (!CheckSiteCollectionList(whitelist, false, out List<WhitelistInfo> notExistSites, out List<WhitelistInfo> validSites, out List<(WhitelistInfo, Exception)> unKnowExceptionSites, out List<string> dupSites))
                {
                    var errorMess = dupSites.Count > 0 ? "RM_RS_SiteWhitelist_HaveDupSiteUrl" : "RM_RS_Whitelist_ErrorMessage";
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString(errorMess) };
                }

                #endregion

                #region save
                List<RMRestoreSiteMapping> insertInfo = new List<RMRestoreSiteMapping>();
                int maxId = RMRestoreSiteMappingDao.GetLastWhitelistIntId();
                foreach (var temp in validSites)
                {
                    temp.Id = Guid.NewGuid().ToString();
                    insertInfo.Add(new RMRestoreSiteMapping()
                    {
                        Id = temp.Id,
                        SourceSiteUrl = temp.SiteCollectionUrl,
                        intId = ++maxId,
                        SettingFlag = RestoreSettingFlag.SearchWhitelist,
                    });
                }
                RMRestoreSiteMappingDao.CreateByBulkCopyAsync(insertInfo).GetAwaiter().GetResult();
                logger.Info("save restore site whitelist success");
                #endregion

                return new RAReturnMessage() { MessageType = RAMessageType.Successful, Extsion1 = validSites };

            }
            catch (Exception e)
            {
                logger.Error($"save restore site whitelist failed,error:{e}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_Whitelist_ErrorMessage") };
            }
        }

        public RestoreSearchWhitelistInfo GetSCWhitelist(int page, int size)
        {
            RestoreSearchWhitelistInfo resultInfo = new();
            try
            {
                if (KeyValueService.IsSCBlackListForEdiscovery())
                {
                    logger.Error("unable operate GetSCwhitelist");
                    return new () { ErrorMessage = I18NEntity.GetString("RM_RS_FullTextIndex_UICacheTypeErrorMessage") };
                }

                var temp = RMRestoreSiteMappingDao.GetWhitelistByPage(++page, size, out int total);
                if (total > 0)
                {
                    logger.Info($"get restore site whitelist count is {temp.Count}");
                    resultInfo.SiteCollections = temp.Select(t => new WhitelistInfo() { SiteCollectionUrl = t.SourceSiteUrl, Id = t.Id }).ToList();
                    resultInfo.TotalCount = total;
                }
                else
                {
                    resultInfo.SiteCollections = [];
                }
                return resultInfo;
            }
            catch (Exception e)
            {
                logger.Error($"get restore site whitelist failed,error:{e}");
                return new RestoreSearchWhitelistInfo() { SiteCollections = [] };
            }
        }

        public bool CheckSiteCollectionList(List<WhitelistInfo> sites, bool isBlacklist, out List<WhitelistInfo> notExistSites, out List<WhitelistInfo> validSites, out List<(WhitelistInfo, Exception)> unKnowExceptionSites, out List<string> dupSites)
        {
            notExistSites = new List<WhitelistInfo>();
            validSites = new List<WhitelistInfo>();
            unKnowExceptionSites = new List<(WhitelistInfo, Exception)>();

            HashSet<string> needCheckedUrls = sites.Select(mapping => mapping.SiteCollectionUrl).ToHashSet(StringComparer.OrdinalIgnoreCase);
            dupSites = sites.GroupBy(x => x.SiteCollectionUrl).Where(group => group.Count() > 1).Select(group => group.Key).ToList();

            var existSites = new HashSet<string>(RMRemoteNode.GetRemoteSiteCollectionBySiteUrls(needCheckedUrls).Select(node => node.url)
                .Concat(ArchiverSiteMasterIndexDao.GetAllBackupSiteCollectionDistinctUrl()),StringComparer.OrdinalIgnoreCase);

            foreach (var siteMappingInfo in sites)
            {
                try
                {
                    if (!existSites.Contains(siteMappingInfo.SiteCollectionUrl))
                    {
                        notExistSites.Add(siteMappingInfo);
                        continue;
                    }
                    if (!dupSites.Contains(siteMappingInfo.SiteCollectionUrl))
                    {
                        validSites.Add(siteMappingInfo);
                        continue;
                    }
                }
                catch (Exception e)
                {
                    logger.Warn($"fail check site {isBlacklist} info, ex:{e}");
                    unKnowExceptionSites.Add((siteMappingInfo, e));
                }
            }

            return dupSites.Count == 0 && notExistSites.Count == 0 && unKnowExceptionSites.Count == 0;
        }

        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.DeleteRestoreSiteWhitelist, BeforeHandler = typeof(ArchiverSettingsBeforeAuditHandler), AfterHandler = typeof(ArchiverSettingsAfterAuditHandler))]
        public RAReturnMessage DeleteSCWhitelist(List<string> ids)
        {
            try
            {
                if (!TenantService.IsNewOpusTenant())
                {
                    logger.Error($"old logic account cann't use Delete sc whitelist");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed };
                }

                if (!IsEnableFullTextIndexSearch())
                {
                    logger.Error($"not enable full text index search");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed };
                }

                if (KeyValueService.IsSCBlackListForEdiscovery())
                {
                    logger.Error("unable operate delete SCwhitelist");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_FullTextIndex_UICacheTypeErrorMessage") };
                }

                RMRestoreSiteMappingDao.BatchDeleteWhitelist(ids.ToArray());
                return new RAReturnMessage() { MessageType = RAMessageType.Successful };
            }
            catch (Exception e)
            {
                logger.Error($"Fail delete restore site whitelist, ids: [{string.Join(';', ids)}], ex:{e}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_OperateFullTextIndexListError") };
            }
        }
        
        public RAReturnMessage ImportSCWhitelist(Stream xlsxFileStream)
        {
            return ImportSiteCollectionList(xlsxFileStream, JobType.ImportSCWhitelist);
        }

        public RAReturnMessage ExportSCWhitelist()
        {
            return ExportSiteCollectionList(JobType.ExportSCWhitelist);
        }

        public RAReturnMessage ExportSCBlacklist()
        {
            return ExportSiteCollectionList(JobType.ExportSCBlacklist);
        }

        private RAReturnMessage ImportSiteCollectionList(Stream xlsxFileStream, JobType jobType)
        {
            var listTypeName = jobType == JobType.ImportSCWhitelist ? "whitelist" : "blacklist";
            try
            {
                if (!TenantService.IsNewOpusTenant())
                {
                    logger.Error($"old logic account can't use import sc {listTypeName}");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed };
                }

                if (!IsEnableFullTextIndexSearch())
                {
                    logger.Error($"not enable full text index search");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed };
                }

                if (KeyValueService.IsSCBlackListForEdiscovery() != (jobType == JobType.ImportSCBlacklist))
                {
                    logger.Error($"unable operate ImportSiteCollectionList,type:{jobType}");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_FullTextIndex_UICacheTypeErrorMessage") };
                }

                var filePrefix = jobType == JobType.ImportSCWhitelist
                    ? JobReportUtility.ImportSCWhitelistFile
                    : JobReportUtility.ImportSCBlacklistFile;
                var folder = jobType == JobType.ImportSCWhitelist
                    ? JobReportUtility.ImportSCWhitelistFolder
                    : JobReportUtility.ImportSCBlacklistFolder;

                string fileName = filePrefix + DateTime.Now.Ticks.ToString() + ".xlsx";
                var blobName = Path.Combine(JobReportUtility.GetTenantIdentity(), folder, fileName);
                RAStorageUtil.UploadReportBlob(blobName, xlsxFileStream);
                var jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    Parameters = blobName,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail
                };
                JobQueueService.AddToDBJobQueue(jqDto);
                return new RAReturnMessage() { MessageType = RAMessageType.Successful };
            }
            catch (Exception e)
            {
                logger.Error($"Fail import site collection {listTypeName} ,ex:{e}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_OperateFullTextIndexListError") };
            }
        }

        private string RealRunImportSiteCollectionJob(JobType jobType, string jobRunByUser, string filePath)
        {
            var listTypeName = jobType == JobType.ImportSCWhitelist ? "whitelist" : "blacklist";
            logger.Info($"Start real run import sc {listTypeName} job");
            var importJobs = JobMonitorService.GetRunningJobs(new List<JobType> { JobType.ImportSCWhitelist, JobType.ImportSCBlacklist});
            string id = JobMonitorService.CreateJob(jobType, jobRunByUser);
            if (importJobs.Count > 0)
            {
                JobMonitorService.UpdateJobStatus(id, JobStatus.Skipped, "RM_ImportWhitelist_JobSkip");
            }
            else
            {
                JobQueueService.HandleMessage(new JobQueueMessage()
                {
                    JobId = id,
                    RunBy = JobRunBy.Control,
                    JobType = jobType,
                    CommandLine = string.Format("{0} {1} {2}", jobType, id, filePath),
                });
            }

            return id;
        }

        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.ImportRestoreSiteWhitelist, BeforeHandler = typeof(ArchiverSettingsBeforeAuditHandler), AfterHandler = typeof(ArchiverSettingsAfterAuditHandler))]
        public string RealRunImportSCWhitelistJob(string jobRunByUser, string filePath)
        {
            return RealRunImportSiteCollectionJob(JobType.ImportSCWhitelist, jobRunByUser, filePath);
        }

        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.ImportRestoreSiteBlacklist, BeforeHandler = typeof(ArchiverSettingsBeforeAuditHandler), AfterHandler = typeof(ArchiverSettingsAfterAuditHandler))]
        public string RealRunImportSCBlacklistJob(string jobRunByUser, string filePath)
        {
            return RealRunImportSiteCollectionJob(JobType.ImportSCBlacklist, jobRunByUser, filePath);
        }

        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.ExportRestoreSiteWhitelist, BeforeHandler = typeof(ArchiverSettingsBeforeAuditHandler), AfterHandler = typeof(ArchiverSettingsAfterAuditHandler))]
        public string RealRunExportSCWhitelistJob(string jobRunByUser)
        {
            return RealRunExportSiteCollectionJob(JobType.ExportSCWhitelist, jobRunByUser);
        }

        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.ExportRestoreSiteBlacklist, BeforeHandler = typeof(ArchiverSettingsBeforeAuditHandler), AfterHandler = typeof(ArchiverSettingsAfterAuditHandler))]
        public string RealRunExportSCBlacklistJob(string jobRunByUser)
        {
            return RealRunExportSiteCollectionJob(JobType.ExportSCBlacklist, jobRunByUser);
        }

        public async Task<bool> CheckWhiteListForGroupsTeamsAsync(string group)
        {
            bool res = false;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                var sites = RemoteNodeService.GetRemoteSiteCollectionByParam(new List<string> { group }, false);
                res = await RealCheckSCEnableFullTextIndex(sites?.FirstOrDefault()?.url);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while Check Group Enable Full Text Index. Error: {e}");
                res = false;
            }
            finally
            {
                stopwatch.Stop();
                logger.Info(@$"Check Group Enable Full Text Index custom {stopwatch.ElapsedMilliseconds} milliseconds, Tenant id:{TenantLocalValue.LogonGroupId}, scope:{group}, res:{res}");
            }
            return res;
        }

        private async Task<bool> RealCheckSCEnableFullTextIndex(string scUrl)
        {
            if (string.IsNullOrWhiteSpace(scUrl))
            {
                throw new Exception(@$"SC URL is empty when check sc enable full text index");
            }

            scUrl = scUrl?.Trim();

            if (!IsEnableFullTextIndexSearch())
            {
                logger.Error("Current user unable use full text index");
                return false;
            }

            if (KeyValueService.IsSCBlackListForEdiscovery())
            {
                return !RMRestoreSiteMappingDao.ExistBlacklistInSiteUrls([scUrl]);
            }
            else
            {
                return RMRestoreSiteMappingDao.ExistWhitelistInSiteUrls([scUrl]);
            }
        }

        private RAReturnMessage ExportSiteCollectionList(JobType jobType)
        {
            var listTypeName = jobType == JobType.ExportSCWhitelist ? "whitelist" : "blacklist";
            try
            {
                if (!TenantService.IsNewOpusTenant())
                {
                    logger.Error($"old logic account can't use export sc {listTypeName}");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed };
                }

                if (!IsEnableFullTextIndexSearch())
                {
                    logger.Error($"not enable full text index search");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed };
                }

                var jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail
                };
                JobQueueService.AddToDBJobQueue(jqDto);
                return new RAReturnMessage() { MessageType = RAMessageType.Successful };
            }
            catch (Exception e)
            {
                logger.Error($"Fail export site collection {listTypeName} ,ex:{e}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_RS_OperateFullTextIndexListError") };
            }
        }

        private string RealRunExportSiteCollectionJob(JobType jobType, string jobRunByUser)
        {
            var listTypeName = jobType == JobType.ExportSCWhitelist ? "whitelist" : "blacklist";
            logger.Info($"Start real run export sc {listTypeName} job");
            string id = JobMonitorService.CreateJob(jobType, jobRunByUser);
            var account = AccountDao.GetActiveUserByNameAsync(TenantLocalValue.LogonUserEmail).GetAwaiter().GetResult();
            var downloadType = jobType == JobType.ExportSCWhitelist
                ? DownloadContentType.ExportSCWhitelist
                : DownloadContentType.ExportSCBlacklist;
            DownloadDataInfoDao.Create(new RMDownloadDataInfo()
            {
                FileDownloadTime = DateTime.UtcNow.Ticks,
                JobId = id,
                RecordsId = Guid.NewGuid(),
                JobStatus = (int)DownloadContentJobStatus.Wait,
                UserId = account.UserId,
                Name = id + ".zip",
                DownloadType = downloadType,
            });

            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = id,
                RunBy = JobRunBy.Control,
                JobType = jobType,
                CommandLine = string.Format("{0} {1}", jobType, id)
            });

            return id;
        }


        public async Task<bool> CheckWhiteListForSharePointSiteAsync(string scUrl)
        {
            bool res = false;
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                res = await RealCheckSCEnableFullTextIndex(scUrl);
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while Check SC Enable Full Text Index. Error: {e}");
                res = false;
            }
            finally
            {
                stopwatch.Stop();
                logger.Info(@$"Check SC Enable Full Text Index custom {stopwatch.ElapsedMilliseconds} milliseconds, Tenant id:{TenantLocalValue.LogonGroupId}, sc:{scUrl}, res:{res}");
            }
            return res;
        }


        public async Task<List<SiteCollectionNodesInfo>> GetAllSiteCollectionNodesByWhitelistAsync()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                var whitelist = RMRestoreSiteMappingDao.GetAllWhitelist().Select(wl => wl.SourceSiteUrl).ToHashSet(StringComparer.OrdinalIgnoreCase);
                if (whitelist.Count == 0)
                {
                    return new List<SiteCollectionNodesInfo>();
                }
                List<SiteCollectionNodesInfo> result = new List<SiteCollectionNodesInfo>();
                var restoreCenterPremission = await SecurityTrimmingHelper.GetUserRestoreCenterFunctionPermissionAsync();
                bool isOpusILAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOAdmin);
                bool isOpusSOAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.SPOAdmin);
                HashSet<string> fullPaths = new HashSet<string>();
                var index = ArchiverSiteMasterIndexDao.GetAllSiteCollectionNodsInfo();
                var permissionContainerIds = await GetPermissionContainerIdsAsync(RMBrowseTreeNodeSourceType.All);
                if (isOpusILAdmin || isOpusSOAdmin)
                {
                    result.AddRange(GetSiteCollectionNodes(fullPaths, index, whitelist));
                }
                else
                {
                    var tempPermission = await SecurityTrimmingHelper.GetUserRestoreCenterFunctionPermissionAsync();
                    if (tempPermission != FunctionSubPermission.None) //user has function permission
                    {
                        var tempAllResult = GetSiteCollectionNodes(fullPaths, index, whitelist);
                        var tempPermissionResult = GetSiteNodesThatHasPermission(index, permissionContainerIds, whitelist);
                        foreach (var temp in tempAllResult)
                        {
                            if (!tempPermissionResult.Exists(t => t.SiteUrl == temp.SiteUrl))
                            {
                                temp.PermissionLevel = (int)tempPermission;
                            }
                            result.Add(temp);
                        }
                    }
                    else
                    {
                        result.AddRange(GetSiteNodesThatHasPermission(index, permissionContainerIds, whitelist));
                    }

                }
                return result.OrderBy(r => r.SiteUrl, StringComparer.Ordinal).ToList();
            }
            catch(Exception ex) 
            {
                logger.Error($@"Fail Get All SiteCollection Nodes By Whitelist Async, ex:{ex}");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                logger.Info(@$"Get All SiteCollection Nodes By Whitelist Async custom {stopwatch.ElapsedMilliseconds} milliseconds, Tenant id:{TenantLocalValue.LogonGroupId}, user id:{TenantLocalValue.LogonUserId}");
            }
        }

        public async Task<List<SiteCollectionNodesInfo>> GetAllSiteCollectionNodesByBlacklistAsync()
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            try
            {
                var blacklist = RMRestoreSiteMappingDao.GetAllBlacklist()
                    .Select(bl => NormalizeSiteUrl(bl.SourceSiteUrl))
                    .Where(url => !string.IsNullOrWhiteSpace(url))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                bool IsBlacklisted(string siteUrl) => blacklist.Contains(NormalizeSiteUrl(siteUrl));

                List<SiteCollectionNodesInfo> result = new List<SiteCollectionNodesInfo>();
                var restoreCenterPremission = await SecurityTrimmingHelper.GetUserRestoreCenterFunctionPermissionAsync();
                bool isOpusILAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionMasks.SPOAdmin);
                bool isOpusSOAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMSOPermissionMasks.SPOAdmin);
                HashSet<string> fullPaths = new HashSet<string>();
                var index = ArchiverSiteMasterIndexDao.GetAllSiteCollectionNodsInfo(new List<int> { (int)SourceFlag.Google });
                var permissionContainerIds = await GetPermissionContainerIdsAsync(RMBrowseTreeNodeSourceType.All);

                if (isOpusILAdmin || isOpusSOAdmin)
                {
                    var allNodes = GetSiteCollectionNodes(fullPaths, index);
                    result.AddRange(allNodes.Where(node => !IsBlacklisted(node.SiteUrl)));
                }
                else
                {
                    if (restoreCenterPremission != FunctionSubPermission.None)
                    {
                        var tempAllResult = GetSiteCollectionNodes(fullPaths, index).Where(node => !IsBlacklisted(node.SiteUrl)).ToList();
                        var tempPermissionResult = GetSiteNodesThatHasPermission(index, permissionContainerIds)
                            .Where(node => !IsBlacklisted(node.SiteUrl))
                            .ToList();
                        foreach (var temp in tempAllResult)
                        {
                            if (!tempPermissionResult.Exists(t => t.SiteUrl == temp.SiteUrl))
                            {
                                temp.PermissionLevel = (int)restoreCenterPremission;
                            }
                            result.Add(temp);
                        }
                    }
                    else
                    {
                        result.AddRange(GetSiteNodesThatHasPermission(index, permissionContainerIds)
                            .Where(node => !IsBlacklisted(node.SiteUrl)));
                    }
                }

                return result.OrderBy(r => r.SiteUrl, StringComparer.Ordinal).ToList();
            }
            catch (Exception ex)
            {
                logger.Error(@$"Fail Get All SiteCollection Nodes By Blacklist Async, ex:{ex}");
                throw;
            }
            finally
            {
                stopwatch.Stop();
                logger.Info(@$"Get All SiteCollection Nodes By Blacklist Async custom {stopwatch.ElapsedMilliseconds} milliseconds, Tenant id:{TenantLocalValue.LogonGroupId}, user id:{TenantLocalValue.LogonUserId}");
            }
        }

        public async Task<List<SiteCollectionNodesInfo>> GetAllGoogleDriveNodesAsync()
        {
            try
            {
                logger.Info("GetAllGoogleDriveNodesAsync start.");
                List<SiteCollectionNodesInfo> result = new List<SiteCollectionNodesInfo>();
                bool isGoogleAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(RMPermissionExtensionMasks.GoogleAdmin);
                bool isGControlLicense = await TenantService.HasInitGControlPlatForm();
                HashSet<string> fullPaths = new HashSet<string>();

                List<DB.Model.ArchiverSiteMasterIndex> index = ArchiverSiteMasterIndexDao.GetAllGoogleNodesInfo();
                
                var permissionContainerIds = await GetPermissionContainerIdsAsync(RMBrowseTreeNodeSourceType.Google);
                logger.Info($"start GetAllGoogleDriveNodesAsync has permission fullpaths.");
                if (isGoogleAdmin || isGControlLicense)
                {
                    result.AddRange(GetGDriveNodes(fullPaths, index));
                }
                else
                {
                    var tempPermission = await SecurityTrimmingHelper.GetUserRestoreCenterFunctionPermissionAsync();
                    if (tempPermission != FunctionSubPermission.None)
                    {
                        var tempAllResult = GetGDriveNodes(fullPaths, index);
                        var tempPermissionResult = GetGDriveNodesThatHasPermission(index, permissionContainerIds);
                        foreach (var temp in tempAllResult)
                        {
                            if (!tempPermissionResult.Exists(t => t.SiteUrl == temp.SiteUrl))
                            {
                                temp.PermissionLevel = (int)tempPermission;
                            }
                            result.Add(temp);
                        }
                    }
                    else
                    {
                        result.AddRange(GetGDriveNodesThatHasPermission(index, permissionContainerIds));
                    }
                }
                return result.OrderBy(r => r.SiteUrl, StringComparer.Ordinal).ToList();
            }
            catch (Exception e)
            {
                logger.Error($"GetAllGoogleDriveNodesAsync failed,errror:{e}.");
                throw;
            }
        }

        public async Task<string> GetGDriveSearchTreeResultForJobAsync(List<ArchiverSiteMasterIndexContract> indexes, ArchiverRestoreResult filterPolicy, List<SiteCollectionNodesInfo> searchNodes)
        {
            var resultStr = string.Empty;
            try
            {
                var result = await HandleGDriveSearchCommonNodeForJobAsync(indexes, filterPolicy, searchNodes);
                resultStr = SerializerHelper.SerializeByDataContractSerializer(result);
            }
            catch (AveException ex)
            {
                logger.Error("Archiver Restore searching failed:", ex.ToString());
                throw;
            }
            catch (Exception ex)
            {
                logger.Error("Error occured while Archiver Restore searching:", ex.ToString());
            }
            return resultStr;
        }

        #endregion

        #region delete archive sc
        public bool IsEnableDeleteArchivedSiteCollection()
        {
            try
            {
                return RMKeyValueDao.IsDeleteArchivedSiteCollectionEnabled();
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while checking IsEnableDeleteArchivedSiteCollection, ERROR:{e}");
                return false;
            }
        }

        public RAReturnMessage RunDeleteArchivedSiteCollectionJob(SiteCollectionNodesInfo siteNodeInfo)
        {
            RAReturnMessage msg = new RAReturnMessage();
            try
            {
                ArgumentCheck.NotNull(siteNodeInfo, nameof(siteNodeInfo));
                if (!TenantService.IsNewOpusTenant())
                {
                    logger.Error($"old logic account can't run delete archived site collection {siteNodeInfo.SiteUrl}");
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed };
                }

                if (!RMKeyValueDao.IsDeleteArchivedSiteCollectionEnabled() || siteNodeInfo == null)
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed };
                }
                logger.Info($"Start archiver delete archived job.");
                string id = string.Empty;

                JobQueueDto jqDto = new()
                {
                    JobType = JobType.DeleteArchivedSiteCollection,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = TenantLocalValue.LogonGroupId,
                    JobRunByUser = TenantLocalValue.LogonUserEmail,
                    Parameters = SerializerHelper.SerializeByDataContractSerializer(siteNodeInfo)
                };

                id = JobQueueService.AddToDBJobQueue(jqDto);
                logger.Info($"RMArchiverSettingsService finished RunArchiverDeleteArchivedDataJob.JobType:{JobType.DeleteArchivedSiteCollection}.LogonGroupId:{TenantLocalValue.LogonGroupId}.RealRunJobUser:{TenantLocalValue.LogonUserId}.JobQueueMessageId:{id}.");
                if (string.IsNullOrEmpty(id))
                {
                    msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
                }
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while RunArchiverDeleteArchivedDataJob, ERROR:{ex}");
                msg = new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty };
            }
            return msg;
        }

        [Audit(Module = AuditModule.RestoreCenter, Category = AuditCategory.RestoreCenter, Action = AuditAction.RunDeleteArchivedSiteCollectionJob, AfterHandler = typeof(ArchiverJobAfterAuditHandler))]
        public string RealRunDeleteArchivedSiteCollectionJob(JobRunBy jobRunBy, string jobRunByUser, string param)
        {
            logger.Info($"Start RealRunArchiverDeleteArchivedSiteCollectionJob.");
            var siteNodeInfo = SerializerHelper.DeserializeByDataContractSerializer<SiteCollectionNodesInfo>(param);
            var scope = siteNodeInfo.SiteUrl;
            var jobType = JobType.DeleteArchivedSiteCollection;
            var jobId = string.Empty;

            var mIndexJobs = RMJobService.GetRunningJobs(JobTypeConstants.JobLevelConflictJobTypes);
            if (mIndexJobs.Count > 0)
            {
                logger.Warn("Current has move index or dedup job running, skip job DeleteArchivedSiteCollection.");
                jobId = RMJobService.CreateJobWithScopeId(jobType, JobStatus.Skipped, jobRunByUser, scope, null, "RM_Job_ScheduledJobConflict");
                return jobId;
            }

            // check job running same job type, same scope (site url),
            var runningJobs = RMJobService.GetRunningArchiverJobSiteUrl(JobTypeConstants.ArchiveSiteConflictType, [scope]);
            var canRunJob = ValidateRunningJobScopeForDASC(scope, runningJobs);
            if (!canRunJob)
            {
                logger.Warn("Current has job running job with the same scope, skip job DeleteArchivedSiteCollection..");
                jobId = RMJobService.CreateJobWithScopeId(jobType, JobStatus.Skipped, jobRunByUser, scope, null, "RM_Job_ScheduledJobConflict");
                return jobId;
            }

            List<JobType> sameScopeJobTypes = JobTypeConstants.ArchiveSiteConflictType;
            var scopes = RMJobService.GetRunningArchiverJobsScopes(sameScopeJobTypes);
            var hasAnyJobWithSameScope = scopes.Any(s => s.Contains(scope));
            if (hasAnyJobWithSameScope)
            {
                logger.Warn("Current has job running sub job with the same scope, skip job DeleteArchivedSiteCollection..");
                jobId = RMJobService.CreateJobWithScopeId(jobType, JobStatus.Skipped, jobRunByUser, scope, null, "RM_Job_ScheduledJobConflict");
                return jobId;
            }

            jobId = RMJobService.CreateJobWithScopeId(jobType, jobRunByUser, scope 
                , jobConflictExtension: GenerateArchiveJobMonitorExtensionForDASC([scope]));

            SubJobDao.UpdateSubJobCount(jobId, 1);

            var subJobId = CreateSubJobForDeleteArchivedSC(jobId, jobType, siteNodeInfo, scope);

            JobQueueService.HandleMessage(new JobQueueMessage()
            {
                JobId = subJobId,
                JobType = jobType,
                CommandLine = $"{jobType} {subJobId} {scope}",
                RunBy = jobRunBy,
            });

            return jobId;
        }
        private string CreateSubJobForDeleteArchivedSC(string jobId, JobType jobType, SiteCollectionNodesInfo siteInfo, string scope)
        {
            string subJobId = string.Format(jobId + "_{0:D3}", 0);
            var subJob = new RMSubJob() { Id = subJobId, ParentId = jobId, StartTime = DateTime.UtcNow.Ticks, JobType = (int)jobType, Progress = 0, Status = (int)JobStatus.Wait, Weight = 100d };
            subJob.Runable = RecordsConstants.SubJob_Runnable_Runing;
            subJob.JobContext = new RMJobContext() { JobId = subJobId, Settings = SerializerHelper.SerializeByDataContractSerializer(siteInfo) };
            subJob.String1 = scope;
            SubJobDao.CreateJob(subJob);
            logger.Info("Create sub job {0} sucessfull, type {1}, weight {2} , Path {3}", subJob.Id, subJob.JobType, subJob.Weight, scope);
            return subJobId;
        }

        private string GenerateArchiveJobMonitorExtensionForDASC(List<string> siteUrls, Contract.Object.TreeMode treeMode = Contract.Object.TreeMode.SO)
        {
            ArchiveJobMonitorExtension extension = new()
            {
                treeMode = treeMode,
                ConflictNodeLevel = ConflictNodeLevel.SiteCollection,
                IsGroupLevelArchive = false,
                SiteUrls = siteUrls
            };
            return SerializerHelper.SerializeByDataContractSerializer(extension);
        }

        private bool ValidateRunningJobScopeForDASC(string siteUrls, List<string> runningUrls)
        {
            foreach (var runningUrl in runningUrls)
            {
                if (RuleSPTreeUtil.IsPrefixWithSlash(runningUrl, siteUrls) || RuleSPTreeUtil.IsPrefixWithSlash(siteUrls, runningUrl))
                {
                    logger.Warn($"current scope :{siteUrls} has running job,so skip run subjob");
                    return false;
                }
            }
            return true;
        }
        #endregion
    }
}
