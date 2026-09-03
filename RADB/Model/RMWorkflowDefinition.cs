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
using AvePoint.RA.Contract.RMWeb;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Model
{
    public class RMWorkflowDefinition : BaseModel
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        /// <summary>
        /// Different versions of workflow use the same referenceid
        /// </summary>
        [Column(TypeName = "uniqueidentifier")]
        [Required]
        public Guid ReferenceId { get; set; }

        [Required]
        public RMWorkflowType Type { get; set; }

        [Column(TypeName = "nvarchar")]
        [Required]
        [MaxLength(512)]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        public string Description { get; set; }

        [Column(TypeName = "nvarchar")]
        [Required]
        [MaxLength(64)]
        public string Version { get; set; }

        /// <summary>
        /// Serialized xml or json string of workflow definition
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        [Required]
        public string ContentStr { get; set; }

        /// <summary>
        /// Serialized xaml string of workflow definition
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string XamlStr { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(256)]
        public string HashCode { get; set; }

        [Required]
        public DateTime CreationTime { get; set; }

        [Required]
        public DateTime LastUpdatedTime { get; set; }

        [Column(TypeName = "nvarchar")]
        [Required]
        [MaxLength(128)]
        public string CreatedBy { get; set; }

        [Column(TypeName = "int")]
        public int Level { get; set; }
    }
}
