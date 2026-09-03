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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.ServiceModel;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.Server.Common.RemoteNode
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IRemoteNodeService
    {
        [OperationContract]
        void DeleteRemoteSiteCollection(List<string> ids);
        [OperationContract]
        void DeleteRemoteWebApplication(List<string> ids);
        [OperationContract]
        void DeleteRemoteWebApplicationRuleAlliance(List<string> ids);
        [OperationContract]
        void DeleteRemoteSiteCollectionsByUrl(List<string> urls);
        [OperationContract]
        void DeleteRemoteSiteCollectionsByName(List<string> names);
        [OperationContract]
        void DeleteRemoteSiteCollectionByParentId(List<string> parentIds);
        [OperationContract]
        void CreateRemoteSiteCollection(RemoteSiteCollection siteCollection);
        [OperationContract]
        void CreateGroupScopeRemoteWebApplication(string groupId, RemoteWebApplication webApplication, EntityObjectPermissionType permissonType);
        [OperationContract]
        List<RemoteSiteCollection> GetAuthorisedRemoteSiteCollections();
        [OperationContract]
        List<RemoteWebApplication> GetRemoteWebApplications();
        [OperationContract]
        RemoteSiteCollection GetRemoteSiteCollectionById(string siteCollectionId, bool isNeedDecrypt = true);
        [OperationContract]
        RemoteSiteCollection GetRemoteSiteCollectionByUrl(string siteCollectionUrl, bool isNeedDecrypt = true);
        [OperationContract]
        RemoteWebApplication GetRemoteWebApplicationById(string webApplicationId);
        [OperationContract]
        List<RemoteSiteCollection> GetRemoteSiteCollectionByParam(List<string> param, bool isUrl = true);
        [OperationContract]
        void UpdateRemoteSiteCollection(RemoteSiteCollection siteCollection);
        [OperationContract]
        void UpdateRemoteWebApplication(RemoteWebApplication webApplication);
        [OperationContract]
        void UpdateSiteCollectionForState(RemoteSiteCollection siteCollection);
        [OperationContract]
        bool IsSitecollectionExistByUrl(string url);
        [OperationContract]
        List<RemoteSiteCollection> GetRemoteSiteCollectionsByWebApplication(RemoteWebApplication webApplication);
        [OperationContract]
        List<RemoteSiteCollection> GetAuthorisedRemoteSiteCollectionsByUser(string accountId, bool isUserInGroup = false);
        [OperationContract]
        List<string> GetAuthorisedRemoteSiteCollectionIdsByUser(string accountId);
        [OperationContract]
        bool IsRemoteWebApplicationExistByNameForUpdate(string name, string id);
        [OperationContract]
        bool IsRemoteSiteCollectionExistByUrl(string url);
        [OperationContract]
        List<RemoteWebApplication> GetAuthorisedAllSiteGroups(bool includeO365 = false, bool includePrivateChannel = false);
        [OperationContract]
        List<RemoteSiteCollection> GetAuthorisedAllSites();
        [OperationContract]
        List<string> GetAuthorisedAllSitesIds();
        [OperationContract]
        List<RemoteWebApplication> GetAuthorisedSkyDriveProGroups();
        [OperationContract]
        List<RemoteSiteCollection> GetAuthorisedSkyDrivePros();
        [OperationContract]
        List<RemoteWebApplication> GetAuthorisedOffice365GroupSitesGroups();
        [OperationContract]
        List<RemoteSiteCollection> GetAuthorisedOffice365GroupSites();
        [OperationContract]
        List<string> GetO365GroupSiteUrlsByNames(List<string> names);
        [OperationContract]
        bool IsSkyDriveProExistInGroup(string SkyDriveProName);
        [OperationContract]
        bool IsOffice365GroupSitesGroupExistByNameForUpdate(string name, string id);
        /// <summary>
        /// 更新Tree的UserAccountInfo使用
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        [OperationContract]
        Dictionary<string, RemoteSiteCollection> GetRemoteSiteCollectionByIds(List<string> ids);
        [OperationContract]
        Dictionary<string, RemoteSiteCollection> GetSiteCollectionByUrls(List<string> urls);
        [OperationContract]
        Dictionary<string, string> GetRemoteSiteCollectionParentIDByIds(List<string> ids);
        [OperationContract]
        Dictionary<string, RemoteWebApplication> GetRemoteWebApplicationsBySCUrl(List<string> scUrls);
        [OperationContract]
        void SyncRemoteSiteCollections(List<RemoteSiteCollection> siteCollections);
        [OperationContract]
        void CreateRemoteWebApplications(List<RemoteWebApplication> webApps);
        [OperationContract]
        List<RemoteNodePara> GetRemoteWebApplicationNodes();
        [OperationContract]
        List<SyncRemoteNodePara> GetAllSiteCollectionNodes();
        [OperationContract]
        void UpdateSyncSiteCollections(List<SyncRemoteNodePara> siteCollections);
        [OperationContract]
        List<RemoteSiteCollection> GetAvailableSiteCollectionsByParent(string parentId);
        [OperationContract]
        List<RemoteSiteCollection> GetAuthorisedAvailableSiteCollectionsByParent(string parentId);
        [OperationContract]
        List<RemoteSiteCollection> GetAdminCenterRemoteSiteCollections();
        [OperationContract]
        List<string> GetGroupIDandSCID(List<string[]> list);
        [OperationContract]
        List<RemoteSiteCollection> GetRemoteSiteCollectionBySiteUrls(List<string> urls);
        [OperationContract]
        List<NodeCollection> GetNodeCollectionByUrls(List<string> urls);
        [OperationContract]
        List<NodeCollection> GetNodeCollectionByParentId(string id);
        [OperationContract]
        List<RemoteWebApplication> GetGroupsByIds(List<string> ids);
        [OperationContract]
        List<string> GetRemoteSitecollectionUrlsByParantIds(List<string> parentIds);
        [OperationContract]
        Dictionary<string, List<NodeCollection>> GetSiteCollectionByParentIds(List<string> ids);
        [OperationContract]
        Dictionary<string, string> GetNodeIdByUrls(List<string> urls);
        [OperationContract]
        Dictionary<string, string> GetWebapplicationBySiteCollectionUrls(List<string> urls);
        [OperationContract]
        RemoteWebApplication GetTop1RemoteWebApplication();
        [OperationContract]
        List<RemoteSiteCollection> GetSiteCollectionByTenantId();
        [OperationContract]
        List<string> GetSiteCollectionIdsByGroupIds(List<string> groupIds);
        [OperationContract]
        List<RemoteSiteCollection> GetUnDecryptRemoteSiteCollectionByIds(List<string> ids);
        [OperationContract]
        RemoteNodePara GetGroupByNameAndNodeLevel(string name, int nodeLevel);
        [OperationContract]
        Dictionary<string, BposConnectionType> GetRemoteSiteCollectionConnectionTypeBySiteUrls(List<string> urls);
        [OperationContract]
        bool IsOffice365GroupSiteExistInGroup(string groupSiteName);
        [OperationContract]
        List<string> GetO365GroupSiteUrls();
        [OperationContract]
        void UpdateSiteCollectionUrlByGroupName(string siteCollectionUrl, string groupName);
        [OperationContract]
        Dictionary<string, long> GetNodeCreateTimeByIds(List<string> ids);

        [OperationContract]
        List<SyncRemoteNodePara> GetAllPrivateChannel();
        [OperationContract]
        bool IsPrivateChannelGroupExist();
        [OperationContract]
        Dictionary<string, List<string>> GetO365GroupSiteName2UrlDicByNames(List<string> names);
        [OperationContract]
        void UpdateO365GroupSiteByNames(List<RemoteSiteCollection> o365GroupSiteCollections);
        [OperationContract]
        List<string> GetPrivatePrivateChannelByGroupTeamSiteContainerIds(List<string> groupTeamSiteContainerIds);
        [OperationContract]
        Dictionary<string, string> GetTeamId2TeamNameDicByTeamIds(List<string> teamIds);
        [OperationContract]
        List<RemoteSiteCollection> GetAuthorisedRemoteSiteCollectionsBySecondParentId(string secondeParentId);
        [OperationContract]
        void UpdateSiteCollectionSecondParentId(List<SyncRemoteNodePara> siteCollections);
        [OperationContract]
        Task<RemoteSiteCollection> GetRemoteNodeFromAosAsync(string o365tenantId, string siteUrlOrName, bool useUrl);
        [OperationContract]
        Task<RemoteSiteCollection> GetRestoreRemoteNodeFromAosAsync(string o365tenantId, string siteUrlOrName);
    }
}
