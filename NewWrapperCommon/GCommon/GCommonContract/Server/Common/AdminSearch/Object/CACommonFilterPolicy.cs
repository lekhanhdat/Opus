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



using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
namespace AvePoint.GCommon.Contract.Server.Common.AdminSearch.Object
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;
    using AvePoint.GCommon.Contract.CommonFilter;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.CentralAdmin.Object;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CACommonFilterPolicy
    {
        [DataMember]
        [XmlAttribute]
        public List<FilterPolicy> CommonFilterPolicies { get; set; }

        [DataMember]
        [XmlAttribute]
        public Dictionary<PolicyLevel, string> CommonFilterExpressions { get; set; }

        [DataMember]
        [XmlAttribute]
        public string Expression { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SecurityFilterPolicy : Extention
    {
        //policy role: user and group; policy condition: contains

        [DataMember]
        [XmlAttribute]
        public string LoginName { get; set; }
        [DataMember]
        [XmlAttribute]
        public SearchForPermissionOption Permission { get; set; }
        [DataMember]
        [XmlAttribute]
        public string CustomizedPermissions { get; set; }
        [DataMember]
        [XmlAttribute]
        public bool ExtractPermission { get; set; }
    }
}

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASearchFilter
    {
        [DataMember]
        public SPObjectLevel ResultLevel { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum SearchForPermissionOption
    {
        [EnumMember]
        SearchForAnyPermission,
        [EnumMember]
        FullControl,
        [EnumMember]
        Design,
        [EnumMember]
        Edit,//add for sp13
        [EnumMember]
        Contribute,
        [EnumMember]
        Read,
        [EnumMember]
        ViewOnly,
        [EnumMember]
        LimitedAccess,
        [EnumMember]
        InputedPermission
    }
}
