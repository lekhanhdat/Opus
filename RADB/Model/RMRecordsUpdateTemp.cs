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
    // !!! Important note:
    // When adding new fields, we must add field as nullable, because this table is used in CSD service
    // Don't change existing fields' name or type, otherwise it will break CSD service
    public class RMRecordsUpdateTemp : BaseModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        [Column(TypeName = "int")]
        public int Id { get; set; }
        [Index]
        [Required]
        [Column(TypeName = "nvarchar")]
        [StringLength(255)]
        public string TempJobId { get; set; }
        [Column(TypeName = "nvarchar(max)")]
        public string FailedRecords { get; set; }

        [Column(TypeName = "int")]
        [Index]
        public int Status { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string ProcessRecords { get; set; }

        [Column(TypeName = "datetime2")]
        [Required]
        public DateTime TimeStamp { get; set; }

        [Column(TypeName = "bit")]
        public bool Waiting4OtherSourceChangeTerm { get; set; }

        [Column(TypeName = "bit")]
        public bool WaitingChangeLabel { get; set; }
    }
}
