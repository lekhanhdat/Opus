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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.ManualApproval.Model
{
    [DataContract]
    public enum ManualApprovalOrderOptions
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        LeafName = 1,
        [EnumMember]
        ApprovalStatus = 2,
        [EnumMember]
        ModifiedBy = 3,
        [EnumMember]
        CreatedBy = 4,
        [EnumMember]
        CollectioinTime = 5,
        [EnumMember]
        RuleName = 6,
        [EnumMember]
        PredictTime = 7,
        [EnumMember]
        QuikcReason = 8,
        [EnumMember]
        ManualModifiedTime = 9,
        [EnumMember]
        MLModifiedTime = 10,
        [EnumMember]
        MLCreatedTime = 11,

        [EnumMember]
        CustomText = 12,
        [EnumMember]
        CustomYesOrNo = 13,
        [EnumMember]
        CustomDateTime = 14,
        [EnumMember]
        CustomNumber = 15,
        [EnumMember]
        DisposalDueDate = 16,
        [EnumMember]
        RecordsId = 17
    }
}
