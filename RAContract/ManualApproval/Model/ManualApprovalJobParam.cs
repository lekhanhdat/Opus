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
using AvePoint.RA.Contract.ManualApproval.Model;
using System.Runtime.Serialization;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System;
using AvePoint.RA.Contract.ControlPlus;

namespace AvePoint.RA.Service.Services.ManualApproval.Model
{
    [DataContract]
    public class ManualApprovalJobParam
    {
        [DataMember]
        public int ApprovalAction { get; set; }
        [DataMember]
        public ManualApprovalQueryDefinition QueryDefintion { get; set; }
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string ApprovalComment { get; set; }
        [DataMember]
        public string QuickReason { get; set; }
		[DataMember]
        public List<Guid> UncheckedItemIds  { get; set; } = new List<Guid>();
        [DataMember]
        public ManualApprovalExtendType ExtendType { get; set; }
        [DataMember]
        public DateTime CustomeExtendDate { get; set; }
        [DataMember]
        public bool IsFromMyhub { get; set; } = false;
        [DataMember]
        public RequesterTypeEnum RequesterType { get; set; }
        [DataMember]
        public string PartitionKeyId { get; set; }
        [DataMember]
        public bool IsJpmc { get; set; } = false;
    }
}
