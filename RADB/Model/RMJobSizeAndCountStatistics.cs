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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    public class RMJobSizeAndCountStatistics : BaseModel
    {
        [Key]
        [Column(TypeName = "uniqueidentifier", Order = 1)]
        public Guid Id { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string JobName { get; set; }

        [Column(TypeName = "int")]
        public int JobType { get; set; }
        [Column(TypeName = "int")]
        public int KeepDataOption { get; set; }
        [Column(TypeName = "bigint")]
        public long Size { get; set; }

        [Column(TypeName = "int")]
        public int EndUserJobCount { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]
        public string Extend { get; set; }
        [Column(TypeName = "int")]
        public int LicenceType { get; set; }
        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string JobId { get; set; }
        [Column(TypeName = "bigint")]
        public long StatisticsTime { get; set; }
        [Column(TypeName = "int")]
        public int Status { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string SiteId { get; set; }
    }
}
