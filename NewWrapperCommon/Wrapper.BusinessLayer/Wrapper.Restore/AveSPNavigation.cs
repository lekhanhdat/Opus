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
    public class AveSPNavigation : IDisposable, AvePoint.Wrapper.Restore.IAveSPNavigation
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        protected readonly IReport reportor = new AveWrapperReport();

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

            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.WebNavigation"))
            {

                try
                {
                    //在PostAction中会走Mapping, 此处不需要
                    //MapNavTitle(navigationInfoList.NavNodes);
                    //WrapperRuntime.WrapperCache.NavigationCache.FindAndAddValue(mCurrentWebId, navigationInfoList);
                    if (!ParentSite.MappingManager.SiteMappingManager.NavNodesCache.ContainsKey(mCurrentWebId))
                    {
                        ParentSite.MappingManager.SiteMappingManager.NavNodesCache.Add(mCurrentWebId, navigationInfoList);
                    }
                }
                catch (Exception e)
                {
                    mLog.Log(AveLogLevel.WARN, string.Format("An error occurred while add navigation node.\n error message:{0}", e));
                    //mLog.Warn(e, "An error occurred while adding navigation node.");
                }

            }

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

            Restore(ParentSite.MappingManager.SiteMappingManager.NavNodesCache);            
        }

        public virtual void Restore(Dictionary<Guid, AveNavigationInfoList> Navigations)
        {
            foreach (var kv in Navigations)
            {
                using (IAveWeb web = ParentSite.SPSite.OpenWeb(kv.Key))
                {
                    if (!web.Exists)
                    {
                        //log, target web does not exist
                        continue;
                    }
                }
                ReplaceUrlAndTitle(kv.Value.NavNodes, kv.Key);
                var navigationSerializer = ParentSite.SPSite.RootWeb.NavigationSerializer;
                navigationSerializer.SetNavigationRestoreSetting(this.ParentSite.NavigationRestoreSetting, reportor);
                navigationSerializer.SetObjectData(kv);
            }
        }

        public IReport GetReport()
        {
            return reportor;
        }

        private void ReplaceUrlAndTitle(List<AveNavigationInfo> navigationNodes, Guid webId)
        {
            foreach (AveNavigationInfo navInfo in navigationNodes)
            {
                if (!string.IsNullOrEmpty(navInfo.ParentTitle))
                {
                    navInfo.ParentTitle = ParentSite.GetNameByLanguageMapping(navInfo.ParentTitle, AveLanguageMappingType.NavigationMapping);
                }
                if (!string.IsNullOrEmpty(navInfo.Title))
                {
                    navInfo.Title = GetTitleAfterMapping(webId, navInfo);
                }
                ReplaceOption replaceOption = new ReplaceOption(true, true);
                // navInfo.Url = AveReplaceProcessor.UrlReplace(navInfo.Url, ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings, replaceOption, ParentSite.SourceSiteInfo, ParentSite.ServerRelativeUrl);
                if (navInfo.Children.Count > 0)
                {
                    ReplaceUrlAndTitle(navInfo.Children, webId);
                }
            }
        }

        //ADO-135815: 先走ListTitleMapping, 如果在这个Mapping中找到, 就不走Language Mapping, 保证Navigation和List Title一致。
        private string GetTitleAfterMapping(Guid webId, AveNavigationInfo navInfo)
        {
            string titleAfterMapping;
            if(!ParentSite.MappingManager.SiteMappingManager.GetValueFromListTitleMappnig(webId, navInfo.Title, out titleAfterMapping))
            {
                titleAfterMapping = navInfo.Title;
            }
            if (!string.Equals(titleAfterMapping, navInfo.Title, StringComparison.Ordinal))
            {
                return titleAfterMapping;
            }
            return ParentSite.GetNameByLanguageMapping(navInfo.Title, AveLanguageMappingType.NavigationMapping);
        }

        public void Dispose()
        {
            //TODO
        }

        #region IAveSPNavigation Members


        IAveSPSite IAveSPNavigation.ParentSite
        {
            get { return ParentSite; }
        }

        #endregion
    }
}
