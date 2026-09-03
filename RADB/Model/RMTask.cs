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
using AvePoint.RA.Contract.Task;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Model
{
    public class RMTask : BaseModel
    {
        [Key]
        [Column(TypeName = "nvarchar", Order = 1)]
        [MaxLength(1024)]
        public string Id { get; set; }

        [Column(TypeName = "int")]
        public TaskType Type { get; set; }

        [Column(TypeName = "int")]
        public RMTaskStatus Status { get; set; }

        [Column(TypeName = "bigint")]
        public long NextRunTime { get; set; }
       
        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        [Index]
        public string ScheduleId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        public string ProfileId { get; set; }

        [Column(TypeName = "bit")]
        public bool DisallowConcurrentExecution { get; set; }

        /// <summary>
        /// Concurrency control property
        /// </summary>
        [Timestamp]
        public byte[] RowVersion1 { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime LastModified { get; set; }
        [NotMapped]
        public RMTaskSchedule Schedule { get; set; }

    }
}
