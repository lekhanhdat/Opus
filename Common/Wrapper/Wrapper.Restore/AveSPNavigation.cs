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
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Restore
{
    public class AveSPNavigation : IDisposable
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private readonly Guid mCurrentWebId;

        public AveSPSite ParentSite { get; private set; }

        public bool OverWrite { set; get; }

        public AveSPNavigation(AveSPSite site)
        {
            ParentSite = site;
        }

        public AveSPNavigation(AveSPSite site, NavigationRestoreSetting setting)
        {
            ParentSite = site;
        }

        public AveSPNavigation(AveSPWeb web)
        {
            web.WebNavigationRestore = true;
            mCurrentWebId = web.SPWeb.ID;
            ParentSite = web.ParentSite;
        }

        public void AddToNavNodesCache(AveNavigationInfoList navigationInfoList)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WebNavigation"))
            {
#endif
                try
                {
                    MapNavTitle(navigationInfoList.NavNodes);
                    WrapperRuntime.WrapperCache.NavigationCache.FindAndAddValue(mCurrentWebId, navigationInfoList);
                    ParentSite.MappingManager.SiteMappingManager.NavNodesCache.Add(mCurrentWebId, navigationInfoList);
                }
                catch (Exception e)
                {
                    mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while add navigation node.\n error message:{0}", e));
                    //mLog.Warn(e, "An error occurred while adding navigation node.");
                }
#if PerformanceLog
            }
#endif
        }

        private void MapNavTitle(List<AveNavigationInfo> navigationInfoList)
        {
            foreach (AveNavigationInfo navNodeInfo in navigationInfoList)
            {
                if (!string.IsNullOrEmpty(navNodeInfo.Title))
                {
                    navNodeInfo.Title = ParentSite.GetNameByLanguageMapping(navNodeInfo.Title, AveLanguageMappingType.NavigationMapping);
                }
                MapNavTitle(navNodeInfo.Children);
            }
        }

        //private IAveWeb tempWeb = null; //This tempWeb is only used in Restore function to avoid memory leak

        public virtual void Restore()
        {
            Restore(WrapperRuntime.WrapperCache.NavigationCache);
            WrapperRuntime.WrapperCache.NavigationCache.Clear();
        }

        public virtual void Restore(AveVolatileCache<Guid, AveNavigationInfoList> Navigations)
        {
            foreach (Guid key in Navigations.Keys)
            {
                using (IAveWeb web = ParentSite.SPSite.OpenWeb(key))
                {
                    if (!web.Exists)
                    {
                        //log, target web does not exist
                        continue;
                    }
                }                
                AveNavigationInfoList value;
                Navigations.TryGetValue(key, out value);
                ReplaceUrlAndTitle(value.NavNodes,GetGroupId());
                MapUniqueIdInNavigations(value.NavNodes);
                MapAppInstanceIdInNavigations(value.NavNodes);
                var pair = new KeyValuePair<Guid, AveNavigationInfoList>(key, value);
                var navigationSerializer = ParentSite.SPSite.RootWeb.NavigationSerializer;
                navigationSerializer.SetNavigationRestoreSetting(this.ParentSite.NavigationRestoreSetting);
                navigationSerializer.SetObjectData(pair);
            }
        }

        private void MapAppInstanceIdInNavigations(List<AveNavigationInfo> navigationNodes)
        {
            string searchText = "appredirect.aspx?instance_id={";       //length 30
            foreach (AveNavigationInfo navInfo in navigationNodes)
            {
                if (!string.IsNullOrEmpty(navInfo.Url) && navInfo.Url.Contains(searchText))
                {
                    int index = navInfo.Url.IndexOf(searchText);
                    string sourceInstanceId = navInfo.Url.Substring(index + 30, 36);        //36 is the length of GUID
                    string loweredSrcInstanceId = sourceInstanceId.ToLower();
                    if (WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AppInstanceidMapping.ContainsKey(loweredSrcInstanceId))
                    {
                        navInfo.Url = navInfo.Url.Replace(sourceInstanceId, WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager.AppInstanceidMapping[loweredSrcInstanceId]);
                    }
                }

                if (navInfo.Children.Count > 0)
                {
                    MapAppInstanceIdInNavigations(navInfo.Children);
                }
            }
        }
        private string GetGroupId()
        {
            object idObject = null;
            if (ParentSite.SPSite.RootWeb.WebTemplateName.Equals("GROUP#0", StringComparison.OrdinalIgnoreCase) &&
                ParentSite.SPSite.RootWeb.AllProperties.ContainsKey("GroupId"))
            {
                idObject = ParentSite.SPSite.RootWeb.AllProperties["GroupId"];
            }
            return idObject == null ? string.Empty : idObject.ToString();
        }
        private void ReplaceUrlAndTitle(List<AveNavigationInfo> navigationNodes,string groupId)
        {
            foreach (AveNavigationInfo navInfo in navigationNodes)
            {
                if (!string.IsNullOrEmpty(navInfo.ParentTitle))
                {
                    string oldTitle = navInfo.ParentTitle;
                    navInfo.ParentTitle = ParentSite.GetNameByLanguageMapping(navInfo.ParentTitle, AveLanguageMappingType.NavigationMapping);
                    mLog.Info("Navigation map:from {0} to {1}",oldTitle, navInfo.ParentTitle);
                }
                if (!string.IsNullOrEmpty(navInfo.Title))
                {
                    string oldTitle = navInfo.Title;
                    navInfo.Title = ParentSite.GetNameByLanguageMapping(navInfo.Title, AveLanguageMappingType.NavigationMapping);
                    mLog.Info("Navigation map:from {0} to {1}", oldTitle, navInfo.Title);
                }
                //Move into Object model.
                //ReplaceOption replaceOption = new ReplaceOption(true, true);
                //if (!string.IsNullOrEmpty(navInfo.Url))
                //{
                //    navInfo.Url = AveReplaceProcessor.UrlReplace(navInfo.Url, ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, replaceOption, ParentSite.SourceSiteInfo, ParentSite.ServerRelativeUrl);
                //    //这个逻辑目前只是Navigation用到。 如果其他地方也要用，请重构到UrlReplace里。
                //    navInfo.Url = AveUrlUtility.ReplaceGroupId(ParentSite.SourceSiteInfo, navInfo.Url, groupId);
                //}
                if (navInfo.Children.Count > 0)
                {
                    ReplaceUrlAndTitle(navInfo.Children, groupId);
                }
            }
        }

        private void MapUniqueIdInNavigations(List<AveNavigationInfo> navigationInfoList)
        {
            foreach (AveNavigationInfo navigationInfo in navigationInfoList)
            {
                //处理url,截取出url中uniqueId等属性. 
                if (!string.IsNullOrEmpty(navigationInfo.Url) && System.Text.RegularExpressions.Regex.IsMatch(navigationInfo.Url, @".*\?.*(sourcedoc\=.*)(\&.*\=.*)*"))
                {
                    Dictionary<string, string> urlParaDic = new Dictionary<string, string>();
                    string tempUrl = navigationInfo.Url.Substring(navigationInfo.Url.LastIndexOf("?") + 1);
                    string[] mc = System.Text.RegularExpressions.Regex.Split(tempUrl, "&");
                    foreach (string m in mc)
                    {
                        string[] pair = System.Text.RegularExpressions.Regex.Split(m, "=");
                        if (pair.Length == 2)
                        {
                            urlParaDic[pair[0]] = pair[1];
                        }
                    }
                    if (urlParaDic.ContainsKey("sourcedoc"))
                    {
                        if (AveSPUtility.IsGuid(urlParaDic["sourcedoc"]))
                        {
                            Guid navigationLinkGuid = new Guid(urlParaDic["sourcedoc"]);
                            if (ParentSite.MappingManager.SiteMappingManager.ItemGuidForReplicatorConflict.ContainsKey(navigationLinkGuid))
                            {
                                navigationInfo.Url = navigationInfo.Url.Replace(navigationLinkGuid.ToString(), ParentSite.MappingManager.SiteMappingManager.ItemGuidForReplicatorConflict[navigationLinkGuid].ToString());
                            }
                        }
                    }
                }                              
                if (navigationInfo.Children.Count > 0)
                {
                    MapUniqueIdInNavigations(navigationInfo.Children);
                }
            }
        }

        public void Dispose()
        {
            //TODO
        }
    }
}
