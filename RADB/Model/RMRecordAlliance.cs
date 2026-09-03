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
    public class RMRecordAlliance : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [Column(TypeName = "uniqueidentifier")]
        public Guid RecordsId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string HoldId { get; set; }

        [Column(TypeName = "bigint")]
        [Index(name: "IX_HoldReleaseTime")]
        public long HoldReleaseTime { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string HoldBy { get; set; }

        [Column(TypeName = "int")]
        public int AllianceType { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid BoxId { get; set; }
        [Column(TypeName = "uniqueidentifier")]
        public Guid LocationId { get; set; }
        [Column(TypeName = "int")]
        public int Level { set; get; }
    }
    /// <summary>
    /// Personal hold使用
    /// </summary>
    public class RMRecordLoanAlliance : BaseModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid RecordsId { get; set; }

        [Column(TypeName = "bigint")]
        [Index(name: "IX_HoldReleaseTime")]
        public long HoldReleaseTime { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string HoldBy { get; set; }
        

        [Column(TypeName = "uniqueidentifier")]
        public Guid ParentId { get; set; }
    }

}
