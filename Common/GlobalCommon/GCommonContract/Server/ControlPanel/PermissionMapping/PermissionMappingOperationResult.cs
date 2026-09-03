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
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PermissionMappingOperationResult
    {
        [DataMember]
        public bool HasError { get; set; }

        [DataMember]
        public PermissionMappingOperationErrorType ErrorType { get; set; }

        [DataMember]
        public List<ValidateUserGroupResult> ValidateUserGroupResults { get; set; }

        [DataMember]
        public List<PermissionMappingDto> PermissionMappings { get; set; }

        [DataMember]
        public List<PermissionMappingUserGroupInfoDto> UserGroupInfo { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ValidateUserGroupResult
    {
        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public bool Exist { get; set; }

        [DataMember]
        public PermissionMappingUserGroupType Type { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PermissionMappingOperationErrorType
    {
        [EnumMember]
        None,
    }
}
