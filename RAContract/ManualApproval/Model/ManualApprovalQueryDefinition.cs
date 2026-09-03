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
using AvePoint.RA.Contract.ManualApproval.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.ManualApproval.Model
{
    [DataContract]
    public class ManualApprovalQueryDefinition
    {
        [DataMember]
        public int PageSize { get; set; }
        [DataMember]
        public bool NeedCalculationCount { get; set; }
        [DataMember]
        public string Continuation { get; set; } = null;
        [DataMember]
        public List<ManualApprovalFilterDefinition> Filters { get; set; } = new List<ManualApprovalFilterDefinition>();
        [DataMember]
        public ManualApprovalOrderOptions OrderBy { get; set; } = ManualApprovalOrderOptions.None;
        [DataMember]
        public string CustomColumnId { get; set; } = string.Empty;
        [DataMember]
        public bool IsDesc { get; set; } = false;
        [DataMember]
        public ManualApprovalTab ManualApprovalTab { get; set; }
        [DataMember]
        public List<ManualApprovalFolderInfo> FolderInfos { get; set; }
        [DataMember]
        public string ManualSiteUrl { get; set; }
        [DataMember]
        public bool IsEnableFolderView { get; set; }
        [DataMember]
        public string TimeFormat { get; set; } = "yyyy-MM-dd";
        [DataMember]
        public bool FromGControl { get; set; }
        [DataMember]
        public bool IsJpmc { get; set; }
        [DataMember]
        public string PartitionKeyId { get; set; }
    }

    public class ManualApprovalFolderInfo
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string NodeId { get; set; }
        [DataMember]
        public string LeafName { get; set; }
        [DataMember]
        public int NodeType { get; set; }
        [DataMember]
        public string ManualSiteUrl { get; set; }
        [DataMember]
        public string ManualFullPath { get; set; }
        [DataMember]
        public string ParentId { get; set; }
    }
}
