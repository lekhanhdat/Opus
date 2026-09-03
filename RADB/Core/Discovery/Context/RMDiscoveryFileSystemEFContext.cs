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
using AvePoint.RA.DB.Model.Discovery.FileSystem;

namespace AvePoint.RA.DB.Core.Discovery.Context
{
    public partial class RMDiscoveryDBEFContext
    {
        public DbSet<RMDiscoveryFSSizeRange> FSSizeRanges { get; set; }

        public DbSet<RMDiscoveryFSWithoutInDate> FSWithoutInDateList { get; set; }

        public DbSet<RMDiscoveryFSRuleInfo> FSRuleInfoes { get; set; }

        public DbSet<RMDiscoveryFSMainJob> FSMainJobs { get; set; }

        public DbSet<RMDiscoveryFSDiscoveryJob> FSDiscoveryJobs { get; set; }

        public DbSet<RMDiscoveryFSAnalysisJob> FSAnalysisJobs { get; set; }

        public DbSet<RMDiscoveryFSConnectionRuleLevelRotData> FSConnectionRuleLevelsRotDataList { get; set; }

        public DbSet<RMDiscoveryFSContainerInfo> FSContainerInfoes { get; set; }

        public DbSet<RMDiscoveryFSConnectionInfo> FSConnectionInfoes { get; set; }

        public DbSet<RMDiscoveryFSFileExtension> FSFileExtensions { get; set; }

        public DbSet<RMDiscoveryFSExecutionInfo> FSExecutionInfoList { get; set; }

        public DbSet<RMDiscoveryFSAggregateTotalData> FSAggregateTotalDataList { get; set; }

        public DbSet<RMDiscoveryFSTagRuleInfo> FSTagRuleInfoes { get; set; }
    }
}
