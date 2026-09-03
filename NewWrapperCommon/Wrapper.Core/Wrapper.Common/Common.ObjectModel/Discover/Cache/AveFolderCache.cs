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
        public bool HasQuery { get; set; }
        public int? ItemId { get; set; }
        public string ListUrl { get; set; }
        public AveWebCacheParameter AveWebCacheParameter { get; private set; }
        public IAveWeb AveWeb { get { return this.AveWebCacheParameter.AveWeb; } }
        public Guid WebId { get { return this.AveWebCacheParameter.WebId; } }
        public IAveSite AveSite { get { return this.AveWebCacheParameter.AveSite; } }
        public Guid SiteId { get { return this.AveWebCacheParameter.SiteId; } }
        public Guid ListId { get; private set; }

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

        public AveFolderCache(AveListCache parentList)
            : this(parentList, parentList.AveWebCacheParameter)
        {
            this.ListId = parentList.ListId;
        }
        /// <summary>
        /// For Unit Test to create FolderCache module
        /// </summary>
        public AveFolderCache()
        { }

        public void InitFolder(ref AveListObject ParentListObject, AveItemObject folderObj)
        {
            Query.InitDiscoverFolder(this, folderObj, ref ParentListObject);
            this.ItemId = folderObj.ID;

        }

        public Dictionary<int, List<AveSecurityObject>> GetChangeSecuritys()
        {
            if (ItemId.HasValue)
            {
                return Query.QueryItemSecurityForIB(this.AveSite.ID, this.AveWeb.ID, this.ListId, ItemId.Value);
            }
            else
            {
                return new Dictionary<int, List<AveSecurityObject>>();
            }
        }

        public void GetSubs(AveItemObject folderObject, bool includeRecycleBin, bool includeVersion, AveListObject parentListObject, bool includeSystemFolder)
        {
            if (!this.HasQuery && !folderObject.AllListContentAdded)
            {
                folderObject.SubFolderObjs.Clear();
                folderObject.SubItemObjs.Clear();
                //folderObject.AttachmentObjs.Clear();
                Query.QueryListItemForFB(this, folderObject, parentListObject, includeRecycleBin, includeSystemFolder, includeVersion);
                this.HasQuery = true;
            }
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

        public void GetStubItems(AveItemObject folderObj, AveListObject parentListObject, bool includeRecycleBin)
        {
            if (this.HasQuery || folderObj.AllListContentAdded)
            {
                folderObj.SubFolderObjs.Clear();
                folderObj.SubItemObjs.Clear();
                this.HasQuery = false;
                folderObj.AllListContentAdded = false;
            }
            Query.QueryStubItemForFB(this, folderObj, parentListObject, includeRecycleBin);
        }

        public int GetAllStubCount(AveItemObject folderObj, AveListObject parentListObject, bool includeRecycleBin = false)
        {
            return Query.GetAllStubCount(this, folderObj, parentListObject, includeRecycleBin);
        }

        public void GetAttachments(AveItemObject itemObject)
        {
            Query.QueryAttachmentByItemObj(this.AveWeb, this.ListId, itemObject);
        }
        public void GetAttachments(string listRootUrl, AveItemObject folderObject)
        {
            Query.QueryAttachmentByItemObj(this.SiteId, listRootUrl, folderObject, this.AveWeb, this.ListId);
        }

        public void AddExtraItemsIntoFolderCatch(AveItemObject folderObject, AveListObject parentListObject,List<AveDiscoverExtraItemBaseInfo> extraItems)
        {
            Query.QueryChangedListItemFromExtraItemList(this, folderObject, parentListObject, extraItems);
        }
    }
}
