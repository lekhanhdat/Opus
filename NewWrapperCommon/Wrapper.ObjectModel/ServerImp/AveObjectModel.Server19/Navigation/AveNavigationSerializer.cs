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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Navigation;
using Microsoft.SharePoint.Publishing;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.ObjectModel.Server19
{
    class AveNavigationSerializer : IAveNavigationSerializer
    {
        readonly AveLogger logger = AveLogger.GetInstance(typeof(AveNavigationSerializer));
        

        readonly AveWeb aveWeb;
        readonly AveNavigationImport importManager;

        public bool NeedBackupFullUrl { set; get; }

        public bool BackupFromInheritedWeb { get; set; }

        public void SetNavigationRestoreSetting(NavigationRestoreSetting setting, IReport reportor)
        {
            importManager.NavigationRestoreSetting = setting;
            importManager.mReportor = reportor;
        }

        public string SourceWebApplicationUrl { get; set; }

        private SPWeb Web
        {
            get { return aveWeb.Web; }
        }

        private AvePublishingSite publishSiteChecker;
        private AvePublishingSite PublishSiteChecker
        {
            get { return publishSiteChecker == null ? new AvePublishingSite() : publishSiteChecker; }
        }

        private AvePublishingWeb publishWebChecker;
        private AvePublishingWeb PublishWebChecker
        {
            get { return publishWebChecker == null ? new AvePublishingWeb() : publishWebChecker; }
        }

        public AveNavigationSerializer(AveWeb web)
        {
            aveWeb = web;
            importManager = new AveNavigationImport(web);

        }

        public AveNavigationSerializer(AveWeb web, object importSettings)
        {
            aveWeb = web;
            importManager = new AveNavigationImport(web);
        }

        public AveNavigationInfoList GetObjectData()
        {
            var objectDictionary = BackupFromInheritedWeb ? GetObjectFromFirstInheritedWeb() : GetObjectFromCurrentWeb();

#if DebugNavigation
            string debugFileLocation = Path.Combine(AveEnv.AgentJobFolder, aveWeb.Title + aveWeb.ID.ToString());
            using (XmlTextWriter textWriter = new XmlTextWriter(debugFileLocation, Encoding.UTF8))
            {
                AveXmlSerializer.Serialize(textWriter, aveWeb.Title, objectDictionary);
            }
#endif


            return objectDictionary;
        }

        internal AveNavigationInfoList GetObjectFromFirstInheritedWeb()
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationSerializer.GetObjectFromFirstInheritedWeb"))
            {

                logger.Info(string.Format("Get Navigation From Inherited while backup web: {0}", Web.Url));
                var nodeList = new AveNavigationInfoList { BackupFromInheritedWeb = true };
                if (AveEnv.IsPublishing && PublishSiteChecker.IsPublishingSite(aveWeb.Site))
                {
                    nodeList.PublishFeatureAppearance = true;
                }
                GetTopLinkFromInheritWeb(nodeList);
                GetQuickLaunchFromInheritWeb(nodeList);
                GetSearchFromWeb(nodeList);
                return nodeList;

            }

        }

        private void GetSearchFromWeb(AveNavigationInfoList nodeList)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationSerializer.GetSearchFromWeb"))
            {
                logger.Info(string.Format("Get Search Navigation from  url: {0}", Web.Url));
                BuildNavNodesTree(nodeList.NavNodes, Web.Navigation.SearchNav, AveNavigationScope.SearchNavigation, true);
            }
        }

        private void GetTopLinkFromInheritWeb(AveNavigationInfoList nodeList)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationSerializer.GetTopLinkFromInheritWeb"))
            {

                #region GetTopLink
                SPWeb tmpTopLinkWeb = Web;
                bool isCurrentTopLinkWeb = true;
                nodeList.SharedTopLink = tmpTopLinkWeb.Navigation.UseShared;

                while (tmpTopLinkWeb.Navigation.UseShared)
                {
                    var parentWeb = tmpTopLinkWeb.ParentWeb;
                    if (!isCurrentTopLinkWeb)
                    {
                        tmpTopLinkWeb.Dispose();
                    }
                    tmpTopLinkWeb = parentWeb;
                    isCurrentTopLinkWeb = false;
                }
                logger.Info("Get Top Link from url:" + tmpTopLinkWeb.Url);

                BuildNavNodesTree(nodeList.NavNodes, tmpTopLinkWeb.Navigation.TopNavigationBar, AveNavigationScope.TopNavigationBar, true);
                if (!isCurrentTopLinkWeb)
                {
                    tmpTopLinkWeb.Dispose();
                }
                #endregion

            }

        }

        private void GetQuickLaunchFromWeb(AveNavigationInfoList nodeList)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationSerializer.GetQuickLaunchFromWeb"))
            {

                logger.Info(string.Format("Get Quick Launch from url: {0}", Web.Url));
                nodeList.ShareQuickLaunch = false;
                BuildNavNodesTree(nodeList.NavNodes, Web.Navigation.QuickLaunch, AveNavigationScope.QuickLaunch, false);

            }

        }

        private void GetQuickLaunchFromInheritWeb(AveNavigationInfoList nodeList)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationSerializer.GetQuickLaunchFromInheritWeb"))
            {

                const string INHERIT_NAVIGATION_PROPERTYNAME = "__InheritCurrentNavigation";
                if (AveEnv.IsPublishing && PublishSiteChecker.IsPublishingSite(aveWeb.Site))
                {
                    SPWeb tmpQuichLaunchWeb = Web;
                    bool isCurrentQuickLaunchWeb = true;
                    while (tmpQuichLaunchWeb.AllProperties[INHERIT_NAVIGATION_PROPERTYNAME] != null &&
                        string.Equals(tmpQuichLaunchWeb.AllProperties[INHERIT_NAVIGATION_PROPERTYNAME].ToString(), bool.TrueString, StringComparison.OrdinalIgnoreCase))
                    {
                        var parentWeb = tmpQuichLaunchWeb.ParentWeb;
                        if (!isCurrentQuickLaunchWeb)
                        {
                            tmpQuichLaunchWeb.Dispose();
                        }
                        tmpQuichLaunchWeb = parentWeb;
                        isCurrentQuickLaunchWeb = false;
                    }
                    logger.Info(string.Format("Get Quick Launch from url: {0}", tmpQuichLaunchWeb.Url));

                    nodeList.ShareQuickLaunch = tmpQuichLaunchWeb.ID != this.Web.ID;

                    BuildNavNodesTree(nodeList.NavNodes, tmpQuichLaunchWeb.Navigation.QuickLaunch, AveNavigationScope.QuickLaunch, true);
                    if (!isCurrentQuickLaunchWeb)
                    {
                        tmpQuichLaunchWeb.Dispose();
                    }
                }
                else
                {
                    logger.Info("Get Quick Launch from url:" + Web.Url);
                    BuildNavNodesTree(nodeList.NavNodes, Web.Navigation.QuickLaunch, AveNavigationScope.QuickLaunch, false);
                }

            }

        }

        internal AveNavigationInfoList GetObjectFromCurrentWeb()
        {
            var nodeList = new AveNavigationInfoList();
            BuildNavNodesTree(nodeList.NavNodes, Web.Navigation.TopNavigationBar, AveNavigationScope.TopNavigationBar, false);
            BuildNavNodesTree(nodeList.NavNodes, Web.Navigation.QuickLaunch, AveNavigationScope.QuickLaunch, false);
            GetSearchFromWeb(nodeList);
            return nodeList;
        }

        private void BuildNavNodesTree(List<AveNavigationInfo> siblingsNode, IEnumerable nodeCollection, AveNavigationScope scope, bool fromInheritedWeb)
        {
            if (nodeCollection == null)
            {
                return;
            }
            int rank = 0;
            foreach (SPNavigationNode node in nodeCollection)
            {
                AveNavigationInfo nodeInfo = ConvertNavNodetoNodeInfo(node, scope, fromInheritedWeb);
                nodeInfo.RankChild = rank++;
                siblingsNode.Add(nodeInfo);
                BuildNavNodesTree(nodeInfo.Children, node.Children, scope, fromInheritedWeb);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "")]
        private AveNavigationInfo ConvertNavNodetoNodeInfo(SPNavigationNode node, AveNavigationScope scope, bool getFromInheritedWeb)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveNavigationSerializer.ConvertNavNodetoNodeInfo"))
            {

                CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
                if (Web != null)
                {
                    culture = Web.UICulture;
                }
                var navNodeInfo = new AveNavigationInfo { Scope = scope, Title = node.TitleResource.GetValueForUICulture(culture), ParentTitle = node.Parent.TitleResource.GetValueForUICulture(culture), IsExternal = node.IsExternal, Eid = node.Id };
                navNodeInfo.Url = ParseUrlWhileGetFromInhertWeb(node, node.Url);
                if (!getFromInheritedWeb && node.IsExternal && node.Url.Equals(node.Navigation.Web.ServerRelativeUrl))
                {
                    navNodeInfo.Url = string.Empty;
                }

                if (getFromInheritedWeb)
                {
                    navNodeInfo.MetaInfo = GetNavNodeMetainfo(new AveWeb(aveWeb.Site as AveSite, node.Navigation.Web), navNodeInfo.Eid);
                }
                else
                {
                    navNodeInfo.MetaInfo = GetNavNodeMetainfo(aveWeb, navNodeInfo.Eid);
                }
                if (navNodeInfo.MetaInfo != null)
                {
                    navNodeInfo.HasMetaInfo = true;
                }
                if ((AveEnv.IsMoss) && node.Properties.Contains("NodeType"))
                {
                    navNodeInfo.NodeType = (int)(Enum.Parse(typeof(AveNodeTypes), node.Properties["NodeType"].ToString()));
                }
                else
                {
                    navNodeInfo.NodeType = -1;
                }
                if (node.Properties.Contains("Target"))
                {
                    navNodeInfo.Target = node.Properties["Target"].ToString();
                }
                if (node.Properties.Contains("Description"))
                {
                    navNodeInfo.Description = node.Properties["Description"].ToString();
                }
                if (node.Properties.Contains("Audience"))
                {
                    navNodeInfo.Audience = node.Properties["Audience"].ToString();
                }
                navNodeInfo.LastModifiedDate = node.LastModified;
                navNodeInfo.TitleResource = new AveUserResource(node.TitleResource).GetUserResourceInfo(this.aveWeb);
                return navNodeInfo;

            }

        }

        private string ParseUrlWhileGetFromInhertWeb(SPNavigationNode navigationNode, string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }
            if (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                || url.StartsWith("ftp://", StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }
            if (string.Equals(navigationNode.Navigation.Web.ServerRelativeUrl, url, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(aveWeb.ServerRelativeUrl, url, StringComparison.OrdinalIgnoreCase))
            {//指向top site本身的Url                
                if (Equals(navigationNode.Properties["BlankUrl"], "True"))
                {
                    return string.Empty;
                }
            }

            string result = string.Empty;
            if (NeedBackupFullUrl)
            {
                if (url.StartsWith(navigationNode.Navigation.Web.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase) &&
                    !url.StartsWith(aveWeb.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
                {
                    result = this.aveWeb.Url + "/" + url.Substring(navigationNode.Navigation.Web.ServerRelativeUrl.Length).Trim('/');
                }
                else
                {
                    result = navigationNode.Navigation.Web.Site.MakeFullUrl(url);
                }
            }
            else
            {
                result = url;
            }

            if (!string.IsNullOrEmpty(SourceWebApplicationUrl))
            {
                return AveUrlUtility.ReplaceWebApplicationForPRItem(result, SourceWebApplicationUrl);
            }
            return result;
        }



        [WrapperOptimization(true)]
        private string GetNavNodeMetainfo(AveWeb web, int Eid)
        {
            APIProxy.Current = () => GetNavNodeMetainfoProxy(Eid);
            return AveProxyProvider.GetProxy().GetNavigationNodeMetainfo(web, Eid);
        }
        private string GetNavNodeMetainfoProxy(int Eid)
        {
            //need to get metainfo by API later on.
            return string.Empty;
        }

        public object SetObjectData(KeyValuePair<Guid, AveNavigationInfoList> navigationInfoList)
        {
            importManager.Run(navigationInfoList);

            return null;
        }

    }
}
