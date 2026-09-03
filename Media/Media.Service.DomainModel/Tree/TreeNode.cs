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
    using System.Text;
    using AvePoint.GCommon.Contract.Tree.Object;
    using global::Media.Common;

    #endregion

    public class TreeNode
    {
        public String Url { get; set; }
        public String Name { get; set; }
        public String Title { get; set; }
        public String DisplayName { get; set; }
        public String Description { get; set; }
        public String FullPath { get; set; }
        public int Depth { get; set; }
        public String FarmName { get; set; }
        public String FarmId { get; set; }
        public String SolutionId { get; set; }
        public TreeNodeLevel TreeNodeLevel { get; set; }
        public DataProtectionStructureLevel StructureLevel { get; set; }
        public TreeNodeType Type { get; set; }
        public Int64 BackupTime { get; set; }
        public Boolean Expanded { get; set; }
        public Boolean CanChildrenBeLoaded { get; set; }
        public Boolean ChildrenLoaded { get; set; }
        public Boolean SelectorHidden { get; set; }
        public Boolean IsAdvancedSearchResult { get; set; }
        public Boolean IsObjectSearchResult { get; set; }
        public Boolean SelectorEnable { get; set; }
        public String DeviceName { get; set; }

        public TreeNodeLevel TypeId { get; set; }
        public String SPObjectId { get; set; }
        public String Id { get; set; }
        public String NodeGuid { get; set; }
        public String Location { get; set; }
        public TreeSelectedMode BackupSelected { get; set; }
        public Boolean CanSelectBackup { get; set; }
        public Boolean IsIndex { get; set; }
        public Boolean NoAgentInstall { get; set; }
        public Boolean CanExpand { get; set; }
        public String ID { get; set; }
        public Int64 BeforeOperationSize { get; set; }
        public DateTime BeforeOperationTime { get; set; }

        public TreeNode Parent { get; set; }
        public List<TreeNode> Children { get; set; }
        public String HVName { get; set; }
        public Int64 HVCreateTime { get; set; }
        public String HVDescription { get; set; }
        public TreeNodeLevel HVType { get; set; }
        public SPType SPType { get; set; }
        public Int32 SPVersion { get; set; }
        public BrowseItemDetails ItemDetails { get; set; }

        public String Sender { get; set; }
        public String DisplayTo { get; set; }
        public Int64 SendDate { get; set; }
        public Boolean HasAttachment { get; set; }
        public String Category { get; set; }
        public String MailAddress { get; set; }
        public long CreatedTime { get; set; }
        public long ModifiedTime { get; set; }
        public long ArchivedTime { get; set; }
        public string PathMD5 { get; set; }
        public string ParentPathMD5 { get; set; }
        public string StoragePolicyId { get; set; }
        public int AccessTierType { get; set; }
        public string ModifiedBy { get; set; }
        public string Author { get; set; }
        public string JobId { get; set; }
        public string SitePath { get; set; }
        public string TypeInIndex { get; set; }
        public bool IsSelectNode { get; set; }
        public string FullPathForUI { get; set; }
        public int Count { get; set; }
        public bool IsArchiveTier { get; set; }
        public long ContentLenth { get; set; }
        public bool IsSoftDeleted { get; set; }
        public long TotalCount { get; set; }
        public string Extension { get; set; }
        public long Size { get; set; }
        public TreeNode()
        {
            this.Children = new List<TreeNode>();
        }

        public EITreeNodeDto ToEITreeNode(String logicalDeviceId)
        {
            return new EITreeNodeDto
            {
                Name = this.Name,
                DisplayName = this.DisplayName,
                FarmName = this.FarmName,
                FullPath = this.FullPath,
                Level = Convert.ToInt32(this.StructureLevel) == 0 ? NodeLevel.Undefined : EnumConverter.ToEnum<NodeLevel>(this.StructureLevel.ToString()),
                DeviceName = this.DeviceName,
                LogicalDeviceIds = new List<String> { logicalDeviceId }
            };
        }

        public VaultTreeNodeDto ToVaultTreeNode()
        {
            return new VaultTreeNodeDto
            {
                Name = this.Name,
                DisplayName = this.DisplayName,
                Title = this.Title,
                Description = this.Description
            };
        }

        public override string ToString()
        {
            return TextNode("", true);
        }

        /// <summary>
        /// 转换Tree型结构Text
        /// </summary>
        /// <param name="prefix">用于此节点之前的\t和连线</param>
        /// <param name="isLastChild">此节点是否是该层最后一个节点</param>
        /// <returns></returns>
        private string TextNode(string prefix, bool isLastChild)
        {
            StringBuilder textBuilder = new StringBuilder();
            textBuilder.Append(prefix + (isLastChild ? "└" : "├") + Name + "-" + new DateTime(BackupTime).ToString() + "\r\n");
            if (Children != null)
            {
                for (int i = 0; i < Children.Count - 1; i++)
                {
                    if (isLastChild)
                    {
                        textBuilder.Append(Children[i].TextNode(prefix + "" + "\t", false));
                    }
                    else
                    {
                        textBuilder.Append(Children[i].TextNode(prefix + "│" + "\t", false));
                    }
                }
                if (Children.Count > 0)
                {
                    if (isLastChild)
                    {
                        textBuilder.Append(Children[Children.Count - 1].TextNode(prefix + "" + "\t", true));
                    }
                    else
                    {
                        textBuilder.Append(Children[Children.Count - 1].TextNode(prefix + "│" + "\t", true));
                    }
                }
            }
            return textBuilder.ToString();
        }
    }
}