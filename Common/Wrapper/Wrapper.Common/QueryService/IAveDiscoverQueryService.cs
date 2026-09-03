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



using System;
using System.Collections.Generic;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public interface IAveDiscoverQueryService : IAveQueryService
    {
        #region Discover

        void InitDiscoverFolder(AveFolderCache folderCache, AveItemObject folderObj, Dictionary<string, AveItemObject> noPropertyFolders, ref AveListObject listObject, AveDiscoverReader discoverReader);

        void InitDiscoverList(AveListCache listCache, AveListObject listObj);

        void InitDiscoverWeb(AveWebCache webCache, AveWebObject webObj);

        string GetListContentTypes(Guid webId, Guid listId);

        #region For Replicator

        AveItemObject GetItemExist(Guid SiteId, Guid webId, Guid listId, Guid id, string dirName, string leafName, bool isListItem, AveDiscoverReader discoverReader);

        DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, Guid id, bool hasDocLibRowId);

        DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, string dirName, string leafName, ref Guid docId);

        DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, Guid tp_Guid, ref Guid docId);

        DateTime GetItemLastModifiedTime(Guid listId, int rowId);
        DateTime GetItemLastModifiedTime(Guid siteId, Guid itemId);

        AveItemObject GetItemVersions(Guid siteId, Guid webId, Guid listId, int docLibRowId);

        Guid GetListItemGuid(Guid webId, Guid listId, Guid tp_Guid, int rowId);

        Guid GetDocIdByTp_Guid(Guid siteId, Guid parentId, Guid tp_Guid);

        bool GetTPGUIDAndDocIdMapping(Guid siteId, Guid parentId, ref Dictionary<Guid, Guid> itemsMapping, ref Dictionary<Guid, Guid> foldersMapping);

        bool IsHaveSameName(Guid siteId, Guid webId, Guid listId, string dirName, string leafName);

        bool IsListItemHaveSameName(Guid siteId, Guid webId, Guid tpGuid, Guid listId, int rowId);

        List<AveWebPartObject> GetItemWebParts(Guid siteId, Guid webId, Guid listId, Guid itemDocId);

        int GetItemSize(Guid siteId, Guid webId, Guid listId, Guid docId, ref string createdBy, ref string modifiedBy);

        #endregion

        #region For Extender
        int GetCurrentUIVersion(Guid siteId, Guid parentId, Guid docId);
        #endregion

        #region FB

        Dictionary<Guid, AveWebObject> QuerySiteWebForFB(Guid siteId);

        AveWebObject QueryRootWeb(Guid siteId);

        Dictionary<Guid, AveWebObject> GetSubWebs(Guid siteId, Guid parentWebId);

        Dictionary<Guid, AveListObject> QueryWebListForFB(Guid siteId, Guid webId);

        Dictionary<Guid, AveViewObject> QueryListViewForFB(Guid siteId, Guid webId, Guid listId);

        void QueryAttachment(AveFolderCache folderCache, AveItemObject item, AveListObject listObject, AveDiscoverReader discoverReader);

        void QuerySubFoldersForFB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, AveDiscoverReader discoverReader);

        void QueryListItemForFB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, AveDiscoverReader discoverReader, bool includeRecycleBin);

        void QueryStubItemForFB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, AveDiscoverReader discoverReader, bool includeRecycleBin);

        int GetAllStubCount(AveFolderCache folderCache, AveItemObject folderObject);

        Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForFB(Guid siteId, Guid webId);

        Dictionary<byte[], AveContentTypeObject> QueryListContentTypeForFB(Guid siteId, Guid webId, Guid listId, string rootFolderUrl, int listType, object flag, ref AveListObject listObject);

        #endregion

        #region Item Level

        Dictionary<int, List<AveSecurityObject>> QueryItemSecurityForIB(Guid webId, Guid listId, int itemId, DateTime startTime, DateTime endTime);
        void QueryItemAttachment(Guid siteId, string listRootUrl, AveItemObject itemObj, AveDiscoverReader discoverReader);

        #endregion

        #region List Level

        IAveQueryDataReader QueryListRootFolder(AveListCache listCache, string itemColumns);

        Dictionary<Guid, AveAlertObject> QueryListAlertForIB(Guid siteId, Guid webId, Guid listId, DateTime startTime, DateTime endTime);

        Dictionary<Guid, AveViewObject> QueryListViewForIB(Guid siteId, Guid webId, Guid listId, DateTime startTime, DateTime endTime);

        Dictionary<int, List<AveSecurityObject>> QueryListSecurityForIB(Guid siteId, Guid webId, Guid listId, DateTime startTime, DateTime endTime);

        Dictionary<byte[], AveContentTypeObject> QueryListContentTypeForIB(Guid siteId, Guid webId, Guid listId, DateTime startTime, DateTime endTime);

        void QuerySystemListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, DateTime startTime, DateTime endTime, AveListObject listObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders, List<AveDiscoverExtraItemBaseInfo> extraItems);

        void QueryListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, DateTime startTime, DateTime endTime, AveListObject listObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders, List<AveDiscoverExtraItemBaseInfo> extraItems);      

        #endregion

        #region Web Level

        void QueryWebRootFolder(AveListCache listCache, AveItemObject rootFolderObject, ref AveListObject listObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders);

        Dictionary<int, List<AveSecurityObject>> QueryWebSecurityForIB(Guid siteId, Guid webId, DateTime startTime, DateTime endTime);

        Dictionary<Guid, AveListObject> QueryListForIB(Guid siteId, Guid webId, DateTime startTime, DateTime endTime);

        Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForIB(Guid siteId, Guid webId, DateTime startTime, DateTime endTime);

        #endregion

        #region Site Level

        int GetSiteChangedForIB(Guid siteId, DateTime startTime, DateTime endTime);

        Dictionary<Guid, AveWebObject> QueryWebForIB(Guid siteId, DateTime startTime, DateTime endTime);

        Dictionary<int, AveSiteMemberObject> QuerySiteSecurityForIB(Guid siteId, DateTime startTime, DateTime endTime);

        #endregion

        #endregion
#region License查询
        long GetWebSize(Guid siteId, Guid webId);
        long GetObjectChangedSize(Guid siteId, Guid webId, Guid listId, string folderPath, DateTime beginTime); 
      
        long GetListSize(Guid siteId, Guid webId, Guid listid);

        long GetFolderSize(Guid siteId, Guid webId, Guid listId,string folderUrl);
#endregion

    }
}
