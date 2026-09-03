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
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace AvePoint.RA.DB.Model.Discovery.AOSP
{
    [Table("RMAOSPOptimizationSettingsInfo")]
    public class RMDiscoveryAOSPOptimizationSettingsInfo : RMDiscoveryDBTable
    {
        [Key]
        [Column(TypeName = "bigint")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
        [Column(TypeName = "uniqueidentifier")]
        public Guid SettingId { get; set; }
        [Column(TypeName = "int")]
        public int Type { get; set; }
        [Column(TypeName = "bigint")]
        public long NextTime { get; set; }
        [Column(TypeName = "nvarchar")]
        public string Setting { get; set; }
        [Column(TypeName = "int")]
        public int Status { get; set; }
        [Column(TypeName = "bit")]
        [DefaultValue(0)]
        public bool IsHandle { get; set; }

        [Column(TypeName = "nvarchar")]
        public string JobId { get; set; }
    }

    public class RMDiscoverAOSPOptimizationJobInfo
    {
        public RMDiscoveryAOSPTenantInfo o365Info { get; set; }
        public RMDiscoveryAOSPOptimizationSettingsInfo settingInfo { get; set; }
    }

    //public class RMDiscoverOptimizationPreScanJobInfo
    //{
    //    public RMDiscoveryOffice365OptimizationSetting SettingInfo { get; set; }
    //    public List<long> SiteIds { get; set; }
    //}
}
