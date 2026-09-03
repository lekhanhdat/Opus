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
using AvePoint.RA.Contract.CodeView;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Monitor;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    public class RMMyhubReportJob : BaseModel
    {
        [Key]
        [Column(Order = 0, TypeName = "nvarchar")]
        [MaxLength(255)]
        [Required]
        [Index]
        public string ScopeId { get; set; }

        [Key]
        [Column(Order = 1, TypeName = "nvarchar")]
        [MaxLength(1024)]
        [Required]
        public string JobId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string RecordId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        public string UserId { get; set; }

        [Column(TypeName = "int")]
        public MyhubReportJobStatus Status { get; set; }

        [Column(TypeName = "int")]
        public MyhubReportJobType JobType { get; set; }

        [Column(TypeName = "bigint")]
        public long ExecutedTime { get; set; }
    }

    public enum MyhubReportJobStatus
    {
        None = -1,
        Wait = 0,
        InProgress = 1,
        Finished = 2,
        Failed = 3,
        FinishWithException = 4,
        Stopped = 5,
        Skipped = 6,
        Stopping = 7,
        Calculating = 8,
    }

    public enum MyhubReportJobType
    {
        HistoryContent = 0,
        DownloadRCCReport = 1
    }
}