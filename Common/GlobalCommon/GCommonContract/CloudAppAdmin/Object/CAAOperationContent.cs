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
    public class CAAOperationContent
    {
        [DataMember]
        public string TenantId { get; set; }

        [DataMember]
        public string TenantName { get; set; }

        [DataMember]
        public string AccessToken { get; set; }

        [DataMember]
        public string AOSAPIUrl { get; set; }

        [DataMember]
        public List<ADUser> UsersToUpdate { get; set; }

        [DataMember]
        public Dictionary<string, object> ChangedProperties { get; set; }

        [DataMember]
        public List<ADGroup> GroupsToUpdate { get; set; }

        [DataMember]
        public List<ADLicense> LicenseToAssign { get; set; }

        [DataMember]
        public List<ADLicense> LicenseToRemove { get; set; }

        [DataMember]
        public bool? IsRemoveAll { get; set; }

        [DataMember]
        public string UsageLocation { get; set; }

        [DataMember]
        public List<ADAppRoleAssignment> ApplicationToAssign { get; set; }

        [DataMember]
        public List<ADAppRoleAssignment> ApplicationToRemove { get; set; }

        [DataMember]
        public List<ADUser> UsersToAdd { get; set; }

        [DataMember]
        public List<ADGroup> GroupsToAdd { get; set; }

        [DataMember]
        public List<ADUser> UsersToRemove { get; set; }

        [DataMember]
        public List<ADGroup> GroupsToRemove { get; set; }

        [DataMember]
        public string UserName { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public List<ADMailbox> MailboxesToAdd { get; set; }

        [DataMember]
        public List<ADMailbox> MailboxesToRemove { get; set; }

        [DataMember]
        public List<CAAPEProfileContent> PEProfiles { get; set; }

        [DataMember]
        public List<CAAPEResultProfileContent> PEResultProfile { get; set; }

        [DataMember]
        public string DocAveTenantId { get; set; }

        [DataMember]
        public string AuditLogConnString { get; set; }

        [DataMember]
        public string Remark1 { get; set; }

        [DataMember]
        public List<string> RelativePlanIds { get; set; }

        [DataMember]
        public string GroupId { get; set; }

        [DataMember]
        public string AdminUrl { get; set; }
    }
}