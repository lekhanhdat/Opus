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
using AvePoint.RA.Contract.Object;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    //[Table("RMScopePermission")]
    public class RMScopePermission : BaseModel
    {
        [Key]
        [Column(TypeName = "int")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Index]
        [Column(TypeName = "nvarchar")]
        [MaxLength(128)]
        [Required]
        public string Scope { get; set; }

        [Index]
        [Column(TypeName = "nvarchar")]
        [MaxLength(128)]
        public string ParentScope { get; set; }

        //[Required]
        //public RMScopePermissionEnum Permission { get; set; }

        /// <summary>
        /// the full path from root to the current, end with and seprated by '/'. e.g. a/b/c/
        /// </summary>
        [Required]
        [Index]
        [Column(TypeName = "nvarchar")]
        public string ScopePath { get; set; }

        //public virtual ICollection<ScopeAccountMapping> Accounts { get; set; }

    }

    public class RMScopeAccountMapping : BaseModel
    {
        [Key]
        [Column(TypeName = "int")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>
        /// foreign key of Id field of RMScopePermission
        /// </summary>
        [Index]
        [Column(TypeName = "int")]
        public int ScopePermission { get; set; }

        /// <summary>
        /// foreign key of Id field of Account
        /// </summary>
        [Index]
        [Column(TypeName = "int")]
        public int Account { set; get; }

        [Required]
        public RMScopePermissionEnum Permission { get; set; }
    }

    public class RMScopePermissionJobInfo : BaseModel
    {
        [Key]
        [Column(TypeName = "nvarchar")]
        [MaxLength(128)]
        [Required]
        public string ScopeId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(128)]
        [Required]
        public string JobId { get; set; }

        [Required]
        public DateTime LastUpdatedTime { get; set; }
    }

}
