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
using System.Collections;
using AveClientRequest.Common;
using Microsoft365.Authentication;

namespace AvePoint.ObjectModel.Common
{
    class AveNavigation : AveClientObject, IAveNavigation
    {
        private AveWeb mWeb;
        private IAveRequest mRequest;
        private AveNavigationNode mQuickLauch;
        private AveNavigationNode mTopNavigationBar;
        private AveNavigationNode mSearchNav;
        private Dictionary<AveNodeTypes, string> mNavigationNodeTypesMapping;
        static AveLogger mLogger = AveLogger.GetInstance(typeof(AveNavigation));

        public AveNavigation(AveWeb web, IAveRequest request, Dictionary<string, object> prop)
        {
            mWeb = web;
            mRequest = request;
            base.DataCache.AddPropertyies(prop);
            InitNavigation();
        }

        internal void InitNavigation()
        {
            Dictionary<string, object> quickLauchProperties = base.DataCache.GetProperty<Dictionary<string, object>>("QuickLaunchParent" + AveObjectModelConstant.ObjectPropertySuffix);
            mQuickLauch = new AveNavigationNode(mWeb, null, null, mRequest, quickLauchProperties);
            Dictionary<string, object> topNavigationBarProperties = base.DataCache.GetProperty<Dictionary<string, object>>("TopNavigationBarParent" + AveObjectModelConstant.ObjectPropertySuffix);
            mTopNavigationBar = new AveNavigationNode(mWeb, null, null, mRequest, topNavigationBarProperties);
            Dictionary<string, object> searchNavProperties = base.DataCache.GetProperty<Dictionary<string, object>>("SearchNavParent" + AveObjectModelConstant.ObjectPropertySuffix);
            mSearchNav = new AveNavigationNode(mWeb, null, null, mRequest, searchNavProperties);
            InitNavigationNodeTypeMapping();
        }

        internal void InitNavigationNodeTypeMapping()
        {
            //need to be accurate in the feature
            mNavigationNodeTypesMapping = new Dictionary<AveNodeTypes, string>();
            mNavigationNodeTypesMapping[AveNodeTypes.Area] = "Area";
            mNavigationNodeTypesMapping[AveNodeTypes.Page] = "Page";
        }

        public IAveNavigationNode Home
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Home"))
                {
                    IAveNavigationNode homeNode = null;
                    foreach (IAveNavigationNode node in this.TopNavigationBar)
                    {
                        if (node.Title.Equals("Home"))
                        {
                            homeNode = node;
                            break;
                        }
                    }
                    base.DataCache.AddProperty("Home",homeNode);
                }
                return base.DataCache.GetProperty<IAveNavigationNode>("Home");
            }
        }

        public IAveNavigationNodeCollection QuickLaunch
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("QuickLaunch"))
                {
                    Dictionary<string, object> quickLaunchProperties = base.DataCache.GetProperty<Dictionary<string, object>>("QuickLaunch" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveNavigationNodeCollection aveNavigationNodCol = new AveNavigationNodeCollection(mWeb, mQuickLauch, mRequest, quickLaunchProperties, "quickLaunch");
                    base.DataCache.AddProperty("QuickLaunch",aveNavigationNodCol);
                    return aveNavigationNodCol;
                }
                return base.DataCache.GetProperty<IAveNavigationNodeCollection>("QuickLaunch");
            }
        }

        public IAveNavigationNodeCollection TopNavigationBar
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("TopNavigationBar"))
                {
                    Dictionary<string, object> topNavigationProperties = base.DataCache.GetProperty<Dictionary<string, object>>("TopNavigationBar" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveNavigationNodeCollection aveNavigationNodCol = new AveNavigationNodeCollection(mWeb, mTopNavigationBar, mRequest, topNavigationProperties, "topNavigationBar");
                    base.DataCache.AddProperty("TopNavigationBar",aveNavigationNodCol);
                    return aveNavigationNodCol;
                }
                return base.DataCache.GetProperty<IAveNavigationNodeCollection>("TopNavigationBar");
            }
        }

        public IAveNavigationNodeCollection SearchNav
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("SearchNav"))
                {
                    Dictionary<string, object> searchNav = base.DataCache.GetProperty<Dictionary<string, object>>("SearchNav" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveNavigationNodeCollection aveNavigationNodCol = new AveNavigationNodeCollection(mWeb, mSearchNav, mRequest, searchNav, "searchNav");
                    base.DataCache.AddProperty("SearchNav",aveNavigationNodCol);
                    return aveNavigationNodCol;
                }
                return base.DataCache.GetProperty<IAveNavigationNodeCollection>("SearchNav");
            }
        }

        public bool UseShared
        {
            get
            {
                return base.DataCache.GetProperty<bool>("UseShared");
            }
            set
            {
                mRequest.UpdateNavigationUseShared(this.mWeb.ServerRelativeUrl, value);
                base.DataCache.AddProperty("UseShared",value);
            }
        }

        public IAveNavigationNode GetNodeById(int id)
        {
            IAveNavigationNode node = null;
            if (Enum.IsDefined(typeof(AveQuickLaunchHeading), id))
            {
                node = this.GetHeadingNodeById(id);
            }
            else
            {
                node = this.GetHeadingNodeById(id);
                if (node == null)
                {
                    node = this.GetQuickLanuchSubNodeById(id);
                }
            }
            return node;
        }

        private IAveNavigationNode GetHeadingNodeById(int id)
        {
            foreach (IAveNavigationNode node in this.QuickLaunch)
            {
                if (node.ID == id)
                {
                    return node;
                }
            }
            foreach (IAveNavigationNode node in this.TopNavigationBar)
            {
                if (node.ID == id)
                {
                    return node;
                }
            }
            return null;
        }

        private IAveNavigationNode GetQuickLanuchSubNodeById(int id)
        {
            foreach (IAveNavigationNode quickLanuchNode in this.QuickLaunch)
            {
                foreach (IAveNavigationNode subNode in quickLanuchNode.Children)
                {
                    if (subNode.ID == id)
                    {
                        return subNode;
                    }
                }
            }
            return null;
        }

        #region IAveNavigation Members


        public AveNavigationInfoList GetNavigations()
        {
            AveNavigationInfoList nodeList = new AveNavigationInfoList();
            foreach (AveNavigationNode node in this.TopNavigationBar)
            {
                AveNavigationInfo navigationInfo = ConvertNavNodetoNodeInfo(node, AveNavigationScope.TopNavigationBar);
                nodeList.NavNodes.Add(navigationInfo);
                BuildNavNodesTree(navigationInfo, node.Children as AveNavigationNodeCollection, AveNavigationScope.TopNavigationBar);
            }
            foreach (AveNavigationNode node in this.QuickLaunch)
            {
                AveNavigationInfo navigationInfo = ConvertNavNodetoNodeInfo(node, AveNavigationScope.QuickLaunch);
                nodeList.NavNodes.Add(navigationInfo);
                BuildNavNodesTree(navigationInfo, node.Children as AveNavigationNodeCollection, AveNavigationScope.QuickLaunch);
            }
            return nodeList;
        }

        private AveNavigationInfo ConvertNavNodetoNodeInfo(AveNavigationNode node, AveNavigationScope scope)
        {
            AveNavigationInfo navNodeInfo = new AveNavigationInfo();
            navNodeInfo.Scope = scope;
            navNodeInfo.Title = node.Title;
            navNodeInfo.Url = node.Url;
            navNodeInfo.ParentTitle = node.Parent.Title;
            navNodeInfo.IsExternal = node.IsExternal;
            if (node.Properties != null && node.Properties.Contains("NodeType"))
            {
                navNodeInfo.NodeType = (int)(Enum.Parse(typeof(AveNodeTypes), node.Properties["NodeType"].ToString()));
            }
            else
            {
                navNodeInfo.NodeType = -1;
            }
            if (node.Properties != null && node.Properties.Contains("Target"))
            {
                navNodeInfo.Target = node.Properties["Target"].ToString();
            }
            return navNodeInfo;
        }

        private void BuildNavNodesTree(AveNavigationInfo parentNode, AveNavigationNodeCollection NodeCollection, AveNavigationScope scope)
        {
            foreach (AveNavigationNode node in NodeCollection)
            {
                try
                {
                    AveNavigationInfo nodeInfo = ConvertNavNodetoNodeInfo(node, scope);
                    parentNode.Children.Add(nodeInfo);
                    BuildNavNodesTree(nodeInfo, node.Children as AveNavigationNodeCollection, scope);
                }
                catch (Exception e)
                {
                    mLogger.Warn(AveObjectModel_CommonResource.BuildNavNodesTreeError, node.Title, this.mWeb.Url, e.ToString());
                    //mLog.Log(AveLogLevel.WARN, "WP10BKAveSPNa293", mAveSPWeb.SPWeb.Id, mAveSPWeb.SPWeb.Url, node.Title, node.Url, e);
                }
            }
        }
        #endregion

        public IAveNavigationNode AddToQuickLaunch(IAveNavigationNode node, AveQuickLaunchHeading heading)
        {
            //Dictionary<string, object> parentDic = new Dictionary<string, object>();
            Dictionary<string, object> newNodeDic = new Dictionary<string, object>();
            //AveObjectCopy.GetObjectBasicProperties(parentDic, node.Parent);
            AveObjectCopy.GetObjectBasicProperties(newNodeDic, node, new string[] { "Parent" });
            if (string.IsNullOrEmpty(node.Title) && string.IsNullOrEmpty(node.Url))
            {
                newNodeDic["AddQuickLaunchHeading"] = true;
                newNodeDic["IsNew"] = true;
                newNodeDic["QuickLaunchHeading"] = (int)heading;
            }
            Dictionary<string, object> returnInfo = this.mRequest.AddNavigationNode(mWeb.ServerRelativeUrl, null, newNodeDic, "quickLaunch");
            AveNavigationNode createdNode = new AveNavigationNode(node.Title, node.Url, node.IsExternal, returnInfo, this.mRequest, this.mWeb, mQuickLauch);
            return createdNode;
        }

        public bool RestoreNavigation(AveNavigationInfoList navigationList, NavigationRestoreSetting setting)
        {
            this.NavigationRestoreSetting = setting;
            string nodes = this.GetNodes(navigationList);
            string searchNodes = this.GetSearchNodes(navigationList);
            System.Collections.Hashtable webAllProp = mWeb.AllProperties.Clone() as System.Collections.Hashtable;
            if (this.UseShared)
            {
                webAllProp["UseShared"] = true;
            }
            bool compoundSupport = mRequest.RestoreNavigation(this.mWeb.ServerRelativeUrl, nodes, webAllProp, navigationList);
            mRequest.RestoreSearchNavigation(this.mWeb.ServerRelativeUrl, searchNodes, webAllProp);
            return compoundSupport;
        }

        private NavigationRestoreSetting NavigationRestoreSetting { get; set; }

        public bool NeedRestore(AveNavigationScope aveNavigationScope, AveNavigationInfoList value)
        {
            if (aveNavigationScope == AveNavigationScope.TopNavigationBar)
            {
                if (mWeb.IsRootWeb && value.SharedTopLink)
                {
                    return NavigationRestoreSetting.NavigationPromoteRestoreSettings == NavigationPromoteRestoreSetting.MoveTopLink || NavigationRestoreSetting.NavigationPromoteRestoreSettings == NavigationPromoteRestoreSetting.MoveBoth;
                }
                return !mWeb.Navigation.UseShared;
            }
            if (aveNavigationScope == AveNavigationScope.QuickLaunch)
            {
                if (mWeb.IsRootWeb && value.ShareQuickLunch)
                {
                    return NavigationRestoreSetting.NavigationPromoteRestoreSettings == NavigationPromoteRestoreSetting.MoveQuickLunch || NavigationRestoreSetting.NavigationPromoteRestoreSettings == NavigationPromoteRestoreSetting.MoveBoth;
                }
                bool quickLunchIsInherited = mWeb.AllProperties != null &&
                       mWeb.AllProperties["__InheritCurrentNavigation"] != null &&
                       string.Equals(mWeb.AllProperties["__InheritCurrentNavigation"].ToString(), "True", StringComparison.OrdinalIgnoreCase);

                return !quickLunchIsInherited;
            }
            return true;
        }

        private bool IsUrlAvailable(string nodeUrl, ITokenProvider tokenProvider)
        {
            string absoluteUrl = nodeUrl;
            if (!nodeUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                !nodeUrl.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                Uri baseUri = new Uri(this.mWeb.Site.Url);
                baseUri = new Uri(baseUri, nodeUrl);
                absoluteUrl = baseUri.AbsoluteUri;
            }
            return AveHttpWebRequestUtility.IsUrlAvailable(absoluteUrl, tokenProvider);
        }

        private string GetNodes(AveNavigationInfoList navigationList)
        {
            Dictionary<string, object> nodesDic = new Dictionary<string, object>();
            nodesDic["Root"] = "<GlobalNav>Global Navigation>>>Container>visible>>>>"
                            + "<CurrentNav>Current Navigation>>>Container>visible>>>>";
            Dictionary<string, object> globalNav = new Dictionary<string, object>();
            Dictionary<string, object> currentNav = new Dictionary<string, object>();
            nodesDic["<\"GlobalNav"] = globalNav;
            nodesDic["<\"CurrentNav"] = currentNav;
            int i = 0;

            var tokenProvider = (this.mWeb.Site as AveSite).Request.TokenProvider;

            foreach (AveNavigationInfo navInfo in navigationList.NavNodes.Where(navInfo => NeedRestore(navInfo.Scope, navigationList)))
            {
                if (navInfo.Scope == AveNavigationScope.SearchNav)
                {
                    continue;
                }
                if (i == 0 && navInfo.NodeType == -1 && 
                    navInfo.Scope != AveNavigationScope.QuickLaunch && 
                    (navInfo.Url.Equals(this.mWeb.Url) || navInfo.Url.Equals(this.mWeb.ServerRelativeUrl) || 
                     navInfo.Url.Equals(this.mWeb.ServerRelativeUrl.TrimEnd('/')+"/default.aspx")))
                {
                    i++;
                    continue;
                }
                i++;

                string id = string.Empty;
                string url = string.Empty;
                Hashtable propertyTable = new Hashtable();
                if (navInfo.HasMetaInfo)
                {
                    propertyTable = GetProperties(navInfo.MetaInfo);
                }
                if (propertyTable.ContainsKey("UrlQueryString") && propertyTable["UrlQueryString"].ToString().StartsWith("RootFolder=", StringComparison.OrdinalIgnoreCase))
                {
                    url = navInfo.Url + "?" + propertyTable["UrlQueryString"].ToString();
                    url = AveHtmlUtility.SimpleEncode(url);
                }
                else
                {
                    url = AveHtmlUtility.SimpleEncode(navInfo.Url);
                }

                if (Enum.IsDefined(typeof(AveQuickLaunchHeading), navInfo.Eid) && this.GetNodeById(navInfo.Eid) != null)
                {
                    id = new Guid("41cd9444-bcd7-46d8-a46a-6b6d6d034272").ToString() + "," + navInfo.Eid.ToString();
                    if (url.StartsWith(this.mWeb.Url, StringComparison.OrdinalIgnoreCase))
                    {
                        string webapp = this.mWeb.Url.Remove(this.mWeb.Url.IndexOf(this.mWeb.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase));
                        url = url.Remove(0, webapp.Length);
                    }
                }
                else if (navInfo.Eid > 2000)
                {
                    id = Guid.Empty.ToString() + "," + (navInfo.Eid - 2000).ToString();
                }
                else
                {
                    id = Guid.Empty.ToString() + "," + navInfo.Eid.ToString();
                }

                string name = AveHtmlUtility.SimpleEncode(navInfo.Title);
                //string url = AveHtmlUtility.SimpleEncode(navInfo.Url);
                string description = AveHtmlUtility.SimpleEncode(navInfo.Description);
                string nodeType = string.Empty;
                if (navInfo.NodeType == -1)
                {
                    nodeType = "Heading";
                }
                else
                {
                    if (mNavigationNodeTypesMapping.ContainsKey((AveNodeTypes)navInfo.NodeType))
                    {
                        nodeType = (navInfo.Scope == AveNavigationScope.TopNavigationBar ? "Global_" : "Current_") + mNavigationNodeTypesMapping[(AveNodeTypes)navInfo.NodeType];
                    }
                    else
                    {
                        nodeType = ((AveNodeTypes)navInfo.NodeType).ToString();
                    }
                }
                if (nodeType.Equals(AveNodeTypes.Area.ToString()))
                {
                    //AveWeb web = (this.mWeb.Site as AveSite).DataCache.GetWeakReferenceObject("OpenWeb" + url) as AveWeb;
                    IAveWeb web = this.mWeb.Site.OpenWeb(url);
                    if (web == null)
                    {
                        mLogger.Warn("Add global_area navigation node failed. due to the web is not exist.weburl is {0}", url);
                        continue;
                    }
                }

                if (!navInfo.IsExternal && !IsUrlAvailable(navInfo.Url, tokenProvider))
                {
                    continue;
                }

                string status = navInfo.IsVisible ? "visible" : "hidden";
                string target = navInfo.Target;
                string audience = navInfo.Audience;
                string created = string.Empty;
                string modified = string.Empty;

                string nodeProp = "<" + id + ">" + name + ">" + url + ">" + description + ">" + nodeType + ">" + status + ">" + target + ">" + audience + ">" + created + ">" + modified;
                if (navInfo.Scope.Equals(AveNavigationScope.TopNavigationBar))
                {
                    globalNav[id] = nodeProp;
                }
                else if (navInfo.Scope.Equals(AveNavigationScope.QuickLaunch))
                {
                    currentNav[id] = nodeProp;
                }

                if (navInfo.Children.Count > 0)
                {
                    Dictionary<string, object> children = new Dictionary<string, object>();
                    GetSubNavigationNodes(children, navInfo.Children, navigationList.SharedTopLink, navigationList.ShareQuickLunch);
                    nodesDic["<\"" + id] = children;
                }
            }
            string nodes = GetNodeString(nodesDic) + "<";
            return nodes;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="childrenNavigationNodes"></param>
        /// <param name="isSharedTopLink">cm需要</param>
        /// <param name="isShareQuickLunch">cm需要</param>
        private void GetSubNavigationNodes(Dictionary<string, object> parent, List<AveNavigationInfo> childrenNavigationNodes, bool isSharedTopLink, bool isShareQuickLunch)
        {
            int i = 0;
            var tokenProvider = (this.mWeb.Site as AveSite).Request.TokenProvider;
            foreach (AveNavigationInfo navInfo in childrenNavigationNodes)
            {
                if (navInfo.Scope == AveNavigationScope.TopNavigationBar && this.UseShared && !isSharedTopLink)
                {
                    continue;
                }
                if (navInfo.Scope == AveNavigationScope.QuickLaunch && mWeb.Properties != null && mWeb.Properties["__InheritCurrentNavigation"] == "True" && !isShareQuickLunch)
                {
                    continue;
                }
                if (i == 0 && navInfo.NodeType == -1 && navInfo.Url.Equals(this.mWeb.ServerRelativeUrl))
                {
                    i++;
                    continue;
                }
                i++;

                string id = string.Empty;
                string url = string.Empty;
                Hashtable propertyTable = new Hashtable();
                if (navInfo.HasMetaInfo)
                {
                    propertyTable = GetProperties(navInfo.MetaInfo);
                }
                if (propertyTable.ContainsKey("UrlQueryString") && propertyTable["UrlQueryString"].ToString().StartsWith("RootFolder=", StringComparison.OrdinalIgnoreCase))
                {
                    url = navInfo.Url + "?" + propertyTable["UrlQueryString"].ToString();
                    url = AveHtmlUtility.SimpleEncode(url);
                }
                else
                {
                    url = AveHtmlUtility.SimpleEncode(navInfo.Url);
                }

                if (Enum.IsDefined(typeof(AveQuickLaunchHeading), navInfo.Eid))
                {
                    id = new Guid("41cd9444-bcd7-46d8-a46a-6b6d6d034272").ToString() + "," + navInfo.Eid.ToString();
                    if (url.StartsWith(this.mWeb.Url, StringComparison.OrdinalIgnoreCase))
                    {
                        string webapp = this.mWeb.Url.Remove(this.mWeb.Url.IndexOf(this.mWeb.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase));
                        url = url.Remove(0, webapp.Length);
                    }
                }
                else if (navInfo.Eid > 2000)
                {
                    id = Guid.Empty.ToString() + "," + (navInfo.Eid - 2000).ToString();
                }
                else
                {
                    id = Guid.Empty.ToString() + "," + navInfo.Eid.ToString();
                }

                string name = AveHtmlUtility.SimpleEncode(navInfo.Title);
                //string url = AveHtmlUtility.SimpleEncode(navInfo.Url);
                string description = navInfo.Description;
                string nodeType = string.Empty;
                if (navInfo.NodeType == -1)
                {
                    nodeType = AveNodeTypes.AuthoredLinkPlain.ToString();
                }
                else
                {
                    nodeType = ((AveNodeTypes)navInfo.NodeType).ToString();
                }
                if (nodeType.Equals(AveNodeTypes.Area.ToString()))
                {
                    //AveWeb web = (this.mWeb.Site as AveSite).DataCache.GetWeakReferenceObject("OpenWeb" + url) as AveWeb;
                    IAveWeb web = this.mWeb.Site.OpenWeb(url);
                    if (web == null)
                    {
                        mLogger.Warn("Add global_area navigation node failed. due to the web is not exist.weburl is {0}", url);
                        continue;
                    }
                }

                if (!navInfo.IsExternal && !IsUrlAvailable(navInfo.Url, tokenProvider))
                {
                    continue;
                }

                string status = navInfo.IsVisible ? "visible" : "hidden";
                string target = navInfo.Target;
                string audience = navInfo.Audience;
                string created = string.Empty;
                string modified = string.Empty;

                string nodeProp = "<" + id + ">" + name + ">" + url + ">" + description + ">" + nodeType + ">" + status + ">" + target + ">" + audience + ">" + created + ">" + modified;
                parent[id] = nodeProp;
                if (navInfo.Children.Count > 0)
                {
                    Dictionary<string, object> children = new Dictionary<string, object>();
                    GetSubNavigationNodes(children, navInfo.Children, isSharedTopLink, isShareQuickLunch);
                    parent["<\"" + id] = children;
                }
            }
        }
        private string GetSearchNodes(AveNavigationInfoList navigationList)
        {
            string nodes = string.Empty;
            //var tokenProvider = (this.mWeb.Site as AveSite).Request.TokenProvider;
            foreach (AveNavigationInfo navInfo in navigationList.NavNodes.Where(navInfo => NeedRestore(navInfo.Scope, navigationList)))
            {
                if (navInfo.Scope != AveNavigationScope.SearchNav)
                {
                    continue;
                }
                string id = navInfo.Eid.ToString();
                string url = AveHtmlUtility.SimpleEncode(navInfo.Url);
                string name = AveHtmlUtility.SimpleEncode(navInfo.Title);
                string description = AveHtmlUtility.SimpleEncode(navInfo.Description);
                string nodeType = ((AveNodeTypes)navInfo.NodeType).ToString();
                string status = navInfo.IsVisible ? "visible" : "hidden";
                string target = navInfo.Target;
                string audience = navInfo.Audience;
                string created = string.Empty;
                string modified = string.Empty;

                string nodeProp = "<" + id + ">" + name + ">" + url + ">" + description + ">" + nodeType + ">" + status + ">" + target + ">" + audience + ">" + created + ">" + modified;
                nodes += nodeProp;
            }
            nodes = "Root" + nodes + "<"; ;
            return nodes;
        }

        private string GetNodeString(Dictionary<string, object> nodesDic)
        {
            string nodes = string.Empty;
            foreach (string key in nodesDic.Keys)
            {
                if (key.Equals("Root"))
                {
                    nodes = key + nodesDic[key].ToString();
                }
                Dictionary<string, object> node = nodesDic[key] as Dictionary<string, object>;
                if (node != null)
                {
                    nodes = nodes + key + GetSubNodesString(node);
                }
            }
            return nodes;
        }

        private string GetSubNodesString(Dictionary<string, object> subNodesDic)
        {
            string nodes = string.Empty;
            foreach (string key in subNodesDic.Keys)
            {
                Dictionary<string, object> node = subNodesDic[key] as Dictionary<string, object>;
                if (node != null)
                {
                    nodes = key + GetSubNodesString(node);
                }
                else
                {
                    nodes += subNodesDic[key].ToString();
                }
            }
            return nodes;
        }

        private static Hashtable GetProperties(string metainfo)
        {
            Hashtable prp = new Hashtable();
            string[] mSplitedString = metainfo.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string mStr in mSplitedString)
            {
                int index1 = mStr.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                int index2 = mStr.IndexOf("|", StringComparison.OrdinalIgnoreCase);
                if (index1 < 0 && index2 < 0)
                {
                    continue;
                }
                string key = index1 > 0 ? mStr.Substring(0, index1) : mStr.Substring(0, index2);
                string value = index2 > 0 ? mStr.Substring(index2 + 1) : String.Empty;
                prp.Add(key, value);
            }
            return prp;
        }
    }
}
