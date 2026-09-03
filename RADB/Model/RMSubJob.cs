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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Model
{
    public class RMSubJob : BaseModel
    {
        [Key]
        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        [Required]
        [Index]
        public string Id { get; set; }
        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        [Index]
        public string ParentId { get; set; }
        [Column(TypeName = "int")]
        [Required]
        [Index]
        public int JobType { get; set; }


        [Column(TypeName = "bigint")]
        public long StartTime { get; set; }

        [Column(TypeName = "bigint")]
        public long EndTime { get; set; }

        [Column(TypeName = "int")]
        [Index]
        public int Status { get; set; }

        [Column(TypeName = "float")]
        public double Progress { get; set; }

        [Column(TypeName = "float")]
        public double Weight { get; set; }

        /// <summary>
        /// job skip或失败时的comment
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string Comment { get; set; }

        /// <summary>
        /// job 最后更新时间。用于判断job是否超时
        /// </summary>
        [Column(TypeName = "bigint")]
        public long LastUpdateTime { get; set; }

        [Column(TypeName = "int")]
        [Index]
        public int Runable { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        public string AgentId { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        public string FarmId { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        public string String1 { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(64)]
        public string O365TenantId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(64)]
        public string SiteId { get; set; }

        /// <summary>
        /// 存job跑的相关Setting和Tree等信息,  会在job结束的时候清除, 想要在Get Job的时候获取需要withContext = true
        /// </summary>
        [NotMapped]
        public RMJobContext JobContext { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid DiscoveryAnalysisJobId { get; set; }
        [Column(TypeName = "int")]
        [Index]
        public int HasCheckedBackupFailed { get; set; }
    }
}
