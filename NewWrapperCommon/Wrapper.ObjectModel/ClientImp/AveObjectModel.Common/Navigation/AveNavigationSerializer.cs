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
        private AveNavigationImport m_NavImportManager;
        private Dictionary<AveNodeTypes, string> mNavigationNodeTypesMapping;
        static AveLogger mLogger = AveLogger.GetInstance(typeof(AveNavigationSerializer));

        private bool backupFromInheritedWeb;

        public AveNavigationSerializer(AveWeb web)
        {
            m_Web = web;
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
            return GetObjectFromFirstUniqueTopNavigationWeb();
        }
        private AveNavigationInfoList GetObjectDataFromCurrentWeb()
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
        }
        private AveNavigationInfoList GetObjectFromFirstUniqueTopNavigationWeb()
        {
            AveNavigationInfoList nodeList = new AveNavigationInfoList();

            if (m_Web.Site.Features[ AveSP2010FeatureDefinitions.PublishingSite] != null)
            {
                nodeList.PublishFeatureAppearance = true;
            }
            IAveWeb firstUniqueTopNavigationWeb = m_Web.FirstUniqueTopLinkBarNavigationWeb;
            if (firstUniqueTopNavigationWeb != null && firstUniqueTopNavigationWeb.ID != m_Web.ID) 
            {
                nodeList.SharedTopLink = true;
            }
            int rank = 0;
            if (firstUniqueTopNavigationWeb.Navigation.TopNavigationBar != null)
            {
                foreach (AveNavigationNode node in firstUniqueTopNavigationWeb.Navigation.TopNavigationBar)
                {
                    AveNavigationInfo navigationInfo = ConvertNavNodetoNodeInfo(node, AveNavigationScope.TopNavigationBar, firstUniqueTopNavigationWeb.ServerRelativeUrl);
                    navigationInfo.RankChild = rank++;
                    nodeList.NavNodes.Add(navigationInfo);
                    BuildNavNodesTree(navigationInfo, node.Children as AveNavigationNodeCollection, AveNavigationScope.TopNavigationBar, firstUniqueTopNavigationWeb.ServerRelativeUrl);
                }
            }
            if (!firstUniqueTopNavigationWeb.ServerRelativeUrl.Equals(m_Web.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                firstUniqueTopNavigationWeb.Dispose();
            }
            IAveWeb firstUniqueQuickLauchNavigationWeb = m_Web.FirstUniqueQuickLaunchNavigationWeb;
            if (firstUniqueQuickLauchNavigationWeb != null && firstUniqueQuickLauchNavigationWeb.ID != m_Web.ID) 
            {
                nodeList.ShareQuickLaunch = true;
            }
            if (firstUniqueQuickLauchNavigationWeb.Navigation.QuickLaunch != null)
            {
                //foreach (AveNavigationNode node in m_Web.Navigation.QuickLaunch) previous code
                foreach (AveNavigationNode node in firstUniqueQuickLauchNavigationWeb.Navigation.QuickLaunch)
                {
                    //AveNavigationInfo navigationInfo = ConvertNavNodetoNodeInfo(node, AveNavigationScope.QuickLaunch);previous code
                    AveNavigationInfo navigationInfo = ConvertNavNodetoNodeInfo(node, AveNavigationScope.QuickLaunch, firstUniqueQuickLauchNavigationWeb.ServerRelativeUrl);
                    navigationInfo.RankChild = rank++;
                    nodeList.NavNodes.Add(navigationInfo);
                    //BuildNavNodesTree(navigationInfo, node.Children as AveNavigationNodeCollection, AveNavigationScope.QuickLaunch); previous code
                    BuildNavNodesTree(navigationInfo, node.Children as AveNavigationNodeCollection, AveNavigationScope.QuickLaunch, firstUniqueQuickLauchNavigationWeb.ServerRelativeUrl);
                }
            }
            if (!firstUniqueQuickLauchNavigationWeb.ServerRelativeUrl.Equals(m_Web.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                firstUniqueQuickLauchNavigationWeb.Dispose();
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
            if (node.Properties != null)
            {
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
                navNodeInfo.MetaInfo = ConstructNavigationMetaInfo(node.Properties);
                navNodeInfo.HasMetaInfo = !string.IsNullOrEmpty(navNodeInfo.MetaInfo);
            }
            return navNodeInfo;
        }

        private string ConstructNavigationMetaInfo(System.Collections.Hashtable navigationProperties)
        {
            StringBuilder metaInfo = new StringBuilder();
            string syncSuffix = ":|";
            if (navigationProperties == null || navigationProperties.Count <= 0)
            {
                return string.Empty;
            }
            foreach (string key in navigationProperties.Keys)
            {
                if (string.IsNullOrEmpty(key))
                {
                    continue;
                }
                metaInfo.Append(key);
                metaInfo.Append(syncSuffix);
                metaInfo.Append(navigationProperties[key]);
                metaInfo.Append("\r\n");
            }
            return metaInfo.ToString();
        }

        private string ParseUrlWhileGetFromInhertWeb(string url, string FirstUniqueTopNavigationWebServerRelativeUrl,bool isExtenal)
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
            if (m_Web.IsRootWeb || isExtenal || string.IsNullOrEmpty(FirstUniqueTopNavigationWebServerRelativeUrl) || m_Web.ServerRelativeUrl.Equals(url, StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }
            if (url.StartsWith(FirstUniqueTopNavigationWebServerRelativeUrl, StringComparison.OrdinalIgnoreCase))
            {
                return (this.m_Web.ServerRelativeUrl.TrimEnd('/') + "/" + url.Substring(FirstUniqueTopNavigationWebServerRelativeUrl.Length).Trim('/')).TrimEnd('/');
            }
            if (url.StartsWith(this.m_Web.Site.ServerRelativeUrl,StringComparison.OrdinalIgnoreCase)) 
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
                }
            }
        }

        public object SetObjectData(KeyValuePair<Guid, AveNavigationInfoList> navigationInfoList)
        {
            m_NavImportManager.Run(navigationInfoList);
            return null;
        }

        #region IAveNavigationSerializer Members

        public void SetNavigationRestoreSetting(NavigationRestoreSetting setting, IReport reportor)
        {
            this.m_NavImportManager.NavigationRestoreSetting = setting;
            this.m_NavImportManager.mReportor = reportor;
        }

        public string SourceWebApplicationUrl { get; set; }

        public bool NeedBackupFullUrl { set; get; }

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
