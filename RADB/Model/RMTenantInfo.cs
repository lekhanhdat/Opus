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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Model
{
    public class RMTenantInfo : BaseModel
    {
        [Key]
        [Column(TypeName = "varchar")]
        [MaxLength(64)]
        public string Id { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(64)]
        [Required]
        public string RegisterEmail { get; set; }

        [Column(TypeName = "int")]
        [Required]
        /// <summary>
        /// 0: enable
        /// 1: disable
        /// 2: softDeleted
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// GB
        /// </summary>
        [Column(TypeName = "int")]
        [Required]
        public int StorageUsageQuota { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(64)]
        public string DBSchema { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(64)]
        public string DBName { get; set; }

        /// <summary>
        /// GB
        /// </summary>
        [Column(TypeName = "int")]
        [Required]
        public int DBUsageQuota { get; set; }

        [Column(TypeName = "datetime2")]
        [Required]
        public DateTime CreateTime { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string StorageSetting { get; set; }

        public DateTime LastModified { get; set; } = DateTime.UtcNow;

        [Column(TypeName = "bit")]
        public bool? EnableCSD { get; set; }
        /// <summary>
        /// For db upgrade, only works before Sep;  0:not start;   1:finish   2:upgrading
        /// </summary>
        [Column(TypeName = "int")]
        public int MovedToNewDB { set; get; }

        /// <summary>
        /// Null: Old Tenants
        /// AvePoint.RA.Contract.Aos.Notification.RMSyncDataState
        /// </summary>
        [Column(TypeName = "int")]
        public int? SyncNodeState { get; set; }

        ///// <summary>
        ///// Null: Old Tenants
        ///// AvePoint.RA.Contract.Aos.Notification.RMSyncDataState
        ///// </summary>
        //[Column(TypeName = "int")]
        //public int? SyncSAState { get; set; }

        [Column(TypeName = "bit")]
        public bool IsUpgradeRemoteNodeForAosId { get; set; }

        [Column(TypeName = "bit")]
        public bool IsUpgradeManualData { get; set; }

        [Column(TypeName = "bit")]
        public bool IsUsedForAOSP { get; set; }
        
        [Column(TypeName = "bit")]
        public bool IsInitForGControlPlatform { get; set; }

        [Column(TypeName = "int")]
        public int MultiGeoStatus { set; get; }
    }

    
}
