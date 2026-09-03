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
using AvePoint.RA.DB.Model.Discovery.Profile;
using AvePoint.RA.DB.Model.Discovery.Salesforce;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.DB.Model.Discovery.Plan;

namespace AvePoint.RA.DB.Core.Discovery.Context
{
    public partial class RMDiscoveryDBEFContext
    {
        public DbSet<RMDiscoveryOffice365MainJob> Office365MainJobs { get; set; }

        public DbSet<RMDiscoveryOffice365DiscoveryJob> Office365DiscoveryJobs { get; set; }

        public DbSet<RMDiscoveryOffice365AnalysisJob> Office365AnalysisJobs { get; set; }

        public DbSet<RMDiscoveryOffice365FileExtension> Office365FileExtensions { get; set; }

        public DbSet<RMDiscoveryOffice365TenantInfo> Office365TenantInfoes { get; set; }

        public DbSet<RMDiscoveryOffice365ContainerInfo> Office365ContainerInfoes { get; set; }

        public DbSet<RMDiscoveryOffice365SiteInfo> Office365SiteInfoes { get; set; }

        public DbSet<RMDiscoveryOffice365RuleInfo> Office365RuleInfoes { get; set; }

        public DbSet<RMDiscoveryOffice365SizeRange> Office365SizeRanges { get; set; }

        public DbSet<RMDiscoveryOffice365WithoutInDate> Office365WithoutInDateList { get; set; }

        public DbSet<RMDiscoveryOffice365ContainerInactiveData> Office365ContainerInactiveDataList { get; set; }

        public DbSet<RMDiscoveryOffice365BasicInactiveData> Office365BasicInactiveDataList { get; set; }

        public DbSet<RMDiscoveryOffice365ContainerRotData> Office365ContainerRotDataList { get; set; }

        public DbSet<RMDiscoveryOffice365BasicRotData> Office365BasicRotDataList { get; set; }

        public DbSet<RMDiscoveryOffice365AggregateTotalData> Office365AggregateTotalDataList { get; set; }

        public DbSet<RMDiscoveryOffice365OptimizationSettingsInfo> Office365OptimizationSettingsInfos { get; set; }

        public DbSet<RMDiscoveryOffice365SiteOptimizationMappingInfo> Office365SiteOptimizationMappingInfos { get; set; }

        public DbSet<RMDiscoveryOffice365SiteOptimizedInfo> Office365SiteOptimizedInfoes { get; set; }

        public DbSet<RMDiscoveryOffice365ContainerOptimizedInfo> Office365ContainerOptimizedInfoes { get; set; }

        public DbSet<RMDiscoveryOffice365TenantConfiguration> Office365O365TenantConfigurationInfoes { get; set; }

        public DbSet<RMDiscoveryOffice365SiteRotData> Office365SiteRotDataList { get; set; }

        public DbSet<RMDiscoveryOffice365SiteInactiveData> Office365SiteInactiveDataList { get; set; }

        public DbSet<RMDiscoveryOffice365BasicRuleLevelRotData> Office365BasicRuleLevelsRotDataList { get; set; }

        public DbSet<RMDiscoveryOffice365BasicCategoryLevelRotData> Office365BasicCategoryLevelsRotDataList { get; set; }

        public DbSet<RMDiscoveryOffice365BasicRootLevelRotData> Office365BasicRootLevelsRotDataList { get; set; }

        public DbSet<RMDiscoveryOffice365ContainerRuleLevelRotData> Office365ContainerRuleLevelsRotDataList { get; set; }

        public DbSet<RMDiscoveryOffice365ContainerCategoryLevelRotData> Office365ContainerCategoryLevelsRotDataList { get; set; }

        public DbSet<RMDiscoveryOffice365ContainerRootLevelRotData> Office365ContainerRootLevelsRotDataList { get; set; }

        public DbSet<RMDiscoveryOffice365ProfileInfo> Office365ProfileInfoes { get; set; }

        public DbSet<RMDiscoveryProfileFailedInfo> Office365ProfileFailedInfoes { get; set; }
        public DbSet<RMDiscoveryPlanProfile> PlanProfiles { get; set; }
        public DbSet<RMDiscoveryPlanSiteMapping> PlanSiteMappings { get; set; }
        public DbSet<RMDiscoveryPlanDalJob> PlanDalJobs { get; set; }
        public DbSet<RMDiscoveryDalJobConfiguration> PlanDalJobConfigurations { get; set; }
        
    }
}
