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
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Google;
using AvePoint.RA.Contract.Explorer;

namespace AvePoint.RA.DB.Model.Discovery.Google
{
    [Table("RMGoogleDriveInfoes")]
    public class RMDiscoveryGoogleDriveInfo : RMDiscoveryDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(TypeName = "int")]
        public int Id { get; set; }

        [Index]
        [Column(TypeName = "nvarchar")]
        [MaxLength(3000)]
        public string DriveId { get; set; }

        [Index]
        [Column(TypeName = "nvarchar")]
        [MaxLength(3000)]
        public string DriveName { get; set; }

        [Index]
        [Column(TypeName = "int")]
        public RMDiscoveryGoogleDriveType DriveType { get; set; }

        [Index]
        [Column(TypeName = "int")]
        public int ContainerId { get; set; }

        [Column(TypeName = "bigint")]
        public long FileTotalSize { get; set; }

        [Column(TypeName = "bigint")]
        public long FileSumCount { get; set; }

        [Column(TypeName = "bigint")]
        [DefaultValue(0L)]
        public long VersionTotalSize { get; set; }

        [Column(TypeName = "int")]
        [DefaultValue(0L)]
        public int MaxFileAge { get; set; }

        [Column(TypeName = "bigint")]
        public long CreateTime { get; set; }

        [Column(TypeName = "bigint")]
        public long ModifiedTime { get; set; }

        [Column(TypeName = "bit")]
        public bool IsRemoved { get; set; }

        [Index]
        [Column(TypeName = "bit")]
        [DefaultValue(0)]
        public bool Hidden { get; set; }
    }
}



