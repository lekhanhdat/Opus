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
using AvePoint.RA.Contract.Aos.Notification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Tenant
{
    public class TenantInfoDto
    {
        public string TenantId { get; set; }
        public string RegisterEmail { get; set; }
        public TenantStatus Status { get; set; }
        public string SchemaName { get; set; }
        public string DBName { get; set; }
        public int StorageQuota { get; set; }
        public string EncryptionKey { get; set; }
        public string StorageSetting { get; set; }
        public int DBQuota { get; set; }
        public string AOSSecurityProfileId { get; set; }
        /// <summary>
        /// 0:not upgrade,  1:finish    2:upgrading  ;  add for cosmos data upgrade
        /// </summary>
        public int ExplorerUpgradeStatus { set; get; }
        public int SyncNodeState { get; set; }

        public bool IsUsedForAOSP { get; set; }
        public bool IsInitForGControlPlatform { get; set; }
        //public int SyncSAState { get; set; }
        public int MultiGeoStatus { get; set; }
    }

    public enum TenantStatus
    {
        Provisioning = -1,
        Normal = 0,
        Disabled = 1,
        Locked = 2,
        SoftDeleted = 3,
        HardDeleting = 4,
    }


    public class RMDBInfoDto 
    {
        public string ContainerName { get; set; }
        public string DBName { get; set; }
        public int DBSize { get; set; }
        public RMDBType Type { get; set; }

        public int Resource { get; set; }
    }

    public enum RMDBType
    {
        TenantDB = 0,
        ExplorerDB = 1,
    }
}
