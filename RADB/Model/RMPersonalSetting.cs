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
using AvePoint.RA.Contract.PersonalSetting;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    public class RMPersonalSetting : BaseModel
    {
        [Key]
        [Column(TypeName = "int", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        [Column(TypeName = "nvarchar")]
        [Index(name: "idx_owner_type_name", order : 1)]
        [MaxLength(64)]
        [Required]
        public string Owner { get; set; }  // user id of account

        [Column(TypeName = "int")]
        [Index(name: "idx_owner_type_name", order: 2)]
        [Required]
        public PersonalSettingType Type { get; set; }

        [Column(TypeName = "nvarchar")]
        [Index(name: "idx_owner_type_name", order: 3)]
        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        [Required]
        public string ContentStr { get; set; }

        [Obsolete]
        [Column(TypeName = "bit")]
        [Required]
        public bool IsDefault { get; set; }

        [Column(TypeName = "bit")]
        [Required]
        public bool IsBuiltIn { get; set; }
    }
}
