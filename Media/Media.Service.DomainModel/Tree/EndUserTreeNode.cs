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
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.Media.Common;
    using global::Media.Common;
    using global::Media.Extension;
    #endregion

    public class EndUserTreeNode
    {
        public String Url { set; get; }
        public String Name { set; get; }
        public Boolean IsHold { set; get; }
        public String Attribute { set; get; }
        public String TimeZoneId { get; set; }
        public Int64 ArchiveTime { get; set; }
        public TreeNodeLevel Level { set; get; }
        public String NodeMd5Value { set; get; }
        public Boolean HasNextPage { get; set; }
        public Int64 FinalDisposition { set; get; } //Retention time
        public EndUserTreeNode ParentNode { set; get; }//下面一层，Item->SiteCollecton 保持顺序结构
        public List<EndUserTreeNode> ChildNodes { set; get; }
        public ArchiverBasicIndex Index { set; get; }

        public EndUserTreeNode()
        { }

        public EndUserArchiverViewNodeDto ToViewNodeDto()
        {
            return new EndUserArchiverViewNodeDto()
            {
                NodeType = EnumConverter.ToEnum<EndUserArchiverNodeType>(this.Level.ToString()),
                Name = this.Name,
                Url = this.Url,
                NodeMd5Value = this.NodeMd5Value,
                Attribute = this.Attribute,
                HasNextPage = this.HasNextPage,
                FinalDisposition = this.FinalDisposition,
                ArchiveTime = this.ArchiveTime,
                TimeZoneId = this.TimeZoneId,
                ParentNode = this.ParentNode == null ? null : this.ParentNode.ToParentViewNode()
            };
        }

        public EndUserTreeNode(ArchiverBasicIndex index)
        {
            this.Index = index;
            this.Level = EnumConverter.ToEnum<TreeNodeLevel>(index.Type.ToNodeLevelByMediaDataTypeString().ToString());
            var position = index.Name.Contains("\\") ? index.Name.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) : index.Name.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
            var tempName = index.Name.Substring(position + 1);
            this.Name = AveConverter.DecodeSpecialChar(tempName);
            this.NodeMd5Value = index.PathMD5;
            this.FinalDisposition = index.FinalDisposition;
            this.ArchiveTime = index.ArchiveTime;
            this.TimeZoneId = index.TimeZoneId;
            this.Attribute = index.Attributes?.Replace(ServiceConstants.ExtraChar.ToString(), Environment.NewLine).Replace(ServiceConstants.Delimiter.ToString(), ":");
        }

        public EndUserTreeNode(ArchiverBasicIndex index, String parentPath)
        {
            if (index.Type.EqualsIgnoreCase("D"))
                this.Level = TreeNodeLevel.Document;
            else if (index.Type.EqualsIgnoreCase("A"))
                this.Level = TreeNodeLevel.Attachment;
            else
                this.Level = EnumConverter.ToEnum<TreeNodeLevel>(index.Type.ToNodeLevelByMediaDataTypeString().ToString());
            var position = index.Name.Contains("\\") ? index.Name.LastIndexOf("\\", StringComparison.OrdinalIgnoreCase) : index.Name.LastIndexOf("/", StringComparison.OrdinalIgnoreCase);
            var tempName = index.Name.Substring(position + 1);
            this.Index = index;
            this.Name = AveConverter.DecodeSpecialChar(tempName);
            if (index.Type.Equals("W", StringComparison.OrdinalIgnoreCase) && parentPath.Contains("\\"))
                this.Url = parentPath + "/" + this.Name;
            else this.Url = parentPath + "\\" + this.Name;
            this.FinalDisposition = index.FinalDisposition;
            this.NodeMd5Value = index.PathMD5;
            this.ArchiveTime = index.ArchiveTime;
            this.TimeZoneId = index.TimeZoneId;
            this.Attribute = index.Attributes.Replace(ServiceConstants.ExtraChar.ToString(), Environment.NewLine).Replace(ServiceConstants.Delimiter.ToString(), ":");
        }

        public override String ToString()
        {
            return TextNode("", true);
        }

        /// <summary>
        /// 转换Tree型结构Text
        /// </summary>
        /// <param name="prefix">用于此节点之前的\t和连线</param>
        /// <param name="isLastChild">此节点是否是该层最后一个节点</param>
        /// <returns></returns>
        String TextNode(String prefix, Boolean isLastChild)
        {
            var textBuilder = new StringBuilder();
            textBuilder.Append(prefix + (isLastChild ? "└" : "├") + Name + "\r\n");
            if (ChildNodes != null)
            {
                for (Int32 i = 0; i < ChildNodes.Count - 1; i++)
                {
                    if (isLastChild)
                        textBuilder.Append(ChildNodes[i].TextNode(prefix + "" + "\t", false));
                    else
                        textBuilder.Append(ChildNodes[i].TextNode(prefix + "│" + "\t", false));
                }
                if (ChildNodes.Count > 0)
                {
                    if (isLastChild)
                        textBuilder.Append(ChildNodes[ChildNodes.Count - 1].TextNode(prefix + "" + "\t", true));
                    else
                        textBuilder.Append(ChildNodes[ChildNodes.Count - 1].TextNode(prefix + "│" + "\t", true));
                }
            }
            return textBuilder.ToString();
        }

        EndUserArchiverViewNodeDto ToParentViewNode()
        {
            var parentNode = new EndUserArchiverViewNodeDto();
            parentNode.Url = this.Url;
            parentNode.NodeMd5Value = this.NodeMd5Value;
            parentNode.Name = this.Name;
            parentNode.NodeType = EnumConverter.ToEnum<EndUserArchiverNodeType>(this.Level.ToString());
            parentNode.ArchiveTime = this.ArchiveTime;
            parentNode.TimeZoneId = this.TimeZoneId;
            parentNode.ParentNode = this.ParentNode == null ? null : this.ParentNode.ToParentViewNode();
            return parentNode;
        }
    }
}