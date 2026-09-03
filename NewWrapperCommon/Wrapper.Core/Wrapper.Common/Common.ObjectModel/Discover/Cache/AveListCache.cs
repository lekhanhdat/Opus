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

namespace AvePoint.Wrapper.Common
{
    public class AveListCache : AveDiscoverCache
    {

        public Guid ListId { get; set; }
        public ChangeType ChangeType { get; set; }
        public AveWebCacheParameter AveWebCacheParameter { get; private set; }
        public IAveWeb AveWeb { get { return this.AveWebCacheParameter.AveWeb; } }
        public Guid WebId { get { return this.AveWebCacheParameter.WebId; } }
        public IAveSite AveSite { get { return this.AveWebCacheParameter.AveSite; } }
        public Guid SiteId { get { return this.AveWebCacheParameter.SiteId; } }
        //public AveWebCache ParentWeb { get; set; }

        /// <summary>
        /// 所有此类的拓展构造方法均要有此方法才能确保Query正常使用
        /// </summary>
        /// <param name="parentWeb"></param>
        private AveListCache(AveDiscoverCache parent, AveWebCacheParameter webParameter)
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

        public AveListCache(AveWebCache parentWeb, Guid listId)
            : this(parentWeb, parentWeb.AveWebCacheParameter)
        {
            this.ListId = listId;
        }
        
        public AveListCache(AveWebCache parentWeb, AveListObject listObject)
            :this(parentWeb, parentWeb.AveWebCacheParameter)
        {
            this.InitDiscoverList(listObject);
            this.ListId = listObject.ListId;
        }
        
        /// <summary>
        /// For Unit Test to create ListCache module
        /// </summary>
        public AveListCache()
        {}
        #region FB

        public void InitRootFolder(AveListObject listObject, AveFolderCache folderCache, AveItemObject folderObject)
        {
            folderObject.ChangeType = this.ChangeType;
            folderObject.ObjType = ItemType.Folder;

            if (listObject.ListId.Equals(Guid.Empty))
            {
                Query.QueryWebRootFolder(this, folderObject);
            }
            else
            {
                Query.QueryListRootFolder(this, listObject, folderObject);
            }
        }

        public Dictionary<Guid, AveViewObject> GetViews()
        {
            return Query.QueryListViewForFB(this.SiteId, this.WebId, this.ListId);
        }
        public void DiscoverAllListContent(AveItemObject rootFolderObject, int maxItemCount, bool includeRecycleBin, bool includeSystemFolder, DiscoverStubOption discoverStubOption)
        {
            rootFolderObject.ClearSubFoldersCache();
            rootFolderObject.ClearSubItemsCache();
            Query.DiscoverAllListContent(this, rootFolderObject, maxItemCount, includeRecycleBin, includeSystemFolder);
        }

        #endregion

        #region IB

        public Dictionary<Guid, AveAlertObject> GetChangeAlerts()
        {
            return Query.QueryListAlertForIB(this.SiteId, this.WebId, this.ListId);
        }

        public Dictionary<Guid, AveViewObject> GetChangeViews()
        {
            return Query.QueryListViewForIB(this.SiteId, this.WebId, this.ListId);
        }

        public Dictionary<int, List<AveSecurityObject>> GetChangeSecuritys()
        {
            return Query.QueryListSecurityForIB(this.SiteId, this.WebId, this.ListId);
        }

        public Dictionary<byte[], AveContentTypeObject> GetChangeListContentTypes()
        {
            return Query.QueryListContentTypeForIB(this.SiteId, this.WebId, this.ListId);
        }
        //public void InitChangeRootFolder(AveListObject listObject, AveFolderCache folderCache, AveItemObject folderObject, DiscoverModeForSOIB discoverMode = DiscoverModeForSOIB.UseBoth, List<AveDiscoverExtraItemBaseInfo> extraItems = null)
        public void InitChangeRootFolder(AveListObject listObject, AveFolderCache folderCache, AveItemObject folderObject, List<AveDiscoverExtraItemBaseInfo> extraItems = null)
        {
            //folderCache.ParentList = this;
            //folderCache.ListId = this.ListId;
            //folderCache.ParentWeb = ParentWeb;
            //folderCache.Query = this.Query;

            folderObject.ChangeType = this.ChangeType;
            folderObject.ObjType = ItemType.Folder;

            if (this.ListId.Equals(Guid.Empty))
            {
                Query.QueryWebRootFolder(this, folderObject);              
                Query.QuerySystemListItemForIB(folderCache, folderObject, extraItems);              
            }
            else
            {
                Query.QueryListRootFolder(this, listObject, folderObject);
                Query.QueryListItemForIB(folderCache, folderObject,listObject, extraItems);               
            }
        }

        #endregion

        public void InitDiscoverList(AveListObject listObject)
        {
            Query.InitDiscoverList(this, listObject);
        }

    }
    /// <summary>
    /// Default value is UseBoth.
    /// </summary>
     [Obsolete("no use now, will remove later")]
    public enum DiscoverModeForSOIB
    {
        UseEventCacheOnly = 1,
        UseRCDBOnly = 2,
        UseBoth = 3,
    }
}
