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
using AvePoint.RA.Contract.Monitor;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    /// <summary>
    /// Archive table for RMJobMonitor. Schema mirrors RMJobMonitor by design.
    /// Table is created on demand via set-based SQL (see RMJobMonitorArchiverService).
    /// </summary>
    public class RMJobMonitorArchive : BaseModel
    {
        [Key]
        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        [Required]
        [Index]
        public string Id { get; set; }


        [Column(TypeName = "int")]
        [Required]
        public int JobType { get; set; }


        [Column(TypeName = "bigint")]
        public long StartTime { get; set; }


        [Column(TypeName = "bigint")]
        public long EndTime { get; set; }


        [Column(TypeName = "int")]
        [Index]
        public int Status { get; set; }


        [Column(TypeName = "int")]
        public int Progress { get; set; }

        [Column(TypeName = "float")]
        public double DoubleProgress { get; set; }

        /// <summary>
        /// 为实现skip job 判断job scope用
        /// </summary>
        [Column(TypeName = "nvarchar")]
        public string ScopeId { get; set; }

        /// <summary>
        /// report的profile id， 应该设置个外键
        /// </summary>
        [Column(TypeName = "int")]
        public int? ProfileId { get; set; }

        /// <summary>
        /// job skip或失败时的comment
        /// </summary>
        [Column(TypeName = "nvarchar(MAX)")]
        public string Comment { get; set; }

        /// <summary>
        /// 标示跑job的用户
        /// </summary>
        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        public string UserName { get; set; }

        /// <summary>
        /// job 最后更新时间。用于判断job是否超时
        /// </summary>
        [Column(TypeName = "bigint")]
        public long LastUpdateTime { get; set; }

        /// <summary>
        /// 如果存在子job, 记录子job的数量.
        /// </summary>
        [Column(TypeName = "int")]
        public int SubJobCount { set; get; }

        /// <summary>
        /// 用于存储运行job节点的container id，
        /// 对于report job为创建profile的user id.如果该字段是空，则只有super admin可以看到
        /// 对于download report job为运行job的user id
        /// </summary>
        [Column(TypeName = "nvarchar")]
        public string ContainerId { get; set; }
        /// <summary>
        /// for recenter job NodeType
        /// </summary>
        [Column(TypeName = "int")]
        public int NodeType { set; get; }
        public MonitorExceptionType ExceptionType { get; set; }

        /// <summary>
        /// ReCenter标示跑job的用户
        /// </summary>
        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        public string AdditionalInformation { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid DiscoveryMainJobId { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid DiscoveryJobId { get; set; }

        [Column(TypeName = "bit")]
        public bool? DAOMigrated { set; get; }

        [Column(TypeName = "int")]
        public int RestoreStatisticStatus { set; get; }

        [Column(TypeName = "nvarchar(MAX)")]
        public string Extension { get; set; }
        [Column(TypeName = "nvarchar(MAX)")]
        public string JobConflictExtension { get; set; }
    }
}
