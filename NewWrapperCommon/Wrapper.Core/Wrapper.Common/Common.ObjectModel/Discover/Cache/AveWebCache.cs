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
    public class AveWebCache : AveDiscoverCache, IDisposable
    {
        public AveWebCacheParameter AveWebCacheParameter { get; private set; }

        public IAveWeb AveWeb { get { return this.AveWebCacheParameter.AveWeb; } }
        public Guid WebId { get { return this.AveWebCacheParameter.WebId; } }
        public IAveSite AveSite { get { return this.AveWebCacheParameter.AveSite; } }
        public Guid SiteId { get { return this.AveWebCacheParameter.SiteId; } }
        /// <summary>
        /// 所有此类的构造方法均要有此方法才能确保Query正常使用
        /// </summary>
        /// <param name="parentSite"></param>
        private AveWebCache(AveDiscoverCache parent, IAveSite site, Guid webId, IAveWeb web = null)
        {
            if (parent != null)
            {
                this.Query = parent.Query;
            }
            if (site != null)
            {
                this.AveWebCacheParameter = new AveWebCacheParameter(site, webId, web);
            }           
        }
        public AveWebCache (AveSiteCache parentSite, Guid webId, IAveWeb web = null)
            : this(parentSite, parentSite.AveSite, webId, web)
        {
        }
        public AveWebCache(AveWebCache parentWeb, Guid webId)
            : this(parentWeb, parentWeb.AveSite, webId)
        {         
        }
        /// <summary>
        /// 此方法会将WebObject填充，请在直接创建WebCache时调用
        /// </summary>
        /// <param name="parentSite"></param>
        /// <param name="webObject"></param>
        public AveWebCache(AveSiteCache parentSite, AveWebObject webObject)
            :this(parentSite, parentSite.AveSite, Guid.Empty)
        {
            this.InitDiscoverWeb(webObject);
            this.AveWebCacheParameter = new AveWebCacheParameter(parentSite.AveSite, webObject.WebID);
        }
        /// <summary>
        /// For Unit Test to create WebCache module
        /// </summary>
        public AveWebCache()
        {}
        #region FB

        public Dictionary<Guid, AveWebObject> GetSubWebs(bool includeRecycleBin)
        {
            return Query.GetSubWebs(this.SiteId, this.WebId, includeRecycleBin);
        }
        
        public Dictionary<Guid, AveListObject> GetLists(bool includeRecycleBin)
        {
            return Query.QueryWebListForFB(this.SiteId, this.WebId, includeRecycleBin);
        }

        #endregion

        #region IB

        public Dictionary<Guid, AveListObject> GetChangeLists()
        {
            return Query.QueryListForIB(this.SiteId, this.WebId);
        }

        public Dictionary<int, List<AveSecurityObject>> GetChangeSecuritys()
        {
            return Query.QueryWebSecurityForIB(this.SiteId, this.WebId);
        }

        #endregion

        public void InitDiscoverWeb(AveWebObject webObj)
        {
             Query.InitDiscoverWeb(this, webObj);
        }

        /// <summary>
        /// This should be only for O365
        /// </summary>
        /// <returns></returns>
        public Dictionary<byte[], AveContentTypeObject> GetContentTypes()
        {
            return Query.QueryWebContentTypeForFB(this.SiteId, this.WebId);
        }

        /// <summary>
        /// If it's a local farm, please use this method to get content types fast. Added by Austin
        /// </summary>
        /// <returns></returns>
        public Dictionary<byte[], AveContentTypeObject> GetContentTypesFast()
        {
            return Query.QueryWebContentTypeForFB(this.SiteId, this.AveWeb.ServerRelativeUrl);
        }

        public void Dispose()
        {
            if (this.AveWebCacheParameter != null)
            {
                this.AveWebCacheParameter.Dispose();
            }
        }
        #region support for migration license
        public long GetWebSize(Guid siteId, Guid webId)
        {
            return Query.GetWebSize(siteId, webId);
        }
        #endregion

    }
    /// <summary>
    /// 此类仅在AveCache中作为引用存储使用，目的是为了让关联的Cache可以使用一个ParentAveWeb
    /// </summary>
    public class AveWebCacheParameter : IDisposable
    {
        #region Reload SPRequest
        
        private const int RELOAD_SPAN_IN_HOURS = 12;

        /// <summary>
        /// SPRequest对象超时时间为24小时，需要重新Reload
        /// PR Item不能调用Reload方法，目前只有Filter Policy才会使用该方法，如果PR Item需要使用的话，需要修改这里
        /// </summary>
        /// <returns></returns>
        public bool ReloadSPRequestIfTimeout()
        {
            if (AveSite.LastReloadTimeUTC != DateTime.MinValue && AveSite.LastReloadTimeUTC.AddHours(RELOAD_SPAN_IN_HOURS) < DateTime.UtcNow)
            {
                AveSite.ReloadSite();
                if (this.mAveWeb != null)
                {
                    this.mAveWeb.ReloadWeb();
                }
                return true;
            }
            return false;
        }

        #endregion

        public Guid WebId { get; private set; }
        public Guid SiteId { get; private set; }
        public IAveSite AveSite { get; private set; }
        private IAveWeb mAveWeb;
        public IAveWeb AveWeb
        {
            get
            {
                ReloadSPRequestIfTimeout();
                if (mAveWeb == null)
                {
                    mAveWeb = this.AveSite.OpenWeb(this.WebId);
                }
                return mAveWeb;
            }
        }

        public AveWebCacheParameter(IAveSite site, Guid webId, IAveWeb web = null)
        {
            this.AveSite = site;
            this.WebId = webId;
            this.SiteId = site.ID;
            this.mAveWeb = web;
        }

        public void Dispose()
        {
            if (mAveWeb != null)
            {
                mAveWeb.Dispose();
            }
        }
    }
}
