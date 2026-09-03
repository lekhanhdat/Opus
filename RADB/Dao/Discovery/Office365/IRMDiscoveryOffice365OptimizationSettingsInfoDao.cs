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
using AvePoint.RA.Contract.Discovery.Model.Query.Progress;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Office365
{
    public interface IRMDiscoveryOffice365OptimizationSettingsInfoDao
    {
        Task<int> AddOrUpdateAsync(RMDiscoveryOffice365OptimizationSettingsInfo settingInfo, Guid O365TenantId);
        Task<RMDiscoveryOffice365OptimizationSettingsInfo> GetSettingInfoByIdAsync(Guid id, Guid O365TenantId);
        Task<RMDiscoveryOffice365OptimizationSettingsInfo> GetSettingInfoBySettingAsync(string id, Guid O365TenantId);
        Task<List<RMDiscoveryOffice365OptimizationSettingsInfo>> GetNeedRunJobSettingAsync(long time, Guid O365TenantId);
        Task<int> UpdateStatusAsync(Guid settingId, DiscoverOptimizationScheduleStatus status, Guid O365TenantId);
        Task<List<RMDiscoveryOffice365OptimizationSettingsInfo>> GetPlanSettingInfoAsync(RMDiscoveryProgressPaginateInfo paginateInfo);
        Task<int> CountPlanSettingInfoAsync(Guid O365TenantId);
        Task<List<string>> GetSettingRelateSitesAsync(Guid o365TenantId, Guid uniqueId, int skip, int take);
        Task<int> CountSettingRelateSiteAsync(Guid o365TenantId, Guid uniqueId);
        Task<RMDiscoveryOffice365OptimizationSettingsInfo> GetSettingInfoBySettingInfoIdAsync(int id, Guid O365TenantId);
        Task<int> removePlanSettingInfoAsync(RMDiscoveryDBEFContext context, Guid settingId);
        Task<RMDiscoveryOffice365OptimizationSettingsInfo> GetLatestSettingAsync(Guid o365TenantId, Guid siteId, long beforeScheduleTicks);
        IAsyncEnumerable<RMDiscoveryOffice365SiteInfo> GetSettingRelatedSitesAsync(Guid o365TenantId, Guid id);
        Task<int> UpdateIsHandleAsync(Guid settingId, bool isHandle, Guid O365TenantId);
    }
}
