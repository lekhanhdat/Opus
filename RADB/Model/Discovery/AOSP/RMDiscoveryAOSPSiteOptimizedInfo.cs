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
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using AvePoint.RA.Contract.Explorer;

namespace AvePoint.RA.DB.Model.Discovery.AOSP
{
    [Table("RMAOSPSiteOptimizedInfoes")]
    public class RMDiscoveryAOSPSiteOptimizedInfo : RMDiscoveryDBTable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column(TypeName = "int")]
        public int Id { get; set; }

        [Index]
        [Column(TypeName = "int")]
        public int SiteId { get; set; }

        [Index]
        [Column(TypeName = "int")]
        [DefaultValue((int)SourceFlag.SharePoint)]
        public SourceFlag ContentSource { get; set; }

        [Index]
        [Column(TypeName = "uniqueidentifier")]
        public Guid SettingId { get; set; }

        [Column(TypeName = "bigint")]
        public long NextOptimizationTime { get; set; }

        [Column(TypeName = "bigint")]
        public long NextOptimizableFileTotalSize { get; set; }

        [Column(TypeName = "bigint")]
        public long NextOptimizableVersionTotalSize { get; set; }

        [Column(TypeName = "bigint")]
        public long Archived { get; set; }

        [Column(TypeName = "bigint")]
        public long Deleted { get; set; }

        [Column(TypeName = "bigint")]
        [DefaultValue(0)]
        public long LastOptimizedTime { get; set; }
        [Column(TypeName = "int")]
        public int ArchivedCount { get; set; }

        [Column(TypeName = "int")]
        public int DeletedCount { get; set; }
    }
}
