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
using AvePoint.RA.Contract.Discovery.Job;
using AvePoint.RA.Contract.Discovery.Model.Profile;
using AvePoint.RA.Contract.Discovery.Model.Query.Google.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;

namespace AvePoint.RA.DB.Model.Discovery.Google
{
    [Table("RMGoogleProfileInfoes")]
    public class RMDiscoveryGoogleProfileInfo : RMDiscoveryDBTable
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        public Guid Id { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(300)]
        public string Name { get; set; }

        [Column(TypeName = "int")]
        public int SizeRange { get; set; }

        [Column(TypeName = "int")]
        public RMDiscoveryGoogleSizeRangeQueryMode SizeRangeQueryMode { get; set; }

        [Column(TypeName = "int")]
        public int GreaterThanEqualWithoutInDate { get; set; }

        [Column(TypeName = "int")]
        public int LessThanEqualWithoutInDate { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(3000)]
        //Json Type: List<int>
        public string FileExtensionIdsJson { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(3000)]
        //Json Type: List<int>
        public string RuleIdsJson { get; set; }

        [Column(TypeName = "nvarchar")]
        public string SortBy { get; set; }

        [Column(TypeName = "bigint")]
        public long CreatedTime { get; set; }

        [Column(TypeName = "bigint")]
        public long ModifiedTime { get; set; }

        [Column(TypeName = "bigint")]
        public long StartScanTime { get; set; }

        [Column(TypeName = "bigint")]
        public long EndScanTime { get; set; }

        [Column(TypeName = "int")]
        public RMDiscoveryJobType ScanType { get; set; }

        [Column(TypeName = "int")]
        public RMDiscoveryJobStatus PrevScanStatus { get; set; }

        [Column(TypeName = "int")]
        public RMDiscoveryJobStatus CurrentScanStatus { get; set; }

        [Column(TypeName = "int")]
        public RMDiscoveryProfileType ProfileType { get; set; }

        [Column(TypeName = "bit")]
        public bool IsBuildIn { get; set; }

        [Column(TypeName = "bit")]
        public bool IsDefault { get; set; }
    }
}
