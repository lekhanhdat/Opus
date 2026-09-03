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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    [RACodeReview("Allen Yin", comment: "此表过于简单,不需要索引")]
    public class RMCPGeneralSetting : BaseModel
    {
        [Key]
        [Column(TypeName="int",Order=1)]
        public int Id { get; set; }
        [Column(TypeName = "int")]
        [Required]
        public int SessionTime { get; set; }
        [Column(TypeName="int")]
        [Required]
        public int SessionTimeUnit { get; set; }
        [Column(TypeName = "nvarchar")]
        [Required]
        public string TimeZone { get; set; }
        [Column(TypeName = "bit")]
        [Required]
        public bool DayLight { get; set; }
        [Column(TypeName="int")]
        [Required]
        public int DataFormat { get; set; }
        [Column(TypeName="int")]
        [Required]
        public int TimeFormat { get; set; }
        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string RegistedEmail { get; set; }
        [Column(TypeName = "nvarchar")]
    
        [MaxLength(255)]
        public string TenantId { get; set; }

        [Column(TypeName="nvarchar")]
        public string EmailSenderDefinition { get; set; }
    }
}
