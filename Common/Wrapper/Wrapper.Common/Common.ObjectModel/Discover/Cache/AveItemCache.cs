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
    public class AveItemCache : AveDiscoverCache
    {
        public int? ItemId { get; set; }
        public AveWebCacheParameter AveWebCacheParameter { get; private set; }
        public IAveWeb AveWeb { get { return this.AveWebCacheParameter.AveWeb; } }
        public Guid WebId { get { return this.AveWebCacheParameter.WebId; } }
        public IAveSite AveSite { get { return this.AveWebCacheParameter.AveSite; } }
        public Guid SiteId { get { return this.AveWebCacheParameter.SiteId; } }
        public Guid ListId { get; private set; }
        //public AveFolderCache ParentFolder { get; set; }
        //public AveWebCache ParentWeb { get; internal set; }
        //public AveSiteCache ParentSite { get { return this.ParentWeb.ParentSite; } }
        /// <summary>
        /// 所有此类的构造方法均要有此方法才能确保Query正常使用
        /// </summary>
        /// <param name="parentFolder"></param>
        private AveItemCache(AveDiscoverCache parent, AveWebCacheParameter webParameter)
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
        public AveItemCache(AveFolderCache parentFolder, int? itemId = null)
            : this(parentFolder, parentFolder.AveWebCacheParameter)
        {
            this.ListId = parentFolder.ListId;
            this.ItemId = itemId;
        }
        public AveItemCache(AveListCache parentList, int? itemId = null)
            : this(parentList,parentList.AveWebCacheParameter)
        {
            this.ListId = parentList.ListId;
            this.ItemId = itemId;
        }
        public AveItemCache(AveWebCache parentWeb, Guid listId, int? itemId = null)
            : this(parentWeb, parentWeb.AveWebCacheParameter)
        {
            this.ListId = listId;
            this.ItemId = itemId;
        }
        public AveItemCache(AveSiteCache parentSiteCache, Guid parentWebId, Guid parentListId, int? itemId = null)
            : this(new AveWebCache(parentSiteCache, parentWebId), parentListId, itemId)
        {
        }
        /// <summary>
        /// For Unit Test to create ItemCache module
        /// </summary>
        public AveItemCache()
        {}
        #region IB

        public Dictionary<int, List<AveSecurityObject>> GetChangeSecuritys()
        {
            if (ItemId.HasValue)
            {
                //return Query.QueryItemSecurityForIB(ParentFolder.ParentWeb.WebID, ParentFolder.ParentList.ListID, ItemId.Value);
                return Query.QueryItemSecurityForIB(this.WebId, this.ListId, ItemId.Value);
            }
            else
            {
                return new Dictionary<int, List<AveSecurityObject>>();
            }
        }

        #endregion
    }
}
