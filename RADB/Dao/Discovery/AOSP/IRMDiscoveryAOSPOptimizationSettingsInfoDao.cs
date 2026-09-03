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
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.AOSP
{
    public interface IRMDiscoveryAOSPOptimizationSettingsInfoDao
    {
        Task<List<RMDiscoveryAOSPOptimizationSettingsInfo>> GetNeedRunJobSettingAsync(long time, Guid O365TenantId);

        Task<int> UpdateIsHandleAsync(Guid settingId, bool isHandle, Guid O365TenantId);

        Task<int> UpdateStatusAsync(Guid settingId, DiscoverOptimizationScheduleStatus status, Guid O365TenantId);

        Task<RMDiscoveryAOSPOptimizationSettingsInfo> GetSettingInfoByIdAsync(Guid id, Guid O365TenantId);

        Task<RMDiscoveryAOSPOptimizationSettingsInfo> GetSettingInfoByJobIdAsync(string jobId, Guid O365TenantId);

        Task<int> removePlanSettingInfoAsync(RMDiscoveryDBEFContext context, Guid settingId);

        Task<RMDiscoveryAOSPOptimizationSettingsInfo> GetLatestSettingAsync(Guid o365TenantId, Guid siteId, long beforeScheduleTicks);

        Task<RMDiscoveryAOSPOptimizationSettingsInfo> GetSettingInfoBySettingAsync(string setting, Guid O365TenantId);

        Task<int> AddOrUpdateAsync(RMDiscoveryAOSPOptimizationSettingsInfo settingInfo, Guid O365TenantId);

        IAsyncEnumerable<RMDiscoveryAOSPSiteInfo> GetSettingRelatedSitesAsync(Guid o365TenantId, Guid id);
    }
}
