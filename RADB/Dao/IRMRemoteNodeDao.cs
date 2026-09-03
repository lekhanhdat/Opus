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
using System.Collections.Generic;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.DB.Model;
using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
using System.Threading.Tasks;
using System;
using AvePoint.GCommon.Contract.SharePointBrowser;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMRemoteNodeDao
    {
        /// <summary>
        /// 删除Remote WebApp
        /// </summary>
        /// <param name="ids">web app id list</param>
        void DeleteRemoteWebApplication(List<string> ids);

        /// <summary>
        /// For debug/test
        /// </summary>
        void ClearAll();

        void CreateRemoteWebApplications(List<RemoteWebApplication> webApplications);

        void UpdateRemoteWebApplications(List<RemoteWebApplication> webApplications);

        #region Site Collection


        #endregion

        #region SkyDrive Pro
        #endregion

        #region Office365 Group Sites
        List<string> GetO365GroupSiteUrlsByNames(List<string> names);
        #endregion

        void CreateRemoteSiteCollectionsByCurrentGroupId(List<RemoteSiteCollection> siteCollections);

        void DeleteRemoteSiteCollectionsByUrl(IEnumerable<string> urls);

        void DeleteRemoteSiteCollectionByParentId(IEnumerable<string> parentIds);

        List<RemoteNodePara> GetRemoteWebApplicationNodes();

        Dictionary<string, string> GetContainerNameBySiteUrls(IEnumerable<string> urls);

        List<SyncRemoteNodePara> GetAllSiteCollectionNodesByPage(int pageIndex, int pageSize);

        int GetRemoteNodesCount();

        void UpdateSyncSiteCollections(List<SyncRemoteNodePara> siteCollections);
        List<RemoteSiteCollection> GetRemoteSiteCollectionBySiteUrls(IEnumerable<string> urls, IEnumerable<string> containerId);
        List<RemoteSiteCollection> GetRemoteSiteCollectionBySiteUrls(IEnumerable<string> urls);
        int GetRemoteSiteCollectionCountByParentId(string parentId, bool includeOrphenNode = true);
        List<RemoteSiteCollection> GetRemoteSiteCollectionsByParentId(string parentId, SiteCollectionState[] states);
        List<RemoteSiteCollection> GetRemoteSiteCollectionsByParentIdByCursor(string parentId, SiteCollectionState[] states, ref string lastId, int pageSize, bool includeOrphenNode = true, SiteCollectionType[] types = null);

        List<RemoteSiteCollection> GetRemoteSiteCollectionsByParentId(string parentId, SiteCollectionState[] states, SiteCollectionType[] types, string[] names = null);


        RMSPSampleTreeNode GetSiteCollections(RMSPSampleTreeNode node, bool checkPermission, bool includeOrphenNode = false);

        RMSPSampleTreeNode GetSiteCollectionsUnderTeams(RMSPSampleTreeNode node);
        
        RMSPSampleTreeNode GetSiteCollectionBySearch(RMSPSampleTreeNode node, bool checkPermission, string searchKey, bool includeOrphanNode = false);
        RMSPSampleTreeNode GetTeamsBySearch(RMSPSampleTreeNode node, bool checkPermission, string searchKey, bool includeOrphanNode = false);
        List<Guid> GetOrphanedODIds();
        Dictionary<string, List<NodeCollection>> GetSiteCollectionByParentIds(List<string> ids);
        Dictionary<string, string> GetTeamId2TeamNameDicByTeamIds(List<string> teamIds);
        RemoteNodePara GetGroupByNameAndNodeLevel(string name, int nodeLevel);

        RemoteNodePara GetGroupByAosIdAndNodeLevel(string aosId, int nodeLevel);

        #region Private Channel

        List<SyncRemoteNodePara> GetAllPrivateChannelByPage(int pageIndex, int pageSize);

        List<RMRemoteNode> GetAllPrivateChannelNodesByPage(int pageIndex, int pageSize);

        List<SyncRemoteNodePara> GetAllPrivateChannel();
        bool IsPrivateChannelGroupExist();
        List<string> GetPrivateChannelByGroupTeamSiteContainerIds(List<string> groupTeamSiteContainerIds);
        #endregion

        HashSet<string> GetO365GroupSiteByUrls(List<string> urls);

        Dictionary<string, string> GetAllSiteAndSPGroupMapping();

        Dictionary<string, List<string>> GetO365GroupSiteName2UrlDicByNames(List<string> names);
        void UpdateO365GroupSiteByUrls(List<RemoteSiteCollection> o365GroupSiteCollections);

        void UpdateSiteCollectionSecondParentId(List<SyncRemoteNodePara> siteCollections);

        RemoteSiteCollection GetRemoteSiteCollectionById(string id);
        RemoteSiteCollection GetRemoteSiteCollectionByObjectId(string objectId);

        (RemoteSiteCollection, List<RemoteSiteCollection>) GetTeamsGroupAndChannelsCollectionByTeamsId(string teamsId, bool needChannel = false);
        List<RemoteSiteCollection> GetTeamsGroupAndChannelsCollectionByListTeamsId(IEnumerable<string> teamsId);
        bool CheckIsOrphanedOD(string scId);

        List<RemoteSiteCollection> GetRemoteSiteCollectionByIds(List<string> ids);

        List<RemoteSiteCollection> GetRemoteSiteCollectionByObjectIds(List<string> ids);

        string GetUrlById(string id);

        List<RemoteSiteCollection> GetRemoteSiteCollectionsByUrls(IEnumerable<string> urls);

        RemoteSiteCollection GetRemoteSiteCollectionByUrl(string url);

        RemoteSiteCollection GetRemoteSiteCollectionByExactUrl(string url);

        RemoteSiteCollection GetRemoteSiteCollectionByHostUrl(string url);

        RemoteSiteCollection GetRemoteSiteCollectionByListUrl(string listUrl);

        List<RemoteWebApplication> GetAllWebApplications(RMBrowseTreeNodeSourceType type = RMBrowseTreeNodeSourceType.All);

        RMSPSampleTreeNode GetWebApplications(RMSPSampleTreeNode node, RMBrowseTreeNodeSourceType type, bool checkPermission);

        Task<RMSPSampleTreeNode> GetWebApplicationsForSearchAsync(RMSPSampleTreeNode node, RMBrowseTreeNodeSourceType type, bool checkPermission);
        Task<RMSPSampleTreeNode> GetWebApplicationsOnlyForSearchAsync(RMSPSampleTreeNode node, RMBrowseTreeNodeSourceType type, bool checkPermission);

        Task<RMSPSampleTreeNode> GetWebApplicationsForExactlySearchAsync(RMSPSampleTreeNode node, RMBrowseTreeNodeSourceType type, bool checkPermission, bool includeOrphanNode);

        Task<RMSPSampleTreeNode> GetWebApplicationsOnlyForExactlySearchAsync(RMSPSampleTreeNode node, RMBrowseTreeNodeSourceType type, bool checkPermission, bool includeOrphanNode);

        RemoteWebApplication GetWebApplicationById(string id);

        List<RemoteWebApplication> GetWebApplicationByIds(List<string> ids);

        string GetContainerIdByName(string containerName, int nodeLevel);

        List<RemoteSiteCollection> GetAllRemoteSiteCollections();

        List<RemoteSiteCollection> GetAllRemoteSiteCollections(int pageIndex, int pageSize, out int totalCount);

        List<RemoteSiteCollection> GetAllRemoteSiteCollections(int pageIndex, int pageSize, string key, out int totalCount);

        List<RemoteSiteCollection> GetMappedRemoteSitesPaged(int pageIndex, int pageSize, string keyword, List<string> selectedNodeIds, out int totalCount);

        List<RemoteWebApplication> GetRemoteWebApplications();

        List<RemoteWebApplication> GetAuthorisedSkyDriveProGroups();

        List<RemoteWebApplication> GetAuthorisedOffice365GroupSitesGroups();

        List<RemoteWebApplication> GetAuthorisedPrivateChannelSitesGroups();

        List<RemoteWebApplication> GetAuthorisedAllSiteGroups(bool includeO365 = false, bool includePrivateChannel = false);

        List<RMRemoteNode> GetAllContainers();

        void UpdateContainers(List<RMRemoteNode> containers);
        List<TreeNodeCollection> GetAuthorisedAllSites();
        List<RemoteSiteCollection> GetRemoteSiteCollectionByParam(List<string> param, bool isUrl = true);
        Dictionary<string, string> GetTeamsIdsOfSites(IEnumerable<string> scUrls);
        HashSet<string> GetHavePermissionTeams(IEnumerable<string> teamIds, IEnumerable<string> permissionContainer);
        List<string> GetAllSPContainerIds(); 
        List<string> GetAllTeamsContainerIds();

        List<RMRemoteNode> GetAllTeamsContainers();
        Dictionary<string, List<RMRemoteNode>> GetAllHasChannelTeamsNodes(string containerId);
        long GetChannnelNodeCount();
        bool CheckSiteExistBySiteId(string siteId);

        RMRemoteNode GetRemoteNodeByParentId(Guid parentId);
        RMRemoteNode GetRemoteNodeById(Guid id);

        bool IsRemoteSiteExist();

        List<RemoteSiteCollection> GetRemoteSiteCollectionsByTeamsId(string teamsId, SiteCollectionState[] states);
        List<RemoteSiteCollection> GetRemoteSiteCollectionsByTeamsIds(List<string> teamsIds, SiteCollectionState[] states);
        bool CheckTeamsExistByTeamsId(string teamsId);

        RemoteSiteCollection GetTeamsNodeBySiteUrl(string url);
        Dictionary<RemoteSiteCollection, List<RemoteSiteCollection>> GetTeamsGroupAndChannelsCollectionBySiteUrls(List<string> siteUrls);
        Dictionary<RemoteSiteCollection, List<RemoteSiteCollection>> GetTeamsGroupAndChannelsCollectionByTeamsAddress(List<string> teamsAddress, bool needChannel = false);
        RemoteSiteCollection GetTeamsNodeByTeamsAddress(string teamsAddress);
        RMSPTreeNode GetSPTeamsNodeByTeamsAddress(string teamsAddress);
        RMSPTreeNode GetTeamsNodeByTeamsId(string teamsId);
        RemoteSiteCollection GetO365TenantIdByName(string name);
        IAsyncEnumerable<List<RMRemoteNode>> GetAllRemoteNodesAsync();
        List<string> GetTeamsIdByContainerId(List<string> containerIds);
        Dictionary<RemoteSiteCollection, List<RemoteSiteCollection>> GetTeamsGroupAndChannelsCollectionByTeamsIds(List<string> teamsId, bool needChannel = false);

        IAsyncEnumerable<List<RMRemoteNode>> GetAllTeamsSiteAsync();

        List<RMRemoteNode> GetAllRemoteSiteCollectionURLsBySource(RMBrowseTreeNodeSourceType type);
        Dictionary<string, List<string>> GetGroupAddressAndRelatedSiteUrlsDic(IEnumerable<string> siteUrls, Dictionary<string, string> teamsIdAddressMapping);
        Dictionary<string, string> GetAllTeamId2TeamNameMapping();
        Dictionary<string ,string> GetAllGoogleDriveName(string searchKey, List<string> scopeIds);
        string GetTenantIdByObjectId(string objectId);
        RMRemoteNode GetGoogleDriveByName(string driveName);
        string GetTenantNameByO365TenantId(string tenantId);
        SearchSiteCollectionLazyLoadResponse SearchSiteCollectionLazyLoad(SearchSiteCollectionLazyLoadRequest condition, bool checkPermission, bool includeOrphenNode = false);

        RemoteNodePara GetRemoteSiteCollectionNodeByUrl(string url);
        List<RemoteSiteCollection> GetRemoteSiteCollectionsByNodeLevel(int nodeLevel);

        Task<(string, string, string)> GetChannelSiteInfoAsync(string siteCollectionUrl);
    }
}