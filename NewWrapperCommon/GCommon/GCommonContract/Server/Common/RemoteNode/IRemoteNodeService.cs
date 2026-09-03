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

using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.GCommon.Contract.Server.Common.RemoteNode
{
    public interface IRemoteNodeService
    {
        string CurrentUserId { get; set; }

        void DeleteRemoteSiteCollection(string id);
        void DeleteRemoteWebApplication(string id);

        void DeleteRemoteSiteCollection(List<string> ids);
        void DeleteRemoteWebApplication(List<string> ids);

        void CreateRemoteSiteCollection(RemoteSiteCollection siteCollection);
        void CreateRemoteSiteCollection(List<RemoteSiteCollection> siteCollections);
        void CreateRemoteSiteCollections(string userId, List<RemoteSiteCollection> siteCollections);
        void CreateRemoteWebApplication(RemoteWebApplication webApplication);
        void CreateGroupScopeRemoteWebApplication(string groupId, RemoteWebApplication webApplication, EntityObjectPermissionType permissonType);
        //List<RemoteSiteCollection> GetAuthorisedRemoteSiteCollections();
        List<string> GetAuthorisedRemoteSiteCollectionIds();
        List<RemoteSiteCollection> GetGlobalRemoteSiteCollections();
        List<RemoteWebApplication> GetRemoteWebApplications();
        List<RemoteSiteCollection> GetAuthorisedRemoteSiteCollections();
        RemoteSiteCollection GetRemoteSiteCollectionById(string siteCollectionId);
        List<RemoteSiteCollection> GetRemoteSiteCollectionsByIdList(List<string> siteCollectionIds);
        RemoteSiteCollection GetRemoteSiteCollectionByUrl(string siteCollectionUrl);
        List<RemoteSiteCollection> GetRemoteSiteCollectionByUrlLikePrefixUrl(string prefixUrl, int MaxCount);

        RemoteWebApplication GetRemoteWebApplicationById(string webApplicationId);
        RemoteWebApplication GetRemoteWebApplicationByUrl(string webApplicationUrl);

        void UpdateRemoteSiteCollection(RemoteSiteCollection siteCollection);
        void UpdateRemoteSiteCollection(List<RemoteSiteCollection> siteCollections);
        void UpdateRemoteWebApplication(RemoteWebApplication webApplication);

        List<RemoteSiteCollection> GetRemoteSiteCollectionsByWebApplication(RemoteWebApplication webApplication);

        List<RemoteSiteCollection> GetAuthorisedRemoteSiteCollectionsByUser(string accountId);

        bool IsRemoteWebApplicationExistByName(string name);

        bool IsRemoteWebApplicationExistByNameList(List<string> nameList);

        bool IsRemoteSiteCollectionExistByUrl(string url);

        List<string> GetNotUseAgentGroup(List<string> agentGroupsIds);

        Dictionary<string, List<RemoteWebApplication>> GetAllRemoteWebApplicationWithGroupId();

        bool IsSiteExistInTenantGroup(string url, string siteGroup, string userId);

        void CreateRemoteSiteCollection(string userId, RemoteSiteCollection siteCollection);

        bool IsUseAppProfiles(List<string> appPrfoileIds);

        bool IsUseOffice365AccountProfiles(List<string> office365AccountProfileIds);
    }
}
