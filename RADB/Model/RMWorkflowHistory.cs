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
    public class RMWorkflowHistory : BaseModel
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        /// <summary>
        /// realted object id
        /// </summary>
        [Column(TypeName = "uniqueidentifier")]
        public Guid ObjectId { get; set; }

        /// <summary>
        /// workflow instance id
        /// </summary>
        [Column(TypeName = "uniqueidentifier")]
        public Guid InstanceId { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid StepId { get; set; }

        [Column(TypeName = "nvarchar")]
        [Required]
        [MaxLength(200)]
        public string StepName { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid ReviewedId { get; set; }

        [Required]
        public DateTime ReviewedTime { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        [Required]
        public string Comment { get; set; }

    }

}
