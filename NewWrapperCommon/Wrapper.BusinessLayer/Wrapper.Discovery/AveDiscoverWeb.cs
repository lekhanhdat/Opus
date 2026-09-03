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
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Xml;

namespace AvePoint.Wrapper.Discovery
{
    public class AveDiscoverWeb : AveDiscoverFilterBase, IAveDiscoverWeb
    {
        //private static readonly AveLogger mLog = AveLogger.GetInstance(typeof(AveDiscoverWeb));

        private AveObjectModelFactory webObjectModelFactory;

        private bool IsNewCreated { get; set; }//从Web级别进入Discover的Query

        internal AveWebCache WebCache { get; set; }

        internal AveWebObject WebObject { get; set; }

        public Guid WebID { get { return WebObject.WebID; } set { WebObject.WebID = value; } }
        public string Title { get { return WebObject.Title; } set { WebObject.Title = value; } }
        public string FullUrl { get { return WebObject.FullUrl; } set { WebObject.FullUrl = value; } }
        public Guid AppInstanceId { get { return WebObject.AppInstanceId; } set { WebObject.AppInstanceId = value; } }
        public string Name { get { return WebObject.Name; } set { WebObject.Name = value; } }
        public bool NavigationChanged { get { return WebObject.NavigationChanged; } set { WebObject.NavigationChanged = value; } }
        public DateTime EventTime { get { return WebObject.EventTime; } set { WebObject.EventTime = value; } }
        public ChangeType ChangeType { get { return WebObject.ChangeType; } set { WebObject.ChangeType = value; } }
        public byte[] DeleteTransactionId { get { return WebObject.DeleteTransactionId; } set { WebObject.DeleteTransactionId = value; } }
        public List<AveSecurityObject> DeleteSecurities { get { return WebObject.DeleteSecurities; } set { WebObject.DeleteSecurities = value; } }//存放permission及permission level的删除事件
        public IAveWeb AveWeb { get { return WebCache.AveWeb; } }
        /// <summary>
        /// 表示Column是否改变
        /// 
        /// 值可能有多值，不一定是单值
        /// </summary>
        public ChangeType ColumnChangeType { get { return WebObject.ColumnChangeType; } }
        /// <summary>
        /// 表示Content Type是否改变
        /// 
        /// 值可能有多值，不一定是单值
        /// </summary>
        public ChangeType ContentTypeChangeType { get { return WebObject.ContentTypeChangeType; } }
        /// <summary>
        /// 表示Navigation是否改变
        /// 
        /// 值可能有多值，不一定是单值
        /// </summary>
        public ChangeType NavigationChangeType { get { return WebObject.NavigationChangeType; } }
        /// <summary>
        /// 表示Permission Level是否改变
        /// 
        /// 值可能有多值，不一定是单值
        /// </summary>
        public ChangeType PermissionLevelChangeType { get { return WebObject.PermissionLevelChangeType; } }
        /// <summary>
        /// 表示Role Assignments是否改变
        /// 
        /// 值可能有多值，不一定是单值
        /// </summary>
        public ChangeType RoleAssignmentsChangeType { get { return WebObject.RoleAssignmentsChangeType; } }

        private void Init(AveSiteCache siteCache, string webRelativeUrl)
        {
            WebObject = new AveWebObject { FullUrl = webRelativeUrl.Trim('/') };
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
        public AveDiscoverWeb(IAveSite site, string webRelativeUrl, DiscoverModule module,AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory)
        {
            AveSiteCache siteCache = new AveSiteCache(site, objectModelFactory, kind, module);
            Init(siteCache, webRelativeUrl);
            //WebCache.Query = objectModelFactory.CreateDiscoveryQuery(site, module);
            IsNewCreated = true;
            //InitDiscoverWeb();
            webObjectModelFactory = objectModelFactory;
        }
        /// <summary>
        /// For IB
        /// </summary>
        public AveDiscoverWeb(IAveSite site, string webRelativeUrl, DateTime startTime, DateTime endTime, DiscoverModule module,AveDiscoveryKind kind, AveObjectModelFactory objectModelFactory)
        {
            AveSiteCache siteCache = new AveSiteCache(site, objectModelFactory, kind, module, startTime, endTime);
            Init(siteCache, webRelativeUrl);
            //WebCache.Query = objectModelFactory.CreateDiscoveryQuery(site, startTime, endTime, module);
            IsNewCreated = true;
            //InitDiscoverWeb();
            webObjectModelFactory = objectModelFactory;
        }

        private AveDiscoverList GetSystemFolderList()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverWeb.GetSystemFolderList"))
            {
                return new AveDiscoverSystemFolderList(this)
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
        }
        #region FB

        [Obsolete("Use IAveDiscoverFolder IAveDiscoverWeb.GetRootFolder() instead.")]
        public AveDiscoverFolder GetRootFolder()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverWeb.GetRootFolder"))
            {
                try
                {
                    return GetSystemFolderList().GetRootFolder();
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetRootFolder.");
                    log.Log(AveLogLevel.WARN, "Exception detail:{0}", ex.ToString());
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetWebRootFolderError);
                }
            }
        }

        public Dictionary<Guid, AveDiscoverWeb> GetSubWebs()
        {
            return GetSubWebs(false);
        }

        /// <summary>
        /// 不会对结果进行Filter Trim，因为如果Sub Site符合Filter就找不到了，外围需要自己调用IsQualified来判断。
        /// </summary>
        [Obsolete("Use Dictionary<Guid, IAveDiscoverWeb> IAveDiscoverWeb.GetSubWebs() instead.")]
        public Dictionary<Guid, AveDiscoverWeb> GetSubWebs(bool includeRecycleBin)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverWeb.GetSubWebs"))
            {
                try
                {
                    Dictionary<Guid, AveDiscoverWeb> subWebs = new Dictionary<Guid, AveDiscoverWeb>();
                    Dictionary<Guid, AveWebObject> subWebObjs = this.WebCache.GetSubWebs(includeRecycleBin);
                    foreach (var temp in subWebObjs)
                    {
                        AveWebObject webObj = temp.Value;
                        if (!webObj.IsAppWeb)
                        {
                            AveDiscoverWeb web = new AveDiscoverWeb(this, webObjectModelFactory)
                            {
                                WebObject = webObj,
                                WebCache = new AveWebCache(this.WebCache, webObj.WebID),
                            };
                            subWebs.Add(temp.Key, web);
                        }
                        else
                        {
                            log.Warn("Skip the App Web {0} , the url is {1}. ", webObj.Title, webObj.FullUrl);
                        }
                    }
                    return subWebs;
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetSubWebs.");
                    log.Log(AveLogLevel.WARN, "Exception detail:{0}", ex.ToString());
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetSubSitesError);
                }
            }
        }

        [Obsolete("Use Dictionary<Guid, IAveDiscoverList> IAveDiscoverWeb.GetLists() instead")]
        public Dictionary<Guid, AveDiscoverList> GetLists()
        {
            return GetLists(false);
        }

        [Obsolete("Use Dictionary<Guid, IAveDiscoverList> IAveDiscoverWeb.GetLists() instead")]
        public Dictionary<Guid, AveDiscoverList> GetLists(bool includeRecycleBin)
        {
            return GetLists(includeRecycleBin, true);
        }

        [Obsolete("Use Dictionary<Guid, IAveDiscoverList> IAveDiscoverWeb.GetLists() instead")]
        public Dictionary<Guid, AveDiscoverList> GetLists(bool includeRecycleBin,bool needSortByDependcy)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverWeb.GetLists"))
            {
                try
                {
                    Dictionary<Guid, AveDiscoverList> lists = new Dictionary<Guid, AveDiscoverList>();
                    if (HasFilter && ResultMode.HasMode(FilterResultMode.Trim) && !IsQualified())
                    {
                        return lists;
                    }
                    Dictionary<Guid, AveListObject> listObjs = this.WebCache.GetLists(includeRecycleBin);
                    if (needSortByDependcy)
                    {
                        var dependencyDic = GetDependencyDictionary(listObjs);
                        var sourceLists = DFSSortUtility<AveListObject>.Sort(listObjs.Values, (source, dependcy) =>
                                               {
                                                   if (dependencyDic.ContainsKey(source.ListId))
                                                   {
                                                       return dependencyDic[source.ListId].Contains(dependcy.ListId);
                                                   }
                                                   return false;
                                               });
                        listObjs = sourceLists.ToDictionary(list => list.ListId, list => list);
                    }
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
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetLists.");
                    log.Log(AveLogLevel.WARN, "Exception detail:{0}", ex.ToString());
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetListError);
                }
            }
        }

        private Dictionary<Guid, List<Guid>> GetDependencyDictionary(Dictionary<Guid, AveListObject> listObjs)
        {
            var dic = new Dictionary<Guid, List<Guid>>();
            try
            {
                foreach (var list in listObjs)
                {
                    if (!string.IsNullOrEmpty(list.Value.Fields))
                    {
                        var doc = new XmlDocument();
                        doc.LoadXml(list.Value.Fields);
                        var fields = doc.DocumentElement.SelectNodes("//Field[@Type='Lookup']//@List");
                        foreach (XmlAttribute attribute in fields)
                        {
                            List<Guid> lookuplists = null;
                            if (!dic.TryGetValue(list.Key, out lookuplists))
                            {
                                lookuplists = new List<Guid>();
                                dic.Add(list.Key, lookuplists);
                            }
                            lookuplists.Add(new Guid(attribute.Value));
                        }
                    }
                }
            }
            catch (Exception e)
            {
                log.Warn("An error occourred while getting dependency list ids. Error:{0}", e);
            }
            return dic;
        }

        /// <summary>
        /// Query web ContentTypes and fill the contentType relatived folder
        /// </summary>
        public Dictionary<byte[], AveContentTypeObject> GetContentTypes()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverWeb.GetContentTypes"))
            {
                try
                {
                    return WebCache.GetContentTypes();
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetContentTypes.");
                    log.Log(AveLogLevel.WARN, "Exception detail:{0}", ex.ToString());
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetCTsError);
                }
            }
        }

        #endregion

        #region IB

        [Obsolete("Use Dictionary<Guid, IAveDiscoverList> IAveDiscoverWeb.GetChangeLists() instead.")]
        public Dictionary<Guid, AveDiscoverList> GetChangeLists()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverWeb.GetChangeLists"))
            {
                try
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
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetChangeLists.");
                    log.Log(AveLogLevel.WARN, "Exception detail:{0}", ex.ToString());
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetListError);
                }
            }
        }

        public List<AveSecurityObject> GetChangeSecurityChanges()
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverWeb.GetChangeSecurityChanges"))
            {
                try
                {
                    var result = new List<AveSecurityObject>();
                    foreach (var list in WebCache.GetChangeSecuritys().Values)
                    {
                        result.AddRange(list);
                    }
                    return result;
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetChangeSecurityChanges.");
                    log.Log(AveLogLevel.WARN, "Exception detail:{0}", ex.ToString());
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetSecuritiesError);
                }
            }
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
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverWeb.GetFilterLists"))
            {
                if (HasFilter && ResultMode.HasMode(FilterResultMode.Trim))
                {
                    return lists.Values.Where(list =>
                        {
                            try
                            {
                                return this.FilterEngine.IsQualified(list.GetFilterObjectInfo(this.FilterPolicies));
                            }
                            catch (NotSupportedException)
                            {
                                throw;
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
        }

        #region FilterBase Members

        public override ObjectInfoBase GetFilterObjectInfo(List<FilterPolicy> policies)
        {
            if (!HasFilterOnThisLevel(policies, PolicyLevel.Site))
            {
                return new SiteInfo();
            }
            return FilterAnalyser.GetWebFilterInfo(policies, this.WebCache.AveWeb);
        }

        #endregion

        #endregion

        #region support for migration license
        public long GetWebSize(Guid siteId, Guid webId)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("AvePoint.Wrapper.Discovery.AveDiscoverWeb.GetWebSize"))
            {
                try
                {
                    return this.WebCache.GetWebSize(siteId, webId);
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.WARN, "An exception occurred while do GetWebSize.");
                    log.Log(AveLogLevel.WARN, "Exception detail:{0}", ex.ToString());
                    throw new AveWrapperDiscoverException(AveInternalResourceKey.Wrapper_Exception_Discovery_AWDGetSizeSizeError);
                }
            }
        }
        #endregion


        Dictionary<Guid, IAveDiscoverList> IAveDiscoverWeb.GetChangeLists()
        {
            return this.GetChangeLists().ToDictionary(kv => kv.Key, kv => kv.Value as IAveDiscoverList);
        }

        Dictionary<Guid, IAveDiscoverList> IAveDiscoverWeb.GetLists()
        {
            return this.GetLists().ToDictionary(kv => kv.Key, kv => kv.Value as IAveDiscoverList);
        }

        Dictionary<Guid, IAveDiscoverList> IAveDiscoverWeb.GetLists(bool includeRecycleBin)
        {
            return this.GetLists(includeRecycleBin).ToDictionary(kv => kv.Key, kv => kv.Value as IAveDiscoverList);
        }

        Dictionary<Guid, IAveDiscoverList> IAveDiscoverWeb.GetLists(bool includeRecycleBin, bool needSortByDependency)
        {
            return this.GetLists(includeRecycleBin,needSortByDependency).ToDictionary(kv => kv.Key, kv => kv.Value as IAveDiscoverList);
        }

        IAveDiscoverFolder IAveDiscoverWeb.GetRootFolder()
        {
            return this.GetRootFolder() as IAveDiscoverFolder;
        }

        Dictionary<Guid, IAveDiscoverWeb> IAveDiscoverWeb.GetSubWebs()
        {
            return this.GetSubWebs().ToDictionary(kv => kv.Key, kv => kv.Value as IAveDiscoverWeb);
        }

        Dictionary<Guid, IAveDiscoverWeb> IAveDiscoverWeb.GetSubWebs(bool includeRecycleBin)
        {
            return this.GetSubWebs(includeRecycleBin).ToDictionary(kv => kv.Key, kv => kv.Value as IAveDiscoverWeb);
        }

        public Dictionary<Guid, IAveDiscoverAppDefinition> GetAppDefinitions()
        {
            Dictionary<Guid, IAveDiscoverAppDefinition> lists = new Dictionary<Guid, IAveDiscoverAppDefinition>();
            IAveAppCatalog appCatalog = webObjectModelFactory.CreateAppCatalog();
            if (appCatalog != null)
            {
                IList<IAveAppInstance> appInstances = appCatalog.GetAppInstances(AveWeb);
                foreach (IAveAppInstance instance in appInstances)
                {
                    AveDiscoverAppDefinition definition = new AveDiscoverAppDefinition() { ProductId = instance.App.ProductId, Name = instance.Title, InstanceId = instance.Id, IsUpdateAvailable = instance.App.IsUpdateAvailable, VersionString = instance.App.VersionString };
                    try
                    {
                        definition.AppFullUrl = instance.AppWebFullUrl == null ? string.Empty : instance.AppWebFullUrl.ToString();
                    }
                    catch (Exception ex)
                    {
                        definition.AppFullUrl = string.Empty;
                        log.Warn(ex.ToString());
                    }
                    lists.Add(definition.ProductId, definition);
                }
            }
            return lists;
        }

        public Dictionary<Guid, IAveDiscoverWeb> GetAppWebs()
        {
            Dictionary<Guid, IAveDiscoverWeb> subWebs = new Dictionary<Guid, IAveDiscoverWeb>();
            Dictionary<Guid, AveWebObject> subWebObjs = this.WebCache.GetSubWebs(false);
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

    public class AveDiscoverAppDefinition : IAveDiscoverAppDefinition
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
