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
    public class RMPhysicalRequest : BaseModel
    {
        /// <summary>
        /// Serach Key
        /// </summary>
        [Key]
        [Column(TypeName = "int", Order = 1)] 
        public int Id { get; set; }

        [Column(TypeName = "int")]
        public int Type { set; get; }

        /// <summary>
        /// Serach Key
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        [MaxLength(1024)]
        public string Title { set; get; }

        /// <summary>
        /// Serach Key
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        [MaxLength(255)]
        public string PhysicalFileId { set; get; }

        [Column(TypeName = "int")]
        public int Status { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string CreatedUserId { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string HoldUserId { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string ManagerUserId { set; get; }

        [Column(TypeName = "bigint")]
        public long CreatedTime { set; get; }

        [Column(TypeName = "bigint")]
        public long ModifiedTime { set; get; }
        /// <summary>
        /// 存储Creation 的Meta信息
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string MetaData { set; get; }

        public int HoldCategory { set; get; }

        [Column(TypeName = "int")]
        public int HoldNumber { set; get; }

        [Column(TypeName = "int")]
        public int HoldUnit { set; get; }
        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string TimeZoneId { set; get; }

        [Column(TypeName = "bit")]
        public bool IsDaylightSavingTime { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string EndTimeStr { set; get; }

        [Column(TypeName = "bigint")]
        public long EndTime { set; get; }

        [Column(TypeName = "nvarchar(max)")]
        public string Comment { set; get; }

        [Column(TypeName = "nvarchar(max)")]
        public string ReviewComment { set; get; }
        /// <summary>
        /// 存储Create Request时设置的权限信息
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string ScopePermissionInfo { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string HoldByDisplayName { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid GroupRequestId { set; get; }

        [Column(TypeName = "nvarchar(max)")]
        public string MoveInfo { set; get; }
    }
}
