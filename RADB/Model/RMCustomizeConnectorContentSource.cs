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
using AvePoint.RA.Contract.CustomizeConnector.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Model
{
    public class RMCustomizeConnectorContentSource : BaseModel
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        public Guid Id { get; set; }

        /// <summary>
        /// The value of the built-in content source is I18N key.
        /// The value of the external custom content source is display name.
        /// </summary>
        [Required]
        [Column(TypeName = "nvarchar")]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar")]
        public string Description { get; set; }

        /// <summary>
        /// The value of custom content source starts from 1000
        /// </summary>
        [Required]
        [Column(TypeName = "int")]
        public int Flag { get; set; }

        [Required]
        [Column(TypeName = "int")]
        public CustomizeConnectorOrigin Origin { get; set; }

        [Required]
        [Column(TypeName = "bigint")]
        public long Created { get; set; }

        [Required]
        [Column(TypeName = "bigint")]
        public long Modified { get; set; }

        [MaxLength(64)]
        [Column(TypeName = "nvarchar")]
        public string CreatedBy { get; set; }

        [MaxLength(64)]
        [Column(TypeName = "nvarchar")]
        public string ModifiedBy { get; set; }

        [Column(TypeName = "bit")]
        public bool IsRemoved { get; set; }

        [NotMapped]
        public List<RMCustomizeConnectorTemplate> Templates { get; set; } = new List<RMCustomizeConnectorTemplate>();
    }
}
