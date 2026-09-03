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
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Profile;
using System.Data.Entity;

namespace AvePoint.RA.DB.Core.Discovery.Context
{
    public partial class RMDiscoveryDBEFContext
    {
        public DbSet<RMDiscoveryAOSPRuleInfo> AOSPRuleInfoes { get; set; }

        public DbSet<RMDiscoveryAOSPSizeRange> AOSPSizeRanges { get; set; }

        public DbSet<RMDiscoveryAOSPMainJob> AOSPMainJobs { get; set; }

        public DbSet<RMDiscoveryAOSPWithoutInDate> AOSPWithoutInDateList { get; set; }

        public DbSet<RMDiscoveryAOSPDiscoveryJob> AOSPDiscoveryJobs { get; set; }

        public DbSet<RMDiscoveryAOSPTenantInfo> AOSPTenantInfoes { get; set; }

        public DbSet<RMDiscoveryAOSPAnalysisJob> AOSPAnalysisJobs { get; set; }

        public DbSet<RMDiscoveryAOSPFileExtension> AOSPFileExtensions { get; set; }

        public DbSet<RMDiscoveryAOSPContainerInfo> AOSPContainerInfoes { get; set; }

        public DbSet<RMDiscoveryAOSPSiteInfo> AOSPSiteInfoes { get; set; }

        public DbSet<RMDiscoveryAOSPAggregateTotalData> AOSPAggregateTotalDataList { get; set; }

        public DbSet<RMDiscoveryAOSPTenantConfiguration> AOSPTenantConfigurationInfoes { get; set; }

        public DbSet<RMDiscoveryAOSPOptimizationSettingsInfo> AOSPOptimizationSettingsInfos { get; set; }

        public DbSet<RMDiscoveryAOSPSiteOptimizationMappingInfo> AOSPSiteOptimizationMappingInfos { get; set; }

        public DbSet<RMDiscoveryAOSPSiteOptimizedInfo> AOSPSiteOptimizedInfoes { get; set; }

        public DbSet<RMDiscoveryAOSPConfiguration> AOSPConfigurations { get; set; }
    }
}
