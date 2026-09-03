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



namespace AvePoint.GCommon.Contract.Server.ControlPanel.Office365
{
    using System;
    using System.Collections.Generic;
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Contract.Tree.Object;

    public interface IMOffice365Service
    {
        RemoteWebApplication GetRemoteWebApplicationById(String id);
        List<RemoteWebApplication> GetAllRemoteWebApplication();
        List<RemoteSiteCollection> GetRemoteSiteCollectionByParentId(String parentId, SiteCollectionState[] states);
        RemoteSiteCollection GetRemoteSiteCollection(string id, bool isNeedDecrypt = true);
        RemoteSiteCollection GetRemoteSiteCollectionByUrl(string siteCollectionUrl, bool isNeedDecrypt = true);
        List<RemoteWebApplication> GetAuthorisedAllSiteGroups(bool includeO365 = false, bool includePrivateChannel = false);
        BposInfo GetBposeAccountBySiteUrl(string siteUrl);
        List<string> GetGroupIDandSCID(List<string[]> list);
        List<RemoteSiteCollection> GetRemoteSiteCollectionBySiteUrls(List<string> urls);
        List<RemoteWebApplication> GetGroupsByIds(List<string> ids);
        Dictionary<string, string> GetTeamId2TeamNameDicByTeamIds(List<string> teamIds);
    }
}
