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
using System.Text;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;

namespace AvePoint.Common.Tree.Util
{
    public class StructureUtil
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(StructureUtil));
        private const string ID_PREFIX = "ID-";
        private const string EXPANDED_PREFIX = "Expanded-";
        private const string CHILDREN_LOADED_PREFIX = "ChildrenLoaded-";
        private const string CHILDREN_COUNT_PREFIX = "ChildrenCount-";
        private const string CHECK_NUMBER_PREFIX = "CheckNumber-";
        private const string INCLUDE_NEW_PREFIX = "IncludeNew-";
        private const string SELECT_ALL_PREFIX = "SelectAll-";
        private const string CURRENT_PAGE_PREFIX = "CurrentPage-";
        private const string START_INDEX_PREFIX = "StartIndex-";
        public static CommonTreeNodeFilter treeNodeFilter = new CommonTreeNodeFilter();
        /// <summary>
        /// 从Tree中删除虚拟节点
        /// </summary>
        /// <param name="node"></param>
        public static void RemoveVirtualNode(SPTreeNodeDto node)
        {
            if (node != null)
            {
                foreach (SPTreeNodeDto child in node.Children)
                {
                    RemoveVirtualNode(child);
                }

                if (node.Level == NodeLevel.Site && node.ChildrenLoaded)
                {
                    //将两个虚节点保存起来
                    SPTreeNodeDto listsNode = GetVirtualNode(node, NodeLevel.Lists);
                    SPTreeNodeDto sitesNode = GetVirtualNode(node, NodeLevel.Sites);
                    SPTreeNodeDto appsNode = GetVirtualNode(node, NodeLevel.Apps);

                    //删除虚节点
                    node.Children.Clear();

                    //用虚节点下的list和site填充子节点
                    if (listsNode != null)
                    {
                        node.Children.AddRange(listsNode.Children);
                    }
                    else
                    {
                        logger.Debug(string.Format("There is no lists virtual node under the {0} node", node.Name));
                        //return;
                    }
                    if (sitesNode != null)
                    {
                        node.Children.AddRange(sitesNode.Children);
                    }
                    else
                    {
                        logger.Debug(string.Format("There is no sites virtual node under the {0} node", node.Name));
                        //return;
                    }
                    node.ExtraOptions.Clear(); // 删除旧的虚节点属性记录
                    if (appsNode != null)
                    {
                        node.Children.AddRange(appsNode.Children);
                        node.ExtraOptions.AddRange(AssembleNodeExtraOptions(appsNode));
                    }

                    //将虚节点的属性记录到其父节点中
                    if (listsNode != null)
                    {
                        node.ExtraOptions.AddRange(AssembleNodeExtraOptions(listsNode));
                    }
                    if (sitesNode != null)
                    {
                        node.ExtraOptions.AddRange(AssembleNodeExtraOptions(sitesNode));
                    }
                }
            }
        }

        /// <summary>
        /// 向Tree中添加虚拟节点
        /// </summary>
        /// <param name="node"></param>
        public static void AddVirtualNode(SPTreeNodeDto node)
        {
            if (node != null)
            {
                foreach (SPTreeNodeDto child in node.Children)
                {
                    AddVirtualNode(child);
                }

                if (node.Level == NodeLevel.Site && node.ChildrenLoaded)
                {
                    //构造虚节点
                    SPTreeNodeDto listsNode = new SPTreeNodeDto() { ID = Guid.NewGuid().ToString(), Name = GConstants.SPNodeName.Lists, DisplayName = GConstants.SPNodeName.Lists, Level = NodeLevel.Lists, CanChildrenBeLoaded = true, FarmID = node.FarmID, SPType = node.SPType, SPVersion = node.SPVersion };
                    SPTreeNodeDto sitesNode = new SPTreeNodeDto() { ID = Guid.NewGuid().ToString(), Name = GConstants.SPNodeName.Sites, DisplayName = GConstants.SPNodeName.Sites, Level = NodeLevel.Sites, CanChildrenBeLoaded = true, FarmID = node.FarmID, SPType = node.SPType, SPVersion = node.SPVersion };
                    SPTreeNodeDto appsNode = new SPTreeNodeDto() { ID = Guid.NewGuid().ToString(), Name = GConstants.SPNodeName.Apps, DisplayName = GConstants.SPNodeName.Apps, Level = NodeLevel.Apps, CanChildrenBeLoaded = true, FarmID = node.FarmID, SPType = node.SPType, SPVersion = node.SPVersion };

                    //将site下的子节点分散到两个虚节点下
                    foreach (SPTreeNodeDto child in node.Children)
                    {
                        if (child.Level == NodeLevel.Sites || child.Level == NodeLevel.Lists || child.Level == NodeLevel.Apps)
                        {
                            logger.Debug(string.Format("Sites or Lists or Apps virtual node already exists under the {0} node, please check your program to access the method.", node.Name));
                        }
                        if (child.Level == NodeLevel.List)
                        {
                            child.Parent = listsNode;
                            listsNode.Children.Add(child);
                        }
                        else if (child.Level == NodeLevel.Site)
                        {
                            child.Parent = sitesNode;
                            sitesNode.Children.Add(child);
                        }
                        else if (child.Level == NodeLevel.App)
                        {
                            child.Parent = appsNode;
                            appsNode.Children.Add(child);
                        }
                    }

                    //清空子节点
                    node.Children.Clear();

                    //设置虚节点属性
                    ParseNodeExtraOptions(listsNode, node.ExtraOptions);
                    ParseNodeExtraOptions(sitesNode, node.ExtraOptions);
                    if (node.SPVersion == GConstants.SPVersion.MOSS13)
                    {
                        ParseNodeExtraOptions(appsNode, node.ExtraOptions);
                    }

                    //将虚拟节点添加到该节点下
                    node.Children.Add(listsNode);
                    listsNode.Parent = node;
                    node.Children.Add(sitesNode);
                    sitesNode.Parent = node;
                    if (node.SPVersion == GConstants.SPVersion.MOSS13)
                    {
                        node.Children.Add(appsNode);
                        appsNode.Parent = node;

                        appsNode.Offset = 0;
                        listsNode.Offset = 1;
                        sitesNode.Offset = 2;
                    }
                    else
                    {
                        listsNode.Offset = 0;
                        sitesNode.Offset = 1;
                    }
                    node.ChildrenCount = node.Children.Count;
                }
            }
        }



        /// <summary>
        /// 向Tree中添加虚拟节点
        /// </summary>
        /// <param name="root"></param>
        public static void AddVirtualNodeForAdminSearch(SPTreeNodeDto root)
        {
            if (root != null)
            {
                foreach (SPTreeNodeDto child in root.Children)
                {
                    AddVirtualNodeForAdminSearch(child);
                }

                #region sites lists rootfolder folders items

                if (root.Level == NodeLevel.Site || root.Level == NodeLevel.RootFolder || root.Level == NodeLevel.Folder)
                {
                    if ((root.Children == null || root.Children.Count == 0))
                    {
                        return;
                    }
                    //构造虚节点
                    SPTreeNodeDto listsNode = new SPTreeNodeDto() { ID = Guid.NewGuid().ToString(), Name = GConstants.SPNodeName.Lists, Level = NodeLevel.Lists, CanChildrenBeLoaded = false, FarmID = root.FarmID, SPType = root.SPType, SPVersion = root.SPVersion };
                    SPTreeNodeDto sitesNode = new SPTreeNodeDto() { ID = Guid.NewGuid().ToString(), Name = GConstants.SPNodeName.Sites, Level = NodeLevel.Sites, CanChildrenBeLoaded = false, FarmID = root.FarmID, SPType = root.SPType, SPVersion = root.SPVersion };
                    SPTreeNodeDto foldersNode = new SPTreeNodeDto() { ID = Guid.NewGuid().ToString(), Name = GConstants.SPNodeName.Folders, Level = NodeLevel.Folders, CanChildrenBeLoaded = false, FarmID = root.FarmID, SPType = root.SPType, SPVersion = root.SPVersion };
                    SPTreeNodeDto itemsNode = new SPTreeNodeDto() { ID = Guid.NewGuid().ToString(), Name = GConstants.SPNodeName.Items, Level = NodeLevel.Items, CanChildrenBeLoaded = false, FarmID = root.FarmID, SPType = root.SPType, SPVersion = root.SPVersion, PageNodeType = PageNodeType.PreNext };

                    listsNode.DisplayName = listsNode.Name;
                    sitesNode.DisplayName = sitesNode.Name;
                    foldersNode.DisplayName = foldersNode.Name;
                    itemsNode.DisplayName = itemsNode.Name;

                    listsNode.SPObjectId = listsNode.ID;
                    sitesNode.SPObjectId = sitesNode.ID;
                    foldersNode.SPObjectId = foldersNode.ID;
                    itemsNode.SPObjectId = itemsNode.ID;

                    //将site下的子节点分散到两个虚节点下
                    if (root.Level == NodeLevel.Site)
                    {
                        foreach (SPTreeNodeDto child in root.Children)
                        {
                            if (child.Level == NodeLevel.List)
                            {
                                listsNode.Children.Add(child);
                            }
                            else if (child.Level == NodeLevel.Site)
                            {
                                sitesNode.Children.Add(child);
                            }
                        }
                        //清空子节点
                        root.Children.Clear();

                        if (listsNode.Children.Count == 0)
                        {
                            listsNode.ChildrenLoaded = false;
                            listsNode.Expanded = false;
                            listsNode.CanChildrenBeLoaded = true;
                        }
                        else
                        {
                            listsNode.ChildrenLoaded = true;
                            listsNode.Expanded = true;
                            listsNode.CanChildrenBeLoaded = false;
                        }
                        if (sitesNode.Children.Count == 0)
                        {
                            sitesNode.ChildrenLoaded = false;
                            sitesNode.Expanded = false;
                            sitesNode.CanChildrenBeLoaded = true;
                        }
                        else
                        {
                            sitesNode.ChildrenLoaded = true;
                            sitesNode.Expanded = true;
                            sitesNode.CanChildrenBeLoaded = false;
                        }
                        listsNode.ChildrenCount = listsNode.Children.Count;
                        sitesNode.ChildrenCount = sitesNode.Children.Count;
                        //将虚拟节点添加到该节点下
                        root.Children.Add(listsNode);
                        root.Children.Add(sitesNode);
                        root.ChildrenCount = root.Children.Count;
                    }
                    else if (root.Level == NodeLevel.RootFolder)
                    {
                        foreach (SPTreeNodeDto child in root.Children)
                        {
                            if (child.Level == NodeLevel.Folder)
                            {
                                foldersNode.Children.Add(child);
                            }
                            else if (child.Level == NodeLevel.Item)
                            {
                                itemsNode.Children.Add(child);
                            }
                        }

                        //清空子节点
                        root.Children.Clear();

                        if (foldersNode.Children.Count == 0)
                        {
                            foldersNode.ChildrenLoaded = false;
                            foldersNode.Expanded = false;
                            foldersNode.CanChildrenBeLoaded = true;
                        }
                        else
                        {
                            foldersNode.ChildrenLoaded = true;
                            foldersNode.Expanded = true;
                            foldersNode.CanChildrenBeLoaded = false;
                        }
                        if (itemsNode.Children.Count == 0)
                        {
                            itemsNode.ChildrenLoaded = false;
                            itemsNode.Expanded = false;
                            itemsNode.CanChildrenBeLoaded = true;
                        }
                        else
                        {
                            itemsNode.ChildrenLoaded = true;
                            itemsNode.Expanded = true;
                            itemsNode.CanChildrenBeLoaded = false;
                        }

                        //将虚拟节点添加到该节点下
                        foldersNode.ChildrenCount = foldersNode.Children.Count;
                        itemsNode.ChildrenCount = itemsNode.Children.Count;
                        root.Children.Add(foldersNode);
                        foldersNode.Parent = root;
                        root.Children.Add(itemsNode);
                        itemsNode.Parent = root;
                        root.ChildrenCount = root.Children.Count;
                        root.ChildrenLoaded = true;
                        root.CanChildrenBeLoaded = false;
                    }
                    else
                    {
                        foreach (SPTreeNodeDto child in root.Children)
                        {
                            if (child.Level == NodeLevel.Folder)
                            {
                                foldersNode.Children.Add(child);
                            }
                            else if (child.Level == NodeLevel.Item)
                            {
                                itemsNode.Children.Add(child);
                            }
                        }

                        //清空子节点
                        root.Children.Clear();

                        if (foldersNode.Children.Count == 0)
                        {
                            foldersNode.ChildrenLoaded = false;
                            foldersNode.Expanded = false;
                            foldersNode.CanChildrenBeLoaded = true;
                        }
                        else
                        {
                            foldersNode.ChildrenLoaded = true;
                            foldersNode.Expanded = true;
                            foldersNode.CanChildrenBeLoaded = false;
                        }
                        if (itemsNode.Children.Count == 0)
                        {
                            itemsNode.ChildrenLoaded = false;
                            itemsNode.Expanded = false;
                            itemsNode.CanChildrenBeLoaded = true;
                        }
                        else
                        {
                            itemsNode.ChildrenLoaded = true;
                            itemsNode.Expanded = true;
                            itemsNode.CanChildrenBeLoaded = false;
                        }

                        foldersNode.ChildrenCount = foldersNode.Children.Count;
                        itemsNode.ChildrenCount = itemsNode.Children.Count;
                        //将虚拟节点添加到该节点下
                        root.Children.Add(foldersNode);
                        foldersNode.Parent = root;
                        root.Children.Add(itemsNode);
                        itemsNode.Parent = root;
                        root.ChildrenCount = root.Children.Count;
                    }
                }
                #endregion



            }
        }

        private static SPTreeNodeDto GetVirtualNode(SPTreeNodeDto node, NodeLevel level)
        {
            foreach (var child in node.Children)
            {
                if (level == child.Level)
                {
                    return child;
                }
            }
            return null;
        }

        /// <summary>
        /// 从虚节点中取出附加信息
        /// </summary>
        /// <param name="virtualNode">虚节点</param>
        /// <returns>附加信息</returns>
        private static List<NodeExtraOption> AssembleNodeExtraOptions(SPTreeNodeDto virtualNode)
        {
            List<NodeExtraOption> extraOptions = new List<NodeExtraOption>();

            NodeExtraOption idOption = new NodeExtraOption() { Key = ID_PREFIX + virtualNode.ID, value = virtualNode.ID };
            NodeExtraOption expandedOption = new NodeExtraOption() { Key = EXPANDED_PREFIX + virtualNode.Level, value = virtualNode.Expanded.ToString() };
            NodeExtraOption childrenLoadedOption = new NodeExtraOption() { Key = CHILDREN_LOADED_PREFIX + virtualNode.Level, value = virtualNode.ChildrenLoaded.ToString() };
            NodeExtraOption checkNumberOption = new NodeExtraOption() { Key = CHECK_NUMBER_PREFIX + virtualNode.Level, value = virtualNode.CheckNumber.ToString() };
            NodeExtraOption includeNewOption = new NodeExtraOption() { Key = INCLUDE_NEW_PREFIX + virtualNode.Level, value = virtualNode.IncludeNew.ToString() };
            NodeExtraOption selectAllOption = new NodeExtraOption() { Key = SELECT_ALL_PREFIX + virtualNode.Level, value = virtualNode.SelectAll.ToString() };
            NodeExtraOption currentPageOption = new NodeExtraOption() { Key = CURRENT_PAGE_PREFIX + virtualNode.Level, value = virtualNode.CurrentPage.ToString() };
            NodeExtraOption startIndexOption = new NodeExtraOption() { Key = START_INDEX_PREFIX + virtualNode.Level, value = virtualNode.StartIndex.ToString() };
            NodeExtraOption childrenCountOption = new NodeExtraOption() { Key = CHILDREN_COUNT_PREFIX + virtualNode.Level, value = virtualNode.ChildrenCount.ToString() };


            extraOptions.Add(idOption);
            extraOptions.Add(expandedOption);
            extraOptions.Add(childrenLoadedOption);
            extraOptions.Add(checkNumberOption);
            extraOptions.Add(includeNewOption);
            extraOptions.Add(selectAllOption);
            extraOptions.Add(currentPageOption);
            extraOptions.Add(startIndexOption);
            extraOptions.Add(childrenCountOption);
            return extraOptions;
        }

        /// <summary>
        /// 将附加信息设置到虚节点内
        /// </summary>
        /// <param name="virtualNode">虚节点</param>
        /// <param name="extraOptions">附加信息</param>
        private static void ParseNodeExtraOptions(SPTreeNodeDto virtualNode, List<NodeExtraOption> extraOptions)
        {
            foreach (NodeExtraOption extraOption in extraOptions)
            {
                if (extraOption.Key.Equals(ID_PREFIX + virtualNode.Level))
                {
                    virtualNode.ID = extraOption.value;
                }
                else if (extraOption.Key.Equals(EXPANDED_PREFIX + virtualNode.Level))
                {
                    virtualNode.Expanded = Boolean.Parse(extraOption.value);
                }
                else if (extraOption.Key.Equals(CHILDREN_LOADED_PREFIX + virtualNode.Level))
                {
                    virtualNode.ChildrenLoaded = Boolean.Parse(extraOption.value);
                }
                else if (extraOption.Key.Equals(CHECK_NUMBER_PREFIX + virtualNode.Level))
                {
                    virtualNode.CheckNumber = Int32.Parse(extraOption.value);
                }
                else if (extraOption.Key.Equals(INCLUDE_NEW_PREFIX + virtualNode.Level))
                {
                    //virtualNode.IncludeNew = Boolean.Parse(extraOption.value);
                    virtualNode.IncludeNew = (IncludeNewState)Enum.Parse(typeof(IncludeNewState), extraOption.value, true);
                }
                else if (extraOption.Key.Equals(SELECT_ALL_PREFIX + virtualNode.Level))
                {
                    //virtualNode.SelectAll = Boolean.Parse(extraOption.value);
                    virtualNode.SelectAll = (SelectAllState)Enum.Parse(typeof(SelectAllState), extraOption.value, true);
                }
                else if (extraOption.Key.Equals(CURRENT_PAGE_PREFIX + virtualNode.Level))
                {
                    virtualNode.CurrentPage = Int32.Parse(extraOption.value);
                }

                else if (extraOption.Key.Equals(CHILDREN_COUNT_PREFIX + virtualNode.Level))
                {
                    virtualNode.ChildrenCount = Int32.Parse(extraOption.value);
                }


                else if (extraOption.Key.Equals(START_INDEX_PREFIX + virtualNode.Level))
                {
                    virtualNode.StartIndex = Int32.Parse(extraOption.value);
                }


            }
        }

        /// <summary>
        /// 将没有分层次的site节点分层
        /// 预备给Admin Search 使用，但是由于结果变为10的结构，所以此方法暂时不使用了
        /// </summary>
        /// <param name="node"></param>
        /*
        public static void ChangeNodeSturcture(SPTreeNodeDto node)
        {
            if (node != null && node.Level <= GConstants.SPNodeLevel.SiteCollection)
            {
                foreach (SPTreeNodeDto child in node.Children)
                {
                    ChangeNodeSturcture(child);
                }
                if (node.Level == GConstants.SPNodeLevel.SiteCollection)
                {
                    ChangeSiteStructure(node);
                }
            }
        }

        private static void ChangeSiteStructure(SPTreeNodeDto parent)
        {
            if (parent.Level <= GConstants.SPNodeLevel.Site)
            {
                Dictionary<string, SPTreeNodeDto> siteDictionary = new Dictionary<string, SPTreeNodeDto>();
                foreach (SPTreeNodeDto site in parent.Children)
                {
                    //构造虚节点
                    SPTreeNodeDto listsNode = new SPTreeNodeDto() { ID = Guid.NewGuid().ToString(), Name = GConstants.SPNodeName.Lists, Level = GConstants.SPNodeLevel.Lists, CanChildrenBeLoaded = true, FarmID = site.FarmID };
                    SPTreeNodeDto sitesNode = new SPTreeNodeDto() { ID = Guid.NewGuid().ToString(), Name = GConstants.SPNodeName.Sites, Level = GConstants.SPNodeLevel.Sites, CanChildrenBeLoaded = true, FarmID = site.FarmID };
                    listsNode.Children.AddRange(site.Children);
                    site.Children.Clear();
                    site.Children.Add(listsNode);
                    site.Children.Add(sitesNode);
                    siteDictionary[site.ID] = site;
                }
                SPTreeNodeDto rootSite = null;
                foreach (SPTreeNodeDto site in parent.Children)
                {
                    if (!siteDictionary.ContainsKey(site.ParentId))
                    {
                        rootSite = site;
                    }
                    else
                    {
                        SPTreeNodeDto sitesNode = GetVirtualNode(siteDictionary[site.ParentId], GConstants.SPNodeLevel.Sites);
                        if (sitesNode != null)
                        {
                            sitesNode.Children.Add(site);
                        }
                    }
                }
                parent.Children.Clear();
                if (rootSite != null)
                {
                    parent.Children.Add(rootSite);
                }
            }
        }
        */

        /// <summary>
        /// 通过ParentId，将一组节点组装成Tree
        /// </summary>
        /// <param name="nodeList"></param>
        /// <returns></returns>
        public static List<SPTreeNodeDto> AssembleTree(List<SPTreeNodeDto> nodeList)
        {
            Dictionary<string, SPTreeNodeDto> nodeDictionary = new Dictionary<string, SPTreeNodeDto>();
            foreach (SPTreeNodeDto node in nodeList)
            {
                nodeDictionary.Add(node.ID, node);
            }
            List<SPTreeNodeDto> result = new List<SPTreeNodeDto>();
            for (int i = 0; i < nodeList.Count; i++)
            {
                SPTreeNodeDto node = nodeList[i];
                if (!string.IsNullOrEmpty(node.ParentId))
                {
                    if (nodeDictionary.ContainsKey(node.ParentId))
                    {
                        nodeDictionary[node.ParentId].Children.Add(node);
                        nodeDictionary[node.ParentId].ChildrenCount = nodeDictionary[node.ParentId].Children.Count;
                    }
                }
                else
                {
                    result.Add(node);
                }
            }
            return result;
        }

        /// <summary>
        /// 设置节点在GUI的显示属性
        /// </summary>
        /// <param name="nodeList"></param>
        /// <param name="CanChildrenBeLoaded">节点是否可以继续展开</param>
        /// <param name="ChildrenLoaded">节点的子节点是否已经被载入</param>
        /// <param name="Expand">节点是否展开</param>
        public static void SetNodeProperties<T>(List<T> nodeList, bool CanChildrenBeLoaded, bool ChildrenLoaded, bool Expand) where T : AveTreeNodeDto<T>
        {
            foreach (T node in nodeList)
            {
                node.CanChildrenBeLoaded = CanChildrenBeLoaded;
                node.ChildrenLoaded = ChildrenLoaded;
                node.Expanded = Expand;
                SetNodeProperties(node.Children, CanChildrenBeLoaded, ChildrenLoaded, Expand);
            }
        }

        /// <summary>
        /// 删除System Folder节点，由于System Folder在前台进行了隐藏，所以此方法废弃。
        /// </summary>
        /// <param name="node"></param>
        //public static void RemoveSystemFolder(SPTreeNodeDto node)
        //{
        //    for (int i = node.Children.Count - 1; i >= 0; i--)
        //    {
        //        if (node.Children[i].Level == NodeLevel.List && node.Children[i].Name.Equals("{System Folder}"))
        //        {
        //            node.Children.RemoveAt(i);
        //        }
        //        else
        //        {
        //            RemoveSystemFolder(node.Children[i]);
        //        }
        //    }
        //}

        public static void SetChildren(SPTreeNodeDto parent, List<SPTreeNodeDto> children)
        {
            foreach (SPTreeNodeDto child in children)
            {
                child.Parent = parent;
                child.CheckNumber = parent.CheckNumber;
            }
            parent.Children = children;
            parent.ChildrenCount = children.Count;
            parent.ChildrenLoaded = true;
            parent.Expanded = true;
        }

        public static void AddChild(SPTreeNodeDto parent, SPTreeNodeDto child)
        {
            if (parent.Children == null)
            {
                parent.Children = new List<SPTreeNodeDto>();
            }
            parent.Children.Add(child);
            child.Parent = parent;
            parent.ChildrenCount = parent.Children.Count;
            parent.ChildrenLoaded = true;
            parent.Expanded = true;
        }


        /// <summary>
        /// 递归遍历Tree，获取选中节点的List。
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<IAveTreeNodeDto> GetSelectedNodeList(IAveTreeNodeDto node)
        {
            List<IAveTreeNodeDto> nodeList = new List<IAveTreeNodeDto>();
            if (node.CheckNumber == GConstants.TreeCheckNumber.CHECKED)
            {
                nodeList.Add(node);
            }
            foreach (IAveTreeNodeDto child in node.Children)
            {
                nodeList.AddRange(GetSelectedNodeList(child));
            }
            return nodeList;
        }

        public static string AssembleRelativePath(FSTreeNodeDto node)
        {
            StringBuilder relativePathBuilder = new StringBuilder();
            while (node != null && node.Level != NodeLevel.Device)
            {
                if (relativePathBuilder.Length == 0)
                {
                    relativePathBuilder.Append(node.Name);
                }
                else
                {
                    relativePathBuilder.Insert(0, node.Name + "\\");
                }
                node = node.Parent;
            }
            return relativePathBuilder.ToString();
        }

        /// <summary>
        /// 自下而上对节点的Children用“模板默认规则+NameFilter”对比该节点显示在界面上的名字进行过滤。
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="searchFilter"></param>
        public static void FilterChildrenBySearchString(IAveTreeNodeDto currentNode, string searchFilter)
        {
            if (ShouldNodeBeReserved(currentNode, searchFilter))
            {
                currentNode.Children.Clear();
                return;
            }

            List<IAveTreeNodeDto> newChildren = new List<IAveTreeNodeDto>();
            foreach (IAveTreeNodeDto child in currentNode.Children)
            {
                child.FilteredOffset = -1;
                if (treeNodeFilter.IsMatch(child, currentNode.FilterPolicy)) //保证只遍历属于当前模块的节点的分支。
                {
                    FilterChildrenBySearchString(child, searchFilter);
                    //深度遍历：如果有子节点满足条件，就不对当前节点进行过滤。
                    if (child.FilteredChildrenCount > 0 || ShouldNodeBeReserved(child, searchFilter))
                    {
                        newChildren.Add(child);
                    }
                }
            }

            int offset = 0;
            currentNode.Children.Clear();
            foreach (IAveTreeNodeDto filteredNode in newChildren)
            {
                filteredNode.Offset = -1;
                filteredNode.FilteredOffset = offset;
                offset++;
                currentNode.Children.Add(filteredNode);
            }
            currentNode.FilteredChildrenCount = offset;
        }

        public static bool ShouldNodeBeReserved(IAveTreeNodeDto node, string searchFilter)
        {
            if (string.IsNullOrEmpty(searchFilter))
            {
                return true;
            }
            IAveTreeNodeDto tempNode = node;

            if (!string.IsNullOrEmpty(tempNode.DisplayName) && tempNode.DisplayName.ToLower().Contains(searchFilter.ToLower()))
            {
                return true;
            }


            return false;
        }



    }
}
