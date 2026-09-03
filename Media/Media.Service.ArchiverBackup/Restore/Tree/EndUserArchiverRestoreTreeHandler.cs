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



using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Tree.Object;
using Merged18NResources.MediaServiceArchiverBackup;
using AvePoint.Media.Service.DomainModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Media.Service.ArchiverBackup
{
    public class EndUserArchiverRestoreTreeHandler
        : IRestoreServiceTreeHandler
    {
        readonly static Object syncIndexItemProceedObject = new Object();
        AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);
        String currentSiteCollectionUrl;
        Boolean isJustCalculateCount;
        ArchiverRestoreJob restoreJob;
        List<IndexItemProceedEventArgs> attachementInfos;

        EventHandler<IndexItemProceedEventArgs> indexItemProceed;
        public IArchiverRestoreIndexService RestoreIndexService { get; set; }

        public event EventHandler<IndexItemProceedEventArgs> IndexItemProceed
        {
            add
            {
                lock (syncIndexItemProceedObject)
                {
                    this.indexItemProceed += value;
                }
            }
            remove
            {
                lock (syncIndexItemProceedObject)
                {
                    this.indexItemProceed -= value;
                }
            }
        }

        public SPTreeNodeDto CutTree(SPTreeNodeDto rootTree)
        {
            SPTreeNodeDto treeNodeDto = null;
            if (rootTree.Level == NodeLevel.Items)
            {
                if (rootTree.CheckNumber == 1 || rootTree.SelectAll == SelectAllState.Checked)
                    treeNodeDto = rootTree;
                else
                {
                    if (rootTree.ChildrenCount > 0)
                    {
                        foreach (SPTreeNodeDto item in rootTree.Children)
                        {
                            if (item.CheckNumber == 1)
                            {
                                treeNodeDto = rootTree;
                                break;
                            }
                        }
                    }
                }
            }
            else
            {
                List<SPTreeNodeDto> children = new List<SPTreeNodeDto>();
                foreach (SPTreeNodeDto child in rootTree.Children)
                {
                    SPTreeNodeDto selectedNodeDto = CutTree(child);
                    if (selectedNodeDto != null)
                        children.Add(selectedNodeDto);
                }
                rootTree.Children.Clear();
                rootTree.Children.AddRange(children);
                if (rootTree.CheckNumber == 1 || rootTree.Children.Count > 0)
                    treeNodeDto = rootTree;
            }
            return treeNodeDto;
        }

        public void ProcessTreeNode(TreeNodeParameter treeParam)
        {
            if (treeParam.RestoreJob is ArchiverRestoreJob)
            {
                this.restoreJob = (ArchiverRestoreJob)treeParam.RestoreJob;
                this.isJustCalculateCount = treeParam.IsJustCalculateCount;
                this.currentSiteCollectionUrl = treeParam.CurrentTree.Name;
                this.attachementInfos = new List<IndexItemProceedEventArgs>();
                this.ProcessNodeDtoInternal(treeParam.CurrentTree.Name, treeParam.CurrentTree);
            }
            else
            {
                throw new ArgumentException("Cannot convert restore job to ArchiverRestoreJob", "treeParam");
            }
        }

        private void ProcessNodeDtoInternal(string currentPath, SPTreeNodeDto nodeDto)
        {
            if (restoreJob.EndUserRequestItems.Count > 0)
            {
                List<EndUserTreeNode> endUserTreeNodes = new List<EndUserTreeNode>();
                foreach (var i in restoreJob.EndUserRequestItems)
                {
                    var currentIndex = RestoreIndexService.GetCurrentIndex(i.PathMD5);
                    var currentNode = new EndUserTreeNode(currentIndex);
                    this.AddParentNode(currentNode);
                    endUserTreeNodes.Add(currentNode);
                }
                if (endUserTreeNodes.Count > 0)
                {
                    EndUserTreeNode standardBranch = GetFirstNode(endUserTreeNodes[0]);

                    for (int i = 1; i < endUserTreeNodes.Count; i++)
                    {
                        var treeNodes = ConvertBranchToTreeNodes(GetFirstNode(endUserTreeNodes[i]));
                        var temp = CombineTwoBranches(standardBranch, treeNodes, 0);
                        if (temp != null)
                        {
                            standardBranch = temp;
                        }
                    }
                    ProcessTreeNode(standardBranch);
                }
            }
            else
            {
                logger.Warn("EndUserRequestItems count is 0");
            }
        }

        private EndUserTreeNode GetFirstNode(EndUserTreeNode node)
        {
            EndUserTreeNode temp = node;
            while (temp.ParentNode != null)
            {
                temp = temp.ParentNode;
            }
            return temp;
        }

        private Dictionary<int, EndUserTreeNode> ConvertBranchToTreeNodes(EndUserTreeNode branch)
        {
            Dictionary<int, EndUserTreeNode> treeNodes = new Dictionary<int, EndUserTreeNode>();
            EndUserTreeNode tempNode = branch;
            int depth = 0;
            treeNodes[depth] = tempNode;
            while (tempNode.ChildNodes != null && tempNode.ChildNodes.Count != 0)
            {
                depth++;
                treeNodes[depth] = tempNode.ChildNodes[0];
                tempNode = tempNode.ChildNodes[0];
            }
            return treeNodes;
        }

        private EndUserTreeNode CombineTwoBranches(EndUserTreeNode standardBranch, Dictionary<int, EndUserTreeNode> compareBranch, int depth)
        {
            EndUserTreeNode resultTree = standardBranch;
            bool hasChanged = default(bool);
            if (resultTree.ChildNodes == null)
            {
                resultTree.ChildNodes = new List<EndUserTreeNode>();
            }
            if (resultTree.ChildNodes.Count != 0 && compareBranch.ContainsKey(depth + 1))
            {
                bool hasNode = default(bool);
                List<EndUserTreeNode> tempChildren = resultTree.ChildNodes;
                foreach (var node in tempChildren)
                {
                    if (node.NodeMd5Value == compareBranch[depth + 1].NodeMd5Value)
                    {
                        hasNode = true;
                        EndUserTreeNode tempNode = CombineTwoBranches(node, compareBranch, depth + 1);
                        if (tempNode != null)
                            node.ChildNodes.Add(tempNode);
                    }
                }
                if (!hasNode)
                {
                    hasChanged = true;
                    if (depth == 0)
                        resultTree.ChildNodes.Add(compareBranch[depth + 1]);
                    else
                        resultTree = compareBranch[depth + 1];
                }
            }
            else if (resultTree.ChildNodes.Count == 0 && compareBranch.ContainsKey(depth + 1))
            {
                hasChanged = true;
                if (depth == 0)
                    resultTree.ChildNodes.Add(compareBranch[depth + 1]);
                else
                    resultTree = compareBranch[depth + 1];
            }
            return (hasChanged || depth == 0) ? resultTree : null;
        }

        public void ProcessTreeNode(EndUserTreeNode node)
        {
            if (node != null)
            {
                OnIndexItemProceed(new IndexItemProceedEventArgs { IndexCount = 1, IndexItem = node.Index, MarkMessage = new RestoreMarkMessage() { Security = SecurityState.Checked, Property = PropertyState.Checked, IsChecked = true, VersionFlag = 1 } });

                if (node.ChildNodes != null)
                {
                    if (node.Level == TreeNodeLevel.List || node.Level == TreeNodeLevel.Folder)
                    {
                        List<EndUserTreeNode> sortList = new List<EndUserTreeNode>();
                        var itemNodeList = (from child in node.ChildNodes
                                            where child.Level == TreeNodeLevel.Item && child.Index != null
                                            && (child.Index.Type.EqualsIgnoreCase("D") || child.Index.Type.EqualsIgnoreCase("I"))
                                            orderby child.Name descending
                                            select child).ToList<EndUserTreeNode>();
                        sortList.AddRange(SortItems(itemNodeList));
                        var attNodeList = (from child in node.ChildNodes
                                           where child.Level == TreeNodeLevel.Item && child.Index != null
                                           && child.Index.Type.EqualsIgnoreCase("A")
                                           orderby child.Name descending
                                           select child).ToList<EndUserTreeNode>();
                        sortList.AddRange(attNodeList);
                        var folderNodeList = (from child in node.ChildNodes
                                              where child.Level == TreeNodeLevel.Folder
                                              orderby child.Name descending
                                              select child).ToList<EndUserTreeNode>();
                        sortList.AddRange(folderNodeList);
                        foreach (var n in sortList)
                        {
                            ProcessTreeNode(n);
                        }

                    }
                    else
                    {
                        foreach (var n in node.ChildNodes)
                        {
                            ProcessTreeNode(n);
                        }
                    }
                }
            }
        }

        protected virtual void OnIndexItemProceed(IndexItemProceedEventArgs args)
        {
            var temp = indexItemProceed;
            if (temp != null) temp(this, args);
        }

        private EndUserTreeNode AddParentNode(EndUserTreeNode currentNode)
        {
            var currentIndex = this.RestoreIndexService.GetCurrentIndex(currentNode.NodeMd5Value);
            if (!currentIndex.Type.EqualsIgnoreCase("E"))
            {
                var parentNode = new EndUserTreeNode();
                if (parentNode.ChildNodes == null)
                {
                    parentNode.ChildNodes = new List<EndUserTreeNode>();
                }
                parentNode.ChildNodes.Add(currentNode);
                var parentIndex = this.RestoreIndexService.GetParentIndex(currentNode.NodeMd5Value);
                var position = parentIndex.Name.Contains("\\") ? parentIndex.Name.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) : parentIndex.Name.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
                var tempName = parentIndex.Name.Substring(position + 1);
                parentNode.Level = parentIndex.Type.ToNodeLevelByMediaDataTypeString().ToString().ToEnum<TreeNodeLevel>();
                parentNode.Name = AveConverter.DecodeSpecialChar(tempName);
                parentNode.NodeMd5Value = parentIndex.PathMD5;
                parentNode.Index = parentIndex;
                this.AddParentNode(parentNode);
                currentNode.ParentNode = parentNode;
                if (currentIndex.Type.EqualsIgnoreCase("W") && parentNode.Url.Contains("\\"))
                    currentNode.Url = parentNode.Url + "/" + currentNode.Name;
                else currentNode.Url = parentNode.Url + "\\" + currentNode.Name;
                this.logger.Debug(MediaServiceArchiverBackupResource.EndUserArchiverBrowserServiceAddParentNodeUrl, currentNode.Url);
                return parentNode;
            }
            else
            {
                currentNode.Url = currentIndex.Name;
                return currentNode;
            }
        }

        /*private int GetItemLevel(EndUserTreeNode node)
        {
            if (node.Index.Type.EqualsIgnoreCase("F"))
            {
                return 2;
            }
            else if (node.Index.Type.EqualsIgnoreCase("D"))
            {
                return 5;
            }
            else if (node.Index.Type.EqualsIgnoreCase("I"))
            {
                return 4;
            }
            else if (node.Index.Type.EqualsIgnoreCase("A"))
            {
                return 3;
            }
            else
            {
                return 1;
            }
        }*/

        private List<EndUserTreeNode> SortItems(List<EndUserTreeNode> items)
        {
            items.Sort((x, y) =>
            {
                int result = string.Compare(x.Index.ItemName, y.Index.ItemName, StringComparison.OrdinalIgnoreCase);
                if (result == 0)
                {
                    if (x.Index.ItemMajorVersion < y.Index.ItemMajorVersion)
                        result = -1;
                    else if (x.Index.ItemMajorVersion > y.Index.ItemMajorVersion)
                        result = 1;
                    else if (Math.Abs(x.Index.ItemMajorVersion - y.Index.ItemMajorVersion) < 1E-06)
                    {
                        if (x.Index.ItemMinorVersion < y.Index.ItemMinorVersion)
                            result = -1;
                        else if (x.Index.ItemMinorVersion > y.Index.ItemMinorVersion)
                            result = 1;
                        else
                        {
                            if (string.Compare(x.Index.Type, y.Index.Type, StringComparison.OrdinalIgnoreCase) > 0)
                                result = -1;
                            else
                                result = 0;
                        }
                    }
                }
                return result;
            });
            return items;
        }
    }
}
