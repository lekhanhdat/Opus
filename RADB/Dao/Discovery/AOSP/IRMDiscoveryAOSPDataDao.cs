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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.DB.Core.Discovery;
using AvePoint.RA.DB.Core.Discovery.Context;
using AvePoint.RA.DB.Model.Discovery.AOSP;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.AOSP
{
    public interface IRMDiscoveryAOSPDataDao
    {
        Task DeleteSiteRuleLevelRotDataListAsync(Guid o365TenantId, int siteId);

        Task AddSiteRuleLevelRotDataListAsync(Guid o365TenantId, List<RMDiscoveryAOSPSiteRuleLevelRotData> dataList);

        Task DeleteSiteCategoryLevelRotDataListAsync(Guid o365TenantId, int siteId);

        Task AddSiteCategoryLevelRotDataListAsync(Guid o365TenantId, List<RMDiscoveryAOSPSiteCategoryLevelRotData> dataList);

        Task DeleteSiteRootLevelRotDataListAsync(Guid o365TenantId, int siteId);

        Task DeleteSiteInactiveDataListAsync(Guid o365TenantId, int siteId);

        Task AddSiteRootLevelRotDataListAsync(Guid o365TenantId, List<RMDiscoveryAOSPSiteRootLevelRotData> dataList);

        Task AddSiteInactiveDataListAsync(Guid o365TenantId, params RMDiscoveryAOSPSiteInactiveData[] dataList);

        Task AddOrUpdateContainerInactiveDataUnderSameContainerAsync(Guid o365TenantId, params RMDiscoveryAOSPContainerInactiveData[] dataList);

        IAsyncEnumerable<RMDiscoveryAOSPSiteInactiveData> GetSiteInactiveDataByContainerIdAsync(Guid o365TenantId, int containerId, List<RMDiscoveryCustomColumn> customColumns);

        Task AddOrUpdateBasicInactiveDataUnderSameContentSourceAsync(Guid o365TenantId, params RMDiscoveryAOSPBasicInactiveData[] dataList);

        Task<List<RMDiscoveryAOSPBasicInactiveData>> GetBasicInactiveDataListAsync(Guid o365TenantId, SourceFlag contentSource, List<RMDiscoveryCustomColumn> customColumns);

        IAsyncEnumerable<RMDiscoveryAOSPContainerInactiveData> GetContainerInactiveDataListAsync(Guid o365TenantId, int containerId, List<RMDiscoveryCustomColumn> customColumns);

        Task AddOrUpdateContainerRuleLevelRotDataUnderSameContainerAsync(Guid o365TenantId, params RMDiscoveryAOSPContainerRuleLevelRotData[] dataList);

        IAsyncEnumerable<RMDiscoveryAOSPSiteRuleLevelRotData> GetSiteRuleLevelRotDataByContainerIdAsync(Guid o365TenantId, int containerId);

        Task AddOrUpdateContainerCategoryLevelRotDataUnderSameContainerAsync(Guid o365TenantId, params RMDiscoveryAOSPContainerCategoryLevelRotData[] dataList);

        IAsyncEnumerable<RMDiscoveryAOSPSiteCategoryLevelRotData> GetSiteCategoryLevelRotDataByContainerIdAsync(Guid o365TenantId, int containerId);

        Task AddOrUpdateContainerRootLevelRotDataUnderSameContainerAsync(Guid o365TenantId, params RMDiscoveryAOSPContainerRootLevelRotData[] dataList);

        IAsyncEnumerable<RMDiscoveryAOSPSiteRootLevelRotData> GetSiteRootLevelRotDataByContainerIdAsync(Guid o365TenantId, int containerId);

        Task AddOrUpdateBasicRuleLevelRotDataUnderSameContentSourceAsync(Guid o365TenantId, params RMDiscoveryAOSPBasicRuleLevelRotData[] dataList);

        Task<List<RMDiscoveryAOSPBasicRuleLevelRotData>> GetBasicRuleLevelRotDataListAsync(Guid o365TenantId, SourceFlag contentSource);

        IAsyncEnumerable<RMDiscoveryAOSPContainerRuleLevelRotData> GetContainerRuleLevelRotDataListAsync(Guid o365TenantId, int containerId);

        Task AddOrUpdateBasicCategoryLevelRotDataUnderSameContentSourceAsync(Guid o365TenantId, params RMDiscoveryAOSPBasicCategoryLevelRotData[] dataList);

        Task<List<RMDiscoveryAOSPBasicCategoryLevelRotData>> GetBasicCategoryLevelRotDataListAsync(Guid o365TenantId, SourceFlag contentSource);

        IAsyncEnumerable<RMDiscoveryAOSPContainerCategoryLevelRotData> GetContainerCategoryLevelRotDataListAsync(Guid o365TenantId, int containerId);

        Task AddOrUpdateBasicRootLevelRotDataUnderSameContentSourceAsync(Guid o365TenantId, params RMDiscoveryAOSPBasicRootLevelRotData[] dataList);

        Task<List<RMDiscoveryAOSPBasicRootLevelRotData>> GetBasicRootLevelRotDataListAsync(Guid o365TenantId, SourceFlag contentSource);

        IAsyncEnumerable<RMDiscoveryAOSPContainerRootLevelRotData> GetContainerRootLevelRotDataListAsync(Guid o365TenantId, int containerId);

        Task<RMDiscoveryAOSPAggregateTotalData> GetAggregateTotalDataAsync(Guid o365TenantId, SourceFlag contentSource);

        Task AddOrUpdateAggregateTotalDataAsync(Guid o365TenantId, RMDiscoveryAOSPAggregateTotalData data);

        Task AddOrUpdateAggregateTotalDataAsync(RMDiscoveryDBEFContext efContext, RMDiscoveryAOSPAggregateTotalData data);

        Task<List<RMDiscoveryAOSPAggregateTotalData>> GetAggregateTotalDataListAsync(Guid o365TenantId);

        IAsyncEnumerable<RMDiscoveryAOSPSiteInactiveData> GetSiteInactiveDataBySqlConditionalExpressionAsync(Guid o365TenantId, int siteId, string sqlConditionalExpression, List<SQLiteParameter> parameters, List<RMDiscoveryCustomColumn> customColumns);

        IAsyncEnumerable<RMDiscoveryAOSPSiteRuleLevelRotData> GetSiteRuleLevelRotDataBySqlConditionalExpressionAsync(Guid o365TenantId, int siteId, string sqlConditionalExpression, List<SQLiteParameter> parameters);

        IAsyncEnumerable<RMDiscoveryAOSPSiteRootLevelRotData> GetSiteRootLevelRotDataBySqlConditionalExpressionAsync(Guid o365TenantId, int siteId, string sqlConditionalExpression, List<SQLiteParameter> parameters);

        IAsyncEnumerable<RMDiscoveryAOSPSiteRootLevelRotData> GetSiteRootLevelRotDataBySqlConditionalExpressionAsync(Guid o365TenantId, List<int> siteIds);

        IAsyncEnumerable<RMDiscoveryAOSPSiteCategoryLevelRotData> GetSiteCategoryLevelRotDataBySqlConditionalExpressionAsync(Guid o365TenantId, List<int> siteIds);

        IAsyncEnumerable<RMDiscoveryAOSPSiteInactiveData> GetSiteInactiveDataBySqlConditionalExpressionAsync(Guid o365TenantId, List<int> siteIds, List<RMDiscoveryCustomColumn> customColumns);
    }
}
