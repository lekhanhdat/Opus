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
using System.Runtime.Serialization;
using AvePoint.RA.Contract.RMWeb.Tree.Base;

namespace AvePoint.RA.Contract.RMWeb.Tree
{
    [DataContract]
    public class RMPhysicalExplorerNode : Node<RMPhysicalExplorerNode>
    {
        [DataMember]
        public string LocationId { get; set; }
        [DataMember]
        public string LocationName { get; set; }
        [DataMember]
        public string BoxId { get; set; }
        [DataMember]
        public string FileId { get; set; }
        [DataMember]
        public bool HasNextPage { get; set; }
        [DataMember]
        public string PagePosition { get; set; }
        [DataMember]
        public int LeafNodeType { get; set; }
        [DataMember]
        public int RecordStatus { get; set; }
        /// <summary>
        /// 标记Tree节点上Box和Folder的Hold状态
        /// </summary>
        [DataMember]
        public bool IsHoldStatus { get; set; }
        [DataMember]
        public bool Checked { get; set; }
        [DataMember]
        public bool BreakInheritance { get; set; }
        [DataMember]
        public bool OnLoan { get; set; }

        /// <summary>
        /// template id path, start from suite to current
        /// </summary>
        [DataMember]
        public string TemplateIdPath { get; set; }
        [DataMember]
        public int TemplateId { get; set; }

        [DataMember]
        public string SearchKey { get; set; }

        [DataMember]
        public bool IsSearch { get; set; }

        [DataMember]
        public bool Expanded { get; set; }

        [DataMember]
        public bool CanSearch { get; set; }
        [DataMember]
        public bool IsSearchFolder { get; set; }
        [DataMember]
        public bool IsGlobalSearch { get; set; }
    }

    public class TermTreeViewDto 
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string TermId { get; set; }
    }

    [DataContract]
    public class RMPhysicalExplorerTermViewNode : Node<RMPhysicalExplorerTermViewNode>
    {
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public Guid TermId { get; set; }

        //Copy From RMPhysicalExplorerNode
        [DataMember]
        public string LocationId { get; set; }
        [DataMember]
        public string LocationName { get; set; }
        [DataMember]
        public string BoxId { get; set; }
        [DataMember]
        public string FileId { get; set; }
        [DataMember]
        public bool HasNextPage { get; set; }
        [DataMember]
        public string PagePosition { get; set; }
        [DataMember]
        public int LeafNodeType { get; set; }
        [DataMember]
        public int RecordStatus { get; set; }
        [DataMember]
        /// <summary>
        /// 标记Tree节点上Box和Folder的Hold状态
        /// </summary>
        public bool IsHoldStatus { get; set; }
        [DataMember]
        public bool Checked { get; set; }
        [DataMember]
        public bool BreakInheritance { get; set; }
        [DataMember]
        public bool OnLoan { get; set; }
        [DataMember]
        /// <summary>
        /// template id path, start from suite to current
        /// </summary>
        public string TemplateIdPath { get; set; }
        [DataMember]
        /// <summary>
        /// currently used for physical records, it includes the id list start from bottom location to parent node
        /// </summary>
        public List<Guid> Ancestors { get; set; }
    }
}
