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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    public class RMHold : BaseModel
    {
        [Key]
        [Column(TypeName = "nvarchar", Order = 1)]
        [MaxLength(1024)]
        public string Id { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string Name { get; set; }

        [Column(TypeName = "int")]
        public int HoldDateType { get; set; }

        [Column(TypeName = "int")]
        public int Number { get; set; }

        [Column(TypeName = "int")]
        public int HoldUnit { get; set; }

        [Column(TypeName = "bigint")]
        public long CalendarTime { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string TimeZoneId { get; set; }

        [Column(TypeName = "bit")]
        public bool IsDaylightSaving { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string Description { get; set; }

        [Column(TypeName = "bigint")]
        public long CreateTime { get; set; }

        [Column(TypeName = "int")]
        public int Type { get; set; }

        [Column(TypeName = "bit")]
        public bool IsEmailNotificationEnabled { get; set; }

        [Column(TypeName = "int")]
        public int ReminderDurationDays { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string EmailRecipients { get; set; }

        [Column(TypeName = "bigint")]
        public long LastSentEmailTime { get; set; }

        [Column(TypeName = "bit")]
        public bool IsHoldManagerEmailNotificationEnabled { get; set; }
    }
}
