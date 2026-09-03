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
using Aspose.Words.Saving;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.ArchivedFullTextIndex;
using AvePoint.RA.Contract.Cache;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.ArchiverRestore;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.RoleAssignments;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.DB.SecurityTrimming.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Permission;
using AvePoint.RA.Service.Security;
using AvePoint.RA.Service.Services.ArchivedFullTextIndex.Query;
using AvePoint.RA.Service.Services.Common;
using AvePoint.RA.Service.Services.ControlPanel;
using AvePoint.RA.Service.TermManagement;
using AvePoint.RA.Web.Common;
using AvePoint.RA.Web.Common.Filters;
using AvePoint.RA.Web.Common.Filters.GoogleDriveFilter;
using AvePoint.RA.Web.Common.WIF;
using AvePoint.RA.Web.Controllers.JobMonitor;
using AvePoint.Wrapper.Restore;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static AvePoint.GCommon.Contract.Server.Common.LogCollector.LogConstants;

namespace AvePoint.RA.Web.Controllers
{
    [RMApiAuthorize(RMSOPermissionMasks.ContentRepositoyEnduser, preferred: false)]
    //[ValidateOnlyGoogleLicenseFilter(exceptPaidForModule: PaidForModule.FileSystem)]
    public class ArchiverRestoreController:BaseApiController
    {
        private readonly CommonUtil.RALogger logger = CommonUtil.RALogger.GetInstance(typeof(ArchiverRestoreController));

        private ITenantService _TenantService;
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>(ref _TenantService);

        private IRestoreSearchService _RestoreSearchService;
        private IRestoreSearchService RestoreSearchService => PlatformWindsorManager.GetService(ref _RestoreSearchService);
        private ISettingProfileService _SettingProfileService => PlatformWindsorManager.GetService<ISettingProfileService>();

        private readonly IRMArchivedFullTextIndexService _archivedFullTextIndexService = PlatformWindsorManager.GetService<IRMArchivedFullTextIndexService>();

        private readonly IRMRestoreSiteMappingDao _restoreSiteMappingDao = PlatformWindsorManager.GetService<IRMRestoreSiteMappingDao>();

        private IKeyValueService _KeyValueService => PlatformWindsorManager.GetService<IKeyValueService>();
        private ILicenseHelperService _LicenseHelperService;
        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService(ref _LicenseHelperService);
        private IUserService _UserService;
        private IUserService UserService => PlatformWindsorManager.GetService(ref _UserService);
        private IRMScopeRoleAssignmentDao _RMScopeRoleAssignmentDao;
        private IRMScopeRoleAssignmentDao RMScopeRoleAssignmentDao => PlatformWindsorManager.GetService(ref _RMScopeRoleAssignmentDao);
        private IJobMonitorDao _JMDao;
        private IJobMonitorDao JMDao => PlatformWindsorManager.GetService(ref _JMDao);
        private IRMSecurityTrimmingHelper _SecurityTrimmingHelper;
        private IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService(ref _SecurityTrimmingHelper);

        private IExplorerService _ExplorerService;
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService(ref _ExplorerService);

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.FSEnduser, RMPermissionExtensionMasks.GoogleAdmin, RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterSearch, PermissionJoinType.Any)]
        public Task<List<SiteCollectionNodesInfo>> GetSiteCollectionsInfo([FromBody] int dataSource)
        {
            switch ((RestoreDataSource)dataSource)
            {
                case RestoreDataSource.M365:
                    return RestoreSearchService.GetAllSiteCollectionNodesAsync();
                case RestoreDataSource.FS:
                    return RestoreSearchService.GetAllConnectionNodesAsync();
                case RestoreDataSource.Teams:
                    return RestoreSearchService.GetAllTeamsNodesAsync();
                case RestoreDataSource.GoogleDrive:
                    return RestoreSearchService.GetAllGoogleDriveNodesAsync();
                default:
                    logger.Error($"the data source type is wrong,type:{(RestoreDataSource)dataSource}");
                    return Task.FromResult<List<SiteCollectionNodesInfo>>(null);
            }
        }
        /// <summary>
        /// 根据父路径MD5和文件夹名称列表，查询并批量恢复指定文件夹。
        /// </summary>
        [HttpPost]
        public async Task<bool> RestoreFoldersByParentAndNames([FromBody] RestoreParamInfo paramInfo)
        {
            int pageIndex = 1;
            int pageSize = 500;
            bool hasMore = true;
            logger.Info($"the param info is,url:{paramInfo.SiteUrl},parentpathmd5:{paramInfo.ParentPathMD5}");
            try
            {
                List<string> allRunJobFolderNames = new List<string>();
                while (hasMore)
                {
                    var searchContract = new ArchiverRestoreResult
                    {
                        SerchContract = new BackupDataSearchContract
                        {
                            SearchNode = new SiteCollectionNodesInfo()
                            {
                                SiteUrl = paramInfo.SiteUrl,
                            },
                            FilterPolicy = new ArchiverRestoreFilter()
                            {
                                DataSource = (int)RestoreDataSource.M365,
                                Level = PolicyLevel.Folder,
                                FilterDeleteType = FilterDeletedType.All,
                                ParentPathMd5 = paramInfo.ParentPathMD5,
                                FilterName = ""
                            }
                        },
                        PageIndex = pageIndex,
                        PageSize = pageSize
                    };

                    var allResult = await RestoreSearchService.GetSearchTreeResultAsync(searchContract);
                    if (allResult?.RestoreSerchNodes == null || allResult.RestoreSerchNodes.Count == 0)
                    {
                        break;
                    }
                    List<ArchiverRestoreSerchResult> filteredNodes = new List<ArchiverRestoreSerchResult>();
                    if (paramInfo.FolderNameList != null && paramInfo.FolderNameList.Count > 0)
                    {
                        logger.Info($"the folder name list is not null,count:{paramInfo.FolderNameList.Count}");
                        filteredNodes = allResult.RestoreSerchNodes
                            .Where(node => paramInfo.FolderNameList.Contains(node.ObjectName)).DistinctBy(node => node.PathMd5)
                            .ToList();
                        allRunJobFolderNames.AddRange(filteredNodes?.Select(a => a.ObjectName));
                    }
                    else
                    {
                        logger.Info($"the folder name list is null,will process all folder under parent path md5");
                        filteredNodes = allResult.RestoreSerchNodes.DistinctBy(node => node.PathMd5).ToList();
                    }
                    foreach (var node in filteredNodes)
                    {
                        logger.Info($"start run restore job for folder:{node.ObjectName},location:{node.Location}");
                        var restoreInfo = new RestoreInfo
                        {
                            NodeObjects = new List<ArchiverRestoreSerchResult> { node },
                            DataSource = (int)RestoreDataSource.M365,
                            RestoreAPPOption = AvePoint.GCommon.Contract.StorageOptimization.Object.RestoreOption.NotOverWrite,
                            RestoreOption = AvePoint.GCommon.Contract.StorageOptimization.Object.RestoreOption.NotOverWrite,
                            RestoreTypeSelect = GCommon.Contract.Server.Common.BackupDataSearch.RestoreType.InPlace,
                            RestoreVersionOption = RestoreDocumentVersionsOption.None,
                        };
                        SaveRestoreSettingAndRun(restoreInfo);
                        logger.Info($"finish run restore job for folder:{node.ObjectName},location:{node.Location}");
                    }
                    //// 修正为最后一页少于pageSize时才结束循环
                    //if (allResult.RestoreSerchNodes.Count < pageSize)
                    //{
                    //    hasMore = false;
                    //}
                    pageIndex++;
                }
                foreach (var name in paramInfo.FolderNameList)
                {
                    if (!allRunJobFolderNames.Contains(name))
                    {
                        logger.Info($"the folder can not be found in search result,folder name:{name}");
                    }
                }
                logger.Info("run folders restore success");
                return true;
            }
            catch (Exception ex)
            {
                logger.Error($"something went wrong when RestoreFoldersByParentAndNames,error:{ex}");
                return false;
            }
        }


        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterSearch, PermissionJoinType.Any)]
        public async Task<SiteCollectionNodesInfo> SearchSiteCollectionInfo([FromBody] string siteUrl)
        {
            return (await RestoreSearchService.GetAllSiteCollectionNodesAsync(siteUrl)).FirstOrDefault();
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterSearch, PermissionJoinType.Any, NeedNewOpusTenant = true)]
        public async Task<SiteCollectionNodesInfo> EDiscoverySearchSiteCollectionInfo([FromBody] string siteUrl)
        {
            return (await RestoreSearchService.GetEdiscoveryAllSiteCollectionNodesAsync(siteUrl)).FirstOrDefault();
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterSearch, RMPermissionExtensionMasks.GoogleAdmin, PermissionJoinType.Any)]
        public Task<bool> IsOnlySupportExactSearchSite()
        {
            return RestoreSearchService.IsOnlySupportExactSearchSiteAsync();
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.FSEnduser, RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterSearch, PermissionJoinType.Any, NeedNewOpusTenant = true)]
        public Task<List<SiteCollectionNodesInfo>> GetSiteCollectionsInfoByWhitelist()
        {
            return RestoreSearchService.GetAllSiteCollectionNodesByWhitelistAsync();
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.FSEnduser, RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterSearch, PermissionJoinType.Any, NeedNewOpusTenant = true)]
        public Task<List<SiteCollectionNodesInfo>> GetSiteCollectionsInfoByBlacklist()
        {
            return RestoreSearchService.GetAllSiteCollectionNodesByBlacklistAsync();
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterSearch, PermissionJoinType.Any)]
        public Task<bool> EDiscoveryIsOnlySupportExactSearchSite()
        {
            return RestoreSearchService.EDiscoveryIsOnlySupportExactSearchSiteAsync();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.FSEnduser, RMPermissionExtensionMasks.GoogleAdmin, RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterSearch, PermissionJoinType.Any)]
        public Task<ArchiverRestoreResult> GetAllSerchResult([FromBody] ArchiverRestoreResult searchContract)
        {
            searchContract.OpenIndexDbTimeoutInMs = 3000;
            switch((RestoreDataSource)searchContract.SerchContract.FilterPolicy.DataSource)
            {
                case RestoreDataSource.M365:
                    return RestoreSearchService.GetSearchTreeResultAsync(searchContract);
                case RestoreDataSource.FS:
                    return RestoreSearchService.GetFSSearchResultAsync(searchContract);
                case RestoreDataSource.Teams:
                    return RestoreSearchService.GetSearchTeamsTreeResultAsync(searchContract);
                case RestoreDataSource.GoogleDrive:
                    return RestoreSearchService.GetDriveSearchTreeResultAsync(searchContract);
                default:
                    logger.Error($"the data source type is wrong,type:{(RestoreDataSource)searchContract.SerchContract.FilterPolicy.DataSource}");
                    return Task.FromResult<ArchiverRestoreResult>(null);
            }
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.FSEnduser, RMPermissionExtensionMasks.GoogleAdmin, RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterSearch, PermissionJoinType.Any)]
        public Task<ArchiverRestoreResult> GetAllSiteCollectionSerchResult([FromBody] ArchiverRestoreResult searchContract)
        {
            return RestoreSearchService.GetAllSiteCollectionSerchResultAsync(searchContract ?? new ArchiverRestoreResult());
        }

        [HttpPost]        
        [RMApiAuthorize(RMPermissionMasks.FSEnduser, RMPermissionExtensionMasks.GoogleAdmin, RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterSearch, PermissionJoinType.Any, NeedNewOpusTenant = true)]
        public bool IsEnableFullTextIndexSearch()
        {
            return RestoreSearchService.IsEnableFullTextIndexSearch();
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.FSEnduser, RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterSearch, PermissionJoinType.Any, NeedNewOpusTenant = true)]
        public bool IsSCBlackListForEdiscovery()
        {
            return _KeyValueService.IsSCBlackListForEdiscovery();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin, NeedNewOpusTenant = true)]
        public RAReturnMessage SwitchFullTextIndexType([FromBody] SwitchFullTextIndexParam param)
        {
            return RestoreSearchService.SwitchFullTextIndexType(param);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin)]
        public bool SendFullTextIndexJobMessage()
        {
            if (_KeyValueService.IsSCBlackListForEdiscovery())
            {
                logger.Info($"Blacklist not need check count.");
            }
            else
            {
                var scWhitelist = _restoreSiteMappingDao.GetWhitelistCount();
                logger.Info($"Whitelist count [{scWhitelist}].");
                if (scWhitelist < 1)
                {
                    return false;
                }
            }

            _archivedFullTextIndexService.SendJobMessage();
            return true;
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.FSEnduser, RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterSearch, PermissionJoinType.Any, NeedNewOpusTenant = true)]
        public Task<ArchiverRestoreResult> GetAllEDiscoverySearchResult([FromBody] ArchiverRestoreResult searchContract)
        {
            var isNewFullTextIndexkeyValue = _KeyValueService.Get(KeyNameCollection.IsNewFullTextIndex);
            if (isNewFullTextIndexkeyValue != null && bool.TryParse(isNewFullTextIndexkeyValue.Value, out var result) && result)
            {
                var querierV1 = new RMArchivedFullTextIndexQuerierV1(searchContract.SerchContract);
                return querierV1.QueryAsync(searchContract.ContinuationToken, searchContract.PageSize);
            }

            var querier = new RMArchivedFullTextIndexQuerier(searchContract.SerchContract);
            return querier.QueryAsync(searchContract.ContinuationToken, searchContract.PageSize, searchContract.CategoryId);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.FSEnduser, RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterSearch, PermissionJoinType.Any)]
        public Task<string> GetSiteLatestArchiverTime([FromBody] SiteCollectionNodesInfo searchNode)
        {
            return _archivedFullTextIndexService.GetSiteLatestArchivedTime(searchNode);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.FSEnduser, RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterSearch, PermissionJoinType.Any)]
        public Task<string> GetLatestArchiverTime()
        {
            return _archivedFullTextIndexService.GetLatestArchivedTime();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public RAReturnMessage ImportSCMappings()
        {
            try
            {
                var file = Request.Form.Files["fileUp"];
                var isCheckOverwrite = Request.Form["isOverride"];
                var oldSetting = _SettingProfileService.UpdateSiteMappingIsOverrideInfo(isCheckOverwrite);
                Logger.Info("tm import file,file name :{0} old setting :{1}", file.FileName, oldSetting);
                string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
                if(extension != "xlsx")
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_JS_JM_ImportFileFormatError") };
                }
                RestoreSearchService.ImportSiteCollectionMapping(file.OpenReadStream());
                return new RAReturnMessage() { MessageType = RAMessageType.Successful };
            }
            catch (Exception ex)
            {
                Logger.Info($"Fail request import SCMappings,ex:{ex}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed};
            }
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public RAReturnMessage ExportSCMappings()
        {
            return RestoreSearchService.ExportSiteCollectionMapping();
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.FSEnduser, RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterSearch, PermissionJoinType.Any, NeedNewOpusTenant = true)]
        public Task<ArchiverRestoreResult> GetEDiscoverySimpleSearchResult([FromBody] ArchiverRestoreSimpleSearchQueryParameter parameter)
        {
            var isNewFullTextIndexkeyValue = _KeyValueService.Get(KeyNameCollection.IsNewFullTextIndex);
            if (isNewFullTextIndexkeyValue != null && bool.TryParse(isNewFullTextIndexkeyValue.Value, out var result) && result)
            {
                var querierV1 = new RMArchivedFullTextIndexSimpleQuerierV1(parameter);
                return querierV1.QueryAsync();
            }
            
            var querier = new RMArchivedFullTextIndexSimpleQuerier(parameter);
            return querier.QueryAsync();
        }
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public RAReturnMessage AddSCMappings([FromBody] List<SiteMappingInfo> siteMappings)
        {
           return RestoreSearchService.AddSCMappings(siteMappings);
        }
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public RestoreSiteMappingInfo GetSCMappings([FromBody] RSMappingPage page)
        {
            return RestoreSearchService.GetSCMappings(page.PageIndex, page.PageSize);
        }
        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterExport, RMPermissionExtensionMasks.GoogleAdmin, PermissionJoinType.Any)]
        public RAReturnMessage ExportSearchResult([FromBody] ArchiverRestoreResult info)
        {
            var result = RestoreSearchService.ExportSearchResult(info);
            return result;
        }
        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public RAReturnMessage DeleteSCMappings([FromBody] List<string> ids)
        {
            return RestoreSearchService.DeleteSCMappings(ids);
        }

        #region Search Whitelist

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin, NeedNewOpusTenant = true)]
        public RAReturnMessage AddSCWhitelist([FromBody] List<WhitelistInfo> siteMappings)
        {
            return RestoreSearchService.AddSCWhitelist(siteMappings);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin, NeedNewOpusTenant = true)]
        [ValidSCWhitelistPermissionFilter]
        public RestoreSearchWhitelistInfo GetSCWhiteList([FromBody] RSMappingPage page)
        {
            return RestoreSearchService.GetSCWhitelist(page.PageIndex, page.PageSize);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin, NeedNewOpusTenant = true)]
        public RAReturnMessage DeleteSCWhitelist([FromBody] List<string> ids)
        {
            return RestoreSearchService.DeleteSCWhitelist(ids);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin, NeedNewOpusTenant = true)]
        public RAReturnMessage ImportSCWhitelist()
        {
            try
            {
                var file = Request.Form.Files["fileUp"];
                Logger.Info("tm import file,file name :{0}", file.FileName);
                string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
                if (extension != "xlsx")
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_JS_JM_ImportFileFormatError") };
                }
                return RestoreSearchService.ImportSCWhitelist(file.OpenReadStream());
            }
            catch (Exception ex)
            {
                Logger.Info($"Fail request import SCWhitelist,ex:{ex}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed };
            }
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin, NeedNewOpusTenant = true)]
        public RAReturnMessage ExportSCWhitelist()
        {
            return RestoreSearchService.ExportSCWhitelist();
        }

        #endregion

        #region Search Blacklist

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin, NeedNewOpusTenant = true)]
        public RAReturnMessage AddSCBlacklist([FromBody] List<WhitelistInfo> siteMappings)
        {
            return RestoreSearchService.AddSCBlacklist(siteMappings);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin, NeedNewOpusTenant = true)]
        public RestoreSearchWhitelistInfo GetSCBlackList([FromBody] RSMappingPage page)
        {
            return RestoreSearchService.GetSCBlacklist(page.PageIndex, page.PageSize);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin, NeedNewOpusTenant = true)]
        public RAReturnMessage DeleteSCBlacklist([FromBody] List<string> ids)
        {
            return RestoreSearchService.DeleteSCBlacklist(ids);
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin, NeedNewOpusTenant = true)]
        public RAReturnMessage ImportSCBlacklist()
        {
            try
            {
                var file = Request.Form.Files["fileUp"];
                Logger.Info("tm import file,file name :{0}", file.FileName);
                string extension = file.FileName.Substring(file.FileName.LastIndexOf(".") + 1);
                if (extension != "xlsx")
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, ErrorMessage = I18NEntity.GetString("RM_JS_JM_ImportFileFormatError") };
                }
                return RestoreSearchService.ImportSCBlacklist(file.OpenReadStream());
            }
            catch (Exception ex)
            {
                Logger.Info($"Fail request import SCBlacklist,ex:{ex}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed };
            }
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin, NeedNewOpusTenant = true)]
        public RAReturnMessage ExportSCBlacklist()
        {
            return RestoreSearchService.ExportSCBlacklist();
        }

        #endregion

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.FSEnduser, RMPermissionExtensionMasks.GoogleAdmin, RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterFullControl, PermissionJoinType.Any)]
        public RAReturnMessage SaveRestoreSettingAndRun([FromBody] RestoreInfo info)
        {
            if (info.RestoreTypeSelect == RestoreType.ToSPOLocation)
            {
                var result = ExplorerService.CheckSPUrl4Job(info.SPOLibOrFolderPath, null);
                if (result == null)
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, ErrorMessage = I18NEntity.GetString("RM_JS_Rule_SPDestUrlError") };
                }
                info.DestDto = result;
            }
            info.IsEndUserJob = false;
            GCommon.Contract.StorageOptimization.Object.RestoreType tempRestoreType;

            switch (info.RestoreTypeSelect)
            {
                case RestoreType.InPlace:
                    tempRestoreType = GCommon.Contract.StorageOptimization.Object.RestoreType.InPlace;
                    break;
                case RestoreType.ToSPOLocation:
                    tempRestoreType = GCommon.Contract.StorageOptimization.Object.RestoreType.ToSPOLocation;
                    break;
                default:
                    tempRestoreType = GCommon.Contract.StorageOptimization.Object.RestoreType.OutPlace;
                    break;
            }
            if (!string.IsNullOrEmpty(info.FailedJobId))
            {
                info.DataSource = (int)RestoreDataSource.M365;
                var permission1 = ((long)SecurityTrimmingHelper.GetUserPermissionAsync<RMPermissionMasks>().GetAwaiter().GetResult()).ToString();
                var soPermission = ((long)SecurityTrimmingHelper.GetUserPermissionAsync<RMSOPermissionMasks>().GetAwaiter().GetResult()).ToString();
                int roleType = GetUserRoleType(permission1, soPermission);
                if (roleType == (int)RMRoleType.ApplicationAdmin)
                {
                    logger.Info("this rerun restore is superadmin run");
                }
                else if (!CheckHasPermissionToRerunRestore(info.FailedJobId))
                {
                    throw new Exception("not has permission");
                }
            }
            try
            {

                if (info.DataSource == (int)RestoreDataSource.M365)
                {
                    logger.Info("this restore data source is M365");
                    if (string.IsNullOrEmpty(info.FailedJobId))
                    {
                        foreach (var tempInfo in BuildRestoreInfos(info))
                        {
                            var tempResult = RestoreSearchService.SaveAndRunRestoreJob(tempInfo, tempRestoreType);
                            if (tempResult.MessageType == RAMessageType.Failed)
                            {
                                return tempResult;
                            }
                        }
                    }
                    else
                    {
                        var tempResult = RestoreSearchService.SaveAndRunRestoreJob(info, tempRestoreType);
                        if (tempResult.MessageType == RAMessageType.Failed)
                        {
                            return tempResult;
                        }
                    }

                }
                else if (info.DataSource == (int)RestoreDataSource.FS)
                {
                    logger.Info("this restore data source is FS");
                    var tempResult = RestoreSearchService.SaveAndRunFSRestoreJob(info, tempRestoreType);
                    if (tempResult.MessageType == RAMessageType.Failed)
                    {
                        return tempResult;
                    }
                }
                else
                {
                    switch(info.DataSource)
                    {
                        case (int)RestoreDataSource.Teams:
                            {
                                logger.Info("this restore data source is Teams");
                                foreach (var tempInfo in BuildTeamsRestoreInfos(info))
                                {
                                    var tempResult = RestoreSearchService.SaveAndRunTeamsRestoreJob(info, tempRestoreType);
                                    if (tempResult.MessageType == RAMessageType.Failed)
                                    {
                                        return tempResult;
                                    }
                                }
                                break;
                            }
                        case (int)RestoreDataSource.GoogleDrive:
                            logger.Info("this restore data source is google drive.");
                            foreach (var tempInfo in BuildGDriveRestoreInfos(info))
                            {
                                var tempResult = RestoreSearchService.SaveAndRunDriveRestoreJob(info, tempRestoreType);
                                if (tempResult.MessageType == RAMessageType.Failed)
                                {
                                    return tempResult;
                                }
                            }
                            break;
                        default:
                            logger.Error($"the data source type is wrong,type:{info.DataSource}");
                            return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, ErrorMessage = "Wrong data source type" };
                    }
                }
                logger.Info($"finish run restore job");
            }
            catch (Exception e)
            {
                logger.Error($"something went wrong when save restore setting,error :{e}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, ErrorMessage = I18NEntity.GetString("RM_RS_SaveRestoreSettingError") };
            }
            return new RAReturnMessage();
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterFullControl, PermissionJoinType.Any)]
        public async Task<RAReturnMessage> PreviewRestore([FromBody] RestoreInfo info)
        {
            if (info.DataSource != (int)RestoreDataSource.M365)
            {
                logger.Error($"preview restore data size only supports M365 data source,current data source:{info.DataSource}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, ErrorMessage = I18NEntity.GetString("RM_AR_PreviewRestore_UnsupportedDataSourceType_ErrorMessage") };
            }
            if (info.NodeObjects != null && info.NodeObjects.Count > RMConstants.PreviewRestoreMaxSelectedObjectCount)
            {
                logger.Warn($"selected objects count:{info.NodeObjects.Count} exceeds the max limit:{RMConstants.PreviewRestoreMaxSelectedObjectCount},can not run preview restore data size job.");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, ErrorMessage = I18NEntity.GetString("RM_AR_PreviewRestore_MaxSelectedObjectsExceeded_ErrorMessage", RMConstants.PreviewRestoreMaxSelectedObjectCount) };
            }
            info.IsEndUserJob = false;
            try
            {
                RAReturnMessage rateLimitResult = await RestoreSearchService.CheckPreviewRestoreRateLimitAsync();
                if (rateLimitResult != null)
                {
                    return rateLimitResult;
                }
                RAReturnMessage tempResult = RestoreSearchService.PreviewRestore(BuildRestoreInfos(info));
                logger.Info($"finish run preview restore data size job");
                return tempResult;
            }
            catch (Exception e)
            {
                logger.Error($"something went wrong when preview restore data size,error :{e}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, ErrorMessage = I18NEntity.GetString("RM_RS_SaveRestoreSettingError") };
            }
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterFullControl, PermissionJoinType.Any)]
        public async Task<RAReturnMessage> PreviewMultiSiteCollectionRestore([FromBody] RestoreInfo info)
        {
            try
            {
                RAReturnMessage rateLimitResult = await RestoreSearchService.CheckPreviewRestoreRateLimitAsync();
                if (rateLimitResult != null)
                {
                    return rateLimitResult;
                }
                return RestoreSearchService.PreviewMultiSiteCollectionRestoreAsync(info);
            }
            catch (Exception e)
            {
                logger.Error($"something went wrong when preview multi site collection restore data size,error :{e}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, ErrorMessage = I18NEntity.GetString("RM_RS_SaveRestoreSettingError") };
            }
        }

        [HttpGet]
        [RMApiAuthorize(RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterFullControl, PermissionJoinType.Any)]
        public Task<RAReturnMessage> GetPreviewRestoreResult([FromQuery] string messageId)
        {
            return RestoreSearchService.GetPreviewRestoreResult(messageId);
        }

        private int GetUserRoleType(string opusILPermission, string opusSOPermission)
        {
            var roleType = opusILPermission.PermissionToRole();
            roleType = roleType > -1 ? roleType : opusSOPermission.SOPermissionToRole();
            return roleType;
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterFullControl, PermissionJoinType.Any)]
        public async Task<RAReturnMessage> SaveMultiSiteCollectionRestoreSettingAndRun([FromBody] RestoreInfo info)
        {
            if(info.NodeObjects != null && info.NodeObjects.Count() > 1)
            {
                return RestoreSearchService.SaveMultiSiteCollectionRestoreSettingAndRunInVirtualJob(new() { RestoreOption = info});
            }
            return await RestoreSearchService.SaveMultiSiteCollectionRestoreSettingAndRunAsync(info);
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterFullControl, PermissionJoinType.Any)]
        public RAReturnMessage SelectAllSiteCollectionRestore([FromBody] SelectMultiScRestoreInfo info)
        {
            info.IsSelectAll = true;
            return RestoreSearchService.SaveMultiSiteCollectionRestoreSettingAndRunInVirtualJob(info);
        }

        [HttpPost]
        public RAReturnMessage SaveSimulateRestoreSettingAndRun([FromBody] List<ArchiverRestoreSerchResult> nodeObjects)
        {
            try
            {
                RestoreInfo info = new RestoreInfo { NodeObjects = nodeObjects };
                return RestoreSearchService.SaveAndRunSimulateRestoreJob(BuildRestoreInfos(info).First());
            }
            catch (Exception e)
            {
                logger.Error($"something went wrong when save simulate restore setting,error :{e}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, ErrorMessage = I18NEntity.GetString("RM_RS_UnkonwExceptionPleaseRetry") };
            }
        }

        [HttpPost]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public RAReturnMessage RunDeleteArchivedSCJob([FromBody] SiteCollectionNodesInfo siteNodeInfo)
        {
            return RestoreSearchService.RunDeleteArchivedSiteCollectionJob(siteNodeInfo);
        }

        [HttpGet]
        [RMApiAuthorize(RMPermissionMasks.ControlPanelAdmin, RMSOPermissionMasks.ControlPanelAdmin)]
        public bool IsEnableDeleteArchivedSiteCollection()
        {
            return RestoreSearchService.IsEnableDeleteArchivedSiteCollection();
        }

        [HttpGet]
        public RAReturnMessage HaveRunningSimulateRestoreJob()
        {
            try
            {
                return RestoreSearchService.HaveRunningSimulateRestoreJob();
            }
            catch (Exception e)
            {
                logger.Error($"Fail check HaveRunningSimulateRestoreJob,error:{e}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty, ErrorMessage = I18NEntity.GetString("RM_RS_UnkonwExceptionPleaseRetry") };
            }
        }

        [HttpGet]
        public RAReturnMessage GetSimulateRestoreJobResult([FromQuery] string jobId)
        {
            try
            {
                return RestoreSearchService.GetSimulareRestoreJobResult(jobId);
            }
            catch (Exception e)
            {
                logger.Error($"Fail check GetSimulareRestoreJobResult,error:{e}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, Extension = string.Empty, ErrorMessage = I18NEntity.GetString("RM_RS_UnkonwExceptionPleaseRetry")  };
            }
        }
        private bool CheckHasPermissionToRerunRestore(string failedJobId)
        {
            try
            {
                PermissionChecker<RMSOPermissionMasks> opusSOPermissionChecker = new(RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterFullControl, LicenseHelperService.HasOpusSOLicense, PermissionJoinType.Any);
                if (!opusSOPermissionChecker.IsNonePermission && opusSOPermissionChecker.CheckPermissionAsync().GetAwaiter().GetResult())
                {
                    var containerIds = GetContainerIdCollectionAsync().GetAwaiter().GetResult();
                    var failedRestoreJob = JMDao.GetJobById(failedJobId);
                    if (containerIds.Contains(failedRestoreJob.ContainerId, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
                return false;
            }
            catch (Exception e)
            {
                logger.Error($"Fail CheckHasPermissionToRerunRestore,error:{e}");
                return false;
            }
        }
        private async Task<List<string>> GetContainerIdCollectionAsync()
        {
            var collection = new List<string>();
            List<string> userAndGroupUserIds = await UserService.GetUserAndGroupUserIdsAsync(TenantLocalValue.LogonUserId);
            var containerGruops = (await RMScopeRoleAssignmentDao.GetAllContainersByUsersAsync(userAndGroupUserIds)).Values;
            foreach (var containers in containerGruops)
            {
                foreach (var container in containers)
                {
                    collection.Add(container.ToString());
                }
            }
            return collection;
        }
        private List<RestoreInfo> BuildRestoreInfos(RestoreInfo info)
        {
            List<RestoreInfo> restoreInfos = new List<RestoreInfo>();
            Dictionary<string, List<ArchiverRestoreSerchResult>> siteWithObject = new Dictionary<string, List<ArchiverRestoreSerchResult>>();
            logger.Info("start generate restore setting nodes");
            var needRestoreObjects = info.NodeObjects;
            foreach (ArchiverRestoreSerchResult obj in needRestoreObjects)
            {
                if (siteWithObject.ContainsKey(obj.SitePath))
                {
                    siteWithObject[obj.SitePath].Add(obj);
                }
                else
                {
                    siteWithObject.Add(obj.SitePath, new List<ArchiverRestoreSerchResult>());
                    siteWithObject[obj.SitePath].Add(obj);
                    logger.Info($"siteWithObject not containe key:{obj.SitePath},add it");
                }
            }
            logger.Info($"finish ganerate siteWithObject,count:{siteWithObject.Count}");
            foreach (var tempKeyValue in siteWithObject)
            {
                logger.Info($"this site with object info is :key:{tempKeyValue.Key},value count:{tempKeyValue.Value?.Count}");
                var tempRestoreInfo = Clone(info);
                tempRestoreInfo.NodeObjects.Clear();
                tempRestoreInfo.NodeObjects = tempKeyValue.Value;
                restoreInfos.Add(tempRestoreInfo);
            }
            return restoreInfos;
        }
        
        private List<RestoreInfo> BuildTeamsRestoreInfos(RestoreInfo info)
        {
            List<RestoreInfo> restoreInfos = new List<RestoreInfo>();
            Dictionary<string, List<ArchiverRestoreSerchResult>> teamsWithObject = new Dictionary<string, List<ArchiverRestoreSerchResult>>();
            logger.Info("start generate restore setting nodes");
            var needRestoreObjects = info.NodeObjects;
            foreach (ArchiverRestoreSerchResult obj in needRestoreObjects)
            {
                if (teamsWithObject.ContainsKey(obj.FullPath))
                {
                    teamsWithObject[obj.FullPath].Add(obj);
                }
                else
                {
                    teamsWithObject.Add(obj.FullPath, new List<ArchiverRestoreSerchResult>());
                    teamsWithObject[obj.FullPath].Add(obj);
                    logger.Info($"teamsWithObject not containe key:{obj.FullPath},add it");
                }
            }
            logger.Info($"finish ganerate teamsWithObject,count:{teamsWithObject.Count}");
            foreach (var tempKeyValue in teamsWithObject)
            {
                logger.Info($"this site with object info is :key:{tempKeyValue.Key},value count:{tempKeyValue.Value?.Count}");
                var tempRestoreInfo = Clone(info);
                tempRestoreInfo.NodeObjects.Clear();
                tempRestoreInfo.NodeObjects = tempKeyValue.Value;
                restoreInfos.Add(tempRestoreInfo);
            }
            return restoreInfos;
        }
        private List<RestoreInfo> BuildGDriveRestoreInfos(RestoreInfo info)
        {
            List<RestoreInfo> restoreInfos = new List<RestoreInfo>();
            Dictionary<string, List<ArchiverRestoreSerchResult>> driveWithObject = new Dictionary<string, List<ArchiverRestoreSerchResult>>();
            logger.Info("start generate restore setting nodes");
            var needRestoreObjects = info.NodeObjects;
            foreach (ArchiverRestoreSerchResult obj in needRestoreObjects)
            {
                if (driveWithObject.ContainsKey(obj.SitePath))
                {
                    driveWithObject[obj.SitePath].Add(obj);
                }
                else
                {
                    driveWithObject.Add(obj.SitePath, new List<ArchiverRestoreSerchResult>());
                    driveWithObject[obj.SitePath].Add(obj);
                    logger.Info($"driveWithObject not containe key:{obj.SitePath},add it");
                }
            }
            logger.Info($"finish ganerate driveWithObject,count:{driveWithObject.Count}");
            foreach (var tempKeyValue in driveWithObject)
            {
                logger.Info($"this drive with object info is :key:{tempKeyValue.Key},value count:{tempKeyValue.Value?.Count}");
                var tempRestoreInfo = Clone(info);
                tempRestoreInfo.NodeObjects.Clear();
                tempRestoreInfo.NodeObjects = tempKeyValue.Value;
                restoreInfos.Add(tempRestoreInfo);
            }
            return restoreInfos;
        }
        private RestoreInfo Clone(RestoreInfo retoreInfo)
        {
            var serialized = JsonConvert.SerializeObject(retoreInfo);
            return JsonConvert.DeserializeObject<RestoreInfo>(serialized);
        }

        [HttpPost]
        [RMApiAuthorize(RMSOPermissionMasks.ContentRepositoyEnduser | RMSOPermissionMasks.RestoreCenterFullControl, PermissionJoinType.Any)]
        public async Task<RAReturnMessage> SaveAdvancedRestoreSettingAndRunAsync([FromBody] RestoreInfo info)
        {
            try
            {
                var tempRestoreType = info.RestoreTypeSelect switch
                {
                    RestoreType.ArchivedStubs => GCommon.Contract.StorageOptimization.Object.RestoreType.ArchivedStubs,
                    RestoreType.M365InPlaceArchivedFiles => GCommon.Contract.StorageOptimization.Object.RestoreType.M365InPlaceArchivedFiles,
                    _ => throw new Exception($"the restore type is not supported, type: {info.RestoreTypeSelect}"),
                };
                if (string.IsNullOrEmpty(info.SPOLibOrFolderPath))
                {
                    throw new Exception("the SPOLibOrFolderPath is null or empty");
                }
                info.DestDto = ExplorerService.CheckSPUrl4Job(info.SPOLibOrFolderPath, null, true);
                if (info.DestDto is null)
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, ErrorMessage = I18NEntity.GetString("RM_JS_Rule_SPDestUrlError") };
                }
                if (info.RestoreScope == GCommon.Contract.StorageOptimization.Object.RestoreScope.SelectedLocationOnly && string.IsNullOrEmpty(info.DestDto.ListPath) && string.IsNullOrEmpty(info.DestDto.FolderPath))
                {
                    return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, ErrorMessage = I18NEntity.GetString("RM_JS_JM_RestoreCenter_InvalidScope") };
                }
                info.RestoreExecutionRequest = new();
                switch (info.DataSource)
                {
                    case (int)RestoreDataSource.M365:
                    case (int)RestoreDataSource.Teams:
                        logger.Info("This restore data source is M365 or Teams");
                        var tempResult = RestoreSearchService.SaveAndRunRestoreJob(info, tempRestoreType);
                        if (tempResult.MessageType == RAMessageType.Failed)
                        {
                            return tempResult;
                        }
                        break;
                    default:
                        throw new Exception($"the data source type is not supported, type: {info.DataSource}");
                }
                logger.Info($"finish run advanced restore job");
                return new RAReturnMessage();
            }
            catch (Exception e)
            {
                logger.Error($"something went wrong when save advanced restore setting, error: {e}");
                return new RAReturnMessage() { MessageType = RAMessageType.Failed, FaildType = RAFailedType.None, ErrorMessage = I18NEntity.GetString("RM_RS_SaveRestoreSettingError") };
            }
        }
    }
}
