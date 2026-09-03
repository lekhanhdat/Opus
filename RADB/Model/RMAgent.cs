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
using AvePoint.GCommon.Contract.CentralAdmin.Object.SharedServices.SearchService;
using AvePoint.Hybrid.Contract.Object;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Model
{
    public class RMAgent : BaseModel
    {
        [Key]
        [Column(TypeName = "uniqueidentifier")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        [Required]
        public string Name { get; set; }

        [Required]
        public SourceType  SourceType { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        [Required]
        public string ClientId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        [Required]
        public string InstallationCode { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string ServerName { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        [Required]
        public string AuthCode { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        [Required]
        public string Version { get; set; }
        
        [Required]
        public ServiceErrors Errors { get; set; }

        [Required]
        public ServiceStatus Status { get; set; }

        [Required]
        [Column(TypeName = "uniqueidentifier")]
        public Guid CertificateId { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        public string Description { get; set; }


        [Column(TypeName = "int")]
        public int JobCounts { set; get; }

        [Column(TypeName = "bigint")]
        public long TimeStamp { set; get; }

        [Column(TypeName = "bigint")]
        public long CPUHZ { set; get; }

        [Column(TypeName = "bigint")]
        public long AvailableCPU { set; get; }

        [Column(TypeName = "bigint")]
        public long TotalMemory { set; get; }

        [Column(TypeName = "bigint")]
        public long AvailableMemeory { set; get; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(1024)]
        public string OSName { set; get; }

        [Column(TypeName = "bigint")]
        public long OSVersionNumber { set; get; }

        [Column(TypeName = "varchar")]
        [MaxLength(64)]
        public string FarmId { get; set; }

        [Column(TypeName = "bit"), DefaultValue(0)]
        public bool IsSupportUpgrade { get; set; }

        [Column(TypeName = "bit"), DefaultValue(0)]
        public bool CollectLog { get; set; }

        [Column(TypeName = "nvarchar")]
        [MaxLength(255)]
        public string DCInternalName { get; set; }
    }
}
