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
using AvePoint.RA.Contract.JobMonitor;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Model
{
    public class RMJobQueue : BaseModel
    {

        [Key]
        [Column(TypeName = "nvarchar", Order = 1)]
        [MaxLength(1024)]
        public string MessageId { get; set; }

        [Column(TypeName = "int")]
        [Required]
        public int JobType { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string Parameters { get; set; }

        [Column(TypeName = "int")]
        [Required]
        public int JobRunType { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        [Required]
        public string JobRunBy { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string PartnerUser { get; set; }

        [Column(TypeName = "nvarchar")]

        [MaxLength(255)]
        [Index]
        public string TenantId { get; set; }

        [Column(TypeName = "bigint")]
        public long CreateTime { get; set; }
        [MaxLength(255)]
        public string ProductVersion { get; set; }

        [Column(TypeName = "int")]
        //[Index(name: "IX_RMJobQueue_Status_UpdateTime", Order = 1)]
        public int Status { set; get; }

        [Column(TypeName = "nvarchar")]
        public string ClientIP { get; set; }

        [Column(TypeName = "int")]
        public ProductType ProductType { get; set; }

        [Column(TypeName = "int")]
        public JobPriority JobPriority { get; set; }

        [Column(TypeName = "bigint")]
        //[Index(name: "IX_RMJobQueue_Status_UpdateTime", Order = 2)]
        public long UpdateTime { get; set; }

    }
}
