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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    public class RMEXOLabel : BaseModel
    {
        [Key]
        [Column(TypeName = "int", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        [Required]
        public string LabelName { get; set; }

        /// <summary>
        /// 0: None, latest savedtime used for UI display.
        /// 1: Last Job Used, just one record at any time.
        /// 2: Pending, change to type 1 when retetion job finished.
        /// 3: failed label
        /// </summary>
        [Column(TypeName = "int")]
        [Required]
        public int Status { get; set; }

        /// <summary>
        /// 0: EXO.
        /// 1: SP.
        /// </summary>
        [Column(TypeName = "int")]
        [Required]
        public int Type { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        /// <summary>
        /// When type = 1, This field equals label id in exchange online.
        /// </summary>
        public Guid LabelId { get; set; }


        [Column(TypeName = "uniqueidentifier")]
        public Guid recordId { get; set; }

        [Column(TypeName = "bigint")]
        [Required]
        public long SavedTime { get; set; }
    }
}
