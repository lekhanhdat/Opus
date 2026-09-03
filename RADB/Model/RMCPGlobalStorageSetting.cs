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
using AvePoint.GCommon.Contract.GranularBackup.Object;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.RA.Contract.CodeView;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AvePoint.RA.DB.Model
{
    [RACodeReview("Allen Yin", comment: "此表过于简单,不需要索引")]
    public class RMCPGlobalStorageSetting: BaseModel
    {
        [Key]
        [Column(TypeName = "int", Order = 1)]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { set; get; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid StoragePolicyId { get; set; }

        [Column(TypeName = "nvarchar")]
        public string StoragePolicyName { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid ExportLocationId { get; set; }

        [Column(TypeName = "nvarchar")]
        public string ExportLocationName { get; set; }

        [Column(TypeName = "uniqueidentifier")]
        public Guid SecurityProfileId { get; set; }

        [Column(TypeName = "nvarchar")]
        public string SecurityProfileName { get; set; }

        [Column(TypeName = "bit")]
        public bool UseCompression { get; set; }

        [Column(TypeName = "bit")]
        public bool UseEncryption { get; set; }

        [Column(TypeName = "int")]
        public int CompressionSpeed { get; set; }

        public DataSecurity CompressionMethod { get; set; }

        public DataSecurity EncryptionMethod { get; set; }

        [Column(TypeName = "nvarchar(max)")]
        public string Extentions { get; set; }
    }
}
