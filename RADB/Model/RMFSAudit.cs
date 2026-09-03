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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    public class RMFSAudit : BaseModel
    {
        [Key]
        [Column(TypeName = "int", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column(TypeName = "int")]
        public int AuditType { get; set; }

        [Column(TypeName = "int")]
        public int AuditLevel { get; set; }

        [Index("IX_RMFSAudit_ConnGroup_Time", Order = 1)]
        [Column(TypeName = "nvarchar")]
        [MaxLength(64)]
        public string ConnectionGroupId { get; set; }

        [Index("IX_RMFSAudit_Conn_Time", Order = 1)]
        [Column(TypeName = "nvarchar")]
        [MaxLength(64)]
        public string ConnectionId { get; set; }

        [Index("IX_RMFSAudit_ItemId")]
        [Column(TypeName = "nvarchar")]
        [MaxLength(256)]
        public string ItemId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(450)]
        public string FullPath { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(450)]
        public string PreviousPath { get; set; }

        [Column(TypeName = "nvarchar(MAX)")]
        public string Content { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(64)]
        public string ClientIP { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(64)]
        public string ExecutedBy { get; set; }

        [Index]                                          
        [Index("IX_RMFSAudit_Conn_Time", Order = 2)]      
        [Index("IX_RMFSAudit_ConnGroup_Time", Order = 2)] 
        [Column(TypeName = "bigint")]
        [Required]
        public long ExecutedTime { get; set; }

        [Column(TypeName = "int")]
        public int Status { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(512)]
        public string ObjectName { get; set; }
    }
}