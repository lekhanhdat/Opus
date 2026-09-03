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


using AvePoint.Common.Portal;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.RemoteNode;
using AvePoint.GCommon.Contract.Server.ControlPanel.FilterPolicy.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object;
using AvePoint.GCommon.Utility;
using AvePoint.Media.Service.DomainModel;
using AvePoint.RA.Common;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Services;
using Cloud.Sdk.Data.AosModern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.Common.RemoteNode.Impl
{
    public class RemoteNodeService : RMServiceBase, IRemoteNodeService
    {
        private static AveLogger _logger = AveLogger.GetInstance(typeof(RemoteNodeService));
        private IRMRemoteNodeDao mRMRemoteNodeDao;
        protected IRMRemoteNodeDao RemoteNodeDao
        {
            get
            {
                if (mRMRemoteNodeDao == null)
                {
                    mRMRemoteNodeDao = (IRMRemoteNodeDao)PlatformWindsorManager.GetService(typeof(IRMRemoteNodeDao));
                }
                return mRMRemoteNodeDao;
            }
        }
        //private AveLogger logger = AveLogger.GetInstance(typeof(RemoteNodeService));
        //public IRemoteNodeDao RemoteNodeDao { get; set; }

        public List<RemoteSiteCollection> GetRemoteSiteCollectionsByWebApplication(RemoteWebApplication webApplication)
        {
            throw new NotImplementedException();
        }

        public List<RemoteSiteCollection> GetRemoteSiteCollectionByParam(List<string> param, bool isUrl = true)
        {
            return RemoteNodeDao.GetRemoteSiteCollectionByParam(param, isUrl);
        }
        public async Task<RemoteSiteCollection> GetRemoteNodeFromAosAsync(string o365tenantId, string siteUrlOrName, bool useUrl)
        {
            try
            {
                _logger.Info($"start GetRemoteNodeFromAos,siteUrlOrName:{siteUrlOrName},o365tenantId:{o365tenantId}");
                var _aosApiClient = AosApiUtility.GetAosModerClient();
                RemoteSiteCollection result = null;
                if (useUrl)
                {
                    var remoteResult = await _aosApiClient.RemoteNodeService.QueryRemoteNodesAsync(new RemoteNodesQueryParameter
                    {
                        NodeTypes = new System.Collections.Generic.List<RemoteNodeType>() { RemoteNodeType.SiteCollection, RemoteNodeType.OneDrive},
                        TenantId = o365tenantId
                    });
                    _logger.Info($"this GetRemoteNodeFromAos is sp or od,siteUrlOrName:{siteUrlOrName}");
                    var spResult = remoteResult.SPSites.Where(a => !string.IsNullOrEmpty(a.Url) && a.Url.Equals(siteUrlOrName, StringComparison.OrdinalIgnoreCase))?.FirstOrDefault();
                    var odResult = remoteResult.OneDrives.Where(a => !string.IsNullOrEmpty(a.Url) && a.Url.Equals(siteUrlOrName, StringComparison.OrdinalIgnoreCase))?.FirstOrDefault();
                    if (spResult != null)
                    {
                        return ConvertToRemoteSiteCollection(spResult);
                    }
                    else if (odResult != null)
                    {
                        return ConvertToRemoteSiteCollection(odResult);
                    }
                    else
                    {
                        _logger.Warn($"can not find remote node from aos,siteUrlOrName:{siteUrlOrName}");
                        return null;
                    }
                }
                else //group or teams type
                {
                    _logger.Info($"this GetRemoteNodeFromAos is teams or group,siteUrlOrName:{siteUrlOrName}");

                    var remoteResult = await _aosApiClient.RemoteNodeService.QueryRemoteNodesAsync(new RemoteNodesQueryParameter
                    {
                        NodeTypes = new System.Collections.Generic.List<RemoteNodeType>() { RemoteNodeType.Office365Group },
                        TenantId = o365tenantId
                    });
                    var groupResult = remoteResult.O365Groups.Where(a => a.Name.Equals(siteUrlOrName, StringComparison.OrdinalIgnoreCase))?.FirstOrDefault();
                    if (groupResult != null)
                    {
                        _logger.Info($"this GetRemoteNodeFromAos is teams or group,convert to RemoteSiteCollection,siteUrlOrName:{siteUrlOrName}");
                        result = new RemoteSiteCollection()
                        {
                            TenantId = groupResult.TenantId,
                            ObjectId = groupResult.ObjectId,
                            url = groupResult.SiteUrl,
                            TemplateName = groupResult.TemplateName,
                            AdminUrl = groupResult.AdminUrl,
                            AuthType = (AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType)groupResult.ConnectionType
                        };
                    }
                    return result;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"this GetRemoteNodeFromAos failed,siteUrlOrName:{siteUrlOrName},reason:{e}");
                return null;
            }
        }
        public async Task<RemoteSiteCollection> GetRestoreRemoteNodeFromAosAsync(string o365tenantId, string siteUrlOrName)
        {
            try
            {
                _logger.Info($"start GetRestoreRemoteNodeFromAosAsync,siteUrlOrName:{siteUrlOrName},o365tenantId:{o365tenantId}");
                var _aosApiClient = AosApiUtility.GetAosModerClient();
                RemoteSiteCollection result = null;

                var remoteResult = await _aosApiClient.RemoteNodeService.QueryRemoteNodesAsync(new RemoteNodesQueryParameter
                {
                    NodeTypes = new System.Collections.Generic.List<RemoteNodeType>() { RemoteNodeType.SiteCollection, RemoteNodeType.OneDrive, RemoteNodeType.Office365Group },
                    TenantId = o365tenantId
                });
                _logger.Info($"this GetRestoreRemoteNodeFromAosAsync is sp or od,siteUrlOrName:{siteUrlOrName}");
                var spResult = remoteResult.SPSites.Where(a => !string.IsNullOrEmpty(a.Url) && a.Url.Equals(siteUrlOrName, StringComparison.OrdinalIgnoreCase))?.FirstOrDefault();
                var odResult = remoteResult.OneDrives.Where(a => !string.IsNullOrEmpty(a.Url) && a.Url.Equals(siteUrlOrName, StringComparison.OrdinalIgnoreCase))?.FirstOrDefault();
                var grResult = remoteResult.O365Groups.Where(a => !string.IsNullOrEmpty(a.SiteUrl) && a.SiteUrl.Equals(siteUrlOrName, StringComparison.OrdinalIgnoreCase))?.FirstOrDefault();
                if (spResult != null)
                {
                    return ConvertToRemoteSiteCollection(spResult);
                }
                else if (odResult != null)
                {
                    return ConvertToRemoteSiteCollection(odResult);
                }
                else if (grResult != null)
                {
                    _logger.Info($"this GetRestoreRemoteNodeFromAosAsync is teams or group,convert to RemoteSiteCollection,siteUrlOrName:{siteUrlOrName}");
                    result = new RemoteSiteCollection()
                    {
                        TenantId = grResult.TenantId,
                        ObjectId = grResult.ObjectId,
                        url = grResult.SiteUrl,
                        TemplateName = grResult.TemplateName,
                        AdminUrl = grResult.AdminUrl,
                        AuthType = (AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType)grResult.ConnectionType
                    };
                    return result;
                }
                else
                {
                    _logger.Info($"this GetRestoreRemoteNodeFromAosAsync can not find from aos,siteUrlOrName:{siteUrlOrName}");
                    return null;
                }
            }
            catch (Exception e)
            {
                _logger.Error($"this GetRemoteNodeFromAos failed,siteUrlOrName:{siteUrlOrName},reason:{e}");
                return null;
            }
        }
        private RemoteSiteCollection ConvertToRemoteSiteCollection(SiteRemoteNode node)
        {
            var result = new RemoteSiteCollection()
            {
                TenantId = node.TenantId,
                ObjectId = node.ObjectId,
                url = node.Url,
                TemplateName = node.TemplateName,
                AdminUrl = node.AdminUrl,
                AuthType = (AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType)node.ConnectionType
            };
            return result;
        }
        public void DeleteRemoteSiteCollection(List<string> ids)
        {
            throw new NotImplementedException();
        }

        public void DeleteRemoteWebApplication(List<string> ids)
        {
            throw new NotImplementedException();
        }

        public void DeleteRemoteWebApplicationRuleAlliance(List<string> ids)
        {
            throw new NotImplementedException();
        }

        public void DeleteRemoteSiteCollectionsByUrl(List<string> urls)
        {
            throw new NotImplementedException();
        }

        public void DeleteRemoteSiteCollectionsByName(List<string> names)
        {
            throw new NotImplementedException();
        }

        public void DeleteRemoteSiteCollectionByParentId(List<string> parentIds)
        {
            throw new NotImplementedException();
        }

        public void CreateRemoteSiteCollection(RemoteSiteCollection siteCollection)
        {
            throw new NotImplementedException();
        }

        public void CreateGroupScopeRemoteWebApplication(string groupId, RemoteWebApplication webApplication, EntityObjectPermissionType permissonType)
        {
            throw new NotImplementedException();
        }

        public List<RemoteSiteCollection> GetAuthorisedRemoteSiteCollections()
        {
            throw new NotImplementedException();
        }

        public List<RemoteWebApplication> GetRemoteWebApplications()
        {
            throw new NotImplementedException();
        }

        public RemoteSiteCollection GetRemoteSiteCollectionById(string siteCollectionId, bool isNeedDecrypt = true)
        {
            throw new NotImplementedException();
        }

        public RemoteSiteCollection GetRemoteSiteCollectionByUrl(string siteCollectionUrl, bool isNeedDecrypt = true)
        {
            throw new NotImplementedException();
        }

        public RemoteWebApplication GetRemoteWebApplicationById(string webApplicationId)
        {
            throw new NotImplementedException();
        }

        public void UpdateRemoteSiteCollection(RemoteSiteCollection siteCollection)
        {
            throw new NotImplementedException();
        }

        public void UpdateRemoteWebApplication(RemoteWebApplication webApplication)
        {
            throw new NotImplementedException();
        }

        public void UpdateSiteCollectionForState(RemoteSiteCollection siteCollection)
        {
            throw new NotImplementedException();
        }

        public bool IsSitecollectionExistByUrl(string url)
        {
            throw new NotImplementedException();
        }

        public List<RemoteSiteCollection> GetAuthorisedRemoteSiteCollectionsByUser(string accountId, bool isUserInGroup = false)
        {
            throw new NotImplementedException();
        }

        public List<string> GetAuthorisedRemoteSiteCollectionIdsByUser(string accountId)
        {
            throw new NotImplementedException();
        }

        public bool IsRemoteWebApplicationExistByNameForUpdate(string name, string id)
        {
            throw new NotImplementedException();
        }

        public bool IsRemoteSiteCollectionExistByUrl(string url)
        {
            throw new NotImplementedException();
        }

        public List<RemoteWebApplication> GetAuthorisedAllSiteGroups(bool includeO365 = false, bool includePrivateChannel = false)
        {
            throw new NotImplementedException();
        }

        public List<RemoteSiteCollection> GetAuthorisedAllSites()
        {
            throw new NotImplementedException();
        }

        public List<string> GetAuthorisedAllSitesIds()
        {
            throw new NotImplementedException();
        }

        public List<RemoteWebApplication> GetAuthorisedSkyDriveProGroups()
        {
            throw new NotImplementedException();
        }

        public List<RemoteSiteCollection> GetAuthorisedSkyDrivePros()
        {
            throw new NotImplementedException();
        }

        public List<RemoteWebApplication> GetAuthorisedOffice365GroupSitesGroups()
        {
            throw new NotImplementedException();
        }

        public List<RemoteSiteCollection> GetAuthorisedOffice365GroupSites()
        {
            throw new NotImplementedException();
        }

        public List<string> GetO365GroupSiteUrlsByNames(List<string> names)
        {
            throw new NotImplementedException();
        }

        public bool IsSkyDriveProExistInGroup(string SkyDriveProName)
        {
            throw new NotImplementedException();
        }

        public bool IsOffice365GroupSitesGroupExistByNameForUpdate(string name, string id)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, RemoteSiteCollection> GetRemoteSiteCollectionByIds(List<string> ids)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, RemoteSiteCollection> GetSiteCollectionByUrls(List<string> urls)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetRemoteSiteCollectionParentIDByIds(List<string> ids)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, RemoteWebApplication> GetRemoteWebApplicationsBySCUrl(List<string> scUrls)
        {
            throw new NotImplementedException();
        }

        public void SyncRemoteSiteCollections(List<RemoteSiteCollection> siteCollections)
        {
            throw new NotImplementedException();
        }

        public void CreateRemoteWebApplications(List<RemoteWebApplication> webApps)
        {
            throw new NotImplementedException();
        }

        public List<RemoteNodePara> GetRemoteWebApplicationNodes()
        {
            throw new NotImplementedException();
        }

        public List<SyncRemoteNodePara> GetAllSiteCollectionNodes()
        {
            throw new NotImplementedException();
        }

        public void UpdateSyncSiteCollections(List<SyncRemoteNodePara> siteCollections)
        {
            throw new NotImplementedException();
        }

        public List<RemoteSiteCollection> GetAvailableSiteCollectionsByParent(string parentId)
        {
            throw new NotImplementedException();
        }

        public List<RemoteSiteCollection> GetAuthorisedAvailableSiteCollectionsByParent(string parentId)
        {
            throw new NotImplementedException();
        }

        public List<RemoteSiteCollection> GetAdminCenterRemoteSiteCollections()
        {
            throw new NotImplementedException();
        }

        public List<string> GetGroupIDandSCID(List<string[]> list)
        {
            throw new NotImplementedException();
        }

        public List<RemoteSiteCollection> GetRemoteSiteCollectionBySiteUrls(List<string> urls)
        {
            throw new NotImplementedException();
        }

        public List<NodeCollection> GetNodeCollectionByUrls(List<string> urls)
        {
            throw new NotImplementedException();
        }

        public List<NodeCollection> GetNodeCollectionByParentId(string id)
        {
            throw new NotImplementedException();
        }

        public List<RemoteWebApplication> GetGroupsByIds(List<string> ids)
        {
            throw new NotImplementedException();
        }

        public List<string> GetRemoteSitecollectionUrlsByParantIds(List<string> parentIds)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, List<NodeCollection>> GetSiteCollectionByParentIds(List<string> ids)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetNodeIdByUrls(List<string> urls)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetWebapplicationBySiteCollectionUrls(List<string> urls)
        {
            throw new NotImplementedException();
        }

        public RemoteWebApplication GetTop1RemoteWebApplication()
        {
            throw new NotImplementedException();
        }

        public List<RemoteSiteCollection> GetSiteCollectionByTenantId()
        {
            throw new NotImplementedException();
        }

        public List<string> GetSiteCollectionIdsByGroupIds(List<string> groupIds)
        {
            throw new NotImplementedException();
        }

        public List<RemoteSiteCollection> GetUnDecryptRemoteSiteCollectionByIds(List<string> ids)
        {
            throw new NotImplementedException();
        }

        public RemoteNodePara GetGroupByNameAndNodeLevel(string name, int nodeLevel)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, BposConnectionType> GetRemoteSiteCollectionConnectionTypeBySiteUrls(List<string> urls)
        {
            throw new NotImplementedException();
        }

        public bool IsOffice365GroupSiteExistInGroup(string groupSiteName)
        {
            throw new NotImplementedException();
        }

        public List<string> GetO365GroupSiteUrls()
        {
            throw new NotImplementedException();
        }

        public void UpdateSiteCollectionUrlByGroupName(string siteCollectionUrl, string groupName)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, long> GetNodeCreateTimeByIds(List<string> ids)
        {
            throw new NotImplementedException();
        }

        public List<SyncRemoteNodePara> GetAllPrivateChannel()
        {
            throw new NotImplementedException();
        }

        public bool IsPrivateChannelGroupExist()
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, List<string>> GetO365GroupSiteName2UrlDicByNames(List<string> names)
        {
            throw new NotImplementedException();
        }

        public void UpdateO365GroupSiteByNames(List<RemoteSiteCollection> o365GroupSiteCollections)
        {
            throw new NotImplementedException();
        }

        public List<string> GetPrivatePrivateChannelByGroupTeamSiteContainerIds(List<string> groupTeamSiteContainerIds)
        {
            throw new NotImplementedException();
        }

        public Dictionary<string, string> GetTeamId2TeamNameDicByTeamIds(List<string> teamIds)
        {
            throw new NotImplementedException();
        }

        public List<RemoteSiteCollection> GetAuthorisedRemoteSiteCollectionsBySecondParentId(string secondeParentId)
        {
            throw new NotImplementedException();
        }

        public void UpdateSiteCollectionSecondParentId(List<SyncRemoteNodePara> siteCollections)
        {
            throw new NotImplementedException();
        }
    }
}
