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
using System.Reflection;
using System.Text;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Utility;

namespace AvePoint.Item.Restore
{
    public class RestoreTreeNode
    {
        public bool IsOutPlace { get; set; }

        private int mDestCount = Int32.MaxValue;

        public bool Checked { get; set; }

        public string Name { get; set; }

        public string Path { get; set; }

        public string SrcPath { get; set; }

        public char Type { get; set; }

        public RestoreTreeNode Parent { get; set; }

        private readonly Dictionary<string, RestoreTreeNode> mChildren = new Dictionary<string, RestoreTreeNode>();

        public Dictionary<string, RestoreTreeNode> Children
        {
            get { return mChildren; }
        }

        public string DestName { get; set; }

        public bool HasDestNode { get; set; }

        public int KeepFolderOption { get; set; }

        public bool IgnoreThisNode { get; set; }

        public int DestCount
        {
            get { return mDestCount; }
            set { mDestCount = value; }
        }


        public RestoreTreeNode GetChild(string name)
        {
            RestoreTreeNode child;
            if (Children != null && Children.TryGetValue(name, out child))
            {
                return child;
            }
            return null;
        }

        public void AddChild(RestoreTreeNode child)
        {
            Children[child.Name] = child;
            child.Parent = this;
        }

        public bool RemoveChild(string name)
        {
            return this.Children != null && this.Children.Remove(name);
        }


        public override string ToString()
        {
            var tree = new StringBuilder("\n");
            AppendNodeString(tree, this, 0);
            return tree.ToString();
        }

        private static void AppendNodeString(StringBuilder tree, RestoreTreeNode node, int level)
        {
            var tab = new string('\t', level);
            tree.Append(tab);
            tree.AppendFormat("{0}Name:{1}, Ignore:{2}, Path:{3}\n", tab, node.Name, node.IgnoreThisNode, node.Path);
            if (node.Children == null)
            {
                return;
            }
            foreach (RestoreTreeNode child in node.Children.Values)
            {
                AppendNodeString(tree, child, level + 1);
            }
        }
    }
    public class Restorer
    {
        /// <summary>
        /// 如果当前节点没有子节点（包括深层子节点）被选中，并且“该节点本身没有被选中或者该节点为虚节点”，就删除该节点。
        /// </summary>
        public static bool DeleteNotUsedNodes(SPTreeNodeDto node)
        {
            bool isUsed = node.CheckNumber != 0;
            for (int i = 0; i < node.Children.Count; ++i)
            {
                SPTreeNodeDto child = node.Children[i];
                if (!DeleteNotUsedNodes(child))
                {
                    node.Children.RemoveAt(i);
                    --i;
                }
            }
            bool isVirtualNode = false;

            switch (node.Level)
            {
                case NodeLevel.Sites:
                case NodeLevel.Lists:
                case NodeLevel.Folders:
                case NodeLevel.Items:
                    isVirtualNode = true;
                    if (node.SelectAll == SelectAllState.Checked)
                    {
                        isUsed = true;
                        isVirtualNode = false;//Item select all的时候，没有反馈item节点信息。
                    }
                    break;
            }
            return node.Children.Count != 0 || (isUsed && !isVirtualNode);
        }
        public static SPTreeNodeDto GetChildNodeByLevel(SPTreeNodeDto node, NodeLevel level)
        {
            return node.Children.FirstOrDefault(subNode => subNode.Level == level);
        }
        public static char NodeValue2Type(NodeLevel value)
        {
            switch (value)
            {
                case NodeLevel.SiteCollection:
                    return AveConstants.TYPE_SITE;

                case NodeLevel.AppData:
                case NodeLevel.Site:
                    return AveConstants.TYPE_WEB;
                case NodeLevel.ProjectOnline:
                    return AveConstants.TYPE_PROJECT;
                //case AveConstants.TYPE_VALUE_DISSCUSSION_FORUM_LIST:
                //case AveConstants.TYPE_VALUE_DOCUMENT_LIBRARY_LIST:
                //case AveConstants.TYPE_VALUE_GENERIC_LIST:
                //case AveConstants.TYPE_VALUE_ISSUES_LIST:
                //case AveConstants.TYPE_VALUE_VOTE_OR_SURVEY_LIST:
                case NodeLevel.List:
                    return AveConstants.TYPE_LIST;
                case NodeLevel.App:
                    return AveConstants.TYPE_APP;
                case NodeLevel.Folder:
                    return AveConstants.TYPE_FOLDER;

                //case AveConstants.TYPE_VALUE_DOCUMENT:
                //    return AveConstants.TYPE_DOCUMENT;

                case NodeLevel.Item:
                    return AveConstants.TYPE_LISTITEM;

                //case AveConstants.TYPE_VALUE_ATTACHMENTS:
                //    return AveConstants.TYPE_ATTACHMENTS;

                //case AveConstants.TYPE_VALUE_VERSION:
                //    return AveConstants.TYPE_VERSION;

                default:
                    throw new AveException("Looks up a localized string similar to Unknown node type: {0}.", value);
            }
        }
    }
    public class InPlaceRestorer
    {
        internal RestoreTreeNode GenerateContentTree(SPTreeNodeDto sourceTree)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.InPlaceRestorer.GenerateContentTree"))
            {
                //Restorer.DeleteNotUsedNodes(sourceTree);
                sourceTree = sourceTree.Children[0]; //Get Farm Node
                var rootNode = new RestoreTreeNode
                {
                    Name = sourceTree.Children[0].Name,
                    Type = AveConstants.TYPE_WEBAPPLICATION,
                    IsOutPlace = false,
                };
                rootNode.Path = rootNode.Name;
                rootNode.HasDestNode = true;
                if (sourceTree.Children[0].CheckNumber == 1)
                {
                    rootNode.Checked = true;
                }
                foreach (SPTreeNodeDto siteCollection in sourceTree.Children.SelectMany(webApp => webApp.Children))
                {
                    GenerateContentTree(rootNode, siteCollection);
                }
                return rootNode;
            }
        }
        private void GenerateContentTree(RestoreTreeNode parentRestoreNode, SPTreeNodeDto srcNode)
        {
            var restoreNode = new RestoreTreeNode
            {
                Name = srcNode.Name,
                Type = Restorer.NodeValue2Type(srcNode.Level),
                HasDestNode = true,
                IsOutPlace = false,
            };
            if (srcNode.CheckNumber != 0)
            {
                restoreNode.Checked = true;
            }
            parentRestoreNode.AddChild(restoreNode);
            bool srcIsLast = srcNode.Children.Count == 0;

            switch (srcNode.Level)
            {
                case NodeLevel.SiteCollection: //destNode must be webapp node.
                    restoreNode.Path = srcNode.Name;
                    if (!srcIsLast)
                    {
                        GenerateContentTree(restoreNode, srcNode.Children[0]);
                    }
                    break;
                case NodeLevel.Site:
                    if (restoreNode.Name.Equals(AveConstants.ROOT_WEB, StringComparison.OrdinalIgnoreCase))
                    {
                        restoreNode.Path = parentRestoreNode.Path;
                    }
                    else
                    {
                        restoreNode.Path = parentRestoreNode.Path + "/" + restoreNode.Name;
                    }
                    if (!srcIsLast)
                    {
                        SPTreeNodeDto subSitesNodeTmp = Restorer.GetChildNodeByLevel(srcNode, NodeLevel.Sites);
                        if (subSitesNodeTmp != null)
                        {
                            foreach (SPTreeNodeDto subSite in subSitesNodeTmp.Children)
                            {
                                GenerateContentTree(restoreNode, subSite);
                            }
                            foreach (RestoreTreeNode subSiteNode in restoreNode.Children.Values)
                            {
                                if (srcNode.Parent.Level != NodeLevel.SiteCollection)
                                {
                                    subSiteNode.Name = restoreNode.Name + "/" + subSiteNode.Name;
                                }
                                restoreNode.Parent.AddChild(subSiteNode);
                                subSiteNode.Parent = restoreNode.Parent;
                            }
                            restoreNode.Children.Clear();
                        }

                        SPTreeNodeDto listsNode = Restorer.GetChildNodeByLevel(srcNode, NodeLevel.Lists);
                        if (listsNode != null)
                        {
                            foreach (SPTreeNodeDto listNode in listsNode.Children /*must be lists here*/)
                            {
                                GenerateContentTree(restoreNode, listNode);
                            }
                        }

                        SPTreeNodeDto projectsNode = Restorer.GetChildNodeByLevel(srcNode, NodeLevel.ProjectOnlines);
                        if (projectsNode != null)
                        {
                            foreach (SPTreeNodeDto projectNode in projectsNode.Children)
                            {
                                GenerateContentTree(restoreNode, projectNode);
                            }
                        }
                    }
                    break;
                case NodeLevel.ProjectOnline:
                    restoreNode.Path = parentRestoreNode.Path + "\\" + restoreNode.Name;
                    break;
                case NodeLevel.List:
                    restoreNode.Path = parentRestoreNode.Path + "\\" + restoreNode.Name;
                    if (!srcIsLast)
                    {
                        SPTreeNodeDto rootFolder = srcNode.Children[0];
                        SPTreeNodeDto subFoldersNode = Restorer.GetChildNodeByLevel(rootFolder, NodeLevel.Folders);
                        if (subFoldersNode != null)
                        {
                            foreach (SPTreeNodeDto subFolder in subFoldersNode.Children)
                            {
                                GenerateContentTree(restoreNode, subFolder);
                            }
                        }
                    }
                    break;
                case NodeLevel.Folder:
                    restoreNode.Path = parentRestoreNode.Path + "\\" + srcNode.Name;
                    SPTreeNodeDto srcFoldersNode = Restorer.GetChildNodeByLevel(srcNode, NodeLevel.Folders);
                    if (srcFoldersNode != null)
                    {
                        foreach (SPTreeNodeDto subFolder in srcFoldersNode.Children)
                        {
                            GenerateContentTree(restoreNode, subFolder);
                        }
                    }
                    break;
                default:
                    break;
            }
        }
    }
    public class OutOfPlaceRestorer
    {
        private static readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public OutofRestoreConfig Config { get; set; }

        public OutOfPlaceRestorer(OutofRestoreConfig outofRestoreConfig)
        {
            Config = outofRestoreConfig;
        }

        private SPTreeNodeDto GetSiteCollectionParentNode(SPTreeNodeDto tree)
        {
            while (tree != null && tree.Level != NodeLevel.SiteCollection)
            {
                tree = tree.Children.FirstOrDefault();
            }
            if (tree != null)
            {
                return tree.Parent;
            }
            throw new ArgumentNullException("Cannot find site collection level node in destination.");
        }

        internal RestoreTreeNode GenerateContentTree(SPTreeNodeDto sourceTree, SPTreeNodeDto destTree)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("GranularRestore.OutOfPlaceRestorer.GenerateContentTree"))
            {
                Restorer.DeleteNotUsedNodes(sourceTree);
                Restorer.DeleteNotUsedNodes(destTree);
                sourceTree = sourceTree.Children[0]; //Get Farm Node
                destTree = GetSiteCollectionParentNode(destTree);

                var rootNode = new RestoreTreeNode
                {
                    Name = destTree.Name,
                    Type = AveConstants.TYPE_WEBAPPLICATION,
                    IsOutPlace = true
                };
                rootNode.Path = rootNode.Name;
                rootNode.HasDestNode = true;
                if (destTree.CheckNumber == 1)
                {
                    rootNode.Checked = true;
                }

                foreach (SPTreeNodeDto siteCollection in sourceTree.Children.SelectMany(webApp => webApp.Children))
                {
                    GenerateContentTree(rootNode, siteCollection, destTree);
                }
                mLog.Info("Looks up a localized string similar to Configuration information: Restore Content to Stub: {0} Keep Site Structure:{1} Keep Folder Structure: {2} Result:\r\n{3}.", Config.RestoreContentsToSub, Config.KeepSiteStructure, Config.KeepFolderStructure, rootNode);

                return rootNode;
            }
        }

        private void GenerateContentTree(RestoreTreeNode parentRestoreNode, SPTreeNodeDto srcNode,
                                         SPTreeNodeDto destNode)
        {
            var restoreNode = new RestoreTreeNode
                                  {
                                      Name = srcNode.Name,
                                      Type = Restorer.NodeValue2Type(srcNode.Level),
                                      HasDestNode = true,
                                      IsOutPlace = true
                                  };
            if (srcNode.CheckNumber != 0)
            {
                restoreNode.Checked = true;
            }
            parentRestoreNode.AddChild(restoreNode);
            bool srcIsLast = srcNode.Children.Count == 0;
            bool destIsLast = destNode.Children.Count == 0;

            switch (srcNode.Level)
            {
                case NodeLevel.SiteCollection: //destNode must be webapp node.
                    if (destIsLast)
                    {
                        if (srcNode.Name.TrimEnd('/').StartsWith(srcNode.Parent.Name.TrimEnd('/'), StringComparison.OrdinalIgnoreCase))
                        {
                            string relateUrl = GetRelateUrl(srcNode.Name, srcNode.Parent.Name);
                            restoreNode.Path = (destNode.Name.Trim('/') + "/" + relateUrl).Trim('/');//需要考虑Root Site Collection的情况
                        }
                        else
                        {//Host Header
                            restoreNode.Path = srcNode.Name.TrimEnd('/');
                        }
                    }
                    else
                    {
                        destNode = destNode.Children[0];
                        restoreNode.Path = destNode.Name;
                        destIsLast = destNode.Children.Count == 0 ? true : string.IsNullOrEmpty(destNode.Children[0].SPObjectId);   //SAAS-11599 如果有两个SC的时候destNode.Children会有上一个的root节点，导致第二个SC的Node节点添加错误。
                        bool isRootWebNodeAddedByPreviousInvocation = false;
                        if (destNode.Children.Count == 1)
                        {
                            SPTreeNodeDto firstNode = destNode.Children[0];
                            if (firstNode.Name == AveConstants.ROOT_WEB && firstNode.Level == NodeLevel.Site && firstNode.Children.Count == 0)
                            {
                                isRootWebNodeAddedByPreviousInvocation = true;
                                //destIsLast = true;
                            }
                        }
                        if (destIsLast && Config.RestoreContentsToSub)//Site Collection to Site Collection, Attach还原应该将Root Site降级
                        {
                            if (srcIsLast)
                            {
                                AddRootWeb(srcNode);
                            }
                            if (!isRootWebNodeAddedByPreviousInvocation)
                            {
                                AddRootWeb(destNode);
                            }
                            srcNode.CheckNumber = destNode.CheckNumber = 0;
                            srcIsLast = destIsLast = false;
                        }
                    }

                    if (!srcIsLast)
                    {
                        if (!destIsLast)
                        {
                            destNode = destNode.Children[0]; //Root Site
                        }
                        GenerateContentTree(restoreNode, srcNode.Children[0], destNode);
                    }
                    break;
                case NodeLevel.AppData:
                case NodeLevel.Site:
                    string destSitePath = parentRestoreNode.Path;

                    bool srcHasList = Restorer.GetChildNodeByLevel(srcNode, NodeLevel.Lists) != null;
                    bool srcHasApp = Restorer.GetChildNodeByLevel(srcNode, NodeLevel.Apps) != null;
                    bool srcHasProject = Restorer.GetChildNodeByLevel(srcNode, NodeLevel.ProjectOnlines) != null;

                    bool parentHasSiteChecked = false;
                    {
                        //检查源端上层是不是选择了Site，如果不是，Site就不需要考虑保持结构了。
                        SPTreeNodeDto srcNodeTmp = srcNode.Parent;
                        while (srcNodeTmp.Level != NodeLevel.WebApplication)
                        {//如果Site Collection选择还原，这Root Web及下面的Sub Site就需要判断Keep Structure选项
                            if (srcNodeTmp.Level != NodeLevel.Sites && srcNodeTmp.CheckNumber != 0)
                            {
                                parentHasSiteChecked = true;
                                break;
                            }
                            srcNodeTmp = srcNodeTmp.Parent;
                        }
                    }

                    if (srcNode.CheckNumber != 0 || (Config.KeepSiteStructure && parentHasSiteChecked) || srcHasList || srcHasApp || srcHasProject)
                    {
                        bool isFirstRestoredSite = false;
                        if (srcNode.Level != NodeLevel.AppData) //SAAS-6371 源端只选择app，目的端选择subsite，destSitePath不能和正常web一样计算
                        {
                            //检查这个Site是不是没有Parent Site被Restore过，如果是第一次的话，就要找到真正的目的Site并考虑Restore Content to Stub选项
                            SPTreeNodeDto srcParentNodeTemp = srcNode;
                            while (srcParentNodeTemp.Level != NodeLevel.SiteCollection)
                            {
                                srcParentNodeTemp = srcParentNodeTemp.Parent;
                                if (srcParentNodeTemp.Level == NodeLevel.Sites)
                                {
                                    srcParentNodeTemp = srcParentNodeTemp.Parent;
                                }
                                if (srcParentNodeTemp.CheckNumber != 0 ||
                                    (Config.KeepSiteStructure && parentHasSiteChecked))
                                {//发现被Restore过的Parent
                                    break;
                                }
                            }
                            isFirstRestoredSite = srcParentNodeTemp.Level == NodeLevel.SiteCollection;

                            if (isFirstRestoredSite)
                            {
                                destSitePath = AveConstants.ROOT_WEB;
                                if (destNode.Level == NodeLevel.Site)
                                {
                                    //这里的DestNode一定是'.'，之所以不在这里进行'./‘的Trim，是因为目的端可能就是Root Site.
                                    while (!destIsLast)
                                    {
                                        //找到真正的目的端Site路径
                                        SPTreeNodeDto subSitesNodeTmp = destNode.Children[0];
                                        if (subSitesNodeTmp.Level == NodeLevel.Lists)
                                        {
                                            break;
                                        }
                                        destNode = subSitesNodeTmp.Children[0];
                                        destSitePath += "/" + destNode.Name;
                                        destIsLast = destNode.Children.Count == 0;
                                    }
                                }
                            }
                        }
                        if ((srcHasList || srcHasApp || srcHasProject) && !(Config.KeepSiteStructure && parentHasSiteChecked) &&
                            srcNode.CheckNumber == 0)
                        {
                            //这种情况下这个Site仅仅是为了保证下面的List能被还原到正确的位置才被还原的.
                            restoreNode.IgnoreThisNode = !isFirstRestoredSite;
                        }
                        else if (isFirstRestoredSite && (!Config.RestoreContentsToSub || parentHasSiteChecked/*这里肯定是Parent Site Collection被选中了，这时的Attach是针对Site Collection的，因此不需要判断，当成Merge来处理*/))
                        {//当前是第一个Resotre的Site，需要判断Merge，以后的直接Attach就行了
                        }
                        else
                        {
                            string siteName = srcNode.Name;
                            if (srcNode.Parent.Level == NodeLevel.SiteCollection) //srcNode.Name is '.'
                            {
                                int endIndex = srcNode.Parent.Name.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
                               ? srcNode.Parent.Name.IndexOf("/", "http://".Length, StringComparison.OrdinalIgnoreCase)
                               : srcNode.Parent.Name.IndexOf("/", "https://".Length, StringComparison.OrdinalIgnoreCase);

                                if (endIndex > 0)//如果除了http://或者https:// 后面没有‘/’就不裁剪
                                {
                                    siteName = srcNode.Parent.Name.Substring(srcNode.Parent.Name.LastIndexOf('/') + 1);
                                }
                                else
                                {
                                    siteName = srcNode.Title;
                                }
                                //这里有一个Bug，Root Site Collection或者HostHeader的Site的Urlname应该是什么？
                            }
                            destSitePath += "/" + siteName;
                        }
                    }
                    else
                    {
                        //不需要还原这个Site。
                        restoreNode.IgnoreThisNode = true;
                    }
                    if (destSitePath.StartsWith("./", StringComparison.Ordinal))
                    {
                        destSitePath = destSitePath.Substring("./".Length);
                    }
                    restoreNode.Path = destSitePath;

                    if (!srcIsLast)
                    {
                        //对于Site Level，首先进行本身的mapping，然后进行SubSites的Mapping，然后将SubSite的Mapping结果挂到Parent上，然后再处理下面的List Mapping。
                        //这么做的原因是SubSite的结果最后都要放到Site Collection上，因为RestoreNode只有一种类型的Children,Site Collection的是Site，Site的是List。
                        SPTreeNodeDto subSitesNodeTmp = Restorer.GetChildNodeByLevel(srcNode, NodeLevel.Sites);
                        if (subSitesNodeTmp != null)
                        {
                            foreach (SPTreeNodeDto subSite in subSitesNodeTmp.Children)
                            {
                                GenerateContentTree(restoreNode, subSite, destNode);
                            }
                            foreach (RestoreTreeNode subSiteNode in restoreNode.Children.Values)
                            {
                                if (srcNode.Parent.Level != NodeLevel.SiteCollection)
                                {
                                    subSiteNode.Name = restoreNode.Name + "/" + subSiteNode.Name;
                                }
                                restoreNode.Parent.AddChild(subSiteNode);
                                subSiteNode.Parent = restoreNode.Parent;
                            }
                            restoreNode.Children.Clear();
                        }

                        SPTreeNodeDto listsNode = Restorer.GetChildNodeByLevel(srcNode, NodeLevel.Lists);
                        if (listsNode != null)
                        {
                            if (!destIsLast)
                            {
                                destNode = destNode.Children[0].Children[0]; //must be list here
                            }
                            foreach (SPTreeNodeDto listNode in listsNode.Children /*must be lists here*/)
                            {
                                GenerateContentTree(restoreNode, listNode, destNode);
                            }
                        }
                        SPTreeNodeDto appsNode = Restorer.GetChildNodeByLevel(srcNode, NodeLevel.Apps);
                        if (appsNode != null && appsNode.Children.Count > 0)
                        {
                            if (!destIsLast)
                            {
                                destNode = destNode.Children[0].Children.Count > 0 ? destNode.Children[0].Children[0] : destNode.Children[0]; //must be list here
                            }
                            foreach (SPTreeNodeDto appNode in appsNode.Children)
                            {
                                GenerateContentTree(restoreNode, appNode, destNode);
                            }
                        }
                        SPTreeNodeDto projectsNode = Restorer.GetChildNodeByLevel(srcNode, NodeLevel.ProjectOnlines);
                        if(projectsNode != null && projectsNode.ChildrenCount > 0)
                        {
                            if (!destIsLast)
                            {
                                destNode = destNode.Children[0];
                            }
                            foreach(SPTreeNodeDto projectNode in projectsNode.Children)
                            {
                                GenerateContentTree(restoreNode, projectNode, destNode);
                            }
                        }
                    }
                    break;
                case NodeLevel.List:
                    if (destNode.Level == NodeLevel.List)
                    {
                        restoreNode.Path = parentRestoreNode.Path + "\\" + destNode.Name;
                    }
                    else
                    {
                        string listName = srcNode.Name.Substring(srcNode.Name.LastIndexOf('\\') + 1);
                        restoreNode.Path = parentRestoreNode.Path + "\\" + listName;
                    }

                    if (!srcIsLast)
                    {
                        SPTreeNodeDto rootFolder = srcNode.Children[0];//Root Folder
                        if (!destIsLast)
                        {
                            destNode = destNode.Children[0]; //Root Folder
                            destIsLast = destNode.Children.Count == 0;
                            while (!destIsLast)
                            {
                                destNode = destNode.Children[0].Children[0];
                                restoreNode.Path += "\\" + destNode.Name;
                                destIsLast = destNode.Children.Count == 0;
                            }
                        }

                        SPTreeNodeDto subFoldersNode = Restorer.GetChildNodeByLevel(rootFolder, NodeLevel.Folders);
                        if (subFoldersNode != null)
                        {
                            foreach (SPTreeNodeDto subFolder in subFoldersNode.Children)
                            {
                                GenerateContentTree(restoreNode, subFolder, destNode);
                            }
                        }
                    }
                    break;
                case NodeLevel.ProjectOnline:
                    string projectName = srcNode.Name.Substring(srcNode.Name.LastIndexOf('\\') + 1);
                    restoreNode.Path = parentRestoreNode.Path + "\\" + projectName;
                    break;
                case NodeLevel.App:
                    string appName = srcNode.Name.Substring(srcNode.Name.LastIndexOf('\\') + 1);
                    restoreNode.Path = parentRestoreNode.Path + "\\" + appName;
                    break;
                case NodeLevel.Folder:
                    //Folder跟Site Level逻辑相似，但简单在下面的几个方面，1.不需要处理Root Folder本身，路径名不需要截取Root Folder
                    //2.下面的Items不用处理 3.Keep Folder Structure肯定有意义。4.Restore Content to Sub只在源端仅仅是Folder时才起作用
                    string destFolderPath = parentRestoreNode.Path;
                    bool srcHasItems = Restorer.GetChildNodeByLevel(srcNode, NodeLevel.Items) != null;
                    if (srcNode.CheckNumber != 0 || Config.KeepFolderStructure || srcHasItems)
                    {
                        bool isFirstRestoredFolder = false;
                        bool allParentsNotChecked = true;
                        {
                            //检查当前Folder是不是第一次被Restore，是的话，就需要找到真正的目的端Folder路径（当目的端是Folder的情况下）
                            SPTreeNodeDto srcParentFolderNodeTemp = srcNode;
                            while (srcParentFolderNodeTemp.Level != NodeLevel.RootFolder)
                            {
                                srcParentFolderNodeTemp = srcParentFolderNodeTemp.Parent;
                                if (srcParentFolderNodeTemp.Level == NodeLevel.Folders)
                                {
                                    srcParentFolderNodeTemp = srcParentFolderNodeTemp.Parent;
                                }
                                if (srcParentFolderNodeTemp.CheckNumber != 0)
                                {
                                    allParentsNotChecked = false;
                                    break;
                                }
                            }
                            isFirstRestoredFolder = allParentsNotChecked && srcNode.CheckNumber != 0;
                        }

                        bool srcIsFolder = true;
                        {
                            SPTreeNodeDto srcTmp = srcNode.Parent;

                            while (srcTmp.Level != NodeLevel.WebApplication)
                            {
                                if (srcTmp.CheckNumber != 0 && srcTmp.Level != NodeLevel.Folder)
                                {
                                    srcIsFolder = false;
                                    break;
                                }
                                srcTmp = srcTmp.Parent;
                            }
                        }
                        if (allParentsNotChecked && srcNode.CheckNumber == 0)
                        {
                            //上层路过的Folder,不能Keep Structure.
                        }
                        else if (srcHasItems && !Config.KeepFolderStructure && srcNode.CheckNumber == 0)
                        {//这种情况下这个Folder仅仅是为了保证下面的Items能被还原到正确的位置才被还原的。如果是Site，就不需要还原了。
                        }
                        else if (srcIsFolder && isFirstRestoredFolder && !Config.RestoreContentsToSub)
                        {
                        }
                        else
                        {
                            destFolderPath += "\\" + srcNode.Name;
                        }
                    }
                    else
                    {
                        restoreNode.IgnoreThisNode = true;
                    }
                    restoreNode.Path = destFolderPath;
                    SPTreeNodeDto srcFoldersNode = Restorer.GetChildNodeByLevel(srcNode, NodeLevel.Folders);
                    if (srcFoldersNode != null)
                    {
                        foreach (SPTreeNodeDto subFolder in srcFoldersNode.Children)
                        {
                            GenerateContentTree(restoreNode, subFolder, destNode);
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        private void AddRootWeb(SPTreeNodeDto parent)
        {
            parent.Children.Add(new SPTreeNodeDto()
            {
                Name = AveConstants.ROOT_WEB,
                Level = NodeLevel.Site,
                Parent = parent,
                CheckNumber = parent.CheckNumber,
                Children = new List<SPTreeNodeDto>()
            }
            );
        }


        private string GetRelateUrl(string srcUrl, string srcParentUrl)
        {
            if (srcUrl.StartsWith(srcParentUrl, StringComparison.OrdinalIgnoreCase))
            {
                return srcUrl.Substring(srcParentUrl.TrimEnd('/').Length).Trim('/');
            }
            else
            {
                string tempUrl = srcUrl.Substring(srcUrl.IndexOf("//", StringComparison.Ordinal) + 2);
                if (tempUrl.Contains("/"))
                {
                    return tempUrl.Substring(tempUrl.IndexOf("/", StringComparison.Ordinal) + 1);
                }
                else
                {
                    return "";
                }
            }
        }

    }
}
