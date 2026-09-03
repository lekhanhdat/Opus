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

namespace AvePoint.GCommon.Contract.CloudAppAdmin.Object
{
    using AvePoint.GCommon.Contract.Common;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ADGroup
    {
        [DataMember]
        public string ObjectId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<ADUser> Members { get; set; }

        [DataMember]
        public List<string> MemberObjectIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string MemberSetId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<ADUser> Owners { get; set; }

        [DataMember]
        public List<string> OwnerObjectIds { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string OwnerSetId { get; set; }

        [DataMember]
        public List<ADAppRoleAssignment> Applications { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string Description { get; set; }

        //public string SourcedFrom { get; set; }
        [DataMember]
        public string MailNickname { get; set; }

        [DataMember]
        public GroupType GroupType { get; set; }

        [DataMember]
        public bool? DirSyncEnabled { get; set; }

        [DataMember]
        public List<ADExtensionProperty> ExtensionProperties { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SimpleADGroup
    {
        [DataMember]
        public string GroupID { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum GroupType
    {
        [EnumMember]
        UnKnown = 0,

        [EnumMember]
        DistributionList = 1,

        [EnumMember]
        MailEnabledSecuirty = 2,

        [EnumMember]
        SecurityGroup = 3,

        [EnumMember]
        O365Group = 4,
    }

    public enum GroupAccessType
    {
        All = 0,
        Public = 1,
        Private = 2
    }
}