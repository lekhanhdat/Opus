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



namespace AvePoint.GCommon.Contract.Server.ControlPanel.PermissionMapping
{
    #region using directives
    using System;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PermissionMappingDto
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string ObjectId { get; set; }

        [DataMember]
        public string ObjectName { get; set; }

        [DataMember]
        public PermissionMappingObjectType ObjectType { get; set; }

        [DataMember]
        public string UserOrGroupId { get; set; }

        [DataMember]
        public string UserOrGroupName { get; set; }

        [DataMember]
        public PermissionMappingUserGroupType UserOrGroupType { get; set; }

        [DataMember]
        public Permission Permission { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PermissionMappingObjectType
    {
        [EnumMember]
        NonSpecified,

        [EnumMember]
        Plan,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PermissionMappingUserGroupType
    {
        [EnumMember]
        User,

        [EnumMember]
        Group,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [Flags]
    public enum Permission
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        Read = 0x01,

        [EnumMember]
        Write = 0x02,

        [EnumMember]
        Execute = 0x04,

        [EnumMember]
        FullControl = Read | Write | Execute,
    }
}
