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




namespace AvePoint.Wrapper.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    #endregion

    public interface IAveDiscoveryQuery : IDisposable
    {
        bool CacheItemProperties { get; set; }
        bool SupportIB { get; set; }

        #region Init Web/List/Folder

        void InitDiscoverWeb(AveWebCache webCache, AveWebObject webObj);
        void InitDiscoverList(AveListCache listCache, AveListObject listObj);
        void InitDiscoverFolder(AveFolderCache folderCache, AveItemObject folderObj);

        #endregion

        #region site leve1

        int GetSiteChangedForIB(Guid siteId);
        Dictionary<Guid, AveWebObject> QueryWebForIB(Guid siteId);
        Dictionary<int, AveSiteMemberObject> QuerySiteSecurityForIB(Guid siteId);

        #endregion

        #region web level

        void QueryWebRootFolder(AveListCache listCache, AveItemObject rootFolderObject);
        void QueryListRootFolderForFullDiscover(AveListCache listCache, AveListObject listObject, AveItemObject rootFolderObject);
        Dictionary<int, List<AveSecurityObject>> QueryWebSecurityForIB(Guid siteId, Guid webId);
        Dictionary<Guid, AveListObject> QueryListForIB(Guid siteId, Guid webId);
        Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForIB(Guid siteId, Guid webId);

        #endregion

        #region list level
        int GetListChangedForRecords(Guid webId, Guid listId);
        int GetListChangedCount(Guid webId, Guid listId);
        Dictionary<string, object> GetListChangedItems(Guid webId, Guid listId);
        Dictionary<string, object> GetListChangedItems(Guid webId, Guid listId, DateTime startTime, DateTime endTime);

        Dictionary<string, object> GetListDeletedItems(Guid webId, Guid listId);
        Dictionary<string, object> GetFolderChangedItems(Guid webId, Guid listId, Guid folderId, DateTime startTime, DateTime endTime);
        Dictionary<string, object> GetFolderAndSubFolderChangedItems(Guid webId, Guid listId, Guid folderId, DateTime startTime, DateTime endTime);
        void QueryListRootFolder(AveListCache listCache, AveListObject listObject, AveItemObject rootFolderObject);
        Dictionary<Guid, AveAlertObject> QueryListAlertForIB(Guid siteId, Guid webId, Guid listId);
        Dictionary<Guid, AveViewObject> QueryListViewForIB(Guid siteId, Guid webId, Guid listId);
        Dictionary<int, List<AveSecurityObject>> QueryListSecurityForIB(Guid siteId, Guid webId, Guid listId);
        Dictionary<byte[], AveContentTypeObject> QueryListContentTypeForIB(Guid siteId, Guid webId, Guid listId);
        void QuerySystemListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, List<AveDiscoverExtraItemBaseInfo> extraItems);
        void QueryListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, List<AveDiscoverExtraItemBaseInfo> extraItems);
        
        #endregion

        #region item level

        Dictionary<int, List<AveSecurityObject>> QueryItemSecurityForIB(Guid webId, Guid listId, int itemId);
        void QueryAttachmentByItemObj(Guid siteId, string listRootUrl, AveItemObject itemObj);
        #endregion

        #region FB

        Dictionary<Guid, AveWebObject> QuerySiteWebForFB(Guid siteId, bool includeAppWeb = false);
        AveWebObject QueryRootWeb(Guid siteId);
        AveWebObject QueryWeb(Guid webId);
        Dictionary<Guid, AveWebObject> GetSubWebs(Guid siteId, Guid parentWebId);
        Dictionary<Guid, AveListObject> QueryWebListForFB(Guid siteId, Guid webId, bool throwException = false);
        Dictionary<Guid, AveViewObject> QueryListViewForFB(Guid siteId, Guid webId, Guid listId);
        void QueryAttachment(AveFolderCache folderCache, AveItemObject folderObject);
        void QuerySubFoldersForFB(AveFolderCache folderCache, AveItemObject itemObject);
        void QuerySubFoldersForFB(AveFolderCache folderCache, AveItemObject folderObject, bool includeRecycleBin, bool includeSystemFolder);
        void QueryListItemForFB(AveFolderCache folderCache, AveItemObject folderObject, bool includeRecycleBin, bool includeSystemFolder);
        Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForFB(Guid siteId, Guid webId);
        Dictionary<byte[], AveContentTypeObject> QueryListContentTypeForFB(Guid siteId, Guid webId, Guid listId, string rootFolderUrl, int listType, object flag);
        #endregion

        #region Support Replicator

        string GetListContentTypes(Guid webId, Guid listId);
        AveItemObject GetItemExist(Guid SiteId, Guid webId, Guid listId, Guid id, string dirName, string leafName, bool isListItem);
        AveItemObject GetItemExistForListener(string webServerRelativeUrl, System.Globalization.CultureInfo culture, Guid listId, Guid tpGuid, string dirName, string leafName, bool isListItem);
        DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, Guid id, bool hasDocLibRowId);
        DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, string dirName, string leafName, ref Guid docId);
        DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, Guid tp_Guid, ref Guid docId);
        DateTime GetItemLastModifiedTime(Guid listId, int rowId);
        DateTime GetItemLastModifiedTime(Guid siteId, Guid itemId);


        AveItemObject GetItemVersions(Guid siteId, Guid webId, Guid listId, int docLibRowId);
        Guid GetListItemGuid(Guid webId, Guid listId, Guid tp_Guid, int rowId);
        Guid GetDocIdByTp_Guid(Guid siteId, Guid parentId, Guid tp_Guid);
        Guid GetDocIdByTp_Guid(Guid siteId, Guid webId, Guid listId, Guid parentId, Guid tp_Guid, int rowId);
        bool GetTPGUIDAndDocIdMapping(Guid siteId, Guid webId, Guid listId, Guid parentId, string folderUrl, ref Dictionary<Guid, Guid> itemsMapping, ref Dictionary<Guid, Guid> foldersMapping);
        bool IsHaveSameName(Guid siteId, Guid webId, Guid listId, string dirName, string leafName);
        bool IsListItemHaveSameName(Guid siteId, Guid webId, Guid tpGuid, Guid listId, int rowId);
        List<AveWebPartObject> GetItemWebParts(Guid siteId, Guid webId, Guid listId, Guid itemDocId);
        int GetItemSize(Guid siteId, Guid webId, Guid listId, Guid docId, ref string createdBy, ref string modifiedBy);
        void ClearItemCache();
        void RemoveFolderCache(List<int> folderIds);

        #endregion

        #region Support Extender
        int GetCurrentUIVersion(Guid siteId, Guid parentId, Guid docId);
        #endregion

        void Dispose();

        IAveDiscoveryQuery CloneObjWithNewRequest(object aveRequest);
        void RemoveFolderCache(string folderServerRelativeUrl);
        void RemoveItemCache(int itemId);

        List<AveProjectObject> QueryProjects();

        #region license查询
        long GetWebSize(Guid siteId, Guid webId);
        long GetObjectChangedSize(Guid siteId, Guid webId, Guid listId,string folderPath, DateTime beginTime);       
        long GetListSize(Guid siteId, Guid webId, Guid listid);
        long GetFolderSize(Guid siteId, Guid webId, Guid listId, string folderUrl);


        #endregion

        #region improve memory

        void QuerySubFoldersForFB(AveFolderCache folderCache, AveItemObject folderObject, bool includeSystemFolder);
        void QuerySubItemsForFB(AveFolderCache folderCache, AveItemObject folderObject, bool includeSystemFolder, ref string pageInfo);

        #endregion

        #region DM
        PreDiscoverDesignListResult PreDiscoverDesignList(string siteUrl, Guid webId, Guid listId, bool includeGhostFile = false, bool includeEmptyFolder = false);
        #endregion
        void QueryListRootFolderWithStructure(AveListCache listCache, AveListObject listObject, AveItemObject rootFolderObject);
        void QueryListRootFolderWithStructureForArchiver(AveListCache listCache, AveListObject listObject, AveItemObject rootFolderObject, SPOFolder SPOFolder);
        IEnumerable<int> QuerySubFoldersWithStructureForFB(AveFolderCache folderCache, AveItemObject folderObject, bool includeSystemFolder);
        IEnumerable<int> QuerySubItemsWithStructureForFB(AveFolderCache folderCache, AveItemObject folderObject);
        //IEnumerable<int> QuerySubFoldersWithStructureForArchiverFB(AveFolderCache folderCache, AveItemObject folderObject, bool includeSystemFolder);
        IEnumerable<int> QuerySubItemsWithStructureForArchiverFB(AveFolderCache folderCache, AveItemObject folderObject);
        IEnumerable<int> QuerySubItemsWithStructureForRecordsFB(AveFolderCache folderCache, AveItemObject folderObject);

        string GetListTitle(Guid siteId, Guid webId, Guid listid);
    }
}
