/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 */
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    public class RMWorkspaceHold : BaseModel
    {
        [Key]
        [Column(TypeName = "varchar")]
        [MaxLength(64)]
        public string Id { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        public string HoldId { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(64)]
        public string WorkplaceId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string HoldBy { get; set; }

        [Column(TypeName = "int")]
        public int SourceType { get; set; }

        [Column(TypeName = "bigint")]
        public long ReleaseTime { get; set; }
    }
}
