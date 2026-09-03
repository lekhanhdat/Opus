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

namespace AvePoint.Wrapper.Discovery
{
    using System;
    using System.Collections.Generic;
    using AvePoint.Common.FilterEngine;
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.Wrapper.Common;

    public interface IAveDiscoverSite: IAveDiscoverFilterBase,IDisposable
    {
        ChangeType ChangeType { get; }
        ChangeType GroupChangeType { get; }
        ChangeType UserChangeType { get; }
        List<AveSiteMemberObject> GetChangeMembers();
        Dictionary<Guid, IAveDiscoverWeb> GetChangeWebs();
        [Obsolete("Please use DiscoverList constructor.")]
        IAveDiscoverList GetDiscoverList(IAveSite site, IAveWeb web, string listUrl);
        Guid GetDocIdByTp_Guid(Guid siteId, Guid webId, Guid listId, Guid parentId, Guid tp_Guid, int rowId);
        IAveDiscoverFolder GetFolderExist(Guid webId, Guid listId, Guid parentId, Guid id, string listRootFolder, string dirName, string leafName, bool isListItem, IAveDiscoverFolder discoverFolder = null);
        IAveDiscoverItem GetItemExist(Guid webId, Guid listId, Guid parentId, Guid id, string listRootFolder, string dirName, string leafName, bool isListItem, IAveDiscoverList discoverList, IAveDiscoverFolder discoverFolder = null);
        [Obsolete("Will Remove")]
        IAveDiscoverItem GetItemExist(Guid webId, Guid listId, Guid parentId, Guid id, string listRootFolder, string dirName, string leafName, bool isListItem, IAveDiscoverFolder discoverFolder = null);
        DateTime GetItemLastModifiedTime(Guid siteId, Guid listId, int rowId);
        DateTime GetItemLastModifiedTime(Guid siteId, Guid itemId);
        DateTime GetItemLastModifiedTime(Guid webId, Guid listId, string dirName, string leafName, ref Guid docId);
        int GetItemSizeAndUserInfo(Guid webId, Guid listId, Guid docId, int level, ref string createdBy, ref string modifiedBy);
        IAveDiscoverItem GetItemVersions(Guid webId, Guid listId, int docLibRowId);
        List<AveWebPartObject> GetItemWebParts(Guid webId, Guid listId, Guid itemDocId);
        IAveDiscoverWeb GetRootWeb();
        void GetSiteChanged();
        long GetSiteSize();
        Dictionary<Guid, Guid> GetTPGUIDAndDocIdMapping(Guid siteId, Guid webId, Guid listId, Guid parentId, string folderUrl);
        Dictionary<Guid, IAveDiscoverWeb> GetWebs();
        Dictionary<Guid, IAveDiscoverWeb> GetAllWebs();
        bool IsHaveSameName(Guid webId, Guid listId, string dirName, string leafName);
        bool IsListItemHaveSameName(Guid webId, Guid tpGuid, Guid listId, int rowId);
        Guid SiteID { get; }
        bool SupportIB { get; }
        Guid WebApplicationId { get; }
    }
}
