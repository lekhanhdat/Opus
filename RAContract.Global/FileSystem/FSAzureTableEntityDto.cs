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
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.FileSystem
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FSAzureTableEntityDto
    {
        [DataMember(EmitDefaultValue = false)]
        public Guid ConnectionId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Guid ScopeID { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Guid CurrentSettingId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Guid FilePathMd5 { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string HighName { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string LowName { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public Guid ParentID { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public int NodeLevel { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public DateTime AchiveTime { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public DateTime ScanTime { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public string RuleId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public DateTime CreateTime { set; get; }
        [DataMember(EmitDefaultValue = false)]
        public DateTime LastModifiedTme { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int KeepDataOption { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool DisposalAction { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool MovedToApprovalTable { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Property { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int Status { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public long SortTicks { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int RuleAction { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string RelatedRecordInfo { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string FullPath { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public long Size { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int HasRelatedDocument { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Guid TermId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string TermName { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int RecordStatus { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public long DestroyedTime { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool HoldStatus { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public long HoldReleaseTime { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int HoldType { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string HoldBy { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string HoldId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string HoldByUsers { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string HoldUntilTimes { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string[] AppendHolds_Array { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public bool NoNeedSendReport { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int InternalApprovedStatus { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int ManualApprovedBy { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public int ManualEscalateFrom { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Guid InternalConnectionId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public long Depth { get; set; } // record depth in the file system
        public int CreateDate { get; set; } // record create date
    }
    public class FSAzureTableEntityDtoWithJobId
    {
        [DataMember(EmitDefaultValue = false)]
        public List<FSAzureTableEntityDto> EntityDtos { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string JobId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsFSHighPerformanceMode { get; set; }
    }
}
