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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.JPMC;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.Archiver;
using AvePoint.RA.SharePoint.ArchiverCommon;
using AvePoint.RA.SharePoint.RMCustomization4JPMC.Scan.Base;
using AvePoint.RA.SharePoint.RMCustomization4JPMC.Scan.Implement;

namespace AvePoint.RA.SharePoint.RMCustomization4JPMC.Scan
{
    internal static class SiteMetricsScannerSelector4JPMC
    {
        private const int ChangeLogRetentionDays = 60;
        private static readonly RALogger mLog = RALogger.GetInstance(typeof(SiteMetricsScannerSelector4JPMC));

        private static IRMNodeFlagDao RMNodeFlagDao => (IRMNodeFlagDao)PlatformWindsorManager.GetService(typeof(IRMNodeFlagDao));

        internal static SiteMetricsScanner4JPMCBase Create(ScanJobSettings scanJobSettings, JPMCTenantConfig jpmcConfig, string siteUrl = "")
        {
            InitializeIncrementalDiscoverState(scanJobSettings);

            if (scanJobSettings?.Configuration?.UseIncrementalDiscover == true)
            {
                var incrementalScanner = new SiteMetricsIncrementalScanner4JPMC(scanJobSettings, jpmcConfig, siteUrl);
                if (incrementalScanner.ShouldFallbackToFullScan)
                {
                    mLog.Warn("Incremental scan database download failed; use full scan instead.");
                    incrementalScanner.Dispose();
                    return new SiteMetricsScanner4JPMC(scanJobSettings, jpmcConfig, siteUrl);
                }

                return incrementalScanner;
            }

            return new SiteMetricsScanner4JPMC(scanJobSettings, jpmcConfig, siteUrl);
        }

        private static void InitializeIncrementalDiscoverState(ScanJobSettings scanJobSettings)
        {
            if (scanJobSettings?.Configuration == null)
            {
                return;
            }

            var configuration = scanJobSettings.Configuration;
            var node = AvePoint.RA.Common.Util.RMDtoConverter.ConvertRMTree2SPTree(scanJobSettings.TreeNode);

            if (node == null || node.Level != NodeLevel.SiteCollection)
            {
                ResetIncrementalDiscoverRange(configuration);
                return;
            }

            var rmNodeFlagDao = RMNodeFlagDao;
            if (rmNodeFlagDao == null)
            {
                mLog.Warn("RMNodeFlagDao is null, skip incremental discover initialization.");
                return;
            }

            if (!Guid.TryParse(node.SPObjectId, out var siteId))
            {
                mLog.Warn($"Invalid site collection object id for node {node.FullPath}, skip incremental discover initialization.");
                return;
            }

            var groupNode = SPTreeNodeManagement.GetGroupNode(node);
            if (groupNode == null || !Guid.TryParse(groupNode.SPObjectId, out var groupId))
            {
                mLog.Warn("GroupId is empty, skip incremental discover initialization.");
                return;
            }

            long endTicks = configuration.IncrementalDiscoverEndTimeTicks;
            if (endTicks <= DateTime.MinValue.Ticks)
            {
                var now = DateTime.UtcNow;
                configuration.IncrementalDiscoverEndTimeTicks = now.Ticks;
                endTicks = configuration.IncrementalDiscoverEndTimeTicks;
            }

            try
            {
                long startTicks = rmNodeFlagDao.GetCollectionTime((int)NodeFlagType.SiteMetrics, groupId, siteId);
                if (startTicks > DateTime.MinValue.Ticks)
                {
                    long retentionBoundary = endTicks - TimeSpan.FromDays(ChangeLogRetentionDays).Ticks;
                    if (retentionBoundary < DateTime.MinValue.Ticks)
                    {
                        retentionBoundary = DateTime.MinValue.Ticks;
                    }

                    if (startTicks < retentionBoundary)
                    {
                        mLog.Info($"Recorded incremental discover time {new DateTime(startTicks, DateTimeKind.Utc):o} is older than the configured {ChangeLogRetentionDays}-day range for {node.FullPath}. Fallback to full scan.");
                    }
                    else if (startTicks < endTicks)
                    {
                        configuration.UseIncrementalDiscover = true;
                        configuration.IncrementalDiscoverStartTimeTicks = startTicks;
                        mLog.Info($"Enable incremental discover for site collection {node.FullPath}. Range: {new DateTime(startTicks, DateTimeKind.Utc):o} - {new DateTime(endTicks, DateTimeKind.Utc):o}.");
                        return;
                    }
                    else
                    {
                        mLog.Warn($"Stored start ticks {startTicks} are not earlier than end ticks {endTicks} for {node.FullPath}, fallback to full scan.");
                    }
                }

                ResetIncrementalDiscoverRange(configuration);
                mLog.Info($"No usable site metrics node flag for {node.FullPath}, fallback to full scan. Recorded start ticks: {startTicks}.");
            }
            catch (Exception ex)
            {
                ResetIncrementalDiscoverRange(configuration);
                mLog.Warn($"Failed to initialize incremental discover settings for {node.FullPath}. Error:{ex}");
            }
        }

        private static void ResetIncrementalDiscoverRange(ScheduleConfiguration configuration)
        {
            if (configuration == null)
            {
                return;
            }

            configuration.UseIncrementalDiscover = false;
            configuration.IncrementalDiscoverStartTimeTicks = DateTime.MinValue.Ticks;
            configuration.IncrementalDiscoverEndTimeTicks = DateTime.MinValue.Ticks;
        }
    }
}
