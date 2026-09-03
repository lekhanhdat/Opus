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

        /// <summary>
        /// 初始化DiscoverFolder
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="folderObj"></param>
        /// <param name="noPropertyFolders"></param>
        /// <param name="listObject"> 缓存在APIDiscover 层，设计有问题，需要修改</param>
        /// <param name="discoverReader"></param>
        void InitDiscoverFolder(AveFolderCache folderCache, AveItemObject folderObj, Dictionary<string, AveItemObject> noPropertyFolders, ref AveListObject listObject, AveDiscoverReader discoverReader);

        /// <summary>
        /// 初始化DiscoverList
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="listCache"></param>
        /// <param name="listObj"></param>
        void InitDiscoverList(AveListCache listCache, AveListObject listObj);

        /// <summary>
        /// 根据FullUrl和SiteId 查询,初始化 DiscoverWeb 的基本信息
        /// </summary>
        /// <param name="webCache"></param>
        /// <param name="webObj"></param>
        void InitDiscoverWeb(AveWebCache webCache, AveWebObject webObj);

        #region For Replicator

        /// <summary>
        /// 根据Id或name check item是否存在，存在返回对应的Item信息
        /// </summary>
        /// <param name="SiteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="parentId"></param>
        /// <param name="id"></param>
        /// <param name="listRootFolder"></param>
        /// <param name="dirName"></param>
        /// <param name="leafName"></param>
        /// <param name="isListItem"></param>
        /// <param name="discoverReader"></param>
        /// <param name="maxMajorwithMinorVersionCount"></param>
        /// <returns></returns>
        AveItemObject GetItemExist(Guid SiteId, Guid webId, Guid listId, Guid parentId, Guid id, string listRootFolder, string dirName, string leafName, bool isListItem, AveDiscoverReader discoverReader, int? maxMajorwithMinorVersionCount);

        /// <summary>
        /// 根据DirName,LeafName获取Item/Document的LastModifiedTime(system file查Doc表，其他item查询UD表)
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="dirName"></param>
        /// <param name="leafName"></param>
        /// <param name="docId"></param>
        /// <returns></returns>
        DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, string dirName, string leafName, ref Guid docId);

        /// <summary>
        /// 根据Item RowId获取Item/Document的LastModifiedTime,查询AllUserData表
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="listId"></param>
        /// <param name="rowId">must be above 0</param>
        /// <returns></returns>
        DateTime GetItemLastModifiedTime(Guid siteId, Guid listId, int rowId);

        /// <summary>
        /// 通过DocId获取Item/Document的LastModifiedTime,AllDocs表
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="itemId"></param>
        /// <returns></returns>
        DateTime GetItemLastModifiedTime(Guid siteId, Guid itemId);

        /// <summary>
        /// 根据DoclibRowId查找该Item下的所有Versions
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="docLibRowId"></param>
        /// <returns>返回的 AveItemObject 本身属性不全</returns>
        AveItemObject GetItemVersions(Guid siteId, Guid listId, int docLibRowId);

        /// <summary>
        /// 根据tp_Guid去查询Item的DocId
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="tp_Guid"></param>
        /// <returns></returns>
        Guid GetDocIdByTp_Guid(Guid siteId, Guid parentId, Guid tp_Guid);

        /// <summary>
        /// 根据parentId获取Document的tp_Guid-tp_DocId,DocId-type的Mapping
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        Dictionary<Guid, Guid> GetTPGUIDAndDocIdMapping(Guid siteId, Guid parentId);

        /// <summary>
        /// 根据Leafname去数据库中查询是否有相同记录
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="dirName"></param>
        /// <param name="leafName"></param>
        /// <returns></returns>
        bool IsHaveSameName(Guid siteId, Guid webId, Guid listId, string dirName, string leafName);

        /// <summary>
        /// 根据tp_Guid去查询数据库中是否有相同记录
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="tpGuid"></param>
        /// <param name="listId"></param>
        /// <param name="rowId"></param>
        /// <returns></returns>
        bool IsListItemHaveSameName(Guid siteId, Guid webId, Guid tpGuid, Guid listId, int rowId);

        /// <summary>
        /// 查询Item上的WebParts
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="itemDocId"></param>
        /// <returns></returns>
        List<AveWebPartObject> GetItemWebParts(Guid siteId, Guid webId, Guid listId, Guid itemDocId);

        /// <summary>
        /// 获取Item的size和ModifiedBy，CreatedBy属性
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="docId"></param>
        /// <param name="level"></param>
        /// <param name="createdBy"></param>
        /// <param name="modifiedBy"></param>
        /// <returns></returns>
        long GetItemSizeAndUserInfo(Guid siteId, Guid webId, Guid listId, Guid docId, int level, ref string createdBy, ref string modifiedBy);

        #endregion

        #region For Extender

        int GetCurrentUIVersion(Guid siteId, Guid parentId, Guid docId);

        /// <summary>
        /// 获取某个item的所有version的stub信息
        /// API Discover extension
        /// </summary>
        /// <param name="versions"></param>
        /// <param name="siteId"></param>
        /// <param name="itemId"></param>
        /// <param name="discoverReader"></param>
        void SetVersionsStubInfo(List<AveVersionObject> versions, Guid siteId, Guid itemId, AveDiscoverReader discoverReader);

        /// <summary>
        /// For Extender,set attachments stubInfo，补全一个Item上所有attachment的stub信息
        /// API Discover extension
        /// </summary>
        /// <param name="attachments">同一个Item或folder上的attachment集合</param>
        /// <param name="siteId"></param>
        /// <param name="discoverReader"></param>
        void SetAttachmentsStubInfo(List<AveItemObject> attachments, Guid siteId, AveDiscoverReader discoverReader);

        #endregion For Extender

        #region FB

        /// <summary>
        /// 获取Site下的所有web信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        Dictionary<Guid, AveWebObject> QuerySiteWebForFB(Guid siteId);

        /// <summary>
        /// 获取Site的RootWeb信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <returns></returns>
        AveWebObject QueryRootWeb(Guid siteId);

        /// <summary>
        /// 根据ParentWebId获取sub webs
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="parentWebId"></param>
        /// <param name="includeRecycleBin">FOR SO</param>
        /// <returns></returns>
        Dictionary<Guid, AveWebObject> GetSubWebs(Guid siteId, Guid parentWebId, bool includeRecycleBin);

        /// <summary>
        /// 获取Web下的所有Lists信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="includeRecycleBin"></param>
        /// <returns></returns>
        Dictionary<Guid, AveListObject> QueryWebListForFB(Guid siteId, Guid webId, bool includeRecycleBin);

        ///<summary>
        /// 获取List下的所有Views信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <returns></returns>
        Dictionary<Guid, AveViewObject> QueryListViewForFB(Guid siteId, Guid webId, Guid listId);
        
        /// <summary>
        /// 获取某Folder下的Items和Versions信息，包括Attachement.
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="folderObject"></param>
        /// <param name="listObject"></param>
        /// <param name="discoverReader"></param>
        /// <param name="includeRecycleBin"></param>
        void QueryListItemForFB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, AveDiscoverReader discoverReader, bool includeRecycleBin, bool includeVersion);

        /// <summary>
        /// 查询某folder下的stub Item信息
        /// 无API实现
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="folderObject"></param>
        /// <param name="listObject"></param>
        /// <param name="discoverReader"></param>
        /// <param name="includeRecycleBin"></param>
        void QueryStubItemForFB(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, AveDiscoverReader discoverReader, bool includeRecycleBin);

        /// <summary>
        /// 获取ParentId下所有Stub Item数量(包括AllDocs表中和AllDocVersions表中)
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="folderObject"></param>
        /// <param name="listObject"></param>
        /// <returns></returns>
        int GetAllStubCount(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, bool includeRecycleBin = false);

        /// <summary>
        /// 查询Web下的所有ContentTypes信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <returns></returns>
        Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForFB(Guid siteId, Guid webId);

        /// <summary>
        /// 查询Web下的所有ContentTypes信息
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="serverRelativeUrl"></param>
        /// <returns></returns>
        Dictionary<byte[], AveContentTypeObject> QueryWebContentTypeForFB(Guid siteId, string serverRelativeUrl);

        #endregion

        #region Item Level Security

        /// <summary>
        /// 从EventCache表中查询Item的Security改变
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="itemId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        Dictionary<int, List<AveSecurityObject>> QueryItemSecurityForIB(Guid siteId, Guid webId, Guid listId, int itemId, DateTime startTime, DateTime endTime);
        void QueryItemAttachment(Guid siteId, string listRootUrl, AveItemObject itemObj, AveDiscoverReader discoverReader);

        void QueryItemFromExtraItemList(AveFolderCache folderCache, AveItemObject folderObject, AveListObject listObject, AveDiscoverReader discoverReader,
            Dictionary<string, AveItemObject> noPropertyFolders, List<AveDiscoverExtraItemBaseInfo> extraItems);
        #endregion

        /// <summary>
        /// API Discover extension
        /// </summary>
        /// <param name="folderCache"></param>
        /// <param name="listObj"></param>
        /// <returns></returns>
        List<Dictionary<String,Object>> GetCheckoutListItems(AveFolderCache folderCache, AveListObject listObj);


        #region List Level

        /// <summary>
        /// 获取List下的RootFolder信息
        /// 效率考虑，有API实现 
        /// </summary>
        /// <param name="listCache"></param>
        /// <param name="discoverReader"></param>
        /// <param name="listObject"></param>
        /// <param name="rootFolderObject"></param>
        void QueryListRootFolder(AveListCache listCache, AveDiscoverReader discoverReader, AveListObject listObject, AveItemObject rootFolderObject);

        /// <summary>
        /// 从EventCache表中获取List下Alert的改变
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        Dictionary<Guid, AveAlertObject> QueryListAlertForIB(Guid siteId, Guid webId, Guid listId, DateTime startTime, DateTime endTime);

        /// <summary>
        /// 获取List下的View信息的改变
        /// 效率考虑，有API实现
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        Dictionary<Guid, AveViewObject> QueryListViewForIB(Guid siteId, Guid webId, Guid listId, DateTime startTime, DateTime endTime);

        Dictionary<int, List<AveSecurityObject>> QueryListSecurityForIB(Guid siteId, Guid webId, Guid listId, DateTime startTime, DateTime endTime);

        Dictionary<byte[], AveContentTypeObject> QueryListContentTypeForIB(Guid siteId, Guid webId, Guid listId, DateTime startTime, DateTime endTime);

        void QuerySystemListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, DateTime startTime, DateTime endTime, AveListObject listObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders, List<AveDiscoverExtraItemBaseInfo> extraItems);
        void QueryListItemForIB(AveFolderCache folderCache, AveItemObject folderObject, DateTime startTime, DateTime endTime, AveListObject listObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders, List<AveDiscoverExtraItemBaseInfo> extraItems);

        #endregion

        #region Web Level

        void QueryWebRootFolder(AveListCache listCache, AveItemObject rootFolderObject, AveDiscoverReader discoverReader, Dictionary<string, AveItemObject> noPropertyFolders);

        Dictionary<int, List<AveSecurityObject>> QueryWebSecurityForIB(Guid siteId, Guid webId, DateTime startTime, DateTime endTime);

        Dictionary<Guid, AveListObject> QueryListForIB(Guid siteId, Guid webId, DateTime startTime, DateTime endTime);

        #endregion

        #region Site Level

        int GetSiteChangedForIB(Guid siteId, DateTime startTime, DateTime endTime);

        bool GetSiteChangedForIB(Guid siteId, DateTime startTime, DateTime endTime, ref ChangeType siteCollectionChangeType, ref ChangeType userChangeType, ref ChangeType groupChangeType);

        Dictionary<Guid, AveWebObject> QueryWebForIB(Guid siteId, DateTime startTime, DateTime endTime);

        Dictionary<int, AveSiteMemberObject> QuerySiteSecurityForIB(Guid siteId, DateTime startTime, DateTime endTime);
        /// <summary>
        ///用于在API Discover中获取User或者Group的属性，用于同步删除。此方法负责添加属性，不负责初始化member数据。
        /// </summary>
        /// <param name="siteMember">User或者Group的集合，请在传入之前进行初始化。</param>
        /// <param name="siteId">Site的Guid，注意，不可以传入Guid.Empty，这样会走入SQL逻辑。</param>
        /// <param name="changeObjType">传入User或Group，区分补充User的属性还是Group的属性，传入其他值此方法空跑不会执行任何操作。</param>
        void QueryUserOrGroupProperty(Dictionary<int, AveSiteMemberObject> siteMember, Guid siteId, ChangeObjectType changeObjType);
        #endregion
        void GetDeleteSites(Dictionary<Guid, AveSiteObject> deletedSites, DateTime startTime, DateTime endTime);

        #endregion
        #region License查询
        long GetWebSize(Guid siteId, Guid webId);
        long GetObjectChangedSize(Guid siteId, Guid webId, Guid listId, string folderPath, DateTime beginTime);

        long GetListSize(Guid siteId, Guid webId, Guid listId);

        long GetFolderSize(Guid siteId, Guid webId, Guid listId, string folderUrl);
        #endregion

        void SetItemStubInfo(List<AveItemObject> allitems, Guid guid);

        void SetItemStubInfo(List<AveItemObject> allitems, Guid guid, bool includeRecycleBin);
        void QueryItemVersionsForAPI(Dictionary<int, AveItemObject> itemCollection, AveListObject listObject, AveDiscoverReader discoverReader);
        void QueryItemVersionsForAPIFB(Guid siteId, Guid parentId, List<AveItemObject> itemObjs, AveListObject listObject, AveDiscoverReader discoverReader);
    }
}
