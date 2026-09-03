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

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASharingSettingOperation : CAOperation
    {
        [DataMember]
        public string SiteCollectionUrl;

        [DataMember]
        public CASharingCapabilities TenantSharingCapability;

        [DataMember]
        public CASharingCapabilities SiteSharingCapability;

        [DataMember]
        public string TenantLevelSharingAllowedDomainList { get; set; }

        [DataMember]
        public string TenantLevelSharingBlockedDomainList { get; set; }

        [DataMember]
        public CASharingDomainRestrictionModes TenantLevelSharingDomainRestrictionMode { get; set; }

        [DataMember]
        public string SharingAllowedDomainList { get; set; }

        [DataMember]
        public string SharingBlockedDomainList { get; set; }

        [DataMember]
        public CASharingDomainRestrictionModes SharingDomainRestrictionMode { get; set; }

        [DataMember]
        public CASharingPermissionType DefaultLinkPermission { get; set; }

        [DataMember]
        public CASharingLinkType DefaultSharingLinkType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CASharingCapabilities
    {
        [EnumMember]
        Disabled = 0,
        [EnumMember]
        ExternalUserSharingOnly = 1,
        [EnumMember]
        ExternalUserAndGuestSharing = 2,
        [EnumMember]
        ExistingExternalUserSharingOnly = 3,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CASharingDomainRestrictionModes
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        AllowList = 1,
        [EnumMember]
        BlockList = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CASharingPermissionType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        View = 1,
        [EnumMember]
        Edit = 2,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CASharingLinkType
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        Direct = 1,
        [EnumMember]
        Internal = 2,
        [EnumMember]
        AnonymousAccess = 3,
    }
}
