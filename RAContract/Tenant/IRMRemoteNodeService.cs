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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.RA.Contract.Aos.Notification;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.Discovery.Model.PlanProfile;
using AvePoint.RA.Contract.Object;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Tenant
{
    public interface IRMRemoteNodeService
    {
        string CreateSyncAllNodesJob();
        string CreateSyncNodesJob(bool isInitJob = false);
        string RealRunSyncNodesJob(JobQueueDto jqDto);
        List<RemoteSiteCollection> GetRemoteSiteCollectionByParam(List<string> param, bool isUrl = true);
        void DeleteRemoteWebApplication(List<string> ids);
        
        void DeleteRemoteSiteCollectionsByUrl(List<string> urls);
        
        void DeleteRemoteSiteCollectionByParentId(List<string> parentIds);
        
        List<string> GetO365GroupSiteUrlsByNames(List<string> names);
        
        void SyncRemoteSiteCollections(List<RemoteSiteCollection> siteCollections);
        
        void CreateRemoteWebApplications(List<RemoteWebApplication> webApps);

        void UpdateRemoteWebApplications(List<RemoteWebApplication> webApplications);


        List<RemoteNodePara> GetRemoteWebApplicationNodes();

        Dictionary<string, string> GetContainerNameBySiteUrls(IEnumerable<string> urls);

        List<SyncRemoteNodePara> GetAllSiteCollectionNodesByPage(int pageIndex, int pageSize);

        int GetRemoteNodesCount();

        void UpdateSyncSiteCollections(List<SyncRemoteNodePara> siteCollections);
        
        List<RemoteSiteCollection> GetRemoteSiteCollectionBySiteUrls(List<string> urls);

        Dictionary<string, string> GetTeamId2TeamNameDicByTeamIds(List<string> teamIds);

        RemoteNodePara GetGroupByNameAndNodeLevel(string name, int nodeLevel);

        RemoteNodePara GetGroupByAosIdAndNodeLevel(string aosId, int nodeLevel);

        HashSet<string> GetO365GroupSiteByUrls(List<string> urls);

        Dictionary<string, List<string>> GetO365GroupSiteName2UrlDicByNames(List<string> names);

        void UpdateO365GroupSiteByUrls(List<RemoteSiteCollection> o365GroupSiteCollections);

        void UpdateSiteCollectionSecondParentId(List<SyncRemoteNodePara> siteCollections);

        Dictionary<string, List<NodeCollection>> GetSiteCollectionByParentIds(List<string> ids);

        List<RemoteSiteCollection> GetRemoteSiteCollectionsByParentId(string parentId, SiteCollectionState[] states);

        List<RemoteSiteCollection> GetRemoteSiteCollectionsByParentId(string parentId, SiteCollectionState[] states, SiteCollectionType[] types, string[] names = null);

        Task<RMSPSampleTreeNode> GetSiteCollectionsAsync(RMSPSampleTreeNode node, bool checkPermission, bool includeOrphanNode = false);

        RMSPSampleTreeNode GetSiteCollectionsUnderTeamsAsync(RMSPSampleTreeNode node);

        List<Guid> GetOrphanedODIds();
        bool IsPrivateChannelGroupExist();

        List<SyncRemoteNodePara> GetAllPrivateChannelByPage(int pageIndex, int pageSize);

        List<SyncRemoteNodePara> GetAllPrivateChannel();

        List<string> GetPrivateChannelByGroupTeamSiteContainerIds(List<string> groupTeamSiteContainerIds);

        RemoteSiteCollection GetRemoteSiteCollectionById(string id);

        RemoteSiteCollection GetRemoteSiteCollectionByObjectId(string id);

        List<RemoteSiteCollection> GetRemoteSiteCollectionByIds(List<string> ids);

        RemoteSiteCollection GetRemoteSiteCollectionByUrl(string url);

        RemoteSiteCollection GetRemoteSiteCollectionByListUrl(string listUrl);

        List<RemoteSiteCollection> GetAllRemoteSiteCollections();

        PagedSiteCollectionResponse GetAllRemoteSiteCollections(int pageIndex, int pageSize, string key);

        List<RemoteWebApplication> GetAllWebApplications(RMBrowseTreeNodeSourceType type = RMBrowseTreeNodeSourceType.All);

        List<RemoteWebApplication> GetWebApplications();

        Task<RMSPSampleTreeNode> GetWebApplicationsAsync(RMSPSampleTreeNode node, RMBrowseTreeNodeSourceType type, bool checkPermission);
        Task<RMSPSampleTreeNode> GetWebApplicationsForSearchAsync(RMSPSampleTreeNode node, RMBrowseTreeNodeSourceType type, bool checkPermission);
        Task<RMSPSampleTreeNode> GetWebApplicationsOnlyForSearchAsync(RMSPSampleTreeNode node, RMBrowseTreeNodeSourceType type, bool checkPermission);
        Task<RMSPSampleTreeNode> GetWebApplicationsForExactlySearchAsync(RMSPSampleTreeNode node, RMBrowseTreeNodeSourceType type, bool checkPermission, bool includeOrphanNode);
        Task<RMSPSampleTreeNode> GetWebApplicationsOnlyForExactlySearchAsync(RMSPSampleTreeNode node, RMBrowseTreeNodeSourceType type, bool checkPermission, bool includeOrphanNode);

        RemoteWebApplication GetWebApplicationById(string id);

        string GetContainerIdByName(string containerName, int nodeLevel);

        List<RemoteWebApplication> GetRemoteWebApplications();

        List<RemoteWebApplication> GetAuthorisedSkyDriveProGroups();

        List<RemoteWebApplication> GetAuthorisedOffice365GroupSitesGroups();

        List<RemoteWebApplication> GetAuthorisedPrivateChannelSitesGroups();

        List<RemoteWebApplication> GetAuthorisedAllSiteGroups(bool includeO365 = false, bool includePrivateChannel = false);

        Task<RMRemoteSiteCollectionPageInfo> GetMappedSitesPagedAsync(RMRemoteSiteCollectionPageRequest request);
        bool IsRemoteSiteExist();

        bool ValidOrphenSiteCollection(RMSPTreeNode siteCollectionNode);

        List<RemoteSiteCollection> GetRemoteSiteCollectionsByTeamsId(string teamsId, SiteCollectionState[] states);

        RemoteSiteCollection GetTeamsNodeByTeamsAddress(string teamsAddress);

        RMSPTreeNode GetTeamsNodeByTeamsId(string teamsId);

        (RemoteSiteCollection, List<RemoteSiteCollection>) GetTeamsGroupAndChannelsCollectionByTeamsId(string teamsId, bool needChannel = false);
        Task<SearchSiteCollectionLazyLoadResponse> SearchSiteCollectionLazyLoad(SearchSiteCollectionLazyLoadRequest condition, bool checkPermission);
        List<RemoteSiteCollection> GetRemoteSiteCollectionsByNodeLevel(int nodeLevel);

        Task<(string, string, string)> GetChannelSiteInfoAsync(string siteCollectionUrl);
    }
}
