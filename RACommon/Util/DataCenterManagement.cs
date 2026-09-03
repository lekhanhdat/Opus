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
using AvePoint.RA.Cache.Services;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.Contract.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Util
{
    public class DataCenterManagent
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(DataCenterManagent));

        private static readonly string RedisDataCenterCacheKeyPrefix = RecordsConstants.DataCenterCacheKeyPrefix;

        private static readonly Dictionary<string, string> TenantDataCenterCache = new Dictionary<string, string>();

        private static readonly object Locker = new object();
        private static IRMCache Cache => PlatformWindsorManager.GetService<IRMCache>();
        public static string GetDataCenter()
        {
            return GetDataCenter(TenantLocalValue.LogonGroupId);
        }

        public static string GetDataCenter(string tenantId)
        {
            Logger.Info($"Get data center by tenant: [{tenantId}].");

            if (!TenantDataCenterCache.TryGetValue(tenantId, out string dataCenter))
            {
                lock (Locker)
                {
                    if (!TenantDataCenterCache.TryGetValue(tenantId, out dataCenter))
                    {
                        Logger.Info($"Not found data center by [{tenantId}] in memory cache.");

                        var cacheKey = RedisDataCenterCacheKeyPrefix + tenantId;
                        dataCenter = Cache.TryGetAsync(cacheKey, () => { return Task.FromResult(RMAosApiClient.GetDatacenter(tenantId)); }).Result;
                        if (string.IsNullOrEmpty(dataCenter))
                        {
                            Logger.Info($"Not found data center by [{tenantId}] in aos.");
                            throw new Exception($"Not found data center by tenant: [{tenantId}].");
                        }
                        TenantDataCenterCache.Add(tenantId, dataCenter);
                    }
                }
            }

            return dataCenter;
        }
    }
}
