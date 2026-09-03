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




using System.Collections.Generic;
using System;

namespace AvePoint.Wrapper.Common
{
    public class AveFolderCache : AveDiscoverCache
    {
        [Flags]
        public enum QueryResult : byte
        {
            None = 0,
            Item = 1,
            Folder = 2,
            FolderAndItem = 3,
            All = 255,
        }

        public QueryResult QueryStatus { get; set; }
        //public bool HasQueryItem { get; set; }
        public int? ItemId { get;  set; }
        public string ListUrl { get; set; }
        public AveWebCacheParameter AveWebCacheParameter { get; private set; }
        public IAveWeb AveWeb { get { return this.AveWebCacheParameter.AveWeb; } }
        public Guid WebId { get { return this.AveWebCacheParameter.WebId; } }
        public IAveSite AveSite { get { return this.AveWebCacheParameter.AveSite; } }
        public Guid SiteId { get { return this.AveWebCacheParameter.SiteId; } }
        public Guid ListId { get; private set; }        
        public bool AttachNeedInited { get; set; }

        /// <summary>
        /// 所有此类的构造方法均要有此方法才能确保Query正常使用
        /// </summary>
        /// <param name="parentWeb"></param>
        private AveFolderCache(AveDiscoverCache parent, AveWebCacheParameter webParameter)
        {
            if (parent != null)
            {
                this.Query = parent.Query;
            }
            if (webParameter != null)
            {
                this.AveWebCacheParameter = webParameter;
            } 
        }
        public AveFolderCache(AveWebCache parentWeb, Guid listId, int? itemId = null)
            : this(parentWeb, parentWeb.AveWebCacheParameter)
        {
            //this.ParentList = parentList as AveListCache;
            this.ListId = listId;
            this.ItemId = itemId;
        }
        public AveFolderCache(AveSiteCache parentSite, Guid parentWebId, Guid parentListId, int? itemId = null)
            : this(new AveWebCache(parentSite, parentWebId), parentListId, itemId)
        {
        }
        public AveFolderCache(AveListCache parentList, int? itemId = null)
            : this(parentList, parentList.AveWebCacheParameter)
        {
            //this.ParentList = parentList as AveListCache;
            this.ListId = parentList.ListId;
            this.ItemId = itemId;
        }
        public AveFolderCache(AveFolderCache parentFolder, int? itemId = null)
            : this(parentFolder, parentFolder.AveWebCacheParameter)
        {
            //this.ParentList = parentList as AveListCache;
            this.ListId = parentFolder.ListId;
            this.ItemId = itemId;
        }
        public AveFolderCache(AveListCache parentList, AveItemObject folderObj)
            : this(parentList, parentList.AveWebCacheParameter)
        {
            this.InitDiscoverFolder(folderObj);
            //folderCache.ParentList.ListID = folder.ParentList.ID;
            //folderCache.ParentList.ParentWeb.WebID = folderCache.ParentWeb.WebID;           
        }
        /// <summary>
        /// For Unit Test to create FolderCache module
        /// </summary>
        public AveFolderCache()
        {}
        public Dictionary<int, List<AveSecurityObject>> GetChangeSecuritys()
        {
            if (ItemId.HasValue)
            {
                return Query.QueryItemSecurityForIB(this.AveWeb.ID, this.ListId, ItemId.Value);
            }
            else
            {
                return new Dictionary<int, List<AveSecurityObject>>();
            }
        }

        public void GetSubFolders(AveItemObject folderObject, bool includeRecycleBin, bool includeSystemFolder)
        {
            if ((QueryStatus & QueryResult.Folder) != QueryResult.Folder)
            {
                folderObject.SubFolderObjs.Clear();
                Query.QuerySubFoldersForFB(this, folderObject, includeRecycleBin, includeSystemFolder);
                QueryStatus |= QueryResult.Folder;
            }
        }

        public void GetSubs(AveItemObject folderObject, bool includeRecycleBin, bool includeSystemFolder)
        {
            if ((QueryStatus & QueryResult.FolderAndItem) != QueryResult.FolderAndItem)
            {
                folderObject.SubFolderObjs.Clear();
                folderObject.SubItemObjs.Clear();
                //folderObject.AttachmentObjs.Clear();
                Query.QueryListItemForFB(this, folderObject, includeRecycleBin, includeSystemFolder);
                QueryStatus |= QueryResult.FolderAndItem;
            }
        }
        
        public void GetChangeSubs(AveItemObject folderObject)
        {
            if ((QueryStatus & QueryResult.FolderAndItem) != QueryResult.FolderAndItem)
            {
                folderObject.SubFolderObjs.Clear();
                folderObject.SubItemObjs.Clear();
                //if (this.ParentList.ListID.Equals(System.Guid.Empty))
                if (this.ListId.Equals(System.Guid.Empty))
                {
                    Query.QuerySystemListItemForIB(this, folderObject,null);
                }
                else
                {
                    Query.QueryListItemForIB(this, folderObject,null);
                }
            }
        }

        public void InitDiscoverFolder(AveItemObject folderObj)
        {
            Query.InitDiscoverFolder(this, folderObj);
            this.ItemId = folderObj.ID;
        }
        /// <summary>
        /// Note: 此函数仅供IQuery InitFolder时调用，外围调用不安全
        /// </summary>
        /// <param name="webId"></param>
        /// <param name="listId"></param>
        public void InitBasicProperties(Guid webId, Guid listId, string listUrl)
        {
            if (!webId.Equals(Guid.Empty))
            {
                IAveSite site = this.AveWebCacheParameter.AveSite;
                this.AveWebCacheParameter = new AveWebCacheParameter(site, webId);
            }
            if (!listId.Equals(Guid.Empty))
            {
                this.ListId = listId;
            }
            if (!string.IsNullOrEmpty(listUrl))
            {
                this.ListUrl = listUrl;
            }
        }

        public void GetSubFoldersWithoutCache(AveItemObject folderObject)
        {
            if ((QueryStatus & QueryResult.FolderAndItem) != QueryResult.FolderAndItem)
            {
                folderObject.SubFolderObjs.Clear();
                Query.QuerySubFoldersForFB(this, folderObject);
                QueryStatus |= QueryResult.FolderAndItem;
            }
        }

        public void GetAttachments(AveItemObject itemObject)
        {
            if (AttachNeedInited)
            {
                itemObject.AttachmentObjs.Clear();
                AttachNeedInited = false;
                Query.QueryAttachment(this, itemObject);
            }
        }

        public void ClearCache(string serverRelativeUrl)
        {
            Query.RemoveFolderCache(serverRelativeUrl);
        }


        #region improve memory

        public void GetSubFolders(AveItemObject folderObject, bool includeSystemFolder)
        {
            if ((QueryStatus & QueryResult.FolderAndItem) != QueryResult.FolderAndItem)
            {
                folderObject.SubFolderObjs.Clear();
                folderObject.SubItemObjs.Clear();
                //folderObject.AttachmentObjs.Clear();
                Query.QuerySubFoldersForFB(this, folderObject, includeSystemFolder);
                this.QueryStatus |= QueryResult.FolderAndItem;
            }
        }

        public void GetSubItems(AveItemObject folderObject, bool includeSystemFolder, ref string pageInfo)
        {
            //content folder和rootfolder的items在分离之后重新获取
            //if ((folderObject.ID.HasValue && folderObject.ID > 0 || string.IsNullOrEmpty(folderObject.ServerRelativeUrl)) )//system folder file & view file没有分离，是和system folder&forms folder一起query的
            //{
                folderObject.SubItemObjs.Clear();
                //folderObject.AttachmentObjs.Clear();
                Query.QuerySubItemsForFB(this, folderObject, includeSystemFolder, ref pageInfo);
            //this.HasQuery = true;
            //}
        }


        #endregion

        public PreDiscoverDesignListResult PreDiscoverDesignList(IAveList list, bool includeGhostFile = false, bool includeEmptyFolder = false)
        {
            return Query.PreDiscoverDesignList(list.ParentWeb.Site.Url, list.ParentWeb.ID, list.ID, includeGhostFile, includeEmptyFolder);
        }

        public IEnumerable<int> GetSubFoldersWithStructure(AveItemObject folderObject, bool includeSystemFolder)
        {
            if ((QueryStatus & QueryResult.FolderAndItem) == QueryResult.FolderAndItem)
            {
                yield break;
            }
            foreach (var foldersCount in Query.QuerySubFoldersWithStructureForFB(this, folderObject, includeSystemFolder))
            {
                yield return foldersCount;
            }
            this.QueryStatus |= QueryResult.FolderAndItem;
        }

        public IEnumerable<int> GetSubItemsWithStructure(AveItemObject folderObject)
        {
            folderObject.SubItemObjs.Clear();
            foreach (var itemsCount in Query.QuerySubItemsWithStructureForFB(this, folderObject))
            {
                yield return itemsCount;
                Query.ClearItemCache();
            }
        }

        public IEnumerable<int> GetSubItemsWithStructureForArchiver(AveItemObject folderObject)
        {
            folderObject.SubItemObjs.Clear();
            foreach (var itemsCount in Query.QuerySubItemsWithStructureForArchiverFB(this, folderObject))
            {
                yield return itemsCount;
                //外围完成此次Query后，清除List下所有ItemCache
                Query.ClearItemCache();
            }
        }


        public IEnumerable<int> GetSubItemIdsWithStructureForRecords(AveItemObject folderObject)
        {
            folderObject.SubItemObjs.Clear();
            return Query.QuerySubItemsWithStructureForRecordsFB(this, folderObject);
            //foreach (var itemsCount in Query.QuerySubItemsWithStructureForArchiverFB(this, folderObject))
            //{
            //    yield return itemsCount;
            //    //外围完成此次Query后，清除List下所有ItemCache
            //    Query.ClearItemCache();
            //}
        }
        /*public IEnumerable<int> GetSubFoldersWithStructureForArchiver(AveItemObject folderObject, bool includeSystemFolder)
        {
            if ((QueryStatus & QueryResult.FolderAndItem) == QueryResult.FolderAndItem)
            {
                yield break;
            }
            foreach (var foldersCount in Query.QuerySubFoldersWithStructureForArchiverFB(this, folderObject, includeSystemFolder))
            {
                yield return foldersCount;
            }
            this.QueryStatus |= QueryResult.FolderAndItem;
        }*/
    }
}
