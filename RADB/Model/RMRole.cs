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
using AvePoint.RA.Contract.RoleAssignments;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Model
{
    public class RMRole: BaseModel
    {
        [Key]
        [Column(TypeName = "int", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int RoleId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        [Required]
        public string RoleName { get; set; }

        [Column(TypeName = "bit")]
        public bool IsSystemAdmin { get; set; }

        [Required]
        public RMRoleType RoleType { get; set; }
        [Column(TypeName = "bigint")]
        public long PermissionMasks { set; get; }
        [Column(TypeName = "bigint")]
        public long ReportingPermission { set; get; }

        [Column(TypeName = "bit")]
        [Required]
        [Index(IsClustered = false)]
        public bool IsRemoved { get; set; }
        [Column(TypeName = "datetime2")]
        [Required]
        public DateTime Modified { get; set; }
        [Column(TypeName = "bigint")]
        public long SubPermission1 { set; get; }
        [Required]
        public RMRoleUpgradeType UpgradeType { set; get; }
        [Column(TypeName = "bigint")]
        public long PermissionExtensionMasks { set; get; }
        [Column(TypeName = "bigint")]
        public long SOPermissionMasks { set; get; }
        [Column(TypeName = "bigint")]
        public long DiscoveryPermissionMasks { get; set; }
        [Column(TypeName = "bigint")]
        public long SalesforceDiscoveryPermissionMasks { get; set; }
        [Column(TypeName = "bigint")]
        public long GoogleROTDiscoveryPermissionMasks { get; set; }
        [Column(TypeName = "bigint")]
        public long FSDiscoveryPermissionMasks { get; set; }

        [Column(TypeName = "bit")]
        public bool IsNewGroup { get; set; } //for so only license upgrade
    }
}
