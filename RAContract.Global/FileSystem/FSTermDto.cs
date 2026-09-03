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
using System.Runtime.Serialization;

namespace AvePoint.RA.Contract.FileSystem
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class FSTermDto
    {
        [DataMember(EmitDefaultValue = false)]
        public int Id { set; get; }       
        [DataMember(EmitDefaultValue = false)]
        public int TermSetId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public Guid UniqueId { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public string Name { get; set; }        
        //[DataMember(EmitDefaultValue = false)]
        //public bool BreakInheritFromParent { get; set; }
        //[DataMember(EmitDefaultValue = false)]
        //public string TimeZoneId { get; set; }
        //[DataMember(EmitDefaultValue = false)]
        //public string RuleInfo { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public long TermExpirationFrom { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public long TermExpirationTo { get; set; }
        //[DataMember(EmitDefaultValue = false)]
        //public bool IsRootTerm { get; set; }
        //[DataMember(EmitDefaultValue = false)]
        //public bool IsDayLight { get; set; }
        //[DataMember(EmitDefaultValue = false)]
        //public double AvailableSpace { get; set; }
        //[DataMember(EmitDefaultValue = false)]
        //public bool IsDefaultTerm { get; set; }     
        //[DataMember(EmitDefaultValue = false)]
        //public bool IsPermanent { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsDeprecated { get; set; }
        [DataMember(EmitDefaultValue = false)]
        public bool IsRemoved { get; set; }
    }
}
