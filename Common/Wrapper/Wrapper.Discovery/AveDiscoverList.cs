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
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Discovery
{
    public class AveDiscoverList : AveDiscoverFilterBase, IDisposable
    {
        internal bool IsNewCreated { get; set; }//从List级别进入Discover的Query

        internal AveListCache ListCache { get; set; }

        internal AveListObject ListObject { get; set; }

        public string ModifiedBy { get { return ListObject.ModifiedBy; } set { ListObject.ModifiedBy = value; } }
        public Guid ListId { get { return ListObject.ListId; } set { ListObject.ListId = value; } }
        public Guid RootFolderId { get { return ListObject.RootFolderId; } set { ListObject.RootFolderId = value; } }
        public string Name { get { return ListObject.Name; } set { ListObject.Name = value; } }
        public string Title { get { return ListObject.Title; } set { ListObject.Title = value; } }
        public int Type { get { return ListObject.Type; } set { ListObject.Type = value; } }
        public int ItemCount { get { return ListObject.ItemCount; } }
        public string RootFolderUrl { get { return ListObject.RootFolderUrl; } set { ListObject.RootFolderUrl = value; } }
        public object Flag { get { return ListObject.Flag; } set { ListObject.Flag = value; } }
        public ChangeType ChangeType { get { return ListObject.ChangeType; } set { ListObject.ChangeType = value; } }
        public int? ServerTemplate { get { return ListObject.ServerTemplate; } set { ListObject.ServerTemplate = value; } }
        public bool? Hidden { get { return ListObject.Hidden; } set { ListObject.Hidden = value; } }
        public int? ListTemplate { get { return ListObject.ListTemplate; } set { ListObject.ListTemplate = value; } }
        public DateTime ModifiedTime { get { return ListObject.ModifiedTime; } set { ListObject.ModifiedTime = value; } }
        public List<AveSecurityObject> DeleteRoleAssignments { get { return ListObject.DeleteRoleAssignments; } set { ListObject.DeleteRoleAssignments = value; } }//存放permission的删除事件

        private void Init(AveSiteCache siteCache, Guid webId, string listRootFolderUrl, IAveWeb web = null)
        {
            ListObject = new AveListObject { RootFolderUrl = listRootFolderUrl };
            AveWebCache webCache = new AveWebCache(siteCache, webId, web);
            ListCache = new AveListCache(webCache, ListObject);            
            //return new AveDiscoverConnection(site.ContentDatabase.DatabaseConnectionString);
        }        

        public AveDiscoverList() { }

        public AveDiscoverList(AveDiscoverFilterBase parent) : base(parent) { }

        [Obsolete("Please use AveDiscoverList(IAveSite site, IAveWeb web, string listRootFolderUrl, DiscoverModule module, AveObjectModelFactory objectModelFactory) instead.", true)]
        public AveDiscoverList(IAveSite site, string listRootFolderUrl, DiscoverModule module, AveObjectModelFactory objectModelFactory)
            : this(site, site.OpenWeb(listRootFolderUrl), listRootFolderUrl, module, objectModelFactory)
        {   }

        [Obsolete("AveDiscoverList(IAveSite site, IAveWeb web, string listRootFolderUrl, DateTime startTime, DateTime endTime, DiscoverModule module, AveObjectModelFactory objectModelFactory)", true)]
        public AveDiscoverList(IAveSite site, string listRootFolderUrl, DateTime startTime, DateTime endTime, DiscoverModule module, AveObjectModelFactory objectModelFactory)
            : this(site, site.OpenWeb(listRootFolderUrl), listRootFolderUrl, startTime, endTime, module, objectModelFactory)
        {   }
        /// <summary>
        /// For FB
        /// </summary>       
        public AveDiscoverList(IAveSite site, IAveWeb web, string listRootFolderUrl, DiscoverModule module, AveObjectModelFactory objectModelFactory)
            : this(site, web.ID, listRootFolderUrl, module, objectModelFactory, web)
        {
        }
        public AveDiscoverList(IAveSite site, Guid webId, string listRootFolderUrl, DiscoverModule module, AveObjectModelFactory objectModelFactory, IAveWeb web = null)
        {
            AveSiteCache siteCache = new AveSiteCache(site, objectModelFactory, module);
            Init(siteCache, webId, listRootFolderUrl.TrimEnd('/'), web);
            //ListCache.Query = AveObjectModelFactory.CreateObjectModelFactory("", null).CreateDiscoveryQuery(site, module);
            IsNewCreated = true;
        }
        /// <summary>
        /// For IB
        /// </summary>
        public AveDiscoverList(IAveSite site, IAveWeb web, string listRootFolderUrl, DateTime startTime, DateTime endTime, DiscoverModule module, AveObjectModelFactory objectModelFactory)
            : this(site, web.ID, listRootFolderUrl, startTime, endTime, module, objectModelFactory, web)
        {            
        }
        public AveDiscoverList(IAveSite site, Guid webId, string listRootFolderUrl, DateTime startTime, DateTime endTime, DiscoverModule module, AveObjectModelFactory objectModelFactory, IAveWeb web = null)
        {
            AveSiteCache siteCache = new AveSiteCache(site, objectModelFactory, module, startTime, endTime);
            Init(siteCache, webId, listRootFolderUrl.TrimEnd('/'), web);
            //ListCache.Query = AveObjectModelFactory.CreateObjectModelFactory("", null).CreateDiscoveryQuery(site, startTime, endTime, module);            
            IsNewCreated = true;
        }
        #region FB

        /// <summary>
        ///Query List Root Folder, 不会对结果进行Trim，因为如果Sub Folder符合Filter就找不到了，外围需要自己调用IsQualified来判断。
        /// </summary>
        /// <returns>List RootFolder</returns>
        public AveDiscoverFolder GetRootFolder(bool initListStructure=false)
        {
            AveDiscoverFolder rootFolder = new AveDiscoverFolder(this)
            {
                Obj = new AveItemObject(),
                FolderCache = new AveFolderCache(this.ListCache),
            };
            ListCache.InitRootFolder(ListObject, rootFolder.FolderCache, rootFolder.Obj, initListStructure);
            return rootFolder;
        }

        public AveDiscoverFolder GetRootFolderForArchiverSPQuery(SPOFolder SPOFolder)
        {
            AveDiscoverFolder rootFolder = new AveDiscoverFolder(this)
            {
                Obj = new AveItemObject(),
                FolderCache = new AveFolderCache(this.ListCache),
            };
            ListCache.InitRootFolderForArchiver(ListObject, rootFolder.FolderCache, rootFolder.Obj, SPOFolder);
            return rootFolder;
        }

        /// <summary>
        /// 用于查询List下所有Item，将所有ItemId缓存在root folder下
        /// </summary>
        /// <returns></returns>
        public AveDiscoverFolder GetRootFolderForFullDiscover()
        {
            AveDiscoverFolder rootFolder = new AveDiscoverFolder(this)
            {
                Obj = new AveItemObject(),
                FolderCache = new AveFolderCache(this.ListCache),
            };
            ListCache.InitRootFolderForFullDiscover(ListObject, rootFolder.FolderCache, rootFolder.Obj);
            return rootFolder;
        }

        /// <summary>
        /// Query list Views and fill the View relatived Item
        /// </summary>
        public Dictionary<Guid, AveViewObject> GetViews()
        {
            return ListCache.GetViews();
        }

        /// <summary>
        /// Query web ContentTypes and fill the contentType relatived folder
        /// </summary>
        public Dictionary<byte[], AveContentTypeObject> GetContentTypes()
        {
            return ListCache.GetContentTypes(this.ListObject);
        }

        #endregion

        #region IB
        public int GetListChanges(Guid webId)
        {
            return this.ListCache.Query.GetListChangedForRecords(webId, ListId);
        }

        public int GetListChangesCount(Guid webId)
        {
            return this.ListCache.Query.GetListChangedCount(webId, ListId);
        }
        public Dictionary<string, object> GetListChangedItems(Guid webId)
        {
            return this.ListCache.Query.GetListChangedItems(webId, ListId);
        }

        public Dictionary<string, object> GetListChangedItems(Guid webId,DateTime startTime,DateTime endTime)
        {
            return this.ListCache.Query.GetListChangedItems(webId, ListId, startTime, endTime);
        }

        public Dictionary<string, object> GetListDeletedItems(Guid webId)
        {
            return this.ListCache.Query.GetListDeletedItems(webId, ListId);
        }

        public Dictionary<string, object> GetFolderChangedItems(Guid webId, Guid folderId, DateTime startTime, DateTime endTime)
        {
            return this.ListCache.Query.GetFolderChangedItems(webId, ListId, folderId, startTime, endTime);
        }

        public Dictionary<string, object> GetFolderAndSubFolderChangedItems(Guid webId, Guid folderId, DateTime startTime, DateTime endTime)
        {
            return this.ListCache.Query.GetFolderAndSubFolderChangedItems(webId, ListId, folderId, startTime, endTime);
        }

        /// <summary>
        /// Query All the  Changed Items in Current List, 不会对结果进行Trim，因为如果Sub Site符合Filter就找不到了，外围需要自己调用IsQualified来判断。
        /// </summary>
        /// <returns>List RootFolder</returns>
        public AveDiscoverFolder GetChangeRootFolder(List<AveDiscoverExtraItemBaseInfo> extraItems = null)
        {
            AveDiscoverFolder rootFolder = new AveDiscoverFolder(this)
            {
                Obj = new AveItemObject(),
                FolderCache = new AveFolderCache(this.ListCache)
            };
            ListCache.InitChangeRootFolder(ListObject, rootFolder.FolderCache, rootFolder.Obj, extraItems);
            return rootFolder;
        }

        public Dictionary<Guid, AveAlertObject> GetChangeAlerts()
        {
            return ListCache.GetChangeAlerts();
        }

        public Dictionary<byte[], AveContentTypeObject> GetChangeListContentTypes()
        {
            return ListCache.GetChangeListContentTypes();
        }

        public Dictionary<Guid, AveViewObject> GetChangeViews()
        {
            return ListCache.GetChangeViews();
        }

        public List<AveSecurityObject> GetChangeSecuritys()
        {
            var result = new List<AveSecurityObject>();
            foreach (var list in ListCache.GetChangeSecuritys().Values)
            {
                result.AddRange(list);
            }
            return result;
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            if (IsNewCreated && this.ListCache != null && this.ListCache.Query != null)
            {
                this.ListCache.Query.Dispose();
            }
            ListCache = null;
        }

        #endregion

        #region FilterBase Members

        public override ObjectInfoBase GetFilterObjectInfo(List<FilterPolicy> policies)
        {
            return FilterAnalyser.GetListFilterInfo(policies, this.ListCache.AveWeb.GetList(this.RootFolderUrl));
        }

        #endregion

        public IAveList GetListObject()
        {
            return this.ListCache.AveWeb.GetList(this.RootFolderUrl);
        }

        public string GetListTitle()
        {
            return ListCache.GetListTitle(this.ListObject);
        }

        #region support migration license
        public long GetObjectChangedSize(Guid siteId, Guid webId, Guid listId,string folderUrl,DateTime beginTime)
        {
            return ListCache.Query.GetObjectChangedSize(siteId, webId, listId, folderUrl, beginTime); 
        }
        public long GetListSize(Guid siteId, Guid webId, Guid listId)
        {
            return ListCache.Query.GetListSize(siteId, webId, listId);

        }
        public long GetFolderSize(Guid siteId, Guid webId, Guid listId, string folderUrl)
        {
            return ListCache.Query.GetFolderSize(siteId, webId, listId,  folderUrl);
        }
        #endregion

        public PreDiscoverDesignListResult PreDiscoverDesignList(IAveList list, bool includeGhostFile = false, bool includeEmptyFolder = false)
        {
            return this.ListCache.Query.PreDiscoverDesignList(list.ParentWeb.Site.Url, list.ParentWeb.ID, list.ID, includeGhostFile, includeEmptyFolder);
        }
    }
}
