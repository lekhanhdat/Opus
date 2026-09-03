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
    public interface IRMDiscoveryOffice365SiteOptimizationMappingTableDao
    {
        Task<int> CountAsync(Guid o365TenantId);
        Task<int> AddOrUpdateAsync(List<RMDiscoveryOffice365SiteOptimizationMappingInfo> updateRuleInfos, Guid O365TenantId);
        Task<RMDiscoveryOffice365SiteOptimizationMappingInfo> GetMappingInfoByNodeIdAsync(long nodeId, Guid O365TenantId);
        Task<List<RMDiscoveryOffice365SiteOptimizationMappingInfo>> GetMappingInfoBySettingIdAsync(Guid settingId, Guid O365TenantId);
        Task<List<RMDiscoveryOffice365SiteOptimizationMappingInfo>> GetAllMappingInfoAsync(Guid O365TenantId);
        Task<List<RMDiscoveryOffice365SiteOptimizationMappingInfo>> GetAllMappingInfoBySettingIdsAsync(Guid O365TenantId, IEnumerable<Guid> settingIds);
        Task<List<RMDiscoveryOffice365SiteOptimizationMappingInfo>> GetAllMappingInfoBySettingIdsAsync(Guid O365TenantId, IEnumerable<Guid> settingIds, int skip, int take);

        Task<int> GetInScopeSiteCount(Guid O365TenantId, int containerId);

        Task<List<long>> GetAllInScopeSiteIds(Guid O365TenantId, IEnumerable<long> itemIds);
        Task removeMappingInfoAsync(RMDiscoveryDBEFContext context, Guid settingId);
        Task<List<long>> GetAllsites(Guid O365TenantId);

        #region V3
        Task<long> CountPHLDataTotalSizeV3(Guid o365TenantId);
        Task<long> GetPHLDataTotalSizeV3ByContainerId(Guid o365TenantId, int containerId);
        #endregion
    }
}
