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
using AvePoint.Wrapper.Resource.ServerAPI2010;

namespace AvePoint.Wrapper.Discovery
{
    public class AveDiscoverSite : AveDiscoverFilterBase,IAveDiscoverSite,IDisposable
    {
        //private static readonly AveLogger mLog = AveLogger.GetInstance(typeof(AveDiscoverSite));

        internal AveSiteCache siteCache;
        internal AveObjectModelFactory siteObjectModelFactory = null;

        public Guid SiteID { get; private set; }
        public IAveSite Site { get { return this.siteCache.AveSite; } }
        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public ChangeType ChangeType { get { return this.siteCache.ChangeType; } }

        /// <summary>
        /// 表示Site Collection User是否有改变
        /// 
        /// 值可能有多值，不一定是单值
        /// </summary>
        public ChangeType UserChangeType { get { return this.siteCache.UserChangeType; } }

        /// <summary>
        /// 表示Site Collection Group是否有改变
        /// 
        /// 值可能有多值，不一定是单值
        /// </summary>
        public ChangeType GroupChangeType { get { return this.siteCache.GroupChangeType; } }

        public bool SupportIB { get { return this.siteCache.Query.SupportIB; } }

        public Guid WebApplicationId { get; private set; }

        [Obsolete("It only for GetDiscoverList method. and will delete soon.")]
        private AveDiscoveryKind kind;

        public AveDiscoverSite(IAveSite aveSite, AveBPOSAccountInfo account, AveDiscoveryKind kind, DiscoverModule module)
        {
            this.siteCache = new AveSiteCache(aveSite, account, kind, module);
            InitParameters(aveSite, module);
            this.kind = kind;
        }

        public AveDiscoverSite(IAveSite aveSite, AveBPOSAccountInfo account, AveDiscoveryKind kind, DiscoverModule module, DateTime startTime, DateTime endTime)
        {
            this.siteCache = new AveSiteCache(aveSite, account, kind, module, startTime, endTime);
            InitParameters(aveSite, module);            
            this.kind = kind;
            StartTime = startTime;
            EndTime = endTime;
        }

       
        public AveDiscoverSite(IAveSite aveSite, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory)
        {
            this.siteCache = new AveSiteCache(aveSite, objectModelFactory,kind, module);
            InitParameters(aveSite, module);
            this.kind = kind;
        }

        public AveDiscoverSite(IAveSite aveSite, DiscoverModule module, AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory, DateTime startTime, DateTime endTime)
        {
            this.siteCache = new AveSiteCache(aveSite, objectModelFactory,kind, module, startTime, endTime);
            InitParameters(aveSite, module);
            this.kind = kind;
        }

        private void InitParameters(IAveSite aveSite, DiscoverModule module)
        {
            SiteID = aveSite.ID;
            if (aveSite.WebApplication != null)
            {
                WebApplicationId = aveSite.WebApplication.ID;
            }
            this.siteObjectModelFactory = siteCache.ObjectModelFactory;
        }

        [Obsolete("Use IAveDiscoverList IAveDiscoverSite.GetDiscoverList(IAveSite site, IAveWeb web, string listUrl) instead.")]
        public AveDiscoverList GetDiscoverList(IAveSite site, IAveWeb web, string listUrl)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverSite.GetDiscoverList"))
            {
                try
                {
                    AveDiscoverList list = new AveDiscoverList(site, web, listUrl, DiscoverModule.None, this.kind, siteObjectModelFactory);
                    if (list.ListCache != null && list.ListCache.Query != null)
                    {
                        list.ListCache.Query.Dispose();
                    }
                    list.ListCache.Query = this.siteCache.Query;
                    list.IsNewCreated = false;
                    return list;
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetDiscoverList. Exception detail: {0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetListError);
                }
            }
        }

        #region FB

        private Dictionary<Guid, AveDiscoverWeb> AddDiscoverWeb(Dictionary<Guid, AveWebObject> webObjs)
        {
            return AddDiscoverWeb(webObjs, false);
        }

        private Dictionary<Guid, AveDiscoverWeb> AddDiscoverWeb(Dictionary<Guid, AveWebObject> webObjs, bool filterAppWeb)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverSite.AddDiscoverWeb"))
            {
                Dictionary<Guid, AveDiscoverWeb> webs = new Dictionary<Guid, AveDiscoverWeb>();
                foreach (KeyValuePair<Guid, AveWebObject> pair in webObjs)
                {
                    if (pair.Value.IsAppWeb && filterAppWeb)
                        continue;

                    webs.Add(pair.Key, new AveDiscoverWeb(this)
                    {
                        WebObject = pair.Value,
                        WebCache = new AveWebCache(this.siteCache, pair.Value.WebID)
                    });
                }
                return webs;
            }
        }

        [Obsolete("Use Dictionary<Guid, IAveDiscoverWeb> IAveDiscoverSite.GetWebs() instead")]
        public Dictionary<Guid, AveDiscoverWeb> GetWebs()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverSite.GetWebs"))
            {
                try
                {
                    return AddDiscoverWeb(this.siteCache.GetWebs(), true);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetWebs. Exception detail: {0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetWebError);
                }
            }
        }

        public Dictionary<Guid, IAveDiscoverWeb> GetAllWebs()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverSite.GetAllWebs"))
            {
                try
                {
                    return AddDiscoverWeb(this.siteCache.GetWebs()).ToDictionary(kv => kv.Key, kv => kv.Value as IAveDiscoverWeb);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetWebs. Exception detail: {0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetWebError);
                }
            }
        }

        /// <summary>
        /// 不会对结果进行Trim，因为如果Sub Site符合Filter就找不到了，外围需要自己调用IsQualified来判断。
        /// </summary>
        [Obsolete("Use IAveDiscoverWeb IAveDiscoverSite.GetRootWeb() instead")]
        public AveDiscoverWeb GetRootWeb()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverSite.GetRootWeb"))
            {
                try
                {
                    AveWebObject rootWebObj = this.siteCache.GetRootWeb();
                    return new AveDiscoverWeb(this, siteObjectModelFactory)
                    {
                        WebObject = rootWebObj,
                        WebCache = new AveWebCache(this.siteCache, rootWebObj.WebID)
                    };
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetRootWeb. Exception detail: {0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetRootWebError);
                }
            }
        }
      
        #endregion

        #region IB

        public void GetSiteChanged()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverSite.GetSiteChanged"))
            {
                try
                {
                    this.siteCache.GetSiteChanged();
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetSiteChanged. Exception detail: {0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetChangedSiteError);
                }
            }
        }

        [Obsolete("Use Dictionary<Guid, IAveDiscoverWeb> IAveDiscoverSite.GetChangeWebs() instead.")]
        public Dictionary<Guid, AveDiscoverWeb> GetChangeWebs()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverSite.GetChangeWebs"))
            {
                try
                {
                    return AddDiscoverWeb(this.siteCache.GetChangeWebs());
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetChangeWebs. Exception detail: {0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetChangedWebError);
                }
            }
        }

        public List<AveSiteMemberObject> GetChangeMembers()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverSite.GetChangeMembers"))
            {
                try
                {
                    return this.siteCache.GetChangeMembers().Values.ToList();
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetChangeMembers. Exception detail: {0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetChangedUsersError);
                }
            }
        }

        #endregion

        #region Support Replicator

        [Obsolete("Use IAveDiscoverItem IAveDiscoverSite.GetItemVersions(Guid webId, Guid listId, int docLibRowId) instead.")]
        public AveDiscoverItem GetItemVersions(Guid webId, Guid listId, int docLibRowId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverSite.GetItemVersions"))
            {
                try
                {
                    AveDiscoverItem item = new AveDiscoverItem(this)
                    {
                        Obj = this.siteCache.GetItemVersions(webId, listId, docLibRowId),
                        ItemCache = new AveItemCache(this.siteCache, webId, listId, docLibRowId),
                    };
                    item.ItemCache.ItemId = item.Obj.ID;
                    return item;
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetItemVersions. Exception detail: {0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetVersionsError);
                }
            }
        }

        //public DateTime GetItemLastModifiedTime(Guid webId, Guid listId, Guid id, bool hasDocLibRowId)
        //{
        //    return this.siteCache.GetItemLastModifiedTime(webId, listId, id, hasDocLibRowId);
        //}

        public DateTime GetItemLastModifiedTime(Guid siteId,Guid listId, int rowId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverSite.GetItemLastModifiedTime"))
            {
                try
                {
                    return this.siteCache.GetItemLastModifiedTime(siteId, listId, rowId);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetItemLastModifiedTime. Exception detail: {0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetItemModifyTimeError);
                }
            }
        }

        public DateTime GetItemLastModifiedTime(Guid siteId, Guid itemId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverSite.GetItemLastModifiedTime"))
            {
                try
                {
                    return this.siteCache.GetItemLastModifiedTime(siteId, itemId);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetItemLastModifiedTime. Exception detail: {0}",ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetItemModifyTimeError);
                }
            }
        }

        public DateTime GetItemLastModifiedTime(Guid webId, Guid listId, string dirName, string leafName, ref Guid docId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverSite.GetItemLastModifiedTime"))
            {
                try
                {
                    return this.siteCache.GetItemLastModifiedTime(webId, listId, dirName, leafName, ref docId);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetItemLastModifiedTime. ,Exception detail: {0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetItemModifyTimeError);
                }
            }
        }

        [Obsolete("Use public IAveDiscoverItem IAveDiscoverSite.GetItemExist(Guid webId, Guid listId, Guid id, string dirName, string leafName, bool isListItem, IAveDiscoverFolder discoverFolder = null) instead")]
        public AveDiscoverItem GetItemExist(Guid webId, Guid listId, Guid parentId, Guid id, string listRootFolder,string dirName, string leafName, bool isListItem,int?maxMajorwithMinorVersionCount, AveDiscoverFolder discoverFolder = null)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverSite.GetItemExist"))
            {
                try
                {
                    AveItemObject itemObj = this.siteCache.GetItemExist(webId, listId, parentId, id, listRootFolder, dirName, leafName, isListItem, maxMajorwithMinorVersionCount);
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
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetItemExist. Exception detail: {0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetItemsError);
                }
            }
        }

        [Obsolete("Use public IAveDiscoverFolder GetFolderExist(Guid webId, Guid listId, Guid id, string dirName, string leafName, bool isListItem, IAveDiscoverFolder discoverFolder = null) instead")]
        public AveDiscoverFolder GetFolderExist(Guid webId, Guid listId, Guid parentId, Guid id, string listRootFolder,string dirName, string leafName, bool isListItem, AveDiscoverFolder discoverFolder = null)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverSite.GetFolderExist"))
            {
                try
                {
                    AveItemObject itemObj = this.siteCache.GetItemExist(webId, listId, parentId, id, listRootFolder, dirName, leafName, isListItem,null);
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
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetFolderExist. Exception detail: {0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetFolderError);
                }
            }
        }

        public Guid GetDocIdByTp_Guid(Guid siteId,Guid webId,Guid listId, Guid parentId, Guid tp_Guid,int rowId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverSite.GetDocIdByTp_Guid"))
            {
                try
                {
                    return this.siteCache.GetDocIdByTp_Guid(siteId, webId, listId, parentId, tp_Guid, rowId);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetDocIdByTp_Guid. Exception detail: {0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetItemsError);
                }
            }
        }

        public Dictionary<Guid, Guid> GetTPGUIDAndDocIdMapping(Guid siteId, Guid webId, Guid listId, Guid parentId, string folderUrl)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverSite.GetTPGUIDAndDocIdMapping"))
            {
                try
                {
                    //return this.siteCache.GetDocIdByTp_Guid(siteId, parentId, tp_Guid);
                    return this.siteCache.GetTPGUIDAndDocIdMapping(siteId, webId, listId, parentId, folderUrl);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetTPGUIDAndDocIdMapping. Exception detail: {0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetGuidDocIdMappingError);
                }
            }
        }

        public bool IsHaveSameName(Guid webId, Guid listId, string dirName, string leafName)
        {
            try
            {
                return this.siteCache.IsHaveSameName(webId, listId, dirName, leafName);
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, "An exception occurred while do IsHaveSameName. Exception detail: {0}", ex);
                throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDValidateNameError);
            }
        }

        public bool IsListItemHaveSameName(Guid webId, Guid tpGuid, Guid listId, int rowId)
        {
            try
            {
                return this.siteCache.IsListItemHaveSameName(webId, tpGuid, listId, rowId);
            }
            catch (Exception ex)
            {
                log.Log(AveLogLevel.WARN, "An exception occurred while do IsListItemHaveSameName. Exception detail: {0}", ex);
                throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDValidateNameError);
            }
        }

        public List<AveWebPartObject> GetItemWebParts(Guid webId, Guid listId, Guid itemDocId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverSite.GetItemWebParts"))
            {
                try
                {
                    return this.siteCache.GetItemWebParts(webId, listId, itemDocId);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetItemWebParts. Exception detail: {0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetItemWebpartsError);
                }
            }
        }

        public long GetItemSizeAndUserInfo(Guid webId, Guid listId, Guid docId, int level, ref string createdBy, ref string modifiedBy)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverSite.GetItemSizeAndUserInfo"))
            {
                try
                {
                    return this.siteCache.GetItemSizeAndUserInfo(webId, listId, docId, level, ref createdBy, ref modifiedBy);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetItemSize. Exception detail: {0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetItemSizeError);
                }
            }
        }
        

        #endregion
            #region support license
        public long GetSiteSize()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverSite.GetSiteSize"))
            {
                try
                {
                    return this.siteCache.GetSiteSize();
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetSiteSize. Exception detail: {0}", ex);
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetSizeSizeError);
                }
            }
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
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverSite.GetFilterObjectInfo"))
            {
                if (!HasFilterOnThisLevel(policies, PolicyLevel.SiteCollection))
                {
                    return new SiteCollectionInfo();
                }
                return FilterAnalyser.GetSiteFilterInfo(policies, siteCache.AveSite);
            }
        }
        #endregion



        Dictionary<Guid, IAveDiscoverWeb> IAveDiscoverSite.GetChangeWebs()
        {
            return this.GetChangeWebs().ToDictionary(kv => kv.Key, kv => kv.Value as IAveDiscoverWeb);
        }

        IAveDiscoverList IAveDiscoverSite.GetDiscoverList(IAveSite site, IAveWeb web, string listUrl)
        {
            return this.GetDiscoverList(site, web, listUrl) as IAveDiscoverList;
        }

        IAveDiscoverFolder IAveDiscoverSite.GetFolderExist(Guid webId, Guid listId, Guid parentId, Guid id, string listRootFolder, string dirName, string leafName, bool isListItem, IAveDiscoverFolder discoverFolder = null)
        {
            return this.GetFolderExist(webId, listId, parentId, id, listRootFolder, dirName, leafName, isListItem, discoverFolder as AveDiscoverFolder) as IAveDiscoverFolder;
        }

        IAveDiscoverItem IAveDiscoverSite.GetItemExist(Guid webId, Guid listId, Guid parentId, Guid id, string listRootFolder, string dirName, string leafName, bool isListItem, IAveDiscoverList discoverList, IAveDiscoverFolder discoverFolder = null)
        {
            //Add default value. 
            int? maxMajorwithMinorVersionCount = (int?)0;
            AveDiscoverList list = discoverList as AveDiscoverList;
            if (list != null)
            {
                maxMajorwithMinorVersionCount = list.ListObject.MaxMajorwithMinorVersionCount;
            }
            return this.GetItemExist(webId, listId, parentId, id, listRootFolder, dirName, leafName, isListItem, maxMajorwithMinorVersionCount, discoverFolder as AveDiscoverFolder) as IAveDiscoverItem;
        }

        IAveDiscoverItem IAveDiscoverSite.GetItemExist(Guid webId, Guid listId, Guid parentId, Guid id, string listRootFolder, string dirName, string leafName, bool isListItem, IAveDiscoverFolder discoverFolder = null)
        {
            return this.GetItemExist(webId, listId, parentId, id, listRootFolder, dirName, leafName, isListItem, (int?)0, discoverFolder as AveDiscoverFolder) as IAveDiscoverItem;
        }

        IAveDiscoverItem IAveDiscoverSite.GetItemVersions(Guid webId, Guid listId, int docLibRowId)
        {
            return this.GetItemVersions(webId, listId, docLibRowId) as IAveDiscoverItem;
        }

        IAveDiscoverWeb IAveDiscoverSite.GetRootWeb()
        {
            return this.GetRootWeb() as IAveDiscoverWeb;
        }

        Dictionary<Guid, IAveDiscoverWeb> IAveDiscoverSite.GetWebs()
        {
            return this.GetWebs().ToDictionary(kv => kv.Key, kv => kv.Value as IAveDiscoverWeb);
        }

        [Obsolete("Use long type")]
        int IAveDiscoverSite.GetItemSizeAndUserInfo(Guid webId, Guid listId, Guid docId, int level, ref string createdBy, ref string modifiedBy)
        {
            return (int)this.GetItemSizeAndUserInfo(webId, listId, docId, level, ref createdBy, ref modifiedBy);
        }
    }
}
