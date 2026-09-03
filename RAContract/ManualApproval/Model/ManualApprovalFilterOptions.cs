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
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.ManualApproval.Model
{
    [DataContract]
    public enum ManualApprovalFilterOptions
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        CollectionTime = 1,
        [EnumMember]
        ActionTime = 2,
        [EnumMember]
        Source = 3,
        [EnumMember]
        ApprovalStatus = 4,
        [EnumMember]
        ModifiedBy = 5,
        [EnumMember]
        CreatedBy = 6,
        [EnumMember]
        RuleName = 7,
        [EnumMember]
        RuleDisposalClass = 8,
        [EnumMember]
        EscalatedFrom = 9,
        [EnumMember]
        Reviewer = 10,
        [EnumMember]
        ApprovedBy = 11,
        [EnumMember]
        LeafName = 12,
        [EnumMember]
        IsRelatedRecords = 13,
        [EnumMember]
        ExtendTime = 14,
        [EnumMember]
        ItemId = 15,
        [EnumMember]
        Workspace = 16,
        [EnumMember]
        MLReviewer = 17,
        [EnumMember]
        PredictTime = 18,
        [EnumMember]
        MLApprovalStatus = 19,
        [EnumMember]
        MLEscalatedFrom = 20,
        [EnumMember]
        MLWorkspace = 21,
        [EnumMember]
        FolderPath = 22,
        [EnumMember]
        QuikcReason = 23,
        [EnumMember]
        Permission = 24,
        [EnumMember]
        ManualModifiedTime = 25,
        [EnumMember]
        MLModifiedTime = 26,
        [EnumMember]
        MLCreatedTime = 27,
        [EnumMember]
        MLPredictTermId = 28,

        [EnumMember]
        CustomText = 29,        
        [EnumMember]
        CustomYesOrNo = 30,
        [EnumMember]
        CustomDateTime = 31,
        [EnumMember]
        CustomNumber = 32,
        [EnumMember]
        DisposalDueDate = 33,
        [EnumMember]
        GControlApprovalStatus = 34,
        [EnumMember]
        GControlReviewer = 35,
        [EnumMember]
        GControlTaskId = 36,
        [EnumMember]
        JpmcConnectionId = 38,
        [EnumMember]
        JpmcModifiedBy = 39,
        [EnumMember]
        JpmcCreatedBy = 40,
		[EnumMember]
        MyhubFolderNodeId=41,
        [EnumMember]
        SourceNoFs = 42,
    }
}
