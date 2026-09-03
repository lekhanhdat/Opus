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
using AvePoint.RA.Contract.TemplateManagement;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    public class RMTemplateRelationship : BaseModel
    {
        /// <summary>
        /// id path from suite to current template seprated by '/'.
        /// the first one is suite unique id, others are template id(not unique it), e.g, 6FEECEA2-2076-4557-AE9C-A90F9EB91617/, 6FEECEA2-2076-4557-AE9C-A90F9EB91617/1/,6FEECEA2-2076-4557-AE9C-A90F9EB91617/1/2/
        /// </summary>
        [Key]
        [Column(TypeName = "nvarchar",Order =1)]
        [MaxLength(2048)]
        public string IdPath { get; set; }

        [Key]
        [Column(TypeName = "int", Order =2)]
        public int Distance { set; get; }

        [Required]
        [Column(TypeName = "uniqueidentifier")]
        [Index]
        public Guid Ancestor { get; set; }

        [Required]
        [Column(TypeName = "uniqueidentifier")]
        public Guid Descendant { get; set; }
        
        /// <summary>
        /// Template type of Descendant
        /// </summary>
        [Column]
        [Required]
        public TemplateType TemplateType { get;  set; }
    }
}
