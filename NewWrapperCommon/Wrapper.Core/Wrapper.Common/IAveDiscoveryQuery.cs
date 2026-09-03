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
        bool SupportIB { get; set; }

        #region Init Web/List/Folder

        void InitDiscoverWeb(AveWebCache webCache, AveWebObject webObj);
        void InitDiscoverList(AveListCache listCache, AveListObject listObj);
        void InitDiscoverFolder(AveFolderCache folderCache, AveItemObject folderObj, ref AveListObject parentListObject);

        #endregion

        #region site leve1

        int GetSiteChangedForIB(Guid siteId);
        /// <summary>
        /// 获取Site Collection上相关因素的change Type
        /// </summary>
        /// <param name="siteId">Site Collection ID</param>
        /// <param name="siteCollectionChangeType">Site Collection Change Type</param>
        /// <param name="userChangeType">Site Collection User Change Type</param>
        /// <param name="groupChangeType">Site Collection Group Change Type</param>
        /// <returns>
        /// true: the site collection is changed.
        /// false: the site collection is not changed.
        /// </returns>
        bool GetSiteChangedForIB(Guid siteId, ref ChangeType siteCollectionChangeType, ref ChangeType userChangeType, ref ChangeType groupChangeType);
        Dictionary<Guid, AveWebObject> QueryWebForIB(Guid siteId);
        Dictionary<int, AveSiteMemberObject> QuerySiteSecurityForIB(Guid siteId);

        #endregion

        #region web level

        void QueryWebRootFolder(AveListCache listCache, AveItemObject rootFolderObject);
        Dictionary<int, List<AveSecurityObject>> QueryWebSecurityForIB(Guid siteId, Guid webId);
        Dictionary<Guid, AveListObject> QueryListForIB(Guid siteId, Guid webId);

        #endregion

        #region list level

        void QueryListRootFolder(AveListCache listCache, AveListObject listObject, AveItemObject rootFolderObject);
        Dictionary<Guid, AveAlertObject> QueryListAlertForIB(Guid siteId, Guid webId, Guid listId);
        Dictionary<Guid, AveViewObject> QueryListViewForIB(Guid siteId, Guid webId, Guid listId);
        Dictionary<int, List<AveSecurityObject>> QueryListSecurityForIB(Guid siteId, Guid webId, Guid listId);
        Dictionary<byte[], AveContentTypeObject> QueryListContentTypeForIB(Guid siteId, Guid webId, Guid listId);
        void QuerySystemListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, List<AveDiscoverExtraItemBaseInfo> extraItems);
        void QuerySystemListItemForIB(AveFolderCache folderCache, AveItemObject folderObject,DiscoverModeForSOIB discoverMode, List<AveDiscoverExtraItemBaseInfo> extraItems);
        void QueryListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, List<AveDiscoverExtraItemBaseInfo> extraItems);
        void QueryListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject,DiscoverModeForSOIB discoverMode, List<AveDiscoverExtraItemBaseInfo> extraItems);
        Dictionary<string, object> GetListChangedItems(Guid webId, Guid listId, DateTime startTime, DateTime endTime);
        #endregion
        #region folder level

        void QueryChangedListItemFromExtraItemList(AveFolderCache folderCache, AveItemObject folderObject,AveListObject listObject, List<AveDiscoverExtraItemBaseInfo> extraItems);
        #endregion
        #region item level

        Dictionary<int, List<AveSecurityObject>> QueryItemSecurityForIB(Guid siteId, Guid webId, Guid listId, int itemId);
        void QueryAttachmentByItemObj(Guid siteId, string listRootFolderUrl, AveItemObject itemObj, IAveWeb web, Guid listId);
        void QueryAttachmentByItemObj(IAveWeb web, Guid listId, AveItemObject itemObject);
        void QueryVersionsByItemObj(AveItemCache itemCache, AveItemObject itemObject);
        
        #endregion

        #region FB

        Dictionary<Guid, AveWebObject> QuerySiteWebForFB(Guid siteId);
        AveWebObject QueryRootWeb(Guid siteId);
        Dictionary<Guid, AveWebObject> GetSubWebs(Guid siteId, Guid parentWebId, bool includeRecycleBin);
        Dictionary<Guid, AveListObject> QueryWebListForFB(Guid siteId, Guid webId);
        Dictionary<Guid, AveListObject> QueryWebListForFB(Guid siteId, Guid webId, bool includeRecycleBin);
        Dictionary<Guid, AveViewObject> QueryListViewForFB(Guid siteId, Guid webId, Guid listId);
        void QueryListItemForFB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject parentListObject, bool includeRecycleBin, bool includeSystemFolder, bool includeVersion);
        void QueryStubItemForFB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject parentListObject, bool includeRecycleBin);
        Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForFB(Guid siteId, Guid webId);
        Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForFB(Guid siteId, string serverRelativeUrl);
        int GetAllStubCount(AveFolderCache folderCache, AveItemObject folderObject, AveListObject ParentListObject, bool includeRecycleBin = false);
        void DiscoverAllListContent(AveListCache listCache, AveItemObject rootFolderObj, int maxItemCount, bool includeRecycleBin, bool includeSystemFolder);
        #endregion

        #region Support Replicator

        AveItemObject GetItemExist(Guid SiteId, Guid webId, Guid listId, Guid parentId, Guid id, string listRootFolder, string dirName, string leafName, bool isListItem, int? maxMajorwithMinorVersionCount);
        DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, string dirName, string leafName, ref Guid docId);
        DateTime GetItemLastModifiedTime(Guid siteId, Guid listId, int rowId);
        DateTime GetItemLastModifiedTime(Guid siteId, Guid itemId);


        AveItemObject GetItemVersions(Guid siteId, Guid webId, Guid listId, int docLibRowId);
        Guid GetDocIdByTp_Guid(Guid siteId, Guid webId, Guid listId, Guid parentId, Guid tp_Guid, int rowId);
        Dictionary<Guid, Guid> GetTPGUIDAndDocIdMapping(Guid siteId, Guid webId, Guid listId, Guid parentId, string folderUrl);
        bool IsHaveSameName(Guid siteId, Guid webId, Guid listId, string dirName, string leafName);
        bool IsListItemHaveSameName(Guid siteId, Guid webId, Guid tpGuid, Guid listId, int rowId);
        List<AveWebPartObject> GetItemWebParts(Guid siteId, Guid webId, Guid listId, Guid itemDocId);
        long GetItemSizeAndUserInfo(Guid siteId, Guid webId, Guid listId, Guid docId, int level, ref string createdBy, ref string modifiedBy);

        #endregion

        #region Support Extender
        int GetCurrentUIVersion(Guid siteId, Guid parentId, Guid docId);
        #endregion

        void Dispose();

        IAveDiscoveryQuery CloneObjWithNewRequest(object aveRequest);


        #region license查询
        long GetWebSize(Guid siteId, Guid webId);
        long GetObjectChangedSize(Guid siteId, Guid webId, Guid listId, string folderPath, DateTime beginTime);
        long GetListSize(Guid siteId, Guid webId, Guid listid);
        long GetFolderSize(Guid siteId, Guid webId, Guid listId, string folderUrl);


        #endregion

    }
}
