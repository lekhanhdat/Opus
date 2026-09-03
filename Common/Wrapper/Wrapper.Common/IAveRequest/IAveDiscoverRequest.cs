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
    using System;
    using System.Collections.Generic;
    public class DMPreDiscoverItem
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
    public class DMPreDiscoverFolder : DMPreDiscoverItem
    {
        public List<DMPreDiscoverItem> Items { get; set; }
        public List<DMPreDiscoverFolder> SubFolders { get; set; }

        public bool HasChildren()
        {
            return (Items != null && Items.Count > 0) || (SubFolders != null && SubFolders.Count > 0);
        }
    }

    public class PreDiscoverDesignListResult
    {
        public Dictionary<int,string> GhostFiles { get; set; }
        public Dictionary<int, string> EmptyFolders { get; set; }
        public Dictionary<string,DMPreDiscoverFolder> PreserveFolders { get; set; }
    }

    public interface IAveDiscoverRequestV1
    {
        IEnumerable<Dictionary<string, object>> QueryFolderWithStructureForFB(Guid webId, Guid listId, string folderServerRelativeUrl, IEnumerable<int> foldersId, IDictionary<string, string> fieldsNeedLoadOfVersion, bool includeSystemFolder = false);

        IEnumerable<Dictionary<string, object>> QueryFolderWithStructureForArchiverFB(Guid webId, Guid listId, string folderServerRelativeUrl, IEnumerable<int> foldersId, IDictionary<string, string> fieldsNeedLoadOfVersion, bool includeSystemFolder = false);

        IEnumerable<Dictionary<string, object>> QueryItemWithStructureForFB(Guid webId, Guid listId, string folderServerRelativeUrl, IEnumerable<int> itemsId, IDictionary<string, string> fieldsNeedLoadOfVersion);

        IEnumerable<Dictionary<string, object>> QueryItemWithStructureForArchiverFB(Guid webId, Guid listId, string folderServerRelativeUrl, IEnumerable<int> itemsId, IDictionary<string, string> fieldsNeedLoadOfVersion);

        Dictionary<string, object> QueryListRootFolderWithStructureCache(Guid siteId, Guid webId, Guid mlistId);

        Dictionary<string, object> QueryListRootFolderForFullDiscover(Guid siteId, Guid webId, Guid mlistId);
    }

    public interface IAveDMDiscoverRequest
    {
        PreDiscoverDesignListResult PreDiscoverDesignList(string siteUrl, Guid webId, Guid listId, bool includeGhostFile = false, bool includeEmptyFolder = false);
    }

    //public interface IAveDiscoverRequest: IAveDMDiscoverRequest,IAveDiscoverRequestV1
    //{

    //    #region Discovery Query

    //    int GetSiteChangedForIB(Guid siteId, DateTime startTime, DateTime endTime, Dictionary<string, object> changeCache);
    //    int GetListChangedForRecords(Guid webId, Guid listId, DateTime startTime, DateTime endTime, Dictionary<string, object> changeCache);
    //    int GetListChangedCount(Guid webId, Guid listId, DateTime startTime, DateTime endTime);
    //    Dictionary<string, object> GetListChangedItems(Guid webId, Guid listId, DateTime startTime, DateTime endTime);
    //    Dictionary<string, object> GetFolderChangedItems(Guid webId, Guid listId, Guid folderId, DateTime startTime, DateTime endTime);
    //    Dictionary<Guid, object> QueryWebForIB(Dictionary<Guid, object> changedWebsInfo);

    //    Dictionary<int, object> QuerySiteSecurityForIB(Guid siteId, DateTime startTime, DateTime endTime);

    //    Dictionary<string, object> QueryRootWeb(Guid siteId);

    //    Dictionary<string, object> QueryWeb(Guid webId);

    //    Dictionary<Guid, object> GetSubWebs(Guid siteId, Guid parentWebId);

    //    Dictionary<Guid, object> QueryListForIB(Guid webId, Dictionary<Guid, object> changedListCache);

    //    Dictionary<Guid, object> QueryListForIB(Guid webId, Dictionary<string, object> changedCache, DateTime startTime, DateTime endTime);

    //    Dictionary<Guid, object> QueryListViewForFB(Guid siteId, Guid webId, Guid listId);

    //    Dictionary<string, object> QueryListRootFolder(Guid siteId, Guid webId, Guid mListID);

    //    Dictionary<string, object> GetItemExist(Guid SiteId, Guid webId, Guid listId, Guid id, string dirName, string leafName, bool isListItem);

    //    DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, Guid id, bool hasDocLibRowId);

    //    DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, string dirName, string leafName, ref Guid docId);

    //    DateTime GetItemLastModifiedTime(Guid siteId, Guid webId, Guid listId, Guid tp_Guid, ref Guid docId);

    //    Dictionary<Guid, object> QueryListAlertForIB(Guid siteId, Guid webId, Guid mListID);

    //    Dictionary<Guid, object> QueryListViewForIB(Guid siteId, Guid webId, Guid mListID);

    //    Dictionary<Guid, object> QueryWebListForFB(Guid siteId, Guid webId);

    //    Dictionary<string, object> QueryCurrentFolder(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, string listUrl);

    //    Dictionary<int, object> GetItemVersions(Guid siteId, Guid webId, Guid listId, int docLibRowId);

    //    Guid GetListItemGuid(Guid webId, Guid listId, Guid tp_Guid, int rowId);

    //    Guid GetDocIdByTp_Guid(Guid siteId, Guid webId, Guid listId, Guid parentId, Guid tp_Guid, int rowId);

    //    bool IsHaveSameName(Guid webId, Guid listId, string dirName, string leafName);

    //    bool IsListItemHaveSameName(Guid siteId, Guid webId, Guid tpGuid, Guid listId, int rowId);

    //    Dictionary<string, object> QueryListItemForFB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, bool loadSubFolders, bool loadSubItems, bool includeSystemFolder);

    //    Dictionary<string, object> QueryListItemForIB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, Dictionary<string, object> changCache);

    //    Dictionary<byte[], object> QueryWebContentTypeForFB(Guid siteId, Guid webId);

    //    Dictionary<byte[], object> QueryListContentTypeForFB(Guid siteId, Guid webId, Guid listId);

    //    Dictionary<string, object> QueryWebRootFolder(Guid webId);

    //    /// <summary>
    //    /// 根据AveQueryOption来check site从startTime到现在是否有变化
    //    /// </summary>
    //    /// <param name="siteUrl">需要check的site collection url</param>
    //    /// <param name="startTime">check的其实时间, 接受时间为当前时间, 使用的是Site.CurrentChangeToken属性</param>
    //    /// <param name="option">需要check哪些操作或者对象, 同ChangeQuery</param>
    //    /// <returns></returns>
    //    bool CheckSiteChanged(string siteUrl, long startTime, AveQueryOption option);

    //    void RemoveFolderCache(string folderServerRelativeUrl);

    //    void RemoveItemCache(int itemId);

    //    void ClearItemCache();

    //    Dictionary<string, object> QueryFolderForFB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, bool includeSystemFolder = false);

    //    Dictionary<string, object> QueryItemForFB(Guid siteId, Guid webId, Guid listId, Guid folderId, string folderUrl, ref string pageInfo, bool includeSystemFolder = false);

    //    Dictionary<string, object> GetItemWebParts(Guid siteId, Guid webId, Guid listId, Guid itemDocId);

    //    Dictionary<string, object> GetItemsByCamlQueryWithAttachments(string webServerRelativeUrl, Guid listId, string[] camlQueryNode);
        
    //    #endregion
    //}
}
