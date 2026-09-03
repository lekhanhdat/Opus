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
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ADApplication
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public List<ADAppRole> Roles { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ADAppRole
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string DisplayName { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ADServicePrincipal
    {
        [DataMember]
        public string ObjectId { get; set; }
        [DataMember]
        public string AppDisplayName { get; set; }
        [DataMember]
        public string AppId { get; set; }
        [DataMember]
        public bool? AppRoleAssignmentRequired { get; set; }
        [DataMember]
        public DateTime? DeletionTimestamp { get; set; }
        [DataMember]
        public string DisplayName { get; set; }
        [DataMember]
        public string ObjectType { get; set; }
        [DataMember]
        public string PublisherName { get; set; }
        [DataMember]
        public bool? AccountEnabled { get; set; }
        [DataMember]
        public List<ADAppRoleAssignedTo> AppRolesAssignedTo { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ADAppRoleAssignedTo
    {
        //[DataMember]
        //public DateTime? CreationTimestamp { get; set; }
        //[DataMember]
        //public DateTime? DeletionTimestamp { get; set; }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string PrincipalDisplayName { get; set; }
        [DataMember]
        public string ObjectId { get; set; }
        [DataMember]
        public string ObjectType { get; set; }
        [DataMember]
        public string PrincipalId { get; set; }
        [DataMember]
        public string PrincipalType { get; set; }
        [DataMember]
        public string ResourceDisplayName { get; set; }
        [DataMember]
        public string ResourceId { get; set; }
    }
}