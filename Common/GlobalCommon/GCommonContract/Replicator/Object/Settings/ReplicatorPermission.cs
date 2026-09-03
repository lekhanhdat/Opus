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






using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Replicator.Object.Settings
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReplicatorPermission
    {
        [DataMember]
        public bool IsInclude { get; set; }

        [DataMember]
        public bool ReceiveSecurityChangeFromDesc { get; set; }

        [DataMember]
        public bool EnableSyncDeletion { get; set; }

        [DataMember]
        public PermissionSiteCollectionLevel SiteCollectionLevel { get; set; }

        [DataMember]
        public PermissionSiteLevel SiteLevel { get; set; }

        [DataMember]
        public PermissionListLevel ListLevel { get; set; }

        [DataMember]
        public PermissionFolderLevel FolderLevel { get; set; }

        [DataMember]
        public PermissionItemLevel ItemLevel { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PermissionSiteCollectionLevel
    {
        [DataMember]
        public bool Users { get; set; }

        [DataMember]
        public bool Groups { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PermissionSiteLevel
    {
        [DataMember]
        public bool Users { get; set; }

        [DataMember]
        public bool Groups { get; set; }

        [DataMember]
        public bool PermissionLevels { get; set; }

        [DataMember]
        public bool SitePermissions { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PermissionListLevel
    {
        [DataMember]
        public bool Users { get; set; }

        [DataMember]
        public bool Groups { get; set; }

        [DataMember]
        public bool ListPermission { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PermissionFolderLevel
    {
        [DataMember]
        public bool Users { get; set; }

        [DataMember]
        public bool Groups { get; set; }

        [DataMember]
        public bool FolderPermission { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PermissionItemLevel
    {
        [DataMember]
        public bool Users { get; set; }

        [DataMember]
        public bool Groups { get; set; }

        [DataMember]
        public bool ItemPermission { get; set; }
    }
}
