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




namespace AvePoint.Media.Service.DomainModel
{
    #region using directives

    using System;
    using System.Collections.Generic;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Tree;
    using AvePoint.GCommon.Contract.Tree.Object;
    using AvePoint.RA.Contract.Object;
    using global::Media.Common;

    #endregion using directives

    public class TreeNodeConverter
        : ITreeNodeConverter
    {
        public SPTreeNodeDto ConvertTreeNodeToSPTreeNode(TreeNode treeNode, NodeLevel level, Boolean ignoreChildren = default(Boolean))
        {
            var spTreeNode = new SPTreeNodeDto()
            {
                ID = treeNode.ID ?? Guid.NewGuid().ToString(),
                Name = treeNode.Name,
                Url = treeNode.Url,
                DisplayName = treeNode.DisplayName,
                Title = treeNode.Title,
                FarmName = treeNode.FarmName,
                FarmID = treeNode.FarmId,
                Description = treeNode.Description,
                Level = EnumConverter.ToEnum<NodeLevel>(treeNode.TreeNodeLevel.ToString()),
                Type = EnumConverter.ToEnum<NodeType>(treeNode.Type.ToString()),
                FullPath = treeNode.FullPath,
                SPObjectId = treeNode.SPObjectId,
                Expanded = treeNode.Expanded,
                SitePath=treeNode.SitePath,
                ChildrenLoaded = treeNode.ChildrenLoaded,
                CanChildrenBeLoaded = treeNode.CanChildrenBeLoaded,
                ChildrenCount = treeNode.Children == null ? 0 : treeNode.Children.Count,
                Parent = treeNode.Parent == null ? null : ConvertTreeNodeToSPTreeNode(treeNode.Parent, level, true),
                SPType = treeNode.SPType,
                SPVersion = treeNode.SPVersion,
                NodeExtension = new NodeExtensionDto()
                {
                    SolutionDetail = new SolutionDetailDTO()
                    {
                        solutionId = new Guid(treeNode.SolutionId ?? new Guid().ToString()),
                    },
                    BackupTime = treeNode.BackupTime,
                    SelectorHidden = treeNode.SelectorHidden,
                    IsAdvancedSearchResult = treeNode.IsAdvancedSearchResult,
                    IsVNode = treeNode.IsObjectSearchResult,
                    SelectorEnable = treeNode.SelectorEnable
                },
                Size = treeNode.Size,
                Extension = treeNode.Extension,
            };
            if (!ignoreChildren)
            {
                if (treeNode.Children != null && treeNode.Children.Count > 0 && !String.IsNullOrEmpty(treeNode.Children[0].HVName))
                {
                    spTreeNode.NodeExtension.HistoryVersions = this.ConvertTreeNodeListToHistoryVersionList(treeNode.Children);
                }
                else if (treeNode.Children != null && treeNode.Children.Count > 0)
                {
                    if (spTreeNode.Level == level)
                    {
                        if (!treeNode.IsSelectNode)
                        {
                            spTreeNode.Children.AddRange(ConvertTreeNodeListToSPTreeNodeList(treeNode.Children, level));
                            return spTreeNode;
                        }
                        spTreeNode.CheckNumber = 1;
                        spTreeNode.SelectAll = SelectAllState.Checked;
                        spTreeNode.Property = PropertyState.Checked;
                        spTreeNode.Security = SecurityState.Checked;
                        spTreeNode.IncludeNew = IncludeNewState.Checked;
                    }
                    spTreeNode.Children.AddRange(ConvertTreeNodeListToSPTreeNodeList(treeNode.Children,level));
                }
                else
                {
                    spTreeNode.CheckNumber = 1;
                    spTreeNode.SelectAll = SelectAllState.Checked;
                    spTreeNode.Property = PropertyState.Checked;
                    spTreeNode.Security = SecurityState.Checked;
                    spTreeNode.IncludeNew = IncludeNewState.Checked;
                }
            }
            return spTreeNode;
        }
        private NodeLevel ConvertGDriveNodeLevel(TreeNodeLevel tLevel)
        {
            return tLevel switch
            {
                TreeNodeLevel.GoogleMyDrive => NodeLevel.GoogleMyDrive,
                TreeNodeLevel.GoogleSharedDrive => NodeLevel.GoogleSharedDrive,
                TreeNodeLevel.GoogleDriveFolder => NodeLevel.GoogleFolder,
                TreeNodeLevel.GoogleDriveFile => NodeLevel.GoogleFile,
                _ => NodeLevel.GoogleFile,
            };
        }
        public GoogleDriveTreeNodeDto ConvertTreeNodeToGDriveTreeNode(TreeNode node, NodeLevel level, Boolean ignoreChildren = default(Boolean))
        {
            GoogleDriveTreeNodeDto treeNodeDto =  new()
            {
                ID = node.PathMD5,
                Name = node.Title ?? "",
                Title = node.Title ?? "",
                FullPath = node.FullPath ?? "",
                Level = ConvertGDriveNodeLevel(node.TreeNodeLevel),
                DisplayName = node.DisplayName,
                Expanded = node.Expanded,
                ChildrenCount = node.Children == null ? 0 : node.Children.Count,
                Parent = node.Parent != null ? ConvertTreeNodeToGDriveTreeNode(node.Parent, level, true) : null,
                Children = node.Children?.ConvertAll(x => ConvertTreeNodeToGDriveTreeNode(x, level)),
                ParentId = node.ParentPathMD5,
                NodeId = node.PathMD5,
                ObjectId = node.ID,
                TenantId = node.FarmId,
                PathMD5 = node.PathMD5,
            };
            if (!ignoreChildren)
            {
                if (node.Children != null && node.Children.Count > 0)
                {
                    if (treeNodeDto.Level == level)
                    {
                        treeNodeDto.CheckNumber = 1;
                        treeNodeDto.SelectAll = SelectAllState.Checked;
                        treeNodeDto.Property = PropertyState.Checked;
                        treeNodeDto.Security = SecurityState.Checked;
                        treeNodeDto.IncludeNew = IncludeNewState.Checked;
                    }
                }
                else
                {
                    treeNodeDto.CheckNumber = 1;
                    treeNodeDto.SelectAll = SelectAllState.Checked;
                    treeNodeDto.Property = PropertyState.Checked;
                    treeNodeDto.Security = SecurityState.Checked;
                    treeNodeDto.IncludeNew = IncludeNewState.Checked;
                }
            }
            return treeNodeDto;
        }
        public List<SPTreeNodeDto> ConvertTreeNodeListToSPTreeNodeList(List<TreeNode> treeNodeList, NodeLevel level)
        {
            var spTreeNodeList = new List<SPTreeNodeDto>();
            if (treeNodeList != null)
            {
                foreach (var treeNode in treeNodeList)
                {
                    spTreeNodeList.Add(this.ConvertTreeNodeToSPTreeNode(treeNode, level));
                }
            }
            return spTreeNodeList;
        }

        public List<GoogleDriveTreeNodeDto> ConvertTreeNodeListToGDriveTreeNodeList(List<TreeNode> treeNodeList, NodeLevel level)
        {
            var gdTreeNodeList = new List<GoogleDriveTreeNodeDto>();
            if (treeNodeList != null)
            {
                foreach (var treeNode in treeNodeList)
                {
                    gdTreeNodeList.Add(this.ConvertTreeNodeToGDriveTreeNode(treeNode, level));
                }
            }
            return gdTreeNodeList;
        }
        public PRTreeNodeDto ConvertTreeNodeToPRTreeNode(TreeNode treeNode)
        {
            var prTreeNode = new PRTreeNodeDto()
            {
                Name = treeNode.Name,
                DisplayName = treeNode.DisplayName,
                Title = treeNode.Title,
                Level = EnumConverter.ToEnum<NodeLevel>(treeNode.TreeNodeLevel.ToString()),
                Type = EnumConverter.ToEnum<NodeType>(treeNode.Type.ToString()),
                FullPath = treeNode.FullPath,
                TypeId = EnumConverter.ToEnum<PRNodeTypeId>(treeNode.TypeId.ToString()),
                SPObjectId = new Guid(treeNode.SPObjectId),
                Location = treeNode.Location,
                BackupSelected = EnumConverter.ToEnum<PRSelectMode>(treeNode.BackupSelected.ToString()),
                CanSelectBackup = treeNode.CanSelectBackup,
                IsIndex = treeNode.IsIndex,
                NoAgentInstall = treeNode.NoAgentInstall,
                CanExpand = treeNode.CanExpand,
                ID = treeNode.ID,
                BeforeOperationSize = treeNode.BeforeOperationSize,
                BeforeOperationTime = treeNode.BeforeOperationTime,
            };
            return prTreeNode;
        }

        public List<PRTreeNodeDto> ConvertTreeNodeListToPRTreeNodeList(List<TreeNode> treeNodeList)
        {
            var prTreeNodeList = new List<PRTreeNodeDto>();
            foreach (var treeNode in treeNodeList)
            {
                prTreeNodeList.Add(this.ConvertTreeNodeToPRTreeNode(treeNode));
            }
            return prTreeNodeList;
        }

        public TreeNode ConvertSPTreeNodeToTreeNode(SPTreeNodeDto spTreeNode)
        {
            var treeNode = new TreeNode()
            {
                ID = spTreeNode.ID,
                Name = spTreeNode.Name,
                DisplayName = spTreeNode.DisplayName,
                Title = spTreeNode.Title,
                SolutionId = spTreeNode.NodeExtension.SolutionDetail == null ? null : spTreeNode.NodeExtension.SolutionDetail.solutionId.ToString(),
                FarmId = spTreeNode.FarmID,
                FarmName = spTreeNode.FarmName,
                Description = spTreeNode.Description,
                TreeNodeLevel = EnumConverter.ToEnum<TreeNodeLevel>(spTreeNode.Level.ToString()),
                FullPath = spTreeNode.FullPath,
                SPObjectId = spTreeNode.SPObjectId,
                Expanded = spTreeNode.Expanded,
                CanChildrenBeLoaded = spTreeNode.CanChildrenBeLoaded,
                ChildrenLoaded = spTreeNode.ChildrenLoaded,
                SelectorHidden = spTreeNode.NodeExtension.SelectorHidden,
                IsAdvancedSearchResult = spTreeNode.NodeExtension.IsAdvancedSearchResult,
                IsObjectSearchResult = spTreeNode.NodeExtension.IsVNode,
                BackupTime = spTreeNode.NodeExtension.BackupTime,
                Parent = spTreeNode.Parent == null ? null : ConvertSPTreeNodeToTreeNode(spTreeNode.Parent),
                SPType = spTreeNode.SPType,
                SPVersion = spTreeNode.SPVersion,
            };
            return treeNode;
        }

        private TreeNode ConvertEITreeNodeToTreeNode(EITreeNodeDto eiTreeNode)
        {
            var treeNode = new TreeNode()
            {
                ID = eiTreeNode.ID,
                Name = eiTreeNode.Name,
                DisplayName = eiTreeNode.DisplayName,
                Title = eiTreeNode.Title,
                FarmId = eiTreeNode.FarmID,
                FarmName = eiTreeNode.FarmName,
                StructureLevel = EnumConverter.ToEnum<DataProtectionStructureLevel>(eiTreeNode.Level.ToString()),
                FullPath = eiTreeNode.FullPath,
                Expanded = eiTreeNode.Expanded,
                CanChildrenBeLoaded = eiTreeNode.CanChildrenBeLoaded,
                ChildrenLoaded = eiTreeNode.ChildrenLoaded,
                SelectorHidden = eiTreeNode.NodeExtension.SelectorHidden,
                IsAdvancedSearchResult = eiTreeNode.NodeExtension.IsAdvancedSearchResult,
                IsObjectSearchResult = eiTreeNode.NodeExtension.IsVNode,
                BackupTime = eiTreeNode.NodeExtension.BackupTime,

                //Parent = eiTreeNode.Parent == null ? null : ConvertEITreeNodeToTreeNode(eiTreeNode.Parent),
            };
            return treeNode;
        }

        public List<TreeNode> ConvertEITreeNodeListToTreeNodeList(List<EITreeNodeDto> eiTreeNodeList)
        {
            var treeNodeList = new List<TreeNode>();
            eiTreeNodeList.ForEach(eiTreeNode => { treeNodeList.Add(this.ConvertEITreeNodeToTreeNode(eiTreeNode)); });
            return treeNodeList;
        }

        public List<HistoryVersion> ConvertTreeNodeListToHistoryVersionList(List<TreeNode> treeNodeList)
        {
            var historyVersionList = treeNodeList.ConvertAll(treeNode => new HistoryVersion()
                {
                    Name = treeNode.HVName,
                    CreateTime = treeNode.HVCreateTime,
                    Description = treeNode.HVDescription,
                    Type = EnumConverter.ToEnum<SolutionType>(treeNode.HVType.ToString()),
                });
            return historyVersionList;
        }

        public List<ExchangeOnlineTreeNodeDto> ConvertTreeNodeListToExchangeTreeNodeList(List<TreeNode> treeNodeList)
        {
            var exchangeTreeNodeList = new List<ExchangeOnlineTreeNodeDto>();
            if (treeNodeList != null)
            {
                foreach (var treeNode in treeNodeList)
                {
                    exchangeTreeNodeList.Add(this.ConvertTreeNodeToExchangeTreeNode(treeNode));
                }
            }
            return exchangeTreeNodeList;
        }

        private ExchangeOnlineTreeNodeDto ConvertTreeNodeToExchangeTreeNode(TreeNode treeNode, Boolean ignoreChildren = default(Boolean))
        {
            var exchangeTreeNode = new ExchangeOnlineTreeNodeDto()
            {
                ID = treeNode.ID ?? Guid.NewGuid().ToString(),
                Name = treeNode.Name,
                DisplayName = treeNode.DisplayName,
                Title = treeNode.Title,
                EmailAddress = treeNode.MailAddress,
                //Level = treeNode.TreeNodeLevel.ToString().ToEnum<NodeLevel>(),
                Level = EnumConverter.ToEnum<NodeLevel>(treeNode.TreeNodeLevel.ToString()),
                Type = EnumConverter.ToEnum<NodeType>(treeNode.Type.ToString()),
                FullPath = treeNode.FullPath,
                Expanded = treeNode.Expanded,
                Sender = treeNode.Sender,
                DisplayTo = treeNode.DisplayTo,
                SendDate = treeNode.SendDate,
                HasAttachment = treeNode.HasAttachment,
                Category = treeNode.Category,
                ChildrenLoaded = treeNode.ChildrenLoaded,
                CanChildrenBeLoaded = treeNode.CanChildrenBeLoaded,
                ChildrenCount = treeNode.Children == null ? 0 : treeNode.Children.Count,
                Parent = treeNode.Parent == null ? null : ConvertTreeNodeToExchangeTreeNode(treeNode.Parent, true),
            };
            exchangeTreeNode.NodeExtension.BackupTime = treeNode.BackupTime;
            exchangeTreeNode.NodeExtension.IsAdvancedSearchResult = treeNode.IsAdvancedSearchResult;
            if (!ignoreChildren)
            {
                exchangeTreeNode.Children.AddRange(ConvertTreeNodeListToExchangeTreeNodeList(treeNode.Children));
            }
            return exchangeTreeNode;
        }

        public TreeNode ConvertExchangeOnlineTreeNodeToTreeNode(ExchangeOnlineTreeNodeDto exchangeTreeNode)
        {
            var treeNode = new TreeNode()
            {
                ID = exchangeTreeNode.ID,
                Name = exchangeTreeNode.Name,
                DisplayName = exchangeTreeNode.DisplayName,
                Title = exchangeTreeNode.Title,
                MailAddress = exchangeTreeNode.EmailAddress,
                SolutionId = exchangeTreeNode.NodeExtension.SolutionDetail == null ? null : exchangeTreeNode.NodeExtension.SolutionDetail.solutionId.ToString(),
                TreeNodeLevel = EnumConverter.ToEnum<TreeNodeLevel>(exchangeTreeNode.Level.ToString()),
                FullPath = exchangeTreeNode.FullPath,
                Expanded = exchangeTreeNode.Expanded,
                CanChildrenBeLoaded = exchangeTreeNode.CanChildrenBeLoaded,
                ChildrenLoaded = exchangeTreeNode.ChildrenLoaded,
                SelectorHidden = exchangeTreeNode.NodeExtension.SelectorHidden,
                IsAdvancedSearchResult = exchangeTreeNode.NodeExtension.IsAdvancedSearchResult,
                IsObjectSearchResult = exchangeTreeNode.NodeExtension.IsVNode,
                BackupTime = exchangeTreeNode.NodeExtension.BackupTime,
                Type = EnumConverter.ToEnum<TreeNodeType>(exchangeTreeNode.Type.ToString()),
                Parent = exchangeTreeNode.Parent == null ? null : ConvertExchangeOnlineTreeNodeToTreeNode(exchangeTreeNode.Parent),
            };
            return treeNode;
        }

        public List<SPTreeNodeDto> ConvertTreeNodeListToTeamsTreeNodeList(List<TreeNode> treeNodeList, NodeLevel level)
        {
            var spTreeNodeList = new List<SPTreeNodeDto>();
            if (treeNodeList != null)
            {
                foreach (var treeNode in treeNodeList)
                {
                    spTreeNodeList.Add(this.ConvertTreeNodeToTeamsTreeNode(treeNode, level));
                }
            }
            return spTreeNodeList;
        }

        public SPTreeNodeDto ConvertTreeNodeToTeamsTreeNode(TreeNode treeNode, NodeLevel level, Boolean ignoreChildren = default(Boolean))
        {
            var spTreeNode = new SPTreeNodeDto()
            {
                ID = treeNode.ID ?? treeNode.SPObjectId ?? Guid.NewGuid().ToString(),
                Name = treeNode.Name,
                Url = treeNode.Url,
                DisplayName = treeNode.DisplayName,
                Title = treeNode.Title,
                FarmName = treeNode.FarmName,
                FarmID = treeNode.FarmId,
                Description = treeNode.Description,
                Level = level,
                Type = ConvertTeamsNodeType(treeNode.Type),
                FullPath = treeNode.FullPath,
                SPObjectId = treeNode.SPObjectId,
                TeamsId = treeNode.SPObjectId,
                TeamName = treeNode.Name,

                Expanded = treeNode.Expanded,
                SitePath = treeNode.SitePath,
                ChildrenLoaded = treeNode.ChildrenLoaded,
                CanChildrenBeLoaded = treeNode.CanChildrenBeLoaded,
                ChildrenCount = treeNode.Children == null ? 0 : treeNode.Children.Count,
                Parent = treeNode.Parent == null ? null : ConvertTreeNodeToSPTreeNode(treeNode.Parent, level, true),
                SPType = treeNode.SPType,
                SPVersion = treeNode.SPVersion,
                NodeExtension = new NodeExtensionDto()
                {
                    SolutionDetail = new SolutionDetailDTO()
                    {
                        solutionId = new Guid(treeNode.SolutionId ?? new Guid().ToString()),
                    },
                    BackupTime = treeNode.BackupTime,
                    SelectorHidden = treeNode.SelectorHidden,
                    IsAdvancedSearchResult = treeNode.IsAdvancedSearchResult,
                    IsVNode = treeNode.IsObjectSearchResult,
                    SelectorEnable = treeNode.SelectorEnable
                },
            };
            //if (!ignoreChildren)
            //{
            //    if (treeNode.Children != null && treeNode.Children.Count > 0 && !String.IsNullOrEmpty(treeNode.Children[0].HVName))
            //    {
            //        spTreeNode.NodeExtension.HistoryVersions = this.ConvertTreeNodeListToHistoryVersionList(treeNode.Children);
            //    }
            //    else if (treeNode.Children != null && treeNode.Children.Count > 0)
            //    {
            //        if (spTreeNode.Level == level)
            //        {
            //            if (!treeNode.IsSelectNode)
            //            {
            //                spTreeNode.Children.AddRange(ConvertTreeNodeListToSPTreeNodeList(treeNode.Children, level));
            //                return spTreeNode;
            //            }
            //            spTreeNode.CheckNumber = 1;
            //            spTreeNode.SelectAll = SelectAllState.Checked;
            //            spTreeNode.Property = PropertyState.Checked;
            //            spTreeNode.Security = SecurityState.Checked;
            //            spTreeNode.IncludeNew = IncludeNewState.Checked;
            //        }
            //        spTreeNode.Children.AddRange(ConvertTreeNodeListToSPTreeNodeList(treeNode.Children, level));
            //    }
            //    else
            //    {
            //        spTreeNode.CheckNumber = 1;
            //        spTreeNode.SelectAll = SelectAllState.Checked;
            //        spTreeNode.Property = PropertyState.Checked;
            //        spTreeNode.Security = SecurityState.Checked;
            //        spTreeNode.IncludeNew = IncludeNewState.Checked;
            //    }
            //}

            // currently only need Teams level node, no need check children
            spTreeNode.CheckNumber = 1;
            spTreeNode.SelectAll = SelectAllState.Checked;
            spTreeNode.Property = PropertyState.Checked;
            spTreeNode.Security = SecurityState.Checked;
            spTreeNode.IncludeNew = IncludeNewState.Checked;

            return spTreeNode;
        }

        private NodeType ConvertTeamsNodeType(TreeNodeType treeNodeType)
        {
            return treeNodeType switch
            {
                TreeNodeType.EOMailBox => NodeType.O365TeamSites,
                TreeNodeType.EOO365Group => NodeType.O365GroupSites,
                // .... channel , planner,...
                _ => NodeType.Office365GroupEntire,
            };
        }
    }
}