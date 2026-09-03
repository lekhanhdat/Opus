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
    public class ManualApprovalExtendDefinition
    {
        [DataMember]
        public List<Guid> ItemIds { get; set; } = new List<Guid>();
        [DataMember]
        public ManualApprovalExtendType ExtendType { get; set; }
        [DataMember]
        public DateTime CustomeExtendDate { get; set; }
        [DataMember]
        public string Comment { get; set; }
    }
    [DataContract]
    public enum ManualApprovalExtendType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        After3Month = 1,
        [DataMember]
        After6Month = 2,
        [DataMember]
        After1Year = 3,
        [DataMember]
        Custom = 4,
        [DataMember]
        Month = 5 ,
        [DataMember]
        Year = 6  ,
        [EnumMember]
        After1Month = 7,
    }
}
