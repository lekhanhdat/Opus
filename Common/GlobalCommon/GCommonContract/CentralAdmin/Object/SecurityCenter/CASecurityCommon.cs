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





using System.Collections.Generic;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object.SecurityCenter
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAGroupInfo : BasicPrincipalInfo
    {
        [DataMember]
        public bool AllowRequestToJoinLeave { get; set; }

        [DataMember]
        public bool AutoAcceptRequestToJoinLeave { get; set; }

        [DataMember]
        public bool AllowMembersEditMembership { get; set; }

        [DataMember]
        public bool OnlyAllowMembersViewMembership { get; set; }

        [DataMember]
        public string RequestToJoinLeaveEmailSetting { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public UserDetail Owner { get; set; }

        [DataMember]
        public List<CAUserInfo> Users { get; set; }

        [DataMember]
        public string NewGroupName { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAUserInfo : BasicPrincipalInfo
    {
        /// <summary>
        ///     if the user have no group, the value should be 0;
        /// </summary>
        [DataMember]
        public int ParentGroupId { get; set; }

        [DataMember]
        public string AboutMe { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Department { get; set; }

        [DataMember]
        public string Email { get; set; }
    }

    [KnownType(typeof(CAUserInfo))]
    [KnownType(typeof(CAGroupInfo))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class BasicPrincipalInfo
    {
        /// <summary>
        /// For Gui
        /// </summary>
        public bool IsSelected { get; set; }

        [DataMember]
        public string BatchUrl { get; set; }

        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string LoginName { get; set; }

        [DataMember]
        public CAPrincipalType PrincipalType { get; set; }

        [DataMember]
        public List<CAPermissionInfo> PermissionInfos { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAPermissionInfo
    {
        [DataMember]
        public bool IsChecked { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public int Value { get; set; } //permission level id

        [DataMember]
        public bool CanEdit { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string SelectNodeUrl { get; set; }

        public ulong PermissionMask { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAEmailInfo
    {
        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public string PersonalMessage { get; set; }

        public CAEmailInfo Clone()
        {
            CAEmailInfo email = new CAEmailInfo();
            email.Subject = Subject;
            email.PersonalMessage = PersonalMessage;
            return email;
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CAPrincipalType
    {
        [EnumMember]
        Node,

        [EnumMember]
        SharePointUser,

        [EnumMember]
        SharePointGroup,

        [EnumMember]
        DomainGroup
    }
}
