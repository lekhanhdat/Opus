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

    public class RMSecurityGroup : BaseModel
    {
        /// <summary>
        /// 1 is AdminGroup ,2 is StandardUser group for upgrade from May2021.
        /// Roleid in RMLnkUserRoles is Id in Security Group ,May 2021 delegate admin.Every Group match one role. 
        /// When support role-based security trimming ,will change to one group can match different roles.
        /// Add [Group role mapping table] Upgrade.
        /// </summary>
        [Key]
        [Column(TypeName = "int", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }
        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        [Required]
        public string Name { get; set; }
        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        public string Description { get; set; }
        [Column(TypeName = "bit")]
        [Required]
        [Index(IsClustered = false)]
        public bool IsRemoved { get; set; }
        [Column(TypeName = "int")]
        public int RoleId { get; set; }//TO DO Contrim Upgrade now or depending on design.
        [Column(TypeName = "bigint")]
        public long ModifiedTime { get; set; }
        [Column(TypeName = "nvarchar(max)")]
        public String NodeString { get; set; }//TreeNode String including check status.
        [Column(TypeName = "nvarchar(max)")]
        public String RuleNodeString { get; set; }//TreeNode String including check status.
        [Column(TypeName = "bit")]
        public bool IsEnableTrim { get; set; }
    }
}
