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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Discovery.Model.Configuration;
using AvePoint.RA.DB.Dao.Discovery.Impl;
using AvePoint.RA.DB.Model.Discovery;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using AvePoint.RA.RACommonUtility.Lcoker;
using AvePoint.RA.DB.Model.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.Office365;
using AvePoint.RA.DB.Dao.Discovery.Impl.Office365;

namespace AvePoint.RA.SharePoint.Archiver.Discovery
{
    public class RMDiscoveryOptimizedCalculator
    {

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDiscoveryOptimizedCalculator));

        private readonly IRMDiscoveryOffice365ProgressDao _optimizationDao = new RMDiscoveryOffice365ProgeressDao();

        private readonly Guid _o365TenantId;

        private readonly RMDiscoveryOffice365SiteInfo _siteInfo;

        private long _archivedTotalSize = 0;

        private long _deletedTotalSize = 0;

        public RMDiscoveryOptimizedCalculator(Guid o365TenantId, RMDiscoveryOffice365SiteInfo siteInfo)
        {
            _o365TenantId = o365TenantId;
            _siteInfo = siteInfo;
        }

        public void IncreaseArchivedSize(long size) => Interlocked.Add(ref _archivedTotalSize, size);

        public void IncreaseDeletedSize(long size) => Interlocked.Add(ref _deletedTotalSize, size);

        public async Task SynchronizeAsync()
        {
            await SynchronizeSiteAsync();
            await SynchronizeContainerAsync();
        }

        private async Task SynchronizeContainerAsync()
        {
            try
            {
                await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryOptimizationCalculate, _siteInfo.ContainerId.ToString(), TimeSpan.FromMinutes(10)))
                {
                    var item = await _optimizationDao.GetContainerOptimizedInfoAsync(_o365TenantId, _siteInfo.ContainerId);
                    item ??= new RMDiscoveryOffice365ContainerOptimizedInfo
                    {
                        ContainerId = _siteInfo.ContainerId,
                    };
                    item.Archived += _archivedTotalSize;
                    item.Deleted += _deletedTotalSize;
                    await _optimizationDao.AddOrUpdateContainerOptimizedInfoAsync(_o365TenantId, item);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while synchronize container optimized data size. Error: {e}");
            }
        }

        private async Task SynchronizeSiteAsync()
        {
            try
            {
                await using (await RMRedisLockHandler.LockAsync(RMRedisLockKey.DiscoveryOptimizationCalculate, _siteInfo.SiteId.ToString(), TimeSpan.FromMinutes(10)))
                {
                    var item = await _optimizationDao.GetSiteOptimizedInfoAsync(_o365TenantId, _siteInfo.Id);
                    item.Archived += _archivedTotalSize;
                    item.Deleted += _deletedTotalSize;
                    //item.NextOptimizationTime = 0;
                    item.SettingId = Guid.Empty;
                    await _optimizationDao.AddOrUpdateSiteOptimizedInfoAsync(_o365TenantId, item);
                }
            }
            catch (Exception e)
            {
                _logger.Error($"An error occurred while synchronize site optimized data size. Error: {e}");
            }
        }
    }
}
