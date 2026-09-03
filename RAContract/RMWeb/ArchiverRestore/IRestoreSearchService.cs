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
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.SharePoint.RestoreJob.Restore.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.ArchiverRestore
{
    public interface IRestoreSearchService
    {
        bool IsEnableFullTextIndexSearch();
        bool ForceEnableFullTextIndexInBackend();
        bool CanSendFullTextIndexJobMessage();
        bool HasReachedIndexSizeLimitation();
        void SyncCategoryDataSize();
        Task<ArchiverRestoreResult> GetSearchTreeResultAsync(ArchiverRestoreResult searchContract, bool needCheckPermission = true);
        Task<ArchiverRestoreResult> GetDriveSearchTreeResultAsync(ArchiverRestoreResult searchContract, bool needCheckPermission = true, bool isControlPlus = false);
        Task<ArchiverRestoreResult> GetSearchTeamsTreeResultAsync(ArchiverRestoreResult searchContract, bool needCheckPermission = true);
        Task<ArchiverRestoreResult> GetFSSearchResultAsync(ArchiverRestoreResult searchContract);
        Task<string> GetSearchTreeResultForJobAsync(List<ArchiverSiteMasterIndexContract> indexes, ArchiverRestoreResult filterPolicy, List<SiteCollectionNodesInfo> searchNodes);
        Task<string> GetGDriveSearchTreeResultForJobAsync(List<ArchiverSiteMasterIndexContract> indexes, ArchiverRestoreResult filterPolicy, List<SiteCollectionNodesInfo> searchNodes);
        List<SPTreeNodeDto> GetSearchNodesFromMedia(List<ArchiverSiteMasterIndexContract> indexes, List<SPTreeNodeDto> searchNodes, RestoreSearchFilterPolicy filterPolicy);
        Task<List<SiteCollectionNodesInfo>> GetSiteCollectionNodesByUrlAsync(string siteUrl);
        Task<List<SiteCollectionNodesInfo>> GetAllSiteCollectionNodesAsync(string siteUrl = null);
        Task<ArchiverRestoreResult> GetAllSiteCollectionSerchResultAsync(ArchiverRestoreResult searchContract);
        Task<RAReturnMessage> SaveMultiSiteCollectionRestoreSettingAndRunAsync(RestoreInfo info, bool needCleanCache = false);
        RAReturnMessage SaveMultiSiteCollectionRestoreSettingAndRunInVirtualJob(SelectMultiScRestoreInfo info);
        bool UpdateMultiSiteCollectionRestoreRunLock();
        void ReleaseMultiSiteCollectionRestoreRunLock();
        Task<List<SiteCollectionNodesInfo>> GetEdiscoveryAllSiteCollectionNodesAsync(string siteUrl = null);
        Task<List<SiteCollectionNodesInfo>> GetAllConnectionNodesAsync();
        Task<List<SiteCollectionNodesInfo>> GetAllTeamsNodesAsync();
        Task<List<SiteCollectionNodesInfo>> GetAllGoogleDriveNodesAsync();
        RAReturnMessage SaveAndRunRestoreJob(RestoreInfo selectedTree,GCommon.Contract.StorageOptimization.Object.RestoreType restoreType, bool? runInWebRole = null);
        RAReturnMessage SaveAndRunFSRestoreJob(RestoreInfo selectedTree, GCommon.Contract.StorageOptimization.Object.RestoreType restoreType, bool? runInWebRole = null);
        RAReturnMessage SaveAndRunTeamsRestoreJob(RestoreInfo selectedTree, GCommon.Contract.StorageOptimization.Object.RestoreType restoreType, bool? runInWebRole = null);
        RAReturnMessage SaveAndRunDriveRestoreJob(RestoreInfo selectedTree, GCommon.Contract.StorageOptimization.Object.RestoreType restoreType, bool? runInWebRole = null);
        RAReturnMessage SaveAndRunSimulateRestoreJob(RestoreInfo selectedTree);
        RAReturnMessage PreviewRestore(List<RestoreInfo> selectedTrees);
        RAReturnMessage PreviewMultiSiteCollectionRestoreAsync(RestoreInfo info);
        Task<RestoreSettingAndTree> ResolvePendingPreviewRestoreTreeAsync(RestoreInfo perSiteCollectionInfo);
        Task<RAReturnMessage> CheckPreviewRestoreRateLimitAsync();
        Task<RAReturnMessage> GetPreviewRestoreResult(string messageId);
        string RealRunPreviewRestoreJob(JobRunBy jobRunBy, string jobRunByUser, string param, string messageId);
        RAReturnMessage HaveRunningSimulateRestoreJob();
        RAReturnMessage GetSimulareRestoreJobResult(string jobId);
        string GetO365TeamsTenantId(string teamsAddress);
        string GetO365TenantId(string siteUrl);
        string RealRunArchiverRestoreJob(JobRunBy jobRunBy, string jobRunByUser, string param,JobType tempJobType, JobPriority jobPriority = JobPriority.Normal);
        string RealRunEndUserArchiverRestoreJob(JobRunBy jobRunBy, string jobRunByUser, string param, JobType tempJobType, JobPriority jobPriority = JobPriority.Normal);
        bool ShouldQueryInJobForEndUserRestore(string param);
        Task<RestoreSettingAndTree> BuildRestoreSettingAndTreeForEndUserJobAsync(EndUserRestoreJobConfig jobConfig);
        System.Threading.Tasks.Task SaveBaseArchiveJobIdMultiRestoreSettingAndRunAsync(BackendBatchRestoreInfo backendBatchRestoreInfo);
        string RealRunMultiSiteCollectionRestoreJob(JobRunBy jobRunBy, string jobRunByUser, string param);
        string RealRunDriveArchiverRestoreJob(JobRunBy jobRunBy, string jobRunByUser, string param,JobType tempJobType);
        string RealRunSimulateArchiverRestoreJob(JobRunBy jobRunBy, string jobRunByUser, string param, string tenantGroupId);
        string RealRunImportSCMappingJob(string jobRunByUser, string filePath);
        string RealRunExportSCMappingJob(string jobRunByUser);
        string RealRunExportSearchResultJob(JobRunBy jobRunBy, string jobRunByUser, string param, JobType tempJobType);
        string RealRunRestoreCenterExportSearchResultJob(JobRunBy jobRunBy, string jobRunByUser, string param, JobType tempJobType);
        RAReturnMessage AddSCMappings(List<SiteMappingInfo> siteMappings);
        bool CheckSCMappings(List<SiteMappingInfo> sources, out List<SiteMappingInfo> targetNotExistData, out List<SiteMappingInfo> notSameSourceData, out List<SiteMappingInfo> unKnowExceptionData, out List<SiteMappingInfo> validData, out Dictionary<string, List<SiteMappingInfo>> dedupData);
        RAReturnMessage ExportSearchResult(ArchiverRestoreResult searchContract);
        RestoreSiteMappingInfo GetSCMappings(int page, int size);

        RAReturnMessage DeleteSCMappings(List<string> ids);
        RAReturnMessage ImportSiteCollectionMapping(Stream xlsxFileStream);

        RAReturnMessage ExportSiteCollectionMapping();

        RAReturnMessage SwitchFullTextIndexType(SwitchFullTextIndexParam param);
        RAReturnMessage AddSCWhitelist(List<WhitelistInfo> whitelist);
        RestoreSearchWhitelistInfo GetSCWhitelist(int page, int size);
        bool CheckSiteCollectionList(List<WhitelistInfo> sites, bool isBlacklist, out List<WhitelistInfo> notExistSites, out List<WhitelistInfo> validSites, out List<(WhitelistInfo, Exception)> unKnowExceptionSites, out List<string> dupSites);
        RAReturnMessage DeleteSCWhitelist(List<string> ids);
        RAReturnMessage AddSCBlacklist(List<WhitelistInfo> blacklist);
        RestoreSearchWhitelistInfo GetSCBlacklist(int page, int size);
        RAReturnMessage DeleteSCBlacklist(List<string> ids);
        RAReturnMessage ImportSCBlacklist(Stream xlsxFileStream);
        RAReturnMessage ImportSCWhitelist(Stream xlsxFileStream);
        RAReturnMessage ExportSCWhitelist();
        RAReturnMessage ExportSCBlacklist();
        string RealRunImportSCWhitelistJob(string jobRunByUser, string filePath);
        string RealRunExportSCWhitelistJob(string jobRunByUser);
        string RealRunExportSCBlacklistJob(string jobRunByUser);
        string RealRunImportSCBlacklistJob(string jobRunByUser, string filePath);
        Task<List<SiteCollectionNodesInfo>> GetAllSiteCollectionNodesByWhitelistAsync();
        Task<List<SiteCollectionNodesInfo>> GetAllSiteCollectionNodesByBlacklistAsync();
        Task<bool> CheckWhiteListForGroupsTeamsAsync(string group);
        Task<bool> CheckWhiteListForSharePointSiteAsync(string scUrl);
        string RealRunTeamsArchiverRestoreJob(JobRunBy jobRunBy, string jobRunByUser, string param, JobType tempJobType, JobPriority jobPriority = JobPriority.Normal);
        Task<bool> IsOnlySupportExactSearchSiteAsync();

        Task<bool> EDiscoveryIsOnlySupportExactSearchSiteAsync();

        bool IsEnableDeleteArchivedSiteCollection();
        RAReturnMessage RunDeleteArchivedSiteCollectionJob(SiteCollectionNodesInfo siteNodeInfo);
        string RealRunDeleteArchivedSiteCollectionJob(JobRunBy jobRunBy, string jobRunByUser, string param);
        
    }
}
