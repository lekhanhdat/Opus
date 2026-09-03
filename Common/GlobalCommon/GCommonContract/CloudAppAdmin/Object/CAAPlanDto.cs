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
    using AvePoint.GCommon.Contract.Server.Common;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Xml.Serialization;

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAAPlanDto : PlanDto
    {
        [DataMember]
        public CAAOperationContent PlanContent { get; set; }

        [DataMember]
        public CAAOperationType OperationType { get; set; }

        [DataMember]
        public string NotificationProfileId { get; set; }

        [DataMember]
        public string ParentPlanId { get; set; }

        [DataMember]
        public List<string> SubPlanIds { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CAAOperationType
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        ResetUserPassword = 1,

        [EnumMember]
        CreateUser = 2,

        [EnumMember]
        EditUser = 3,

        [EnumMember]
        DeleteUser = 4,

        [EnumMember]
        AssignUserApplication = 5,

        [EnumMember]
        AssignUserLicense = 6,

        [EnumMember]
        ManageUserGroup = 7,

        [EnumMember]
        CreateGroup = 8,

        [EnumMember]
        DeleteGroup = 9,

        [EnumMember]
        AssignGroupApplication = 10,

        [EnumMember]
        AssignGroupLicense = 11,

        [EnumMember]
        AddUserMailboxAccess = 12,

        [EnumMember]
        RemoveUserMailboxAccess = 13,

        [EnumMember]
        EditGroup = 14,

        [EnumMember]
        DeleteUserFromRecycleBin = 15,

        [EnumMember]
        RestoreUserFromRecycleBin = 16,

        [EnumMember]
        RunPE = 17,

        [EnumMember]
        RunWhatIfPE = 18,

        [EnumMember]
        RunPEConflict = 19,

        [EnumMember]
        RunPEReport = 20,

        [EnumMember]
        RemoveUserLicense = 26,

        [EnumMember]
        ReplaceUserLicense = 27,

        [EnumMember]
        RemoveGroupLicense = 28,

        [EnumMember]
        ReplaceGroupLicense = 21,

        [EnumMember]
        RemoveUserApplication = 22,

        [EnumMember]
        ReplaceUserApplication = 23,

        [EnumMember]
        RemoveGroupApplication = 24,

        [EnumMember]
        ReplaceGroupApplication = 25,

        [EnumMember]
        InviteUser = 29,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CAALoadDetailType
    {
        [EnumMember]
        UserApplications = 0,

        [EnumMember]
        UserLicenses = 1,

        [EnumMember]
        UserGroups = 2,

        [EnumMember]
        UserAccessEmails = 3,

        [EnumMember]
        UserLoadAll = 4,

        [EnumMember]
        UserProperty = 5,

        [EnumMember]
        GroupApplications = 6,

        [EnumMember]
        GroupProperty = 7,

        [EnumMember]
        UserPropertyWithoutRole = 8,

        [EnumMember]
        GroupMembers = 9
    }

    [XmlRoot(ElementName = "CAAPlanSetting")]
    public class CAAPlanSettings
    {
        [XmlAttribute]
        public int OperationType { get; set; }
        [XmlAttribute]
        public CAAOperationContent PlanContent { get; set; }

        [XmlAttribute]
        public string NotificationId { get; set; }

        [XmlAttribute]
        public string ParentPlanId { get; set; }
        [XmlAttribute]
        public List<string> SubPlanIds { get; set; }
    }
}