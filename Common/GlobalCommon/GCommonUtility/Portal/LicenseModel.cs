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

using AvePoint.GCommon.Contract.AccountManager.Object;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace AvePoint.Common.Portal
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class LicenseModel : IProfileContent
    {
        [DataMember]
        public LicenseType Type { get; set; }
        [DataMember]
        public Int32 UserSeat { get; set; }
        [DataMember]
        public int ExtendStorage { get; set; }
        [DataMember]
        public List<LicenseUnitDto> Units { get; set; }
        [DataMember]
        public LicenseVersion Version { get; set; }
        [DataMember]
        public Extension Extension { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum LicenseType
    {
        [EnumMember]
        Trial = 0,
        [EnumMember]
        Enterprise = 1,
        [EnumMember]
        Internal = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum LicenseVersion
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Old = 1,
        [EnumMember]
        New = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class Extension
    {
        [DataMember]
        public bool Byos { get; set; }

        [DataMember]
        public int CustomerSize { get; set; }

        [DataMember]
        public int RetentionYear { get; set; }

    }
}
