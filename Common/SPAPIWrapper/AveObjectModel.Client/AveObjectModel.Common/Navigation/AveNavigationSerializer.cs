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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource.Client;
namespace AvePoint.ObjectModel.Common
{
    internal class AveNavigationSerializer : IAveNavigationSerializer
    {
        private AveWeb m_Web;
        private IAveRequest m_Request;
        private AveNavigationImport m_NavImportManager;
        private Dictionary<AveNodeTypes, string> mNavigationNodeTypesMapping;
        static AveLogger mLogger = AveLogger.GetInstance(typeof(AveNavigationSerializer));

        private bool backupFromInheritedWeb;

        public AveNavigationSerializer(AveWeb web, IAveRequest request)
        {
            m_Web = web;
            m_Request = request;
            m_NavImportManager = new AveNavigationImport(web);
            InitNavigationNodeTypeMapping();
        }

        internal void InitNavigationNodeTypeMapping()
        {
            //need to be accurate in the feature
            mNavigationNodeTypesMapping = new Dictionary<AveNodeTypes, string>();
            mNavigationNodeTypesMapping[AveNodeTypes.Area] = "Area";
            mNavigationNodeTypesMapping[AveNodeTypes.Page] = "Page";
        }

        public AveNavigationInfoList GetObjectData()
        {
            if (m_Web.Site.CompatibilityLevel == 15)
            {
                return GetSP2013Navigation();
            }
            else
            {
                return GetSP2010Navigation();
            }
        }
        /*private AveNavigationInfoList GetObjectDataFromCurrentWeb()
        {
            AveNavigationInfoList nodeList = new AveNavigationInfoList();
            int rankChild = 0;
            foreach (AveNavigationNode node in m_Web.Navigation.TopNavigationBar)
            {
                AveNavigationInfo navigationInfo = ConvertNavNodetoNodeInfo(node, AveNavigationScope.TopNavigationBar);
                navigationInfo.RankChild = rankChild++;
                nodeList.NavNodes.Add(navigationInfo);
                BuildNavNodesTree(navigationInfo, node.Children as AveNavigationNodeCollection, AveNavigationScope.TopNavigationBar);
            }
            rankChild = 0;
            foreach (AveNavigationNode node in m_Web.Navigation.QuickLaunch)
            {
                AveNavigationInfo navigationInfo = ConvertNavNodetoNodeInfo(node, AveNavigationScope.QuickLaunch);
                navigationInfo.RankChild = rankChild++;
                nodeList.NavNodes.Add(navigationInfo);
                BuildNavNodesTree(navigationInfo, node.Children as AveNavigationNodeCollection, AveNavigationScope.QuickLaunch);
            }
            return nodeList;
        }*/
        private AveNavigationInfoList GetObjectFromFirstUniqueTopNavigationWeb(AveNavigationInfoList nodeList)
        {            
            Guid publishingFeatureId = new Guid("f6924d36-2fa8-4f0b-b16d-06b7250180fa");

            if (m_Web.Site.Features[publishingFeatureId] != null)
            {
                nodeList.PublishFeatureAppearance = true;
            }
            IAveWeb firstUniqueTopNavigationWeb = m_Web.FirstUniqueTopLinkBarNavigationWeb;
            if (firstUniqueTopNavigationWeb != null && firstUniqueTopNavigationWeb.ID != m_Web.ID) 
            {
                nodeList.SharedTopLink = true;
            }
            int rankChild = 0;
            if (firstUniqueTopNavigationWeb?.Navigation.TopNavigationBar != null)
            {
                foreach (AveNavigationNode node in firstUniqueTopNavigationWeb.Navigation.TopNavigationBar)
                {
                    AveNavigationInfo navigationInfo = ConvertNavNodetoNodeInfo(node, AveNavigationScope.TopNavigationBar, firstUniqueTopNavigationWeb.ServerRelativeUrl);
                    navigationInfo.RankChild = rankChild++;
                    nodeList.NavNodes.Add(navigationInfo);
                    BuildNavNodesTree(navigationInfo, node.Children as AveNavigationNodeCollection, AveNavigationScope.TopNavigationBar, firstUniqueTopNavigationWeb.ServerRelativeUrl);
                }
            }
            if (firstUniqueTopNavigationWeb != null && !firstUniqueTopNavigationWeb.ServerRelativeUrl.Equals(m_Web.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                firstUniqueTopNavigationWeb.Dispose();
            }
            IAveWeb firstUniqueQuickLauchNavigationWeb = m_Web.FirstUniqueQuickLaunchNavigationWeb;
            if (firstUniqueQuickLauchNavigationWeb != null && firstUniqueQuickLauchNavigationWeb.ID != m_Web.ID) 
            {
                nodeList.ShareQuickLunch = true;
            }
            //foreach (AveNavigationNode node in m_Web.Navigation.QuickLaunch) previous code
            if (firstUniqueQuickLauchNavigationWeb?.Navigation.QuickLaunch != null)
            {
                foreach (AveNavigationNode node in firstUniqueQuickLauchNavigationWeb.Navigation.QuickLaunch)
                {
                    //AveNavigationInfo navigationInfo = ConvertNavNodetoNodeInfo(node, AveNavigationScope.QuickLaunch);previous code
                    AveNavigationInfo navigationInfo = ConvertNavNodetoNodeInfo(node, AveNavigationScope.QuickLaunch, firstUniqueQuickLauchNavigationWeb.ServerRelativeUrl);
                    navigationInfo.RankChild = rankChild++;
                    nodeList.NavNodes.Add(navigationInfo);
                    //BuildNavNodesTree(navigationInfo, node.Children as AveNavigationNodeCollection, AveNavigationScope.QuickLaunch); previous code
                    BuildNavNodesTree(navigationInfo, node.Children as AveNavigationNodeCollection, AveNavigationScope.QuickLaunch, firstUniqueQuickLauchNavigationWeb.ServerRelativeUrl);
                }
            }
            if (firstUniqueQuickLauchNavigationWeb != null && !firstUniqueQuickLauchNavigationWeb.ServerRelativeUrl.Equals(m_Web.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                firstUniqueQuickLauchNavigationWeb.Dispose();
            }
            foreach (AveNavigationNode node in m_Web.Navigation.SearchNav)
            {
                AveNavigationInfo navigationInfo = ConvertNavNodetoNodeInfo(node, AveNavigationScope.SearchNav);
                navigationInfo.RankChild = rankChild++;
                nodeList.NavNodes.Add(navigationInfo);
                BuildNavNodesTree(navigationInfo, node.Children as AveNavigationNodeCollection, AveNavigationScope.SearchNav);
            }
            return nodeList;
        }
        private AveNavigationInfo ConvertNavNodetoNodeInfo(AveNavigationNode node, AveNavigationScope scope, string UseFirstUniqueTopNavigationWebServerRelativeUrl = null)
        {
            AveNavigationInfo navNodeInfo = new AveNavigationInfo();
            navNodeInfo.Eid = node.ID;
            navNodeInfo.Scope = scope;
            navNodeInfo.Title = node.Title;
            navNodeInfo.Url = ParseUrlWhileGetFromInhertWeb(node.Url, UseFirstUniqueTopNavigationWebServerRelativeUrl,node.IsExternal);

            navNodeInfo.ParentTitle = node.Parent.Title;
            navNodeInfo.IsExternal = node.IsExternal;
            navNodeInfo.IsVisible = node.IsVisible;
            if (node.Properties != null && node.Properties.Contains("NodeType"))
            {
                string nodeType = node.Properties["NodeType"].ToString();
                if (nodeType.Contains("_"))
                {
                    nodeType = nodeType.Substring(nodeType.IndexOf('_') + 1);
                }
                if (mNavigationNodeTypesMapping.ContainsValue(nodeType))
                {
                    navNodeInfo.NodeType = (int)mNavigationNodeTypesMapping.First(tempDic => tempDic.Value == nodeType).Key;
                }
                else
                {
                    navNodeInfo.NodeType = (int)(Enum.Parse(typeof(AveNodeTypes), nodeType));
                }
            }
            else
            {
                navNodeInfo.NodeType = -1;
            }
            if (node.Properties != null && node.Properties.Contains("Target"))
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
            return navNodeInfo;
        }

        private AveNavigationInfoList GetSP2013Navigation()
        {                                    
            IAveRequest request = m_Request as IAveRequest;
            Ave2013NavigationInfo navigationInfo = request.Get2013Navigation(m_Web.ServerRelativeUrl, true);
            return GetObjectFromFirstUniqueTopNavigationWeb(navigationInfo);
        }

        private AveNavigationInfoList GetSP2010Navigation()
        {
            AveNavigationInfoList navigationInfo = new AveNavigationInfoList();
            return GetObjectFromFirstUniqueTopNavigationWeb(navigationInfo);
        }

        private string ParseUrlWhileGetFromInhertWeb(string url, string FirstUniqueTopNavigationWebServerRelativeUrl,bool isExtenal)
        {
            if (string.IsNullOrEmpty(url))
            {
                return string.Empty;
            }
            if (m_Web.IsRootWeb || isExtenal || string.IsNullOrEmpty(FirstUniqueTopNavigationWebServerRelativeUrl) || m_Web.ServerRelativeUrl.Equals(url, StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }
            if (url.StartsWith(FirstUniqueTopNavigationWebServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                return (this.m_Web.ServerRelativeUrl + "/" + url.Substring(FirstUniqueTopNavigationWebServerRelativeUrl.Length).Trim('/')).TrimEnd('/');
            }
            if (url.StartsWith(this.m_Web.Site.ServerRelativeUrl)) 
            {
                return url;
            }
            if (url.StartsWith(Uri.UriSchemeHttp) || url.StartsWith(Uri.UriSchemeHttps))
            {
                return url;
            }
            return AveUrlUtility.CombineUrl(this.m_Web.Site.Url, url).TrimEnd('/');
        }

        private void BuildNavNodesTree(AveNavigationInfo parentNode, AveNavigationNodeCollection NodeCollection, AveNavigationScope scope, string FirstUniqueTopNavigationWebServerRelativeUrl = null)
        {
            int rank = 0;
            foreach (AveNavigationNode node in NodeCollection)
            {
                try
                {
                    AveNavigationInfo nodeInfo = ConvertNavNodetoNodeInfo(node, scope, FirstUniqueTopNavigationWebServerRelativeUrl);
                    nodeInfo.RankChild = rank++;
                    parentNode.Children.Add(nodeInfo);
                    BuildNavNodesTree(nodeInfo, node.Children as AveNavigationNodeCollection, scope, FirstUniqueTopNavigationWebServerRelativeUrl);
                }
                catch (Exception e)
                {
                    mLogger.Debug(AveObjectModel_CommonResource.BuildNavNodesTreeErrorSerializer, node.Title, this.m_Web.Url, e.ToString());
                    //mLog.Log(AveLogLevel.WARN, "WP10BKAveSPNa293", mAveSPWeb.SPWeb.Id, mAveSPWeb.SPWeb.Url, node.Title, node.Url, e);
                }
            }
        }

        public object SetObjectData(KeyValuePair<Guid, AveNavigationInfoList> navigationInfoList)
        {
            m_NavImportManager.Run(navigationInfoList);
            return null;
        }

        #region IAveNavigationSerializer Members

        public void SetNavigationRestoreSetting(NavigationRestoreSetting setting)
        {
            this.m_NavImportManager.NavigationRestoreSetting = setting;
        }

        public string SourceWebApplicationUrl { get; set; }

        public bool BackupFromInheritedWeb
        {
            get
            {
                return this.backupFromInheritedWeb;
            }
            set
            {
                backupFromInheritedWeb = value;
            }
        }

        #endregion
    }
}
