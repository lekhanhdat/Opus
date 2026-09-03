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

namespace AvePoint.GCommon.Contract.CloudAppAdmin.Message
{
    using AvePoint.GCommon.Contract.CloudAppAdmin.Object;
    using AvePoint.GCommon.Contract.Common;
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UpdateUserMessage
    {
        [DataMember]
        public string TenantId { get; set; }

        [DataMember]
        public List<ADUser> Users { get; set; }

        [DataMember]
        public CAAOperationType OperationType { get; set; }

        [DataMember]
        public string NotificationProfileId { get; set; }

        [DataMember]
        public Dictionary<string, object> ChangedProperties { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UpdateGroupMessage
    {
        [DataMember]
        public string TenantId { get; set; }

        [DataMember]
        public List<ADGroup> Groups { get; set; }

        [DataMember]
        public CAAOperationType OperationType { get; set; }

        [DataMember]
        public string NotificationProfileId { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AssignUserLicenseMessage
    {
        [DataMember]
        public string TenantId { get; set; }

        [DataMember]
        public List<ADLicense> LicenseToAssign { get; set; }

        [DataMember]
        public List<ADLicense> LicenseToRemove { get; set; }

        [DataMember]
        public List<ADUser> Users { get; set; }

        [DataMember]
        public string NotificationProfileId { get; set; }

        [DataMember]
        public string UsageLocation { get; set; }

        [DataMember]
        public CAAOperationType? operationType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AssignUserApplicationMessage
    {
        [DataMember]
        public string TenantId { get; set; }

        [DataMember]
        public List<ADAppRoleAssignment> ApplicationToAssign { get; set; }

        [DataMember]
        public List<ADAppRoleAssignment> ApplicationToRemove { get; set; }

        [DataMember]
        public List<ADUser> Users { get; set; }

        [DataMember]
        public CAAOperationType? operationType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ManageUserGroupMessage
    {
        [DataMember]
        public string TenantId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<ADUser> UsersToAdd { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<ADGroup> GroupsToAdd { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<ADUser> UsersToRemove { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<ADGroup> GroupsToRemove { get; set; }

        public CAAOperationType OperationType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AssignGroupLicenseMessage
    {
        [DataMember]
        public string TenantId { get; set; }

        [DataMember]
        public List<ADLicense> LicenseToAssign { get; set; }

        [DataMember]
        public List<ADLicense> LicenseToRemove { get; set; }

        [DataMember]
        public bool? IsRemoveAll { get; set; }

        [DataMember]
        public List<ADGroup> Groups { get; set; }

        [DataMember]
        public string NotificationProfileId { get; set; }

        [DataMember]
        public string UsageLocation { get; set; }

        [DataMember]
        public CAAOperationType? operationType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class AssignGroupApplicationMessage
    {
        [DataMember]
        public string TenantId { get; set; }

        [DataMember]
        public List<ADAppRoleAssignment> ApplicationToAssign { get; set; }

        [DataMember]
        public List<ADAppRoleAssignment> ApplicationToRemove { get; set; }

        [DataMember]
        public List<ADGroup> Groups { get; set; }

        [DataMember]
        public CAAOperationType? operationType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ManageUserEmailAccessMessage
    {
        [DataMember]
        public string TenantId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string UserName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string Password { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<ADUser> Users { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<ADMailbox> MailboxesToAdd { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public List<ADMailbox> MailboxesToRemove { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public CAAOperationType OperationType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BatchUpdateUserMessage
    {
        [DataMember]
        public string TenantId { get; set; }

        [DataMember]
        public ADUser UserCommonSettings { get; set; }

        [DataMember]
        public CAAOperationType OperationType { get; set; }

        [DataMember]
        public string NotificationProfileId { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public byte[] FileData { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BatchUpdateGroupMessage
    {
        [DataMember]
        public string TenantId { get; set; }

        [DataMember]
        public ADGroup GroupCommonSettings { get; set; }

        [DataMember]
        public CAAOperationType OperationType { get; set; }

        [DataMember]
        public string NotificationProfileId { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public byte[] FileData { get; set; }
    }
}