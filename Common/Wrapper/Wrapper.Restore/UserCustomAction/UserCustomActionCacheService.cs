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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Wrapper.Restore
{
    
    public class UserCustomActionCache
    {
        public IList<AveUserCustomActionInfo> CustomActions { get; set; }
        public AveRestoreMode RestoreMode { get; set; }
        public UserCustomActionCache(AveRestoreMode restoreMode, IList<AveUserCustomActionInfo> data)
        {
            CustomActions = data;
            RestoreMode=restoreMode;
        }
    }
    public class SiteUserCustomActionCache : UserCustomActionCache
    {
        public SiteUserCustomActionCache(Guid siteId, AveRestoreMode restoreMode, IList<AveUserCustomActionInfo> data)
          : base(restoreMode, data)
        {
            SiteId = siteId;
        }
        public Guid SiteId { get; set; }
    }
    public class WebUserCustomActionCache : SiteUserCustomActionCache
    {
        public WebUserCustomActionCache(Guid siteId, Guid webId, AveRestoreMode restoreMode, IList<AveUserCustomActionInfo> data)
           : base(siteId, restoreMode, data)
        {
            WebId = webId;
        }
        public Guid WebId { get; set; }
    }
    public class ListUserCustomActionCache : WebUserCustomActionCache
    {
        public ListUserCustomActionCache(Guid siteId, Guid webId, Guid listId, AveRestoreMode restoreMode, IList<AveUserCustomActionInfo> data)
            : base(siteId,webId,restoreMode,data)
        {
            ListId = listId;
        }
       public Guid ListId { get; set; }
    }
    public class UserCustomActionCacheService
    {
        protected AveSPSite ParentAveSite { get; set; }
        Queue<UserCustomActionCache> Cache;
        protected static AvePoint.GCommon.AveLogger mLog = AvePoint.GCommon.AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public UserCustomActionCacheService(AveSPSite aveSPSite)
        {
            ParentAveSite = aveSPSite;
            Cache = new Queue<UserCustomActionCache>();
        }

        public void CacheData(IList<AveUserCustomActionInfo> data,AveRestoreMode restoreMode)
        {
            var cache = new SiteUserCustomActionCache(ParentAveSite.SPSite.ID, restoreMode, data);
            Cache.Enqueue(cache);
            //restore site control 
            mLog.Info($"[SiteUserCustomActionCache]_Url:{ParentAveSite.SPSite.ServerRelativeUrl},CurrentCount:{Cache.Count()}");
        }

        public void CacheData(AveSPWeb aveSPWeb,IList<AveUserCustomActionInfo> data, AveRestoreMode restoreMode)
        {
            var webId = aveSPWeb.SPWeb.ID;
            var cache = new WebUserCustomActionCache(ParentAveSite.SPSite.ID, webId, restoreMode, data);
            Cache.Enqueue(cache);
            //restore web control 
            mLog.Info($"[WebUserCustomActionCache]_Url:{ParentAveSite.SPSite.ServerRelativeUrl},CurrentCount:{Cache.Count()}");
        }

        public void CacheData(AveSPList aveSPList,IList<AveUserCustomActionInfo> data, AveRestoreMode restoreMode)
        {
            var webId = aveSPList.ParentWeb.SPWeb.ID;
            var listId = aveSPList.SPList.ID;
            var cache = new ListUserCustomActionCache(ParentAveSite.SPSite.ID,webId,listId, restoreMode, data);
            Cache.Enqueue(cache);
            //restore list control 
            mLog.Info($"[ListUserCustomActionCache]_Url:{ParentAveSite.SPSite.ServerRelativeUrl},CurrentCount:{Cache.Count()}");
        }

        public void RestoreFromCache()
        {
            IAveSite spSite=ParentAveSite.SPSite;
            IAveWeb spWeb=null;
            AveSiteMappingManager mapping=ParentAveSite.MappingManager.SiteMappingManager;
            while (Cache.Count > 0)
            {
                var item = Cache.Dequeue();
                if (item is ListUserCustomActionCache)
                {
                    var listData = item as ListUserCustomActionCache;
                    spWeb = SwitchWeb(spSite, spWeb, listData.WebId);
                    var list = spWeb.Lists.GetById(listData.ListId);
                    var restoreControl = new AveSPListUserCustomActionCollection(list, mapping);
                    restoreControl.Restore(item.CustomActions, item.RestoreMode);
                }
                else if (item is WebUserCustomActionCache)
                {
                    var webdata = item as WebUserCustomActionCache;
                    spWeb = SwitchWeb(spSite, spWeb, webdata.WebId);
                    var restoreControl = new AveSPWebUserCustomActionCollection(spWeb,mapping);
                    restoreControl.Restore(item.CustomActions, item.RestoreMode);
                }
                else if (item is SiteUserCustomActionCache)
                {

                    var restoreControl = new AveSPSiteUserCustomActionCollection(spSite, mapping);
                    restoreControl.Restore(item.CustomActions, item.RestoreMode);
                }
            }
        }

        private IAveWeb SwitchWeb(IAveSite parent,IAveWeb original,Guid webId)
        {
            if (original == null)
            {
                return parent.OpenWeb(webId);
            }
            else if (original.ID == webId)
            {
                return original;
            }
            else
            {
                original.Dispose();
                return parent.OpenWeb(webId);
            }
        }
    }
}
