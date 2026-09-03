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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    [RACodeReview("Allen Yin",comment:"type 加一个非聚集索引，id和jobmonitor表建立外键关系")]
    public class RMProfile : BaseModel
    {
        [Key]
        [Column(TypeName = "int", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string Description { get; set; }

        [Required]
        public int Type { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string Extension1 { get; set; }
        [Column(TypeName = "nvarchar(max)")]
        public string Extension2 { get; set; }

        [Column(TypeName = "bigint")]
        public long Modified { get; set; }

        [Column(TypeName = "bit")]
        [Required]
        public bool IsRemoved { get; set; }

        [Column(TypeName = "bit")]
        public bool IsCreated { get; set; }

        [Column(TypeName = "bit")]
        public bool IsDestoryed { get; set; }

        [Required]
        public int RangeType { get; set; }


        /// <summary>
        /// 用于存储运行job节点的container id，对于report job为创建profile的user id
        /// </summary>
        [Column(TypeName = "nvarchar")]
        public string CreateProfileLogonUserId { get; set; }

        [Column(TypeName = "int")]
        public SourceFlag Source { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string ScheduleId { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string Extension3 { get; set; }

        [Column(TypeName = "int")]
        public int? ObjectLevel { get; set; }
    }
}
