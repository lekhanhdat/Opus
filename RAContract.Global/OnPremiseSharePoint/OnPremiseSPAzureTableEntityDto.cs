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
using AvePoint.RA.Contract.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.OnPremiseSharePoint
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class OnPremiseSPAzureTableEntityDto
    {
        [DataMember(EmitDefaultValue = false)]
        public Guid Id { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public Guid NodeID { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public Guid ParentID { set; get; } 
        [DataMember(EmitDefaultValue = false)]
        public Guid RuleID { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public int RuleAction { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsManualRule { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string JobID { set; get; } 
        [DataMember(EmitDefaultValue = false)]
        public Guid ScopeID { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string ScopePath { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public int Status { set; get; } 
        [DataMember(EmitDefaultValue = false)]
        public bool MovedToApprovalTable { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int UIVersion { set; get; } 
        [DataMember(EmitDefaultValue = false)]
        public int ArchiveLevel { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public int CacheNodeType { set; get; } 
        [DataMember(EmitDefaultValue = false)]
        public string JsonMeta { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public int SourceFlag { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string SortTicks { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public int HasRelatedDocument { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int DeleteRelatedRecords { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string RelatedRecordInfo { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public long LastModifiedTime { set; get; } 
        [DataMember(EmitDefaultValue = false)]
        public string LeafName { set; get; } 
        [DataMember(EmitDefaultValue = false)]
        public int Level { set; get; } 
        [DataMember(EmitDefaultValue = false)]
        public DateTime ExpireTime { set; get; } 
        [DataMember(EmitDefaultValue = false)]
        public int LibRowID { set; get; } 
        [DataMember(EmitDefaultValue = false)]
        public Guid ListId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public int NodeType { set; get; } 
        [DataMember(EmitDefaultValue = false)]
        public string Path { set; get; } 
        [DataMember(EmitDefaultValue = false)]
        public string Property { set; get; } 
        [DataMember(EmitDefaultValue = false)]
        public int SPNodeLevel { set; get; } 
        [DataMember(EmitDefaultValue = false)]
        public long ScanItemID { set; get; } 
        [DataMember(EmitDefaultValue = false)]
        public DateTime ScanTime { set; get; } 
        [DataMember(EmitDefaultValue = false)]
        public string SiteUrl { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string SiteId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string RegistedSiteId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public Guid WebId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string Metadata { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public DateTime ArchivedTime { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public Guid SiteGroupId { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public int KeepDataStatus { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string SiteTitle { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public int ExplorerStatus { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsRejectData { set; get; }
    }
}
