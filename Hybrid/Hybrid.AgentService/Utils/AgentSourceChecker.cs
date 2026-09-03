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
using AvePoint.Hybrid.Contract.Object;
using AvePoint.Hybrid.Utility;
using AvePoint.RA.Common.TransientFault;
using AvePoint.RA.Hybrid.Browser.Contract;
using System;
using System.Collections.Generic;

namespace AvePoint.Hybrid.AgentService.Utils
{
    public class AgentSourceChecker
    {

        private static readonly AveRetryPolicy RetryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(3, TimeSpan.FromSeconds(10)));

        private static readonly Dictionary<SourceType, Func<bool>> SourceMapping = new Dictionary<SourceType, Func<bool>>
        {
            { SourceType.FileSystem, CheckFileSystemHasError },
            { SourceType.SharePoint, CheckSharePointHasError }
        };

        public static ServiceErrors CheckAgentSourceHasError()
        {
            var res = ServiceErrors.None;
            foreach(var mapping in SourceMapping)
            {
                if(mapping.Value())
                {
                    res |= (ServiceErrors)(int)mapping.Key;
                }
            }
            return res;
        }

        private static bool CheckFileSystemHasError()
        {
            return false;
        }

        private static bool CheckSharePointHasError()
        {
            var tenantId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerTenantId);
            var agentId = CommonConfiguration.getConfig(HybridAppSettingKey.CustomerAgentId);
            var farmId = RetryPolicy.ExecuteAction(() => HybridBrowserUtil.Instance.Browse(HybridBrowserType.SharePointOnPremFarm, ""));
            return string.IsNullOrEmpty(farmId);
        }
    }
}
