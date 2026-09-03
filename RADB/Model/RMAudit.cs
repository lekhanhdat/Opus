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
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Model
{
    [RACodeReview("Allen Yin",comment:"ExecuteOn 需要设置成聚集索引，其他action，category等column，如果后续需要有filter功能再酌情添加索引")]
    public class RMAudit : BaseModel
    {
        [Key]
        [Column(TypeName = "int", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        /// <summary>
        /// 执行操作的用户名
        /// </summary>
        [Column(TypeName = "nvarchar")]
        public string UserId { get; set; }

        /// <summary>
        /// 用户所在组
        /// </summary>
        [Column(TypeName = "nvarchar")]
        public string Role { get; set; }

        /// <summary>
        /// 该模块的代号
        /// </summary>
        [Column(TypeName = "int")]
        public int Module { get; set; }

        public int Category { get; set; }

        /// <summary>
        /// Xml格式的详细信息
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string Content { get; set; }

        /// <summary>
        /// 该操作的代号（如删除操作）
        /// </summary>
        [Column(TypeName = "int")]
        public int Action { get; set; }

        /// <summary>
        /// 该操作成功与否
        /// </summary>
        [Column(TypeName = "int")]
        public int Status { get; set; }//0:Successfule,1:Failed,2:Exception

        /// <summary>
        /// 时间
        /// </summary>
        [Column(TypeName = "bigint")]
        [Required]
        public long ExecuteOn { get; set; }

        [Column(TypeName = "nvarchar")]
        public string Method { get; set; }

        /// <summary>
        /// 操作对象
        /// </summary>
        [Column(TypeName = "nvarchar")]
        public string Object { get; set; }

        [Column(TypeName = "nvarchar")]
        public string UserName { get; set; }
        
        [Column(TypeName = "nvarchar")]
        public string ClientIP { get; set; }
    }
}
