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
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;

namespace AvePoint.GCommon.Contract.AccountManager.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class TenantUserResultDto
    {
        [DataMember]
        public List<AccountMappingDto> Accounts { get; set; }

        [DataMember]
        public List<AccountMappingDto> SuccAccounts { get; set; }

        [DataMember]
        public List<AccountMappingDto> ExsitedAccounts { get; set; }

        [DataMember]
        public List<AccountMappingDto> FailedAccounts { get; set; }

        [DataMember]
        public AccountMappingDto Account { get; set; }

        [DataMember]
        public GroupDto Group { get; set; }

        [DataMember]
        public TenantUserResultStatus Status { get; set; }

        [DataMember]
        public List<PlanDto> CheckUserPlans { get; set; }

        [DataMember]
        public List<ProfileDto> CheckUserProfiles { get; set; }

        [DataMember]
        public List<RemoteSiteCollection> CheckUserRemoteSiteCollection { get; set; }

        [DataMember]
        public Dictionary<string, List<string>> PlanSiteCollectionMapping { get; set; }

        [DataMember]
        public Dictionary<PlanDto, List<string>>  PlanGroupAndPlanMapping { get; set; }

        [DataMember]
        public string Message { get; set; }

        public TenantUserResultDto()
        {
            CheckUserPlans = new List<PlanDto>();
            CheckUserProfiles = new List<ProfileDto>();
            CheckUserRemoteSiteCollection = new List<RemoteSiteCollection>();
        }
    }

    public enum TenantUserResultStatus
    {
        SUCC,
        FAIL
    }

    public enum TenantUserActiveStatus
    {
        ACTIVE,
        DEACTIVE
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UserPermission
    {
        [DataMember]
        public string UserId { get; set; }
        [DataMember]
        public string ObjectId { get; set; }

        [DataMember]
        public List<EntityObjectPermissionType> Permission { get; set; }

        public EntityObjectPermissionType GetPermission()
        {

            EntityObjectPermissionType permissionType = EntityObjectPermissionType.None;
            if (Permission == null)
            {
                return permissionType;
            }
            foreach (EntityObjectPermissionType objectPermissionType in Permission)
            {
                permissionType |= objectPermissionType;
            }
            return permissionType;
        }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class TenantUserDataDto
    {
        [DataMember]
        public AccountMappingDto CurrentAccountMapping { get; set; }

        [DataMember]
        public string Accounts { get; set; }

        [DataMember]
        public string Schema { get; set; }

        [DataMember]
        public string Host { get; set; }

        [DataMember]
        public int Port { get; set; }

        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string GroupId { get; set; }

        [DataMember]
        public long Time { get; set; }

        [DataMember]
        public List<string> UserIds { get; set; }

        [DataMember]
        public List<string> PlanIds { get; set; }

        [DataMember]
        public List<string> SiteCollectionIds { get; set; }

        [DataMember]
        public List<string> ProfileIds { get; set; }
        
        [DataMember]
        public ObjectRoleType RoleType { get; set; }

        [DataMember]
        public TenantUserActiveStatus ActiveStatus { get; set; }

        [DataMember]
        public List<UserPermission> Permissions { get; set; }

    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UsingPlanInfoDto
    {
        [DataMember]
        public List<string> UsingPlanNames { get; set; }
        [DataMember]
        public List<string> NotDeleteSiteCollectionIds { get; set; }
    }
}
