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
using System.Linq;
using System.Collections.Generic;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Contract.CommonFilter;
using AvePoint.Common.FilterEngine;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Discovery
{
    public class AveDiscoverWeb : AveDiscoverFilterBase, IDisposable
    {
        private AveObjectModelFactory webObjectModelFactory;

        private bool IsNewCreated { get; set; }//从Web级别进入Discover的Query

        internal AveWebCache WebCache { get; set; }

        internal AveWebObject WebObject { get; set; }

        public Guid WebID { get { return WebObject.WebID; } set { WebObject.WebID = value; } }
        public string Title { get { return WebObject.Title; } set { WebObject.Title = value; } }
        public string FullUrl { get { return WebObject.FullUrl; } set { WebObject.FullUrl = value; } }
        public string Name { get { return WebObject.Name; } set { WebObject.Name = value; } }
        public bool NavigationChanged { get { return WebObject.NavigationChanged; } set { WebObject.NavigationChanged = value; } }
        public DateTime EventTime { get { return WebObject.EventTime; } set { WebObject.EventTime = value; } }
        public ChangeType ChangeType { get { return WebObject.ChangeType; } set { WebObject.ChangeType = value; } }
        public Guid AppInstanceId { get { return WebObject.AppInstanceId; } set { WebObject.AppInstanceId = value; } }
        public List<AveSecurityObject> DeleteSecurities { get { return WebObject.DeleteSecurities; } set { WebObject.DeleteSecurities = value; } }//存放permission及permission level的删除事件
        public IAveWeb AveWeb { get { return WebCache.AveWeb; } }

        private void Init(AveSiteCache siteCache, string webRelativeUrl)
        {
            WebObject = new AveWebObject { FullUrl = webRelativeUrl.TrimEnd('/') };
            WebCache = new AveWebCache(siteCache, WebObject);
            //return new AveDiscoverConnection(site.ContentDatabase.DatabaseConnectionString);
        }

        //private void InitDiscoverWeb()
        //{
        //    WebCache.InitDiscoverWeb(WebObject);
        //}

        public AveDiscoverWeb() { }

        public AveDiscoverWeb(AveDiscoverFilterBase parent) : base(parent) { }

        public AveDiscoverWeb(AveDiscoverFilterBase parent, AveObjectModelFactory objectModelFactory)
            : base(parent)
        {
            webObjectModelFactory = objectModelFactory;
        }
        /// <summary>
        /// For FB
        /// </summary>
        public AveDiscoverWeb(IAveSite site, string webRelativeUrl, DiscoverModule module, AveObjectModelFactory objectModelFactory)
        {
            AveSiteCache siteCache = new AveSiteCache(site, objectModelFactory, module);
            Init(siteCache, webRelativeUrl);
            //WebCache.Query = objectModelFactory.CreateDiscoveryQuery(site, module);
            IsNewCreated = true;
            //InitDiscoverWeb();
            webObjectModelFactory = objectModelFactory;
        }
        /// <summary>
        /// For IB
        /// </summary>
        public AveDiscoverWeb(IAveSite site, string webRelativeUrl, DateTime startTime, DateTime endTime, DiscoverModule module, AveObjectModelFactory objectModelFactory)
        {
            AveSiteCache siteCache = new AveSiteCache(site, objectModelFactory, module, startTime, endTime);
            Init(siteCache, webRelativeUrl);
            //WebCache.Query = objectModelFactory.CreateDiscoveryQuery(site, startTime, endTime, module);
            IsNewCreated = true;
            //InitDiscoverWeb();
            webObjectModelFactory = objectModelFactory;
        }

        private AveDiscoverList GetSystemFolderList()
        {
            return new AveDiscoverList(this)
             {
                 ListObject = new AveListObject
                 {
                     Name = "{System Folder}",
                     Title = "{System Folder}",
                     Type = 1 //Set system folder as DocList Type
                 },
                 ListCache = new AveListCache(this.WebCache, Guid.Empty)
             };
        }
        #region FB

        public AveDiscoverFolder GetRootFolder()
        {
            return GetSystemFolderList().GetRootFolder();
        }
        /// <summary>
        /// 不会对结果进行Filter Trim，因为如果Sub Site符合Filter就找不到了，外围需要自己调用IsQualified来判断。
        /// </summary>
        public Dictionary<Guid, AveDiscoverWeb> GetSubWebs(bool excludeAppWeb = false)
        {
            Dictionary<Guid, AveDiscoverWeb> subWebs = new Dictionary<Guid, AveDiscoverWeb>();
            Dictionary<Guid, AveWebObject> subWebObjs = this.WebCache.GetSubWebs();
            foreach (var temp in subWebObjs)
            {
                AveWebObject webObj = temp.Value;
                if (webObj.IsAppWeb && excludeAppWeb)
                {
                    continue;
                }
                AveDiscoverWeb web = new AveDiscoverWeb(this, this.webObjectModelFactory)
                {
                    WebObject = webObj,
                    WebCache = new AveWebCache(this.WebCache, webObj.WebID),
                };
                subWebs.Add(temp.Key, web);
            }
            return subWebs;
        }

        public Dictionary<Guid, AveDiscoverList> GetLists(bool throwException = false)
        {
            Dictionary<Guid, AveDiscoverList> lists = new Dictionary<Guid, AveDiscoverList>();
            if (HasFilter && ResultMode.HasMode(FilterResultMode.Trim) && !IsQualified())
            {
                return lists;
            }
            Dictionary<Guid, AveListObject> listObjs = this.WebCache.GetLists(throwException);
            foreach (var kvp in listObjs)
            {
                AveListObject listObj = kvp.Value as AveListObject;
                AveDiscoverList discoverList = new AveDiscoverList(this)
                {
                    ListObject = listObj,
                    ListCache = new AveListCache(this.WebCache, listObj.ListId)
                };
                lists.Add(discoverList.ListId, discoverList);
            }
            lists.Add(Guid.Empty, GetSystemFolderList());
            return GetFilterLists(lists);
        }

        /// <summary>
        /// Query web ContentTypes and fill the contentType relatived folder
        /// </summary>
        public Dictionary<byte[], AveContentTypeObject> GetContentTypes()
        {
            return WebCache.GetContentTypes();
        }

        #endregion

        #region IB

        public Dictionary<Guid, AveDiscoverList> GetChangeLists()
        {
            Dictionary<Guid, AveDiscoverList> lists = new Dictionary<Guid, AveDiscoverList>();
            if (HasFilter && ResultMode.HasMode(FilterResultMode.Trim) && !IsQualified())
            {
                return lists;
            }
            Dictionary<Guid, AveListObject> listObjs = this.WebCache.GetChangeLists();
            foreach (AveListObject listObj in listObjs.Values)
            {
                AveDiscoverList discoverList = new AveDiscoverList(this)
                {
                    ListObject = listObj,
                    ListCache = new AveListCache(this.WebCache, listObj.ListId)
                };
                lists.Add(discoverList.ListId, discoverList);                
            }
            return GetFilterLists(lists);
        }

        public Dictionary<byte[], AveContentTypeObject> GetChangeContentTypes()
        {
            return WebCache.GetChangeContentTypes();
        }

        public List<AveSecurityObject> GetChangeSecurityChanges()
        {
            var result = new List<AveSecurityObject>();
            foreach (var list in WebCache.GetChangeSecuritys().Values)
            {
                result.AddRange(list);
            }
            return result;
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// 做Filter job的时候，需要保存DiscoverWeb，防止提前释放。
        /// </summary>
        public void Dispose()
        {
            if (this.WebCache != null)
            {
                if (IsNewCreated && this.WebCache.Query != null)
                {
                    this.WebCache.Query.Dispose();
                }
                this.WebCache.Dispose();
            }
        }

        #endregion

        #region Filter

        private Dictionary<Guid, AveDiscoverList> GetFilterLists(Dictionary<Guid, AveDiscoverList> lists)
        {
            if (HasFilterWithLevel(PolicyLevel.List | PolicyLevel.Library) && ResultMode.HasMode(FilterResultMode.Trim))
            {
                return lists.Values.Where(list =>
                    {
                        try
                        {
                            return !string.IsNullOrEmpty(list.RootFolderUrl) && this.FilterEngine.IsQualified(list.GetFilterObjectInfo(this.FilterPolicies));
                        }
                        catch (Exception ex)
                        {
                            log.Warn("An error occurred when filter lists. Name:{0}, Reason:{1}.", list.Name, ex.ToString());
                            return false;
                        }
                    }).ToDictionary(list => list.ListId);
            }
            return lists;
        }

        #region FilterBase Members

        public override ObjectInfoBase GetFilterObjectInfo(List<FilterPolicy> policies)
        {
            return FilterAnalyser.GetWebFilterInfo(policies, this.WebCache.AveWeb);
        }

        #endregion

        #endregion

        #region support for migration license
        public long GetWebSize(Guid siteId, Guid webId)
        {
           return this.WebCache.GetWebSize(siteId, webId);
        }
        #endregion

        public Dictionary<Guid, AveDiscoverAppDefinition> GetAppDefinitions()
        {
            Dictionary<Guid, AveDiscoverAppDefinition> lists = new Dictionary<Guid, AveDiscoverAppDefinition>();
            IAveAppCatalog appCatalog = webObjectModelFactory.CreateAppCatalog();
            if (appCatalog != null)
            {
                IList<IAveAppInstance> appInstances = appCatalog.GetAppInstances(AveWeb);
                foreach (IAveAppInstance instance in appInstances)
                {
                    AveDiscoverAppDefinition definition = new AveDiscoverAppDefinition() { ProductId = instance.App.ProductId, Name = instance.Title, InstanceId = instance.Id };
                    definition.AppFullUrl = instance.AppWebFullUrl == null ? string.Empty : instance.AppWebFullUrl.ToString();
                    lists.Add(definition.ProductId, definition);
                }
            }
            return lists;
        }

        public List<AveProjectObject> GetProjects()
        {
            return this.WebCache.GetProjects();
        }

        public Dictionary<Guid, AveDiscoverWeb> GetAppWebs()
        {
            Dictionary<Guid, AveDiscoverWeb> subWebs = new Dictionary<Guid, AveDiscoverWeb>();
            Dictionary<Guid, AveWebObject> subWebObjs = this.WebCache.GetSubWebs();
            foreach (var temp in subWebObjs)
            {
                AveWebObject webObj = temp.Value;
                if (webObj.IsAppWeb)
                {
                    AveDiscoverWeb web = new AveDiscoverWeb(this, this.webObjectModelFactory)
                    {
                        WebObject = webObj,
                        WebCache = new AveWebCache(this.WebCache, webObj.WebID),
                    };
                    subWebs.Add(temp.Key, web);
                }

            }
            return subWebs;
        }

        public Guid GetAppInstanceIDByProductID(Guid productId)
        {
            return AveWeb.GetAppInstancesByProductId(productId)[0].Id;
        }
    }

    public class AveDiscoverAppDefinition
    {

        public Guid ProductId
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
        public string AppFullUrl
        {
            get;
            set;
        }
        public Guid InstanceId
        {
            get;
            set;
        }

        public bool IsUpdateAvailable
        {
            get;
            set;
        }

        public string VersionString
        {
            get;
            set;
        }
    }
}
