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
    public class RMRule: BaseModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Column(TypeName = "int")]
        public int Id { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        [Index]
        public Guid RuleId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(2000)]
        public string RuleName { get; set; }
        [Column(TypeName = "int")]
        public int RuleLevel { get; set; }
        [Column(TypeName = "int")]
        public int DisposalAction { get; set; }
        [Column(TypeName = "bit")]
        public bool DeleteRecords { get; set; }
        [Column(TypeName = "bit")]
        public bool IsRemoved { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        [MaxLength(5000)]
        public string Description { get; set; }

        [Column(TypeName = "bigint")]
        public long ModifyTime { get; set; }
        [Column(TypeName = "int")]
        public int ExchangeDisposalAction { get; set; }
        [Column(TypeName = "int")]
        public int PhysicalDisposalAction { get; set; }
        [Column(TypeName = "int")]
        public int FSDisposalAction { get; set; }
        [Column(TypeName = "int")]
        public int SPLocalDisposalAction { get; set; }
        [Column(TypeName = "int")]
        public int OneDriveDisposalAction { get; set; }
        [Column(TypeName = "int")]
        public int AzureFileDisposalAction { get; set; }
        [Column(TypeName = "int")]
        public int BoxDisposalAction { get; set; }
        [Column(TypeName = "int")]
        public int ConnectorDisposalAction { get; set; }
        [Column(TypeName = "int")]
        public int GoogleDriveDisposalAction { get; set; }
        [Column(TypeName = "int")]
        public int TeamsDisposalAction { get; set; }
        [Column(TypeName = "nvarchar(max)")]
        public string Extension { get; set; }
        [Column(TypeName = "nvarchar(max)")]
        [MaxLength(5000)]
        public string DisposalClass { get; set; }
        [Column(TypeName = "int")]
        public int ModelType { get; set; }

        [Column(TypeName = "bit")]
        public bool? DAOMigrated { get; set; }
    }
}
