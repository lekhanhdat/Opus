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
using System.Data.Entity;
using AvePoint.RA.DB.Model.Discovery.Google;

namespace AvePoint.RA.DB.Core.Discovery.Context
{
    public partial class RMDiscoveryDBEFContext
    {
        public DbSet<RMDiscoveryGoogleSizeRange> GoogleSizeRanges { get; set; }

        public DbSet<RMDiscoveryGoogleWithoutInDate> GoogleWithoutInDateList { get; set; }

        public DbSet<RMDiscoveryGoogleRuleInfo> GoogleRuleInfoes { get; set; }

        public DbSet<RMDiscoveryGoogleOrganizationInfo> GoogleOrganizationInfoes { get; set; }

        public DbSet<RMDiscoveryGoogleMainJob> GoogleMainJobs { get; set; }

        public DbSet<RMDiscoveryGoogleDiscoveryJob> GoogleDiscoveryJobs { get; set; }

        public DbSet<RMDiscoveryGoogleAnalysisJob> GoogleAnalysisJobs { get; set; }

        public DbSet<RMDiscoveryGoogleDriveRuleLevelRotData> GoogleDriveRuleLevelsRotDataList { get; set; }

        public DbSet<RMDiscoveryGoogleContainerInfo> GoogleContainerInfoes { get; set; }

        public DbSet<RMDiscoveryGoogleDriveInfo> GoogleDriveInfoes { get; set; }

        public DbSet<RMDiscoveryGoogleFileExtension> GoogleFileExtensions { get; set; }
       
        public DbSet<RMDiscoveryGoogleExecutionInfo> GoogleExecutionInfoList { get; set; }

        //public DbSet<RMDiscoveryGoogleBasicRotData> GoogleBasicRotDataList { get; set; }

        //public DbSet<RMDiscoveryGoogleContainerRotData> GoogleContainerRotDataList { get; set; }

        //public DbSet<RMDiscoveryGoogleDriveRotData> GoogleDriveRotDataList { get; set; }

        //public DbSet<RMDiscoveryGoogleContainerInactiveData> GoogleContainerInactiveDataList { get; set; }

        //public DbSet<RMDiscoveryGoogleBasicInactiveData> GoogleBasicInactiveDataList { get; set; }

        //public DbSet<RMDiscoveryGoogleDriveInactiveData> GoogleDriveInactiveDataList { get; set; }

        public DbSet<RMDiscoveryGoogleAggregateTotalData> GoogleAggregateTotalDataList { get; set; }

        //public DbSet<RMDiscoveryGoogleBasicRuleLevelRotData> GoogleBasicRuleLevelsRotDataList { get; set; }

        //public DbSet<RMDiscoveryGoogleBasicCategoryLevelRotData> GoogleBasicCategoryLevelsRotDataList { get; set; }

        //public DbSet<RMDiscoveryGoogleBasicRootLevelRotData> GoogleBasicRootLevelsRotDataList { get; set; }

        //public DbSet<RMDiscoveryGoogleContainerRuleLevelRotData> GoogleContainerRuleLevelsRotDataList { get; set; }

        //public DbSet<RMDiscoveryGoogleContainerCategoryLevelRotData> GoogleContainerCategoryLevelsRotDataList { get; set; }

        //public DbSet<RMDiscoveryGoogleContainerRootLevelRotData> GoogleContainerRootLevelsRotDataList { get; set; }

        public DbSet<RMDiscoveryGoogleProfileInfo> GoogleProfileInfoes { get; set; }

        public DbSet<RMDiscoveryGoogleProfileFailedInfo> GoogleProfileFailedInfoes { get; set; }
    }
}
