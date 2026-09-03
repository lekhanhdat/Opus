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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    public class RMMailbox : BaseModel
    {
        [Key]
        [Column(TypeName = "char")]
        [MaxLength(36)]
        public string Id { set; get; }

        [Index]
        [Required]
        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string Name { get; set; }

        [Index]
        [Column(TypeName = "char")]
        [MaxLength(36)]
        public string ParentId { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(64)]
        public string ObjectId { get; set; }

        [Column(TypeName = "int")]
        public int State { get; set; }

        [Index]
        [Required]
        [Column(TypeName = "int")]
        public int NodeLevel { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        public string UserName { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(64)]
        public string SPVersion { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string ServiceUrl { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(64)]
        public string TenantId { get; set; }

        [Column(TypeName = "int")]
        public int AuthType { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(36)]
        public string ServiceAccountId { get; set; }

        [Column(TypeName = "int")]
        public int AppType { get; set; }

        [Column(TypeName = "int")]
        public int MailboxType { get; set; }

        [Column(TypeName = "int")]
        public int ScanSource { get; set; }

        [Column(TypeName = "bit")]
        public bool FromDAO { get; set; }

        [Column(TypeName = "bigint")]
        public long CreateTime { get; set; }

        [Column(TypeName = "bigint")]
        public long ModifiedDate { get; set; }

        [Column(TypeName = "varchar")]
        [MaxLength(64)]
        public string AosId { get; set; }
    }
}
