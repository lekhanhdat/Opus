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
using System.Linq;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Discovery
{
    public class AveDiscoverSite : AveDiscoverFilterBase, IDisposable
    {
        internal AveSiteCache siteCache;
        internal AveObjectModelFactory siteObjectModelFactory = null;

        public Guid SiteID { get; private set; }

        public IAveSite Site { get { return this.siteCache.AveSite; } }

        public ChangeType ChangeType { get { return this.siteCache.ChangeType; } }

        public bool SupportIB { get { return this.siteCache.Query.SupportIB; } }

        public bool CacheItemProperties { get { return siteCache.Query.CacheItemProperties; } set { siteCache.Query.CacheItemProperties = value; } }

        public Guid WebApplicationId { get; private set; }
     

        public AveDiscoverSite(IAveSite aveSite, AveBPOSAccountInfo account, AveDiscoveryKind kind, DiscoverModule module)
        {
            SiteID = aveSite.ID;
            if (aveSite.WebApplication != null)
            {
                WebApplicationId = aveSite.WebApplication.ID;
            }
            this.siteCache = new AveSiteCache(aveSite, account, kind, module);
            this.siteObjectModelFactory = siteCache.ObjectModelFactory;           
        }

        public AveDiscoverSite(IAveSite aveSite, AveBPOSAccountInfo account, AveDiscoveryKind kind, DiscoverModule module, DateTime startTime, DateTime endTime)
        {
           
            SiteID = aveSite.ID;
            if (aveSite.WebApplication != null)
            {
                WebApplicationId = aveSite.WebApplication.ID;
            }
            this.siteCache = new AveSiteCache(aveSite, account, kind, module, startTime, endTime);
            this.siteObjectModelFactory = siteCache.ObjectModelFactory;

        }

        /// <summary>
        /// only for replicator, incremental discover
        /// </summary>
        /// <param name="aveSite"></param>
        /// <param name="module"></param>
        /// <param name="objectModelFactory"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        public AveDiscoverSite(IAveSite aveSite, DiscoverModule module, AveObjectModelFactory objectModelFactory, DateTime startTime, DateTime endTime)
        {
            SiteID = aveSite.ID;
            if (aveSite.WebApplication != null)
            {
                WebApplicationId = aveSite.WebApplication.ID;
            }            
            //if (objectModelFactory != null && (objectModelFactory.ContextKind == AveContextKind.ClientObjectModel || objectModelFactory.ContextKind == AveContextKind.WebServiceObjectModel))
            //{
            //    this.siteCache = new AveSiteCache(aveSite, objectModelFactory, AveDiscoveryKind.API, module, startTime, endTime);
            //}
            //else
            //{
            //    this.siteCache = new AveSiteCache(aveSite, objectModelFactory, AveDiscoveryKind.Database, module, startTime, endTime);
            //}
            this.siteObjectModelFactory = objectModelFactory;
            this.siteCache = new AveSiteCache(aveSite, objectModelFactory, module, startTime, endTime);
        }

        /// <summary>
        /// only for replicator, full discover
        /// </summary>
        /// <param name="aveSite"></param>
        /// <param name="module"></param>
        /// <param name="objectModelFactory"></param>
        public AveDiscoverSite(IAveSite aveSite, DiscoverModule module, AveObjectModelFactory objectModelFactory)
        {
            SiteID = aveSite.ID;
            if (aveSite.WebApplication != null)
            {
                WebApplicationId = aveSite.WebApplication.ID;
            }
            //if (objectModelFactory != null && (objectModelFactory.ContextKind == AveContextKind.ClientObjectModel || objectModelFactory.ContextKind == AveContextKind.WebServiceObjectModel))
            //{
            //    this.siteCache = new AveSiteCache(aveSite, objectModelFactory, AveDiscoveryKind.API, module);
            //}
            //else
            //{
            //    this.siteCache = new AveSiteCache(aveSite, objectModelFactory, AveDiscoveryKind.Database, module);
            //}
            this.siteObjectModelFactory = objectModelFactory;
            this.siteCache = new AveSiteCache(aveSite, objectModelFactory, module);
        }

        public AveDiscoverList GetDiscoverList(IAveSite site, IAveWeb web, string listUrl)
        {
            AveDiscoverList list = new AveDiscoverList(site, web, listUrl, DiscoverModule.None, siteObjectModelFactory);
            if (list.ListCache != null && list.ListCache.Query != null)
            {
                list.ListCache.Query.Dispose();
            }
            list.ListCache.Query = this.siteCache.Query;
            list.IsNewCreated = false;
            return list;
        }

        #region FB

        private Dictionary<Guid, AveDiscoverWeb> AddDiscoverWeb(Dictionary<Guid, AveWebObject> webObjs)
        {
            Dictionary<Guid, AveDiscoverWeb> webs = new Dictionary<Guid, AveDiscoverWeb>();
            foreach (KeyValuePair<Guid, AveWebObject> pair in webObjs)
            {
                webs.Add(pair.Key, new AveDiscoverWeb(this, this.siteObjectModelFactory)
                {
                    WebObject = pair.Value,
                    WebCache = new AveWebCache(this.siteCache, pair.Value.WebID)
                });
            }
            return webs;
        }

        public Dictionary<Guid, AveDiscoverWeb> GetWebs(bool includeAppWeb = false)
        {
            return AddDiscoverWeb(this.siteCache.GetWebs(includeAppWeb));
        }

        /// <summary>
        /// 不会对结果进行Trim，因为如果Sub Site符合Filter就找不到了，外围需要自己调用IsQualified来判断。
        /// </summary>
        public AveDiscoverWeb GetRootWeb()
        {
            AveWebObject rootWebObj = this.siteCache.GetRootWeb();
            return new AveDiscoverWeb(this, this.siteObjectModelFactory)
            {
                WebObject = rootWebObj,
                WebCache = new AveWebCache(this.siteCache, rootWebObj.WebID)                
            };
        }

        public AveDiscoverWeb GetWeb(Guid webId)
        {
            AveWebObject webObj = this.siteCache.GetWeb(webId);
            return new AveDiscoverWeb(this, this.siteObjectModelFactory)
            {
                WebObject = webObj,
                WebCache = new AveWebCache(this.siteCache, webObj.WebID)
            };
        }
      
        #endregion

        #region IB

        public void GetSiteChanged()
        {
            this.siteCache.GetSiteChanged();
        }

        public Dictionary<Guid, AveDiscoverWeb> GetChangeWebs()
        {
            return AddDiscoverWeb(this.siteCache.GetChangeWebs());
        }

        public List<AveSiteMemberObject> GetChangeMembers()
        {
            return this.siteCache.GetChangeMembers().Values.ToList();
        }

        #endregion

        #region Support Replicator

        public string GetListContentTypes(Guid webId, Guid listId)
        {
            return this.siteCache.GetListContentTypes(webId, listId);
        }

        public AveDiscoverItem GetItemVersions(Guid webId, Guid listId, int docLibRowId)
        {
            AveDiscoverItem item = new AveDiscoverItem(this)
            {
                Obj = this.siteCache.GetItemVersions(webId, listId, docLibRowId),
                ItemCache = new AveItemCache(this.siteCache, webId, listId, docLibRowId),
            };
            item.ItemCache.ItemId = item.Obj.ID;
            return item;
        }

        //public DateTime GetItemLastModifiedTime(Guid webId, Guid listId, Guid id, bool hasDocLibRowId)
        //{
        //    return this.siteCache.GetItemLastModifiedTime(webId, listId, id, hasDocLibRowId);
        //}

        public DateTime GetItemLastModifiedTime(Guid listId, int rowId)
        {
            return this.siteCache.GetItemLastModifiedTime(listId, rowId);
        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid itemId)
        {
            return this.siteCache.GetItemLastModifiedTime(siteId, itemId);
        }

        public DateTime GetItemLastModifiedTime(Guid webId, Guid listId, string dirName, string leafName, ref Guid docId)
        {
            return this.siteCache.GetItemLastModifiedTime(webId, listId, dirName, leafName, ref docId);
        }

        public DateTime GetItemLastModifiedTime(Guid webId, Guid listId, Guid tp_Guid, ref Guid docId)
        {
            return this.siteCache.GetItemLastModifiedTime(webId, listId, tp_Guid, ref docId);
        }

        public AveDiscoverItem GetItemExist(Guid webId, Guid listId, Guid id, string dirName, string leafName, bool isListItem, AveDiscoverFolder discoverFolder = null)
        {
            AveItemObject itemObj = this.siteCache.GetItemExist(webId, listId, id, dirName, leafName, isListItem);
            if (itemObj != null)
            {
                AveDiscoverItem item = new AveDiscoverItem(this)
                {
                    Obj = itemObj,
                    ItemCache = new AveItemCache(this.siteCache, webId, listId)
                };
                //item.ItemCache.ParentFolder = (discoverFolder != null) ? discoverFolder.FolderCache : null;
                return item;
            }
            else
            {
                return null;
            }
        }

        public AveDiscoverItem GetItemExistForListener(string webServerRelativeUrl, Guid webId, System.Globalization.CultureInfo culture, Guid listId, Guid tpGuid, string dirName, string leafName, bool isListItem)
        {
            AveItemObject itemObj = this.siteCache.GetItemExistForListener(webServerRelativeUrl, culture, listId, tpGuid, dirName, leafName, isListItem);
            if (itemObj != null)
            {
                AveDiscoverItem item = new AveDiscoverItem(this)
                {
                    Obj = itemObj,
                    ItemCache = new AveItemCache(this.siteCache, webId, listId)
                };
                //item.ItemCache.ParentFolder = (discoverFolder != null) ? discoverFolder.FolderCache : null;
                return item;
            }
            else
            {
                return null;
            }
        }

        public AveDiscoverFolder GetFolderExistForListener(string webServerRelativeUrl, Guid webId, System.Globalization.CultureInfo culture, Guid listId, Guid tpGuid, string dirName, string leafName, bool isListItem)
        {
            AveItemObject itemObj = this.siteCache.GetItemExistForListener(webServerRelativeUrl, culture, listId, tpGuid, dirName, leafName, isListItem);
            if (itemObj != null)
            {
                AveDiscoverFolder folder = new AveDiscoverFolder(this)
                {
                    Obj = itemObj,
                    FolderCache = new AveFolderCache(this.siteCache, webId, listId)
                };
                return folder;
            }
            else
            {
                return null;
            }
        }

        public AveDiscoverFolder GetFolderExist(Guid webId, Guid listId, Guid id, string dirName, string leafName, bool isListItem, AveDiscoverFolder discoverFolder = null)
        {
            AveItemObject itemObj = this.siteCache.GetItemExist(webId, listId, id, dirName, leafName, isListItem);
            if (itemObj != null)
            {
                AveDiscoverFolder folder = new AveDiscoverFolder(this)
                {
                    Obj = itemObj,
                    FolderCache = new AveFolderCache(this.siteCache, webId, listId)
                };
                return folder;
            }
            else
            {
                return null;
            }
        }

        public Guid GetListItemGUID(Guid webId, Guid listId, Guid tp_Guid, int rowId)
        {
            return this.siteCache.GetListItemGuid(webId, listId, tp_Guid, rowId);
        }

        public Guid GetDocIdByTp_Guid(Guid siteId, Guid parentId, Guid tp_Guid)
        {
            return this.siteCache.GetDocIdByTp_Guid(siteId, parentId, tp_Guid);
        }

        public Guid GetDocIdByTp_Guid(Guid siteId,Guid webId,Guid listId, Guid parentId, Guid tp_Guid,int rowId)
        {
            return this.siteCache.GetDocIdByTp_Guid(siteId, webId, listId, parentId, tp_Guid, rowId);
        }

        public bool GetTPGUIDAndDocIdMapping(Guid siteId, Guid webId, Guid listId, Guid parentId, string folderUrl, ref Dictionary<Guid, Guid> itemsMapping, ref Dictionary<Guid, Guid> foldersMapping)
        {
            //return this.siteCache.GetDocIdByTp_Guid(siteId, parentId, tp_Guid);
            return this.siteCache.GetTPGUIDAndDocIdMapping(siteId, webId,listId,parentId,folderUrl, ref itemsMapping, ref foldersMapping);
        }

        public bool IsHaveSameName(Guid webId, Guid listId, string dirName, string leafName)
        {
            return this.siteCache.IsHaveSameName(webId, listId, dirName, leafName);
        }

        public bool IsListItemHaveSameName(Guid webId, Guid tpGuid, Guid listId, int rowId)
        {
            return this.siteCache.IsListItemHaveSameName(webId, tpGuid, listId, rowId);
        }

        public List<AveWebPartObject> GetItemWebParts(Guid webId, Guid listId, Guid itemDocId)
        {
            return this.siteCache.GetItemWebParts(webId, listId, itemDocId);
        }

        public int GetItemSize(Guid webId, Guid listId, Guid docId, ref string createdBy, ref string modifiedBy)
        {
            return this.siteCache.GetItemSize(webId, listId, docId, ref createdBy, ref modifiedBy);
        }

        #endregion
        #region support license
        public long GetSiteSize()
        {
            return this.siteCache.GetSiteSize();
        }
        #endregion
        public void Dispose()
        {
            if (this.siteCache != null)
            {
                this.siteCache.Dispose();
                this.siteCache = null;
            }
        }

        #region FilterBase Members

        public override ObjectInfoBase GetFilterObjectInfo(List<FilterPolicy> policies)
        {
            return FilterAnalyser.GetSiteFilterInfo(policies, siteCache.AveSite);
        }
        #endregion

    }
}
