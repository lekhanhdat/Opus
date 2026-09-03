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
using AvePoint.RA.DB.Model.Discovery;
using AvePoint.RA.DB.Model.Discovery.Office365;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao.Discovery.Office365
{
    public interface IRMDiscoveryOffice365DataV3Dao
    {
        Task AddOrUpdateContainerInactiveDataUnderSameContainerAsync(Guid o365TenantId, params RMDiscoveryOffice365ContainerInactiveData[] dataList);

        Task UpsertContainerInactiveDataAsync(Guid o365TenantId, RMDiscoveryOffice365ContainerInactiveData data);

        IAsyncEnumerable<RMDiscoveryOffice365ContainerInactiveData> GetContainerInactiveDataListAsync(Guid o365TenantId, int containerId, List<RMDiscoveryCustomColumn> customColumns);

        Task AddOrUpdateBasicInactiveDataUnderSameContentSourceAsync(Guid o365TenantId, params RMDiscoveryOffice365BasicInactiveData[] dataList);

        Task UpsertBasicInactiveDataAsync(Guid o365TenantId, RMDiscoveryOffice365BasicInactiveData data);

        Task<List<RMDiscoveryOffice365BasicInactiveData>> GetBasicInactiveDataListAsync(Guid o365TenantId, SourceFlag contentSource, List<RMDiscoveryCustomColumn> customColumns);

        Task DeleteBasicInactiveDataAsync(Guid o365TenantId, SourceFlag contentSource);

        Task DeleteSiteInactiveDataListAsync(Guid o365TenantId, int siteId);

        Task AddSiteInactiveDataListAsync(Guid o365TenantId, params RMDiscoveryOffice365SiteInactiveData[] dataList);

        IAsyncEnumerable<RMDiscoveryOffice365SiteInactiveData> GetSiteInactiveDataByContainerIdAsync(Guid o365TenantId, int containerId, List<RMDiscoveryCustomColumn> customColumns);

        IAsyncEnumerable<RMDiscoveryOffice365SiteInactiveData> GetSiteInactiveDataBySqlConditionalExpressionAsync(Guid o365TenantId, int siteId, string sqlConditionalExpression, List<SQLiteParameter> parameters, List<RMDiscoveryCustomColumn> customColumns);

        Task DeleteSiteRuleLevelRotDataListAsync(Guid o365TenantId, int siteId);

        Task AddSiteRuleLevelRotDataListAsync(Guid o365TenantId, List<RMDiscoveryOffice365SiteRuleLevelRotData> dataList);

        IAsyncEnumerable<RMDiscoveryOffice365SiteRuleLevelRotData> GetSiteRuleLevelRotDataByContainerIdAsync(Guid o365TenantId, int containerId);

        Task DeleteSiteCategoryLevelRotDataListAsync(Guid o365TenantId, int siteId);

        Task AddSiteCategoryLevelRotDataListAsync(Guid o365TenantId, List<RMDiscoveryOffice365SiteCategoryLevelRotData> dataList);

        IAsyncEnumerable<RMDiscoveryOffice365SiteCategoryLevelRotData> GetSiteCategoryLevelRotDataByContainerIdAsync(Guid o365TenantId, int containerId);

        Task DeleteSiteRootLevelRotDataListAsync(Guid o365TenantId, int siteId);

        Task AddSiteRootLevelRotDataListAsync(Guid o365TenantId, List<RMDiscoveryOffice365SiteRootLevelRotData> dataList);

        IAsyncEnumerable<RMDiscoveryOffice365SiteRootLevelRotData> GetSiteRootLevelRotDataBySqlConditionalExpressionAsync(Guid o365TenantId, int siteId, string sqlConditionalExpression, List<SQLiteParameter> parameters);

        IAsyncEnumerable<RMDiscoveryOffice365SiteCategoryLevelRotData> GetSiteCategoryLevelRotDataBySqlConditionalExpressionAsync(Guid o365TenantId, int siteId, string sqlConditionalExpression, List<SQLiteParameter> parameters);

        IAsyncEnumerable<RMDiscoveryOffice365SiteRuleLevelRotData> GetSiteRuleLevelRotDataBySqlConditionalExpressionAsync(Guid o365TenantId, int siteId, string sqlConditionalExpression, List<SQLiteParameter> parameters);

        IAsyncEnumerable<RMDiscoveryOffice365SiteRootLevelRotData> GetSiteRootLevelRotDataByContainerIdAsync(Guid o365TenantId, int containerId);

        Task AddOrUpdateContainerRuleLevelRotDataUnderSameContainerAsync(Guid o365TenantId, params RMDiscoveryOffice365ContainerRuleLevelRotData[] dataList);

        Task UpsertContainerRuleLevelRotDataAsync(Guid o365TenantId, RMDiscoveryOffice365ContainerRuleLevelRotData data);

        IAsyncEnumerable<RMDiscoveryOffice365ContainerRuleLevelRotData> GetContainerRuleLevelRotDataListAsync(Guid o365TenantId, int containerId);

        Task AddOrUpdateContainerCategoryLevelRotDataUnderSameContainerAsync(Guid o365TenantId, params RMDiscoveryOffice365ContainerCategoryLevelRotData[] dataList);

        Task UpsertContainerCategoryLevelRotDataAsync(Guid o365TenantId, RMDiscoveryOffice365ContainerCategoryLevelRotData data);

        IAsyncEnumerable<RMDiscoveryOffice365ContainerCategoryLevelRotData> GetContainerCategoryLevelRotDataListAsync(Guid o365TenantId, int containerId);

        Task AddOrUpdateContainerRootLevelRotDataUnderSameContainerAsync(Guid o365TenantId, params RMDiscoveryOffice365ContainerRootLevelRotData[] dataList);

        Task UpsertContainerRootLevelRotDataAsync(Guid o365TenantId, RMDiscoveryOffice365ContainerRootLevelRotData data);

        IAsyncEnumerable<RMDiscoveryOffice365ContainerRootLevelRotData> GetContainerRootLevelRotDataListAsync(Guid o365TenantId, int containerId);

        Task AddOrUpdateBasicRuleLevelRotDataUnderSameContentSourceAsync(Guid o365TenantId, params RMDiscoveryOffice365BasicRuleLevelRotData[] dataList);

        Task UpsertBasicRuleLevelRotDataAsync(Guid o365TenantId, RMDiscoveryOffice365BasicRuleLevelRotData data);

        Task<List<RMDiscoveryOffice365BasicRuleLevelRotData>> GetBasicRuleLevelRotDataListAsync(Guid o365TenantId, SourceFlag contentSource);

        Task AddOrUpdateBasicCategoryLevelRotDataUnderSameContentSourceAsync(Guid o365TenantId, params RMDiscoveryOffice365BasicCategoryLevelRotData[] dataList);

        Task UpsertBasicCategoryLevelRotDataAsync(Guid o365TenantId, RMDiscoveryOffice365BasicCategoryLevelRotData data);

        Task<List<RMDiscoveryOffice365BasicCategoryLevelRotData>> GetBasicCategoryLevelRotDataListAsync(Guid o365TenantId, SourceFlag contentSource);

        Task AddOrUpdateBasicRootLevelRotDataUnderSameContentSourceAsync(Guid o365TenantId, params RMDiscoveryOffice365BasicRootLevelRotData[] dataList);

        Task UpsertBasicRootLevelRotDataAsync(Guid o365TenantId, RMDiscoveryOffice365BasicRootLevelRotData data);

        Task<List<RMDiscoveryOffice365BasicRootLevelRotData>> GetBasicRootLevelRotDataListAsync(Guid o365TenantId, SourceFlag contentSource);
    }
}
