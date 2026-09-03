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
    public class ArchiverIndexSubInfo : BaseModel
    {
        [Key]
        [Column(TypeName = "nvarchar")]//64
        public string Id { get; set; }

        [Column(TypeName = "bigint")]
        public long RetentionTime { get; set; }

        [Column("JobId", TypeName = "nvarchar")]//255
        [Index(name: "IX_ArchiverIndexSubInfo_JobId")]
        public string SubSubJobId { get; set; }

        [Column(TypeName = "nvarchar")]//64
        public string StorageId { get; set; }

        [Column(TypeName = "nvarchar")]
        public string CurrentStorageId { get; set; }

        [Column(TypeName = "bigint")]
        public long KeepTime { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]//max
        [MaxLength]
        public string Extension { get; set; }

        [Column(TypeName = "nvarchar")]//255
        public string StorageInfo { get; set; }

        [Column(TypeName = "bigint")]
        public long MediaDataSize { get; set; }

        [Column(TypeName = "bigint")]
        public long AgentDataSize { get; set; }


        //DAO中没有，为了进一步精确merge index sub job的准确性，添加此column，用于记录每一个sub-sub job的状态,目前还没添加任何的使用，所以DAO升级数据可以暂时忽略这个值
        // DAO Migration 过来的数据 MergeIndexState 会设置为 8
        [Column(TypeName = "int")]
        public int MergeIndexState { get; set; }

        [Column(TypeName = "nvarchar")]
        public string RuleId { get; set; }

        [Column(TypeName = "int")]
        public int SourceFlag { get; set; }

        [Column(TypeName = "int")]
        public int DataFlag { get; set; }

        [Column(TypeName = "bit")]
        public bool? DAOMigrated { get; set; }
        [Column(TypeName = "int")]
        public int? RetentionCount { get; set; }
        [Column(TypeName = "int")]
        public int? RetentionSource { get; set; }
        
        [Column(TypeName = "varchar")]
        [MaxLength(50)]
        [Index(name: "IX_ArchiverIndexSubInfo_SubJobId")]
        public string? SubJobId { get; set; }
        [Column(TypeName = "int")]
        public int DeletedStatus { get; set; }
        [Column(TypeName = "bigint")]
        public long SoftDeleteTime { get; set; }
    }
}
